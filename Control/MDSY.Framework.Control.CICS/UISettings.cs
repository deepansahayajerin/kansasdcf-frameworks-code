using Microsoft.Win32;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Drawing;
using System.ComponentModel;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Control.CICS
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
    #endregion

    /// <summary>
    /// This class allows you to handle specific events on the settings class:
    /// The SettingChanging event is raised before a setting's value is changed.
    /// The PropertyChanged event is raised after a setting's value is changed.
    /// The SettingsLoaded event is raised after the setting values are loaded.
    /// The SettingsSaving event is raised before the setting values are saved.
    /// </summary>
    public static class UISettings
    {
        #region private static attributes 
        #endregion

        #region private static properties
        #endregion

        #region public static properties
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
                return GetConfigElement("BackgroundARGB", Color.Black);;
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
                return GetConfigElement("DecimalSeparator", '.');
            }
        }
        public static string CompressDefaultDelimiter
        {
            get
            {
                return GetConfigElement("CompressDefaultDelimiter", ",");
            }
        }
        public static bool DisplaySetKeyDescriptor
        {
            get
            {
                return GetConfigElement("DisplaySetKeyDescriptor", true);
            }
        }
        public static int LineSize
        {
            get
            {
                return GetConfigElement("LineSize", 80);;
            }
        }
        public static int PageSize
        {
            get
            {
                return GetConfigElement("PageSize", 25);
            }
        }
        public static string DefaultDateFormat
        {
            get
            {
                return GetConfigElement("DefaultDateFormat", "U"); ;
            }
        }
        public static char ThousandSeparator
        {
            get
            {
                return GetConfigElement("ThousandSeparator", ',');
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
                return GetConfigElement("StartProgram", "CT02P000");
            }
        }
        public static string ApplicationCulture
        {
            get
            {
                return GetConfigElement("ApplicationCulture", "pt-BR");
            }
        }
        public static string DataConnectionString
        {
            get
            {
                return ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString");
            }
        }
        public static string BLNamespace
        {
            get
            {
                return GetConfigNamespacesElement("BLNamespace", "ClientName.BL");
            }
        }
        public static string DALNamespace
        {
            get
            {
                return GetConfigNamespacesElement("DALNamespace", "ClientName.DAL");
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
        public static string BLAssemblyName
        {
            get
            {
                return GetConfigNamespacesElement("BLAssemblyName", "ClientName.BL");
            }
        }
        public static string BLAssemblyPath
        {
            get
            {
                return GetConfigNamespacesElement("BLAssemblyPath", "ClientName.BL");
            }
        }
        public static string DALAssemblyName
        {
            get
            {
                return GetConfigNamespacesElement("DALAssemblyName", "ClientName.DAL");
            }
        }
        public static bool ForceFBAOnLineSequential
        {
            get
            {
                return GetConfigElement("ForceFBAOnLineSequential", false);
            }
        }
        public static bool NonMandatoryLogging
        {
            get
            {
                return GetConfigElement("NonMandatoryLogging", true);;
            }
        }
        public static string BLDirectory
        {
            get
            {
                return GetConfigElement("BLDirectory", ".");;
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
                return GetConfigElement("DefaultDataCatalog", ".");;
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
                return GetConfigElement("DefaultInputDelimiterCharacter", ";");;
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
        public static string ProcessSortPath
        {
            get
            {
                return GetConfigElement("ProcessSortPath", "C:\\Program Files (x86)\\Alchemy Solutions\\NeoSort\\Nsort.exe");
            }
        }
        public static string LogFileName
        {
            get
            {
                return GetConfigElement("LogFileName", "C:\\ATERAS.txt");
            }
        }
        public static bool LogFileEnabled
        {
            get
            {
                return GetConfigElement("LogFileEnabled", false);
            }
        }
        public static bool LogFileErrorOnly
        {
            get
            {
                return GetConfigElement("LogFileErrorOnly", false);
            }
        }
        public static string PLNamespace
        {
            get
            {
                return GetConfigNamespacesElement("BMSPLNamespace", "ClientName.PL");
            }
        }
        public static string PLAssemblyName
        {
            get
            {
                return GetConfigNamespacesElement("BMSPLAssemblyName", "ClientName.PL");
            }
        }
        public static string PLAssemblyPath
        {
            get
            {
                return GetConfigNamespacesElement("BMSPLAssemblyPath", "ClientName.PL");
            }
        }

        public static int CacheSizeItem
        {
            get
            {
                return GetConfigElement("CacheSizeItem", 10);
            }
        }
        public static string RJEFolderPath
        {
            get
            {
                return GetConfigElement("RJEFolderPath", "C:\\temp\\");
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
                return GetConfigElement("SecurityLDAPPath",  "LDAP://OU=dotNET,OU=WSUSWS,DC=SOPH,DC=COM");
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
                return GetConfigElement("StoreProcedurePrefixName", "dbs_");;
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
        public static System.Drawing.Font Font
        {
            get
            {
                FontStyle fs = new FontStyle();
                if (FontBold)
                {
                    fs = FontStyle.Bold;
                }
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
                                value = new System.Drawing.Font(value.Name, 9);
                                break;
                            case 18:
                            case 19:
                                value = new System.Drawing.Font(value.Name, 20);
                                break;
                            case 22:
                                value = new System.Drawing.Font(value.Name, 23);
                                break;
                            case 24:
                            case 25:
                                value = new System.Drawing.Font(value.Name, 26);
                                break;
                            default:
                                /// do nothing we are fine
                                break;
                        }

                        /// the maximum value
                        if (value.Size >= 28)
                        {
                            value = new System.Drawing.Font(value.Name, 27);
                        }
                    }
                    else
                    {
                        switch ((int)value.Size)
                        {
                            case 8:
                                value = new System.Drawing.Font(value.Name, 7);
                                break;
                            case 18:
                            case 19:
                                value = new System.Drawing.Font(value.Name, 17);
                                break;
                            case 22:
                                value = new System.Drawing.Font(value.Name, 21);
                                break;
                            case 24:
                            case 25:
                                value = new System.Drawing.Font(value.Name, 23);
                                break;
                            case 28:
                            case 29:
                            case 30:
                                value = new System.Drawing.Font(value.Name, 27);
                                break;
                            default:
                                /// do nothing we are fine
                                break;
                        }

                        /// do not allow to go smaller than 1
                        if (value.Size < 1)
                        {
                            value = new System.Drawing.Font(value.Name, 1);
                        }
                    }
                }
            }
        }

        public static string ApplicationCode
        {
            get
            {
                return GetConfigElement("ApplicationCode", "");
            }
        }
        /// <summary>
        /// Database Names Type: Either 'ATP' for Short names or 'CONV' or for Long
        /// </summary>
        public static string DatabaseNamesType
        {
            get
            {
                return GetConfigElement("DatabaseNamesType", "");
            }
        }
        #endregion

        #region private methods
        /// <summary>
        ///  Retrieve and remove registryFieldName value from "$LOCAL_MACHINE\Software\Ateras" and returns on the registryValue
        ///  parameter. Will return false if key entry does not exists.
        /// </summary>
        /// <remarks>The registry entry will be removed, since its only function was for values entered during installation to be tricled down
        /// to the app config file</remarks>
        //private static bool GetValueAndRemoveItFromRegistry(string registryFieldName, out string registryValue)
        //{
        //    bool valueRetrieved = false;
        //    registryValue = null;

        //    try
        //    {
        //        RegistryKey aterasKey = Registry.LocalMachine.OpenSubKey(@"Software\Ateras", true);

        //        if (aterasKey == null)
        //        {
        //            /// 64-bit OS
        //            aterasKey = Registry.LocalMachine.OpenSubKey(@"Software\Ateras\Wow6432Node", true);
        //        }

        //        if (aterasKey != null)
        //        {
        //            registryValue = (string)aterasKey.GetValue(registryFieldName);
        //            aterasKey.DeleteValue(registryFieldName);
        //            valueRetrieved = true;
        //        }
        //    }
        //    catch
        //    {
        //        /// do nothing.
        //    }

        //    return valueRetrieved;
        //}
        private static Color GetConfigElement(string name, Color defaultValue)
        {
            return Color.FromArgb(GetConfigElement(name, defaultValue.ToArgb()));
        }
        private static bool GetConfigElement(string name, bool defaultValue)
        {
            return Convert.ToBoolean(GetConfigElement(name, defaultValue.ToString()));
        }
        private static float GetConfigElement(string name,  float defaultValue)
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
            string returnValue = null;
            returnValue = ConfigSettings.GetAppSettingsStringFromSection("Namespaces", name);
            return returnValue;
        }
        private static string GetConfigElement(string name, string defaultValue)
        {
            string returnValue = null;
            returnValue = ConfigSettings.GetAppSettingsString(name);
            return returnValue;
        }

        //private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
        //    // Add code to handle the SettingChangingEvent event here.
        //}
        //private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
        //    // Add code to handle the SettingsSaving event here.
        //}
        #endregion

        #region public static  methods


        /// <summary>
        /// Applies data from the registry to app.config file.
        /// This is used by the install program. The registry is not used directly by the converted app.
        /// </summary>
        /// <param name="refreshOption">Choice of what should be attempted to update.</param>
        /// <remarks>The value from the registry is deleted after use, so future changes
        /// to the system will no be updated.</remarks>
        public static void RefreshFromRegistry(RefreshFromRegistryOption refreshOption)
        {
            //string value;
            switch (refreshOption)
            {
                case RefreshFromRegistryOption.DatabaseServerName:
                    #region DatabaseServerName
                    //if (GetValueAndRemoveItFromRegistry("DatabaseServerFromInstall", out value))
                    //{
                    //    /// save the new data set entry
                    //    // THS DataConnectionString = "Data Source=" + value + ";Initial Catalog=DSS_NewDal;Integrated Security=True;Max Pool Size=2000;";
                    //    Save();
                    //}
                    break;
                    #endregion
                case RefreshFromRegistryOption.RJEFolderPath:
                    #region RJEFolderPath
                    //if (GetValueAndRemoveItFromRegistry("RJEFOLDERPATH", out value))
                    //{
                    //    /// save the new data set entry
                    //    RJEFolderPath = value;
                    //    Save();
                    //}
                    break;
                    #endregion
            }
        }
        #endregion

    }
}
