using Microsoft.Win32;
using System;
using System.IO;
//using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.IO.Common
{
    #region enum
    /// <summary>
    /// Possible actions for the RefreshFromRegistry
    /// </summary>
    public enum RefreshFromRegistryOption
    {
        DatabaseServerName,
        /// <summary>
        /// The Alchemy drop directory
        /// </summary>
        RJEFolderPath
    }

    public enum PFKeysDisplayFormat
    {
        Normal, // %YN - Normal tabular Software AG format (1 to 12)
        Sequential, // %YS - sequential format
        PCSequential // %YP
    }

    public enum PFKeysRangeOfDisplayedKeys
    {
        YF, // displays first range of function keys (1 to 12)
        YL // displays last range of function keys (13 to 24)
    }
    #endregion

    /// <summary>
    /// This class allows you to handle specific events on the settings class:
    /// The SettingChanging event is raised before a setting's value is changed.
    /// The PropertyChanged event is raised after a setting's value is changed.
    /// The SettingsLoaded event is raised after the setting values are loaded.
    /// The SettingsSaving event is raised before the setting values are saved.
    /// </summary>
    public static partial class Settings
    {
        private static readonly string STR_SettingsAddLock = "STR_SettingsAddLockaosnetuh123[098";
        private static readonly string STR_SortEngineNeoSort = "NeoSort";
        private static readonly string STR_SortEngineAteras = "Ateras";
        private static readonly string STR_ExternalSort_SortEngineProvider = "ExternalSort_SortEngineProvider";
        private static readonly string STR_ExternalSort_UseParallelProcessing = "ExternalSort_UseParallelProcessing";
        //private static readonly string STR_ExternalSort_ProcessSecondPassInMemory = "ExternalSort_ProcessSecondPassInMemory";
        private static readonly string STR_ExternalSort_ReadRecordCount = "ExternalSort_ReadRecordCount";
        private static readonly string STR_ThrowOverflowException = "ThrowOverflowException";

        #region private static properties

        private static string _configurationEntryInitLock = "_configurationEntryInitLocksntaoec.93948";
        
        //private static Configuration _configurationEntry;
        [ThreadStatic]
        private static string _messagePosition;
        [ThreadStatic]
        private static PFKeysDisplayFormat _PFKeysDisplayFormat;
        [ThreadStatic]
        private static PFKeysRangeOfDisplayedKeys _PFKeysRangeOfDisplayedKeys = Common.PFKeysRangeOfDisplayedKeys.YF;
        [ThreadStatic]
        private static bool _PFKeysBothLines;
        [ThreadStatic]
        private static int _PFKeysLineNumber;
        [ThreadStatic]
        private static bool _PFKeysAtTop;
        [ThreadStatic]
        private static bool? _lowerCase = null;
        [ThreadStatic]
        private static bool? _throwOverflowException = null;

        private static char? _decimalSeparator = null;
        private static bool? _logSMTPEnabled = null;
        private static bool? _logFileEnabled = null;
        private static bool? _supportOld = null;

        //private static Configuration ConfigurationEntry
        //{
        //    get
        //    {
        //        if (_configurationEntry == null)
        //        {
        //            bool lockTaken = false;
        //            Monitor.Enter(_configurationEntryInitLock, ref lockTaken);
        //            try
        //            {
        //                if (_configurationEntry == null)
        //                {
        //                    /// try normal mode 
        //                    _configurationEntry = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        //                }
        //            }
        //            catch (Exception ex)
        //            {

        //                throw new Exception("The requested ConfigurationEntry could not be loaded", ex);

        //            }
        //            finally
        //            {
        //                if (lockTaken)
        //                {
        //                    Monitor.Exit(_configurationEntryInitLock);
        //                }
        //            }
        //        }

        //        return _configurationEntry;
        //    }
        //}
        #endregion

        #region public static properties

        public static bool ThrowOverflowException
        {
            get
            {
                if (_throwOverflowException == null)
                    _throwOverflowException = GetConfigElement(STR_ThrowOverflowException, false);
                return (bool)_throwOverflowException;
            }
            set
            {
                _throwOverflowException = value;
            }
        }
        #region log
        public static int LogMessageCacheSize
        {
            get
            {
                return GetConfigElement("LogMessageCacheSize", 100);
            }
        }
        #region log smtp
        public static bool LogSMTPEnabled
        {
            get
            {
                if (_logSMTPEnabled == null)
                    _logSMTPEnabled = GetConfigElement("LogSMTPEnabled", false);
                return (bool)_logSMTPEnabled;
            }
            //set
            //{
            //    SetConfigElement("LogSMTPEnabled", value);
            //}
        }
        public static bool LogSMTPErrorOnly
        {
            get
            {
                return GetConfigElement("LogSMTPErrorOnly", true);
            }
        }
        public static string LogSMTPServer
        {
            get
            {
                return GetConfigElement("LogSMTPServer", "mail.server.com");
            }
        }
        public static int LogSMTPPort
        {
            get
            {
                return GetConfigElement("LogSMTPPort", 25);
            }
        }
        public static bool LogSMTPUseCurrentUserCredentials
        {
            get
            {
                return GetConfigElement("LogSMTPUseCurrentUserCredentials", true);
            }
        }
        public static string LogSMTPUserName
        {
            get
            {
                return GetConfigElement("LogSMTPUserName", "user@server.com");
            }
        }
        public static string LogSMTPPassword
        {
            get
            {
                return GetConfigElement("LogSMTPPassword", "mypassword");
            }
        }
        public static string LogSMTPFrom
        {
            get
            {
                return GetConfigElement("LogSMTPFrom", "fromUser@server.com");
            }
        }
        public static string LogSMTPTo
        {
            get
            {
                return GetConfigElement("LogSMTPTo", "toUser@server.com");
            }
        }
        public static string LogSMTPSubject
        {
            get
            {
                return GetConfigElement("SMTPSubject", "Log entry from Ateras Framework.");
            }
        }

        public static bool Autoskip
        {
            get
            {
                return GetConfigElement("Autoskip", true);
            }
        }

        #endregion
        #region log file
        public static bool LogFileEnabled
        {
            get
            {
                if (_logFileEnabled == null)
                    _logFileEnabled = GetConfigElement("LogFileEnabled", true);
                return (bool)_logFileEnabled;
            }
            //set
            //{
            //    SetConfigElement("LogFileEnabled", value);
            //}
        }
        public static bool LogBySession
        {
            get
            {
                return GetConfigElement("LogBySession", false);
            }
        }
        public static bool LogByUser
        {
            get
            {
                return GetConfigElement("LogByUser", false);
            }
        }
        public static bool LogByDate
        {
            get
            {
                return GetConfigElement("LogByDate", false);
            }
        }
        public static bool LogFileErrorOnly
        {
            get
            {
                return GetConfigElement("LogFileErrorOnly", true);
            }
        }
        public static bool LogFileProgramEntries
        {
            get
            {
                return GetConfigElement("LogFileProgramEntries", true);
            }
        }
        public static string LogFileName
        {
            get
            {
                return GetConfigElement("LogFileName", "C:/ATERAS.txt");
            }
        }
        public static long LogFileSizeLimit
        {
            get
            {
                return GetConfigElement("LogFileSizeLimit", 5000000);
            }
        }
        public static bool LogFileOverwrite
        {
            get
            {
                return GetConfigElement("LogFileOverwrite", true);
            }
        }
        #endregion
        public static bool SupportOld
        {
            get
            {
                if (_supportOld == null)
                    _supportOld = GetConfigElement("SupportOld", false);
                return (bool)_supportOld;
            }

        }
        #endregion
        /// <summary>
        /// Gets or sets whether the second pass through data during an external 
        /// sort operation is handled in memory. Defaults to <c>true</c>.
        /// </summary>
        public static bool ExternalSort_ProcessSecondPassInMemory
        {
            get
            {
                // processing 2nd pass externally is not yet implemented. If you enable
                // the next line, you'll have to write support...
                //return GetConfigElement(STR_ExternalSort_ProcessSecondPassInMemory, true);
                return true;
            }
            set
            {
                //SetConfigElement(STR_ExternalSort_ProcessSecondPassInMemory, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of records to read in each scoop through an external data source 
        /// during the first pass of an external file sort operation. Defaults to <c>100</c>.
        /// </summary>
        public static int ExternalSort_ReadRecordCount
        {
            get
            {
                return GetConfigElement(STR_ExternalSort_ReadRecordCount, 100);
            }
        }

        public static string ExternalSort_SortEngineProvider
        {
            get
            {
                //return GetConfigElement(STR_ExternalSort_SortEngineProvider, STR_SortEngineAteras);
                return GetConfigElement(STR_ExternalSort_SortEngineProvider, STR_SortEngineNeoSort);
            }
        }

        public static bool ExternalSort_UseAterasSort
        {
            get
            {
                return (ExternalSort_SortEngineProvider == STR_SortEngineAteras);
            }
        }
        public static bool ExternalSort_UseNeoSort
        {
            get { return (ExternalSort_SortEngineProvider == STR_SortEngineNeoSort); }
        }

        /// <summary>
        /// Gets or sets whether the external sort process should use PLINQ when possible. 
        /// Defaults to <c>true</c>.
        /// </summary>
        /// <remarks>
        /// <note>This is not guaranteed to result in performance gains.</note>
        /// Performance is based on numerous factors; setting this to <c>true</c> might 
        /// improve performance in large datasets on multi-core servers. 
        /// </remarks>
        public static bool ExternalSort_UseParallelProcessing
        {
            get
            {
                return GetConfigElement(STR_ExternalSort_UseParallelProcessing, true);
            }
        }

        /// <summary>
        /// Gets or sets the Natural profile and session parameter determines the default mode for video-terminal input.
        /// </summary>
        public static char InputMode
        {
            get
            {
                return GetConfigElement("InputMode", 'F');
            }
        }

        /// <summary>
        /// Gets or sets the Natural profile and session parameter specifies the control character for Natural terminal 
        /// commands; that is, the character which is to be used as the first character of any terminal command.
        /// </summary>
        public static char CharacterForTerminalCommands
        {
            get
            {
                return GetConfigElement("CharacterForTerminalCommands", '%');
            }
        }

        public static char HelpCharacter
        {
            get
            {
                return GetConfigElement("HelpCharacter", '?');
            }
        }
        /// <summary>
        /// This Natural profile and session parameter defines the character to be used as a delimiter character for INPUT statements in keyword/delimiter mode.
        /// Within a Natural session, the profile parameter ID> can be overridden by the session parameter <ID>.
        /// </summary>
        public static char InputDelimiterCharacter
        {
            get
            {
                return GetConfigElement("InputDelimiterCharacter", ',');
            }
        }
        public static bool ForceTerminalWindowSize
        {
            get
            {
                return GetConfigElement("ForceTerminalWindowSize", true);
            }
        }
        public static int BlinkMessageCount
        {
            get
            {
                return GetConfigElement("BlinkMessageCount", 2);
            }
        }
        public static Color ColorBlue
        {
            get
            {
                return GetConfigElement("ColorBlue", Color.LightBlue);
            }
        }
        public static Color ColorGreen
        {
            get
            {
                return GetConfigElement("ColorGreen", Color.LimeGreen);
            }
        }
        public static Color ColorPink
        {
            get
            {
                return GetConfigElement("ColorPink", Color.Pink);
            }
        }
        public static Color ColorRed
        {
            get
            {
                return GetConfigElement("ColorRed", Color.Red);
            }
        }
        public static Color ColorTurquoise
        {
            get
            {
                return GetConfigElement("ColorTurquoise", Color.Turquoise);
            }
        }
        public static Color ColorYellow
        {
            get
            {
                return GetConfigElement("ColorYellow", Color.Yellow);
            }
        }
        public static Color Protected
        {
            get
            {
                return GetConfigElement("ProtectedARGB", Color.Cyan);
            }
        }
        public static Color Background
        {
            get
            {
                return GetConfigElement("BackgroundARGB", Color.Black); ;
            }
        }
        public static Color ProtectedIntense
        {
            get
            {
                return GetConfigElement("ProtectedIntenseARGB", Color.White);
            }
        }
        public static Color UnprotectedSelected
        {
            get
            {
                return GetConfigElement("UnprotectedSelectedARGB", Color.Lime);
            }
        }
        public static Color UnprotectedIntense
        {
            get
            {
                return GetConfigElement("UnprotectedIntenseARGB", Color.White);
            }
        }
        public static Color Unprotected
        {
            get
            {
                return GetConfigElement("UnprotectedARGB", Color.Cyan);
            }
        }
        /// <summary>
        /// Gets or sets whether the external sort process should use PLINQ when possible. 
        /// Defaults to <c>true</c>.
        /// </summary>
        /// <remarks>
        /// <note>This is not guaranteed to result in performance gains.</note>
        /// Performance is based on numerous factors; setting this to <c>true</c> might 
        /// improve performance in large datasets on multi-core servers. 
        /// </remarks>
        public static bool IsBatch
        {
            get
            {
                return GetConfigElement("IsBatch", false);
            }
        }
        public static char DecimalSeparator
        {
            get
            {
                if (_decimalSeparator == null)
                    _decimalSeparator = GetConfigElement("DecimalSeparator", '.');
                return (char)_decimalSeparator;
            }
        }
        public static string CompressDefaultDelimiter
        {
            get
            {
                return GetConfigElement("CompressDefaultDelimiter", ",");
            }
        }

        public static int LineSize
        {
            get
            {
                return GetConfigElement("LineSize", 80); ;
            }
        }
        public static int PageSize
        {
            get
            {
                return GetConfigElement("PageSize", 25);
            }
        }
        public static int LOG_LS
        {
            get
            {
                return GetConfigElement("Logical_LineSize", 80); ;
            }
        }
        public static int LOG_PS
        {
            get
            {
                return GetConfigElement("Logical_PageSize", 60); ;
            }
        }
        public static char DTFORM
        {
            get
            {
                return GetConfigElement("DTFORM", "I")[0];
            }
        }

        public static int ULANG
        {
            get
            {
                return GetConfigElement("ULANG", 1);
            }
        }

    public static string DefaultDateFormatCs
        {
            get
            {
                string tmp = "";
                switch (DTFORM)
                {
                    case 'E':
                        switch (DF)
                        {
                            case 'S':
                                tmp = "dd/MM/yy";
                                break;
                            case 'I':
                                tmp = "ddMMyyyy";
                                break;
                            case 'L':
                                tmp = "dd/MM/yyyy";
                                break;
                            default:
                                throw new Exception("DF parameter has invalid value '" + DF + "'");
                        }
                        break;
                    case 'G':
                        switch (DF)
                        {
                            case 'S':
                                tmp = "dd.MM.yy";
                                break;
                            case 'I':
                                tmp = "ddMMyyyy";
                                break;
                            case 'L':
                                tmp = "dd.MM.yyyy";
                                break;
                            default:
                                throw new Exception("DF parameter has invalid value '" + DF + "'");
                        }
                        break;
                    case 'I':
                        switch (DF)
                        {
                            case 'S':
                                tmp = "yy-MM-dd";
                                break;
                            case 'I':
                                tmp = "yyyyMMdd";
                                break;
                            case 'L':
                                tmp = "yyyy-MM-dd";
                                break;
                            default:
                                throw new Exception("DF parameter has invalid value '" + DF + "'");
                        }
                        break;
                    case 'U':
                        switch (DF)
                        {
                            case 'S':
                                tmp = "MM/dd/yy";
                                break;
                            case 'I':
                                tmp = "MMddyyyy";
                                break;
                            case 'L':
                                tmp = "MM/dd/yyyy";
                                break;
                            default:
                                throw new Exception("DF parameter has invalid value '" + DF + "'");
                        }
                        break;
                    default:
                        throw new Exception("DTFORM parameter has an valid value '" + DTFORM + "'");

                }
                return tmp;
            }
        }
        public static char ThousandSeparator
        {
            get
            {
                return GetConfigElement("ThousandSeparator", ',');
            }
        }

        public static int AterasTextAreaWidth
        {
            get
            {
                return GetConfigElement("AterasTextAreaWidth", 10);
            }
        }
        public static int AterasTextAreaHeight
        {
            get
            {
                return GetConfigElement("AterasTextAreaHeight", 25);
            }
        }
        public static int AterasTextAreaToolStripHeight
        {
            get
            {
                return GetConfigElement("AterasTextAreaToolStripHeight", 25);
            }
        }
        public static int AterasTextAreaModifier
        {
            get
            {
                return GetConfigElement("AterasTextAreaModifier", 2);
            }
        }
        public static char NegativeSign
        {
            get
            {
                return GetConfigElement("NegativeSign", '-');
            }
        }
        public static char PositiveSign
        {
            get
            {
                return GetConfigElement("PositiveSign", '+');
            }
        }
        public static string StartProgram
        {
            get
            {
                return GetConfigElement("StartProgram", "");
            }
        }

        /// <summary>
        /// Gets or sets if the code version should be output to the console
        /// each time there is a fetch.
        /// </summary>
        public static bool DebugCodeVersion
        {
            get
            {
                return GetConfigElement("DebugCodeVersion", false);
            }
        }
        public static string InputFileCodePage
        {
            get
            {
                return GetConfigElement("InputFileCodePage", string.Empty);
            }
        }
        public static string InputFileEncodingBodyName
        {
            get
            {
                return GetConfigElement("InputFileEncodingBodyName", string.Empty);
            }
        }
        public static bool ForceFBAOnLineSequential
        {
            get
            {
                return GetConfigElement("ForceFBAOnLineSequential", false);
            }
        }
        public static string DataConnectionString
        {
            get
            {
                return ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString"); 
            }
        }
        public static string DataProviderName
        {
            get
            {
                return ConfigSettings.GetConnectionStrings("DataConnectionString", "providerName");
            }
        }
        public static string SecurityConnectionString
        {
            get
            {
                return ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");
            }
        }

        public static string SecurityProviderName
        {
            get
            {
                return ConfigSettings.GetConnectionStrings("SecurityConnectionString", "providerName");
            }
        }
        public static string BLNamespace
        {
            get
            {
                return GetConfigNamespacesElement("BLNamespace", "");
            }
        }
        public static string CommonAssemblyName
        {
            get
            {
                return GetConfigNamespacesElement("CommonAssemblyName", string.Empty);
            }
        }
        public static string CommonNamespace
        {
            get
            {
                return GetConfigNamespacesElement("CommonNamespace", string.Empty);
            }
        }
        public static string OnlineAssemblyName
        {
            get
            {
                return GetConfigNamespacesElement("OnlineAssemblyName", string.Empty);
            }
        }
        public static string OnlineNamespace
        {
            get
            {
                return GetConfigNamespacesElement("OnlineNamespace", string.Empty);
            }
        }
        public static string BLAssemblyName
        {
            get
            {
                return GetConfigNamespacesElement("BLAssemblyName", "");
            }
        }
        public static string ExternalAssemblyName
        {
            get
            {
                return GetConfigNamespacesElement("ExternalAssemblyName", "");
            }
        }
        public static string ExternalNamespace
        {
            get
            {
                return GetConfigNamespacesElement("ExternalNamespace", "");
            }
        }
        public static string BLDirectory
        {
            get
            {
                return GetConfigElement("BLDirectory", "."); ;
            }
        }
        public static string DefaultDataPath
        {
            get
            {
                return GetConfigElement("DefaultDataPath", "."); ;
            }
        }
        public static string DefaultDataCatalog
        {
            get
            {
                return GetConfigElement("DefaultDataCatalog", "."); ;
            }
        }
        public static string LoadLibPath
        {
            get
            {
                return GetConfigElement("LoadLibPath", ".");
            }
        }
        public static string DBIDPrepend
        {
            get
            {
                return GetConfigElement("DBIDPrepend", ";");
            }
        }
        public static string BatchServerName
        {
            get
            {
                return GetConfigElement("BatchServerName", "localhost");
            }
        }
        public static string DefaultInputDelimiterCharacter
        {
            get
            {
                return GetConfigElement("DefaultInputDelimiterCharacter", ";"); ;
            }
        }
        public static string AterasTextAreaStatusDockStyle
        {
            get
            {
                return GetConfigElement("AterasTextAreaStatusDockStyle", "bottom");
            }
        }
        public static int DatabaseCacheSize
        {
            get
            {
                return GetConfigElement("DatabaseCacheSize", 20);
            }
        }
        public static int SortFileCacheSize
        {
            get
            {
                return GetConfigElement("SortFileCacheSize", 1000);
            }
        }
        public static string ProcessSortPath
        {
            get
            {
                return GetConfigElement("ProcessSortPath", "C:/Program Files (x86)/Alchemy Solutions/NeoSort/Nsort.exe");
            }
        }
        public static string PLNamespace
        {
            get
            {
                return GetConfigNamespacesElement("PLNamespace", "ClientName.PL");
            }
        }
        public static string PLAssemblyName
        {
            get
            {
                return GetConfigNamespacesElement("PLAssemblyName", "ClientName.PL");
            }
        }
        public static int CacheSizeItem
        {
            get
            {
                return GetConfigElement("CacheSizeItem", 10);
            }
        }

        public static bool UseViewNames
        {
            get
            {
                return GetConfigElement("UseViewNames", true);
            }
        }
        public static string RJEFolderPath
        {
            get
            {
                string rjeFoldePath = GetConfigElement("RJEFolderPath", "C:/temp/");

                if (!rjeFoldePath.TrimEnd().EndsWith("/"))
                {
                    rjeFoldePath = rjeFoldePath.TrimEnd() + '/';
                }

                return rjeFoldePath;
            }
        }
        public static string SecurityLDAPServer
        {
            get
            {
                return GetConfigElement("SecurityLDAPServer", "DALDC");
            }
        }
        public static string SecurityLDAPPath
        {
            get
            {
                return GetConfigElement("SecurityLDAPPath", "LDAP://OU=dotNET,OU=WSUSWS,DC=SOPH,DC=COM");
            }
        }
        public static string SecurityLDAPMainframeOPR
        {
            get
            {
                return GetConfigElement("SecurityLDAPMainframeOPR", "mainframeuserid");
            }
        }
        public static string SecurityDirectories
        {
            get
            {
                return GetConfigElement("SecurityDirectories", "CN=TFS Project Creators,CN=SBSTERMINAL");
            }
        }
        public static string StoreProcedurePrefixName
        {
            get
            {
                return GetConfigElement("StoreProcedurePrefixName", "usp_"); ;
            }
        }
        public static string DefaultSchemaName
        {
            get
            {
                return GetConfigElement("DefaultSchemaName", "dbo"); ;
            }
        }
        public static string DeploymentEnvironment
        {
            get
            {
                return GetConfigElement("DeploymentEnvironment", string.Empty);
            }
        }
        public static bool FontBold
        {
            get
            {
                return GetConfigElement("FontBold", false);
            }
        }
        /// <summary>
        /// Gets or sets the number of blank lines to print
        /// at the top of page when asked to print the current
        /// view
        /// </summary>
        public static int TopMarginPrintLine
        {
            get
            {
                return GetConfigElement("TopMarginPrintLine", 1);
            }
        }
        public static Font FontPrinter
        {
            get
            {
                FontStyle fs = new FontStyle();
                if (FontBold)
                {
                    fs = FontStyle.Bold;
                }
                return new Font(GetConfigElement("FontFamily", "Courier New"), GetConfigElement("PrintFontSize", 12.0f), fs);
            }
            set
            {
                if (value.Name == "Courier New")
                {
                    if (Font.Size < value.Size)
                    {
                        switch ((int)value.Size)
                        {
                            case 8:
                                value = new Font(value.Name, 9);
                                break;
                            case 18:
                            case 19:
                                value = new Font(value.Name, 20);
                                break;
                            case 22:
                                value = new Font(value.Name, 23);
                                break;
                            case 24:
                            case 25:
                                value = new Font(value.Name, 26);
                                break;
                            default:
                                /// do nothing we are fine
                                break;
                        }

                        /// the maximum value
                        if (value.Size >= 28)
                        {
                            value = new Font(value.Name, 27);
                        }
                    }
                    else
                    {
                        switch ((int)value.Size)
                        {
                            case 8:
                                value = new Font(value.Name, 7);
                                break;
                            case 18:
                            case 19:
                                value = new Font(value.Name, 17);
                                break;
                            case 22:
                                value = new Font(value.Name, 21);
                                break;
                            case 24:
                            case 25:
                                value = new Font(value.Name, 23);
                                break;
                            case 28:
                            case 29:
                            case 30:
                                value = new Font(value.Name, 27);
                                break;
                            default:
                                /// do nothing we are fine
                                break;
                        }

                        /// do not allow to go smaller than 1
                        if (value.Size < 1)
                        {
                            value = new Font(value.Name, 1);
                        }
                    }
                }
            }
        }
        public static Font Font
        {
            get
            {
                FontStyle fs = FontBold ? FontStyle.Bold : new FontStyle();
                return new Font(GetConfigElement("FontFamily", "Courier New"), GetConfigElement("FontSize", 12.0f), fs);
            }
            set
            {
                if (value.Name == "Courier New")
                {
                    if (Font.Size < value.Size)
                    {
                        switch ((int)value.Size)
                        {
                            case 8:
                                value = new Font(value.Name, 9);
                                break;
                            case 18:
                            case 19:
                                value = new Font(value.Name, 20);
                                break;
                            case 22:
                                value = new Font(value.Name, 23);
                                break;
                            case 24:
                            case 25:
                                value = new Font(value.Name, 26);
                                break;
                            default:
                                /// do nothing we are fine
                                break;
                        }

                        /// the maximum value
                        if (value.Size >= 28)
                        {
                            value = new Font(value.Name, 27);
                        }
                    }
                    else
                    {
                        switch ((int)value.Size)
                        {
                            case 8:
                                value = new Font(value.Name, 7);
                                break;
                            case 18:
                            case 19:
                                value = new Font(value.Name, 17);
                                break;
                            case 22:
                                value = new Font(value.Name, 21);
                                break;
                            case 24:
                            case 25:
                                value = new Font(value.Name, 23);
                                break;
                            case 28:
                            case 29:
                            case 30:
                                value = new Font(value.Name, 27);
                                break;
                            default:
                                /// do nothing we are fine
                                break;
                        }

                        /// do not allow to go smaller than 1
                        if (value.Size < 1)
                        {
                            value = new Font(value.Name, 1);
                        }
                    }
                }
            }
        }
        public static int AterasTextAreaToolTotalStripHeight
        {
            get
            {
                int height = AterasTextAreaToolStripHeight;

                switch (AterasTextAreaStatusDockStyle.ToLower())
                {
                    case "top":
                        height += height;
                        break;
                    default:
                        break;
                }

                return height;
            }
        }
        /// <summary>
        /// *APPLIC-ID(A8) - This system variable contains the ID of the library to which the user is currently logged on.
        /// </summary>
        public static string APPLIC_ID
        {
            get
            {
                return GetConfigElement("ApplicationId", string.Empty);
            }
        }
        public static string DBID
        {
            get
            {
                return GetConfigElement("DBID", "0");
            }
        }
        public static string APPLIC_NAME
        {
            get
            {
                return GetConfigElement("ApplicationName", string.Empty);
            }
        }
        public static string TPSYS
        {
            get
            {
                return GetConfigElement("TPSystem", string.Empty);
            }
        }
        public static string MACHINE_CLASS
        {
            get
            {
                return GetConfigElement("MachineClass", string.Empty);
            }
        }
        public static string HOSTNAME
        {
            get
            {
                return GetConfigElement("HostName", string.Empty);
            }
        }
        public static string PARM_USER
        {
            get
            {
                return GetConfigElement("ParmUser", "PROD");
            }
        }
        public static string HARDWARE
        {
            get
            {
                return GetConfigElement("Hardware", string.Empty);
            }
        }
        public static string OS
        {
            get
            {
                return GetConfigElement("OperatingSystem", string.Empty);
            }
        }
        public static string OSVERS
        {
            get
            {
                return GetConfigElement("OperatingSystemVersion", string.Empty);
            }
        }
        public static string OPSYS
        {
            get
            {
                return GetConfigElement("OperatingSystem", string.Empty);
            }
        }
        public static string INIT_PROGRAM
        {
            get
            {
                return GetConfigElement("Init_Program", string.Empty);
            }
        }
        public static string INIT_USER
        {
            get
            {
                return GetConfigElement("InitUser", string.Empty);
            }
        }
        public static string LoginIDType
        {
            get
            {
                return GetConfigElement("LoginIDType", "login");
            }
        }
        public static string Default_Identity
        {
            get
            {
                return GetConfigElement("Default_Identity", string.Empty);
            }
        }
        public static string STEPLIB
        {
            get
            {
                return GetConfigElement("Steplib", string.Empty);
            }
        }
        public static string ApplicationCode
        {
            get
            {
                return GetConfigElement("ApplicationCode", string.Empty);
            }
        }
        public static string NaturalMessageFilePath
        {
            get
            {
                return GetConfigElement("NaturalMessageFilePath", "");
            }
        }
        /// <summary>
        /// Database Names Type: Either 'ATP' for Short names or 'CONV' or for Long
        /// </summary>
        public static string DatabaseNamesType
        {
            get
            {
                return GetConfigElement("DatabaseNamesType", string.Empty);
            }
        }
        public static char DF
        {
            get
            {
                return GetConfigElement("DF", "S")[0];
            }
        }

        public static bool UnderscoreLine
        {
            get
            {
                return GetConfigElement("UnderscoreLine", false);
            }
        }
        /// <summary>
        /// Database Names Type: Either 'ATP' for Short names or 'CONV' or for Long
        /// </summary>
        public static string DefaultDateMask
        {
            get
            {
                string tmp = "";

                switch (DTFORM)
                {
                    case 'E':
                        switch (DF)
                        {
                            case 'S':
                                tmp = "DD/MM/YY";
                                break;
                            case 'I':
                                tmp = "DDMMYYYY";
                                break;
                            case 'L':
                                tmp = "DD/MM/YYYY";
                                break;
                            default:
                                throw new Exception("DF parameter has invalid value '" + DF + "'");
                        }
                        break;
                    case 'G':
                        switch (DF)
                        {
                            case 'S':
                                tmp = "DD.MM.YY";
                                break;
                            case 'I':
                                tmp = "DDMMYYYY";
                                break;
                            case 'L':
                                tmp = "DD.MM.YYYY";
                                break;
                            default:
                                throw new Exception("DF parameter has invalid value '" + DF + "'");
                        }
                        break;
                    case 'I':
                        switch (DF)
                        {
                            case 'S':
                                tmp = "YY-MM-DD";
                                break;
                            case 'I':
                                tmp = "YYYYMMDD";
                                break;
                            case 'L':
                                tmp = "YYYY-MM-DD";
                                break;
                            default:
                                throw new Exception("DF parameter has invalid value '" + DF + "'");
                        }
                        break;
                    case 'U':
                        switch (DF)
                        {
                            case 'S':
                                tmp = "MM/DD/YY";
                                break;
                            case 'I':
                                tmp = "MMDDYYYY";
                                break;
                            case 'L':
                                tmp = "MM/DD/YYYY";
                                break;
                            default:
                                throw new Exception("DF parameter has invalid value '" + DF + "'");
                        }
                        break;
                    default:
                        throw new Exception("DTFORM parameter has invalid value '" + DTFORM + "'");

                }

                return tmp;
            }
        }
        /// <summary>
        /// Database Names Type: Either 'ATP' for Short names or 'CONV' or for Long (C# version)
        /// </summary>
        public static string DefaultDateMaskCs
        {
            get
            {
                string tmp = "";

                switch (DTFORM)
                {
                    case 'E':
                        switch (DF)
                        {
                            case 'S':
                                tmp = "dd/MM/yy";
                                break;
                            case 'I':
                                tmp = "ddMMyyyy";
                                break;
                            case 'L':
                                tmp = "dd/MM/yyyy";
                                break;
                            default:
                                throw new Exception("DF parameter has invalid value '" + DF + "'");
                        }
                        break;
                    case 'G':
                        switch (DF)
                        {
                            case 'S':
                                tmp = "dd.MM.yy";
                                break;
                            case 'I':
                                tmp = "ddMMyyyy";
                                break;
                            case 'L':
                                tmp = "dd.MM.yyyy";
                                break;
                            default:
                                throw new Exception("DF parameter has invalid value '" + DF + "'");
                        }
                        break;
                    case 'I':
                        switch (DF)
                        {
                            case 'S':
                                tmp = "yy-MM-dd";
                                break;
                            case 'I':
                                tmp = "yyyyMMdd";
                                break;
                            case 'L':
                                tmp = "yyyy-MM-dd";
                                break;
                            default:
                                throw new Exception("DF parameter has invalid value '" + DF + "'");
                        }
                        break;
                    case 'U':
                        switch (DF)
                        {
                            case 'S':
                                tmp = "MM/dd/yy";
                                break;
                            case 'I':
                                tmp = "MMddyyyy";
                                break;
                            case 'L':
                                tmp = "MM/dd/yyyy";
                                break;
                            default:
                                throw new Exception("DF parameter has invalid value '" + DF + "'");
                        }
                        break;
                    default:
                        throw new Exception("DTFORM parameter has an valid value '" + DTFORM + "'");

                }

                return tmp;
            }
        }
        /// <summary>
        /// Database Names Type: Either 'ATP' for Short names or 'CONV' or for Long
        /// </summary>
        public static string DefaultTimeMask
        {
            get
            {
                return GetConfigElement("DefaultTimeMask", "HH:II:SS");
            }
        }
        /// <summary>
        /// Database Names Type: Either 'ATP' for Short names or 'CONV' or for Long (C# version)
        /// </summary>
        public static string DefaultTimeMaskCs
        {
            get
            {
                return DefaultTimeMask.Replace("II", "mm").Replace("SS", "ss");
            }
        }
        /// <summary>
        /// Message Position setting
        /// </summary>
        public static string MessagePosition
        {
            get
            {
                if (_messagePosition == null)
                    _messagePosition = GetConfigElement("MessagePosition", "Bottom");
                return _messagePosition;
            }
            set
            {
                _messagePosition = value;
            }
        }

        public static PFKeysDisplayFormat PFKeysDisplayFormat
        {
            get
            {
                return _PFKeysDisplayFormat;
            }
            set
            {
                _PFKeysDisplayFormat = value;
            }
        }

        public static PFKeysRangeOfDisplayedKeys PFKeysRangeOfDisplayedKeys
        {
            get
            {
                return _PFKeysRangeOfDisplayedKeys;
            }
            set
            {
                _PFKeysRangeOfDisplayedKeys = value;
            }
        }

        public static bool PFKeysBothLines
        {
            get
            {
                return _PFKeysBothLines;
            }
            set
            {
                _PFKeysBothLines = value;
            }
        }

        public static bool PFKeysAtTop
        {
            get
            {
                return _PFKeysAtTop;
            }
            set
            {
                _PFKeysAtTop = value;
            }
        }

        public static int PFKeysLineNumber
        {
            get
            {
                return _PFKeysLineNumber;
            }
            set
            {
                _PFKeysLineNumber = value;
            }
        }

        public static string SERVER_TYPE
        {
            get
            {
                return GetConfigElement("ServerType", ""); ;
            }
        }

        public static bool? LowerCase
        {
            get
            {
                return _lowerCase == null
                    ? GetConfigElement("LowerCase", true)
                    : (bool)_lowerCase;
            }
            set
            {
                _lowerCase = value;
            }
        }

        public static int YSLW
        {
            get
            {
                return GetConfigElement("YSLW", 0);
            }
        }

        public static string NATVERS
        {
            get { return "6.1.1"; }
        }

        public static bool UseStoredProcs
        {
            get
            {
                return GetConfigElement("UseStoredProcs", true);
            }
        }
    #endregion

    #region private methods
    private static Color GetConfigElement(string name, Color defaultValue)
        {
            return Color.FromArgb(GetConfigElement(name, defaultValue.ToArgb()));
        }

        private static bool GetConfigElement(string name, bool defaultValue)
        {
            return Convert.ToBoolean(GetConfigElement(name, defaultValue.ToString()));
        }

        private static float GetConfigElement(string name, float defaultValue)
        {
            return Convert.ToSingle(GetConfigElement(name, defaultValue.ToString()));
        }

        private static char GetConfigElement(string name, char defaultValue)
        {
            return Convert.ToChar(GetConfigElement(name, defaultValue.ToString()));
        }

        private static int GetConfigElement(string name, int defaultValue)
        {
            return Convert.ToInt32(GetConfigElement(name, defaultValue.ToString()));
        }

        private static string GetConfigNamespacesElement(string name, string defaultValue)
        {
            string returnValue = defaultValue;

            if (!(String.IsNullOrEmpty(ConfigSettings.GetAppSettingsStringFromSection("Namespaces", name))))
                returnValue = ConfigSettings.GetAppSettingsStringFromSection("Namespaces", name);

            return returnValue;
        }

        private static string GetConfigElement(string name, string defaultValue)
        {
            string returnValue = defaultValue;

            if (!(String.IsNullOrEmpty(ConfigSettings.GetAppSettingsString(name))))
                returnValue = ConfigSettings.GetAppSettingsString(name);

            return returnValue;
        }
        #endregion

        #region public static  methods
        #region clean up
        /// <summary>
        /// WARNING: Do not use this if you don't know what you are doing. 
        /// Cleans up all the thread static fields on this class.
        /// </summary>
        public static void ThreadStaticCleanUp()
        {
            _configurationEntryInitLock = Guid.NewGuid().ToString();
        }

        #endregion
        #endregion
    }
}
