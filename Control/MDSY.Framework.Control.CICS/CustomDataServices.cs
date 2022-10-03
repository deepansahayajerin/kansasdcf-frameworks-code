using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using MDSY.Framework.Data.Vsam;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer;
using MDSY.Framework.Interfaces;
using System.Xml;
using System.Data.SqlClient;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Control.CICS
{
    public class CustomDataServices
    {
        public static string[] WS_T_CTRMID;
        public static string[] WS_T_CLASSA;
        public static string[] WS_T_CLASSB;
        public static string _t_classA;
        public static string _t_classB;
        public static string _t_cfacu;
        public static string _t_cwhse;
        public static byte[] _t_pcipl;
        public static byte[] _t_pcilp;
        public static string T_CTRMID;
        public static string T_CLASSA;
        public static string T_CLASSB;

        public static void GetTFLTerminals(string terminal)
        {
            WS_T_CTRMID = new string[15];
            WS_T_CLASSA = new string[15];
            WS_T_CLASSB = new string[15];

            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");
            if (String.IsNullOrEmpty(connectionString))
            {
                Console.Write("Security table connection string not available");
                throw new Exception("Security table connection string not available");
            }

            try
            {
                int array = 0;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("Select top (5) Forced_Term_ID, T_CLASSA, T_CLASSB, T_CFACU, T_CWHSE " +
                        "from Users where Forced_Term_ID > @Forced_Term_ID and User_Active = 1 order by Forced_Term_ID", connection))
                    {
                        command.Parameters.AddWithValue("@Forced_Term_ID", terminal);
                        try
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    WS_T_CTRMID[array] = reader["Forced_term_ID"] == System.DBNull.Value ? string.Empty : (string)reader["Forced_term_ID"];
                                    WS_T_CLASSA[array] = reader["T_CLASSA"] == System.DBNull.Value ? string.Empty : (string)reader["T_CLASSA"];
                                    WS_T_CLASSB[array] = reader["T_CLASSB"] == System.DBNull.Value ? string.Empty : (string)reader["T_CLASSB"];
                                    array++;
                                }
                            }
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }
            catch 
            {
                throw new Exception("Cannot connect or read Security table");
            }
        }

        public static  void GetTFLData(string userID)
        {
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
            {
                Console.Write("Security table connection string not available");
                throw new Exception("Security table connection string not available");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("select User_Mainframe_ID, T_CLASSA, T_CLASSB, T_CFACU, T_CWHSE, T_PCIPL, T_PCILP from Users where User_Mainframe_ID = @User_Mainframe_ID and User_Active = 1", connection))
                    {
                        command.Parameters.AddWithValue("@User_Mainframe_ID", userID);
                        try
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    _t_classA = reader["T_CLASSA"] == System.DBNull.Value ? string.Empty : (string)reader["T_CLASSA"];
                                    _t_classB = reader["T_CLASSB"] == System.DBNull.Value ? string.Empty : (string)reader["T_CLASSB"];
                                    _t_cfacu = reader["T_CFACU"] == System.DBNull.Value ? string.Empty : (string)reader["T_CFACU"];
                                    _t_cwhse = reader["T_CWHSE"] == System.DBNull.Value ? string.Empty : (string)reader["T_CWHSE"];
                                    _t_pcipl = reader["T_PCIPL"] == System.DBNull.Value ? new byte[]{ } : (byte[])reader["T_PCIPL"];
                                    _t_pcilp = reader["T_PCILP"] == System.DBNull.Value ? new byte[] { } : (byte[])reader["T_PCILP"];
                                }
                            }
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                throw new Exception("Cannot connect to or read Security table", exc);
            }
        }

        public static void GetTFLClassData(string termID)
        {
            T_CTRMID = string.Empty;
            T_CLASSA = string.Empty;
            T_CLASSB = string.Empty;

            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
            {
                Console.Write("Security table connection string not available");
                throw new Exception("Security table connection string not available");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("select Forced_Term_ID, T_CLASSA, T_CLASSB from Users where Forced_Term_ID = @Forced_Term_ID and User_Active = 1", connection))
                    {
                        command.Parameters.AddWithValue("@Forced_Term_ID",termID);
                        try
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    T_CTRMID = reader["Forced_Term_ID"] == System.DBNull.Value ? string.Empty : (string)reader["Forced_Term_ID"];
                                    T_CLASSA = reader["T_CLASSA"] == System.DBNull.Value ? string.Empty : (string)reader["T_CLASSA"];
                                    T_CLASSB = reader["T_CLASSB"] == System.DBNull.Value ? string.Empty : (string)reader["T_CLASSB"];
                                }
                            }
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                throw new Exception("Cannot connect to or read Security table", exc);
            }
        }

        public static void UpdateTFLClassData(string termID)
        {
            string connectionString = ConfigSettings.GetConnectionStrings("SecurityConnectionString", "connectionString");

            if (String.IsNullOrEmpty(connectionString))
            {
                Console.Write("Security table connection string not available");
                throw new Exception("Security table connection string not available");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("Update Users set T_CLASSA = @T_CLASSA, T_CLASSB = @T_CLASSB where Forced_Term_ID = @termID", connection))
                    {
                        command.Parameters.AddWithValue("@termID", termID);
                        command.Parameters.AddWithValue("@T_CLASSA", T_CLASSA);
                        command.Parameters.AddWithValue("@T_CLASSB", T_CLASSB);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception exc)
            {
                throw new Exception("Cannot connect or read Security table", exc);
            }
        }

    }
}
