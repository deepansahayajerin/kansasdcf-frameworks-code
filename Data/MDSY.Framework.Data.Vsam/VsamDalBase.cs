using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using MDSY.Framework.Buffer;
using MDSY.Framework.Buffer.Services;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Interfaces;
using MDSY.Framework.Buffer.BaseClasses;
using MDSY.Framework.Core;
using MDSY.Framework.Configuration.Common;
using Oracle.ManagedDataAccess.Client;

namespace MDSY.Framework.Data.Vsam
{
    public abstract class VsamDalBase : PredefinedRecordBase
    {
        #region Constructors
        public VsamDalBase()
        {
            VsamDalRecord = RecordOfLength(50);
            SetNewDBConnection();
            ReadCache = 25;
            HasVsamKey = true;
            IsSequentialRead = false;
            AlternateIndex = string.Empty;
            UseAlternateIndex = false;
            ReadKey = new VsamKey();
            LastKey = new VsamKey();
            // if no CommandTimeout key is found set value to normal default of 30 seconds

            _commandTimeout = String.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("CommandTimeout")) ? 30
                : Convert.ToInt32(ConfigSettings.GetAppSettingsString("CommandTimeout"));

            _doLogging = ConfigSettings.GetAppSettingsBool("LogFileEnabled");

            _logErrorsOnly = ConfigSettings.GetAppSettingsBool("LogFileErrorOnly");

            UseReadAhead = ConfigSettings.GetAppSettingsBool("UseReadAhead");
        }

        public VsamDalBase(DbConnection passedConnection)
        {
            VsamDalRecord = RecordOfLength(50);
            connection = passedConnection;
            SetUpCommandObject();
            ReadCache = 20;
            HasVsamKey = true;
            IsSequentialRead = false;
            AlternateIndex = string.Empty;
            UseAlternateIndex = false;
            ReadKey = new VsamKey();
            LastKey = new VsamKey();
            // if no CommandTimeout key is found set value to normal default of 30 seconds
            _commandTimeout = String.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("CommandTimeout")) ? 30
                : Convert.ToInt32(ConfigSettings.GetAppSettingsString("CommandTimeout"));

            _doLogging = ConfigSettings.GetAppSettingsBool("LogFileEnabled");

            _logErrorsOnly = ConfigSettings.GetAppSettingsBool("LogFileErrorOnly");

            UseReadAhead = ConfigSettings.GetAppSettingsBool("UseReadAhead");
        }
        #endregion

        public void PrepRecord(int length)
        {
            if (VsamDalRecord.Length != length)
            {
                VsamDalRecord = RecordOfLength(length);
            }
        }

        #region Private Properties
        private DbProviderFactory dbFactory { get; set; }
        private DbConnection connection { get; set; }
        private DbDataReader reader { get; set; }

        private DbDataAdapter dataAdapter { get; set; }
        private DataTable insertDataTable;
        private string _readOper;
        private string _order;
        private bool isPartialSearch;
        private const string comma = ",";
        private const string primaryKey = "VSAM_KEY";

        private string parmPrefix = "@";
        private string providerName;
        private string IdentitySql = "Select Scope_Identity()";
        private const string STR_LatestID_SQLServer = "; Select Scope_Identity();";
        private const string STR_LatestID_Oracle = " returning {0} into {1} ";
        private const string STR_OracleLatestID = ":OracleLatestID";
        private const string STR_OracleCurrentTimstamp = "current_Timestamp";
        private const string STR_OracleRowLock = " FOR UPDATE";
        private string parmTop = "Top({0})";
        private string parmTop1 = "Top(1)";
        private string parmFetch = " Fetch First {0} rows only ";
        private string parmFetch1 = " Fetch First 1 rows only ";
        private bool _doLogging = false;
        private bool _logErrorsOnly = false;
        private int insertWriteCounter;
        private string _lastCommand = string.Empty;
        private string _currentKeyName = string.Empty;
        private string _duplicateKeyLogic = string.Empty;
        private string _connectionString = string.Empty;
        private int _commandTimeout = 30;   //Default value
        private bool isVSAMOracle = false;
        private bool isVSAMsqlServer = false;
        #endregion

        #region Protected Properties
        public IRecord VsamDalRecord { get; set; }
        public DataTable VsamDalDataTable { get; set; }
        public DataTable VsamDalDataTable2 { get; set; }
        public DataTable ChildDataTable { get; set; }
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string WhereClauseHint { get; set; }
        protected DbCommand Command { get; set; }
        protected DbCommand Command2 { get; set; }
        public int DataTableCurrentRow { get; set; }
        protected StringBuilder TableColumns { get; set; }
        protected StringBuilder TableParms { get; set; }
        protected StringBuilder UpdateColumns { get; set; }
        protected StringBuilder UpdateCachedQuery { get; set; }
        public VsamKey LastKey { get; set; }
        protected string ChildTableName { get; set; }
        public string IdColumnName { get; set; }
        protected Int64 LastIDColumn { get; set; }
        protected bool isMultiView { get; set; }
        protected string SequenceName { get; set; }
        protected long InsertedID { get; private set; }
        public bool HasVsamKey { get; set; }
        public int VsamKeyLength { get; set; }
        public int VsamKeyOffset { get; set; }
        protected List<string> MultiViewTableList { get; set; }
        public Dictionary<string, AlternateIndex> AlternateIndexes { get; set; }
        public bool IsSequentialRead { get; set; }
        public bool IsBinaryKey { get; set; }
        protected VsamKey ReadKey { get; set; }
        public string AlternateIndex { get; set; }
        public bool UseAlternateIndex { get; set; }
        public int ReadCache { get; set; }
        public bool UseReadAhead { get; set; }
        public string ConnectionString
        {
            get
            {
                if (connection != null) return connection.ConnectionString; else return "";
            }
            private set { }
        }
        public DbTransaction DalTransaction { get; set; }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Copy database column values to record buffer fields 
        /// </summary>
        public abstract void SetRecordData();

        /// <summary>
        /// Create database parameters from the record buffer fields 
        /// </summary>
        /// <param name="command"></param>
        public abstract void SetUpdateParameters(DbCommand command);


        /// <summary>
        /// Check logic for determining child table for multi-view tables
        /// </summary>
        public abstract void SetMultiViewTable();

        #endregion

        #region DB Operations
        /// <summary>
        /// Open database connection
        /// </summary>
        public void OpenConnection()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
        }

        /// <summary>
        /// Close database connection
        /// </summary>
        public void CloseConnection()
        {
            if (connection.State == ConnectionState.Open)
                connection.Close();
        }

        /// <summary>
        /// Get database rows based on start key. Number of rows retrived will be determined by readCache setting.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="readOptions"></param>
        /// <returns></returns>
        public int StartRead(VsamKey key, params ReadOption[] readOptions)
        {
            if (isVSAMOracle)
            {
                parmFetch = string.Format(parmFetch, ReadCache.ToString());
                parmTop = String.Empty;
            }
            else
            {
                parmTop = string.Format(parmTop, ReadCache.ToString());
                parmFetch = String.Empty;
            }
            bool useLike = CheckReadOptions(key, readOptions);
            Command.Parameters.Clear();
            StringBuilder joinLogic = new StringBuilder();
            if (isMultiView)
            {
                int ctr = 1;
                foreach (string mvTable in MultiViewTableList)
                {
                    joinLogic.Append(String.Format(" left outer join {0} mv{1} on mv{1}.{2} = t.{2} ",
                        mvTable, ctr.ToString(), IdColumnName));
                    ctr++;
                }
            }
            if (HasVsamKey && !IsSequentialRead)
            {
                SetKeyData(key, "StartKey", useLike);
                if (!IsBinaryKey && string.IsNullOrEmpty(key.StringKey))
                {
                    Command.CommandText = string.Format("SET ARITHABORT ON; with crow as (Select {0} {4} from {1} where {2} is not null order by {2} {3} ) Select * from crow inner join {1} t on t.{4} = crow.{4} {5} order by t.{2} {6}",
                      parmTop, (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName, _currentKeyName, _order, IdColumnName, joinLogic.ToString(), parmFetch);
                }
                else
                {
                    Command.CommandText = string.Format("SET ARITHABORT ON; with crow as (Select {0} {6} from {1} where {2} {3} @StartKey {4} order by {2} {5} ) Select * from crow inner join {1} t on t.{6} = crow.{6} {7} order by t.{2} {8}",
                      parmTop, (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName, _currentKeyName, _readOper, _duplicateKeyLogic, _order, IdColumnName, joinLogic.ToString(), parmFetch);
                }
            }
            else
            {
                if (isVSAMOracle)
                {
                    if (readOptions[0].ToString() == "GT")
                    {
                        Command.Parameters.Clear();
                        Command.Parameters.Add(CreateParameter("StartID", key.BinaryKey));
                        //Command.CommandText = string.Concat("SELECT * FROM ", TableName, " WHERE VSAM_KEY > :StartID ORDER BY ", TableName, "_ID ASC ", parmFetch);
                        Command.CommandText = string.Concat("SELECT * FROM ", TableName, " WHERE VSAM_KEY > :StartID ORDER BY VSAM_KEY ASC ", parmFetch);
                    }
                }
                else
                {
                    if (key.StringKey == null || key.StringKey.IsMinValue())
                    {
                        key.StringKey = "0";
                    }
                    if (key.StringKey == "0")
                    {
                        Command.CommandText = string.Format("SET ARITHABORT ON; Select {0} * from {1} t{2} " + (!string.IsNullOrEmpty(WhereClauseHint) && WhereClauseHint.StartsWith("where") ? WhereClauseHint : (!string.IsNullOrEmpty(WhereClauseHint) ? "where " + WhereClauseHint : "")) + " order by t.{3} asc {5}",
                           parmTop, (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName, joinLogic.ToString(), IdColumnName, parmFetch);
                    }
                    else
                    {
                        Command.Parameters.Add(CreateParameter("StartID", Convert.ToInt32(key.StringKey)));
                        Command.CommandText = string.Format("SET ARITHABORT ON; Select {0} * from {1} t{2} where t.{3} {4} @StartID " + (!string.IsNullOrEmpty(WhereClauseHint) ? WhereClauseHint.Replace("where ", "and ") : "") + " order by t.{3} asc {5}",
                           parmTop, (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName, joinLogic.ToString(), IdColumnName, _readOper, parmFetch);
                    }
                }
            }
            DataTableCurrentRow = -1;
            LastKey = new VsamKey();

            return ExecuteSelect();
        }

        public void ReadAhead()
        {
            try
            {
                if (Command2.CommandText.Contains("with crow as"))     // for now ReadAhead only working with Sequential Read.
                    return;

                VsamDalDataTable2 = new DataTable();
                //Command2.CommandText = (Command2.CommandText + "|").Replace(";|", "").Replace("|", "").Replace(" top(" + ReadCache.ToString() + ") ", " top(" + (ReadCache * 2).ToString() + ") ") + " OFFSET " + ReadCache.ToString() + " ROWS FETCH NEXT " + ReadCache.ToString() + " ROWS ONLY ";
                Command2.CommandText = (Command2.CommandText + "|").Replace(";|", "").Replace("|", "").Replace(" top(" + ReadCache.ToString() + ") ", " ") + " OFFSET " + ReadCache.ToString() + " ROWS FETCH NEXT " + ReadCache.ToString() + " ROWS ONLY ";

                OpenConnection();
                CreateLogMessage(Command2);
                //if (DalTransaction != null)
                //{
                //    Command.Transaction = DalTransaction;
                //}

                if (dataAdapter == null)
                {
                    dataAdapter = dbFactory.CreateDataAdapter();
                }
                dataAdapter.SelectCommand = Command2;
                dataAdapter.Fill(VsamDalDataTable2);
            }
            catch { }
        }

        /// <summary>
        /// Retrieves the next row from the internal datatable. If on last row of datatable, retrieve more rows from the database.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int ReadNext(VsamKey key)
        {
            DataTableCurrentRow++;
            int keyLength = VsamKeyLength;
            if (!(isVSAMOracle))
            {
                if (key.StringKey != null && key.StringKey.Length < VsamKeyLength)
                    keyLength = key.StringKey.Length;
            }

            bool isNewKey = false;
            if (IsBinaryKey)
            {
                if (LastKey.BinaryKey != null)
                {
                    if (LastKey.BinaryKey.Length > key.BinaryKey.Length)
                    {
                        byte[] tmp = new byte[key.BinaryKey.Length];
                        Array.Copy(LastKey.BinaryKey, tmp, tmp.Length);
                        isNewKey = !key.BinaryKey.SequenceEqual<byte>(tmp);
                    }
                    else if (LastKey.BinaryKey.Length < key.BinaryKey.Length)
                    {
                        byte[] tmp = new byte[LastKey.BinaryKey.Length];
                        Array.Copy(key.BinaryKey, tmp, tmp.Length);
                        isNewKey = !LastKey.BinaryKey.SequenceEqual<byte>(tmp);
                    }
                    else
                        isNewKey = !LastKey.BinaryKey.SequenceEqual<byte>(key.BinaryKey);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(LastKey.StringKey))
                {
                    if (!IsSequentialRead)
                    {
                        if (LastKey.StringKey.Length > key.StringKey.Length)
                            isNewKey = !LastKey.StringKey.StartsWith(key.StringKey);
                        else
                            isNewKey = !key.StringKey.StartsWith(LastKey.StringKey);
                    }
                }
                else
                {
                    LastKey.StringKey = string.Empty;
                }
            }
            if (!isNewKey && !UseAlternateIndex && VsamDalDataTable != null && VsamDalDataTable2 != null && DataTableCurrentRow >= VsamDalDataTable.Rows.Count)
            {
                VsamDalDataTable = VsamDalDataTable2;
                VsamDalDataTable2 = null;
                DataTableCurrentRow = 0;
            }
            else if (VsamDalDataTable == null || DataTableCurrentRow >= VsamDalDataTable.Rows.Count || (UseAlternateIndex && AlternateIndexes[AlternateIndex].Name != _currentKeyName) || isNewKey)
            {
                if (IsSequentialRead && LastKey.StringKey == string.Empty)
                {
                    LastKey.StringKey = "0";
                }
                if ((LastKey.BinaryKey == null && IsBinaryKey) || (LastKey.StringKey == null && !IsBinaryKey))
                {
                    LastKey = key;
                }

                if (UseAlternateIndex && AlternateIndexes[AlternateIndex].IsbinaryKey && LastKey.BinaryKey == null
                    || UseAlternateIndex && (!AlternateIndexes[AlternateIndex].IsbinaryKey) && LastKey.StringKey == null)
                {
                    LastKey = key;
                }

                if (IsBinaryKey)
                {
                    if (LastKey.BinaryKey != key.BinaryKey)
                        LastKey = key;
                }
                else
                {
                    if (LastKey.StringKey != key.StringKey)
                        LastKey = key;
                }

                StartRead(LastKey, ReadOption.GT);
                if (VsamDalDataTable.Rows.Count > 0)
                {
                    DataTableCurrentRow = 0;
                }
                else
                {
                    return 10;
                }
                if (UseReadAhead)
                    try
                    {
                        if (VsamDalDataTable.Rows.Count.Equals(ReadCache))
                        {
                            if (Command2 == null)
                                Command2 = connection.CreateCommand();
                            Command2.CommandText = Command.CommandText;   // just to capture before it may change
                            Command2.Parameters.Clear();
                            if (Command.Parameters.Count > 0)
                            {
                                foreach (DbParameter p in Command.Parameters)
                                {
                                    DbParameter param = Command2.CreateParameter();
                                    param.ParameterName = p.ParameterName;
                                    param.DbType = p.DbType;
                                    param.Value = p.Value;
                                    Command2.Parameters.Add(param);
                                }
                            }
                            System.Threading.Thread t = new System.Threading.Thread(ReadAhead);
                            t.Start();
                        }
                    }
                    catch { }
            }
            else if (UseAlternateIndex)
            {
                if (AlternateIndexes[AlternateIndex].IsbinaryKey)
                {
                    if (AlternateIndexes[AlternateIndex].LastKey.BinaryKey == key.BinaryKey)
                        DataTableCurrentRow++;
                    else if (LastKey.BinaryKey != null && (LastKey.BinaryKey == key.BinaryKey))
                        DataTableCurrentRow++;
                }
                else
                {
                    if (AlternateIndexes[AlternateIndex].LastKey.StringKey == key.StringKey)
                        DataTableCurrentRow++;
                    else if (LastKey.StringKey != null && (LastKey.StringKey == key.StringKey))
                        DataTableCurrentRow++;
                }
            }
            SetRecordData();
            LastIDColumn = (Int64)VsamDalDataTable.Rows[DataTableCurrentRow][0];
            if (string.IsNullOrEmpty(IdColumnName))
            {
                IdColumnName = VsamDalDataTable.Columns[0].ColumnName;
            }

            if (IsSequentialRead)
            {
                if (isVSAMOracle)
                {
                    LastKey.VsamKeyLength = VsamKeyLength;
                    LastKey.VsamKeyOffset = VsamKeyOffset;
                }
                else
                    LastKey.StringKey = Convert.ToString(VsamDalDataTable.Rows[DataTableCurrentRow][IdColumnName]);
            }
            _lastCommand = "ReadNext";
            return 0;

        }

        /// <summary>
        /// Retrieves the previous row from the internal datatable. If on first row of datatable, retrieve more rows from the database.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int ReadPrev(VsamKey key)
        {
            if (_lastCommand != "ReadNext")
                DataTableCurrentRow--;

            bool isNewKey = false;
            if (IsBinaryKey)
            {
                if (LastKey.BinaryKey != null)
                {
                    if (LastKey.BinaryKey.Length > key.BinaryKey.Length)
                    {
                        byte[] tmp = new byte[key.BinaryKey.Length];
                        Array.Copy(LastKey.BinaryKey, tmp, tmp.Length);
                        isNewKey = !key.BinaryKey.SequenceEqual<byte>(tmp);
                    }
                    else if (LastKey.BinaryKey.Length < key.BinaryKey.Length)
                    {
                        byte[] tmp = new byte[LastKey.BinaryKey.Length];
                        Array.Copy(key.BinaryKey, tmp, tmp.Length);
                        isNewKey = !LastKey.BinaryKey.SequenceEqual<byte>(tmp);
                    }
                    else
                        isNewKey = !LastKey.BinaryKey.SequenceEqual<byte>(key.BinaryKey);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(LastKey.StringKey))
                {
                    if (LastKey.StringKey.Length > key.StringKey.Length)
                        isNewKey = !LastKey.StringKey.StartsWith(key.StringKey);
                    else
                        isNewKey = !key.StringKey.StartsWith(LastKey.StringKey);
                }
            }

            if (VsamDalDataTable == null || DataTableCurrentRow < 0 || (UseAlternateIndex && AlternateIndexes[AlternateIndex].Name != _currentKeyName) || isNewKey)
            {
                if (IsSequentialRead && LastKey.StringKey == string.Empty)
                {
                    LastKey.StringKey = "0";
                }
                if (LastKey.BinaryKey == null || LastKey.StringKey == null)
                {
                    LastKey = key;
                }
                StartRead(LastKey, ReadOption.LT);
                if (VsamDalDataTable.Rows.Count > 0)
                {
                    DataTableCurrentRow = VsamDalDataTable.Rows.Count - 1;
                }
                else
                {
                    return 10;
                }
            }
            else if (UseAlternateIndex && AlternateIndexes[AlternateIndex].LastKey.StringKey == key.StringKey.Substring(0, AlternateIndexes[AlternateIndex].KeyLength))
            {
                string currentKey = (string)VsamDalDataTable.Rows[DataTableCurrentRow][primaryKey];
                if (AlternateIndexes[AlternateIndex].LastKey.StringKey == currentKey && _lastCommand != "ReadNext")
                    DataTableCurrentRow--;
            }
            else if (LastKey.StringKey == key.StringKey.Substring(0, VsamKeyLength))
            {
                string currentKey = (string)VsamDalDataTable.Rows[DataTableCurrentRow][primaryKey];
                if (LastKey.StringKey == currentKey && _lastCommand != "ReadNext")
                    DataTableCurrentRow--;
            }


            SetRecordData();
            LastIDColumn = (Int64)VsamDalDataTable.Rows[DataTableCurrentRow][0];
            if (string.IsNullOrEmpty(IdColumnName))
            {
                IdColumnName = VsamDalDataTable.Columns[0].ColumnName;
            }
            if (IsSequentialRead)
            {
                LastKey.StringKey = Convert.ToString(VsamDalDataTable.Rows[DataTableCurrentRow][IdColumnName]);
            }
            _lastCommand = "ReadPrev";
            return 0;

        }

        /// <summary>
        /// Read a specific record from database table based on the logical key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int ReadByKey(VsamKey key)
        {
            StringBuilder joinLogic = new StringBuilder();
            if (isMultiView)
            {
                int ctr = 1;
                foreach (string mvTable in MultiViewTableList)
                {
                    joinLogic.Append(String.Format(" left outer join {0} mv{1} on mv{1}.{2} = t.{2} ",
                        mvTable, ctr.ToString(), IdColumnName));
                    ctr++;
                }
            }

            SetKeyData(key, "ReadKey");

            if (isVSAMOracle)
            {
                Command.CommandText = string.Concat("SELECT * FROM ", TableName, " WHERE VSAM_KEY = :ReadKey");
            }
            else
            {
                Command.CommandText = string.Format("Select * from {0} t {1} where {2} = @ReadKey", (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName, joinLogic.ToString(), _currentKeyName);
                if (TableName.Contains("PTFL") || TableName.Contains("LTFL") || TableName.Contains("PTFH") || TableName.Contains("LTFH"))
                {
                    Command.CommandText = string.Format("Select * from {0} t {1} with (NOLOCK) where {2} = @ReadKey", (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName, joinLogic.ToString(), _currentKeyName);
                }
            }

            int result = ExecuteSelect();
            if (result == 0 && VsamDalDataTable.Rows.Count == 0)
            {
                result = 23;
            }
            else
            {

                DataTableCurrentRow = 0;
                SetRecordData();
                LastIDColumn = (Int64)VsamDalDataTable.Rows[DataTableCurrentRow][0];
                if (string.IsNullOrEmpty(IdColumnName))
                {
                    IdColumnName = VsamDalDataTable.Columns[0].ColumnName;
                }
            }
            _lastCommand = "ReadbyKey";
            return result;
        }

        /// <summary>
        ///  Read the first record from database table matching a partial key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyLength"></param>
        /// <returns></returns>
        public int ReadPartialKey(VsamKey key, int keyLength)
        {
            SetKeyData(key, "ReadKey");

            Command.CommandText = string.Format("Select * from ( select t.*, row_number() over (order by {0}) as rn from {1} t where t.{0} >= @ReadKey ) ot where rn = 1 order by {0} asc",
                _currentKeyName, (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName);
            int rtn = ExecuteSelect();
            int result = rtn == 0 && VsamDalDataTable.Rows.Count == 0 ? 23 : 0;  //

            if (result == 0)
            {
                SetRecordData();
                LastIDColumn = (Int64)VsamDalDataTable.Rows[DataTableCurrentRow][0];
                if (string.IsNullOrEmpty(IdColumnName))
                {
                    IdColumnName = VsamDalDataTable.Columns[0].ColumnName;
                }
            }
            else
            {
                result = 23;
            }
            _lastCommand = "ReadbyPatialKey";
            return result;
        }

        /// <summary>
        /// Insert a new row in database table.
        /// </summary>
        /// <returns></returns>
        public int Write()
        {
            Command.Parameters.Clear();
            TableColumns = new StringBuilder(); TableParms = new StringBuilder(); UpdateColumns = new StringBuilder();
            if (isMultiView)
            // Multi View Parent table insert
            {
                SetMultiViewTable();
                Command.CommandText = string.Format("Insert into {0} (COPYBOOK_KEY) Values ('{1}')", (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName, ChildTableName);
                ExecuteInsert();
                Command.Parameters.Add(CreateParameter(IdColumnName, InsertedID));
            }

            SetUpdateParameters(Command);

            if (!isMultiView)
            // Single table insert
            {
                if (isVSAMOracle)
                    Command.CommandText = string.Concat("INSERT INTO ", TableName, " (VSAM_DATA, ", TableName, "_LASTUPDATED) VALUES (:VSAM_DATA, current_Timestamp)");
                else
                    Command.CommandText = string.Format("Insert into {0} ( {1} ) Values ( {2} ) ", (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName, TableColumns, TableParms);
            }
            else
            // Multi View child table insert
            {
                Command.CommandText = string.Format("Insert into {0} ( {1} ) Values ( {2} ) ", (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + ChildTableName, TableColumns, TableParms);
            }
            return ExecuteInsert();
        }

        public int WriteUsingBulkCopy(int insertCache)
        {
            if (insertWriteCounter >= insertCache)
            {
                InsertBulkData();
            }
            else
            {
                if (insertDataTable == null)
                {
                    insertDataTable = new DataTable();
                    DataColumn vsamData = new DataColumn();
                    vsamData.DataType = System.Type.GetType("System.Byte[]");
                    vsamData.ColumnName = "VSAM_DATA";
                    insertDataTable.Columns.Add(vsamData);
                    DataColumn updateDate = new DataColumn();
                    updateDate.DataType = System.Type.GetType("System.DateTime");
                    updateDate.ColumnName = string.Concat(TableName, "_LastUpdated");
                    insertDataTable.Columns.Add(updateDate);
                }
                insertDataTable.Rows.Add(this.AsBytes, DateTime.Now);
                insertWriteCounter++;
            }

            return 0;
        }

        public void FinishBulkCopyWrite()
        {
            if (insertWriteCounter > 0)
            {
                InsertBulkData();
            }
        }

        /// <summary>
        /// Update existing row on database table.
        /// </summary>
        /// <returns></returns>
        public int ReWrite()
        {
            Command.Parameters.Clear();
            TableColumns = new StringBuilder();
            TableParms = new StringBuilder();
            UpdateColumns = new StringBuilder();
            SetMultiViewTable();
            SetUpdateParameters(Command);
            Command.Parameters.Add(CreateParameter("UpdateID", LastIDColumn));
            if (!isMultiView)
            {
                if (isVSAMOracle)
                    Command.CommandText = string.Concat("UPDATE ", TableName, " SET VSAM_DATA = :VSAM_DATA, ", TableName, "_LASTUPDATED = current_Timestamp WHERE ", IdColumnName, " = :UpdateID");
                else
                    Command.CommandText = string.Format("Update {0} set {1} where {2} = @UpdateID  ", (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName, UpdateColumns, IdColumnName);
            }
            else
            {
                Command.CommandText = string.Format("Update {0} set {1} where {2} = @UpdateID  ", (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + ChildTableName, UpdateColumns, IdColumnName);
            }
            int result = 0;
            int attempts = 0;
            string message = "";
            string stackTrace = "";
            Exception exception = new Exception();
            while (attempts < 5)
            {
                try
                {
                    result = ExecuteNonQuery();
                    break;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    message = ex.Message;
                    stackTrace = ex.StackTrace;
                    if (ex.Message.Contains("deadlocked") || ex.Message.Contains("Execution Timeout Expired"))
                    {
                        System.Threading.Thread.Sleep(10);
                        attempts++;
                        SimpleLogging.LogMandatoryMessageToFile("**** DEADLOCK - Attempt " + attempts + " to update table ****");
                    }
                    else
                    {
                        if (ex.Message.ToUpper().Contains("UNIQUE CONSTRAINT") || ex.Message.Contains("Cannot insert duplicate key row"))
                        {
                            return 22;
                        }
                        else
                            throw new Exception(string.Concat("DB problem: ", ex.Message, " ", Command.CommandText, FormatCommandParms(Command), "\n" + ex.StackTrace));
                    }
                }
            }
            if (attempts == 5)
                throw new Exception(string.Concat("DB problem: ", message, " ", Command.CommandText, FormatCommandParms(Command), "\n" + stackTrace));

            return result;
        }

        /*Following code used for Cached updates public int ReWriteUsingCachedUpdate()
        {
            if (UpdateCachedQuery == null)
            {
                UpdateCachedQuery = new StringBuilder();
            }
            SetDataTableColumns();
            updateReWriteCounter++;
            return 0;
        }

        public void SendCachedUpdatesToDatabase()
        {
            if (updateReWriteCounter > 0)
            {
                Command.CommandText = UpdateCachedQuery.ToString();
                Command.ExecuteNonQuery();
                updateReWriteCounter = 0;
                UpdateCachedQuery = new StringBuilder();
                Command.Parameters.Clear();
            }
       */

        /// <summary>
        /// Delete row on database table based on logical key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int Delete(VsamKey key)
        {
            SetKeyData(key, "DeleteKey");

            Command.CommandText = string.Format("Delete from {0} where {1} = @DeleteKey ", (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName, _currentKeyName);
            int deleteReturn = ExecuteNonQuery();

            return deleteReturn;
        }

        public int Delete()
        {
            if (isVSAMOracle)
                Command.Parameters.Clear();

            Command.Parameters.Add(CreateParameter("DeleteID", LastIDColumn));

            if (isVSAMOracle)
            {
                Command.CommandText = string.Concat("DELETE FROM ", TableName, " WHERE ", IdColumnName, " = :DeleteID");
            }
            else
            {
                Command.CommandText = string.Format("Delete from {0} where {1} = @DeleteID ", (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName, IdColumnName);
            }

            int deleteReturn = ExecuteNonQuery();

            return deleteReturn;
        }

        /// <summary>
        /// Retrieve Child table row based on foreign key 
        /// </summary>
        /// <param name="childTableName"></param>
        /// <param name="IDName"></param>
        public void GetChildData(string childTableName, string IDName)
        {
            Command.CommandText = string.Format("Select * from {0} where {1} = {2}  ", (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + childTableName, IDName, LastIDColumn);

            ExecuteChildSelect();
        }

        /// <summary>
        /// Execute Select Sql command and load results into internal datatable
        /// </summary>
        /// <returns></returns>
        protected int ExecuteSelect()
        {

            VsamDalDataTable = new DataTable();
            Command.CommandType = CommandType.Text;
            Command.CommandTimeout = _commandTimeout;
            Exception exception = new Exception();
            int attempts = 0;

            OpenConnection();
            CreateLogMessage();
            if (DalTransaction != null)
            {
                Command.Transaction = DalTransaction;
            }

            if (dataAdapter == null)
            {
                dataAdapter = dbFactory.CreateDataAdapter();
            }
            while (attempts < 5)
            {
                try
                {
                    dataAdapter.SelectCommand = Command;
                    dataAdapter.Fill(VsamDalDataTable);
                    break;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    if (ex.Message.Contains("deadlocked") || ex.Message.Contains("Execution Timeout Expired"))
                    {
                        System.Threading.Thread.Sleep(10);
                        attempts++;
                        SimpleLogging.LogMandatoryMessageToFile("**** DEADLOCK - Attempt " + attempts + " to read table ****");
                    }
                    else
                    {
                        attempts = 5;
                    }
                }
            }
            if (attempts == 5)
                throw new Exception(string.Concat("DB problem: ", exception.Message, " ", Command.CommandText, FormatCommandParms(Command), exception.StackTrace));

            return 0;
        }

        /// <summary>
        /// Execute update or delete query
        /// </summary>
        /// <returns></returns>
        protected int ExecuteNonQuery()
        {
            int attempts = 0;
            int rowCount = 0;
            Exception exception = new Exception();
            while (attempts < 5)
            {
                try
                {
                    OpenConnection();
                    Command.CommandType = CommandType.Text;
                    CreateLogMessage();
                    if (DalTransaction != null)
                    {
                        Command.Transaction = DalTransaction;
                    }
                    int ExecuteResult = Command.ExecuteNonQuery();
                    rowCount = ExecuteResult;
                    Command.Parameters.Clear();
                    break;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    if (ex.Message.Contains("deadlocked") || ex.Message.Contains("Execution Timeout Expired"))
                    {
                        System.Threading.Thread.Sleep(10);
                        attempts++;
                        SimpleLogging.LogMandatoryMessageToFile("**** DEADLOCK - Attempt " + attempts + " for ExecuteNonQuery ****");
                    }
                    else
                    {
                        attempts = 5;
                    }
                }
            }
            if (attempts == 5)
                throw new Exception(string.Concat("DB problem: ", exception.Message, " ", Command.CommandText, "\n" + exception.StackTrace));

            if (rowCount != 1)
                return 23;
            else
                return 0;
        }

        /// <summary>
        /// Execute Insert query
        /// </summary>
        /// <returns></returns>
        protected int ExecuteInsert()
        {
            int result = 0;
            try
            {
                InsertedID = 0;
                OpenConnection();
                Command.CommandType = CommandType.Text;
                CreateLogMessage();
                if (DalTransaction != null)
                {
                    if (DalTransaction.Connection == null)
                        DalTransaction.Connection.Open();
                    Command.Transaction = DalTransaction;
                }
                if (isVSAMOracle)
                {
                    IdentitySql = string.Format(" RETURNING {0}_ID INTO :CurrID", TableName);
                    DbParameter dbParm = CreateParameter("CurrID", 0);
                    dbParm.Direction = ParameterDirection.Output;
                    Command.Parameters.Add(dbParm);
                }
                //Command.CommandText = IdentitySql;
                //if (isMultiView)
                int attempts = 0;
                Exception exception = new Exception();
                if (Command.Parameters.Contains("@" + IdColumnName))
                {
                    int inserted = 0;

                    while (attempts < 5)
                    {
                        try
                        {
                            inserted = Command.ExecuteNonQuery();
                            break;
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                            if (ex.Message.Contains("deadlocked") || ex.Message.Contains("Execution Timeout Expired"))
                            {
                                System.Threading.Thread.Sleep(10);
                                attempts++;
                                SimpleLogging.LogMandatoryMessageToFile("**** DEADLOCK - Attempt " + attempts + " to insert record ****");
                            }
                            else
                            {
                                if (ex.Message.ToUpper().Contains("UNIQUE CONSTRAINT") || ex.Message.Contains("Cannot insert duplicate key row"))
                                {
                                    result = 22;
                                    return result;
                                }
                                else
                                    attempts = 5;
                            }
                        }
                    }
                    if (attempts == 5)
                        throw new Exception(string.Concat("DB problem: ", exception.Message, " ", Command.CommandText, exception.StackTrace));

                    if (inserted > 0)
                        InsertedID = (long)Command.Parameters["@" + IdColumnName].Value;
                }
                else
                {
                    if (isVSAMOracle)
                        Command.CommandText = string.Concat(Command.CommandText, IdentitySql);
                    else
                        Command.CommandText = string.Concat(Command.CommandText, "; ", IdentitySql);

                    long inserted = 0;
                    while (attempts < 5)
                    {
                        try
                        {
                            if (isVSAMOracle)
                            {
                                inserted = Convert.ToInt64(Command.ExecuteNonQuery());
                                inserted = int.Parse(Command.Parameters[":CurrID"].Value.ToString());
                            }
                            else
                            {
                                inserted = Convert.ToInt64(Command.ExecuteScalar());
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                            if (ex.Message.Contains("deadlocked") || ex.Message.Contains("Execution Timeout Expired"))
                            {
                                System.Threading.Thread.Sleep(10);
                                attempts++;
                                SimpleLogging.LogMandatoryMessageToFile("**** DEADLOCK - Attempt " + attempts + " to insert record ****");
                            }
                            else
                            {
                                if (ex.Message.ToUpper().Contains("UNIQUE CONSTRAINT") || ex.Message.Contains("Cannot insert duplicate key row"))
                                {
                                    result = 22;
                                    return result;
                                }
                                else
                                    attempts = 5;
                            }
                        }
                    }
                    if (attempts == 5)
                        throw new Exception(string.Concat("DB problem: ", exception.Message, " ", Command.CommandText, FormatCommandParms(Command), exception));

                    InsertedID = inserted;
                }

                LastIDColumn = (Int64)InsertedID;
                Command.Parameters.Clear();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("UNIQUE CONSTRAINT") || ex.Message.Contains("Cannot insert duplicate key row"))
                {
                    result = 22;
                    return result;
                }
                else
                    throw new Exception(string.Format("DB problem: {0} {1}{2}", ex.Message, Command.CommandText, FormatCommandParms(Command)), ex);
            }
            return result;
        }

        /// <summary>
        /// Execute select query to retieve child table data
        /// </summary>
        /// <returns></returns>
        protected int ExecuteChildSelect()
        {

            ChildDataTable = new DataTable();
            Command.CommandType = CommandType.Text;

            OpenConnection();
            CreateLogMessage();
            if (DalTransaction != null)
            {
                Command.Transaction = DalTransaction;
            }

            using (DbDataAdapter dataAdapter = dbFactory.CreateDataAdapter())
            {
                dataAdapter.SelectCommand = Command;
                dataAdapter.Fill(ChildDataTable);
            }
            return 0;
            //}    <-- there is no point in catch blocks that simply re-throw
            //catch (Exception ex)
            //{
            //    throw ex;

            //}
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Create DBParameter object with parm data
        /// </summary>
        /// <param name="parmName"></param>
        /// <param name="parmValue"></param>
        /// <returns></returns>
        protected DbParameter CreateParameter(string parmName, object parmValue)
        {
            DbParameter param = Command.CreateParameter();
            param.ParameterName = string.Concat(parmPrefix, parmName);
            if (parmValue is IField)
            {
                IField field = (IField)parmValue;
                if (field.IsNumericType)
                {
                    if (field.IsNumericValue())
                    {
                        param.Value = field.GetValue<decimal>();
                    }
                    else
                    {
                        param.Value = 0;
                    }
                }
                else if (field.AsBytes[0] == 0x00)
                {
                    field.Assign(" ");
                }
                else
                {
                    param.Value = field.AsString();
                }
            }
            else
            {
                param.Value = parmValue;
                if (parmValue is string)
                {
                    param.DbType = DbType.AnsiStringFixedLength;
                    string parmString = (string)parmValue;
                    param.Size = parmString.Length;
                }
                else if (parmValue is byte[])
                {
                    param.DbType = DbType.Binary;
                    byte[] parmBytes = (byte[])parmValue;
                    param.Size = parmBytes.Length;
                }

            }
            if (TableColumns != null && parmName != "UpdateID")
            {
                if (TableColumns.Length > 0)
                {
                    TableColumns.Append(comma);
                    TableParms.Append(comma);
                    UpdateColumns.Append(comma);
                }
                TableColumns.Append(parmName);
                TableParms.Append(param.ParameterName);
                UpdateColumns.Append(string.Concat(parmName, " = ", param.ParameterName));
            }
            return param;
        }

        private static IRecord RecordOfLength(int length, string value = "")
        {
            return BufferServices.Factory.NewRecord("VsmDalRecord", rec =>
            {
                rec.CreateNewField("Text", FieldType.String, length, value);
            });
        }

        public IRecord GetRecord()
        {
            return this.Record;
        }

        /// <summary>
        /// Set up database connection objects
        /// </summary>
        private void SetNewDBConnection()
        {
            try
            {
                string connectionString = ConfigSettings.GetConnectionStrings("VSAMConnectionString", "connectionString");

                SetUpDBFactory();

                connection = dbFactory.CreateConnection();
                connection.ConnectionString = connectionString;

                SetUpCommandObject();
            }
            catch (Exception ex)
            {
                throw new Exception("Error with DBFactory", ex);
            }
        }

        private void SetUpDBFactory()
        {
            providerName = ConfigSettings.GetConnectionStrings("VSAMConnectionString", "providerName");

            if (providerName.Contains("Oracle"))
            {
                DbProviderFactories.RegisterFactory(providerName, OracleClientFactory.Instance);
                isVSAMOracle = true;
                parmPrefix = ":";

            }
            else
            {
                DbProviderFactories.RegisterFactory(providerName, SqlClientFactory.Instance);
                isVSAMsqlServer = true;
            }

            dbFactory = DbProviderFactories.GetFactory(providerName);
        }
        /// <summary>
        /// Set up database client command object
        /// </summary>
        private void SetUpCommandObject()
        {
            Command = connection.CreateCommand();
            if (dbFactory == null)
            {
                SetUpDBFactory();
            }
        }

        /// <summary>
        /// Parse ReadOptions and set parameters used for selecting database data
        /// </summary>
        /// <param name="readOptions"></param>
        private bool CheckReadOptions(VsamKey key, params ReadOption[] readOptions)
        {
            _readOper = ">=";
            //isReadForUpdate = false;
            isPartialSearch = false;
            _order = "";
            bool useLike = false;

            if (readOptions != null)
            {
                foreach (ReadOption ro in readOptions)
                {
                    if (ro == ReadOption.EQUAL)
                    {
                        if (!UseAlternateIndex && key.VsamKeyLength > 0 && key.VsamKeyLength < VsamKeyLength)
                        {
                            useLike = true;
                            _readOper = "like";
                        }
                        else
                            _readOper = "=";
                    }
                    else if (ro == ReadOption.GTEQ)
                    {
                        _readOper = ">=";
                    }
                    else if (ro == ReadOption.GT)
                    {
                        _readOper = ">";
                    }
                    else if (ro == ReadOption.LTEQ)
                    {
                        _readOper = "<=";
                        _order = "DESC";
                    }
                    else if (ro == ReadOption.LT)
                    {
                        _readOper = "<";
                        _order = "DESC";
                    }
                    else if (ro == ReadOption.Update)
                    {
                        // isReadForUpdate = true;
                    }
                    else if (ro == ReadOption.PartialSearch)
                    {
                        isPartialSearch = true;
                    }
                }

            }
            return useLike;
        }

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

        private void CreateLogMessage()
        {
            CreateLogMessage(Command);
        }

        private void CreateLogMessage(DbCommand cmd)
        {
            if (!_doLogging) return;
            if (_logErrorsOnly) return;

            string commandtext = string.Empty;
            if (!string.IsNullOrEmpty(cmd.CommandText))
                commandtext = cmd.CommandText;

            StringBuilder sbParms = new StringBuilder();
            foreach (DbParameter param in cmd.Parameters)
            {
                if ((param.ParameterName != null))
                    if (param.DbType == DbType.Binary)
                    {
                        string hex = BitConverter.ToString((byte[])param.Value).Replace("-", string.Empty);
                        sbParms.AppendFormat("       {0}=0x{1},\r\n", param.ParameterName, hex);
                    }
                    else
                    {
                        sbParms.AppendFormat("       {0}={1},\r\n", param.ParameterName, param.Value);
                    }

            }
            commandtext += "\r\n" + sbParms.ToString();
            SimpleLogging.LogMandatoryMessageToFile(commandtext);

        }

        private void InsertBulkData()
        {
            OpenConnection();
            if (isVSAMOracle)
            {
                using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection.ConnectionString))
                {
                    // column mappings
                    string dateColumn = string.Concat(TableName, "_LastUpdated");
                    bulkCopy.ColumnMappings.Add("VSAM_DATA", "VSAM_DATA");
                    bulkCopy.ColumnMappings.Add(dateColumn, dateColumn);
                    bulkCopy.DestinationTableName = (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName;
                    bulkCopy.WriteToServer(insertDataTable);
                }
            }
            else
            {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection.ConnectionString))
                {
                    // column mappings
                    string dateColumn = string.Concat(TableName, "_LastUpdated");
                    bulkCopy.ColumnMappings.Add("VSAM_DATA", "VSAM_DATA");
                    bulkCopy.ColumnMappings.Add(dateColumn, dateColumn);
                    bulkCopy.DestinationTableName = (!String.IsNullOrEmpty(Schema) ? "[" + Schema + "]." : "") + TableName;
                    bulkCopy.WriteToServer(insertDataTable);
                }
            }
            insertWriteCounter = 0;
            insertDataTable.Clear();
        }

        private void SetKeyData(VsamKey key, string parmKeyName, bool useLike = false)
        {
            _currentKeyName = primaryKey;
            _duplicateKeyLogic = string.Empty;
            bool isThisKeyBinary = IsBinaryKey;
            int keyLength = 0;

            if (!UseAlternateIndex && key.VsamKeyLength > 0 && key.VsamKeyLength < VsamKeyLength)
                keyLength = key.VsamKeyLength;
            else
                keyLength = VsamKeyLength;

            if (UseAlternateIndex)
            {
                _currentKeyName = AlternateIndexes[AlternateIndex].Name;
                isThisKeyBinary = AlternateIndexes[AlternateIndex].IsbinaryKey;
                keyLength = AlternateIndexes[AlternateIndex].KeyLength;
                if ((_lastCommand == "ReadNext" || _lastCommand == "ReadPrev") && AlternateIndexes[AlternateIndex].DuplicatesAllowed)
                {
                    if (_readOper == ">")
                    {
                        _readOper = ">=";
                        _duplicateKeyLogic = string.Concat(" and ", IdColumnName, " > ", LastIDColumn);
                    }
                    if (_readOper == "<")
                    {
                        _readOper = "<=";
                        _duplicateKeyLogic = string.Concat(" and ", IdColumnName, " > ", LastIDColumn);
                    }
                }
            }
            Command.Parameters.Clear();
            if (!isThisKeyBinary)
            {
                if (key.StringKey.Length > keyLength)
                {
                    if (keyLength == 0)
                        keyLength = key.StringKey.Length;
                    key.StringKey = key.StringKey.Substring(0, keyLength);
                }
                if (useLike)
                    key.StringKey += "%";

                Command.Parameters.Add(CreateParameter(parmKeyName, key.StringKey));
            }
            else
            {
                if (key.BinaryKey.Length > keyLength)
                {
                    byte[] tmpKeyBytes = new byte[keyLength];
                    System.Array.Copy(key.BinaryKey, tmpKeyBytes, keyLength);
                    Command.Parameters.Add(CreateParameter(parmKeyName, tmpKeyBytes));
                }
                else
                    Command.Parameters.Add(CreateParameter(parmKeyName, key.BinaryKey));
            }

        }

        #endregion

    }
}
