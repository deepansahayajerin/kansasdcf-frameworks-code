using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using MDSY.Utilities.Security;
using MDSY.Framework.Core;
using System.Diagnostics;
using MDSY.Framework.Configuration.Common;
using Oracle.ManagedDataAccess.Client;

namespace MDSY.Framework.Control.CICS
{
    public class TDQueueInUtilityOracle : ITDQueue
    {
        private static Dictionary<String, LinkedList<byte[]>> TDQ = new Dictionary<String, LinkedList<byte[]>>();
        string eventLogName = ConfigSettings.GetAppSettingsString("EventLogName");
        public byte[] ReadTransientQueue(string queueName, int dataLength)
        {
            try
            {
                DBSUtil.Condition = HandleCondition.NORMAL;

                byte[] queueData = new byte[dataLength];
                Monitor.Enter(TDQ);
                queueData = ReadTransientQueueFromDB(queueName, dataLength);
                Monitor.Exit(TDQ);

                if (queueData == null)
                    DBSUtil.Condition = HandleCondition.QZERO;
                return queueData;
            }
            catch
            {
                DBSUtil.Condition = HandleCondition.ERROR;
            }

            return null;
        }

        public void WriteTransientQueue(string queueName, byte[] queueData, int queueLength)
        {
            DCTEntry dct = null;
            DBSUtil.Condition = HandleCondition.NORMAL;

            try
            {
                dct = DCTEntry.GetDctEntry(queueName.ToString());
            }
            catch (DestinationNotFound)
            {
                DBSUtil.Condition = HandleCondition.QIDERR;
                //throw;
            }

            if (dct == null) return;

            if (dct.DestType != "INTRA" && dct.DestType != "EXTRA" && dct.DestType != "")
            {
                throw new NotImplementedException();
            }



            int level = 1;
            Monitor.Enter(TDQ);

            level = WriteTransientQueueToDB(queueName, queueData);

            Monitor.Exit(TDQ);

            // Check triggering
            if (!string.IsNullOrEmpty(dct.TransId))
            {
                if (level >= dct.TriggerLevel)
                {
                    // Start transaction on the specified terminal
                    String _termId = (dct.DestFacility == "") ? dct.DestID : dct.DestFacility;
                    try
                    {
                        TriggerTransaction(dct.TransId, _termId);
                    }
                    catch (Exception ex)
                    {
                        DBSUtil.Condition = HandleCondition.ERROR;
                        using (EventLog eventLog = new EventLog(eventLogName))
                        {
                            eventLog.Source = eventLogName;
                            eventLog.WriteEntry("TDQueue Start Transaction: " + dct.TransId + " Terminal: " + _termId + " Delete failed. \r\n" + ex, EventLogEntryType.Error, 113);
                        }
                    }
                }
            }
        }

        public void DeleteTransientQueue(string queueName)
        {
            try
            {
                DBSUtil.Condition = HandleCondition.NORMAL;

                Monitor.Enter(TDQ);
                DeleteTransientQueueFromDB(queueName);
                Monitor.Exit(TDQ);
            }
            catch
            {
                DBSUtil.Condition = HandleCondition.ERROR;
            }
        }


        private int WriteTransientQueueToDB(string queueName, byte[] queueData)
        {
            int level = 1;

            string connectionString = ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString"); ;

            if (String.IsNullOrEmpty(connectionString))
                return 0;

            try
            {
                StringBuilder sql = new StringBuilder("");
                sql.Append("insert into MDSY_TDTS_DATA (QNAME, DATA) ");
                sql.Append("values (:QNAME, :DATA); ");
                sql.Append("select count(*) from MDSY_TDTS_DATA (nolock) where QNAME = :QNAME; ");
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand(sql.ToString(), connection))
                    {
                        command.Parameters.Add(":QNAME", queueName);
                        command.Parameters.Add(":DATA", queueData);
                        level = (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception exc)
            {
                CatchError(exc);
            }

            return level;
        }

        private void DeleteTransientQueueFromDB(string queueName)
        {
            string connectionString = ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString");

            try
            {
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("delete from MDSY_TDTS_DATA where QNAME = :QNAME; ", connection))
                    {
                        command.Parameters.Add(":QNAME", queueName);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exc)
            {
                CatchError(exc);
            }

            return;
        }

        private byte[] ReadTransientQueueFromDB(string queueName, int dataLength)
        {
            byte[] queueData = new byte[] { };
            StringBuilder sql = new StringBuilder("");
            string connectionString = ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString"); ;

            try
            {
                sql.Append("declare @id as int; ");
                sql.Append("declare @data as varbinary(max); ");
                sql.Append("select top 1 @id = ID from MDSY_TDTS_DATA where QNAME = :QNAME order by TIMESTAMP asc; ");
                sql.Append("select @data = DATA from MDSY_TDTS_DATA where ID = @id; ");
                sql.Append("delete from MDSY_TDTS_DATA where ID = @id; ");
                sql.Append("select @data; ");
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand(sql.ToString(), connection))
                    {
                        command.Parameters.Add(":QNAME", queueName);
                        try
                        {
                            queueData = (byte[])command.ExecuteScalar();
                        }
                        catch
                        {
                            queueData = new byte[dataLength];
                            DBSUtil.Condition = HandleCondition.QZERO;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                CatchError(exc);
            }

            return queueData;
        }

        private void TriggerTransaction(string transId, string termId)
        {
            int threadCount = Security.GetUserThreadCount(ServiceControl.UserID);
            Security.SetUserThreadCount(ServiceControl.UserID, ++threadCount);
            try
            {
                DBSUtil.LogInformationMessage("Triggering transaction: transId=" + transId + " termId=" + termId + " - " + (ServiceControl.UserID != null ? ServiceControl.UserID : "NOID ") + DateTime.Now.Ticks.ToString());

                string[] parms = new string[] { transId, termId, ServiceControl.UserID };
                NamedBackgroundWorker backgroundWorker = new NamedBackgroundWorker(transId + termId + (ServiceControl.UserID != null ? ServiceControl.UserID : "") + "-" + DateTime.Now.Ticks.ToString());
                backgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorker_RunProcess);
                // probably dont need the RunWorkerCompleted but for now put in here to see if we ever do finish.
                backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunProcessCompleted);
                backgroundWorker.RunWorkerAsync(parms);
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog(eventLogName))
                {
                    eventLog.Source = eventLogName;
                    eventLog.WriteEntry("TDQueue failed. \r\n" + ex + " for user " + ServiceControl.UserID, EventLogEntryType.Error, 113);
                }
            }
        }

        private void BackgroundWorker_RunProcess(object sender, DoWorkEventArgs e)
        {
            String programName = "";

            try
            {
                string transId = ((string[])e.Argument)[0];
                int threadCount = 0;
                programName = TransactionControl.Instance.GetProgramName(transId.ToUpper());
                Type programType = DBSUtil.GetBLType(programName);
                OnlineProgramBase programInstance;
                if (programType == null) return;
                programInstance = (OnlineProgramBase)Activator.CreateInstance(programType, null);

                DBSUtil.CheckProgramLogging(programType.Name, "BackgroundWorker Enter Program ", ((string[])e.Argument)[2]);
                if (programInstance.ExecuteMain() == 12 && ServiceControl.CurrentException != null)
                {
                    DBSUtil.CheckProgramLogging(programName, "TDQueue failed in program " + programName + ". \r\n" + CatchError(ServiceControl.CurrentException), LogMessageType.Error);
                    threadCount = Security.GetUserThreadCount(((string[])e.Argument)[2]);
                    Security.SetUserThreadCount(((string[])e.Argument)[2], --threadCount);
                    Exception exi = ServiceControl.CurrentException;
                    while (exi != null)
                    {
                        SimpleLogging.LogMandatoryMessageToFile("**** ERROR - Backgroundworker_RunProcess (Inner loop) - TDQueue " + programName + "\r\n Message: "
                                 + exi.Message + "\r\n StackTrace: " + exi.StackTrace);
                        exi = exi.InnerException;

                    }
                    throw new Exception("TDQueue " + programName + " failed. Please see inner exception for more details", ServiceControl.CurrentException);
                }

                programInstance.Data.ForceDbClose();
                DBSUtil.CheckProgramLogging(programType.Name, "BackgroundWorker Exit Program ", ((string[])e.Argument)[2]);
                threadCount = Security.GetUserThreadCount(((string[])e.Argument)[2]);
                Security.SetUserThreadCount(((string[])e.Argument)[2], --threadCount);
            }
            catch (Exception ex)
            {
                if (ServiceControl.CurrentException == null)
                    CatchError(ex);
                ServiceControl.CurrentException = ex;
                SimpleLogging.LogMandatoryMessageToFile("**** ERROR - Backgroundworker_RunProcess (Main catch) - TDQueue " + programName + "\r\n Message: " + ex.Message
                        + "\r\n StackTrace: " + ex.StackTrace.ToString());
                Exception exi = ServiceControl.CurrentException;
                while (exi != null)
                {
                    SimpleLogging.LogMandatoryMessageToFile("**** ERROR - Backgroundworker_RunProcess (Main catch) - TDQueue " + programName + "\r\n Message: "
                             + exi.Message + "\r\n StackTrace: " + exi.StackTrace);
                    exi = exi.InnerException;
                }
                using (EventLog eventLog = new EventLog(eventLogName))
                {
                    eventLog.Source = eventLogName;
                    eventLog.WriteEntry("TDQueue failed. \r\n" + ex.Message + " for user " + ServiceControl.UserID, EventLogEntryType.Error, 113);
                }
                //throw new Exception("TDQueue " + programName + " failed. Please see inner exception for more details", ex);
            }
        }

        private void BackgroundWorker_RunProcessCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Exception exi = e.Error;
                while (exi != null)
                {
                    SimpleLogging.LogMandatoryMessageToFile("**** ERROR - Backgroundworker_RunProcessCompleted for user " + ServiceControl.UserID + "\r\n Message: "
                             + exi.Message + "\r\n StackTrace: " + exi.StackTrace);
                    exi = exi.InnerException;
                }
                using (EventLog eventLog = new EventLog(eventLogName))
                {
                    eventLog.Source = eventLogName;
                    eventLog.WriteEntry("Background worker process failed. \r\n" + e.Error + " for user " + ServiceControl.UserID, EventLogEntryType.Error, 113);
                }
#if DEBUG
                throw new Exception("Background worker process failed. Please see inner exception for more details", e.Error);
#endif
            }
        }

        private string CatchError(Exception ex)
        {
            StringBuilder errMessage = new StringBuilder();
            errMessage.AppendLine("******************************************************************");
            errMessage.AppendLine("== Exception Message ==");
            errMessage.AppendLine(string.Concat("    ", ex.Source, " ABORTING DUE TO THE FOLLOWING ERROR"));
            errMessage.AppendLine(ex.Message);
            errMessage.AppendLine("******************************************************************");
            errMessage.AppendLine("== StackTrace ==");
            errMessage.AppendLine(ex.StackTrace);
            errMessage.AppendLine("******************************************************************");
            while (ex.InnerException != null)
            {
                Exception exi = ex.InnerException;
                errMessage.AppendLine("== Inner Exception Message ==");
                errMessage.AppendLine(exi.Message);
                errMessage.AppendLine("******************************************************************");
                errMessage.AppendLine("== StackTrace ==");
                errMessage.AppendLine(exi.StackTrace);
                errMessage.AppendLine("******************************************************************");

                ex = ex.InnerException;
            }

            return errMessage.ToString();
        }

    }
}
