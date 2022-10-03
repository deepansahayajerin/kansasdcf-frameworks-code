using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Threading;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Logging Messages
    /// </summary>
    public static class SimpleLogging
    {
        [ThreadStatic]
        private static string _DefaultLogFileName;
        private static string _DotLogFileExt = ".txt";
        private static long FileSizeLimit = 1000000;
        private static bool _NonMandatoryLoggingTurnedOn = false;
        private static bool _LogFileIncludeTimeStamp = true;
        private static bool _LogPreventOverlay = false;
        private static bool PrintLogs = false;
        [ThreadStatic]
        private static int _sessionNumber;
        [ThreadStatic]
        private static string _sessionID;

        static SimpleLogging()
        {
            string sNonMandatoryLogging = ConfigSettings.GetAppSettingsString("NonMandatoryLogging");
            string sLogFileErrorOnly = ConfigSettings.GetAppSettingsString("LogFileErrorOnly");
            string sLogFileEnabled = ConfigSettings.GetAppSettingsString("LogFileEnabled");
            _LogPreventOverlay = ConfigSettings.GetAppSettingsBool("LogPreventOverlay");

            //by default include the TimeStamp
            //however the default GetAppSettingsBool returns false if the key is not found
            //lets work around it
            if (ConfigSettings.GetAppSettingsString("LogIncludeTimestamp") != String.Empty)
                _LogFileIncludeTimeStamp = ConfigSettings.GetAppSettingsBool("LogIncludeTimestamp");
            //end the work around

            if ((sNonMandatoryLogging != null && sNonMandatoryLogging.ToLower() == "true") ||
                ((sLogFileEnabled != null && sLogFileEnabled.ToLower() == "true") && (sLogFileErrorOnly == null || sLogFileErrorOnly.ToLower() == "false")))
            {
                _NonMandatoryLoggingTurnedOn = true;
            }
            if (!(String.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("LogFileSize"))))
            {
                string sSize = ConfigSettings.GetAppSettingsString("LogFileSize");
                FileSizeLimit = (Convert.ToInt64(sSize)) * 1000000;
            }
            if (!string.IsNullOrEmpty(sLogFileEnabled) && sLogFileEnabled.ToUpper() == "TRUE")
            {
                PrintLogs = true;
            }
        }

        /// <summary>
        /// Returns the Log file without the .txt or .TXT
        /// </summary>
        public static string RemoveTXTExtension(string currString)
        {
            string returnString = currString;

            if (currString.EndsWith(_DotLogFileExt))
            {
                returnString = currString.Remove(currString.LastIndexOf(_DotLogFileExt));
            }
            else if (currString.EndsWith(_DotLogFileExt.ToUpper()))
            {
                returnString = currString.Remove(currString.LastIndexOf(_DotLogFileExt.ToUpper()));
            }

            return returnString;
        }
        /// <summary>
        /// Returns the Log file name
        /// </summary>
        public static string DefaultLogFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_DefaultLogFileName))
                {
                    string sp = ConfigSettings.GetAppSettingsString("LogFileName");
                    if (!(String.IsNullOrEmpty(sp)))
                    {
                        _DefaultLogFileName = sp;

                        /// this tells us if the _activeLogFileName contains a full path
                        if ((!_DefaultLogFileName.Contains(":") && (!_DefaultLogFileName.Contains(@"\\"))))
                        {
                            _DefaultLogFileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _DefaultLogFileName);
                        }
                    }
                    else
                    {
                        _DefaultLogFileName = "c:\\MDSYLog" + _DotLogFileExt;
                    }

                    //Log By User?
                    string logByUser = ConfigSettings.GetAppSettingsString("LogByUser");
                    if (logByUser.ToUpper() == "TRUE")
                    {
                        _DefaultLogFileName = RemoveTXTExtension(_DefaultLogFileName);
                        if (!string.IsNullOrEmpty(GlobalVariables.UserID))
                        {
                            _DefaultLogFileName = String.Concat(_DefaultLogFileName, "_", GlobalVariables.UserID.Trim(), _DotLogFileExt);
                        }
                    }

                    string logBySession = ConfigSettings.GetAppSettingsString("LogBySession");
                    //Log By Date?
                    string logByDate = ConfigSettings.GetAppSettingsString("LogByDate");
                    if (logByDate.ToUpper() == "TRUE")
                    {
                        _DefaultLogFileName = RemoveTXTExtension(_DefaultLogFileName);
                        _DefaultLogFileName = String.Concat(_DefaultLogFileName, "_", DateTime.Now.ToString("yyyyMMddhhmmss"), _DotLogFileExt);
                    }
                    else
                    //Log By Session
                    if (logBySession.ToUpper() == "TRUE")
                    {
                        _DefaultLogFileName = RemoveTXTExtension(_DefaultLogFileName);
                        if (!string.IsNullOrEmpty(_sessionID))
                        {
                            _DefaultLogFileName = String.Concat(_DefaultLogFileName, "_", _sessionID.Replace("=", "").Replace(";", "").Replace(":", "").Replace(",", ""), _DotLogFileExt);
                        }
                        else
                        {
                            if (_sessionNumber == 0)
                            {
                                _DefaultLogFileName = String.Concat(_DefaultLogFileName, "_", DateTime.Now.ToFileTime(), _DotLogFileExt);
                            }
                            else
                            {
                                _DefaultLogFileName = String.Concat(_DefaultLogFileName, "_", _sessionNumber.ToString().PadLeft(9, '0'), _DotLogFileExt);
                            }
                        }
                    }
                }
                return _DefaultLogFileName;
            }
        }

        /// <summary>
        /// Use this property to turn non-mandatory logging on or off. Typically to be used
        /// in conjunction with a config setting.
        /// </summary>
        public static bool NonMandatoryLoggingActive
        {
            get { return _NonMandatoryLoggingTurnedOn; }
            set { _NonMandatoryLoggingTurnedOn = value; }
        }

        /// <summary>
        /// Returns Session Number
        /// </summary>
        public static int SessionNumber
        {
            get { return _sessionNumber; }
            set { _sessionNumber = value; }
        }

        public static string SessionID
        {
            get { return _sessionID; }
            set { _sessionID = value; }
        }

        /// <summary>
        /// Mandatory messages are unaffected by whether non-mandatory logging is
        /// active or not. Example of mandatory logging: Exceptions trace.
        /// No filename supplied, so use the default (see const)
        /// </summary>
        /// <param name="msg">Message to be logged</param>
        public static void LogMandatoryMessageToFile(string msg)
        {
            _LogMessageToFile(DefaultLogFileName, msg);
        }

        /// <summary>
        /// Mandatory messages are unaffected by whether non-mandatory logging is
        /// active or not. Example of mandatory logging: Exceptions trace.
        /// No filename supplied, so use the default (see const)
        /// </summary>
        /// <param name="msg">Message to be logged</param>
        /// <param name="userID">user ID</param>
        /// <param name="backgroundWorker">Flaf to indicate that it was initiated by a background Worker task</param>
        public static void LogMandatoryMessageToFile(string msg, string userID, bool backgroundWorker)
        {
            if (userID != "")
            {
                if (!DefaultLogFileName.Contains(userID.Trim()))
                {
                    _DefaultLogFileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ConfigSettings.GetAppSettingsString("LogFileName"));
                    _DefaultLogFileName = String.Concat(_DefaultLogFileName, "_", userID.Replace("=", "").Replace(";", "").Replace(":", "").Replace(",", ""), _DotLogFileExt);
                }
            }
            _LogMessageToFile(DefaultLogFileName, msg);
        }


        public static string RetrieveLatestLogMessages(int msgCount)
        {
            return _RetrieveLogMessages(DefaultLogFileName, msgCount);
        }

        /// <summary>
        /// Mandatory messages are unaffected by whether non-mandatory logging is
        /// active or not. Example of mandatory logging: Exceptions trace.
        /// </summary>
        /// <param name="filename">File name for log file</param>
        /// <param name="msg">Message to be logged</param>
        public static void LogMandatoryMessageToFile(string filename, string msg)
        {
            _LogMessageToFile(filename, msg);
        }

        /// <summary>
        /// These logging functions are non-mandatory, and thus rely on
        /// whether or not non-mandatory logging is turned on or off.
        /// Example use: logging every action key pressed by a user. You'd
        /// probably not want to do that in production; thus, non-mandatory.
        /// </summary>
        /// <param name="msg">Message to be logged</param>
        public static void LogMessageToFile(string msg)
        {
            if (_NonMandatoryLoggingTurnedOn)
            {
                _LogMessageToFile(DefaultLogFileName, msg);
            }
        }

        /// <summary>
        /// These logging functions are non-mandatory, and thus rely on
        /// whether or not non-mandatory logging is turned on or off.
        /// Example use: logging every action key pressed by a user. You'd
        /// probably not want to do that in production; thus, non-mandatory.
        /// </summary>
        /// <param name="filename">Log file name</param>
        /// <param name="msg">Message to be logged</param>
        public static void LogMessageToFile(string filename, string msg)
        {
            if (_NonMandatoryLoggingTurnedOn)
            {
                _LogMessageToFile(filename, msg);
            }
        }

        private static void _LogMessageToFile(string filename, string msg)
        {
            if (!PrintLogs)
            {
                return;
            }
            TextWriter tw = null;
            string logLine = "";

            try
            {
                FileInfo fi = new FileInfo(filename);
                if (fi == null || fi.Exists == false || fi.Length > FileSizeLimit)
                {       
                    if (System.IO.File.Exists(filename) && _LogPreventOverlay)
                    {
                        if (fi.Length > FileSizeLimit)
                        {
                            System.IO.File.Copy(filename, filename.Replace(_DotLogFileExt, "_" + DateTime.Now.ToString("HH_mm_ss") + _DotLogFileExt));
                        }
                    }
                    //Create the file if any othe following conditions is true:
                    //We can't get info on it.
                    //It doesn't exist.
                    //If the file grew over our limit.
                    tw = TextWriter.Synchronized(File.CreateText(filename));
                }
                else
                {
                    int twCount = 0;
                    while (twCount < 20)
                    {
                        try
                        {
                            tw = TextWriter.Synchronized(File.AppendText(filename));
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(100);
                            twCount++;
                        }
                    }
                    if (twCount == 20)
                    {
                        //using (EventLog eventLog = new EventLog("Application"))
                        //{
                        //    eventLog.Source = "Application";
                        //    eventLog.WriteEntry("Warning - Could not append to file " + filename + " Log Message: " + msg, EventLogEntryType.Warning);
                        //}
                        return;
                    }
                }
                Exception exception = new Exception();
                int attempts = 0;
                while (attempts < 10)
                {
                    try
                    {
                        if (_LogFileIncludeTimeStamp)
                            logLine = System.String.Format("LOG {0} {1}: {2}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFF"), Thread.CurrentThread.Name, "\r\n" + msg);
                        else
                            logLine = msg;

                        tw.WriteLine(logLine);
                        tw.Flush();
                        tw.Close();
                        break;
                    }
                    catch (Exception exc)
                    {
                        exception = exc;
                        Thread.Sleep(100);
                        attempts++;
                    }
                }
                if (tw != null)
                    tw.Close();

                if (attempts == 10)
                {
                    //using (EventLog eventLog = new EventLog("Application"))
                    //{
                    //    eventLog.Source = "Application";
                    //    eventLog.WriteEntry("Warning - Writing of log message after " + attempts + " attempts in file " + filename + " failed: " + exception.Message + " Log Message: " + msg, EventLogEntryType.Warning);
                    //}
                }
            }
            catch (Exception ex)
            {
                //using (EventLog eventLog = new EventLog("Application"))
                //{
                //    eventLog.Source = "Application";
                //    eventLog.WriteEntry("Warning - Writing of log message in file " + filename + " failed " + ex.Message + " Log Message: " + msg, EventLogEntryType.Warning);
                //}
            }
        }
        private static string _RetrieveLogMessages(string filename, int msgCount)
        {

            FileInfo fi = new FileInfo(filename);
            if (fi == null || fi.Exists == false || fi.Length == 0)
            {
                return string.Empty;
            }
            try
            {
                StringBuilder returnString = new StringBuilder();
                string[] fileLines = File.ReadAllLines(filename);
                int firstLine = 0;
                if (fileLines.Length > msgCount)
                {
                    firstLine = fileLines.Length - msgCount;
                }

                for (int ctr = fileLines.Length - 1; ctr >= firstLine; ctr--)
                {
                    returnString.AppendLine(fileLines[ctr]);
                }

                return returnString.ToString();
            }
            catch
            {
                return string.Empty;
            }

        }
    }
}
