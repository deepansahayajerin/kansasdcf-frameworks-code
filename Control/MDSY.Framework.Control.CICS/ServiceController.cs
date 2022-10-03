using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using MDSY.Framework.Service.Interfaces;
using MDSY.Framework.Buffer;
using MDSY.Framework.Buffer.BaseClasses;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Core;
using System.Globalization;
using System.Reflection;
using System.ServiceModel;
using MDSY.Framework.Configuration.Common;


namespace MDSY.Framework.Control.CICS
{
    public static class ServiceController
    {

        #region private static properties
        [ThreadStatic]
        private static EventWaitHandle _internalThreadHolder;

        [ThreadStatic]
        private static EventWaitHandle _externalThreadHolder;

        [ThreadStatic]
        private static List<IAterasServiceItem> _inServiceThreadShareData;

        [ThreadStatic]
        private static OnlineControl _control;
        #endregion

        #region public static properties
        public volatile static ConcurrentDictionary<int, bool> _cancelThread = new ConcurrentDictionary<int, bool>();
        public static List<IAterasServiceItem> ThreadShareData
        {
            get
            {
                return ServiceThreadShareData;
            }
            set
            {
                ServiceThreadShareData = value;
            }
        }

        public static List<IAterasServiceItem> ServiceThreadShareData
        {
            get { return _inServiceThreadShareData; }
            set { _inServiceThreadShareData = value; }
        }

        public static bool IsService
        {
            get
            {
                bool isService = false;

                if (_internalThreadHolder != null)
                {
                    isService = true;
                }

                return isService;
            }
        }
        /// <summary>
        /// Gets or sets the thread for the internal system holder
        /// </summary>
        public static EventWaitHandle InternalThreadHolder
        {
            get { return _internalThreadHolder; }
            set { _internalThreadHolder = value; }
        }
        /// <summary>
        /// Gets or sets the thread for the external Service system holder
        /// </summary>
        public static EventWaitHandle ExternalThreadHolder
        {
            get { return _externalThreadHolder; }
            set { _externalThreadHolder = value; }
        }

        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void RunThread()
        {
            DBSUtil.ExternalThreadHolder = _externalThreadHolder;
            DBSUtil.InternalThreadHolder = _internalThreadHolder;
            DBSUtil.ServiceThreadShareData = _inServiceThreadShareData;

            CheckThreadData();


            if (!IsService)
            {
                System.Environment.Exit(12);
            }
            else
            {
                // let the external have it
                ExternalThreadHolder.Set();
            }
        }

        public static void SetUpTFLSessionVariables(string t_classA, string t_classB, string t_cfacu, string t_cwhse, byte[] t_pcipl, byte[] t_pcilp)
        {
            ServiceControl.T_CLASSA = t_classA;
            ServiceControl.T_CLASSB = t_classB;
            ServiceControl.T_CFACU = t_cfacu;
            ServiceControl.T_CWHSE = t_cwhse;
            ServiceControl.T_PCIPL = t_pcipl;
            ServiceControl.T_PCILP = t_pcilp;
        }
            public static void SetUpSessionVariables(string userID, string opID, string termID, string sessionID, OperationContext currentContext, CustomServiceData customData)
        {
            ServiceControl.ClearVarTS();
            ServiceControl.CurrentContext = currentContext;
            ServiceControl.CustomData = customData;
            ServiceControl.SessionID = sessionID;
            ServiceControl.OPID = opID;
            ServiceControl.TERMID = termID;
            ServiceControl.APPLID = ConfigSettings.GetAppSettingsString("ApplID");

            if (userID.Contains(@"\"))
            {
                int slashPos = userID.LastIndexOf(@"\");
                ServiceControl.UserID = userID.Substring(slashPos + 1);

            }
            else
            {
                ServiceControl.UserID = userID;
            }
            ServiceControl.ClientSessionNumber = ServiceControl.GlobalSessionNumber;
            SimpleLogging.SessionNumber = ServiceControl.ClientSessionNumber;
            SimpleLogging.SessionID = ServiceControl.SessionID;

            if (ConfigSettings.GetAppSettingsString("CultureInfo") != null)
            {
                ServiceControl.CultureInfo = ConfigSettings.GetAppSettingsString("CultureInfo");
                CultureInfo.GetCultureInfo(ServiceControl.CultureInfo);
            }
            else
                ServiceControl.CultureInfo = CultureInfo.CurrentCulture.ToString();

            if (string.IsNullOrEmpty(ServiceControl.ServiceName))
            {
                string serviceURL = ConfigSettings.GetAppSettingsString("ServiceAddress");
                ServiceControl.ServiceName = serviceURL.Substring(serviceURL.LastIndexOf(@"/") + 1);
            }

        }

        /// <summary>
        /// Checks data from thread conversation
        /// </summary>
        private static void CheckThreadData()
        {

            try
            {

                Thread.CurrentThread.CurrentCulture = new CultureInfo(ServiceControl.CultureInfo);
                OperationContext.Current = ServiceControl.CurrentContext;

                // Set Custom Data from ICustomData implementation
                // ServiceControl.CustomData.SetCustomCollection();

                if (_control == null)
                    _control = new OnlineControl();

                _control.EIBTRMID.SetValue(ServiceControl.TERMID);
                CICSServiceItemKey CICSServiceItemKey = null;
                if (ServiceThreadShareData[0] is CICSServiceItemKey)
                {
                    CICSServiceItemKey = (CICSServiceItemKey)ServiceThreadShareData[0];
                    //CICSServiceItemControl serviceControl = (CICSServiceItemControl)ServiceThreadShareData.Find(o => o.Name.ToUpper() == "TRANSID");

                    if (CICSServiceItemKey != null)
                    {
                        if (CICSServiceItemKey.Name.Contains("EntryPoint:"))
                        {
                            string entryPoint = CICSServiceItemKey.Name.Substring(CICSServiceItemKey.Name.IndexOf(":") + 1).Trim();
                            if (entryPoint.Length < 5)
                            {
                                _control.ReturnTransID = entryPoint;
                                _control.EIBTRNID.SetValue(entryPoint);
                                _control.TransferProgram = string.Empty;
                            }
                            else
                                _control.TransferProgram = entryPoint;
                        }

                        while (CICSServiceItemKey.KeyPressed != "QUIT")
                        {

                            if (_control.DFHCOMMAREA == null)
                                _control.EIBCALEN.SetValue(0);
                            else
                                _control.EIBCALEN.SetValue(_control.DFHCOMMAREA.Length);

                            CICSServiceItemKey = (CICSServiceItemKey)ServiceThreadShareData[0];
                            _control.EIBAID.SetValue(DBSUtil.ConvertPFKey(CICSServiceItemKey.KeyPressed));
                            CheckTransactionID(CICSServiceItemKey);

                            if ((_control.TransferProgram == string.Empty && _control.ReturnTransID == string.Empty)
                                || (_control.isMapWaitingSend && _control.TransferProgram == string.Empty))  //Update to check if TransferProgram has a value to skip the Map send - Jetro issue 835
                            {
                                ExternalThreadHolder.Set();
                                CICSServiceItemKey = (CICSServiceItemKey)ServiceThreadShareData[0];
                                if (CICSServiceItemKey.KeyPressed == "QUIT")
                                {
                                    if (CICSServiceItemKey.Name.ToUpper() != "ERROR")
                                        return;
                                }
                                _control.isMapWaitingSend = false;
                                InternalThreadHolder.WaitOne();
                            }
                            if (_cancelThread.Count > 0 && _cancelThread[Thread.CurrentThread.ManagedThreadId])
                            {
                                bool flag = false;
                                _cancelThread.Remove(Thread.CurrentThread.ManagedThreadId, out flag);
                                break;
                            }
                        }
                        return;
                    }

                    //LastKey = CICSServiceItemKey.KeyPressed;
                    int cur_pos;
                    if (int.TryParse(CICSServiceItemKey.CurrentPosition, out cur_pos))
                    {
                    }
                    string currentControl = CICSServiceItemKey.CurrentControl;

                    ServiceThreadShareData.Add(new CICSServiceItemControl("Message", "Enter Transaction:", true, 80, "BLUE"));
                    ServiceThreadShareData.Add(new CICSServiceItemControl("TransID", "____", false, 4, ""));

                }
            }
            catch (Exception ex)
            {
                CatchError(ex);
            }
        }

        /// <summary>
        /// Check ServiceKey for control Transaction ID
        /// </summary>
        /// <param name="serviceKey"></param>
        private static void CheckTransactionID(CICSServiceItemKey serviceKey)
        {
            _control.ExitProgram = false;

            if (serviceKey.KeyPressed == "ESCAPE")
            {
                DBSUtil.ServiceThreadShareData.Clear();
                ServiceThreadShareData.Add(new CICSServiceItemKey("Quit", "QUIT", "", "1", false, ""));
                return;
            }

            if (serviceKey.Name == "EntryPoint:QUIT")
            {
                serviceKey.KeyPressed = "QUIT";
                return;
            }

            CICSServiceItemKey QuitProgram = (CICSServiceItemKey)ServiceThreadShareData[0];
            if (QuitProgram.KeyPressed == "QUIT")
                return;

            if (serviceKey.KeyPressed == "CLEAR" && _control.TransferProgram == "")
            {
                if (ServiceControl.TWARecord != null)
                    ServiceControl.TWARecord.InitializeWithLowValues();
            }

            if (_control.TransferProgram != null && _control.TransferProgram != string.Empty)
            {
                string newProgram = _control.TransferProgram;
                _control.TransferProgram = string.Empty;
                ExecuteProgram(newProgram);
                return;
            }

            if (_control.ReturnTransID == string.Empty || _control.ReturnTransID.IsMinValue())
                _control.ReturnTransID = _control.EIBTRNID.DisplayValue;

            try
            {
                if (TransactionControl.Instance.ProgramNames.ContainsKey(_control.ReturnTransID.ToUpper()))
                {
                    _control.EIBTRNID.SetValue(_control.ReturnTransID);
                    _control.ReturnTransID = string.Empty;
                    if (_control.isStartTransaction)
                    {
                        _control.DFHCOMMAREA = null;
                        _control.EIBCALEN.SetValue(0);
                        _control.isStartTransaction = false;
                        ServiceControl.TWARecord.InitializeWithLowValues();
                    }
                    ExecuteProgram(TransactionControl.Instance.GetProgramName(_control.EIBTRNID.AsString().ToUpper()));
                }
                else
                {
                    DBSUtil.ServiceThreadShareData.Clear();
                    DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemKey("Error", "QUIT", null, "0", true, ""));
                    System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

                    DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemControl("Message",
                        string.Concat("Cannot find transaction ID for ReturnTransID = '", _control.ReturnTransID.ToUpper(), "' ",
                        Environment.NewLine, Environment.NewLine, DBSUtil.GetLatestLogMessages(100)),
                        true, 80, "Red"));
                    _control.isMapWaitingSend = true;

                    SendToEventLog(string.Concat("Cannot find transaction ID for ReturnTransID = '", _control.ReturnTransID.ToUpper()));
                }
            }
            catch (Exception ex)
            {
                CatchError(ex);
            }
        }

        /// <summary>
        /// Execute Business Layer program
        /// </summary>
        /// <param name="programName"></param>
        private static void ExecuteProgram(string programName)
        {
            try
            {
                Type programType = DBSUtil.GetBLType(programName.Trim());
                OnlineProgramBase programInstance;
                if (programType == null) return;
                programInstance = (OnlineProgramBase)Activator.CreateInstance(programType, _control);

                DBSUtil.CheckProgramLogging(programType.Name, "Enter Program ");
                programInstance.ExecuteMain();
                DBSUtil.CheckProgramLogging(programType.Name, "Exit Program ");
                //if (ServiceControl.AppDbTransaction != null)             
                //{
                //    if (ServiceControl.AppDbTransaction.Connection != null)
                //    {
                //        ServiceControl.AppDbTransaction.Commit();
                //    }
                //    ServiceControl.AppDbTransaction = null;
                //}

                if (ServiceControl.CurrentException != null)
                {
                    throw new Exception("ExecuteProgram failed, please see inner exception for more details", ServiceControl.CurrentException);
                }

                if (_control.DFHCOMMAREA != null)
                {
                    _control.EIBCALEN.SetValue(_control.DFHCOMMAREA.Length);
                }
                CICSServiceItemKey CICSServiceItemKey = (CICSServiceItemKey)ServiceThreadShareData[0];
                if (CICSServiceItemKey.KeyPressed != "RETURN" && CICSServiceItemKey.KeyPressed != "QUIT")
                    CICSServiceItemKey.KeyPressed = DBSUtil.ConvertToPFKey(_control.EIBAID.DisplayValue);
            }
            catch (Exception ex)
            {
                CatchError(ex);
            }
        }

        private static void CatchError(Exception ex)
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

            DBSUtil.ServiceThreadShareData.Clear();
            DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemKey("Error", "QUIT", null, "0", true, ""));
            DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemControl("Message", errMessage.ToString(), true, 80, "Red"));

            SendToEventLog(errMessage.ToString());
        }

        private static void SendToEventLog(string logMessage)
        {
            if (!(logMessage.Contains("WaitHandle.InternalWaitOne(")))
            {
                try
                {
                    if (!System.Diagnostics.EventLog.SourceExists(ServiceControl.ServiceName))
                    {
                        System.Diagnostics.EventLog.CreateEventSource(ServiceControl.ServiceName, ServiceControl.ServiceName);
                    }

                    System.Diagnostics.EventLog appLog = new System.Diagnostics.EventLog();
                    appLog.Source = ServiceControl.ServiceName;
                    appLog.WriteEntry(string.Concat("Error from User: ", ServiceControl.UserID, Environment.NewLine, logMessage), System.Diagnostics.EventLogEntryType.Error);
                }
                catch
                {

                }
            }
        }
    }
}