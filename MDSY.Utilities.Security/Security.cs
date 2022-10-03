using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Specialized;
using MDSY.Utilities.Security.Properties;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Collections;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Utilities.Security
{
    public class Security
    {
        #region private constants...
        private const string STR_AnrFilterUserName = "(anr= {0})";
        private const string STR_UnknownUser = "unknown";
        private const string STR_Baduser = "baduser";
        public const int UF_DISABLED = 0x0002;
        public const int UF_LOCKED = 0x0010;
        public const int UF_EXPIRED = 0x800000;

        #endregion

        #region private methods
        /// <summary>
        /// Create LDAP connection object.
        /// </summary>
        /// <param name="ldapServer">"DALDC" or "rizzo.leeds-art.ac.uk"</param> 
        /// <param name="ldapPath">"LDAP://OU=dotNET,OU=WSUSWS,DC=SOPH,DC=COM" or "LDAP://OU=staffusers,DC=leeds-art,DC=ac,DC=uk" or "LDAP://CN=Users,DC=soph,DC=com"</param>
        /// <returns></returns>
        private static DirectoryEntry createDirectoryEntry(string ldapServer, string ldapPath)
        {
            // create and return new LDAP connection with desired settings
            DirectoryEntry ldapConnection = new DirectoryEntry(ldapServer) { Path = ldapPath, AuthenticationType = AuthenticationTypes.Secure };

            return ldapConnection;
        }

        //private static void CleanUserSessions()
        //{
        //    string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

        //    if (connectionString == null || connectionString.Length == 0)
        //        return;

        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        using (SqlCommand command = new SqlCommand("delete from User_Sessions where datediff(minute, Last_Update_Date, SYSDATETIME()) > @sessionTimeout", connection))
        //        {
        //            int userSessionTimeout = 20;
        //            int.TryParse(ConfigSettings.GetAppSettingsString("serverOperationTimeout"), out userSessionTimeout);
        //            if (userSessionTimeout == 0)
        //                userSessionTimeout = 20;
        //            command.Parameters.AddWithValue("@sessionTimeout", userSessionTimeout);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        private static StringCollection GetUserGroupMembership(string ladpPath, string strUser)
        {
            StringCollection groups = new StringCollection();
            try
            {
                DirectoryEntry obEntry = new DirectoryEntry(ladpPath);
                DirectorySearcher srch = new DirectorySearcher(obEntry,
                    "(sAMAccountName=" + strUser + ")");
                SearchResult res = srch.FindOne();
                if (null != res)
                {
                    DirectoryEntry obUser = new DirectoryEntry(res.Path);
                    // Invoke Groups method.
                    object obGroups = obUser.Invoke("Groups");
                    foreach (object ob in (IEnumerable)obGroups)
                    {
                        // Create object for each group.
                        DirectoryEntry obGpEntry = new DirectoryEntry(ob);
                        groups.Add(obGpEntry.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                 
            }
            return groups;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Return the logged-in Windows user name.
        /// </summary>
        /// <param name="maxLength">For mainfame code, 8 chars max. Defaults to -1, which means no max length.</param>
        /// <returns>The user ID, stripped of domain information.</returns>
        public static string GetWindowsUser(int maxLength = -1)
        {
            string windowsUser;
            string strLogin = WindowsIdentity.GetCurrent().Name;
            if (String.IsNullOrEmpty(strLogin))
            {
                windowsUser = STR_UnknownUser;
            }
            else
            {
                int lastSlash = strLogin.LastIndexOf('\\');  // domain\\user
                int lastCharPos = strLogin.Length - 1;
                if (lastSlash >= lastCharPos) //Slash was last char? Odd, but possible I suppose.
                {
                    windowsUser = STR_Baduser;
                }
                else
                {
                    // Start copying one char after the last slash (to skip the domain), but only if that doesn't put you past the end of the string.
                    int positionToStartCopy = lastSlash + 1 < lastCharPos - 1 ?
                        lastSlash + 1 :
                        lastCharPos - 1;

                    int lengthToCopy = lastCharPos - positionToStartCopy + 1;

                    if (maxLength == -1)
                    {
                        windowsUser = strLogin.Substring(positionToStartCopy);
                    }
                    else
                    {
                        if (lastCharPos - positionToStartCopy > maxLength)
                            lengthToCopy = maxLength;
                        windowsUser = strLogin.Substring(positionToStartCopy, lengthToCopy);
                    }
                }
            }

            return windowsUser;
        }

        /// <summary>
        /// Connects to a specified Active Directory LDAP repository (server and path must be specified).
        /// Searches for an LDAP property for a specified user.
        /// Returns true or false (on errors or user or property not found).
        /// </summary>
        /// <param name="ldapServer">"DALDC"</param>
        /// <param name="ldapPath">"LDAP://OU=dotNET,OU=WSUSWS,DC=SOPH,DC=COM"</param>
        /// <param name="userName">The user's Active Directory login name</param>
        /// <param name="propertyName">The LDAP property to search for, such as "mainframeuserid"</param>
        /// <param name="returnValue">The value associated with the searched for property.
        /// If a multi-value element was requested, such as group membership, a comma-delimited string will be returned.
        /// If error occurs, this returnValue will be empty.</param>
        /// <param name="errorString">If an error occurs, the method will return false and attempt to populate this string. Otherwise empty.</param>
        /// <returns></returns>
        public static bool SearchLDAP(string ldapServer, string ldapPath, string userName,
            string propertyName, ref string returnValue, ref string errorString)
        {
            bool result = false;
            returnValue = String.Empty;
            errorString = String.Empty;

            try
            {
                DirectoryEntry myLdapConnection = createDirectoryEntry(ldapServer, ldapPath);
                // create search object which operates on LDAP connection object
                // and set search object to only find the user specified

                //search.Filter = "(cn=" + username + ")"; // cn is what was recommended, but I had to use anr.
                DirectorySearcher search = new DirectorySearcher(myLdapConnection) { Filter = String.Format(STR_AnrFilterUserName, userName) };

                // create results objects from search object
                SearchResult searchResult = search.FindOne();
                if (searchResult != null)
                {
                    // user exists, cycle through LDAP fields (cn, telephonenumber etc.)
                    ResultPropertyCollection fields = searchResult.Properties;
                    if (fields != null && fields.Contains(propertyName))
                    {
                        //Build a string that's the concatenated, comma-separated values from the collection.
                        StringBuilder builder = new StringBuilder();
                        foreach (Object valueCollection2 in fields[propertyName])
                        {
                            if (valueCollection2 != null && valueCollection2.ToString().Length > 0)
                            {
                                builder.AppendFormat("{0},", valueCollection2.ToString());
                            }
                        }

                        returnValue = builder.ToString().TrimEnd(',');
                        if (builder.Length <= 0)
                        {
                            errorString = String.Format(Resources.ErrorZeroLengthCollection, propertyName);
                        }
                        else
                        {
                            result = true;
                        }
                        // We just want the first match for this operload.
                        //Keep in mind LDAP can return mutliple values, such as a list of groups.
                        //Object valueCollection = fields[propertyName][0];                      
                        //if (valueCollection != null && valueCollection.ToString().Length > 0)
                        //{
                        //    returnValue = valueCollection.ToString();
                        //    return true;
                        //}
                        //else
                        //{
                        //    errorString = String.Format("Value collection null or zero length for property [{0}].", propertyName);
                        //}
                    }
                    else
                    {
                        errorString = String.Format(Properties.Resources.ErrorPropertyNotFound, propertyName);
                    }
                }
                else
                {
                    errorString = String.Format(Properties.Resources.ErrorUserNameNotFound, userName);
                }
            }
            catch (Exception e)
            {
                errorString = String.Format(Properties.Resources.ErrorExceptionCaught, e);
            }
            return result;
        }

        public static bool AuthenticateLDAPUser(string ldapServer, string ldapPath, string domain, string userName,
            string password, ref string returnValue)
        {
            bool flag = false;
            try
            {
                PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, ldapServer, userName, password);
                {
                    flag = principalContext.ValidateCredentials(userName, password);
                }
            }
            catch (Exception ex)
            {
                returnValue = ex.Message;
                return false;
            }

            returnValue = (flag == true) ? String.Empty : @"Username and/or Password incorrect";
            return flag;
        }

        public static bool AuthenticateLDAPUserAndGroup(string ldapServer, string ldapPath, string domain, string userName,
            string password, string GroupName, ref string returnValue)
        {
            bool flag = false;
            try
            {
                PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, ldapServer, userName, password);

                if (!principalContext.ValidateCredentials(userName, password))
                    return false;

                StringCollection sc = GetUserGroupMembership(ldapPath, userName);

                foreach (string sGroup in sc)
                {
                    if (sGroup.Substring(3) == GroupName)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                returnValue = ex.Message;
                return false;
            }

            returnValue = (flag == true) ? String.Empty : @"Username and/or Password incorrect";
            return flag;
        }

        /// <summary>
        /// Returns Mainframe ID from Security User table 
        /// </summary>
        /// <param name="networkID"></param>
        /// <returns></returns>
        public static string GetMainframeIDFromApplicationSecurity(string networkID)
        {
            string mainframeID = "";
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return mainframeID;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("select User_Mainframe_ID from Users where User_Network_ID = @User_Network_ID and User_Active = 1", connection))
                    {
                        command.Parameters.AddWithValue("@User_Network_ID", networkID);
                        object temp = command.ExecuteScalar();
                        if (temp != null)
                            mainframeID = (string)temp;
                    }
                }
            }
            catch (Exception exc)
            {
                // log error
            }

            return mainframeID;
        }

        /// <summary>
        /// Get Full User Name
        /// </summary>
        /// <param name="mainframeID"></param>
        /// <returns></returns>
        public static string GetUserName(string mainframeID)
        {
            string userName = "";
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return userName;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("Select CONCAT(RTRIM(First_Name), ' ', RTRIM(Last_Name)) AS UserName  from Users where User_Mainframe_ID = @User_Mainframe_ID and User_Active = 1", connection))
                    {
                        command.Parameters.AddWithValue("@User_Mainframe_ID", mainframeID);
                        object temp = command.ExecuteScalar();
                        if (temp != null)
                            userName = (string)temp;
                    }
                }
            }
            catch (Exception exc)
            {
                // log error
            }

            return userName;
        }

        /// <summary>
        /// Get Mainframe ID based on Network ID
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetMainframeIDFromApplicationSecurity(string userID, string password)
        {
            string mainframeID = "";
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return mainframeID;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("select User_Mainframe_ID from Users where User_Network_ID = @User_Network_ID and password = @password and User_Active = 1", connection))
                    {
                        command.Parameters.AddWithValue("@User_Network_ID", userID);
                        command.Parameters.AddWithValue("@password", password);
                        object temp = command.ExecuteScalar();
                        if (temp != null)
                            mainframeID = (string)temp;
                    }
                }
            }
            catch (Exception exc)
            {
                // log error
            }

            return mainframeID;
        }

        /// <summary>
        /// Return all Mainframe IDs based on Network ID
        /// </summary>
        /// <param name="network_ID"></param>
        /// <returns></returns>
        public static List<string> GetMainframeIDs(string network_ID)
        {
            List<string> mainframeID = new List<string>();
            try
            {
                string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

                if (String.IsNullOrEmpty(connectionString))
                    return mainframeID;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("SELECT Users.User_Mainframe_ID, Profiles.Profile_ID, Profile_Name FROM Profiles " +
                            "INNER JOIN Users ON profiles.Profile_ID = Users.Profile_ID " +
                            "WHERE Users.User_Network_ID = " + "'" + network_ID + "'" + " AND Users.User_Active = 1 ORDER BY Users.User_Mainframe_ID", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                mainframeID.Add((string)reader["User_Mainframe_ID"] + " - " + (string)reader["Profile_Name"]);
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                //TODO: log error
                throw exc;
            }

            return mainframeID;
        }

        /// <summary>
        /// Return OPID for a user based on mainframe User ID
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static string GetOPIDFromApplicationSecurity(string userID, string password = null)
        {
            string opid = "";
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return opid;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    if (password == null)
                    {
                        using (SqlCommand command = new SqlCommand("select User_OPID from Users where User_Mainframe_ID = @User_Mainframe_ID and User_Active = 1", connection))
                        {
                            command.Parameters.AddWithValue("@User_Mainframe_ID", userID);
                            object temp = command.ExecuteScalar();
                            if (temp != null)
                                opid = (string)temp;
                        }
                    }
                    else
                    {
                        using (SqlCommand command = new SqlCommand("select User_OPID from Users where User_Mainframe_ID = @User_Mainframe_ID and User_Active = 1 and password = @password", connection))
                        {
                            command.Parameters.AddWithValue("@User_Mainframe_ID", userID);
                            command.Parameters.AddWithValue("@password", password);
                            object temp = command.ExecuteScalar();
                            if (temp != null)
                                opid = (string)temp;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                // log error
            }

            return opid;
        }

        /// <summary>
        /// Returns TermID for a User from Security User table 
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static string GetTERMIDFromApplicationSecurity(string userID)
        {
            string termID = "";
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return termID;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("select Forced_Term_ID from Users where User_Mainframe_ID = @User_Mainframe_ID and User_Active = 1", connection))
                    {
                        command.Parameters.AddWithValue("@User_Mainframe_ID", userID);
                        object temp = command.ExecuteScalar();
                        if (temp != null)
                            termID = (string)temp;
                    }
                }
            }
            catch (Exception exc)
            {
                // log error
            }

            return termID;
        }

        /// <summary>
        ///  Returns Web Session ID for a User from Security User table 
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static string GetUserSessionID(string userID)
        {
            string sessionID = "";
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return sessionID;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("select User_Web_Session_ID from Users where User_Mainframe_ID = @User_Mainframe_ID and User_Active = 1", connection))
                    {
                        command.Parameters.AddWithValue("@User_Mainframe_ID", userID);
                        object temp = command.ExecuteScalar();
                        if (temp != null)
                            sessionID = (string)temp;
                    }
                }
            }
            catch (Exception exc)
            {
                // log error
            }

            return sessionID;
        }

        /// <summary>
        /// Updates SessionID for User in Application Security DB
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public static bool SetUserSessionID(string userID, string sessionID)
        {
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return false;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("update Users set User_Web_Session_ID = @CurrentSessionID, Last_Update_Date = SYSDATETIME() where User_Mainframe_ID = @User_Mainframe_ID ", connection))
                    {
                        command.Parameters.AddWithValue("@User_Mainframe_ID", userID);
                        if (string.IsNullOrEmpty(sessionID))
                            command.Parameters.AddWithValue("@CurrentSessionID", DBNull.Value);
                        else
                            command.Parameters.AddWithValue("@CurrentSessionID", sessionID);
                        int rowcnt = command.ExecuteNonQuery();
                        if (rowcnt == 0)
                            return false;
                    }
                }
            }
            catch (Exception exc)
            {
                return false;
            }

            return true;
        }

        public static int GetUserIndentity(string user)
        {
            int userID = 0;
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("select User_ID from Users where User_Mainframe_ID = @User_Mainframe_ID and User_Active = 1", connection))
                    {
                        command.Parameters.AddWithValue("@User_Mainframe_ID", user);
                        object temp = command.ExecuteScalar();
                        if (temp != null)
                            userID = (int)temp;
                    }
                }
            }
            catch (Exception exc)
            {
                // log error
            }

            return userID;
        }

        /// <summary>
        /// Add User Session when allowing Multiple Sessions
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        //public static bool AddUserSession(string user, string sessionID)
        //{
        //    CleanUserSessions();

        //    int userID = GetUserIndentity(user);

        //    string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

        //    if (String.IsNullOrEmpty(connectionString))
        //        return false;

        //    try
        //    {
        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();
        //            using (SqlCommand command = new SqlCommand("Insert into User_Sessions (User_ID, Web_Session_ID, Service_Session_ID, Last_Update_Date) values (@User_ID, @CurrentSessionID, null, SYSDATETIME() ) ", connection))
        //            {
        //                command.Parameters.AddWithValue("@User_ID", userID);
        //                if (string.IsNullOrEmpty(sessionID))
        //                    command.Parameters.AddWithValue("@CurrentSessionID", DBNull.Value);
        //                else
        //                    command.Parameters.AddWithValue("@CurrentSessionID", sessionID);
        //                int rowcnt = command.ExecuteNonQuery();
        //                if (rowcnt == 0)
        //                    return false;
        //            }
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        return false;
        //    }

        //    return true;
        //}
        /// <summary>
        /// Add USer Session Row with websession ID and WCF thread
        /// </summary>
        /// <param name="user"></param>
        /// <param name="webSessionID"></param>
        /// <param name="WCFSThreadID"></param>
        /// <returns></returns>
        //public static bool AddUserSessions(string user, string webSessionID, string WCFThreadID)
        //{
        //    CleanUserSessions();

        //    int userID = GetUserIndentity(user);

        //    string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

        //    if (String.IsNullOrEmpty(connectionString))
        //        return false;

        //    try
        //    {
        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();
        //            using (SqlCommand command = new SqlCommand("Insert into User_Sessions (User_ID, Web_Session_ID, Service_Session_ID, Last_Update_Date) values (@User_ID, @CurrentSessionID, @WCFThreadID, SYSDATETIME() ) ", connection))
        //            {
        //                command.Parameters.AddWithValue("@User_ID", userID);
        //                if (string.IsNullOrEmpty(webSessionID))
        //                    command.Parameters.AddWithValue("@CurrentSessionID", DBNull.Value);
        //                else
        //                    command.Parameters.AddWithValue("@CurrentSessionID", webSessionID);
        //                if (string.IsNullOrEmpty(WCFThreadID))
        //                    command.Parameters.AddWithValue("@WCFThreadID", DBNull.Value);
        //                else
        //                    command.Parameters.AddWithValue("@WCFThreadID", WCFThreadID);
        //                int rowcnt = command.ExecuteNonQuery();
        //                if (rowcnt == 0)
        //                    return false;
        //            }
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        //public static bool UpdateUserSession(string user, string sessionID)
        //{
        //    CleanUserSessions();

        //    int userID = GetUserIndentity(user);

        //    string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

        //    if (String.IsNullOrEmpty(connectionString))
        //        return false;

        //    try
        //    {
        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();
        //            using (SqlCommand command = new SqlCommand("update User_Sessions set Last_Update_Date = SYSDATETIME() where User_ID=@User_ID and Web_Session_ID=@CurrentSessionID", connection))
        //            {
        //                command.Parameters.AddWithValue("@User_ID", userID);
        //                if (string.IsNullOrEmpty(sessionID))
        //                    command.Parameters.AddWithValue("@CurrentSessionID", DBNull.Value);
        //                else
        //                    command.Parameters.AddWithValue("@CurrentSessionID", sessionID);
        //                int rowcnt = command.ExecuteNonQuery();
        //                if (rowcnt == 0)
        //                    return false;
        //            }
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        /// <summary>
        /// Delete User Session
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        //public static bool DeleteUserSession(string user, string sessionID)
        //{
        //    int userID = GetUserIndentity(user);

        //    string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

        //    if (String.IsNullOrEmpty(connectionString))
        //        return false;

        //    try
        //    {
        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();
        //            using (SqlCommand command = new SqlCommand("Delete USer_Sessions where Web_Session_ID = @CurrentSessionID and User_ID = @User_ID ", connection))
        //            {
        //                command.Parameters.AddWithValue("@User_ID", userID);
        //                if (string.IsNullOrEmpty(sessionID))
        //                    command.Parameters.AddWithValue("@CurrentSessionID", DBNull.Value);
        //                else
        //                    command.Parameters.AddWithValue("@CurrentSessionID", sessionID);
        //                int rowcnt = command.ExecuteNonQuery();
        //                if (rowcnt == 0)
        //                    return false;
        //            }
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        /// <summary>
        ///  Returns the thread count for a User from Security User table 
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static int GetUserThreadCount(string userID)
        {
            int threadCount = 0;
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return threadCount;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("select User_Background_Thread_Count from Users where User_Mainframe_ID = @User_Mainframe_ID and User_Active = 1", connection))
                    {
                        command.Parameters.AddWithValue("@User_Mainframe_ID", userID);
                        object temp = command.ExecuteScalar();
                        if (temp != null)
                            threadCount = (int)temp;
                    }
                }
            }
            catch (Exception exc)
            {
                // log error
            }

            return threadCount;
        }

        /// <summary>
        /// Updates background thread count for User in Application Security DB
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="threadCount"></param>
        /// <returns></returns>
        public static bool SetUserThreadCount(string userID, int threadCount)
        {
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return false;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("update Users set User_Background_Thread_Count = @User_Background_Thread_Count, Last_Update_Date = SYSDATETIME() where User_Mainframe_ID = @User_Mainframe_ID ", connection))
                    {
                        command.Parameters.AddWithValue("@User_Mainframe_ID", userID);
                        if (threadCount == 0)
                            command.Parameters.AddWithValue("@User_Background_Thread_Count", DBNull.Value);
                        else
                            command.Parameters.AddWithValue("@User_Background_Thread_Count", threadCount);
                        int rowcnt = command.ExecuteNonQuery();
                        if (rowcnt == 0)
                            return false;
                    }
                }
            }
            catch (Exception exc)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks for Valid User from Users Table
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static bool IsValidUser(string userID)
        {
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return false;
            bool isValidUser = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("Select User_ID from  Users where User_Mainframe_ID = @User_Mainframe_ID and User_Active = 1", connection))
                    {
                        command.Parameters.AddWithValue("@User_Mainframe_ID", userID);
                        object qobj = command.ExecuteScalar();
                        if (qobj != null)
                            isValidUser = true;
                    }
                }
            }
            catch (Exception exc)
            {
                return isValidUser;
            }

            return isValidUser;
        }

        /// <summary>
        /// Check for System avaliability
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns></returns>
        public static bool IsSystemAvaliable(string systemName)
        {
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return true;
            bool isSystemUp = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("Select System_Is_Operational from Systems where System_Name = @SystemName ", connection))
                    {
                        command.Parameters.AddWithValue("@SystemName", systemName);
                        object qobj = command.ExecuteScalar();
                        if (qobj != null)
                            isSystemUp = (bool)qobj;
                    }
                }
            }
            catch (Exception exc)
            {
                return isSystemUp;
            }

            return isSystemUp;
        }

        public static DataSet LoadApplications(string searchKey = null, string sorting = "Asc", string sortExpression = "Application_Name")
        {
            string query = string.Empty;
            DataSet ds = new DataSet();

            if (searchKey == string.Empty || searchKey == null)
                query = "select * from Applications";
            else
            {
                if (sorting == "Asc")
                    query = "select * from Applications where " + sortExpression + " >= '" + searchKey + "'";
                else
                    query = "select * from Applications where " + sortExpression + " <= '" + searchKey + "'";
            }
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(query, connection))
                {
                    da.Fill(ds, "Applications");
                }
            }

            return ds;
        }

        public static void UpdateApplications(DataTable applicationsDataTable)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                using (SqlDataAdapter da = new SqlDataAdapter("select * from Applications", connection))
                {
                    using (SqlCommandBuilder cb = new SqlCommandBuilder(da))
                    {
                        da.UpdateCommand = cb.GetUpdateCommand();
                        da.Update(applicationsDataTable);
                    }
                }
            }
        }

        public static void UpdateApplications(Dictionary<string, string> values, bool insert)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(insert
                    ? "insert into applications (Application_Name, Application_Description, Application_Entry_Program, Application_Entry_Trans_Code, Application_Entry_URL, Last_Update_Date, Application_Execution_Code, Application_Parameter) values(@Application_Name, @Application_Description, @Application_Entry_Program, @Application_Entry_Trans_Code, @Application_Entry_URL, SYSDATETIME(), @Application_Execution_Code, @Application_Parameter)"
                    : "update applications set Application_Name = @Application_Name, Application_Description = @Application_Description, Application_Entry_Program = @Application_Entry_Program, Application_Entry_Trans_Code = @Application_Entry_Trans_Code, Application_Entry_URL = @Application_Entry_URL, Last_Update_Date = SYSDATETIME(), Application_Execution_Code = @Application_Execution_Code, Application_Parameter = @Application_Parameter where Application_Id = @Application_Id"
                    , connection))
                {
                    int appID = 0;
                    int.TryParse(values["Application_Id"], out appID);
                    command.Parameters.AddWithValue("@Application_Id", appID);
                    command.Parameters.AddWithValue("@Application_Name", values["Application_Name"]);
                    command.Parameters.AddWithValue("@Application_Description", values["Application_Description"]);
                    command.Parameters.AddWithValue("@Application_Entry_Program", values["Application_Entry_Program"]);
                    command.Parameters.AddWithValue("@Application_Entry_Trans_Code", values["Application_Entry_Trans_Code"]);
                    command.Parameters.AddWithValue("@Application_Entry_URL", values["Application_Entry_URL"]);
                    command.Parameters.AddWithValue("@Application_Execution_Code", values["Application_Execution_Code"]);
                    command.Parameters.AddWithValue("@Application_Parameter", values["Application_Parameter"]);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteApplication(int appID)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("delete from applications where Application_Id = @Application_Id", connection))
                {
                    command.Parameters.AddWithValue("@Application_Id", appID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static DataSet LoadGroups(string searchKey = null, string sorting = "Asc", string sortExpression = "Group_Name")
        {
            string query = string.Empty;
            DataSet ds = new DataSet();

            if (searchKey == string.Empty || searchKey == null)
                query = "select * from Groups";
            else
            {
                if (sorting == "Asc")
                    query = "select * from Groups where " + sortExpression + " >= '" + searchKey + "'";
                else
                    query = "select * from Groups where " + sortExpression + " <= '" + searchKey + "'";
            }
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(query, connection))
                {
                    da.Fill(ds, "SecurityGroups");
                }
            }

            return ds;
        }

        public static void UpdateGroups(DataTable groupsDataTable)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                using (SqlDataAdapter da = new SqlDataAdapter("select * from groups", connection))
                {
                    using (SqlCommandBuilder cb = new SqlCommandBuilder(da))
                    {
                        da.UpdateCommand = cb.GetUpdateCommand();
                        da.Update(groupsDataTable);
                    }
                }
            }
        }

        public static void UpdateGroups(Dictionary<string, string> values, bool insert)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(insert
                    ? "insert into groups (Group_ID, Group_Name) values(@Group_ID, @Group_Name)"
                    : "update groups set Group_ID = @Group_ID, Group_Name = @Group_Name where Group_ID = @Group_ID"
                    , connection))
                {
                    command.Parameters.AddWithValue("@Group_Id", values["Group_Id"]);
                    command.Parameters.AddWithValue("@Group_Name", values["Group_Name"]);
                    //command.Parameters.AddWithValue("@originalGroup_Id", values["originalGroup_Id"]);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteGroup(int groupID)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("delete from groups where Group_ID = @Group_ID", connection))
                {
                    command.Parameters.AddWithValue("@Group_ID", groupID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static DataSet LoadSystems()
        {
            DataSet ds = new DataSet();

            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                using (SqlDataAdapter da = new SqlDataAdapter("select * from systems", connection))
                {
                    da.Fill(ds, "Systems");
                }
            }

            return ds;
        }

        public static void UpdateSystems(DataTable systemsDataTable)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                using (SqlDataAdapter da = new SqlDataAdapter("select * from systems", connection))
                {
                    using (SqlCommandBuilder cb = new SqlCommandBuilder(da))
                    {
                        da.UpdateCommand = cb.GetUpdateCommand();
                        da.Update(systemsDataTable);
                    }
                }
            }
        }

        public static void UpdateSystems(Dictionary<string, string> values, bool insert)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(insert
                    ? "insert into systems (System_ID, System_Name, System_Description, System_Is_Operational, System_Date_Updated) values(@System_ID, @System_Name, @System_Description, @System_Is_Operational, SYSDATETIME())"
                    : "update systems set System_Name = @System_Name, System_Description = @System_Description, System_Is_Operational = @System_Is_Operational, System_Date_Updated = SYSDATETIME() where System_ID = @System_ID"
                    , connection))
                {
                    int systemID = 0;
                    int.TryParse(values["System_ID"], out systemID);
                    command.Parameters.AddWithValue("@System_ID", systemID);
                    command.Parameters.AddWithValue("@System_Name", values["System_Name"]);
                    command.Parameters.AddWithValue("@System_Description", values["System_Description"]);
                    command.Parameters.AddWithValue("@System_Is_Operational", bool.Parse(values["System_Is_Operational"]));

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteSystem(int systemID)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("delete from systems where System_ID = @System_ID", connection))
                {
                    command.Parameters.AddWithValue("@System_ID", systemID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static DataSet LoadUsers(int profileID = -1, string searchKey = null, string sorting = "Asc", string sortExpression = "User_Mainframe_ID")
        {
            string query = string.Empty;
            DataSet ds = new DataSet();

            if (profileID == -1)
            {
                if (searchKey == string.Empty || searchKey == null)
                    query = "select * from Users";
                else
                {
                    if (sorting == "Asc")
                        query = "select * from Users where " + sortExpression + " >= '" + searchKey + "'";
                    else
                        query = "select * from Users where " + sortExpression + " <= '" + searchKey + "'";
                }

                using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(query, connection))
                    {
                        da.Fill(ds, "Users");
                    }
                }
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
                {
                    SqlCommand command = new SqlCommand("select * from Users where Profile_ID = @Profile_ID", connection);
                    command.Parameters.AddWithValue("@Profile_ID", profileID);
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        da.Fill(ds, "Users");
                    }
                }
            }

            return ds;
        }

        public static void UpdateUsers(DataTable usersDataTable)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                using (SqlDataAdapter da = new SqlDataAdapter("select * from Users", connection))
                {
                    using (SqlCommandBuilder cb = new SqlCommandBuilder(da))
                    {
                        da.UpdateCommand = cb.GetUpdateCommand();
                        da.Update(usersDataTable);
                    }
                }
            }
        }

        public static void UpdateUsers(Dictionary<string, string> values, bool insert)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(insert
                    ? "insert into users (User_Network_ID, User_Mainframe_ID, User_OPID, Last_Name, First_Name, User_Active, Forced_Term_ID, Profile_ID, Alternate_Buyer_Code, Last_Update_Date) values(@User_Network_ID, @User_Mainframe_ID, @User_OPID, @Last_Name, @First_Name, @User_Active, @Forced_Term_ID, @Profile_ID, @Alternate_Buyer_Code, SYSDATETIME())"
                    : "update users set User_Network_ID = @User_Network_ID, User_Mainframe_ID = @User_Mainframe_ID, User_OPID = @User_OPID, Last_Name = @Last_Name, First_Name = @First_Name, User_Active = @User_Active, Forced_Term_ID = @Forced_Term_ID, Profile_ID = @Profile_ID, Alternate_Buyer_Code = @Alternate_Buyer_Code, Last_Update_Date = SYSDATETIME() where User_Id = @User_Id"
                    , connection))
                {
                    int userID = 0;
                    int.TryParse(values["User_ID"], out userID);
                    command.Parameters.AddWithValue("@User_Id", userID);
                    command.Parameters.AddWithValue("@User_Network_ID", values["User_Network_ID"]);
                    command.Parameters.AddWithValue("@User_Mainframe_ID", values["User_Mainframe_ID"]);
                    command.Parameters.AddWithValue("@User_OPID", values["User_OPID"]);
                    command.Parameters.AddWithValue("@Last_Name", values["Last_Name"]);
                    command.Parameters.AddWithValue("@First_Name", values["First_Name"]);
                    command.Parameters.AddWithValue("@User_Active", bool.Parse(values["User_Active"]));
                    command.Parameters.AddWithValue("@Forced_Term_ID", values["Forced_Term_ID"]);
                    command.Parameters.AddWithValue("@Profile_ID", string.IsNullOrEmpty(values["Profile_ID"]) ? 0 : int.Parse(values["Profile_ID"]));
                    command.Parameters.AddWithValue("@Alternate_Buyer_Code", values["Alternate_Buyer_Code"]);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteUser(int userID)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("delete from users where User_Id = @User_Id", connection))
                {
                    command.Parameters.AddWithValue("@User_Id", userID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static DataSet LoadProfiles(string searchKey = null, string sorting = "Asc", string sortExpression = "Profile_Name")
        {
            string query = string.Empty;
            DataSet ds = new DataSet();

            if (searchKey == string.Empty || searchKey == null)
                query = "select * from Profiles";
            else
            {
                if (sorting == "Asc")
                    query = "select * from Profiles where " + sortExpression + " >= '" + searchKey + "'";
                else
                    query = "select * from Profiles where " + sortExpression + " <= '" + searchKey + "'";
            }


            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(query, connection))
                {
                    da.Fill(ds, "Profiles");
                }
            }

            return ds;
        }

        public static void UpdateProfileApps(DataTable usersDataTable)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                using (SqlDataAdapter da = new SqlDataAdapter("select * from Profile_Applications", connection))
                {
                    using (SqlCommandBuilder cb = new SqlCommandBuilder(da))
                    {
                        da.UpdateCommand = cb.GetUpdateCommand();
                        da.Update(usersDataTable);
                    }
                }
            }
        }


        public static void UpdateProfileApps(Dictionary<string, string> values, bool insert)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(insert
                    ? "insert into Profile_Applications (Profile_ID, Application_ID, AppProfile_ID, Application_Order) values(Profile_ID, Application_ID, AppProfile_ID, Application_Order)"
                    : "update Profile_Applications set Profile_ID = @Profile_ID, Application_ID = @Application_ID, AppProfile_ID = @AppProfile_ID, Application_Order = @Application_Order where Profile_Application_ID = @Profile_Application_ID"
                    , connection))
                {
                    command.Parameters.AddWithValue("@Profile_Application_ID", int.Parse(values["Profile_Application_ID"]));
                    command.Parameters.AddWithValue("@Profile_ID", int.Parse(values["Profile_ID"]));
                    command.Parameters.AddWithValue("@Application_ID", string.IsNullOrEmpty(values["Application_ID"]) ? 0 : int.Parse(values["Application_ID"]));
                    command.Parameters.AddWithValue("@AppProfile_ID", string.IsNullOrEmpty(values["AppProfile_ID"]) ? 0 : int.Parse(values["AppProfile_ID"]));
                    command.Parameters.AddWithValue("@Application_Order", int.Parse(values["Application_Order"]));
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteProfileApp(int profileAppID)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("delete from Profile_Applications where Profile_Application_ID = @Profile_Application_ID", connection))
                {
                    command.Parameters.AddWithValue("@Profile_Application_ID", profileAppID);
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion
    }
}
