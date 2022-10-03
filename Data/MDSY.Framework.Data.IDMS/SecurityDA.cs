#region Using Directives
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using MDSY.Framework.Configuration.Common;
#endregion
namespace MDSY.Framework.Data.IDMS
{
	/// <summary>
	/// Summary description for SecurityDA.
	/// </summary>
		
	public class SecurityDA
	{
		public SecurityDA()
		{
	
		}
		#region Private Members
		private string _connectionString = string.Empty;
		private DbConnection _connection;
        private DbProviderFactory _dbFactory;
        //		private string termID = null;
        #endregion

        #region Public Properties
        protected string ConnectionString
		{
			get
			{
				try
				{
					if (_connectionString == string.Empty)
					{
						_connectionString = ConfigSettings.GetAppSettingsString("SQLC");
					}
				}
				catch(Exception ex)
				{
					return ex.Message;
				}

				return _connectionString;
			}
		}
		protected DbConnection Connection 
		{
			get
			{
				if (_connection == null)
					{
                    _dbFactory = DbProviderFactories.GetFactory("SqlClient");
                    _connection = _dbFactory.CreateConnection();
                    _connection.ConnectionString = ConnectionString;
                }
				return _connection;
			}
		}
		#endregion

		#region public methods
		//***************
		//****************  Get termid of session ***************************************
		//***************
        //public string GetTermID(string sessionId)
        //{
        //    try
        //    {
        //        return "0001";
        //        Connection.Open();
        //        SqlCommand command = new SqlCommand("GetWebSession",Connection);
        //        command.CommandType = CommandType.StoredProcedure;
        //        command.Parameters.AddWithValue("@sessionID", sessionId);

        //        termID = (string)command.ExecuteScalar();
        //        Connection.Close();
        //    }
        //    catch
        //    {
        //        return "ERROR";
        //    }
        //    finally
        //    {
        //        Connection.Close();
        //    }

        //    if (termID != null)
        //        return termID;
        //    else
        //        return "ERROR";
           
        //}	
        ////***************
        ////****************  Update New Web Session to the Database ***************************************
        ////***************
        //public string UPDWebSession(string userID, string sessionID)
        //{
        //    try
        //    {
        //        return "0001";
        //        Connection.Open();
        //        SqlCommand command = new SqlCommand("StartWebSession", Connection);
        //        command.CommandType = CommandType.StoredProcedure;
        //        command.Parameters.AddWithValue("@userID", userID);
        //        command.Parameters.AddWithValue("@sessionID", sessionID);

        //        termID = (string)command.ExecuteScalar();
        //        Connection.Close();
        //    }
        //    catch
        //    {
        //    return "ERROR";
        //    }
        //    finally
        //    {
        //        Connection.Close();
        //    }
        //    if (termID != null)
        //        return termID;
        //    else
        //        return "ERROR";
        //}
        ////***************
        ////****************  Update Web Session timeout  ***************************************
        ////***************
        //public int UPDWebSessionTimeout(string sessionID)
        //{
        //    try
        //    {
        //        return 0;
        //        Connection.Open();
        //        SqlCommand command = new SqlCommand("UpdWebSessionTimeout", Connection);
        //        command.CommandType = CommandType.StoredProcedure;
        //        command.Parameters.AddWithValue("@sessionID", sessionID);

        //        int iRet = command.ExecuteNonQuery();
        //        Connection.Close();
        //        return iRet;
        //    }
        //    catch (Exception ex)
        //    {
        //        string exmessage = ex.Message;
				
        //        return 8;
        //    }
        //    finally
        //    {
        //        Connection.Close();
        //    }
        //} 
        ////***************
        ////****************  Quit Session by updating WebSession to nulls ***************************************
        ////***************
        //public string UPDWebSessionNull(string sessionID)
        //{
        //    try
        //    {
        //        return "Success";
        //        SqlCommand command = new SqlCommand("QuitWebSession", Connection);
        //        command.CommandType = CommandType.StoredProcedure;
        //        command.Parameters.AddWithValue("@sessionID", sessionID);

        //        Connection.Open();
        //        if (command.ExecuteNonQuery() > 0)
        //        {
        //            Connection.Close();
        //            return "Success";
        //        }
        //        else
        //        {
        //            Connection.Close();
        //            return "Error";
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //    finally
        //    {
        //        Connection.Close();
        //    }

        //} 
        //public string UPDAllWebSessionNull()
        //{
        //    try
        //    {
        //        return "Success";
        //        SqlCommand command = new SqlCommand("QuitAllWebSessions", Connection);
        //        command.CommandType = CommandType.StoredProcedure;

        //        Connection.Open();
        //        if (command.ExecuteNonQuery() > 0)
        //        {
        //            Connection.Close();
        //            return "Success";
        //        }
        //        else
        //        {
        //            Connection.Close();
        //            return "Error";
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //    finally
        //    {
        //        Connection.Close();
        //    }

        //} 
     #endregion 

	}
}
