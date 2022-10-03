using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using MDSY.Framework.Service.Interfaces;
using MDSY.Framework.Control.CICS;
using MDSY.Utilities.Security;
using Microsoft.AspNetCore.Identity;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Control.CICS
{
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, TransactionTimeout = "00:10:00")]
    [Serializable]
    public class Service : ICICSService
    {
        #region private attributes
        private string _userID;
        private string _opID;
        private string _termID;
        private string _sessionID;
        private OperationContext _currentContext;
        private CustomServiceData _customData;
        private Thread _thread;
        private EventWaitHandle _internalWait = new AutoResetEvent(false);
        private EventWaitHandle _externalWait = new AutoResetEvent(false);

        private List<IAterasServiceItem> _currentControls = new List<IAterasServiceItem>();
        #endregion

        #region private methods

        private void SystemGoThread(object threadShareData)
        {
            try
            {
                Thread.CurrentThread.Name = "Service" + _userID + "-" + DateTime.Now.Ticks.ToString();
                Thread.CurrentThread.IsBackground = true;
                ServiceController.ThreadShareData = (List<IAterasServiceItem>)threadShareData;

                // run application on this thread.
                ServiceController.InternalThreadHolder = _internalWait;
                ServiceController.ExternalThreadHolder = _externalWait;
                ServiceController.SetUpSessionVariables(_userID, _opID, _termID, _sessionID, _currentContext, _customData);
                if (ConfigSettings.GetAppSettingsStringFromSection("Namespaces", "BLAssemblyName").Contains("Jetro"))
                    ServiceController.SetUpTFLSessionVariables(CustomDataServices._t_classA, CustomDataServices._t_classB, CustomDataServices._t_cfacu, 
                                                           CustomDataServices._t_cwhse, CustomDataServices._t_pcipl, CustomDataServices._t_pcilp);
                ServiceController.RunThread();
            }
            catch 
            {
                
            }
        }

        private void SystemGo()
        {
            _externalWait.Reset();
            if ((_thread == null) || (!_thread.IsAlive))
            {
                SetUpSessionVariables();

                _thread = new Thread(new ParameterizedThreadStart(SystemGoThread));

                _thread.Start(_currentControls);

            }
            else
            {
                _internalWait.Set(); // make the inner thread go.
            }

            _externalWait.WaitOne(); // make this thread stop for now.
        }

        private void SetUpSessionVariables()
        {
            ServiceControl.GlobalSessionNumber++;
        }
        #endregion

        public Service(string sessionID)
        {
            _sessionID = sessionID;
        }

        #region public methods
        /// <summary>
        /// Simple test of connection for the Service system.
        /// </summary>
        /// <returns></returns>
        public bool Test()
        {
            return true;
        }
        public void SetValues(List<IAterasServiceItem> controlList)
        {
            _currentControls.Clear();
            for (int count = 0; count < controlList.Count; count++)
            {
                _currentControls.Add(controlList[count]);
            }
        }
        public void Run()
        {
            try
            {
                SystemGo();
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                Console.ReadLine();
            }
        }

        public List<IAterasServiceItem> Run(List<IAterasServiceItem> controlList)
        {
            try
            {
                SetValues(controlList);
                SystemGo();
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                Console.ReadLine();
            }
            return GetValues();
        }
        public List<IAterasServiceItem> GetValues()
        {
            return _currentControls;
        }

        public string SessionId()
        {
            return _sessionID;
        }

        public CustomServiceData GetCustomData()
        {
            return ServiceControl.CustomData;
        }

        public void SetCustomData(CustomServiceData customData)
        {
            try
            {
                ServiceControl.CustomData = customData;
                ServiceControl.CustomData.SetCustomCollection();
                _customData = ServiceControl.CustomData;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        public void Initialize(string LoginUserID, string TermID)
        {
            try
            {
                _termID = TermID;

                if (LoginUserID == null)
                    LoginUserID = "";
                if (ConfigSettings.GetAppSettingsString("LoginIDType") == "mainframe")
                {
                    _userID = Security.GetMainframeIDFromApplicationSecurity(LoginUserID); //ServiceSecurityContext.Current.PrimaryIdentity.Name;
                    _opID = Security.GetOPIDFromApplicationSecurity(_userID);
                    _termID = Security.GetTERMIDFromApplicationSecurity(_userID);
                }
                else if (ConfigSettings.GetAppSettingsString("LoginIDType") == "login")
                {
                    _userID = LoginUserID;
                    _opID = Security.GetOPIDFromApplicationSecurity(LoginUserID);
                    _termID = Security.GetTERMIDFromApplicationSecurity(_userID);
                    if (string.IsNullOrEmpty(_termID))
                        _termID = _userID;
                }
                else
                {
                    _userID = LoginUserID; //ServiceSecurityContext.Current.PrimaryIdentity.Name;
                    string mainID = Security.GetMainframeIDFromApplicationSecurity(LoginUserID);
                    _opID = Security.GetOPIDFromApplicationSecurity(mainID);
                    _termID = Security.GetTERMIDFromApplicationSecurity(mainID);
                    if (string.IsNullOrEmpty(_termID))
                        _termID = _userID;
                }
                //if (ConfigSettings.GetAppSettingsString("BLAssemblyName").Contains("Jetro"))
                //    CustomDataServices.GetTFLData(_userID);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                throw new Exception("Initialize method failed. Please see Internal exception for more details.", ex);
            }
        }

        public void Cleanup()
        {
            Dispose();
        }
        public void Dispose()
        {
            if ((_thread != null) && (_thread.IsAlive))
            {
                //_thread.Abort();
                ServiceController._cancelThread[_thread.ManagedThreadId] = true;
                _internalWait.Set();
                _externalWait.Set();
                _thread.Join();
                _thread = null;
            }
        }
        #endregion
    }
}
