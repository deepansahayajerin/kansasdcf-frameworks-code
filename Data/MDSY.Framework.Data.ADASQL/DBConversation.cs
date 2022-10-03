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

namespace MDSY.Framework.Data.ADASQL
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
        private int _sqlCode;
        private string parmPrefix = "@";
        private Dictionary<string, ReaderQuery> _readers;
        private List<object> _intoVariables;
        private List<object> _extraVariables;
        private string LastQueryUsed = "";
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

        public IField SqlCode { get; set; }

        public int RESPONSE_CODE { get; set; }

        public int QUANTITY { get; set; }
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
            _extraVariables = new List<object>();
            _command = _connection.CreateCommand();
            if (Transaction != null)
            {
                _command.Transaction = Transaction;
            }

            CurrentDataTable = new DataTable();
            _command.CommandType = CommandType.Text;
            _command.CommandText = RemoveIntoVariables(sqlString, parms);

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

                SimpleLogging.LogMessageToFile(string.Concat("ADASQL: ", _command.CommandText, FormatCommandParms(_command)));
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

            _command = _connection.CreateCommand();
            if (Transaction != null)
            {
                _command.Transaction = Transaction;
            }

            _command.CommandType = CommandType.Text;
            _command.CommandText = sqlString;

            if (parms != null && parms.Count() > 0)
            {
                SetPassedParms(parms);
            }

            try
            {
                int returnCode = _command.ExecuteNonQuery();
                if (returnCode > 0)
                    SetSqlCode(0);
                else
                    SetSqlCode(100);
                SimpleLogging.LogMessageToFile(string.Concat("ADASQL: ", _command.CommandText, FormatCommandParms(_command)));

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
        /// Define query text from Define Cursor syntax
        /// </summary>
        /// <param name="queryName"></param>
        /// <param name="queryText"></param>
        /// <param name="parms"></param>
        public void SetQueryText(string queryName, string queryText, string options,  params object[] parms)
        {
            if (queryText.Contains(" AT END "))
            {
                queryText = queryText.Replace(" AT END ", " END ");
            }
            if (!_readers.ContainsKey(queryName))
                _readers.Add(queryName, new ReaderQuery(queryName, queryText, options, parms));
            SetSqlCode(0);
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
                _readers.Add(queryName, new ReaderQuery(queryName, queryText, null, parms));
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
                LastQueryUsed = queryName;
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
                LastQueryUsed = queryName;
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
                int spacePOS = queryName.IndexOf(' ');
                if (spacePOS > 0)
                {
                    queryName = queryName.Substring(0, spacePOS).Trim();
                }
                LastQueryUsed = queryName;
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
                                fieldValue.AssignFrom(_readers[queryName].DataReader.GetString(ctr));
                            }
                            else if (fieldObj is IGroup)
                            {
                                string varcharString = _readers[queryName].DataReader.GetString(ctr);
                                IGroup groupField = (IGroup)fieldObj;
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
                                isFieldNull = false;
                            }
                            ctr++;
                        }


                    }
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

        public string GetViewData(string queryName, string columnName)
        {
            string value = string.Empty;
            if (!_readers.ContainsKey(queryName))
            {
                return "ERROR!!";
            }

            if (_readers[queryName].DataReader.GetSchemaTable().Columns.Contains(columnName))
                value = (string)_readers[queryName].DataReader[columnName];
            else
                value = "ColumnError!!";

            return value;
        }

        public string GetViewData(string columnName)
        {
            string value = string.Empty;
            if (!_readers.ContainsKey(LastQueryUsed))
            {
                return "ERROR!!";
            }
            if (_readers[LastQueryUsed].DataReader.GetSchemaTable().Columns.Contains(columnName))
                value = (string)_readers[LastQueryUsed].DataReader[columnName];
            else 
                value = "ColumnError!!";

            return value;
        }

        public bool SetViewData(string viewName, string ColumnName, IField source)
        {
            return true;
        }

        public bool SetViewData(string ColumnName, string value)
        {
            return true;
        }

        /// <summary>
        /// Get SQLCA
        /// </summary>
        /// <returns></returns>
        public string GetSqlca()
        {
            throw new NotImplementedException("GetSqlca not yet implemented");
        }

        /// <summary>
        /// Returns latest sqlcode
        /// </summary>
        /// <returns></returns>
        public int GetSqlCode()
        {
            return _sqlCode;
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

        public int GetISN (string viewName)
        {
            return _readers[viewName].ISN;
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

            }
            catch (Exception ex)
            {
                throw new Exception("Error with DBFactory", ex);
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
                string qFirst = rQuery.QueryText.Substring(0, fromPos).Replace("}","@P");

                string[] qParts = qFirst.Split(new char[1] { '{' });
                StringBuilder sbQuery = new StringBuilder();
                int parmCtr = 0;
                foreach (string qPart in qParts)
                {
                    if (qPart.Trim().EndsWith("@P") || qPart.Trim().EndsWith("@P,") || qPart.Trim().EndsWith("@P ,"))
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

            _command.CommandText = rQuery.QueryText;
            LastQueryUsed = rQuery.ReaderName;

            if (rQuery.FieldParms != null && rQuery.FieldParms.Count() > 0)
            {
                SetPassedParms(rQuery.FieldParms);
            }

            try
            {
                rQuery.DataReader = _command.ExecuteReader();
                SimpleLogging.LogMessageToFile(string.Concat("SQL Open Reader: ", rQuery.ReaderName, ": ", _command.CommandText, FormatCommandParms(_command)));
            }
            catch (Exception ex)
            {
                string message = string.Concat("DB problem creating Reader: ", rQuery.ReaderName, ex.Message, " ", _command.CommandText, FormatCommandParms(_command));
                SimpleLogging.LogMessageToFile(message);
                throw new Exception(message, ex);

            }

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
                    string parmName = string.Format("@Parm{0}", parmCtr + 1);
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
                        if (isNull)
                        {
                            _command.Parameters.Add(CreateParameter(parmName, DBNull.Value));
                        }
                        else
                        {

                            if (parmField.FieldType == FieldType.String)
                            {
                                _command.Parameters.Add(CreateParameter(parmName, parmField.BytesAsString));
                            }
                            else
                                _command.Parameters.Add(CreateParameter(parmName, parmField.GetValue<decimal>()));
                        }
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
            return param;
        }

        private void SetSqlCode(int code)
        {
            if (SqlCode != null)
                SqlCode.Assign(code);
            _sqlCode = code;
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

            int intoPos = sqlQuery.IndexOf(" INTO ");
            if (intoPos < 0) return sqlQuery;
            int fromPos = sqlQuery.IndexOf(" FROM ");

            string[] intoParms = sqlQuery.Substring(intoPos + 5, fromPos - intoPos - 5).Split(',');
            int ctr = 0;
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

            for (int ctr2 = ctr; ctr2 < fieldParms.Count(); ctr2++)
            {
                _extraVariables.Add(fieldParms[ctr2]);
            }

            string newSQL = sqlQuery.Remove(intoPos, fromPos - intoPos);
            int parmCtr = 0;
            if (_extraVariables.Count > 0)
            {
                string[] sqlWords = newSQL.Split(' ');
                for (int wctr = 0; wctr < sqlWords.Length; wctr++)
                {
                    if (sqlWords[wctr].StartsWith("{"))
                    {
                        sqlWords[wctr] = string.Concat("{", parmCtr.ToString(), "}");
                        parmCtr++;
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
                                    DateTime testDate = (DateTime)selectDT.Rows[0][colCtr];
                                    field.AssignFrom(testDate.ToString("yyyy-MM-dd"));
                                }
                                else if (selectDT.Columns[colCtr].DataType == typeof(System.TimeSpan))
                                {
                                    TimeSpan testSpan = (TimeSpan)selectDT.Rows[0][colCtr];
                                    field.AssignFrom(testSpan.ToString("hh\\:mm\\:ss"));
                                }
                            }
                            else if (field is IGroup)
                            {
                                string varcharString = (string)selectDT.Rows[0][colCtr];
                                IGroup groupField = (IGroup)field;
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
                        }
                        colCtr++;
                    }
                }
            }
        }
        #endregion

    }
}
