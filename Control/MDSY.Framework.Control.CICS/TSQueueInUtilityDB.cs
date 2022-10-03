using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using MDSY.Framework.Core;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Control.CICS
{
    public class TSQueueInUtilityDB : ITSQueue
    {
        private static Dictionary<String, LinkedList<byte[]>> TSQ = new Dictionary<String, LinkedList<byte[]>>();
        string eventLogName = ConfigSettings.GetAppSettingsString("EventLogName");

        public byte[] ReadTemporaryQueue(string queueName, int queueLength, int queueItem, RowPosition itemPosition, QueueOption queueOption = QueueOption.None)
        {
            DBSUtil.Condition = HandleCondition.NORMAL;

            byte[] queueData;
            Monitor.Enter(TSQ);
            queueData = ReadTemporaryQueueFromDB(queueName, queueItem);
            Monitor.Exit(TSQ);

            if (queueData == null)
                DBSUtil.Condition = HandleCondition.QZERO;

            return queueData;
        }

        public int WriteTemporaryQueue(string queueName, byte[] queueData, int queueLength, int queueItem, QueueOption queueOption = QueueOption.None)
        {
            DBSUtil.Condition = HandleCondition.NORMAL;
            bool isRewrite = (queueOption == QueueOption.Rewrite);
            Monitor.Enter(TSQ);
            WriteTemporaryQueueToDB(queueName, queueData, queueItem, isRewrite);
            if (!TSQ.ContainsKey(queueName))
                TSQ.Add(queueName, null);
            Monitor.Exit(TSQ);

            return queueItem;
        }

        public void DeleteTemporaryQueue(string queueName)
        {
            DBSUtil.Condition = HandleCondition.NORMAL;

            Monitor.Enter(TSQ);
            DeleteTemporaryQueueFromDB(queueName);
            if (TSQ.ContainsKey(queueName))
                TSQ.Remove(queueName);
            Monitor.Exit(TSQ);
        }


        private void WriteTemporaryQueueToDB(string queueName, byte[] queueData, int queueItem, bool isRewrite)
        {

            string connectionString = ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return;

            try
            {
                string sqlstring = string.Empty;
                if (isRewrite)
                {
                    sqlstring = "  update MDSY_TDTS_DATA  set DATA = @DATA where QNAME = @QNAME and ITEMNO = @ITEMNO; ";
                    //sqlstring.Append("  update MDSY_TDTS_DATA ");
                    //sqlstring.Append("  set DATA = @DATA ");
                    //sqlstring.Append("  where QNAME = @QNAME and ITEMNO = @ITEMNO; ");
                }
                else
                {
                    sqlstring = " insert into MDSY_TDTS_DATA (QNAME, ITEMNO, DATA) values (@QNAME, @ITEMNO, @DATA)";
                    //sqlstring.Append("  insert into MDSY_TDTS_DATA (QNAME, ITEMNO, DATA) ");
                    //sqlstring.Append("  values (@QNAME, @ITEMNO, @DATA) ");

                }
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sqlstring, connection))
                    {
                        command.Parameters.AddWithValue("@QNAME", queueName);
                        command.Parameters.AddWithValue("@ITEMNO", queueItem);
                        command.Parameters.AddWithValue("@DATA", queueData);
                        command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog(eventLogName))
                {
                    eventLog.Source = eventLogName;
                    eventLog.WriteEntry("TDQueue failed. \r\n" + ex, EventLogEntryType.Error, 113);
                }
            }

            return;
        }

        private void DeleteTemporaryQueueFromDB(string queueName)
        {
            string connectionString = ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString");

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("delete from MDSY_TDTS_DATA where QNAME = @QNAME; ", connection))
                    {
                        command.Parameters.AddWithValue("@QNAME", queueName);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog(eventLogName))
                {
                    eventLog.Source = eventLogName;
                    eventLog.WriteEntry("TDQueue failed. \r\n" + ex, EventLogEntryType.Error, 113);
                }
            }

            return;
        }

        private byte[] ReadTemporaryQueueFromDB(string queueName, int itemNo)
        {
            byte[] queueData = new byte[] { };
            string sqlString = string.Empty;
            string connectionString = ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString");

            try
            {

                sqlString = "select top 1 DATA from MDSY_TDTS_DATA where QNAME = @QNAME and ITEMNO = @ITEMNO order by TIMESTAMP asc; ";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sqlString, connection))
                    {
                        command.Parameters.AddWithValue("@QNAME", queueName);
                        command.Parameters.AddWithValue("@ITEMNO", itemNo);
                        queueData = (byte[])command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog(eventLogName))
                {
                    eventLog.Source = eventLogName;
                    eventLog.WriteEntry("TDQueue failed. \r\n" + ex, EventLogEntryType.Error, 113);
                }
            }

            return queueData;
        }


    }
}
