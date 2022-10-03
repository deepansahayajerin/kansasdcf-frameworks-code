using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MDSY.Framework.Configuration.Common
{
    #region enum
    #endregion

    public class ConfigSettings
    {
        private static string GetSingleFileLocation()
        {
            using var processModule = Process.GetCurrentProcess().MainModule;
            return Path.GetDirectoryName(processModule?.FileName);
        }
        private static string GetBasePath()
        {
            string regularFileLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string publishedSingleFileLocation = GetSingleFileLocation();
            string location = regularFileLocation;

            if (!File.Exists(regularFileLocation + "\\appsettings.json"))
                location = publishedSingleFileLocation;

            return location;
        }
        private static readonly IConfigurationBuilder _jsonConfigurationBuilder = new ConfigurationBuilder().SetBasePath(GetBasePath()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        private static readonly IConfiguration _jsonConfiguration = _jsonConfigurationBuilder.Build();

        #region private static properties
        #endregion

        #region public methods

        public static string GetJSONFileLocation()
        {
            return string.Concat(GetBasePath(), @"\appsettings.json");
        }
        /// <summary>
        /// Reads app setting from configuration file. If the entry doesn't exist return string.Empty
        /// </summary>
        public static string GetAppSettingsString(string key)
        {
            string retValue = string.Empty;

            if (_jsonConfiguration.GetSection("AppSettings").GetSection(key).Exists())
                retValue = _jsonConfiguration.GetSection("AppSettings").GetSection(key).Value;

            return retValue;
        }

        /// <summary>
        /// Reads app setting from configuration file. If the entry doesn't exist return false
        /// </summary>
        public static bool GetAppSettingsBool(string key)
        {
            bool retValue = false;

            if (_jsonConfiguration.GetSection("AppSettings").GetSection(key).Exists())
                retValue = Convert.ToBoolean(_jsonConfiguration.GetSection("AppSettings").GetSection(key).Value);

            return retValue;
        }

        /// <summary>
        /// Reads setting from configuration file. If the entry doesn't exist return string.Empty
        /// </summary>
        public static string GetAppSettingsStringFromSection(string section, string key)
        {
            string retValue = string.Empty;

            if (_jsonConfiguration.GetSection(section).GetSection(key).Exists())
                retValue = _jsonConfiguration.GetSection(section).GetSection(key).Value;

            return retValue;
        }

        /// <summary>
        /// Reads setting from configuration file. If the entry doesn't exist return false
        /// </summary>
        public static bool GetAppSettingsBoolFromSection(string section, string key)
        {
            bool retValue = false;

            if (_jsonConfiguration.GetSection(section).GetSection(key).Exists())
                retValue = Convert.ToBoolean(_jsonConfiguration.GetSection(section).GetSection(key).Value);

            return retValue;
        }

        /// <summary>
        /// Reads connection setting from configuration file. If the entry doesn't exist return string.Empty
        /// </summary>
        public static string GetConnectionStrings(string section, string key)
        {
            string retValue = string.Empty;

            if (_jsonConfiguration.GetSection("ConnectionStrings").GetSection(section).GetSection(key).Exists())
                retValue = _jsonConfiguration.GetSection("ConnectionStrings").GetSection(section).GetSection(key).Value;

            return retValue;
        }
        /// <summary>
        /// Reads connection string from configuration file. If the entry doesn't exist return string.Empty
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConnectionStrings(string key)
        {
            string retValue = string.Empty;

            if (_jsonConfiguration.GetSection("ConnectionStrings").GetSection(key).Exists())
                retValue = _jsonConfiguration.GetSection("ConnectionStrings").GetSection(key).Value;

            return retValue;
        }

        /// <summary>
        /// Reads environment variables. If the entry doesn't exist an empty dictionary string,string is returned.
        /// </summary>
        public static Dictionary<string, string> GetEnvironmentVariables()
        {
            Dictionary<string, string> retValue = new Dictionary<string, string>();

            if (_jsonConfiguration.GetSection("environmentVariables").Exists())
                retValue = _jsonConfiguration.GetSection("environmentVariables").GetChildren().ToDictionary(x => x.Key, x => x.Value);

            return retValue;
        }

        /// <summary>
        /// Reads environment variables. If the entry doesn't exist an empty dictionary string,string is returned.
        /// </summary>
        public static Dictionary<string, string> GetRemoteBatchEnvironmentVariables()
        {
            Dictionary<string, string> retValue = new Dictionary<string, string>();
            string RemoteBatchEnvVarPrefix = GetAppSettingsString("RemoteBatchEnvVarPrefix");

            if (!String.IsNullOrEmpty(RemoteBatchEnvVarPrefix))
            {
                foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
                {
                    if (de.Key.ToString().StartsWith(RemoteBatchEnvVarPrefix))
                        retValue.Add(de.Key.ToString().Substring(8), de.Value.ToString());
                }
            }

            return retValue;
        }

        /// <summary>
        /// Reads key overrides from configuration file. If the entry doesn't exist an empty dictionary string,string is returned.
        /// </summary>
        public static Dictionary<string, string> GetKeyOverrides()
        {
            Dictionary<string, string> retValue = new Dictionary<string, string>();

            if (_jsonConfiguration.GetSection("keyOverrides").Exists())
                retValue = _jsonConfiguration.GetSection("keyOverrides").GetChildren().ToDictionary(x => x.Key, x => x.Value);

            return retValue;
        }
        #endregion
    }
}
