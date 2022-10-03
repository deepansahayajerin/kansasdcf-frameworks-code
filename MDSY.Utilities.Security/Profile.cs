using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Utilities.Security
{
    public class Profile
    {
        /// <summary>
        ///  Returns Profile name for a User from Security User table 
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static string GetUserProfileName(string userID)
        {
            string profile = "";
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return profile;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "usp_GetUserProfile";
                        command.Parameters.AddWithValue("@UserID", userID);
                        object tempProfile = command.ExecuteScalar();
                        if (tempProfile != null)
                            profile = (string)tempProfile;
                    }
                }
            }
            catch (Exception exc)
            {
                profile = "*ERROR*";
            }

            return profile;
        }

        /// <summary>
        /// Returns profile description
        /// </summary>
        /// <param name="profileName"></param>
        /// <returns></returns>
        public static string GetProfileDescription(string profileName)
        {
            string profileDesc = "";
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return profileDesc;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "usp_GetProfileDescription";
                        command.Parameters.AddWithValue("@ProfileName", profileName);
                        object tempProfile = command.ExecuteScalar();
                        if (tempProfile != null && tempProfile != DBNull.Value)
                            profileDesc = (string)tempProfile;
                        else
                            profileDesc = profileName;
                    }
                }
            }
            catch (Exception exc)
            {
                profileDesc = "*ERROR*";
            }

            return profileDesc;
        }

        /// <summary>
        /// Returns DataTable with Profile Application Data
        /// </summary>
        /// <param name="profileName"></param>
        /// <returns></returns>
        public static DataTable GetProfileApplications(string profileName)
        {
            DataTable profileDataTable = new DataTable();
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
                return null;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "usp_GetProfileData";
                        command.Parameters.AddWithValue("@ProfileName", profileName);
                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                        {
                            dataAdapter.Fill(profileDataTable);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                profileDataTable = null;
            }

            return profileDataTable;
        }

        public static DataSet LoadProfileApplications(int profileID)
        {
            DataSet ds = new DataSet();

            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                SqlCommand command = new SqlCommand("select * from Profile_Applications where Profile_ID = @Profile_ID", connection);
                command.Parameters.AddWithValue("@Profile_ID", profileID);
                using (SqlDataAdapter da = new SqlDataAdapter(command))
                {
                    da.Fill(ds, "Profile_Applications");
                }
            }

            return ds;
        }

        public static DataSet LoadProfileUsers(int profileID)
        {
            DataSet ds = new DataSet();

            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                SqlCommand command = new SqlCommand("select * from Users where Profile_ID = @Profile_ID", connection);
                command.Parameters.AddWithValue("@Profile_ID", profileID);
                using (SqlDataAdapter da = new SqlDataAdapter(command))
                {
                    da.Fill(ds, "Profile_Users");
                }
            }

            return ds;
        }

        public static void UpdateProfiles(DataSet profilesDataSet)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                using (SqlDataAdapter da = new SqlDataAdapter("select * from profiles", connection))
                {
                    using (SqlCommandBuilder cb = new SqlCommandBuilder(da))
                    {
                        da.UpdateCommand = cb.GetUpdateCommand();
                        da.Update(profilesDataSet.Tables[0]);
                    }
                }
            }
        }

        public static void UpdateProfiles(Dictionary<string, string> values, bool insert)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(insert
                    ? "insert into profiles (Profle_Name, Profile_Description) values(@Profile_Name, @Profile_Description)"
                    : "update profiles set Profile_Name = @Profile_Name, Profile_Description = @Profile_Description where Profile_ID = @Profile_ID"
                    , connection))
                {
                    command.Parameters.AddWithValue("@Profile_ID", values["Profile_ID"]);
                    command.Parameters.AddWithValue("@Profile_Name", values["Profile_Name"]);
                    command.Parameters.AddWithValue("@Profile_Description", values["Profile_Description"]);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteProfile(int profileID)
        {
            using (SqlConnection connection = new SqlConnection(ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("delete from profiles where Profile_ID = @Profile_ID", connection))
                {
                    command.Parameters.AddWithValue("@Profile_ID", profileID);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
