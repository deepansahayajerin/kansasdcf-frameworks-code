using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
//using System.Configuration;
using Microsoft.Extensions.Configuration;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.UI.Angular
{
    [Serializable]
    public class MDSYSession
    {
        private static Dictionary<string, MDSYSession> _sessionObjects = new Dictionary<string, MDSYSession>();
        private static int _sessionTimeout = int.Parse(string.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("SessionTimeout"))
            ? "20" 
            : ConfigSettings.GetAppSettingsString("SessionTimeout"));
        private static string _connectionStrings = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");
        //private static int _sessionTimeout = int.Parse(ConfigurationManager.AppSettings["SessionTimeout"] ?? "20");
        //private static string _connectionString = ConfigurationManager.ConnectionStrings["SecurityConnectionString"].ToString();

        private readonly Dictionary<string, object> _sessionData = new Dictionary<string, object>();
        private readonly string _sessionId;
        private DateTime _lastAccessed;

        private MDSYSession(string sessionId)
        {
            _sessionId = sessionId;
            _lastAccessed = DateTime.Now;
        }

        public static MDSYSession CreateSession(string sessionId)
        {
            CleanSessions();

            MDSYSession newSession = new MDSYSession(sessionId);
            lock (_sessionObjects)
            {
                _sessionObjects.Add(sessionId, newSession);
            }

            return newSession;
        }

        public void Close()
        {
            lock (_sessionObjects)
            {
                string id = this.Id;
                this._sessionData.Clear();
                if (_sessionObjects.ContainsKey(id))
                    _sessionObjects.Remove(id);
            }
        }

        public static MDSYSession GetSession(String sessionId)
        {
            CleanSessions();

            MDSYSession session = null;

            lock (_sessionObjects)
            {
                if (_sessionObjects.ContainsKey(sessionId))
                    session = _sessionObjects[sessionId];
                else
                    session = MDSYSession.CreateSession(sessionId);
            }

            return session;
        }

        private static void CleanSessions()
        {
            lock (_sessionObjects)
            {
                try
                {
                    List<string> outdatedSessions = new List<string>();

                    foreach (MDSYSession session in _sessionObjects.Values)
                    {
                        if (session._lastAccessed.AddMinutes(_sessionTimeout) < DateTime.Now)
                            outdatedSessions.Add(session.Id);
                    }

                    foreach (string id in outdatedSessions)
                        _sessionObjects[id].Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    throw;
                }
            }
        }

        public object this[string name]
        {
            get
            {
                _lastAccessed = DateTime.Now;
                return _sessionData.ContainsKey(name) ? _sessionData[name] : null;
            }
            set
            {
                _lastAccessed = DateTime.Now;
                _sessionData[name] = value;
            }
        }

        public string Id
        {
            get 
            {
                _lastAccessed = DateTime.Now;
                return _sessionId; 
            }
        }
    }
}
