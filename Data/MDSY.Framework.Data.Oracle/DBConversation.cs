using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer;
using MDSY.Framework.Core;
using MDSY.Framework.Interfaces;
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Data.Oracle
{
    public class DBConversation
    {
        public DBConversation()
        {
            SetNewDBConnection();
        }

        #region Private variables
        private DbConnection _connection;
        private DbTransaction _transaction;
        private DbCommand _command;
        private DataTable _currentDataTable;
        private DbDataAdapter _dataAdapter;
        private DbProviderFactory _dbFactory;
        private string parmPrefix = ":";
        private Dictionary<string, ReaderQuery> _readers;
        private Dictionary<string, string> _dynamicQueries;
        private List<object> _intoVariables;
        private List<object> _extraVariables;
        #endregion

        #region Public properties
        /// <summary>
        /// Sets and returns a reference to the DbConnection object.
        /// </summary>
        public DbConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        /// <summary>
        /// Sets and returns a reference to the DbTransaction object.
        /// </summary>
        public DbTransaction Transaction
        {
            get { return _transaction; }
            set { _transaction = value; }
        }

        public DataTable CurrentDataTable { get; set; }

        public string DefaultDateFormat { get; set; }

        //public IField SqlCode { get; set; }

        public Oracle_SQLCA SQLCA { get; set; }
        #endregion

        #region Public Methods

        /// <summary>
        /// Rollback teh transaction
        /// </summary>
        public void Rollback()
        {
            try
            {

                if (Transaction != null)
                {
                    Transaction.Rollback();
                }
            }
            catch
            {
                //Write Log rollback error
            }
            finally
            {
                if (Transaction != null)
                {
                    Transaction.Dispose();
                    Transaction = null;
                }
                SetSqlCode(0);
            }

        }

        /// <summary>
        /// Commit The transaction
        /// </summary>
        public void SavePoint()
        {
            try
            {
                if (_transaction != null && _transaction.Connection != null)
                {
                    _transaction.Commit();
                    if (Connection != null && Connection.State == ConnectionState.Open)
                    {
                        _transaction = Connection.BeginTransaction();
                    }
                }
            }
            catch
            {
                //Write Log syncpoint error
                _transaction.Dispose();
                _transaction = null;
            }
            SetSqlCode(0);
        }

        /// <summary>
        /// Close file/database connect
        /// </summary>
        public void CloseConnection()
        {

            if (Transaction != null)
            {
                _transaction.Commit();
                _transaction.Rollback();
                Transaction.Dispose();
                Transaction = null;
            }
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
            SetSqlCode(0);
        }

        /// <summary>
        /// Forces transaction and connection to be closed
        /// </summary>
        public void ForceDbClose()
        {

            if (Transaction != null)
            {
                if (Transaction.Connection != null)
                {
                    Transaction.Commit();
                }
                Transaction.Dispose();
                Transaction = null;
            }

            if (Connection.State == ConnectionState.Open)
                Connection.Close();
        }

        /// <summary>
        /// execute SQl Select of SQl Fetch command
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="parms"></param>
        public void ExecuteSqlQuery(string sqlString, params Object[] parms)
        {

            OpenConnection();
            SQLCA.SQLERRD[3].SetValue(0);
            _extraVariables = new List<object>();
            _command = _connection.CreateCommand();
            if (Transaction != null)
            {
                _command.Transaction = Transaction;
            }

            CurrentDataTable = new DataTable();
            _command.CommandType = CommandType.Text;
            _command.CommandText = RemoveIntoVariables(sqlString, parms);
            _command.CommandText = CheckForTimeStampColumns(_command.CommandText);
            if (BatchControl.CurrentSchema.Trim() != string.Empty)
            {
                _command.CommandText = SetSchemaForTables(_command.CommandText);
            }

            if (_extraVariables.Count() > 0)
            {
                SetPassedParms(_extraVariables.ToArray());
            }

            if (_dataAdapter == null)
            {
                _dataAdapter = _dbFactory.CreateDataAdapter();
            }

            try
            {
                _dataAdapter.SelectCommand = _command;
                _dataAdapter.Fill(CurrentDataTable);
                SQLCA.SQLERRD[3].SetValue(CurrentDataTable.Rows.Count);
                if (_intoVariables.Count > 0 && CurrentDataTable.Rows.Count > 0)
                {
                    if (CurrentDataTable.Rows.Count == 1)
                    {
                        UpdateIntoVariables(CurrentDataTable);
                        SetSqlCode(0);
                    }
                    else
                    {
                        SetSqlCode(-811);
                    }
                }
                else
                {
                    SetSqlCode(100);
                }

                SimpleLogging.LogMessageToFile(string.Concat("Oracle SQL: ", _command.CommandText, FormatCommandParms(_command)));
            }
            catch (Exception ex)
            {
                string message = string.Concat("DB problem: ", ex.Message, " ", _command.CommandText, FormatCommandParms(_command));
                SimpleLogging.LogMandatoryMessageToFile(message);
                throw new Exception(message, ex);

            }
        }

        /// <summary>
        /// Execute non select query
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="parms"></param>
        public void ExecuteSql(string sqlString, params Object[] parms)
        {

            OpenConnection();
            SQLCA.SQLERRD[3].SetValue(0);
            _command = _connection.CreateCommand();
            if (Transaction != null)
            {
                _command.Transaction = Transaction;
            }
            bool isDropOrCreate = sqlString.Trim().ToUpper().StartsWith("DROP ") || sqlString.Trim().ToUpper().StartsWith("CREATE ");
            _command.CommandType = CommandType.Text;
            _command.CommandText = sqlString;

            if (BatchControl.CurrentSchema.Trim() != string.Empty)
            {
                _command.CommandText = SetSchemaForTables(_command.CommandText);
            }

            if (parms != null && parms.Count() > 0)
            {
                SetPassedParms(parms);
            }

            try
            {
                int returnCode = _command.ExecuteNonQuery();
                SQLCA.SQLERRD[3].SetValue(returnCode);
                if (returnCode > 0 || (isDropOrCreate))
                    SetSqlCode(0);
                else
                    SetSqlCode(100);
                SimpleLogging.LogMessageToFile(string.Concat("Oracle SQL: ", _command.CommandText, FormatCommandParms(_command)));

            }

            catch (Exception ex)
            {
                string message = string.Concat("DB problem: ", ex.Message, " ", _command.CommandText, FormatCommandParms(_command));
                SimpleLogging.LogMandatoryMessageToFile(message);
                if (ex.Message.Contains("SQLSTATE=23505"))
                {
                    SetSqlCode(-803);
                    return;
                }
                throw new Exception(message, ex);

            }
        }

        /// <summary>
        /// Execute Stored proc
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="parms"></param>
        public void ExecuteStoredProc(string sqlString, params Object[] parms)
        {

            OpenConnection();

            _command = _connection.CreateCommand();
            if (Transaction != null)
            {
                _command.Transaction = Transaction;
            }
            _command.CommandText = sqlString;

            if (parms != null && parms.Count() > 0)
            {
                SetStoredProcParms(parms);
            }
            if (sqlString.ToUpper().StartsWith("EXEC") || sqlString.ToUpper().StartsWith("CALL "))
            {
                _command.CommandType = CommandType.Text;
                if (BatchControl.StoredProcSchema.Trim() != string.Empty)
                {
                    _command.CommandText = _command.CommandText.Insert(_command.CommandText.IndexOf(' ') + 1, string.Concat(BatchControl.StoredProcSchema, "."));
                }
            }
            else
            {
                _command.CommandType = CommandType.StoredProcedure;
                if (_command.CommandText.Contains(" "))
                {
                    int spacePos = _command.CommandText.IndexOf(" ");
                    _command.CommandText = _command.CommandText.Substring(0, spacePos);
                }
                if (BatchControl.StoredProcSchema.Trim() != string.Empty)
                {
                    _command.CommandText = string.Concat(BatchControl.StoredProcSchema, ".", _command.CommandText);
                }
            }

            try
            {
                int returnCode = _command.ExecuteNonQuery();
                SetSqlCode(0);
                int parmCtr = 0;
                foreach (DbParameter dbParm in _command.Parameters)
                {
                    if (parms[parmCtr] is FieldNullIndicator)
                    {
                        parmCtr++;
                        if (parms.Length <= parmCtr + 1) break;
                    }
                    if (dbParm.Direction == ParameterDirection.Output || dbParm.Direction == ParameterDirection.InputOutput)
                    {
                        if (parms[parmCtr] is IBufferValue)
                        {
                            IBufferValue field = (IBufferValue)parms[parmCtr];
                            FieldNullIndicator nullInd = null;
                            if (parms.Length > parmCtr + 1 && parms[parmCtr + 1] is FieldNullIndicator)
                            {
                                nullInd = (FieldNullIndicator)parms[parmCtr + 1];
                            }
                            UpdateFieldFromDBParm(field, dbParm, nullInd);
                        }
                    }
                    parmCtr++;
                }
                SimpleLogging.LogMessageToFile(string.Concat("Oracle SQL: ", _command.CommandText, FormatCommandParms(_command)));

            }

            catch (Exception ex)
            {
                string message = string.Concat("DB problem: ", ex.Message, " ", _command.CommandText, FormatCommandParms(_command));
                SimpleLogging.LogMandatoryMessageToFile(message);
                if (ex.Message.Contains("SQLSTATE=23505"))
                {
                    SetSqlCode(-803);
                    return;
                }
                throw new Exception(message, ex);

            }
        }

        public void ExecuteSqlSet(string sqlString, params Object[] parms)
        {

            OpenConnection();

            _command = _connection.CreateCommand();
            if (Transaction != null)
            {
                _command.Transaction = Transaction;
            }
            _command.CommandText = sqlString;
            bool setFieldIsParm = (sqlString.Trim()[4] == '{');
            if (parms != null && parms.Count() > 0)
            {
                SetSetParms(setFieldIsParm, parms);
            }

            _command.CommandType = CommandType.Text;

            try
            {
                _command.ExecuteNonQuery();
                SetSqlCode(0);

                int parmCtr = 0;
                foreach (DbParameter dbParm in _command.Parameters)
                {

                    if (parms[parmCtr] is IBufferValue)
                    {
                        IBufferValue field = (IBufferValue)parms[parmCtr];
                        UpdateFieldFromDBParm(field, dbParm);
                    }
                    parmCtr++;
                }
                SimpleLogging.LogMessageToFile(string.Concat("Oracle SQL: ", _command.CommandText, FormatCommandParms(_command)));

            }

            catch (Exception ex)
            {
                string message = string.Concat("DB problem: ", ex.Message, " ", _command.CommandText, FormatCommandParms(_command));
                SimpleLogging.LogMandatoryMessageToFile(message);
                if (ex.Message.Contains("SQLSTATE=23505"))
                {
                    SetSqlCode(-803);
                    return;
                }
                throw new Exception(message, ex);

            }
        }
        /// <summary>
        /// Prepare dynamic sql string for execution
        /// </summary>
        /// <param name="sqlValue"></param>
        /// <returns></returns>
        public void PrepareDynamicSql(string sqlName, IBufferValue sqlObject)
        {
            string prepSql = string.Empty;
            //Check for Varchar group; replace question mark with {0}
            if (sqlObject is IGroup)
            {
                IGroup groupField = (IGroup)sqlObject;
                List<IBufferElement> fieldList = groupField.Elements.ToList();
                if (fieldList.Count() == 2 && fieldList[0] is IField)
                {
                    IField checkTypeField = (IField)fieldList[0];
                    if (checkTypeField.FieldType == FieldType.CompShort)
                    {
                        IField varCharField = (IField)fieldList[1];
                        prepSql = varCharField.AsString().Trim();
                    }
                    else
                    {
                        prepSql = groupField.AsString().Trim();
                    }
                }
            }
            else
                prepSql = sqlObject.BytesAsString;

            prepSql = ReplaceQuestionMarks(prepSql);

            if (!_dynamicQueries.ContainsKey(sqlName))
                _dynamicQueries.Add(sqlName, prepSql);
            else
                _dynamicQueries[sqlName] = prepSql;
            SetSqlCode(0);
        }
        /// <summary>
        /// Execute dynamic Sql statement from Field/Group  
        /// </summary>
        /// <param name="sqlText"></param>
        public void ExecuteDynamicSql(string dynamicSqlName, params Object[] parms)
        {
            if (_dynamicQueries.ContainsKey(dynamicSqlName))
            {
                if (_dynamicQueries[dynamicSqlName].Trim().ToUpper().StartsWith("UPDATE") || _dynamicQueries[dynamicSqlName].Trim().ToUpper().StartsWith("DELETE")
                    || _dynamicQueries[dynamicSqlName].Trim().ToUpper().StartsWith("INSERT") || _dynamicQueries[dynamicSqlName].Trim().ToUpper().StartsWith("CREATE")
                    || _dynamicQueries[dynamicSqlName].Trim().ToUpper().StartsWith("DROP"))
                {
                    ExecuteSql(_dynamicQueries[dynamicSqlName], parms);
                }
                else
                {
                    ExecuteSqlQuery(_dynamicQueries[dynamicSqlName], parms);
                }
            }
            else
            {
                SetSqlCode(-518);
            }
        }

        /// <summary>
        /// Define query text from Define Cursor syntax
        /// </summary>
        /// <param name="queryName"></param>
        /// <param name="queryText"></param>
        /// <param name="parms"></param>
        public void SetQueryText(string queryName, string queryText, params object[] parms)
        {
            if (queryText.Contains(" AT END "))
            {
                queryText = queryText.Replace(" AT END ", " END ");
            }
            if (!_readers.ContainsKey(queryName))
                _readers.Add(queryName, new ReaderQuery(queryName, queryText, parms));
            SetSqlCode(0);
        }

        /// <summary>
        /// Open the DataReader using saved query 
        /// </summary>
        /// <param name="queryName"></param>
        public void OpenReader(string queryName)
        {
            if (_readers.ContainsKey(queryName))
            {
                CreateReader(_readers[queryName]);
                SetSqlCode(0);
            }
            else
            {
                SetSqlCode(34);
            }

        }

        /// <summary>
        /// Close the Data reader based on the query name
        /// </summary>
        /// <param name="queryName"></param>
        public void CloseReader(string queryName)
        {
            if (_readers.ContainsKey(queryName))
            {
                _readers[queryName].DataReader.Close();
                _readers[queryName].DataReader.Dispose();
                _readers[queryName].DataReader = null;
                SetSqlCode(0);
            }
            else
            {
                SetSqlCode(34);
            }
        }

        /// <summary>
        /// Fetch the next row from the DataReader based on the named query
        /// </summary>
        /// <param name="queryName"></param>
        /// <param name="fields"></param>
        public void FetchReaderRow(string queryName, params object[] fields)
        {
            try
            {
                SQLCA.SQLERRD[3].SetValue(0);
                int spacePOS = queryName.IndexOf(' ');
                if (spacePOS > 0)
                {
                    queryName = queryName.Substring(0, spacePOS).Trim();
                }
                bool goodRead = _readers[queryName].DataReader.Read();
                bool isFieldNull = false;
                if (goodRead)
                {
                    int ctr = 0;
                    foreach (object fieldObj in fields)
                    {
                        if (fieldObj is FieldNullIndicator)
                        {
                            FieldNullIndicator fni = (FieldNullIndicator)fieldObj;
                            if (isFieldNull)
                            {
                                fni.NullFieldInd.Assign(-1);
                            }
                            else
                            {
                                fni.NullFieldInd.Assign(0);
                            }

                        }
                        else if (fieldObj is IBufferValue)
                        {
                            IBufferValue fieldValue = (IBufferValue)fieldObj;

                            if (_readers[queryName].DataReader.IsDBNull(ctr))
                            {
                                isFieldNull = true;
                                fieldValue.InitializeWithLowValues();
                            }
                            else if (fieldObj is IField)
                            {
                                isFieldNull = false;
                                string colType = _readers[queryName].DataReader.GetDataTypeName(ctr).ToUpper();
                                if (colType == "DATE" || colType == "DATETIME")
                                {
                                    DateTime colDate = _readers[queryName].DataReader.GetDateTime(ctr);
                                    fieldValue.AssignFrom(colDate.ToString(DefaultDateFormat));
                                }
                                else if (colType == "INT16" || colType == "SHORT")
                                    fieldValue.SetValue(_readers[queryName].DataReader.GetInt16(ctr));
                                else if (colType == "INT32" || colType == "INT")
                                    fieldValue.SetValue(_readers[queryName].DataReader.GetInt32(ctr));
                                else if (colType == "INT64" || colType == "LONG")
                                    fieldValue.SetValue(_readers[queryName].DataReader.GetInt64(ctr));
                                else if (colType == "DECIMAL")
                                    fieldValue.SetValue(_readers[queryName].DataReader.GetDecimal(ctr));
                                else
                                fieldValue.AssignFrom(_readers[queryName].DataReader.GetString(ctr));
                            }
                            else if (fieldObj is IGroup)
                            {
                                string varcharString = _readers[queryName].DataReader.GetString(ctr);
                                IGroup groupField = (IGroup)fieldObj;
                                CheckForVarCharGroupAndAssign(groupField, varcharString);
                                isFieldNull = false;
                            }
                            ctr++;
                        }


                    }
                    SQLCA.SQLERRD[3].SetValue(1);
                    SetSqlCode(0);
                }
                else
                {
                    SetSqlCode(100);
                }
            }
            catch (Exception ex)
            {
                SetSqlCode(26);
                throw new Exception(string.Concat("DB Fetch row problem: ", ex.Message, " ", ex.StackTrace));
            }
        }

        /// <summary>
        /// Fetch the multiple rows from the DataReader based on the named query. Load values into Field Arrays.
        /// </summary>
        /// <param name="queryName"></param>
        /// <param name="fetchRows"></param>
        /// <param name="fields"></param>
        public void FetchReaderRows(string queryName, int fetchRows, params object[] fields)
        {
            try
            {
                SQLCA.SQLERRD[3].SetValue(0);
                int spacePOS = queryName.IndexOf(' ');
                if (spacePOS > 0)
                {
                    queryName = queryName.Substring(0, spacePOS).Trim();
                }

                bool goodRead = _readers[queryName].DataReader.Read();
                if (!goodRead)
                {
                    SetSqlCode(100);
                    return;
                }
                int rowctr = 0;
                for (rowctr = 0; rowctr < fetchRows && goodRead; rowctr++)
                {
                    bool isFieldNull = false;
                    if (goodRead)
                    {
                        int ctr = 0;
                        foreach (object fieldObj in fields)
                        {
                            if (fieldObj is FieldNullIndicator)
                            {
                                FieldNullIndicator fni = (FieldNullIndicator)fieldObj;
                                if (isFieldNull)
                                {
                                    fni.NullFieldInd.Assign(-1);
                                }
                                else
                                {
                                    fni.NullFieldInd.Assign(0);
                                }

                            }
                            else if (fieldObj is IArrayElementAccessorBase)
                            {
                                if (fieldObj is IArrayElementAccessor<IField>)
                                {
                                    IArrayElementAccessor<IField> fieldValue = (IArrayElementAccessor<IField>)fieldObj;

                                    if (_readers[queryName].DataReader.IsDBNull(ctr))
                                    {
                                        isFieldNull = true;
                                        fieldValue[rowctr + 1].InitializeWithLowValues();
                                    }
                                    else
                                    {
                                        isFieldNull = false;
                                        string colType = _readers[queryName].DataReader.GetDataTypeName(ctr).ToUpper();
                                        if (colType == "DATE" || colType == "DATETIME")
                                        {
                                            DateTime colDate = _readers[queryName].DataReader.GetDateTime(ctr);
                                            fieldValue[rowctr + 1].AssignFrom(colDate.ToString(DefaultDateFormat));
                                        }
                                        else if (colType == "INT16" || colType == "SHORT")
                                            fieldValue[rowctr + 1].SetValue(_readers[queryName].DataReader.GetInt16(ctr));
                                        else if (colType == "INT32" || colType == "INT")
                                            fieldValue[rowctr + 1].SetValue(_readers[queryName].DataReader.GetInt32(ctr));
                                        else if (colType == "INT64" || colType == "LONG")
                                            fieldValue[rowctr + 1].SetValue(_readers[queryName].DataReader.GetInt64(ctr));
                                        else if (colType == "DECIMAL")
                                            fieldValue[rowctr + 1].SetValue(_readers[queryName].DataReader.GetDecimal(ctr));
                                        else
                                            fieldValue[rowctr + 1].AssignFrom(_readers[queryName].DataReader.GetString(ctr));
                                    }
                                }
                                else if (fieldObj is IArrayElementAccessor<IGroup>)
                                {
                                    string varcharString = _readers[queryName].DataReader.GetString(ctr);
                                    IArrayElementAccessor<IGroup> groupField = (IArrayElementAccessor<IGroup>)fieldObj;
                                    List<IBufferElement> fieldList = groupField[rowctr + 1].Elements.ToList();
                                    if (fieldList.Count() == 2 && fieldList[0] is IField)
                                    {
                                        IField checkTypeField = (IField)fieldList[0];
                                        if (checkTypeField.FieldType == FieldType.CompShort)
                                        {
                                            IField varCharField = (IField)fieldList[1];
                                            varCharField.AssignFrom(varcharString);
                                            checkTypeField.SetValue(varcharString.Length);
                                        }
                                        else
                                        {
                                            groupField[rowctr + 1].AssignFrom(varcharString);
                                        }
                                    }
                                    else
                                    {
                                        groupField[rowctr + 1].AssignFrom(varcharString);
                                    }
                                    isFieldNull = false;
                                }
                                ctr++;
                            }


                        }
                        SQLCA.SQLERRD[3].SetValue(rowctr);
                        SetSqlCode(0);
                        goodRead = _readers[queryName].DataReader.Read();
                    }
                }
            }
            catch (Exception ex)
            {
                SetSqlCode(26);
                throw new Exception(string.Concat("DB Fetch row problem: ", ex.Message, " ", ex.StackTrace));
            }
        }

        /// <summary>
        /// Get SQLCA
        /// </summary>
        /// <returns></returns>
        public Oracle_SQLCA GetSqlca()
        {
            return SQLCA;
        }

        /// <summary>
        /// Returns latest sqlcode
        /// </summary>
        /// <returns></returns>
        public int GetSqlCode()
        {
            return SQLCA.SQLCODE.AsInt();  
        }

        /// <summary>
        /// Open the database connection
        /// </summary>
        public void OpenConnection()
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();
                SetSqlCode(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Issue with OpenConnection " + ex.Message);
                SetSqlCode(8);
                //throw new Exception(string.Concat("DB Open problem: ", ex.Message, " ", ex.StackTrace));
            }

        }

        /// <summary>
        /// Commit the transaction
        /// </summary>
        public void Commit()
        {
            SavePoint();
            SetSqlCode(0);
        }
        /// <summary>
        /// Sets the schema name to be used for table queries
        /// </summary>
        /// <param name="schemaName"></param>
        public void SetCurrentSchema(string schemaName)
        {
            BatchControl.CurrentSchema = schemaName;
            SetSqlCode(0);
        }
        /// <summary>
        /// Sets the schema name to be used for table queries
        /// </summary>
        /// <param name="schemaField"></param>
        public void SetCurrentSchema(IField schemaField)
        {
            SetCurrentSchema(schemaField.AsString().Trim());
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Format command parameters for debugging string
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static string FormatCommandParms(DbCommand command)
        {
            StringBuilder commandValues = new StringBuilder();

            foreach (DbParameter dbParm in command.Parameters)
            {
                //commandValues.AppendLine(string.Concat("ParmName: ", dbParm.ParameterName, " ParmType: ", dbParm.DbType, " ParmValue: '", dbParm.Value.ToString(), "'"));
                commandValues.AppendFormat("ParmName: {0} ParmType: {1} ParmValue: '{2}'", dbParm.ParameterName, dbParm.DbType, dbParm.Value).AppendLine();
            }

            return commandValues.ToString();
        }

        /// <summary>
        /// Set up database connection objects
        /// </summary>
        private void SetNewDBConnection()
        {
            try
            {
                string providerName = ConfigSettings.GetAppSettingsString("DbConnFactory");
                string connectionString = ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString");
                _dbFactory = DbProviderFactories.GetFactory(providerName);

                _connection = _dbFactory.CreateConnection();
                _connection.ConnectionString = connectionString;

                _readers = new Dictionary<string, ReaderQuery>();
                _dynamicQueries = new Dictionary<string, string>();

                //Get Default schema from config file
                if (string.IsNullOrEmpty(BatchControl.CurrentSchema))
                {
                    BatchControl.CurrentSchema = ConfigSettings.GetAppSettingsString("DefaultSchema");
                }


                if (!(String.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("DefaultDateFormat"))))
                {
                    DefaultDateFormat = ConfigSettings.GetAppSettingsString("DefaultDateFormat");
                }
                else
                    DefaultDateFormat = "yyyy-MM-dd";

                SQLCA = new Oracle_SQLCA();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Issue with SetNewDBConnection " + ex.Message);
                //throw new Exception("Error with DBFactory", ex);
            }
        }

        /// <summary>
        /// Create DataReader based on Select query 
        /// </summary>
        /// <param name="rQuery"></param>
        private void CreateReader(ReaderQuery rQuery)
        {
            OpenConnection();

            _command = _connection.CreateCommand();
            if (Transaction != null)
            {
                _command.Transaction = Transaction;
            }
            _command.CommandType = CommandType.Text;


            //Check for parms before FROM
            int fromPos = rQuery.QueryText.ToUpper().IndexOf(" FROM ");
            int firstBracket = rQuery.QueryText.IndexOf('{');
            if (firstBracket > -1 && firstBracket < fromPos)
            {
                string qFirst = rQuery.QueryText.Substring(0, fromPos).Replace("}",":P");

                string[] qParts = qFirst.Split(new char[1] { '{' });
                StringBuilder sbQuery = new StringBuilder();
                int parmCtr = 0;
                foreach (string qPart in qParts)
                {
                    if (qPart.Trim().EndsWith(":P") || qPart.Trim().EndsWith(":P,") || qPart.Trim().EndsWith(":P ,"))
                    {
                        if (rQuery.FieldParms[parmCtr] is IBufferValue)
                        {
                            IBufferValue pValue = (IBufferValue)rQuery.FieldParms[parmCtr];
                            sbQuery.Append(string.Concat("'", pValue.DisplayValue.Replace(Char.ToString((char)0x00), ""), "'"));
                            if (qPart.Trim().EndsWith(","))
                                sbQuery.Append(",");
                        }
                        else
                        {
                            sbQuery.Append("?");
                        }
                        rQuery.FieldParms[parmCtr] = "**IGNORE**";
                        parmCtr++;
                    }
                    else
                        sbQuery.Append(qPart);
                }
                rQuery.QueryText = string.Concat(sbQuery.ToString(), " ", rQuery.QueryText.Substring(fromPos));
            }

            _command.CommandText = CheckForTimeStampColumns(rQuery.QueryText);

            if (BatchControl.CurrentSchema.Trim() != string.Empty)
            {
                _command.CommandText = SetSchemaForTables(_command.CommandText);
            }


            if (rQuery.FieldParms != null && rQuery.FieldParms.Count() > 0)
            {
                SetPassedParms(rQuery.FieldParms);
            }

            try
            {
                rQuery.DataReader = _command.ExecuteReader();
                SimpleLogging.LogMessageToFile(string.Concat("Oracle Open Reader: ", rQuery.ReaderName, ": ", _command.CommandText, FormatCommandParms(_command)));
                SetSqlCode(0);
            }
            catch (Exception ex)
            {
                string message = string.Concat("DB problem creating Reader: ", rQuery.ReaderName, ex.Message, " ", _command.CommandText, FormatCommandParms(_command));
                SimpleLogging.LogMandatoryMessageToFile(message);
                throw new Exception(message, ex);

            }

        }

        private string CheckForTimeStampColumns(string sqlQuery)
        {
            if (TimeStampOverride.Instance.TimeStampList.Count == 0)
                return sqlQuery;
            bool hasTimeStamp = false;
            int fromPos = sqlQuery.ToUpper().IndexOf(" FROM ");
            int startPos = 0;
            if (sqlQuery.ToUpper().Trim().StartsWith("SELECT "))
                startPos = 7;
            string[] queryParts = sqlQuery.Substring(startPos, fromPos - startPos).Split(',');
            for (int ctr = 0; ctr < queryParts.Length; ctr++)
            {
                string testPart = queryParts[ctr].Trim();
                string testPartNoPrefix = testPart;
                if (testPart.Contains("."))
                    testPartNoPrefix = testPart.Substring(testPart.LastIndexOf(".", StringComparison.InvariantCulture) + 1);
                if (TimeStampOverride.Instance.TimeStampList.Contains(testPartNoPrefix))
                {
                    hasTimeStamp = true;
                    queryParts[ctr] = string.Concat("VARCHAR(", testPart, ")");
                }
            }
            string returnString = sqlQuery;
            if (hasTimeStamp)
            {
                StringBuilder sbQuery = new StringBuilder();
                if (startPos == 7)
                    sbQuery.Append("SELECT ");
                foreach (string queryPart in queryParts)
                {
                    sbQuery.Append(queryPart.Trim());
                    sbQuery.Append(" , ");
                }
                sbQuery.Remove(sbQuery.Length - 2, 2);
                returnString = string.Concat(sbQuery.ToString(), " ", sqlQuery.Substring(fromPos));
            }

            return returnString;
        }

        /// <summary>
        /// Create Query parameters 
        /// </summary>
        /// <param name="parms"></param>
        private void SetPassedParms(object[] parms)
        {
            List<string> stringParms = new List<string>();
            int parmCtr = 0;
            try
            {
                for (int i = 0; i < parms.Length; i++)
                {
                    string parmName = string.Format(":Parm{0}", parmCtr + 1);
                    bool isNull = false;
                    if (parms[i] is FieldNullIndicator)
                    {
                        continue;
                    }
                    else if (parms[i] is String)
                    {
                        string pString = (string)parms[i];
                        if (pString == "**IGNORE**") continue;
                        _command.Parameters.Add(CreateParameter(parmName, parms[i]));
                    }
                    else if (parms[i] is IField)
                    {
                        IField parmField = (IField)parms[i];
                        if (parms.Length > i + 1 && parms[i + 1] is FieldNullIndicator)
                        {
                            FieldNullIndicator fni = (FieldNullIndicator)parms[i + 1];
                            isNull = (fni.NullFieldInd.IsEqualTo(-1));
                            if (!isNull)
                            {
                                if (parmField.IsMinValue())
                                    isNull = true;
                            }
                        }

                        SetFieldParm(parmName, parmField, isNull);

                        _command.Parameters[parmName].Size = parmField.DisplayLength;
                    }
                    else if (parms[i] is IGroup)
                    {
                        IGroup parmGroup = (IGroup)parms[i];
                        if (parms.Length > i + 1 && parms[i + 1] is FieldNullIndicator)
                        {
                            FieldNullIndicator fni = (FieldNullIndicator)parms[i + 1];
                            isNull = (fni.NullFieldInd.IsEqualTo(-1));
                            if (!isNull)
                            {
                                if (parmGroup.IsMinValue())
                                    isNull = true;
                            }
                        }
                        if (isNull)
                        {
                            _command.Parameters.Add(CreateParameter(parmName, DBNull.Value));
                        }
                        else
                        {
                            int startidx = 0;
                            if (parmGroup.Elements.First() is IField)
                            {
                                IField checkTypeField = (IField)parmGroup.Elements.First();
                                if (parmGroup.Elements.Count() == 2 && checkTypeField.FieldType == FieldType.CompShort)
                                    startidx = 2;
                            }
                            _command.Parameters.Add(CreateParameter(parmName, parmGroup.BytesAsString.Substring(startidx)));
                        }
                    }
                    else
                        _command.Parameters.Add(CreateParameter(parmName, parms[i]));
                    parmCtr++;
                    stringParms.Add(parmName);
                }

                _command.CommandText = string.Format(_command.CommandText, stringParms.ToArray());
            }
            catch (Exception ex)
            {
                StringBuilder sbparms = new StringBuilder();
                foreach (string sparm in stringParms)
                {
                    sbparms.Append(string.Concat(",", sparm));
                }
                string message = string.Concat("DB parm problem: ", ex.Message, " ", _command.CommandText, sbparms.ToString());
                SimpleLogging.LogMandatoryMessageToFile(message);

                throw ex;
            }

        }
        /// <summary>
        /// Set Pareaametsr for Stored proc
        /// </summary>
        /// <param name="parms"></param>
        private void SetStoredProcParms(object[] parms)
        {
            List<string> stringParms = new List<string>();
            int parmCtr = 0;
            try
            {
                for (int i = 0; i < parms.Length; i++)
                {
                    string parmName = string.Format(":Parm{0}", parmCtr + 1);
                    bool isNull = false;
                    if (parms[i] is FieldNullIndicator)
                    {
                        continue;
                    }
                    else if (parms[i] is String)
                    {
                        string pString = (string)parms[i];
                        if (pString == "**IGNORE**") continue;
                        _command.Parameters.Add(CreateParameter(parmName, parms[i]));
                    }
                    else if (parms[i] is IField)
                    {
                        IField parmField = (IField)parms[i];
                        parmName = string.Concat(":", parmField.Name);
                        if (parms.Length > i + 1 && parms[i + 1] is FieldNullIndicator)
                        {
                            FieldNullIndicator fni = (FieldNullIndicator)parms[i + 1];
                            isNull = (fni.NullFieldInd.IsEqualTo(-1));
                            if (!isNull)
                            {
                                if (parmField.IsMinValue())
                                    isNull = true;
                            }
                        }

                        SetFieldParm(parmName, parmField, isNull);

                        _command.Parameters[parmName].Size = parmField.DisplayLength;
                    }
                    else if (parms[i] is IGroup)
                    {
                        IGroup parmGroup = (IGroup)parms[i];
                        parmName = string.Concat(":", parmGroup.Name);
                        if (parms.Length > i + 1 && parms[i + 1] is FieldNullIndicator)
                        {
                            FieldNullIndicator fni = (FieldNullIndicator)parms[i + 1];
                            isNull = (fni.NullFieldInd.IsEqualTo(-1));
                            if (!isNull)
                            {
                                if (parmGroup.IsMinValue())
                                    isNull = true;
                            }
                        }
                        if (isNull)
                        {
                            _command.Parameters.Add(CreateParameter(parmName, DBNull.Value));
                        }
                        else
                        {
                            int startidx = 0;
                            if (parmGroup.Elements.First() is IField)
                            {
                                IField checkTypeField = (IField)parmGroup.Elements.First();
                                if (parmGroup.Elements.Count() == 2 && checkTypeField.FieldType == FieldType.CompShort)
                                    startidx = 2;
                            }
                            _command.Parameters.Add(CreateParameter(parmName, parmGroup.BytesAsString.Substring(startidx)));
                        }
                        _command.Parameters[parmName].Size = parmGroup.LengthInBuffer;
                            }
                            else
                        _command.Parameters.Add(CreateParameter(parmName, parms[i]));

                    parmCtr++;
                    //Following commented code needed for SQL Server
                    //if (_command.Parameters[parmName].Direction == ParameterDirection.Output)
                    //    stringParms.Add(string.Concat(parmName, " OUTPUT"));
                    //else
                    stringParms.Add(parmName);
                }

                _command.CommandText = string.Format(_command.CommandText, stringParms.ToArray());
            }
            catch (Exception ex)
            {
                StringBuilder sbparms = new StringBuilder();
                foreach (string sparm in stringParms)
                {
                    sbparms.Append(string.Concat(",", sparm));
                }
                string message = string.Concat("DB parm problem: ", ex.Message, " ", _command.CommandText, sbparms.ToString());
                SimpleLogging.LogMandatoryMessageToFile(message);

                throw ex;
            }

        }
        /// <summary>
        /// Set up parameters for SQl SET 
        /// </summary>
        /// <param name="isSetFieldParm"></param>
        /// <param name="parms"></param>
        private void SetSetParms(bool isSetFieldParm, object[] parms)
        {
            List<string> stringParms = new List<string>();
            int parmCtr = 0;
            try
            {
                for (int i = 0; i < parms.Length; i++)
                {
                    string parmName = string.Format(":Parm{0}", parmCtr + 1);
                    bool isNull = false;
                    if (parms[i] is FieldNullIndicator)
                    {
                        continue;
                    }
                    else if (parms[i] is String)
                    {
                        string pString = (string)parms[i];
                        if (pString == "**IGNORE**") continue;
                        _command.Parameters.Add(CreateParameter(parmName, parms[i]));
                    }
                    else if (parms[i] is IField)
                    {
                        IField parmField = (IField)parms[i];
                        parmName = string.Concat(":", parmField.Name);
                        DbParameter newParm;
                        if (parms.Length > i + 1 && parms[i + 1] is FieldNullIndicator)
                        {
                            FieldNullIndicator fni = (FieldNullIndicator)parms[i + 1];
                            isNull = (fni.NullFieldInd.IsEqualTo(-1));
                            if (!isNull)
                            {
                                if (parmField.IsMinValue())
                                    isNull = true;
                        }
                    }

                        SetFieldParm(parmName, parmField, isNull);

                        _command.Parameters[parmName].Size = parmField.DisplayLength;
                    }
                    else if (parms[i] is IGroup)
                    {
                        IGroup parmGroup = (IGroup)parms[i];
                        parmName = string.Concat(":", parmGroup.Name);
                        if (parms.Length > i + 1 && parms[i + 1] is FieldNullIndicator)
                        {
                            FieldNullIndicator fni = (FieldNullIndicator)parms[i + 1];
                            isNull = (fni.NullFieldInd.IsEqualTo(-1));
                            if (!isNull)
                            {
                                if (parmGroup.IsMinValue())
                                    isNull = true;
                            }
                        }
                        if (isNull)
                        {
                            _command.Parameters.Add(CreateParameter(parmName, DBNull.Value));
                        }
                        else
                        {
                            int startidx = 0;
                            if (parmGroup.Elements.First() is IField)
                            {
                                IField checkTypeField = (IField)parmGroup.Elements.First();
                                if (parmGroup.Elements.Count() == 2 && checkTypeField.FieldType == FieldType.CompShort)
                                    startidx = 2;
                            }
                            _command.Parameters.Add(CreateParameter(parmName, parmGroup.BytesAsString.Substring(startidx)));
                        }
                        _command.Parameters[parmName].Size = parmGroup.LengthInBuffer;
                    }
                    else
                        _command.Parameters.Add(CreateParameter(parmName, parms[i]));

                    stringParms.Add(parmName);
                    if (parmCtr == 0 && isSetFieldParm)
                        _command.Parameters[parmName].Direction = ParameterDirection.Output;
                    parmCtr++;
                }

                _command.CommandText = string.Format(_command.CommandText, stringParms.ToArray());
            }
            catch (Exception ex)
            {
                StringBuilder sbparms = new StringBuilder();
                foreach (string sparm in stringParms)
                {
                    sbparms.Append(string.Concat(",", sparm));
                }
                string message = string.Concat("DB parm problem: ", ex.Message, " ", _command.CommandText, sbparms.ToString());
                SimpleLogging.LogMandatoryMessageToFile(message);

                throw ex;
            }

        }
        /// <summary>
        /// Set IField parameter
        /// </summary>
        /// <param name="parmName"></param>
        /// <param name="parmField"></param>
        private void SetFieldParm(string parmName, IField parmField, bool nullFlag)
        {
            DbParameter dbParm;
            if (parmField.DBColumnType == DBColumnType.BigInt)
            {
                if (nullFlag)
                    dbParm = CreateParameter(parmName, DBNull.Value);
                else
                    dbParm = CreateParameter(parmName, parmField.BytesAsString);
                dbParm.DbType = DbType.AnsiString;
                dbParm.Size = 26;
            }
            else if (parmField.DBColumnType == DBColumnType.Date)
            {
                if (nullFlag || (parmField.AsDateString().Replace(".", "").Replace("-", "").Trim() == string.Empty))
                    dbParm = CreateParameter(parmName, DBNull.Value);
                else
                    dbParm = CreateParameter(parmName, parmField.AsDateString());
                dbParm.DbType = DbType.Date;
            }
            else if (parmField.FieldType == FieldType.String)
            {
                if (nullFlag)
                    dbParm = CreateParameter(parmName, DBNull.Value);
                else
                    dbParm = CreateParameter(parmName, parmField.BytesAsString);
            }
            else if (parmField.FieldType == FieldType.CompInt)
            {
                if (nullFlag)
                    dbParm = CreateParameter(parmName, DBNull.Value);
                else
                    dbParm = CreateParameter(parmName, parmField.AsInt());
                dbParm.DbType = DbType.Int32;
            }
            else if (parmField.FieldType == FieldType.CompShort)
            {
                if (nullFlag)
                    dbParm = CreateParameter(parmName, DBNull.Value);
                else
                    dbParm = CreateParameter(parmName, parmField.AsInt16());
                dbParm.DbType = DbType.Int16;
            }
            else if (parmField.FieldType == FieldType.CompLong)
            {
                if (nullFlag)
                    dbParm = CreateParameter(parmName, DBNull.Value);
                else
                    dbParm = CreateParameter(parmName, parmField.AsInt64());
                dbParm.DbType = DbType.Int64;
            }
            else if (parmField.FieldType == FieldType.SignedDecimal || parmField.FieldType == FieldType.UnsignedDecimal
                || parmField.FieldType == FieldType.SignedNumeric || parmField.FieldType == FieldType.UnsignedNumeric
                || parmField.FieldType == FieldType.PackedDecimal || parmField.FieldType == FieldType.UnsignedPackedDecimal)
            {
                if (nullFlag)
                    dbParm = CreateParameter(parmName, DBNull.Value);
                else
                    dbParm = CreateParameter(parmName, parmField.AsDecimal());
                dbParm.DbType = DbType.Decimal;
            }
            else
            {
                if (nullFlag)
                    dbParm = CreateParameter(parmName, DBNull.Value);
                else
                    dbParm = CreateParameter(parmName, parmField.AsString());
            }

            _command.Parameters.Add(dbParm);

            if (parmField.DecimalDigits > 0)
            {
                IDbDataParameter DecimalParm = _command.Parameters[parmName];
                DecimalParm.Scale = Convert.ToByte(parmField.DecimalDigits);
            }

        }
        /// <summary>
        /// UpdateField(IbufferValue) wih output parm value
        /// </summary>
        /// <param name="field"></param>
        /// <param name="dbParm"></param>
        private void UpdateFieldFromDBParm(IBufferValue field, DbParameter dbParm, FieldNullIndicator fni = null)
        {
            if (fni != null)
            {
                fni.NullFieldInd.Assign(0);
            }
            if (dbParm.Value.GetType() == typeof(System.DBNull))
            {
                field.InitializeWithLowValues();
                if (fni != null)
                { 
                    fni.NullFieldInd.Assign(-1);
                }
            }
            else if (dbParm.DbType == DbType.String)
            {
                if (field is IGroup)
                {
                    IGroup groupField = (IGroup)field;
                    CheckForVarCharGroupAndAssign(groupField, (string)dbParm.Value);
                }
                else
                {
                field.SetValue((string)dbParm.Value);
                }
            }
            else if (dbParm.DbType == DbType.Decimal)
                field.SetValue((decimal)dbParm.Value);
            else if (dbParm.DbType == DbType.Int16)
                field.SetValue((short)dbParm.Value);
            else if (dbParm.DbType == DbType.Int32)
                field.SetValue((int)dbParm.Value);
            else if (dbParm.DbType == DbType.Int64)
                field.SetValue((long)dbParm.Value);
            else if (dbParm.DbType == DbType.DateTime)
                field.SetValue((DateTime)dbParm.Value);
            else if (dbParm.DbType == DbType.Binary)
                field.SetValue((Byte[])dbParm.Value);
        }

        /// <summary>
        /// Create DbParameter
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parmName"></param>
        /// <param name="parmValue"></param>
        /// <returns></returns>
        private DbParameter CreateParameter(string parmName, object parmValue)
        {
            DbParameter param = _command.CreateParameter();
            param.ParameterName = parmName;
            param.Value = parmValue;
            if (parmValue is string)
                param.DbType = DbType.String;


            return param;
        }

        private void SetSqlCode(int code)
        {
            SQLCA.SQLCODE.SetValue(code);
        }

        /// <summary>
        /// Remove 'INTO' variables from the SQL string and add to intoParms List
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        private string RemoveIntoVariables(string sqlQuery, Object[] fieldParms)
        {
            _intoVariables = new List<object>();

            int intoPos = sqlQuery.ToUpper().IndexOf(" INTO ");
            if (intoPos < 0) return sqlQuery;
            int fromPos = sqlQuery.ToUpper().IndexOf(" FROM ");

            string[] intoParms = sqlQuery.Substring(intoPos + 5, fromPos - intoPos - 5).Split(',');
            int ctr = 0;
            int.TryParse(intoParms[0].Trim().Replace("{", "").Replace("}", ""), out ctr);

            if (ctr > 0)
            // Set ExtraVariables from host variables before an INTO
            {
                for (int ctr2 = 0; ctr2 < ctr; ctr2++)
                {
                    //Replace Host Variables before INTO with string value
                    if (fieldParms[ctr2] is IField)
                    {
                        IField field = (IField)fieldParms[ctr2];
                        if (field.IsNumericType)
                            sqlQuery = sqlQuery.Replace(string.Concat("{", ctr2.ToString(), "}"), field.AsDecimal().ToString());
                        else
                            sqlQuery = sqlQuery.Replace(string.Concat("{", ctr2.ToString(), "}"), string.Concat("'", field.BytesAsString, "'"));
                    }
                    else
                    {
                        IBufferValue fieldValue = (IBufferValue)fieldParms[ctr2];
                        sqlQuery = sqlQuery.Replace(string.Concat("{", ctr2.ToString(), "}"), string.Concat("'", fieldValue.BytesAsString, "'"));
                    }
                    //_extraVariables.Add(fieldParms[ctr2]);
                }
                intoPos = sqlQuery.ToUpper().IndexOf(" INTO ");
                fromPos = sqlQuery.ToUpper().IndexOf(" FROM ");
            }

            //Set Into Variables
            foreach (string parm in intoParms)
            {
                _intoVariables.Add(fieldParms[ctr]);
                ctr++;
                if (ctr < fieldParms.Count() && fieldParms[ctr] is FieldNullIndicator)
                {
                    _intoVariables.Add(fieldParms[ctr]);
                    ctr++;
                }
            }

            //ExtraVariables created from host variables after FROM
            for (int ctr2 = ctr; ctr2 < fieldParms.Count(); ctr2++)
            {
                _extraVariables.Add(fieldParms[ctr2]);
            }

            //Reset Query string removing INTO variables 
            int otherParmCtr = 0;
            string newSQL = sqlQuery.Remove(intoPos, fromPos - intoPos);
            if (_extraVariables.Count > 0)
            {
                string[] sqlWords = newSQL.Split(' ');
                for (int wctr = 0; wctr < sqlWords.Length; wctr++)
                {
                    if (sqlWords[wctr].StartsWith("{"))
                    {
                        sqlWords[wctr] = string.Concat("{", otherParmCtr.ToString(), "}");
                        otherParmCtr++;
                    }
                }
                StringBuilder sbSql = new StringBuilder();
                for (int wctr = 0; wctr < sqlWords.Length; wctr++)
                {
                    sbSql.Append(sqlWords[wctr]);
                    sbSql.Append(" ");
                }
                newSQL = sbSql.ToString();
            }

            return newSQL;
        }

        /// <summary>
        /// Updates the 'INTO' fields stored in the intoParms list from the Columns retuned from the Sql query 
        /// </summary>
        /// <param name="selectDT"></param>
        private void UpdateIntoVariables(DataTable selectDT)
        {
            if (selectDT.Rows.Count > 0)
            {
                bool isFieldNull = false;
                int colCtr = 0;
                for (int parmCtr = 0; parmCtr < _intoVariables.Count; parmCtr++)
                {
                    if (_intoVariables[parmCtr] is FieldNullIndicator)
                    {
                        FieldNullIndicator fni = (FieldNullIndicator)_intoVariables[parmCtr];
                        if (isFieldNull)
                        {
                            fni.NullFieldInd.Assign(-1);
                        }
                        else
                        {
                            fni.NullFieldInd.Assign(0);
                        }
                    }
                    else if (_intoVariables[parmCtr] is IBufferValue)
                    {
                        IBufferValue field = (IBufferValue)_intoVariables[parmCtr];

                        if (selectDT.Rows[0][colCtr] == DBNull.Value)
                        {
                            isFieldNull = true;
                            field.InitializeWithLowValues();
                        }
                        else
                        {
                            isFieldNull = false;

                            if (selectDT.Rows[0][colCtr] == DBNull.Value)
                                field.InitializeWithLowValues();
                            else if (field is IField)
                            {
                                if (selectDT.Columns[colCtr].DataType == typeof(System.String))
                                    field.AssignFrom((string)selectDT.Rows[0][colCtr]);
                                else if (selectDT.Columns[colCtr].DataType == typeof(System.Decimal))
                                {
                                    decimal testDec = (decimal)selectDT.Rows[0][colCtr];
                                    field.AssignFrom(testDec.ToString());
                                }
                                else if (selectDT.Columns[colCtr].DataType == typeof(System.Int16))
                                {
                                    short testNbr = (short)selectDT.Rows[0][colCtr];
                                    field.AssignFrom(testNbr.ToString());
                                }
                                else if (selectDT.Columns[colCtr].DataType == typeof(System.Int32))
                                {
                                    int testNbr = (int)selectDT.Rows[0][colCtr];
                                    field.AssignFrom(testNbr.ToString());
                                }
                                else if (selectDT.Columns[colCtr].DataType == typeof(System.Int64))
                                {
                                    long testNbr = (long)selectDT.Rows[0][colCtr];
                                    field.AssignFrom(testNbr.ToString());
                                }
                                else if (selectDT.Columns[colCtr].DataType == typeof(System.Byte[]))
                                {
                                    byte[] testBytes = (byte[])selectDT.Rows[0][colCtr];
                                    field.AssignFrom(testBytes);
                                }
                                else if (selectDT.Columns[colCtr].DataType == typeof(System.DateTime))
                                {
                                    IField tempField = (IField)_intoVariables[parmCtr];
                                    DateTime testDate = (DateTime)selectDT.Rows[0][colCtr];
                                    if (tempField.DBColumnType == DBColumnType.DateTime)
                                    {
                                        field.AssignFrom(testDate.ToString("yyyy-MM-dd-HH:mm:ss.ffffff"));
                                    }
                                    else
                                    {
                                        field.AssignFrom(testDate.ToString(DefaultDateFormat));
                                    }
                                }
                                else if (selectDT.Columns[colCtr].DataType == typeof(System.TimeSpan))
                                {
                                    TimeSpan testSpan = (TimeSpan)selectDT.Rows[0][colCtr];
                                    field.AssignFrom(testSpan.ToString("hh\\:mm\\:ss"));
                                }
                            }
                            else if (field is IGroup)
                            {

                                CheckForVarCharGroupAndAssign((IGroup)field, (string)selectDT.Rows[0][colCtr]);

                            }
                        }
                        colCtr++;
                    }
                    else if (_intoVariables[parmCtr] is string)
                    {
                        if (selectDT.Rows[0][colCtr] == DBNull.Value)
                            _intoVariables[parmCtr] = string.Empty;
                        else
                        {
                            _intoVariables[parmCtr] = (string)selectDT.Rows[0][colCtr];
                        }
                    }
                }
            }
        }

        private string SetSchemaForTables(string sqlString)
        {
            string[] sqlParts = sqlString.Split(' ');
            // Split query and iterate through parts and uprefix table names as needed with Schema


            if ((sqlString.ToUpper().StartsWith("SELECT") || sqlString.ToUpper().StartsWith("DECLARE") || sqlString.ToUpper().StartsWith("WITH")))
            {
                bool nextParmIsTable = false;
                bool lastParmWasTable = false;
                for (int ctr = 0; ctr < sqlParts.Length; ctr++)
                {
                    if (sqlParts[ctr].ToUpper() == "FROM" || (sqlParts[ctr].ToUpper() == "JOIN" && (sqlParts[ctr - 1].ToUpper() == "OUTER" || sqlParts[ctr - 1].ToUpper() == "INNER")))
                    {
                        nextParmIsTable = true;
                        lastParmWasTable = false;
                    }
                    else if (nextParmIsTable)
                    {
                        if (sqlParts[ctr].ToUpper().StartsWith("WHERE") || sqlParts[ctr].ToUpper().StartsWith("ORDER") || sqlParts[ctr].ToUpper().StartsWith("ON")
                            || sqlParts[ctr].ToUpper().StartsWith("("))
                        {
                            nextParmIsTable = false;
                            lastParmWasTable = false;
                        }
                        else if (lastParmWasTable)
                        {
                            if (sqlParts[ctr].EndsWith(","))
                                lastParmWasTable = false;
                        }
                        else if (!sqlParts[ctr].Contains(".") && !sqlParts[ctr].StartsWith("(") && !sqlParts[ctr].Trim().IsEmpty())
                        {
                            sqlParts[ctr] = string.Concat(BatchControl.CurrentSchema, ".", sqlParts[ctr]);
                            lastParmWasTable = true;
                        }
                        else if (sqlParts[ctr].StartsWith("SESSION.") || sqlParts[ctr].StartsWith("SYSIBM."))
                            nextParmIsTable = false;
                        else if (sqlParts[ctr].Contains("."))
                            lastParmWasTable = true;
                    }
                }
            }
            else
            {
                bool nextParmIsTable = false;
                for (int ctr = 0; ctr < sqlParts.Length; ctr++)
                {
                    if (sqlParts[ctr].ToUpper() == "UPDATE" ||
                        (sqlParts[ctr].ToUpper() == "INTO" && sqlParts[ctr - 1].ToUpper() == "INSERT") ||
                        (sqlParts[ctr].ToUpper() == "FROM" && sqlParts[ctr - 1].ToUpper() == "DELETE"))
                    {
                        nextParmIsTable = true;
                    }
                    else if (nextParmIsTable)
                    {
                        if (sqlParts[ctr].ToUpper().StartsWith("WHERE") || sqlParts[ctr].ToUpper().StartsWith("SET") ||
                           sqlParts[ctr].ToUpper().StartsWith("("))
                        {
                            nextParmIsTable = false;
                        }
                        else if (!sqlParts[ctr].Contains(".") && !sqlParts[ctr].StartsWith("(") && !sqlParts[ctr].Trim().IsEmpty())
                    {
                        sqlParts[ctr] = string.Concat(BatchControl.CurrentSchema, ".", sqlParts[ctr]);
                        nextParmIsTable = false;
                    }
                        else if (sqlParts[ctr].Contains("."))
                            nextParmIsTable = false;
                    }
                }

                }
                StringBuilder sbSql = new StringBuilder();
                foreach (string sqlPart in sqlParts)
                {
                    sbSql.AppendFormat("{0} ", sqlPart);
                }
                sqlString = sbSql.ToString();
            return sqlString;
        }

        private string ReplaceQuestionMarks(string sqlString)
        {
            string updatedSql = sqlString;
            if (sqlString.Contains("?"))
            {
                StringBuilder newSqlString = new StringBuilder();
                string[] sqlParms = sqlString.Split(' ');
                int qmCtr = 0;
                for (int ctr = 0; ctr < sqlParms.Length; ctr++)
                {
                    if (sqlParms[ctr].Contains("?"))
                    {
                        sqlParms[ctr] = sqlParms[ctr].Replace("?", string.Concat("{", qmCtr.ToString(), "}"));
                        qmCtr++;
                    }
                    newSqlString.Append(sqlParms[ctr]);
                    newSqlString.Append(" ");
                }
                updatedSql = newSqlString.ToString();
            }
            return updatedSql;
        }

        private void CheckForVarCharGroupAndAssign(IGroup group, string dbValue)
        {
            string varcharString = dbValue;
            IGroup groupField = (IGroup)group;
            List<IBufferElement> fieldList = groupField.Elements.ToList();
            if (fieldList.Count() == 2 && fieldList[0] is IField)
            {
                IField checkTypeField = (IField)fieldList[0];
                if (checkTypeField.FieldType == FieldType.CompShort)
                {
                    IField varCharField = (IField)fieldList[1];
                    varCharField.AssignFrom(varcharString);
                    checkTypeField.SetValue(varcharString.Length);
            }
                else
            {
                    groupField.AssignFrom(varcharString);
                }
            }
            else
            {
                groupField.AssignFrom(varcharString);
            }
        }
        #endregion

    }
}
