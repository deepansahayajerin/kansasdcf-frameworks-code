using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer;
using MDSY.Framework.Interfaces;
using MDSY.Framework.Buffer.Common;
using System.Reflection;
using MDSY.Framework.Buffer.Services;
using Oracle.ManagedDataAccess.Client;
using MDSY.Framework.Configuration.Common;
using System.Globalization;
using MDSY.Framework.Buffer.BaseClasses;

namespace MDSY.Framework.Data.IDMS
{
    [Serializable]
    public class DBConversation
    {
        #region  Private Fields
        private const string STR_Member = "Member";
        private const string STR_Owner = "Owner";
        private const string STR_All = "All";
        private const string STR_PriorPointerSuffix = "_P";
        private const string STR_NextPointerSuffix = "_N";
        private const string STR_OracleGetCurrVal = "Select {0}.{1}_Seq.CURRVAL from dual";
        private const string STR_OracleGetMaxID = "Select Max{0} from {1}.{2}";
        private const string STR_OracleLatestID = ":OracleLatestID";
        private DBCurrency _currentDBCurrency;
        private bool _isAutoStatus;
        private RecordCurrency _reccurrency;
        private ListCurrency _listcurrency;

        private string _providerName;
        private DbProviderFactory _dbFactory;
        private DbConnection _connection;
        private DbTransaction _transaction;
        private DbCommand _command;
        private int _commandTimeout = 30;   //Default value
        private DbDataReader dr;
        private DbAccessType _dbAccessType;
        private string _storeProcedurePrefixName;
        private long _StartID;
        private int _returnCode;
        private DalRecordBase _ownerDalRecord;
        private ErrorStatusRecord _errorStatusRecord;
        string _parmPrefix = "@";
        string _lastList = string.Empty;
        string _commandRecordName = string.Empty;
        //private string _returnCodeName = "@ReturnCode";
        private SQLCommandUtility _sqlCommandUtility;
        private IField _currentTable;
        private IField _currentList;
        private IRecord InternalRecord;
        private DataTable localDT;

        private bool _canDropCurrencies = true;
        private bool _updateLists = true;
        private bool _updateRecord = false;
        private bool _getLatest = false;
        private bool _inGetInListByKey = false;
        private bool _isOracle = false;
        private bool _isAreaSweepReverse = false;

        //For performance
        private int _tempDbCache = 0;
        private string PerformanceCriteria = "";
        private string PerformanceStatement = "";

        [ThreadStatic]
        private static string _connectionString = string.Empty;
        [ThreadStatic]
        private static string _dALAssemblyName = string.Empty;
        [ThreadStatic]
        private static Dictionary<string, int> _queueCurrency;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns string representation of the ReturnCode value.
        /// </summary>
        public string DBStatus
        {
            get
            {
                return _errorStatusRecord.ReturnCode.AsString();
            }
        }

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

        /// <summary>
        /// Assings CurrentTable to the ErrorRecord object and returns a reference to the ErrorRecord object.
        /// </summary>
        public IField ErrorTableGroup
        {
            get
            {
                _errorStatusRecord.ErrorRecord.SetValue(CurrentTable);
                return _errorStatusRecord.ErrorRecord;
            }

        }

        /// <summary>
        /// Assings CurrentTable to the ErrorRecord object and returns a reference to the ErrorRecord object.
        /// </summary>
        public IField ErrorTable
        {
            get
            {
                return _errorStatusRecord.ErrorRecord;
            }
            set
            {
                _errorStatusRecord.ErrorRecord.SetValue(value);
            }

        }

        /// <summary>
        /// Assigns the string representation of the CurrentList value to the ErrorSet object and returns a reference to the ErrorSet object.
        /// </summary>
        public IField ErrorList
        {
            get
            {
                return _errorStatusRecord.ErrorSet;
            }
            set
            {
                _errorStatusRecord.ErrorSet.SetValue(value);
            }
        }

        /// <summary>
        /// Returns a reference to the internal IField object, which contains the name of the current list.
        /// </summary>
        public IField CurrentList
        {
            get { return _currentList; }
        }

        /// <summary>
        /// Returns a reference to the internal IField object, which contains the name of the current table.
        /// </summary>
        public IField CurrentTable
        {
            get { return _currentTable; }
        }

        /// <summary>
        /// Returns a reference to the internal IField object, which contains the name of the current table.
        /// </summary>
        public IField CurrentTableGroup
        { get { return _currentTable; } }


        /// <summary>
        /// Sets and returns the name of the database.
        /// </summary>
        public string DBName
        {
            get;
            set;
        }

        /// <summary>
        /// Sets and returns the name of the current database conversation.
        /// </summary>
        public string ConversationName
        {
            get { return _currentDBCurrency.ConversationName; }
            set { _currentDBCurrency.ConversationName = value; }
        }

        /// <summary>
        /// Sets and returns a reference to the current DBCurrency object.
        /// </summary>
        public DBCurrency CurrentDBCurrency
        {
            get { return _currentDBCurrency; }
            set { _currentDBCurrency = value; }
        }

        /// <summary>
        /// Sets and returns the maximum number of rows that can be read from the database.
        /// </summary>
        public int MaxRows
        {
            get;
            set;
        }

        /// <summary>
        /// Sets and returns WHERE clause criteria text of the current SQL statement.
        /// </summary>
        public string WhereCriteria
        {
            get;
            set;
        }

        /// <summary>
        /// Sets and returns a flag value, that specifies whether currencies must be updated or not.
        /// </summary>
        public bool NoSaveCurrencyOnLink
        {
            get;
            set;
        }

        /// <summary>
        /// Sets and returns a reference to the collection of the current DAL records.
        /// </summary>
        public IDictionary<string, DalRecordBase> CurrentDalRecords
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a reference to IField object, which contains return code value.
        /// </summary>
        public IField ReturnCode { get { return _errorStatusRecord.ReturnCode; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains StatusGood value.
        /// </summary>
        public ICheckField StatusGood { get { return _errorStatusRecord.StatusGood; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains RowNotFound value.
        /// </summary>
        public ICheckField RowNotFound { get { return _errorStatusRecord.RowNotFound; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains EndOfList value.
        /// </summary>
        public ICheckField EndOfList { get { return _errorStatusRecord.EndOfList; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains AnyError value.
        /// </summary>
        public ICheckField AnyError { get { return _errorStatusRecord.AnyError; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains AnyStatus value.
        /// </summary>
        public ICheckField AnyStatus { get { return _errorStatusRecord.AnyStatus; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains EndOfIndex value.
        /// </summary>
        public ICheckField EndOfIndex { get { return _errorStatusRecord.EndOfIndex; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains IndexRowNotFound value.
        /// </summary>
        public ICheckField IndexRowNotFound { get { return _errorStatusRecord.IndexRowNotFound; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains QueueIDNotFound value.
        /// </summary>
        public ICheckField QueueIDNotFound { get { return _errorStatusRecord.QueueIDNotFound; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains QueueRecordNotFound value.
        /// </summary>
        public ICheckField QueueRecordNotFound { get { return _errorStatusRecord.QueueRecordNotFound; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains ScratchAreaNotFound value.
        /// </summary>
        public ICheckField ScratchAreaNotFound { get { return _errorStatusRecord.ScratchAreaNotFound; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains ScratchAreaNotFound value.
        /// </summary>
        public ICheckField ScratchNotFound { get { return _errorStatusRecord.ScratchAreaNotFound; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains ScratchRecordNotFound value.
        /// </summary>
        public ICheckField ScratchRecordNotFound { get { return _errorStatusRecord.ScratchRecordNotFound; } }

        /// <summary>
        /// Returns a reference to ICheckField object, which contains ScratchRecordReplaced value.
        /// </summary>
        public ICheckField ScratchRecordReplaced { get { return _errorStatusRecord.ScratchRecordReplaced; } }

        /// <summary>
        /// Sets and returns database call value.
        /// </summary>
        public int DBCall { get; set; }

        /// <summary>
        /// Returns a reference to IField object, which contains CallSave record error status.
        /// </summary>
        public IField DBCallSave { get { return _errorStatusRecord.CallSave; } }

        /// <summary>
        /// Returns a reference to IField object, which contains ErrStatSave record error status.
        /// </summary>
        public IField ErrStatSave { get { return _errorStatusRecord.ErrStatSave; } }

        /// <summary>
        /// Returns a reference to IField object, which contains ErrorSet record error status.
        /// </summary>
        public IField ErrorSet { get { return _errorStatusRecord.ErrorSet; } }

        /// <summary>
        /// Direct_DBKey_Type class encapsulates DBKey value.
        /// </summary>
        public class Direct_DBKey_Type
        {
            private int _Direct_DBKey_Element = 0;

            /// <summary>
            /// Sets and returns integer value of the DBKey.
            /// </summary>
            public int Int
            {
                get { return _Direct_DBKey_Element; }
                set { _Direct_DBKey_Element = value; }
            }

            /// <summary>
            /// Sets and returns string representation of the DBKey's numeric value.
            /// Throws an ApplicationException on the attempt to assing a not numeric value.
            /// </summary>
            public string Text
            {

                get { return _Direct_DBKey_Element.ToString(); }
                set
                {
                    int intValue;
                    if (Int32.TryParse(value, out intValue))
                    {
                        _Direct_DBKey_Element = intValue;
                    }
                    else
                        throw new ApplicationException("Direct_DBKey: Attempt to set non numeric data in int field");
                }
            }
        }

        /// <summary>
        /// Contains a reference to a Direct_DBKey_Type object.
        /// </summary>
        public Direct_DBKey_Type Direct_DBKey = new Direct_DBKey_Type();

        /// <summary>
        /// Sets and returns a message prefix text.
        /// </summary>
        public string MessagePrefix { get; set; }

        /// <summary>
        /// Returns the value of the ConnectionStringKey tag from the configuration file.
        /// Throws an ApplicationException if ConnectionStringKey tag is not defined.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (_connectionString == null || _connectionString == string.Empty)
                {
                    try
                    {
                        string connectStringKey = "MainDB";
                        //connectStringKey = configBuilder.Build().GetSection("ConnectionStrings").GetSection("DBConnectionString").Value;
                        IConnectionString _connectionStringInfo = GetDBConnectionInfo();
                        _connectionString = _connectionStringInfo.GetConnectionString("DBConnectionString");
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(String.Concat("Invalid Database Connection String! ", ex.Message));
                    }
                }
                return _connectionString;
            }
        }

        /// <summary>
        /// Returns the name of the DAL assembly as it is specified in the DALAssemblyName tag of the configuration file.
        /// Returns null if DALAssemblyName tag is not defined in the configuration file.
        /// </summary>
        public static string DALAssemblyName
        {
            get
            {
                if (string.IsNullOrEmpty(_dALAssemblyName))
                    _dALAssemblyName = ConfigSettings.GetAppSettingsStringFromSection("Namespaces", "DALAssemblyName");
                return _dALAssemblyName;
            }
        }
        /// <summary>
        /// Queue collection of current Queue Rec ids  
        /// </summary>
        public static IDictionary<string, int> QueueCurrency
        {
            get
            {
                if (_queueCurrency == null)
                {
                    _queueCurrency = new Dictionary<string, int>();
                }

                return _queueCurrency;
            }
        }

        /// <summary>
        /// Sets and returns status of the SaveKeyList.
        /// </summary>
        public ListStatus SaveKeyListStatus { get; set; }
        #endregion

        #region Public Constructors
        /// <summary>
        /// Creates an instance of DBConversation class and initializes it with the provided parameter values.
        /// </summary>
        /// <param name="dbCurrency">A reference to the current DBCurrency object.</param>
        /// <param name="isAutoStatus">A flag, which defines whether the check for autostatus exceptions should be performed.</param>
        public DBConversation(DBCurrency dbCurrency, bool isAutoStatus)
        {
            InitFields();
            //InitData();
            _currentDBCurrency = dbCurrency;
            GetConfigurationData();
            _isAutoStatus = isAutoStatus;
            DBCall = 0;
        }

        /// <summary>
        /// Creates a new instance of DBConversation class with a new instance of the current DBCurrecy object.
        /// </summary>
        public DBConversation()
        {
            InitFields();
            //InitData();
            _currentDBCurrency = new DBCurrency();
            GetConfigurationData();
        }
        #endregion

        #region Public Methods
        #region Select Command Methods
        /// <summary>
        /// Get a database table row (or multiple non-unique rows) based on business key (CALC)
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetByKey(DalRecordBase dalRecord, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            try
            {
                dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
                dalRecord.DBOperation = DbOperation.Select;
                CheckForConnection();

                SetUpSqlCommand(dalRecord, "SelectByBuskey");

                dalRecord.dt = new DataTable();
                dalRecord.DataTableCurrentRow = 0;
                for (int i = _command.Parameters.Count - 1; i >= 0; i--)
                {
                    if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                        _command.Parameters.RemoveAt(i);
                }
                try
                {
                    using (dr = _command.ExecuteReader())
                    {
                        dalRecord.dt.Load(dr);
                    }
                    dalRecord.RefreshCache = false;
                }
                catch (Exception ex)
                {
                    if (Transaction != null)
                        Rollback("");
                    throw new ApplicationException(String.Format("DBRecordBase.GetByKey() failed, DataFlag: {0}{1}", DataFlag, ex.Message), ex);
                }
                if (dalRecord.dt.Rows.Count == 0)
                {
                    RecordReturnCode = 326;

                    RecordCurrency _reccurrency = dalRecord.CurrentRecord;
                    if (_reccurrency != null)
                    {
                        //*** Set Table currencies ***
                        string[] keyarray = new string[_reccurrency.CurrencyKeys.Keys.Count];
                        _reccurrency.CurrencyKeys.Keys.CopyTo(keyarray, 0);
                        //foreach (string skey in keyarray)
                        //{
                        //    if (dalRecord.dt.Columns.Contains(skey))
                        //        _reccurrency.CurrencyKeys[skey] = null;
                        //}
                        _reccurrency.RecordActionCode = RowStatus.NoRow;
                    }
                }
                else
                {
                    RecordReturnCode = 0;
                    SetRecCurrency(dalRecord, STR_All);
                    if (DataFlag == DBReturnData.Yes)
                        dalRecord.SetRecordData();
                }
                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                {
                    CheckAutostatusExceptions(dbExceptions);
                }
                if (RecordReturnCode != 0)
                {
                    ErrorTable.SetValue(dalRecord.TableName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Data Access Layer GetByKey problem: ", dalRecord.RecordName, " : ", ex.Message));
            }
        }

        public void GetByKey(DalRecordBase dalRecord, DBReturnData DataFlag, DBLockType lockType, params DbAllow[] dbExceptions)
        {
            try
            {
                dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
                dalRecord.DBOperation = DbOperation.Select;
                CheckForConnection();

                SetUpSqlCommand(dalRecord, "SelectByBuskey", lockType);

                dalRecord.dt = new DataTable();
                dalRecord.DataTableCurrentRow = 0;
                for (int i = _command.Parameters.Count - 1; i >= 0; i--)
                {
                    if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                        _command.Parameters.RemoveAt(i);
                }
                try
                {
                    using (dr = _command.ExecuteReader())
                    {
                        dalRecord.dt.Load(dr);
                    }
                    dalRecord.RefreshCache = false;
                }
                catch (Exception ex)
                {
                    if (Transaction != null)
                        Rollback("");
                    throw new ApplicationException(String.Format("DBRecordBase.GetByKey() failed, DataFlag: {0}{1}", DataFlag, ex.Message), ex);
                }
                if (dalRecord.dt.Rows.Count == 0)
                {
                    RecordReturnCode = 326;

                    RecordCurrency _reccurrency = dalRecord.CurrentRecord;
                    if (_reccurrency != null)
                    {
                        //*** Set Table currencies ***
                        string[] keyarray = new string[_reccurrency.CurrencyKeys.Keys.Count];
                        _reccurrency.CurrencyKeys.Keys.CopyTo(keyarray, 0);
                        foreach (string skey in keyarray)
                        {
                            if (dalRecord.dt.Columns.Contains(skey))
                                _reccurrency.CurrencyKeys[skey] = null;
                        }
                        _reccurrency.RecordActionCode = RowStatus.NoRow;
                    }
                }
                else
                {
                    RecordReturnCode = 0;
                    SetRecCurrency(dalRecord, STR_All);
                    if (DataFlag == DBReturnData.Yes)
                        dalRecord.SetRecordData();
                }
                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                {
                    CheckAutostatusExceptions(dbExceptions);
                }
                if (RecordReturnCode != 0)
                {
                    ErrorTable.SetValue(dalRecord.TableName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Data Access Layer GetByKey problem: ", dalRecord.RecordName, " : ", ex.Message));
            }
        }

        /// <summary>
        /// Get a database table row based on ID Column
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="IDCol">The value of the record's key column.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetByIdCol(DalRecordBase dalRecord, long IDCol, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            try
            {
                if (IDCol == 0)
                {
                    RecordReturnCode = 302;
                    return;
                }
                dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
                bool isCurrentUpdateCommand = (dalRecord.DBOperation == DbOperation.Update);
                dalRecord.DBOperation = DbOperation.Select;
                dalRecord.IDColumnValue = IDCol;

                bool found = false;
                if (!dalRecord.RefreshCache && !isCurrentUpdateCommand)
                {
                    if (dalRecord.dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dalRecord.dt.Rows.Count; i++)
                        {
                            if (dalRecord.dt.Columns.IndexOf("ROWTYPE") == 0)
                            {
                                string colValue = (string)dalRecord.dt.Rows[i][0];
                                if (colValue == "next" || colValue == "prev")
                                    break;
                            }

                            if (dalRecord.dt.Rows[i].RowState != DataRowState.Deleted && Convert.ToInt64(dalRecord.dt.Rows[i][dalRecord.IDColumnName]) == IDCol)
                            {
                                dalRecord.DataTableCurrentRow = i;
                                found = true;
                                break;
                            }
                        }
                    }
                }

                if (!found)
                {
                    CheckForConnection();

                    SetUpSqlCommand(dalRecord, "SelectByID");

                    dalRecord.dt = new DataTable();
                    dalRecord.DataTableCurrentRow = 0;
                    for (int i = _command.Parameters.Count - 1; i >= 0; i--)
                    {
                        if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                            _command.Parameters.RemoveAt(i);
                    }
                    try
                    {
                        using (dr = _command.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            dalRecord.dt.Load(dr, LoadOption.OverwriteChanges);
                        }
                        dalRecord.RefreshCache = false;
                    }
                    catch (Exception ex)
                    {
                        if (Transaction != null)
                            Rollback("");

                        throw new ApplicationException("DBRecordBase.GetByIdCol() failed, IDCol: " + IDCol.ToString() + " DataFlags: " + DataFlag.ToString() + ex.Message, ex);
                    }
                }

                if (dalRecord.dt.Rows.Count == 0)
                {
                    RecordReturnCode = 326;
                }
                else
                {
                    RecordReturnCode = 0;

                    // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                    bool canDropCurrencies = _canDropCurrencies;
                    _canDropCurrencies = false;
                    SetRecCurrency(dalRecord, _updateLists ? STR_All : dalRecord.LastListName);
                    _canDropCurrencies = canDropCurrencies;

                    if (DataFlag == DBReturnData.Yes)
                    {
                        dalRecord.SetRecordData();
                    }
                }
                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                    CheckAutostatusExceptions(dbExceptions);
                if (RecordReturnCode != 0)
                {
                    ErrorTable.SetValue(dalRecord.TableName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Data Access Layer GetByID problem: ", dalRecord.RecordName, " : ", ex.Message));
            }
        }

        /// <summary>
        ///  Get a database table row based on ID Column (from IField)
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="IDColField">A reference to the IField object that keeps the value of the record's key column.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetByIdCol(DalRecordBase dalRecord, IField IDColField, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            // SAAQ tickets 5946,6209,6417, 6423, 6456
            string idRecordName = ((IAssignable)IDColField).GetIdRecordName();
            if (idRecordName.Length > 0 && idRecordName != dalRecord.RecordName
                && IDColField.Record != null && IDColField.Record.Name != null
                && !_currentDBCurrency.RecordTable.ContainsKey(IDColField.Record.Name)) // i.e. parent record is just a group and not a database related record
            {
                RecordReturnCode = 326; // record cannot be found
                return;
            }

            GetByIdCol(dalRecord, IDColField.AsInt(), DataFlag, dbExceptions);
        }

        /// <summary>
        /// Get Next Duplicate Key Row 
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetDupByKey(DalRecordBase dalRecord, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
            dalRecord.DBOperation = DbOperation.Select;
            dalRecord.DataTableCurrentRow++;
            if (dalRecord.dt.IsInitialized && dalRecord.dt.Rows.Count == 1)
            {
                long saveDBKey = dalRecord.IDColumnValue;
                //Reload by calc key to Check for duplicate calc keys
                GetByKey(dalRecord, DataFlag, DBLockType.None, dbExceptions);
                if (dalRecord.dt.Rows.Count == 1)
                {
                    dalRecord.DataTableCurrentRow++;
                }
                else
                //Reposition on current DBkey
                {
                    for (int x = 0; x < dalRecord.dt.Rows.Count; x++)
                    {
                        dalRecord.DataTableCurrentRow = x + 1;
                        if (Convert.ToInt64(dalRecord.dt.Rows[x][dalRecord.IDColumnName]) == saveDBKey)
                        {
                            break;
                        }
                    }
                }
            }
            if (dalRecord.dt.IsInitialized && dalRecord.DataTableCurrentRow < dalRecord.dt.Rows.Count)
            {
                RecordReturnCode = 0;

                // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                bool canDropCurrencies = _canDropCurrencies;
                _canDropCurrencies = false;
                SetRecCurrency(dalRecord, STR_All);
                _canDropCurrencies = canDropCurrencies;

                if (DataFlag == DBReturnData.Yes)
                    dalRecord.SetRecordData();
            }
            else
            {
                RecordReturnCode = 326;
            }

            if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
            {
                CheckAutostatusExceptions(dbExceptions);
            }
            if (RecordReturnCode != 0)
            {
                ErrorTable.SetValue(dalRecord.TableName);
            }
        }

        /// <summary>
        /// Get in List
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="gttype">Specifies, which record needs to be retrieved, for example NEXT, PRIOR, LAST, FIRST.</param>
        /// <param name="listname">The name of the set where the DAL record belongs to.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetInList(DalRecordBase dalRecord, RowPosition gttype, string listname, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            try
            {
                RecordReturnCode = 0;
                listname = listname.Replace('-', '_').Trim();
                if (listname != dalRecord.LastListName)
                    dalRecord.dt.Clear();


                dalRecord.LastListName = listname;
                dalRecord.Command = null;
                dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
                dalRecord.CurrentList = _currentDBCurrency.ListTable[listname];

                if (dalRecord.CurrentList == null)
                {
                    //One Advanced loads IDMS data in the reverse order of that which Modern Systems does.
                    //There is an appsetting, AreaSweepReverse, that if true will switch first to last, next to prior, etc...
                    RowPosition currentgttype = gttype;

                    if (_isAreaSweepReverse)
                    {
                        if (gttype == RowPosition.First)
                            currentgttype = RowPosition.Last;
                        else if (gttype == RowPosition.Last)
                            currentgttype = RowPosition.First;
                        else if (gttype == RowPosition.Next)
                            currentgttype = RowPosition.Prior;
                        else if (gttype == RowPosition.Prior)
                            currentgttype = RowPosition.Next;
                    }
                    GetInArea(dalRecord, currentgttype, DataFlag, dbExceptions);
                    return;
                }
                dalRecord.DBOperation = DbOperation.Select;

                if (dalRecord.CurrentList.ListPositionCode == ListStatus.OnNone && _currentDBCurrency.GetListOwnerID(dalRecord.CurrentList.ListName) == 0 && dalRecord.CurrentList.OwnerCur != null)
                {
                    // owner is not specified
                    RecordReturnCode = 306;
                    return;
                }

                if (gttype == RowPosition.First || gttype == RowPosition.Last || dalRecord.CurrentList.ListPositionCode == ListStatus.OnOwnerRow)
                {
                    dalRecord.dt.Clear();
                    dalRecord.IDColumnValue = 0;
                }
                if (gttype == RowPosition.Next)
                {
                    if (dalRecord.CurrentRecord.CurrencyKeys[dalRecord.IDColumnName] != null && dalRecord.IDColumnValue != Convert.ToInt64(dalRecord.CurrentRecord.CurrencyKeys[dalRecord.IDColumnName]))
                    {
                        dalRecord.dt.Clear();
                        dalRecord.IDColumnValue = 0;
                    }

                    if (CheckListStatus(listname) == RowStatus.MissOnUsing)
                    {
                        if (dalRecord.CurrentList.MissOnUsingPrev != 0 || dalRecord.CurrentList.MissOnUsingNext != 0)
                        {
                            if (dalRecord.CurrentList.MissOnUsingNext == 0)
                            {
                                RecordReturnCode = 307;
                                UpdateListStatus(listname, RowStatus.NoRow, ListStatus.OnOwnerRow);
                                return;
                            }
                            else
                            {
                                dalRecord.dt.Clear();
                                dalRecord.IDColumnValue = 0;
                                GetByIdCol(dalRecord, dalRecord.CurrentList.MissOnUsingNext, DataFlag, dbExceptions);
                            }

                            dalRecord.CurrentList.MissOnUsingNext = 0;
                            dalRecord.CurrentList.MissOnUsingPrev = 0;
                        }
                        else
                        {
                            // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                            bool canDropCurrencies = _canDropCurrencies;
                            _canDropCurrencies = false;
                            //Update for issue 5741
                            GetByIdCol(dalRecord, _currentDBCurrency.GetListCurrentID(listname), DataFlag, dbExceptions);
                            _canDropCurrencies = canDropCurrencies;
                        }

                        if (_errorStatusRecord.StatusGood.Value)
                        {
                            return;
                        }
                    }
                    else if (CheckListStatus(listname) == RowStatus.NoRow && CheckListPosition(listname) == ListStatus.OnNone)
                    {
                        if (dalRecord.CurrentList.ListOrd == ListOrder.SORTED || dalRecord.CurrentList.ListFkName == null)
                        {
                            GetInList(RowPosition.First, listname, DataFlag, dbExceptions);
                        }
                        else
                        {
                            RecordReturnCode = 307;
                            //dalRecord.InitializeValues();
                            return;
                        }
                    }
                    else
                    {
                        //dalRecord.CurrentList.DataTableCurrentRow++;
                        dalRecord.DataTableCurrentRow++;

                        //because dt.Rows[#].Delete does not delete the row we need to verify we are not pointing at a deleted row
                        if (dalRecord.DataTableCurrentRow < dalRecord.dt.Rows.Count)
                        {
                            while (dalRecord.dt.Rows[dalRecord.DataTableCurrentRow].RowState == DataRowState.Deleted)
                            {
                                dalRecord.DataTableCurrentRow++;
                                if (dalRecord.DataTableCurrentRow >= dalRecord.dt.Rows.Count)
                                    break;
                            }
                        }
                    }

                }
                else if (gttype == RowPosition.Current)
                {
                    //Check for invalid list currency on Get Current condition
                    if (CheckListStatus(listname) == RowStatus.NoRow || dalRecord.CurrentList.ListPositionCode == ListStatus.OnOwnerRow || dalRecord.CurrentList.ListPositionCode == ListStatus.OnNone || dalRecord.CurrentRecord.RecordActionCode == RowStatus.DeletedRow)
                    {
                        RecordReturnCode = 306;
                        return;
                    }
                }
                else if (gttype == RowPosition.Prior)
                {
                    if (dalRecord.CurrentRecord.CurrencyKeys[dalRecord.IDColumnName] != null && dalRecord.IDColumnValue != Convert.ToInt64(dalRecord.CurrentRecord.CurrencyKeys[dalRecord.IDColumnName]))
                    {
                        dalRecord.dt.Clear();
                        dalRecord.IDColumnValue = 0;
                    }

                    if (CheckListStatus(listname) == RowStatus.MissOnUsing)
                    {
                        if (dalRecord.CurrentList.MissOnUsingPrev != 0 || dalRecord.CurrentList.MissOnUsingNext != 0)
                        {
                            if (dalRecord.CurrentList.MissOnUsingPrev == 0)
                            {
                                RecordReturnCode = 307;
                                UpdateListStatus(listname, RowStatus.NoRow, ListStatus.OnOwnerRow);
                                return;
                            }
                            else
                            {
                                bool canDropCurrencies = _canDropCurrencies;
                                _canDropCurrencies = false;
                                GetByIdCol(dalRecord, dalRecord.CurrentList.MissOnUsingPrev, DataFlag, dbExceptions);
                                _canDropCurrencies = canDropCurrencies;
                            }
                            dalRecord.CurrentList.MissOnUsingNext = 0;
                            dalRecord.CurrentList.MissOnUsingPrev = 0;
                        }
                        else
                        {
                            // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                            // Changed to DBReturnData.No for issue 6120
                            bool canDropCurrencies = _canDropCurrencies;
                            _canDropCurrencies = false;
                            GetByIdCol(dalRecord, _currentDBCurrency.GetListCurrentID(listname), DBReturnData.No, dbExceptions);
                            _canDropCurrencies = canDropCurrencies;
                        }

                        if (_errorStatusRecord.StatusGood.Value)
                        {
                            return;
                        }
                    }
                    else if (CheckListStatus(listname) == RowStatus.NoRow && CheckListPosition(listname) == ListStatus.OnNone)
                    {
                        if (dalRecord.CurrentList.ListOrd == ListOrder.SORTED || dalRecord.CurrentList.ListFkName == null)
                        {
                            GetInList(RowPosition.Last, listname, DataFlag, dbExceptions);
                        }
                        else
                        {
                            RecordReturnCode = 307;
                            return;
                        }
                    }
                    else
                    {
                        //dalRecord.CurrentList.DataTableCurrentRow--;
                        dalRecord.DataTableCurrentRow--;

                        //because dt.Rows[#].Delete does not delete the row we need to verify we are not pointing at a deleted row
                        if (dalRecord.DataTableCurrentRow >= 0 && dalRecord.dt.Rows.Count > dalRecord.DataTableCurrentRow)
                        {
                            while (dalRecord.dt.Rows[dalRecord.DataTableCurrentRow].RowState == DataRowState.Deleted)
                            {
                                dalRecord.DataTableCurrentRow--;
                                if (dalRecord.DataTableCurrentRow < 0)
                                    break;
                            }
                        }
                    }
                }


                if (dalRecord.DataTableCurrentRow < 0 || dalRecord.DataTableCurrentRow >= dalRecord.dt.Rows.Count)
                // Retrieve data from Database
                {
                    SelectListRows(dalRecord, gttype, listname);
                }

                if (_errorStatusRecord.StatusGood.Value)
                {
                    //dalRecord.DataTableCurrentRow = dalRecord.CurrentList.DataTableCurrentRow;
                    bool canDropCurrencies = _canDropCurrencies;
                    _canDropCurrencies = false;
                    SetRecCurrency(dalRecord, _updateLists ? STR_All : listname);
                    _canDropCurrencies = canDropCurrencies;

                    if (DataFlag == DBReturnData.Yes)
                    {
                        dalRecord.SetRecordData();
                    }
                }
                else
                    if (_errorStatusRecord.EndOfList.Value)
                {
                    dalRecord.IDColumnValue = 0;
                    UpdateListStatus(listname, RowStatus.NoRow, ListStatus.OnOwnerRow);
                    dalRecord.CurrentRecord.DropRecordSetCurrencyKeys(_currentDBCurrency.ListTable[listname].ListFkName);
                    //dalRecord.CurrentList.DataTableCurrentRow = 0;
                    dalRecord.DataTableCurrentRow = 0;
                }

                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                    CheckAutostatusExceptions(dbExceptions);
            }
            catch (Exception ex)
            {
                string commandtext = string.Empty;
                if (_command != null && _command.CommandText != null)
                    commandtext = _command.CommandText;
                else
                    commandtext = "Null";
                //RecordReturnCode = 399;
                throw new Exception(string.Concat("Data Access Layer GetInList problem: ", dalRecord.RecordName, " : ", listname, " : ", commandtext, " : ", ex.Message));
            }
            finally
            {
                dalRecord.SetReturnCode(RecordReturnCode);
                if (RecordReturnCode != 0)
                {
                    ErrorList.SetValue(listname);
                    ErrorTable.SetValue(dalRecord.TableName);
                }
            }
        }

        /// <summary>
        /// Get in List
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="gttype">Specifies, which record needs to be retrieved, for example NEXT, PRIOR, LAST, FIRST.</param>
        /// <param name="rowNumber">Start row number.</param>
        /// <param name="listname">The name of the set where the DAL record belongs to.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetInList(DalRecordBase dalRecord, RowPosition gttype, int rowNumber, string listname, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            try
            {
                // Check for Row Number get in list
                if (gttype != RowPosition.RowID || rowNumber == 0)
                {
                    RecordReturnCode = 304;
                    return;
                }

                RecordReturnCode = 0;
                dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
                dalRecord.CurrentList = _currentDBCurrency.ListTable[listname];
                dalRecord.DBOperation = DbOperation.Select;
                dalRecord.StartRow = rowNumber;
                dalRecord.dt = new DataTable();
                dalRecord.DataTableCurrentRow = 0;

                CheckForConnection();

                SetUpSqlCommand(dalRecord, "SelectInListByRowNumber");
                for (int i = _command.Parameters.Count - 1; i >= 0; i--)
                {
                    if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                        _command.Parameters.RemoveAt(i);
                }
                try
                {
                    using (dr = _command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        dalRecord.dt.Load(dr, LoadOption.OverwriteChanges);
                    }
                    dalRecord.RefreshCache = false;
                }
                catch (Exception ex)
                {
                    if (Transaction != null)
                        Rollback("");

                    throw new ApplicationException(String.Format("DBRecordBase.GetInListByRowID() failed, setname: {0} RowNumber: {1} DataFlag: {2}{3}",
                                   listname,
                                   rowNumber,
                                   DataFlag,
                                   ex.Message), ex);
                }

                if (dalRecord.dt.Rows.Count == 0)
                {
                    RecordReturnCode = 307;
                }
                else
                {
                    RecordReturnCode = 0;

                    // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                    bool canDropCurrencies = _canDropCurrencies;
                    _canDropCurrencies = false;
                    SetRecCurrency(dalRecord, STR_All);
                    _canDropCurrencies = canDropCurrencies;

                    if (DataFlag == DBReturnData.Yes)
                    {
                        dalRecord.SetRecordData();
                    }
                }

                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                    CheckAutostatusExceptions(dbExceptions);
            }
            catch (Exception ex)
            {
                string commandtext = string.Empty;
                if (_command.CommandText != null)
                    commandtext = _command.CommandText;
                RecordReturnCode = 399;
                throw new Exception(string.Concat("Data Access Layer GetInList problem: ", dalRecord.RecordName, " : ", listname, " : ", commandtext, " : ", ex.Message));
            }
            finally
            {
                dalRecord.SetReturnCode(RecordReturnCode);
                if (RecordReturnCode != 0)
                {
                    ErrorList.SetValue(listname);
                    ErrorTable.SetValue(dalRecord.TableName);
                }
            }
        }

        /// <summary>
        /// Get in List with out Record name - For Multi Member Sets
        /// </summary>
        /// <param name="gttype">Specifies, which record needs to be retrieved, for example NEXT, PRIOR, LAST, FIRST.</param>
        /// <param name="listname">The name of the set where the DAL record belongs to.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetInList(RowPosition gttype, string listname, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            listname = listname.Replace('-', '_').Trim();
            CheckForConnection();
            _command = _connection.CreateCommand();
            _command.Transaction = Transaction;

            ListCurrency listInfo = _currentDBCurrency.ListTable[listname];
            if (listInfo.MemberList == null || listInfo.MemberList.Count == 0)
            {
                if (CurrentDalRecords.ContainsKey(listInfo.MemberCur.RecordName))
                {
                    GetInList(CurrentDalRecords[listInfo.MemberCur.RecordName], gttype, listname, DataFlag, dbExceptions);
                }
                else
                {
                    DalRecordBase dalRec = GetInstanceOfDalRecord(listInfo.MemberCur.RecordTypeName);
                    if (dalRec != null)
                    {
                        CurrentDalRecords.Add(listInfo.MemberCur.RecordName, dalRec);
                        GetInList(CurrentDalRecords[listInfo.MemberCur.RecordName], gttype, listname, DataFlag, dbExceptions);
                    }
                    else
                        throw new Exception(string.Concat("Invalid Record Name for Get In List: ", listInfo.MemberCur.RecordName));
                }

            }
            else
            // Check for Multi Member Set
            {

                _sqlCommandUtility.CurrentDbCurrency = _currentDBCurrency;
                _sqlCommandUtility.SetUpCommandFindMemberInMultiMemberList(listInfo, gttype, _command);
                DalRecordBase tempDalRec = null;
                //for (int i = _command.Parameters.Count - 1; i >= 0; i--)
                //{
                //    if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                //        _command.Parameters.RemoveAt(i);
                //}
                try
                {
                    //   string recordType = (string)_command.ExecuteScalar();
                    DataTable dtMultiMemberList = new DataTable();
                    using (dr = _command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        dtMultiMemberList.Load(dr, LoadOption.OverwriteChanges);
                    }
                    if (dtMultiMemberList == null || dtMultiMemberList.Rows.Count == 0)
                        SetReturnCode(307);
                    else
                    {
                        string mmTableName = ((string)dtMultiMemberList.Rows[0][listInfo.MultiMemberTypeKey]).Trim();
                        if (!CurrentDalRecords.ContainsKey(mmTableName))
                        {
                            DalRecordBase newdalRec = GetInstanceOfDalRecord(mmTableName);
                            if (newdalRec != null)
                            {
                                CurrentDalRecords.Add(mmTableName, newdalRec);
                            }
                        }
                        foreach (DalRecordBase dalRecBase in CurrentDalRecords.Values)
                        {
                            if (dalRecBase.TableName == mmTableName)
                            {
                                tempDalRec = dalRecBase;
                                break;
                            }
                        }

                        if (tempDalRec != null)
                        {
                            GetByOtherKey(tempDalRec, listInfo.ListFkName, Convert.ToInt64(dtMultiMemberList.Rows[0][listInfo.ListFkName]),
                                listInfo.ListSequenceObject, (DateTime)dtMultiMemberList.Rows[0][listInfo.ListSequenceObject], DataFlag, dbExceptions);
                        }
                        else
                        {
                            // commented out because of ticket 6014
                            //throw new ApplicationException("Get In Mult Member List failed, ListName: " + listname);
                        }
                        _currentList.SetValue(listname);
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Get In Mult Member List failed, ListName: " + listname + ex.Message, ex);
                }
                finally
                {
                    if (RecordReturnCode != 0)
                    {
                        ErrorList.SetValue(listname);
                    }
                }
            }
        }

        /// <summary>
        /// Get In Multi Member List
        /// </summary>
        /// <param name="setRecords">Collection of DAL records, which participate in the multi-record set as member records. 
        /// One of the records will be updated and set as current runtime record.</param>
        /// <param name="gttype">Specifies, which record needs to be retrieved, for example NEXT, PRIOR, LAST, FIRST.</param>
        /// <param name="listname">The name of the set where the DAL record belongs to.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetInList(List<DalRecordBase> setRecords, RowPosition gttype, string listname, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            listname = listname.Replace('-', '_').Trim();
            CheckForConnection();
            _command = _connection.CreateCommand();
            _command.Transaction = Transaction;

            ListCurrency listInfo = _currentDBCurrency.ListTable[listname];

            _sqlCommandUtility.CurrentDbCurrency = _currentDBCurrency;
            _sqlCommandUtility.SetUpCommandFindMemberInMultiMemberList(listInfo, gttype, _command);
            DalRecordBase tempDalRec = null;
            for (int i = _command.Parameters.Count - 1; i >= 0; i--)
            {
                if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                    _command.Parameters.RemoveAt(i);
            }
            try
            {
                DataTable dtMultiMemberList = new DataTable();
                using (dr = _command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    dtMultiMemberList.Load(dr, LoadOption.OverwriteChanges);
                }
                if (dtMultiMemberList == null || dtMultiMemberList.Rows.Count == 0)
                {
                    SetReturnCode(307);
                    UpdateListStatus(listname, RowStatus.NoRow, ListStatus.OnOwnerRow);
                }
                else
                {
                    foreach (DalRecordBase dalRecBase in setRecords)
                    {
                        if (dalRecBase.TableName == ((string)dtMultiMemberList.Rows[0][0]).Trim())
                        {
                            tempDalRec = dalRecBase;
                            break;
                        }
                    }
                    if (tempDalRec != null)
                    {
                        GetByOtherKey(tempDalRec, listInfo.ListFkName, Convert.ToInt64(dtMultiMemberList.Rows[0][listInfo.ListFkName]),
                                listInfo.ListSequenceObject, (DateTime)dtMultiMemberList.Rows[0][listInfo.ListSequenceObject], DataFlag, dbExceptions);
                    }
                    else
                    {
                        throw new ApplicationException("Get In Mult Member List failed, ListName: " + listname);
                    }
                    _currentList.SetValue(listname);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Get In Mult Member List failed, ListName: " + listname + ex.Message, ex);
            }
            finally
            {
                if (RecordReturnCode != 0)
                {
                    ErrorList.SetValue(listname);
                }
            }
        }

        /// <summary>
        /// Get a database table row based on ID Column
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="keyName">Name of the column that corresponds to the key value.</param>
        /// <param name="keyValue">Value of the key.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        private void GetByOtherKey(DalRecordBase dalRecord, string keyName, long keyValue, string seqKeyName, DateTime seqKeyValue, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            try
            {

                dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
                dalRecord.DBOperation = DbOperation.Select;
                CheckForConnection();

                dalRecord.Command = _command; dalRecord.ParmPrefix = _parmPrefix;
                _commandRecordName = dalRecord.RecordName;

                _sqlCommandUtility.SetUpSelectForMultiMemberDetailRecords(dalRecord, keyName, keyValue, seqKeyName, seqKeyValue);

                dalRecord.dt = new DataTable();
                dalRecord.DataTableCurrentRow = 0;
                for (int i = _command.Parameters.Count - 1; i >= 0; i--)
                {
                    if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                        _command.Parameters.RemoveAt(i);
                }
                try
                {
                    using (dr = _command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        dalRecord.dt.Load(dr, LoadOption.OverwriteChanges);
                    }
                    dalRecord.RefreshCache = false;
                }
                catch (Exception ex)
                {
                    if (Transaction != null)
                        Rollback("");

                    throw new ApplicationException("DBRecordBase.GetByOtherKey() failed, " + keyName + " = " + keyValue.ToString() + " DataFlags: " + DataFlag.ToString() + ex.Message, ex);
                }

                if (dalRecord.dt.Rows.Count == 0)
                {
                    RecordReturnCode = 326;
                }
                else
                {
                    RecordReturnCode = 0;

                    // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                    bool canDropCurrencies = _canDropCurrencies;
                    _canDropCurrencies = false;
                    SetRecCurrency(dalRecord, _updateLists ? STR_All : dalRecord.LastListName);
                    _canDropCurrencies = canDropCurrencies;

                    if (DataFlag == DBReturnData.Yes)
                    {
                        dalRecord.SetRecordData();
                    }
                }
                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                    CheckAutostatusExceptions(dbExceptions);
                if (RecordReturnCode != 0)
                {
                    ErrorTable.SetValue(dalRecord.RecordName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Data Access Layer GetOtherKey problem: ", dalRecord.RecordName, " : ", ex.Message));
            }
        }

        /// <summary>
        /// Get in List by Key
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="listname">The name of the set where the DAL record belongs to.</param>
        /// <param name="keyField">Name of the search key column.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetInListByKey(DalRecordBase dalRecord, string listname, string keyField, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            try
            {
                _inGetInListByKey = true;
                bool isKeyMatch = false;
                dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
                dalRecord.CurrentList = _currentDBCurrency.ListTable[listname];
                if (dalRecord.CurrentList.OwnerCur != null && GetCurrentOwnerID(dalRecord.CurrentList.OwnerCur.RecordName, dalRecord.CurrentList.OwnerCur) == 0)
                {
                    RecordReturnCode = 306;
                    return;
                }

                dalRecord.DBOperation = DbOperation.Select;
                dalRecord.dt = new DataTable();
                //dalRecord.CurrentList.DataTableCurrentRow = 0;
                dalRecord.DataTableCurrentRow = 0;
                dalRecord.SearchKey = keyField;

                CheckForConnection();

                SetUpSqlCommand(dalRecord, "SelectInListUsing");

                try
                {
                    // Check for Exact match
                    _command.CommandText = dalRecord.MultipleKeysSqlList[0];
                    for (int i = _command.Parameters.Count - 1; i >= 0; i--)
                    {
                        if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                            _command.Parameters.RemoveAt(i);
                    }
                    using (dr = _command.ExecuteReader(CommandBehavior.Default))
                    {
                        dalRecord.dt.Load(dr);
                    }
                    dalRecord.RefreshCache = false;

                    //On not exact match, find next matching row
                    if (dalRecord.dt.Rows.Count == 0)
                    {
                        for (int ctr = 1; ctr < dalRecord.MultipleKeysSqlList.Count; ctr++)
                        {
                            if (ctr > 1)
                            {
                                _command.Parameters.RemoveAt(_command.Parameters.Count - 1);
                            }
                            _command.CommandText = dalRecord.MultipleKeysSqlList[ctr];
                            for (int i = _command.Parameters.Count - 1; i >= 0; i--)
                            {
                                if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                                    _command.Parameters.RemoveAt(i);
                            }
                            using (dr = _command.ExecuteReader(CommandBehavior.Default))
                            {
                                dalRecord.dt.Load(dr);
                            }
                            dalRecord.RefreshCache = false;

                            if (dalRecord.dt.Rows.Count > 1)
                                break;
                        }
                    }
                    else
                    {
                        isKeyMatch = true;
                    }

                }
                catch (Exception ex)
                {
                    if (Transaction != null)
                        Rollback("");
                    RecordReturnCode = 399;
                    throw new ApplicationException(String.Format("DBRecordBase.GetInListByKey() failed, setname: {0} Keystring: {1} DataFlag: {2}{3}",
                                                       listname,
                                                       keyField,
                                                       DataFlag,
                                                       ex.Message), ex);
                }
                if (dalRecord.dt.Rows.Count == 0)
                {
                    RecordReturnCode = 326;
                    UpdateListStatus(listname, RowStatus.NoRow, ListStatus.OnNone);
                }
                else
                {

                    // Check for keymatch or next positioning
                    if (isKeyMatch)
                    {
                        // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                        bool canDropCurrencies = _canDropCurrencies;
                        _canDropCurrencies = false;
                        SetRecCurrency(dalRecord, STR_All);
                        _canDropCurrencies = canDropCurrencies;

                        RecordReturnCode = 0;
                        if (DataFlag == DBReturnData.Yes)
                        {
                            dalRecord.SetRecordData();
                        }
                        // Get datatable list starting with current ID
                        SelectListRows(dalRecord, RowPosition.Current, listname);
                    }
                    // IF not a search key match, set next currency
                    else
                    {
                        if (dalRecord.dt.Rows.Count > 1)
                        {
                            UpdateListStatus(listname, RowStatus.MissOnUsing, ListStatus.OnMemberRow);
                            //Update for handling next and prev - issues 8113, 8747, 8807
                            dalRecord.CurrentList.MissOnUsingPrev = 0;
                            dalRecord.CurrentList.MissOnUsingNext = 0;
                            if (((string)dalRecord.dt.Rows[0]["rowType"]) == "prev")
                            {
                                dalRecord.CurrentList.MissOnUsingPrev = Convert.ToInt64(dalRecord.dt.Rows[0][dalRecord.IDColumnName]);
                                if (((string)dalRecord.dt.Rows[1]["rowType"]) == "next")
                                    dalRecord.CurrentList.MissOnUsingNext = Convert.ToInt64(dalRecord.dt.Rows[1][dalRecord.IDColumnName]);
                                else if (dalRecord.dt.Rows.Count > 2 && ((string)dalRecord.dt.Rows[2]["rowType"]) == "next")
                                    dalRecord.CurrentList.MissOnUsingNext = Convert.ToInt64(dalRecord.dt.Rows[2][dalRecord.IDColumnName]);
                            }
                            else if (((string)dalRecord.dt.Rows[0]["rowType"]) == "next")
                            {
                                dalRecord.CurrentList.MissOnUsingNext = Convert.ToInt64(dalRecord.dt.Rows[0][dalRecord.IDColumnName]);
                                if (((string)dalRecord.dt.Rows[1]["rowType"]) == "prev")
                                    dalRecord.CurrentList.MissOnUsingPrev = Convert.ToInt64(dalRecord.dt.Rows[1][dalRecord.IDColumnName]);
                                else if (dalRecord.dt.Rows.Count > 2 && ((string)dalRecord.dt.Rows[2]["rowType"]) == "prev")
                                    dalRecord.CurrentList.MissOnUsingPrev = Convert.ToInt64(dalRecord.dt.Rows[2][dalRecord.IDColumnName]);
                            }

                            UpdateRecStatus(dalRecord.RecordName, RowStatus.NoRow);
                        }
                        else
                        {
                            if (((string)dalRecord.dt.Rows[0]["rowType"]) == "prev")
                            {
                                UpdateListStatus(listname, RowStatus.MissOnUsing, ListStatus.OnMemberRow);
                                dalRecord.CurrentList.MissOnUsingPrev = Convert.ToInt64(dalRecord.dt.Rows[0][dalRecord.IDColumnName]);
                                dalRecord.CurrentList.MissOnUsingNext = 0;
                                UpdateRecStatus(dalRecord.RecordName, RowStatus.NoRow);
                            }
                            else if (((string)dalRecord.dt.Rows[0]["rowType"]) == "next")
                            {
                                UpdateListStatus(listname, RowStatus.MissOnUsing, ListStatus.OnMemberRow);
                                dalRecord.CurrentList.MissOnUsingPrev = 0;
                                dalRecord.CurrentList.MissOnUsingNext = Convert.ToInt64(dalRecord.dt.Rows[0][dalRecord.IDColumnName]);
                                UpdateRecStatus(dalRecord.RecordName, RowStatus.NoRow);
                            }
                            else
                            {
                                SetListCurrency(dalRecord, listname, STR_Member);
                                UpdateListStatus(listname, RowStatus.MissOnUsing, ListStatus.OnMemberRow);
                                UpdateRecStatus(dalRecord.RecordName, RowStatus.NoRow);
                                // Get datatable list starting with current ID
                                SelectListRows(dalRecord, RowPosition.Current, listname);
                            }
                        }
                        RecordReturnCode = 326;
                    }

                }
                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                    CheckAutostatusExceptions(dbExceptions);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Data Access Layer GetInListByKey problem: ", dalRecord.RecordName, " : ", listname, " : ", ex.Message));
            }
            finally
            {
                _inGetInListByKey = false;
                dalRecord.SetReturnCode(RecordReturnCode);
                if (RecordReturnCode != 0)
                {
                    ErrorList.SetValue(listname);
                    ErrorTable.SetValue(dalRecord.TableName);
                }
            }
        }

        /// <summary>
        /// Get table row from list using sort key (IField)
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="listname">The name of the set where the DAL record belongs to.</param>
        /// <param name="keyField">Name of the search key column.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetInListByKey(DalRecordBase dalRecord, string listname, IField keyField, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            dalRecord.UsingOneFieldOnly = true;
            _inGetInListByKey = true;
            try
            {
                string keyValue = keyField.AsString();
                //Check for Key length - if keyfield shorter than key length, get other bytes in keyfield buffer for the key length - issue 8610
                if (dalRecord.ListKeyLengths.ContainsKey(listname) && dalRecord.ListKeyLengths[listname] > keyField.LengthInBuffer)
                {
                    keyValue = keyField.Record.GetSubstring(keyField.PositionInBuffer + 1, dalRecord.ListKeyLengths[listname]).TrimEnd('\0');
                }

                GetInListByKey(dalRecord, listname, keyValue, DataFlag, dbExceptions);
            }
            finally
            {
                _inGetInListByKey = false;
            }
        }

        /// <summary>
        /// Get table row from list using sort key (IGroup)
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="listname">The name of the set where the DAL record belongs to.</param>
        /// <param name="keyField">Name of the search key column.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetInListByKey(DalRecordBase dalRecord, string listname, IGroup keyField, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            dalRecord.UsingOneFieldOnly = false;
            GetInListByKey(dalRecord, listname, keyField.AsString(), DataFlag, dbExceptions);
        }
        /// <summary>
        /// Get table row from list using sort key (predefinedRecord)
        /// </summary>
        /// <param name="dalRecord"></param>
        /// <param name="listname"></param>
        /// <param name="keyField"></param>
        /// <param name="DataFlag"></param>
        /// <param name="dbExceptions"></param>
        public void GetInListByKey(DalRecordBase dalRecord, string listname, PredefinedRecordBase keyField, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            dalRecord.UsingOneFieldOnly = false;
            GetInListByKey(dalRecord, listname, keyField.AsString(), DataFlag, dbExceptions);
        }

        /// <summary>
        /// Get table row from list using sort key  
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="listname">The name of the set where the DAL record belongs to.</param>
        /// <param name="keyField">Name of the search key column.</param>
        /// <param name="searchType">RowPosition type is Current specified.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetInListByKey(DalRecordBase dalRecord, string listname, IBufferValue keyField, RowPosition searchType, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            if (searchType == RowPosition.Current)
                dalRecord.SelectUsingPosition = searchType;
            else
                dalRecord.SelectUsingPosition = RowPosition.All;

            if (keyField is IGroup)
            {
                GetInListByKey(dalRecord, listname, (IGroup)keyField, DataFlag, dbExceptions);
            }
            else
            {
                GetInListByKey(dalRecord, listname, (IField)keyField, DataFlag, dbExceptions);
            }

            dalRecord.SelectUsingPosition = RowPosition.All;
        }


        /// <summary>
        /// Get Latest Row
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetLatest(DalRecordBase dalRecord, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            //*** Moynahan  GetLatest is just like GetDupeKey, but don't increment dalRecord.DataTableCurrentRow

            dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
            dalRecord.DBOperation = DbOperation.Select;
            //dalRecord.DataTableCurrentRow++;
            if (dalRecord.dt.IsInitialized && dalRecord.DataTableCurrentRow < dalRecord.dt.Rows.Count && dalRecord.dt.Rows.Count > 1)
            {
                RecordReturnCode = 0;

                // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                bool canDropCurrencies = _canDropCurrencies;
                _canDropCurrencies = false;
                SetRecCurrency(dalRecord, STR_All);
                _canDropCurrencies = canDropCurrencies;

                if (DataFlag == DBReturnData.Yes)
                    dalRecord.SetRecordData();
            }
            else
            {
                // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                bool canDropCurrencies = _canDropCurrencies;
                _canDropCurrencies = false;
                _getLatest = true;
                GetByIdCol(dalRecord, _currentDBCurrency.GetTableCurrentID(dalRecord.RecordName), DataFlag, dbExceptions);
                _canDropCurrencies = canDropCurrencies;
                _getLatest = false;

                if (dalRecord.dt.Rows.Count == 0 || RecordReturnCode == 302)
                    RecordReturnCode = 306;
            }

            if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
            {
                CheckAutostatusExceptions(dbExceptions);
            }
            if (RecordReturnCode != 0)
            {
                ErrorTable.SetValue(dalRecord.TableName);
            }
        }

        /// <summary>
        /// Get Owner Table row
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="listname">The name of the set where the DAL record belongs to.</param>
        /// <param name="DataFlag">A flag that specifies whether the record should be populated with the retrieved data from the database.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void GetOwner(DalRecordBase dalRecord, string listname, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            listname = listname.Replace('-', '_').Trim();
            RecordReturnCode = 0;
            try
            {
                //if (CheckListStatus(listname) != RowStatus.GoodRow)
                if (CheckListPosition(listname) == ListStatus.OnNone) // ticket 8621
                {
                    RecordReturnCode = 306;
                    return;
                }
                // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                bool canDropCurrencies = _canDropCurrencies;
                _canDropCurrencies = false;

                GetByIdCol(dalRecord, GetListOwnerID(_currentDBCurrency.ListTable[listname], true), DataFlag, dbExceptions);

                _canDropCurrencies = canDropCurrencies;
            }
            finally
            {
                dalRecord.SetReturnCode(RecordReturnCode);
                if (RecordReturnCode != 0)
                {
                    ErrorList.SetValue(listname);
                    ErrorTable.SetValue(dalRecord.TableName);
                }
            }
        }

        /// <summary>
        /// Get Index Key
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="listname">The name of the set where the DAL record belongs to.</param>
        /// <param name="gttype">Specifies, which record needs to be retrieved, for example NEXT, PRIOR, LAST, FIRST.</param>
        /// <param name="UsingKey">Name of the key.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        /// <returns>Returns the value of the index key.</returns>
        public long GetIndexKey(DalRecordBase dalRecord, string listname, RowPosition gttype, IBufferValue UsingKey, params DbAllow[] dbExceptions)
        {
            return GetIndexKey(dalRecord, listname, gttype, UsingKey.BytesAsString, dbExceptions);
        }
        public long GetIndexKey(DalRecordBase dalRecord, string listname, RowPosition gttype, string UsingKey, params DbAllow[] dbExceptions)
        {

            if (dalRecord == null)
            {
                ListCurrency lcurrency = _currentDBCurrency.ListTable[listname];
                if (CurrentDalRecords.ContainsKey(lcurrency.MemberCur.RecordName))
                {
                    dalRecord = CurrentDalRecords[lcurrency.MemberCur.RecordName];
                }
                else
                {
                    DalRecordBase dalRec = GetInstanceOfDalRecord(lcurrency.MemberCur.RecordTypeName);
                    if (dalRec != null)
                    {
                        CurrentDalRecords.Add(lcurrency.MemberCur.RecordName, dalRec);
                        dalRecord = dalRec;
                    }
                }
            }

            if (dalRecord.CurrentList != null && !string.IsNullOrEmpty(dalRecord.CurrentList.ListName) && dalRecord.CurrentList.ListName != listname)
                dalRecord.dt.Clear();
            dalRecord.CurrentList = _currentDBCurrency.ListTable[listname];
            dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];

            listname = listname.Replace('-', '_').Trim();

            if (gttype != RowPosition.Using)
            {
                if (CheckListStatus(listname) == RowStatus.MissOnUsing)
                {
                    long nextPrevIDCol = 0;
                    if (gttype == RowPosition.Next)
                    {
                        if (dalRecord.dt.Rows.Count > 0 && (string)dalRecord.dt.Rows[0][0] == "next")
                            nextPrevIDCol = (long)dalRecord.dt.Rows[0][1];
                        else if (dalRecord.dt.Rows.Count == 1 && (string)dalRecord.dt.Rows[0][0] == "prev")
                        {
                            dalRecord.dt.Clear();
                            dalRecord.IDColumnValue = 0;
                            SelectListRows(dalRecord, RowPosition.Next, listname);
                            if (dalRecord.dt.Rows.Count > 0)
                                nextPrevIDCol = Convert.ToInt64(dalRecord.dt.Rows[0][dalRecord.IDColumnName]);
                        }
                    }
                    else if (gttype == RowPosition.Prior)
                    {
                        if (dalRecord.dt.Rows.Count > 0 && (string)dalRecord.dt.Rows[0][0] == "prev")
                            nextPrevIDCol = (long)dalRecord.dt.Rows[0][1];
                        else if (dalRecord.dt.Rows.Count > 1 && (string)dalRecord.dt.Rows[1][0] == "prev")
                            nextPrevIDCol = (long)dalRecord.dt.Rows[1][1];
                        else if (dalRecord.dt.Rows.Count > 2 && (string)dalRecord.dt.Rows[2][0] == "prev")
                            nextPrevIDCol = (long)dalRecord.dt.Rows[2][1];
                    }
                    if (gttype == RowPosition.Last || gttype == RowPosition.First)
                    {
                        dalRecord.dt.Clear();
                        dalRecord.IDColumnValue = 0;
                        SelectListRows(dalRecord, gttype, listname);
                    }
                    else
                    {
                        if (nextPrevIDCol == 0)
                            nextPrevIDCol = _currentDBCurrency.GetListCurrentID(listname);
                        //bool canDropCurrencies = _canDropCurrencies;
                        //_canDropCurrencies = false;
                        GetByIdCol(dalRecord, nextPrevIDCol, DBReturnData.No, dbExceptions);
                        //_canDropCurrencies = canDropCurrencies;
                    }
                }
                else if ((CheckListStatus(listname) == RowStatus.NoRow && CheckListPosition(listname) == ListStatus.OnNone) || dalRecord.dt.Columns[0].DataType == typeof(String))
                    SelectListRows(dalRecord, gttype, listname);
                else if (gttype == RowPosition.Next)
                    dalRecord.DataTableCurrentRow++;
                else if (gttype == RowPosition.Prior)
                    dalRecord.DataTableCurrentRow++;

                if (dalRecord.DataTableCurrentRow < 0 || dalRecord.DataTableCurrentRow >= dalRecord.dt.Rows.Count)
                // Retrieve data from Database
                {
                    SelectListRows(dalRecord, gttype, listname);
                }
                else
                {
                    RecordReturnCode = 0;
                }

                if (_errorStatusRecord.StatusGood.Value)
                {
                    UpdateListStatus(listname, RowStatus.GoodRow, ListStatus.OnMemberRow);
                    SetListCurrency(dalRecord, listname, STR_Member);
                    dalRecord.GetKeyParameters(listname);
                    return _currentDBCurrency.GetListCurrentID(listname);
                }
                else if (_errorStatusRecord.EndOfList.Value)
                {
                    SetReturnCode(1707);
                    UpdateListStatus(listname, RowStatus.NoRow, ListStatus.OnNone);
                }


                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                    CheckAutostatusExceptions(dbExceptions);


                return _currentDBCurrency.GetListCurrentID(listname);
            }
            else
            {
                //For (gttype == RowPosition.Using)
                dalRecord.UsingOneFieldOnly = false;
                GetInListByKey(dalRecord, listname, UsingKey, DBReturnData.No, dbExceptions);
                if (CheckListStatus(listname) == RowStatus.GoodRow)
                    return _currentDBCurrency.GetListCurrentID(listname);
            }
            RecordReturnCode = 1726;
            return 0;
        }

        /// <summary>
        /// Save Key
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="rptype">Takes the value Current or Owner. The method returns 0 for any other RowPosition value.</param>
        /// <param name="listname">An optional parameter (i.e. can be null), which specifies the name of the set.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        /// <returns>Returns currency key value of the current instance of the specified record type.</returns>
        public long SaveKey(DalRecordBase dalRecord, RowPosition rptype, string listname, params DbAllow[] dbExceptions)
        {
            RecordReturnCode = 0;

            dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
            if (listname != null)
            {
                listname = listname.Replace('-', '_').Trim();
                dalRecord.CurrentList = _currentDBCurrency.ListTable[listname];
            }

            if (rptype == RowPosition.Current)
            {
                return _currentDBCurrency.GetTableCurrentID(dalRecord.RecordName);
            }
            else if (rptype == RowPosition.Owner)
            {
                return _currentDBCurrency.GetListOwnerID(listname);
            }
            else
            {
                // Handle next & Prior in List
                return 0;
            }
        }

        /// <summary>
        /// Return table row ID from Table currency
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="rptype">Takes the value Current. The method returns 0 for any other RowPosition value.</param>
        /// <returns>Returns currency key value of the current instance of the specified record type.</returns>
        public long SaveKey(DalRecordBase dalRecord, RowPosition rptype)
        {
            RecordReturnCode = 0;
            dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];

            long acceptKey = rptype == RowPosition.Current ?
                       _currentDBCurrency.GetTableCurrentID(dalRecord.RecordName) :
                       0;
            if (acceptKey == 0)
                acceptKey = -1;

            FieldEx.IdRecordName = "";

            return acceptKey;
        }

        /// <summary>
        /// Return table row ID from List currency
        /// </summary>
        /// <param name="listName">Specifies the name of the set.</param>
        /// <param name="rptype">Optional parameter. Takes the value Current, Owner, Next, Prior. Default value is Current. The method returns 0 for any other RowPosition value.</param>
        /// <returns>Returns currency key value of the current instance of the specified record type.</returns>
        public long SaveKey(string listName, RowPosition rptype = RowPosition.Current)
        {
            listName = listName.Replace('-', '_').Trim();
            RecordReturnCode = 0;
            SaveKeyListStatus = _currentDBCurrency.ListTable[listName].ListPositionCode;
            if (rptype == RowPosition.Owner)
            {
                SaveKeyListStatus = ListStatus.OnOwnerRow;
                MDSY.Framework.Buffer.FieldEx.IdRecordName = _currentDBCurrency.ListTable[listName.AsString().Replace('-', '_').Trim()].OwnerCur.RecordName;
                return _currentDBCurrency.GetListOwnerID(listName, true);
            }
            else if (rptype == RowPosition.Current)
            {
                if (_currentDBCurrency.ListTable[listName].ListPositionCode == ListStatus.OnOwnerRow)
                {
                    MDSY.Framework.Buffer.FieldEx.IdRecordName = _currentDBCurrency.ListTable[listName.AsString().Replace('-', '_').Trim()].OwnerCur.RecordName;
                    return _currentDBCurrency.GetListOwnerID(listName, true);
                }
                else if (_currentDBCurrency.ListTable[listName].ListPositionCode == ListStatus.OnMemberRow)
                {
                    MDSY.Framework.Buffer.FieldEx.IdRecordName = _currentDBCurrency.ListTable[listName.AsString().Replace('-', '_').Trim()].MemberCur.RecordName;
                    return _currentDBCurrency.GetListCurrentID(listName);
                }
                else
                    return 0;
            }
            else if (rptype == RowPosition.Next)
            {
                return GetNextKeyInList(listName);
            }
            else if (rptype == RowPosition.Prior)
            {
                return GetPriorKeyInList(listName);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Return Table row id from List currency
        /// </summary>
        /// <param name="listField">Specifies the name of the set.</param>
        /// <param name="rptype">Optional parameter. Takes the value Current, Owner, Next, Prior. Default value is Current. The method returns 0 for any other RowPosition value.</param>
        /// <returns>Returns currency key value of the current instance of the specified record type.</returns>
        public long SaveKey(IField listField, RowPosition rptype = RowPosition.Current)
        {
            return SaveKey(listField.AsString(), rptype);
        }

        /// <summary>
        /// Return Table row Id from Run unit currency
        /// </summary>
        /// <param name="rptype">Optional parameter. Takes the value Current, Owner, Next, Prior. Default value is Current. The method returns 0 for any other RowPosition value.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        /// <returns>Returns currency key value of the current instance of the specified record type.</returns>
        public long SaveKey(RowPosition rptype, params DbAllow[] dbExceptions)
        {
            //Update for concurrency testing - issue 4921
            return _currentDBCurrency.CurrentIdCol;
        }
        #endregion

        #region Update Command Methods
        /// <summary>
        /// Exclude table row from list
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="listname">The name of the set.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void ExcludeFromList(DalRecordBase dalRecord, string listname, params DbAllow[] dbExceptions)
        {
            dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
            dalRecord.CurrentList = _currentDBCurrency.ListTable[listname];
            dalRecord.ListUpdateType = "Exclude";
            dalRecord.DBOperation = DbOperation.Update;
            if (CheckRecStatus(dalRecord.RecordName) != RowStatus.GoodRow)
            {
                RecordReturnCode = 1106;
                return;
            }
            CheckForConnection();
            SetTransaction();

            SetUpSqlCommand(dalRecord, "UpdateList");

            try
            {
                _returnCode = _command.ExecuteNonQuery();

                if (_returnCode > 0)
                {
                    RecordReturnCode = 0;
                    if (dalRecord.CurrentList.MemberList == null || dalRecord.CurrentList.MemberList.Count == 0)
                    {
                        UpdateCurrentConversation(dalRecord.RecordName, _currentDBCurrency.GetTableCurrentID(dalRecord.RecordName));
                        UpdateListStatus(listname, RowStatus.DeletedRow, ListStatus.OnNone);

                        if (dalRecord.dt.Rows.Count > 0)
                        {
                            if (string.IsNullOrEmpty(dalRecord.CurrentList.ListFkName))
                            {
                                if (!string.IsNullOrEmpty(dalRecord.CurrentList.ListSequenceObject))
                                {
                                    if (dalRecord.dt.Columns[dalRecord.CurrentList.ListSequenceObject].ReadOnly)
                                    {
                                        SimpleLogging.LogMandatoryMessageToFile("ExcludeFromList 1 dt.column " + dalRecord.dt.Columns[dalRecord.CurrentList.ListSequenceObject] + " is set to read only.");
                                        dalRecord.dt.Columns[dalRecord.CurrentList.ListSequenceObject].ReadOnly = false;
                                    }
                                    dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][dalRecord.CurrentList.ListSequenceObject] = DBNull.Value;
                                }
                                else if (dalRecord.dt.Columns.Contains(string.Concat(listname, "_MEMIND")))
                                {
                                    if (dalRecord.dt.Columns[string.Concat(listname, "_MEMIND")].ReadOnly)
                                    {
                                        SimpleLogging.LogMandatoryMessageToFile("ExcludeFromList 2 dt.column " + dalRecord.dt.Columns[string.Concat(listname, "_MEMIND")] + " is set to read only.");
                                        dalRecord.dt.Columns[string.Concat(listname, "_MEMIND")].ReadOnly = false;
                                    }
                                    dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][string.Concat(listname, "_MEMIND")] = DBNull.Value;
                                }
                                else
                                {
                                    if (dalRecord.dt.Columns[string.Concat(listname, "_MEMIND")].ReadOnly)
                                    {
                                        SimpleLogging.LogMandatoryMessageToFile("ExcludeFromList 3 dt.column " + dalRecord.dt.Columns[string.Concat(listname, "_MEMIND")] + " is set to read only.");
                                        dalRecord.dt.Columns[string.Concat(listname, "_MEMIND")].ReadOnly = false;
                                    }
                                    dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][string.Concat(listname, "_MEMIND")] = DBNull.Value;
                                }
                            }
                            else
                            {
                                if (dalRecord.dt.Columns[dalRecord.CurrentList.ListFkName].ReadOnly)
                                    dalRecord.dt.Columns[dalRecord.CurrentList.ListFkName].ReadOnly = false;

                                dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][dalRecord.CurrentList.ListFkName] = DBNull.Value;
                            }
                        }
                    }
                }
                else
                {
                    RecordReturnCode = 1106;
                }
                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                {
                    CheckAutostatusExceptions(dbExceptions);
                }
            }
            catch (Exception ex)
            {
                Rollback("");
                Connection.Close();

                throw new ApplicationException(String.Format("DBRecordBase.ExcludeFromList() failed, setname: {0} Exception: {1}", listname, ex.Message), ex);
            }
            finally
            {
                dalRecord.SetReturnCode(RecordReturnCode);
                if (RecordReturnCode != 0)
                {
                    ErrorList.SetValue(listname);
                    ErrorTable.SetValue(dalRecord.TableName);
                }
            }
        }

        /// <summary>
        /// Include table row in list 
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="listname">The name of the set.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void IncludeInList(DalRecordBase dalRecord, string listname, long ownerCurId = 0)
        {

            dalRecord.CurrentList = _currentDBCurrency.ListTable[listname];

            IncludeInList(dalRecord, listname, dalRecord.CurrentList.MemberCur.CurrencyKeys, dalRecord.CurrentList.ListPositionCode, dalRecord.CurrentList.ListActionCode, ownerCurId);
        }

        /// <summary>
        /// Include table row in list 
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="listname">The name of the set.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void IncludeInList(DalRecordBase dalRecord, string listname, params DbAllow[] dbExceptions)
        {

            dalRecord.CurrentList = _currentDBCurrency.ListTable[listname];

            IncludeInList(dalRecord, listname, dalRecord.CurrentList.MemberCur.CurrencyKeys, dalRecord.CurrentList.ListPositionCode, dalRecord.CurrentList.ListActionCode, 0, dbExceptions);
        }

        /// <summary>
        /// Include table row in list
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="listname">The name of the set.</param>
        /// <param name="currencyKeys">Collection of currency keys from the current member record of the current set.</param>
        /// <param name="listStatus">ListPositionCode of the current set.</param>
        /// <param name="rowStatus">ListActionCode of the current set.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void IncludeInList(DalRecordBase dalRecord, string listname, Hashtable currencyKeys, ListStatus listStatus, RowStatus rowStatus, long ownerCurId = -1, params DbAllow[] dbExceptions)
        {
            dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
            dalRecord.CurrentList = _currentDBCurrency.ListTable[listname];
            GetLatest(dalRecord, DBReturnData.No);

            if (CheckRecStatus(dalRecord.RecordName) != RowStatus.GoodRow)
            {
                RecordReturnCode = 706;
                return;
            }

            if (IsListMember(listname)) // ticket 8819
            {
                RecordReturnCode = 716;
                return;
            }
            else
            {
                RecordReturnCode = 0;
            }

            IncludeInList_core(dalRecord, listname, currencyKeys, listStatus, rowStatus, ownerCurId, dbExceptions);
        }

        /// <summary>
        /// Include in List as called after Insert Db row - needed for connecting junction table row or linklist
        /// </summary>
        /// <param name="dalRecord"></param>
        /// <param name="listname"></param>
        /// <param name="currencyKeys"></param>
        /// <param name="listStatus"></param>
        /// <param name="rowStatus"></param>
        /// <param name="ownerCurId"></param>
        /// <param name="dbExceptions"></param>
        private void IncludeInList_for_InsertDBRow(DalRecordBase dalRecord, string listname, Hashtable currencyKeys, ListStatus listStatus, RowStatus rowStatus, int ownerCurId = -1, params DbAllow[] dbExceptions)
        {
            //dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
            dalRecord.CurrentList = _currentDBCurrency.ListTable[listname];
            //GetLatest(dalRecord, DBReturnData.No);

            if (CheckRecStatus(dalRecord.RecordName) != RowStatus.GoodRow)
            {
                RecordReturnCode = 706;
                return;
            }

            // OA added because any automatic record must be included in list - ticket 5953 - Added OM for issue 8735
            if (IsListMember(listname) && dalRecord.CurrentList.ListOpt != ListOptions.MA && dalRecord.CurrentList.ListOpt != ListOptions.OA && dalRecord.CurrentList.ListOpt != ListOptions.OM)
            {
                RecordReturnCode = 716;
                return;
            }
            else
            {
                RecordReturnCode = 0;
            }

            IncludeInList_core(dalRecord, listname, currencyKeys, listStatus, rowStatus, ownerCurId, dbExceptions);
        }

        /// <summary>
        /// Core process for Include in list - connection member to a set
        /// </summary>
        /// <param name="dalRecord"></param>
        /// <param name="listname"></param>
        /// <param name="currencyKeys"></param>
        /// <param name="listStatus"></param>
        /// <param name="rowStatus"></param>
        /// <param name="ownerCurId"></param>
        /// <param name="dbExceptions"></param>
        private void IncludeInList_core(DalRecordBase dalRecord, string listname, Hashtable currencyKeys, ListStatus listStatus, RowStatus rowStatus, long ownerCurId, params DbAllow[] dbExceptions)
        {
            dalRecord.ColumnList.Clear(); dalRecord.ColumnParms.Clear();
            dalRecord.ListUpdateType = "Include"; dalRecord.DBOperation = DbOperation.Update;
            dalRecord.RefreshCache = true;
            CheckForConnection();
            SetTransaction();

            long saveOwnerKey = 0;
            if (dalRecord.CurrentList.OwnerCur != null)
                if (dalRecord.CurrentList.ListOpt == ListOptions.MA || dalRecord.CurrentList.ListOpt == ListOptions.OA)
                {
                    saveOwnerKey = ownerCurId != -1 && ownerCurId != 0 ? ownerCurId : Convert.ToInt64(dalRecord.CurrentList.OwnerCur.CurrencyKeys[dalRecord.CurrentList.OwnerCur.IdColName]);
                }
                else
                {
                    saveOwnerKey = ownerCurId != -1 && ownerCurId != 0 ? ownerCurId : GetCurrentOwnerID(dalRecord.CurrentList.OwnerCur.RecordName, dalRecord.CurrentList.OwnerCur);
                }

            if (dalRecord.CurrentList.ListOrd == ListOrder.LinkList)
            {
                string _linkListKeyNext = string.Concat(listname, STR_NextPointerSuffix);
                string _linkListKeyPrior = string.Concat(listname, STR_PriorPointerSuffix);
                long _nextPointer = 0;
                long _priorPointer = 0;
                //Issue 4616 - set pointers to 0 is ListStatus is OnNone
                if (listStatus == ListStatus.OnNone && rowStatus != RowStatus.DeletedRow)
                {
                    // OnNone ListStatus - Get last row in list to obtain Prior pointer
                    long saveDBKey = dalRecord.IDColumnValue;
                    GetInList(dalRecord, RowPosition.Last, listname, DBReturnData.No);
                    if (ReturnCode.IsEqualTo(0))
                    {
                        long saveNewOwnerKey = Convert.ToInt64(dalRecord.CurrentList.OwnerCur.CurrencyKeys[dalRecord.CurrentList.OwnerCur.IdColName]);
                        if (dalRecord.IDColumnValue == saveDBKey)
                        {
                            _priorPointer = 0;
                        }
                        else
                        {
                            _priorPointer = dalRecord.IDColumnValue;
                        }
                        if (saveNewOwnerKey != saveOwnerKey)
                        {
                            // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                            bool canDropCurrencies = _canDropCurrencies;
                            _canDropCurrencies = false;
                            GetByIdCol(CurrentDalRecords[dalRecord.CurrentList.OwnerCur.RecordName], saveOwnerKey, DBReturnData.No);
                            _canDropCurrencies = canDropCurrencies;
                        }
                    }
                    // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                    bool cdp = _canDropCurrencies;
                    _canDropCurrencies = false;
                    GetByIdCol(dalRecord, saveDBKey, DBReturnData.No);
                    _canDropCurrencies = cdp;

                    _nextPointer = 0;
                }
                else if (listStatus == ListStatus.OnMemberRow || (listStatus == ListStatus.OnNone && rowStatus == RowStatus.DeletedRow))
                {
                    // Set Next Pointer
                    if (dalRecord.CurrentList.MemberCur.CurrencyKeys.ContainsKey(_linkListKeyNext))
                    {
                        _nextPointer = currencyKeys[_linkListKeyNext] == null ? 0 : Convert.ToInt64(currencyKeys[_linkListKeyNext]);
                    }
                    // Set Prior Pointer
                    // Issue 5244 - We need to differentiate whether we are inserting after a delete or not to get the correct prior pointer
                    if (rowStatus == RowStatus.DeletedRow)
                        _priorPointer = currencyKeys[_linkListKeyPrior] == null ? 0 : Convert.ToInt64(currencyKeys[_linkListKeyPrior]);
                    else
                        _priorPointer = currencyKeys[dalRecord.IDColumnName] == null ? 0 : Convert.ToInt64(currencyKeys[dalRecord.IDColumnName]);
                }
                else if (listStatus == ListStatus.OnOwnerRow)
                {
                    // OnOwner ListStatus - Get first row in list to obtain Next pointer
                    long saveDBKey = dalRecord.IDColumnValue;
                    _updateLists = false;
                    GetInList(dalRecord, RowPosition.First, listname, DBReturnData.No);
                    _updateLists = true;
                    if (ReturnCode.IsEqualTo(0))
                    {
                        long saveNewOwnerKey = Convert.ToInt64(dalRecord.CurrentList.OwnerCur.CurrencyKeys[dalRecord.CurrentList.OwnerCur.IdColName]);
                        if (dalRecord.IDColumnValue == saveDBKey)
                        {
                            if (dalRecord.dt.Rows.Count > 1)
                                _nextPointer = Convert.ToInt64(dalRecord.dt.Rows[1][dalRecord.IDColumnName]);
                        }
                        else
                        {
                            _nextPointer = dalRecord.IDColumnValue;
                        }
                        if (saveNewOwnerKey != saveOwnerKey)
                        {
                            // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                            bool canDropCurrencies = _canDropCurrencies;
                            _canDropCurrencies = false;
                            GetByIdCol(CurrentDalRecords[dalRecord.CurrentList.OwnerCur.RecordName], saveOwnerKey, DBReturnData.No);
                            _canDropCurrencies = canDropCurrencies;
                        }
                    }
                    // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                    bool cdp = _canDropCurrencies;
                    _canDropCurrencies = false;
                    GetByIdCol(dalRecord, saveDBKey, DBReturnData.No);
                    _canDropCurrencies = cdp;

                    _priorPointer = 0;
                }
                dalRecord.CurrentList.MemberCur.CurrencyKeys[_linkListKeyPrior] = _priorPointer;
                dalRecord.CurrentList.MemberCur.CurrencyKeys[_linkListKeyNext] = _nextPointer;

            }

            if (saveOwnerKey != 0)
                dalRecord.CurrentList.OwnerCur.CurrencyKeys[dalRecord.CurrentList.OwnerCur.IdColName] = saveOwnerKey;

            SetUpSqlCommand(dalRecord, "UpdateList");

            try
            {
                _returnCode = _command.ExecuteNonQuery();

                if (_returnCode > 0)
                {

                    GetLatest(dalRecord, DBReturnData.No);
                    RecordReturnCode = 0;
                    dalRecord.dt.Clear();
                }
                else
                {
                    RecordReturnCode = 706;
                }
                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                {
                    CheckAutostatusExceptions(dbExceptions);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Cannot insert duplicate key") || ex.Message.Contains("SQLSTATE=23505") || ex.Message.Contains("ORA-00001"))
                {
                    RecordReturnCode = 705;
                    return;
                }
                Rollback("");
                Connection.Close();

                throw new ApplicationException(String.Format("DBRecordBase.IncludeInList() failed, setname: {0}{1}", listname, ex.Message), ex);
            }
            finally
            {
                dalRecord.SetReturnCode(RecordReturnCode);
                if (RecordReturnCode != 0)
                {
                    ErrorList.SetValue(listname);
                    ErrorTable.SetValue(dalRecord.TableName);
                }
            }
        }

        /// <summary>
        /// Modify columns in table row
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void ModifyDBRow(DalRecordBase dalRecord, params DbAllow[] dbExceptions)
        {
            RecordReturnCode = 0;
            dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
            CheckCurrentTableId(dalRecord);
            dalRecord.DBOperation = DbOperation.Update;
            dalRecord.ColumnList.Clear(); dalRecord.ColumnParms.Clear(); dalRecord.ColumnUpdateSets.Clear();
            if (RecordReturnCode != 0)
            {
                RecordReturnCode = RecordReturnCode + 800;
                return;
            }
            CheckForConnection();
            SetTransaction();

            SetUpSqlCommand(dalRecord, "Update");

            try
            {
                _returnCode = _command.ExecuteNonQuery();

                if (_returnCode > 0)
                {
                    RecordReturnCode = 0;

                    //dalRecord.dt.Rows.Clear(); // datatable row should be updated otherwise old data stays in the datatable
                    //GetLatest(dalRecord, DBReturnData.Yes);
                    //  can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                    //bool canDropCurrencies = _canDropCurrencies;
                    //_updateRecord = true;
                    //_canDropCurrencies = false;
                    //SetRecCurrency(dalRecord, STR_All);
                    //_canDropCurrencies = canDropCurrencies;
                    //_updateRecord = false;

                    // Update datatable rows with modified data
                    bool canDropCurrencies = _canDropCurrencies;
                    _canDropCurrencies = false;
                    _updateLists = false;
                    _updateRecord = true;
                    //Save Current Datatable and CurrentRow
                    int rowIndex = dalRecord.DataTableCurrentRow;
                    DataTable oldDt = dalRecord.dt.Clone();
                    foreach (DataRow dtRow in dalRecord.dt.Rows)
                    {
                        oldDt.ImportRow(dtRow);
                    }
                    //Get Modified Record contents
                    GetByIdCol(dalRecord, dalRecord.IDColumnValue, DBReturnData.No);
                    for (int index = 0; index < oldDt.Columns.Count; index++)
                    {
                        oldDt.Columns[index].ReadOnly = false;
                        if (dalRecord.dt.Columns.Contains(oldDt.Columns[index].ColumnName))
                            oldDt.Rows[rowIndex][index] = dalRecord.dt.Rows[0][oldDt.Columns[index].ColumnName];
                    }
                    dalRecord.dt = oldDt;
                    dalRecord.DataTableCurrentRow = rowIndex;
                    _canDropCurrencies = canDropCurrencies;
                    _updateLists = true;

                }
                else
                {
                    RecordReturnCode = 806;
                }
                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                {
                    CheckAutostatusExceptions(dbExceptions);
                }
                if (RecordReturnCode != 0)
                {
                    ErrorTable.SetValue(dalRecord.TableName);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Cannot insert duplicate key") || ex.Message.Contains("SQLSTATE=23505") || ex.Message.Contains("ORA-00001"))
                {
                    RecordReturnCode = 805;
                    ErrorTable.SetValue(dalRecord.TableName);
                    return;
                }

                Rollback("");
                Connection.Close();

                throw new ApplicationException(String.Format("DBRecordBase.ModifyDBRow() failed {0}", ex.Message), ex);
            }
        }
        #endregion

        #region Delete Command Methods
        /// <summary>
        /// Delete Database row and check EraseType for further cascading delete set members rows
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="Erasetype">Specifies which records should be disconnected and can take one of the following values:
        /// <list type="bullet">
        /// <item><description>CascadeNone</description></item>
        /// <item><description>CascadePermanent</description></item>
        /// <item><description>CascadeSelective</description></item>
        /// <item><description>CascadeAll</description></item>
        /// </list>
        /// </param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void DeleteDBRow(DalRecordBase dalRecord, DeleteRowOption Erasetype, params DbAllow[] dbExceptions)
        {
            RecordReturnCode = 0;
            // Top level Delete
            _ownerDalRecord = null;
            DeleteDatabaseRow(dalRecord, Erasetype, dbExceptions);
        }

        #endregion

        #region Insert Command Methods

        /// <summary>
        /// Insert new row into database table
        /// </summary>
        /// <param name="dalRecord">A reference to the current DAL record object.</param>
        /// <param name="dbExceptions">Auto status exceptions.</param>
        public void InsertDBRow(DalRecordBase dalRecord, params DbAllow[] dbExceptions)
        {
            dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
            dalRecord.DBOperation = DbOperation.Insert; dalRecord.ColumnList.Clear(); dalRecord.ColumnParms.Clear();
            dalRecord.RefreshCache = true;
            CheckForConnection();
            SetTransaction();

            try
            {
                SetUpSqlCommand(dalRecord, "Insert");

                int newID = Convert.ToInt32(_command.ExecuteScalar());
                if (_isOracle)
                {
                    // _command.CommandText = string.Format(STR_OracleGetCurrVal, _sqlCommandUtility.SchemaName, dalRecord.CurrentRecord.TableName);
                    // _command.CommandText = string.Format(STR_OracleGetMaxID, dalRecord.IDColumnName, _sqlCommandUtility.SchemaName, dalRecord.CurrentRecord.TableName);
                    // newID = Convert.ToInt32(_command.ExecuteScalar());
                    newID = (int)_command.Parameters[STR_OracleLatestID].Value;
                }

                if (newID > 0)
                {
                    Hashtable currencyKeys = (Hashtable)dalRecord.CurrentRecord.CurrencyKeys.Clone();
                    Dictionary<string, ListStatus> listStatus = new Dictionary<string, ListStatus>();
                    Dictionary<string, RowStatus> rowStatus = new Dictionary<string, RowStatus>();
                    foreach (string lkey in dalRecord.CurrentRecord.ListNames.Keys)
                    {
                        if (dalRecord.CurrentRecord.ListNames[lkey] == "Member")
                        {
                            if (_currentDBCurrency.ListTable.ContainsKey(lkey))
                            {
                                ListCurrency listcurrency = (ListCurrency)_currentDBCurrency.ListTable[lkey];
                                if (listcurrency != null)
                                {
                                    listStatus.Add(lkey, listcurrency.ListPositionCode);
                                    rowStatus.Add(lkey, listcurrency.MemberCur.RecordActionCode);
                                }
                            }
                        }
                    }

                    // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                    bool canDropCurrencies = _canDropCurrencies;
                    _canDropCurrencies = false;
                    GetByIdCol(dalRecord, newID, DBReturnData.Yes);
                    _canDropCurrencies = canDropCurrencies;

                    //** Check for linklists or mult member lists for further updates
                    foreach (string lkey in dalRecord.CurrentRecord.ListNames.Keys)
                    {
                        if (dalRecord.CurrentRecord.ListNames[lkey] == "Member")
                        {
                            if (_currentDBCurrency.ListTable.ContainsKey(lkey))
                            {
                                ListCurrency listcurrency = (ListCurrency)_currentDBCurrency.ListTable[lkey];
                                if (listcurrency != null)
                                    if (((listcurrency.ListOpt == ListOptions.OA || listcurrency.ListOpt == ListOptions.MA) && listcurrency.ListOrd == ListOrder.LinkList) || (listcurrency.MemberList != null && listcurrency.MemberList.Count > 0))
                                    {
                                        IncludeInList_for_InsertDBRow(dalRecord, lkey, currencyKeys, listStatus[lkey], rowStatus[lkey]);
                                    }
                            }
                        }
                    }
                    //********************

                    RecordReturnCode = 0;
                }
                else
                {
                    RecordReturnCode = 1205;
                }
                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                    CheckAutostatusExceptions(dbExceptions);
                if (RecordReturnCode != 0)
                {
                    ErrorTable.SetValue(dalRecord.TableName);
                }
            }
            catch (Exception ex)
            {

                if (ex.Message.Contains("Cannot insert duplicate key") || ex.Message.Contains("SQLSTATE=23505") || ex.Message.Contains("ORA-00001"))
                {
                    RecordReturnCode = 1205;
                    ErrorTable.SetValue(dalRecord.TableName);
                    return;
                }
                else
                {
                    if ((dr != null) && (!dr.IsClosed))
                        dr.Close();
                    Rollback("");
                    Connection.Close();
                }

                StringBuilder sb = new StringBuilder();

                foreach (DbAllow allow in dbExceptions)
                {
                    sb.Append(allow.ToString());
                    sb.Append(" ");
                }

                throw new ApplicationException(String.Format("DBRecordBase.InsertDBRow() failed, Exceptions: {0}{1}", sb, ex.Message), ex);
            }
        }

        #endregion

        #region List Methods

        /// <summary>
        /// Check to see if current row is in specified list
        /// </summary>
        /// <param name="listname">The name of the set.</param>
        /// <returns>Returns true if the current record is a member of the specified set.</returns>
        public bool IsInList(string listname)
        {
            listname = listname.Replace('-', '_').Trim();
            bool isInList = false;

            if (_currentDBCurrency != null && _currentDBCurrency.CurrentIdCol != 0)
            {
                if (_currentDBCurrency.ListTable.ContainsKey(listname) && _currentDBCurrency.RecordTable.ContainsKey(_currentDBCurrency.CurrentRecordName))
                {
                    ListCurrency listCurrency = _currentDBCurrency.ListTable[listname];
                    if (listCurrency != null)
                    {
                        RecordCurrency recordCurrency = _currentDBCurrency.RecordTable[_currentDBCurrency.CurrentRecordName];

                        if (recordCurrency != null)
                        {
                            //validation
                            if (recordCurrency.CurrencyKeys == null
                                || recordCurrency.CurrencyKeys[recordCurrency.IdColName] == null || recordCurrency.CurrencyKeys[recordCurrency.IdColName] == DBNull.Value
                                    || _currentDBCurrency.CurrentIdCol != Convert.ToInt64(recordCurrency.CurrencyKeys[recordCurrency.IdColName]))
                            {
                                throw new Exception("Current record ID do not match with CurrentIdCol");
                            }

                            if (listCurrency.OwnerCur == null)
                            {
                                if (recordCurrency.CurrencyKeys.Contains(string.Concat(listname, "_MEMIND")))
                                {
                                    if (recordCurrency.CurrencyKeys[string.Concat(listname, "_MEMIND")] != DBNull.Value && (string)recordCurrency.CurrencyKeys[string.Concat(listname, "_MEMIND")] == "Y")
                                        isInList = true;
                                }
                                else if (recordCurrency.CurrencyKeys.Contains(string.Concat(listname, "_SEQ")))
                                {
                                    if (recordCurrency.CurrencyKeys[string.Concat(listname, "_SEQ")] != DBNull.Value)
                                        isInList = true;
                                }
                                else if (recordCurrency.CurrencyKeys.Contains(listCurrency.ListName))
                                {
                                    if (recordCurrency.CurrencyKeys[listCurrency.ListName] != null && (string)recordCurrency.CurrencyKeys[listCurrency.ListName] == "Y")
                                    {
                                        isInList = true;
                                    }
                                }
                            }
                            // if list owner is defined - issues 8993, 8977
                            else if (listCurrency.OwnerCur != null && listCurrency.OwnerCur.CurrencyKeys != null
                                && ((listCurrency.OwnerCur.CurrencyKeys[listCurrency.OwnerCur.IdColName] != null
                                && listCurrency.OwnerCur.CurrencyKeys[listCurrency.OwnerCur.IdColName] != DBNull.Value)
                                || (listCurrency.OwnerCur.CurrencyKeys[listCurrency.ListFkName] != null
                                && listCurrency.OwnerCur.CurrencyKeys[listCurrency.ListFkName] != DBNull.Value)))
                            {
                                // Following commented out - issues 9191, 9108
                                //if (listCurrency.OwnerCur.RecordName == recordCurrency.RecordName && listCurrency.OwnerCur.IdColName == recordCurrency.IdColName
                                //    && (int)listCurrency.OwnerCur.CurrencyKeys[recordCurrency.IdColName] == (int)recordCurrency.CurrencyKeys[recordCurrency.IdColName])
                                //{
                                //    isInList = true;
                                //}
                                //// record is member
                                //else 
                                if (recordCurrency.ListNames.ContainsKey(listname))
                                {

                                    // workaround for SE-DECOMP-MENU - ticket 8905 and 8990
                                    if (recordCurrency.CurrencyKeys.ContainsKey(listCurrency.ListFkName)
                                        && listCurrency.OwnerCur.CurrencyKeys.ContainsKey(listCurrency.ListFkName))
                                    {
                                        if (recordCurrency.CurrencyKeys[listCurrency.ListFkName] != null && recordCurrency.CurrencyKeys[listCurrency.ListFkName] != DBNull.Value
                                           && Convert.ToInt64(recordCurrency.CurrencyKeys[listCurrency.ListFkName]) != 0
                                           && listCurrency.OwnerCur.CurrencyKeys[listCurrency.ListFkName] != null && listCurrency.OwnerCur.CurrencyKeys[listCurrency.ListFkName] != DBNull.Value
                                           && Convert.ToInt64(recordCurrency.CurrencyKeys[listCurrency.ListFkName]) == Convert.ToInt64(listCurrency.OwnerCur.CurrencyKeys[listCurrency.ListFkName]))
                                        {
                                            isInList = true;
                                        }
                                        else if (recordCurrency.CurrencyKeys[listCurrency.ListFkName] != null && recordCurrency.CurrencyKeys[listCurrency.ListFkName] != DBNull.Value
                                           && Convert.ToInt64(recordCurrency.CurrencyKeys[listCurrency.ListFkName]) != 0)
                                        {
                                            isInList = true;
                                        }
                                    }
                                    // normal code as it should be
                                    else if (recordCurrency.CurrencyKeys.ContainsKey(listCurrency.ListFkName)
                                            && recordCurrency.CurrencyKeys[listCurrency.ListFkName] != null && recordCurrency.CurrencyKeys[listCurrency.ListFkName] != DBNull.Value
                                            && Convert.ToInt64(recordCurrency.CurrencyKeys[listCurrency.ListFkName]) != 0
                                                && Convert.ToInt64(recordCurrency.CurrencyKeys[listCurrency.ListFkName]) == Convert.ToInt64(listCurrency.OwnerCur.CurrencyKeys[listCurrency.OwnerCur.IdColName]))
                                    {
                                        isInList = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (!isInList)
                SetReturnCode("1601");
            if (RecordReturnCode != 0)
            {
                ErrorList.SetValue(listname);
            }
            return isInList;
        }

        private bool IsListMember(string listname)
        {
            listname = listname.Replace('-', '_').Trim();
            bool isInList = false;

            if (_currentDBCurrency.ListTable.ContainsKey(listname))
            {
                ListCurrency listCurrency = _currentDBCurrency.ListTable[listname];

                if (listCurrency.ListActionCode == RowStatus.GoodRow)
                {
                    if (listCurrency.ListPositionCode == ListStatus.OnMemberRow)
                    {
                        if (listCurrency.MemberCur != null) // && listCurrency.ListPositionCode == ListStatus.OnMemberRow) - commented out because of the ticket 8648
                        {
                            if (listCurrency.MemberCur.CurrencyKeys != null
                                    && listCurrency.MemberCur.CurrencyKeys[listCurrency.MemberCur.IdColName] != null
                                        && listCurrency.MemberCur.CurrencyKeys[listCurrency.MemberCur.IdColName].ToString() != string.Empty)
                                isInList = _currentDBCurrency.CurrentIdCol == Convert.ToInt64(listCurrency.MemberCur.CurrencyKeys[listCurrency.MemberCur.IdColName]);
                        }
                    }
                    else if (listCurrency.ListPositionCode == ListStatus.OnOwnerRow)
                    {

                        RecordCurrency recordCurrency = _currentDBCurrency.RecordTable[_currentDBCurrency.CurrentRecordName];

                        if (_currentDBCurrency.CurrentIdCol > 0 && _currentDBCurrency.CurrentIdCol != Convert.ToInt64(recordCurrency.CurrencyKeys[recordCurrency.IdColName]))
                            throw new Exception("RecordTable is not updated with current record currencies");

                        isInList = recordCurrency.CurrencyKeys.ContainsKey(listCurrency.ListFkName)
                            && recordCurrency.CurrencyKeys[listCurrency.ListFkName] != null && recordCurrency.CurrencyKeys[listCurrency.ListFkName] != DBNull.Value
                                && Convert.ToInt64(recordCurrency.CurrencyKeys[listCurrency.ListFkName]) == Convert.ToInt64(listCurrency.OwnerCur.CurrencyKeys[listCurrency.OwnerCur.IdColName]);
                    }
                }
            }

            if (!isInList)
                SetReturnCode("1601");
            if (RecordReturnCode != 0)
            {
                ErrorList.SetValue(listname);
            }
            return isInList;
        }

        /// <summary>
        /// Check to see if current row does not own any records in specified list
        /// </summary>
        /// <param name="listname">The name of the set.</param>
        /// <returns>Returns true if the set is empty, otherwise returns false.</returns>
        public bool IsListEmpty(string listname)
        {
            listname = listname.Replace('-', '_').Trim();
            CheckForConnection();
            _command = _connection.CreateCommand();
            _command.Transaction = Transaction;

            ListCurrency checkListCurrency = _currentDBCurrency.ListTable[listname];
            _sqlCommandUtility.CurrentDbCurrency = _currentDBCurrency;
            _sqlCommandUtility.SetUpCommandForListCheck(checkListCurrency, _command);

            try
            {
                object testObj = _command.ExecuteScalar();
                if (testObj == null)
                {
                    SetReturnCode("0000");
                    return true;
                }
                else
                {
                    SetReturnCode("1601");
                    return false;
                }
            }
            catch (Exception ex)
            {
                return true;
                throw new ApplicationException("ISListEmpty failed, ListName: " + listname + ex.Message, ex);
            }
            finally
            {
                if (RecordReturnCode != 0)
                {
                    ErrorList.SetValue(listname);
                }
            }
        }

        private bool IsListEmpty(ListCurrency checkListCurrency, long ownerID)
        {
            CheckForConnection();
            _command = _connection.CreateCommand();
            _command.Transaction = Transaction;

            _sqlCommandUtility.CurrentDbCurrency = _currentDBCurrency;
            _sqlCommandUtility.SetUpCommandForListCheck(checkListCurrency, _command, ownerID);

            try
            {
                object testObj = _command.ExecuteScalar();
                return testObj == null;
            }
            catch (Exception ex)
            {
                return true;
                throw new ApplicationException("ISListEmpty failed, ListName: " + checkListCurrency.ListName + ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets the list owner ID Column(foreign key) value of a specified list from Currency data
        /// </summary>
        /// <param name="ListName"></param>
        /// <returns></returns>
        private long GetListOwnerID(ListCurrency listCurrency, bool isGetOwner = false)
        {
            long returnID = 0;

            if (listCurrency.ListPositionCode == ListStatus.OnOwnerRow && listCurrency.ListActionCode == RowStatus.GoodRow
                && !IsListEmpty(listCurrency, Convert.ToInt64(listCurrency.OwnerCur.CurrencyKeys[listCurrency.OwnerCur.IdColName])))
            {
                returnID = Convert.ToInt64(listCurrency.OwnerCur.CurrencyKeys[_listcurrency.OwnerCur.IdColName]);
            }
            else if (listCurrency.MemberCur != null && listCurrency.MemberCur.CurrencyKeys != null
                && listCurrency.MemberCur.CurrencyKeys.ContainsKey(listCurrency.ListFkName)
                && listCurrency.MemberCur.CurrencyKeys[listCurrency.ListFkName] != null
                && listCurrency.MemberCur.CurrencyKeys[listCurrency.ListFkName] != DBNull.Value)
            {
                returnID = Convert.ToInt64(listCurrency.MemberCur.CurrencyKeys[listCurrency.ListFkName]);
            }
            else if (listCurrency.OwnerCur.CurrencyKeys[listCurrency.ListName] == null)
            {
                returnID = Convert.ToInt64(listCurrency.OwnerCur.CurrencyKeys[_listcurrency.OwnerCur.IdColName]);
            }

            // If there is a second set, override the ID with that value
            if (listCurrency.OwnerCur.CurrencyKeys[listCurrency.ListName] != null)
            {
                if (returnID != Convert.ToInt64(listCurrency.OwnerCur.CurrencyKeys[listCurrency.ListName]) && listCurrency.ListPositionCode != ListStatus.OnOwnerRow)
                {
                    //Updated for SAAQ-FA issue 3910  and issue 4003 and issue 4670
                    //if ((_listcurrency.MemberCur.CurrencyKeys[_listcurrency.OwnerCur.IdColName] != null) && isGetOwner)
                    //Check for returnID == 0 for issue 8665
                    if (isGetOwner || returnID == 0)
                        returnID = Convert.ToInt64(listCurrency.OwnerCur.CurrencyKeys[listCurrency.ListName]);
                }
            }

            return returnID;
        }

        /// <summary>
        /// Update list currency keys
        /// </summary>
        /// <param name="listname">The name of the set.</param>
        /// <param name="reccurrency">Provides an instance of currency object. It belongs to the record, which currency keys are used for updating the set.</param>
        /// <param name="rowtype">'Member' or 'Owner' type of the row.</param>
        public void UpdateListKeys(string listname, RecordCurrency reccurrency, string rowtype)
        {
            listname = listname.Replace('-', '_').Trim();
            _listcurrency = (ListCurrency)_currentDBCurrency.ListTable[listname];

            if (rowtype == STR_Member)
            {
                if (_listcurrency.ListFkName != null)
                {
                    // Check to see if Member particpates in List
                    if (reccurrency.CurrencyKeys[_listcurrency.ListFkName].ToString() != string.Empty)
                    {
                        _listcurrency.ListActionCode = RowStatus.GoodRow;
                        _listcurrency.MemberCur = reccurrency.Clone();
                        _listcurrency.ListPositionCode = ListStatus.OnMemberRow;
                        if (_listcurrency.OwnerCur != null)
                            _listcurrency.OwnerCur.CurrencyKeys[_listcurrency.OwnerCur.IdColName] = _listcurrency.MemberCur.CurrencyKeys[_listcurrency.ListFkName];
                    }
                }
                else
                // System Index List 
                {
                    _listcurrency.ListActionCode = RowStatus.GoodRow;
                    _listcurrency.MemberCur = reccurrency.Clone();
                    _listcurrency.ListPositionCode = ListStatus.OnMemberRow;
                }
            }
            else if (rowtype == STR_Owner)
            {
                _listcurrency.ListActionCode = RowStatus.GoodRow;
                _listcurrency.OwnerCur = reccurrency.Clone();
                _listcurrency.ListPositionCode = ListStatus.OnOwnerRow;
            }
        }

        /// <summary>
        /// Update list status fields
        /// </summary>
        /// <param name="ListName">The name of the set.</param>
        /// <param name="rStatus">Specifies ListActionCode value.</param>
        /// <param name="lPosition">Specifies ListPositionCode value.</param>
        public void UpdateListStatus(string ListName, RowStatus rStatus, ListStatus lPosition)
        {
            ListName = ListName.Replace('-', '_').Trim();
            if (_currentDBCurrency.ListTable.ContainsKey(ListName))
            {
                _listcurrency = (ListCurrency)_currentDBCurrency.ListTable[ListName];
                if (_listcurrency != null)
                {
                    _listcurrency.ListActionCode = rStatus;
                    _listcurrency.ListPositionCode = lPosition;
                    if (lPosition == ListStatus.OnOwnerRow && _listcurrency.OwnerCur == null)
                        _listcurrency.ListPositionCode = ListStatus.OnNone;
                }
            }
            else
                throw new ApplicationException(ListName + ":Invalid List for List Status");

            // THS: If this throws an exception, go see if the ListName was added to
            // _keycurrency.ListTable on the A474H820 constructor (or equivalent).
            // You'll see a lot of calls to _keycurrency.ListTable.Add for each list type.
        }

        /// <summary>
        /// Set End Of List condition in specified list
        /// </summary>
        /// <param name="ListName">The name of the set.</param>
        public void SetEndofList(string ListName)
        {
            ListName = ListName.Replace('-', '_').Trim();
            if (_currentDBCurrency.ListTable.ContainsKey(ListName))
            {
                _listcurrency = _currentDBCurrency.ListTable[ListName];
                _listcurrency.ListActionCode = RowStatus.NoRow;
                _listcurrency.ListPositionCode = ListStatus.OnNone;
                if (_listcurrency.OwnerCur != null)
                {
                    UpdateRecCurrency(_listcurrency.OwnerCur.RecordName, _listcurrency.OwnerCur);
                    UpdateCurrentConversation(_listcurrency.OwnerCur.RecordName, _currentDBCurrency.GetTableCurrentID(_listcurrency.OwnerCur.RecordName));
                }
            }
            else
                throw new ApplicationException(ListName + ":Invalid List for List Status");
        }

        /// <summary>
        /// Returns List RowStatus of the specified list
        /// </summary>
        /// <param name="ListName">The name of the set.</param>
        /// <returns>Returns ListActionCode value. If specified set cannot be found, then returns NoRow value.</returns>
        public RowStatus CheckListStatus(string ListName)
        {
            ListName = ListName.Replace('-', '_').Trim();
            if (_currentDBCurrency.ListTable.ContainsKey(ListName))
            {
                _listcurrency = (ListCurrency)_currentDBCurrency.ListTable[ListName];
                return _listcurrency.ListActionCode;
            }
            else
                return RowStatus.NoRow;
        }

        /// <summary>
        /// Returns list ListStatus of the specified list
        /// </summary>
        /// <param name="ListName">The name of the set.</param>
        /// <returns>Returns ListPositionCode value. If specified set cannot be found, then returns OnNone value.</returns>
        public ListStatus CheckListPosition(string ListName)
        {
            ListName = ListName.Replace('-', '_').Trim();
            if (_currentDBCurrency.ListTable.ContainsKey(ListName))
            {
                _listcurrency = (ListCurrency)_currentDBCurrency.ListTable[ListName];
                return _listcurrency.ListPositionCode;
            }
            else
                return ListStatus.OnNone;
        }

        /// <summary>
        /// Check to see if list contains current member currency
        /// </summary>
        /// <param name="ListName">The name of the set.</param>
        /// <returns>Returns true if member currency keys are not null or empty, returns false otherwise.</returns>
        public bool CheckListMemberCurrencyKey(string ListName)
        {
            if (_currentDBCurrency.ListTable.ContainsKey(ListName))
            {
                _listcurrency = (ListCurrency)_currentDBCurrency.ListTable[ListName];
                if (_listcurrency.MemberCur.CurrencyKeys[ListName] != null && _listcurrency.MemberCur.CurrencyKeys[ListName].ToString() != string.Empty)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Database Command Methods

        /// <summary>
        /// Set currency Record name and key value
        /// </summary>
        /// <param name="RecName">DAL record name.</param>
        /// <param name="KeyValue">The value of the key (i.e. primary key column) currency key.</param>
        public void UpdateCurrentConversation(string RecName, long KeyValue)
        {
            _currentDBCurrency.CurrentRecordName = RecName;
            _currentDBCurrency.CurrentIdCol = KeyValue;
        }

        /// <summary>
        /// Check for active Database connection; If not open, then open connection
        /// </summary>
        public void CheckForConnection()
        {
            System.Diagnostics.Debug.WriteLine(" in CheckForConnection");
            if (_connection == null)
            {
                _connection = _dbFactory.CreateConnection();
                _connection.ConnectionString = ConnectionString;
            }
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        //public void CheckForUtilityDBConnection()
        //{
        //    System.Diagnostics.Debug.WriteLine(" in CheckForConnection");
        //    if (_connection == null)
        //    {
        //        _connection = _dbFactory.CreateConnection();
        //    }
        //    IConnectionString _connectionStringInfo = GetDBConnectionInfo();
        //    string connectionString = _connectionStringInfo.GetConnectionString("SecurityConnectionString");
        //    if (string.IsNullOrEmpty(connectionString))
        //        _connection.ConnectionString = ConnectionString;
        //    else
        //        _connection.ConnectionString = connectionString;
        //    if (_connection.State != ConnectionState.Open)
        //        _connection.Open();
        //}

        /// <summary>
        /// Set up new database transaction (BeginTransaction)
        /// </summary>
        public void SetTransaction()
        {
            if (_transaction == null || (_transaction != null && _transaction.Connection == null))
                _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// Rollback current database transaction
        /// </summary>
        /// <param name="rbType">Specifies rollback type. If the value is 'CONTINUE', starts a new transaction.</param>
        public void Rollback(string rbType)
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }

            if (rbType == "CONTINUE")
                _transaction = _connection.BeginTransaction();
            else
                CloseConnection();

            RecordReturnCode = 0;
        }

        /// <summary>
        /// Commit database transaction
        /// </summary>
        /// <param name="commitType">This parameter is obsolete and can take any string value.</param>
        public void Commit(string commitType)
        {
            Commit();
            RecordReturnCode = 0;
            //if (commitType.Trim() == "ALL" || commitType.Trim() == "TASK" )
            //  Commit types TASK and ALL not supportted
        }

        /// <summary>
        /// Commit database transaction
        /// </summary>
        public void Commit()
        {
            if (_transaction != null && _transaction.Connection != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
            }
            _transaction = null;
            RecordReturnCode = 0;
        }

        /// <summary>
        /// Commit database transaction and close the database connection
        /// </summary>
        public void Finish()
        {
            if (_transaction != null && _transaction.Connection != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
            CloseConnection();
            RecordReturnCode = 0;
        }

        /// <summary>
        /// Close database connection
        /// </summary>
        public void CloseConnection()
        {
            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    Commit();
                    _connection.Close();
                }
            }
        }

        /// <summary>
        /// Rollback current database transaction and close database connection
        /// </summary>
        public void Abort()
        {
            Rollback("");
            CloseConnection();
        }

        /// <summary>
        /// Check autostatus exceptions
        /// </summary>
        /// <param name="dbExceptions">Auto status exceptions</param>
        public void CheckAutostatusExceptions(DbAllow[] dbExceptions)
        {

            if (dbExceptions != null)
            {
                foreach (DbAllow TestStatus in dbExceptions)
                {
                    if (RecordReturnCode == (int)TestStatus)
                        return;
                }
                throw new ApplicationException(String.Format("Unexpected DB Status Error: {0}", _errorStatusRecord.ReturnCode.AsString()));
            }
            else
                if (!Enum.IsDefined(typeof(DbAutoStatus), _errorStatusRecord.ReturnCode.AsInt()))
                throw new ApplicationException(String.Format("Unexpected DB Status Error: {0}", _errorStatusRecord.ReturnCode.AsString()));
        }

        /// <summary>
        /// Set Status only updates retrun code to 0; No database areas exist in thsi environment
        /// </summary>
        /// <param name="tableGroup">Parameter is not used.</param>
        /// <param name="statusType">Parameter is not used.</param>
        public void SetStatus(string tableGroup, string statusType)
        {
            SetReturnCode(0);
        }

        /// <summary>
        /// Initializes the Error status record
        /// </summary>
        public void ResetErrorStatus()
        {
            _errorStatusRecord.Record.ResetToInitialValue();
            ErrorTable.SetValue("");
            ErrorList.SetValue("");

        }
        #endregion

        #region Misc. Methods

        public void UpdateRecStatus(string RecName, RowStatus rStatus)
        {
            if (_currentDBCurrency.RecordTable.ContainsKey(RecName))
            {
                _reccurrency = (RecordCurrency)_currentDBCurrency.RecordTable[RecName];
                _reccurrency.RecordActionCode = rStatus;
            }
            else
            {
                throw new ApplicationException("Invalid Table Name for RecStatus");
            }
        }

        public RowStatus CheckRecStatus(string RecName)
        {
            if (_currentDBCurrency.RecordTable.ContainsKey(RecName))
            {
                _reccurrency = (RecordCurrency)_currentDBCurrency.RecordTable[RecName];
                return _reccurrency.RecordActionCode;
            }
            else
                return RowStatus.NoRow;
        }

        public void UpdateCurrencies(DBConversation sourceConversation)
        {
            List<string> Reckeys = new List<string>(CurrentDBCurrency.RecordTable.Keys);
            foreach (string recKey in Reckeys)
            {
                if (sourceConversation.CurrentDBCurrency.RecordTable.ContainsKey(recKey))
                {
                    CurrentDBCurrency.RecordTable[recKey] = sourceConversation.CurrentDBCurrency.RecordTable[recKey];
                }
            }
            List<string> Listkeys = new List<string>(CurrentDBCurrency.ListTable.Keys);
            foreach (string listKey in Listkeys)
            {
                if (sourceConversation.CurrentDBCurrency.ListTable.ContainsKey(listKey))
                {
                    CurrentDBCurrency.ListTable[listKey] = sourceConversation.CurrentDBCurrency.ListTable[listKey];
                }
            }
            CurrentDBCurrency.CurrentRecordName = sourceConversation.CurrentDBCurrency.CurrentRecordName;
            CurrentDBCurrency.CurrentIdCol = sourceConversation.CurrentDBCurrency.CurrentIdCol;
            CurrentList.SetValue(sourceConversation.CurrentList);
            CurrentTable.SetValue(sourceConversation.CurrentTable);

        }

        public void GetRecordData(DalRecordBase dalRecord, params DbAllow[] dbExceptions)
        {
            RecordReturnCode = 0;
            if (dalRecord.dt.Rows.Count > 0)
                dalRecord.SetRecordData();
            else
            {
                if (CurrentDalRecords.ContainsKey(dalRecord.RecordName))
                {
                    dalRecord = CurrentDalRecords[dalRecord.RecordName];
                    if (dalRecord.dt.Rows.Count > 0)
                        dalRecord.SetRecordData();
                }
            }
        }

        public void ClearRecordKeys(string RecName)
        {
            if (_currentDBCurrency.RecordTable.ContainsKey(RecName))
            {
                _reccurrency = (RecordCurrency)_currentDBCurrency.RecordTable[RecName];
                string[] keyarray = new string[_reccurrency.CurrencyKeys.Count];
                _reccurrency.CurrencyKeys.Keys.CopyTo(keyarray, 0);
                foreach (string keyString in keyarray)
                {
                    _reccurrency.CurrencyKeys[keyString] = null;
                }
            }
        }

        public DataTable ExecuteSql(string sqlString, params object[] parms)
        {
            try
            {
                SetReturnCode(0);
                CheckForConnection();
                _command = _connection.CreateCommand();
                _command.CommandText = sqlString;
                int parmCtr = 1;
                if (parms != null)
                {
                    while (parmCtr <= parms.Length)
                    {
                        DbParameter sqlParm = _command.CreateParameter();
                        sqlParm.ParameterName = string.Concat(":Parm", parmCtr.ToString());
                        sqlParm.Value = parms[parmCtr - 1];
                        _command.Parameters.Add(sqlParm);
                        parmCtr++;
                    }
                    for (int i = _command.Parameters.Count - 1; i >= 0; i--)
                    {
                        if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                            _command.Parameters.RemoveAt(i);
                    }
                }
                using (dr = _command.ExecuteReader())
                {
                    localDT = new DataTable();
                    localDT.Load(dr);
                }
                return localDT;
            }
            catch (Exception ex)
            {
                if (Transaction != null)
                    Rollback("");
                throw new ApplicationException(string.Concat("Execute SQL Error!", ex.Message));
            }


        }

        public void SetTempDBCache(int temporaryDBCache)
        {
            _tempDbCache = temporaryDBCache;
        }

        public void SetPerformanceCriteria(string performanceCriteria)
        {
            PerformanceCriteria = performanceCriteria;
        }

        public void SetPerformanceStatement(string performanceStatement)
        {
            PerformanceStatement = performanceStatement;
        }

        public void RefreshAllCurrency()
        {
            if (_currentDBCurrency != null)
                _currentDBCurrency.ClearCurrency();
        }

        #endregion

        #region Queue Methods
        /// <summary>
        /// Returns queue data string from Queue database table
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="queuePosition"></param>
        /// <param name="queueRowID"></param>
        /// <returns></returns>
        public byte[] GetQueue(string queueName, ScratchOption scratchOption, RowPosition queuePosition, int queueRowID, params DbAllow[] dbExceptions)
        {

            try
            {
                SetReturnCode(0);
                CheckForConnection();
                _command = _connection.CreateCommand();
                _command.Transaction = Transaction;
                _sqlCommandUtility.SetUpUtilityCommandString(_command, "SelectQueue", queueName, null, queuePosition, 0, 0);
                for (int i = _command.Parameters.Count - 1; i >= 0; i--)
                {
                    if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                        _command.Parameters.RemoveAt(i);
                }
                using (dr = _command.ExecuteReader())
                {
                    localDT = new DataTable();
                    localDT.Load(dr);
                }
            }
            catch (Exception ex)
            {
                if (Transaction != null)
                    Rollback("");
                SetReturnCode(4404);
                throw new ApplicationException(string.Concat("Get Queue Failed!", ex.Message));
            }

            if (localDT.Rows.Count > 0)
            {
                int currentRecID = -99;
                int rowCtr = -1;
                if (QueueCurrency.ContainsKey(queueName))
                    currentRecID = QueueCurrency[queueName];

                if (queuePosition == RowPosition.First)
                {
                    rowCtr = 0;
                }
                else if (queuePosition == RowPosition.Next)
                {
                    if (currentRecID == -99)
                        rowCtr++;
                    else
                    {
                        rowCtr = 0;
                        for (int ctr = 0; ctr < localDT.Rows.Count; ctr++)
                        {
                            int checkRecID = Convert.ToInt32((long)localDT.Rows[ctr]["RECID"]);
                            if (checkRecID > currentRecID)
                            {
                                break;
                            }
                            else if (checkRecID == currentRecID)
                            {
                                rowCtr++;
                                break;
                            }
                            rowCtr++;
                        }

                    }
                }
                else if (queuePosition == RowPosition.Last)
                {
                    rowCtr = localDT.Rows.Count - 1;
                }
                else
                {
                    for (int ctr = 0; ctr < localDT.Rows.Count; ctr++)
                    {
                        int checkRecID = Convert.ToInt32((long)localDT.Rows[ctr]["RECID"]);
                        if (queuePosition == RowPosition.Current && checkRecID == currentRecID)
                        {
                            rowCtr = ctr;
                            break;
                        }
                        else if (queuePosition == RowPosition.Prior && checkRecID < currentRecID)
                        {
                            rowCtr = ctr;
                            break;
                        }
                        else if (queuePosition == RowPosition.RowID && checkRecID == queueRowID)
                        {
                            rowCtr = ctr;
                            break;
                        }
                    }
                }
                if (rowCtr == -1)
                    rowCtr = 0;

                if (rowCtr >= localDT.Rows.Count)
                {
                    SetReturnCode(4404);
                    return null;
                }

                byte[] returnArray;
                if (localDT.Columns["REC_DATA"].DataType == typeof(String))
                    returnArray = ConvertHexStringToByteArray((string)localDT.Rows[rowCtr]["REC_DATA"]);
                else
                    returnArray = (byte[])localDT.Rows[rowCtr]["REC_DATA"];

                currentRecID = Convert.ToInt32((long)localDT.Rows[rowCtr]["RECID"]);

                if (!QueueCurrency.ContainsKey(queueName))
                    QueueCurrency.Add(queueName, currentRecID);
                else
                    QueueCurrency[queueName] = currentRecID;

                if (scratchOption == ScratchOption.Delete)
                    DeleteQueue(queueName, RowPosition.Current);

                return returnArray;
            }
            else
            {
                SetReturnCode(4404);
                return null;
            }
        }

        /// <summary>
        /// Inserts new queue data into Queue database table
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="queueData"></param>
        /// <param name="queuePosition"></param>
        /// <param name="queueRowID"></param>
        /// <param name="queueRetention"></param>
        public void PutQueue(string queueName, byte[] queueData, RowPosition queuePosition, int queueRetention, params DbAllow[] dbExceptions)
        {

            try
            {
                CheckForConnection();
                _command = _connection.CreateCommand();
                int queueRecID = 0;
                bool queueExists = false;
                byte[] checkQueue = GetQueue(queueName, ScratchOption.Keep, RowPosition.Last, 0, DbAllow.QueueNotFound);
                if (checkQueue != null)
                {
                    if (localDT.Rows.Count > 0)
                    {
                        queueExists = true;
                        if (queuePosition == RowPosition.Last)
                        {
                            long lastRecID = (long)localDT.Rows[localDT.Rows.Count - 1]["RECID"];
                            queueRecID = Convert.ToInt32(lastRecID) + 1;
                        }
                        else if (queuePosition == RowPosition.First)
                        {
                            long firstRecID = (long)localDT.Rows[0]["RECID"];
                            queueRecID = Convert.ToInt32(firstRecID) - 1;
                        }
                    }
                }

                SetTransaction();
                _command.Transaction = Transaction;
                _sqlCommandUtility.SetUpUtilityCommandString(_command, "PutQueue", queueName, queueData, queuePosition, queueRecID, queueRetention);

                if (queueExists)
                {
                    string[] sqlStrings = _command.CommandText.Split(new string[] { " ;  " }, StringSplitOptions.None);
                    _command.CommandText = string.Concat("BEGIN ", sqlStrings[1]);
                }

                _command.ExecuteNonQuery();

                if (!QueueCurrency.ContainsKey(queueName))
                    QueueCurrency.Add(queueName, queueRecID);
                else
                    QueueCurrency[queueName] = queueRecID;

                SetReturnCode(0);
            }
            catch (Exception ex)
            {
                SetReturnCode(4499);
                throw new ApplicationException(string.Concat("Put Queue Failed!", ex.Message));
            }

        }

        /// <summary>
        /// Delete queue data from Queue database table 
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="queuePosition"></param>
        /// <param name="queueRowID"></param>
        public void DeleteQueue(string queueName, RowPosition queuePosition, params DbAllow[] dbExceptions)
        {
            try
            {
                CheckForConnection();
                _command = _connection.CreateCommand();
                int queRecID = 0;
                if (queuePosition == RowPosition.Current)
                {
                    if (QueueCurrency.ContainsKey(queueName))
                        queRecID = QueueCurrency[queueName];

                }
                else if (queuePosition == RowPosition.All)
                {
                    QueueCurrency.Remove(queueName);
                }

                SetTransaction();
                _command.Transaction = Transaction;
                _sqlCommandUtility.SetUpUtilityCommandString(_command, "DeleteQueue", queueName, null, queuePosition, queRecID, 0);

                _command.ExecuteNonQuery();
                SetReturnCode(0);

                if (queuePosition == RowPosition.Current)
                {
                    //Check to see if Current queue removed was last one for the ID
                    _sqlCommandUtility.SetUpUtilityCommandString(_command, "SelectQueue", queueName, null, RowPosition.All, 0, 0);
                    using (dr = _command.ExecuteReader())
                    {
                        localDT = new DataTable();
                        localDT.Load(dr);
                    }
                    if (localDT.Rows.Count == 0)
                    {
                        _sqlCommandUtility.SetUpUtilityCommandString(_command, "DeleteQueue", queueName, null, RowPosition.All, queRecID, 0);
                        _command.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex)
            {
                if (Transaction != null)
                    Rollback("");
                SetReturnCode(4499);
                throw new ApplicationException(string.Concat("Delete Queue Failed!", ex.Message));
            }

        }
        #endregion

        #region EditCodeTable Methods
        /// <summary>
        /// Returns Collection of Edit/Code tables data string from Queue database table
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllEditCodeTables()
        {

            try
            {
                SetReturnCode(0);
                CheckForConnection();
                _command = _connection.CreateCommand();
                _command.Transaction = Transaction;
                _sqlCommandUtility.SetUpEditCodeUtilityCommandString(_command, "SelectAllEditCodeTables", "");
                using (dr = _command.ExecuteReader())
                {
                    localDT = new DataTable();
                    localDT.Load(dr);
                }
            }
            catch (Exception ex)
            {
                if (Transaction != null)
                    Rollback("");
                SetReturnCode(4404);
                throw new ApplicationException(string.Concat("Get EditCode tables Failed!", ex.Message));
            }


            return localDT;

        }

        /// <summary>
        /// Replace Edit table on Utility DB
        /// </summary>
        /// <param name="editTableName"></param>
        /// <param name="editTableEntries"></param>
        /// <param name="editType"></param>
        public void ReplaceEditTable(string editTableName, IList<string> editTableEntries, string editType)
        {

            try
            {
                CheckForConnection();
                _command = _connection.CreateCommand();
                SetTransaction();
                _command.Transaction = Transaction;
                _sqlCommandUtility.SetUpEditCodeUtilityCommandString(_command, "ReplaceEditCodeTableHdr", editTableName);
                _command.ExecuteNonQuery();
                _sqlCommandUtility.SetUpEditCodeUtilityCommandString(_command, "AddEditCodeTableDtl", editTableName);
                short sortOrder = 0;
                foreach (string editValue in editTableEntries)
                {
                    sortOrder++;
                    _command.Parameters[1].Value = editValue;
                    _command.Parameters[3].Value = sortOrder;
                    _command.ExecuteNonQuery();
                }

                SetReturnCode(0);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Concat("Replace Edit Table Failed!", ex.Message));
            }

        }
        /// <summary>
        /// Replace Code Table on Utility DB
        /// </summary>
        /// <param name="codeTableName"></param>
        /// <param name="codeTableEntries"></param>
        public void ReplaceCodeTable(string codeTableName, IDictionary<string, string> codeTableEntries)
        {

            try
            {
                CheckForConnection();
                _command = _connection.CreateCommand();
                SetTransaction();
                _command.Transaction = Transaction;
                _sqlCommandUtility.SetUpEditCodeUtilityCommandString(_command, "ReplaceEditCodeTable", codeTableName);
                _command.ExecuteNonQuery();
                short sortOrder = 0;
                foreach (string inValue in codeTableEntries.Keys)
                {
                    sortOrder++;
                    _command.Parameters[1].Value = inValue;
                    _command.Parameters[2].Value = codeTableEntries[inValue];
                    _command.Parameters[3].Value = sortOrder;
                    _command.ExecuteNonQuery();
                }

                SetReturnCode(0);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Concat("Replace Code Table Failed!", ex.Message));
            }

        }

        /// <summary>
        /// Delete queue data from Queue database table 
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="queuePosition"></param>
        /// <param name="queueRowID"></param>
        public void DeleteEditCodeTable(string tableName)
        {
            try
            {
                CheckForConnection();
                _command = _connection.CreateCommand();
                SetTransaction();
                _command.Transaction = Transaction;
                _sqlCommandUtility.SetUpEditCodeUtilityCommandString(_command, "DeleteEditCodeTable", tableName);

                _command.ExecuteNonQuery();
                SetReturnCode(0);
            }
            catch (Exception ex)
            {
                if (Transaction != null)
                    Rollback("");
                SetReturnCode(4499);
                throw new ApplicationException(string.Concat("Delete edit/Code table Failed!", ex.Message));
            }

        }
        #endregion

        /// <summary>
        /// Set the Return code with specified string
        /// </summary>
        /// <param name="value">The value of the retun code. Must not be null. If an empty sting then sets return code to 0.</param>
        public void SetReturnCode(string value)
        {
            if (value.Trim() == string.Empty)
                RecordReturnCode = 0;
            else
                RecordReturnCode = Int32.Parse(value);
        }

        /// <summary>
        /// Set the Return code with specified integer
        /// </summary>
        /// <param name="value">The value of the return code.</param>
        public void SetReturnCode(int value)
        {
            RecordReturnCode = value;
        }

        /// <summary>
        /// Set the return code with 0
        /// </summary>
        public void SetReturnCodeWithZeroes()
        {
            RecordReturnCode = 0;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// ReturnCode property
        /// </summary>
        private int RecordReturnCode
        {
            get
            {
                return _errorStatusRecord.ReturnCode.AsInt();
            }
            set
            {
                _errorStatusRecord.ReturnCode.Assign(value);
                _errorStatusRecord.ErrStatSave.Assign(value);
            }
        }

        /// <summary>
        /// Update Record Currecy 
        /// </summary>
        /// <param name="RecName"></param>
        /// <param name="rcurrency"></param>
        private void UpdateRecCurrency(string RecName, RecordCurrency rcurrency)
        {
            if (_currentDBCurrency.RecordTable.ContainsKey(RecName))
            {
                _reccurrency = (RecordCurrency)_currentDBCurrency.RecordTable[RecName];
                _reccurrency = rcurrency;
            }
            else
            {
                throw new ApplicationException("Invalid Table Name for RecStatus");
            }
        }

        /// <summary>
        /// Select a datatable of rows from the database
        /// </summary>
        /// <param name="dalRecord"></param>
        /// <param name="gttype"></param>
        /// <param name="setname"></param>
        private void SelectListRows(DalRecordBase dalRecord, RowPosition gttype, string setname)
        {
            // Fill Data Table with Rows from database ================================================          
            // bool isGetLast = false;
            _StartID = _currentDBCurrency.GetListCurrentID(setname);

            if (_StartID != 0 && CheckListStatus(setname) == RowStatus.GoodRow && dalRecord.IDColumnValue == 0 && gttype != RowPosition.First && gttype != RowPosition.Last)
            {
                dalRecord.IDColumnValue = _StartID;
            }

            // Set up first and last options parms
            if (gttype == RowPosition.First)
            {
                gttype = RowPosition.Next;
                _StartID = 0;
            }
            else if (gttype == RowPosition.Last)
            {
                gttype = RowPosition.Prior;
                //isGetLast = true;
                _StartID = 0;
            }
            dalRecord.SelectOrder = gttype;

            // Retrieve data from database into datatable 
            CheckForConnection();

            SetUpSqlCommand(dalRecord, "SelectInList");
            for (int i = _command.Parameters.Count - 1; i >= 0; i--)
            {
                if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                    _command.Parameters.RemoveAt(i);
            }
            try
            {
                dalRecord.dt.Reset();
                using (dr = _command.ExecuteReader())
                {
                    dalRecord.dt.Load(dr, LoadOption.OverwriteChanges);
                }
                dalRecord.RefreshCache = false;

                if (dalRecord.dt.Rows.Count > 0 && gttype == RowPosition.Prior)
                {
                    if (dalRecord.CurrentList.ListOrd != ListOrder.LinkList)
                    {
                        DataColumn dataColumn = dalRecord.dt.Columns["rownbr"];
                        bool isRnbr = false;
                        if (dataColumn == null)
                        {
                            dataColumn = dalRecord.dt.Columns["rnbr"];
                            isRnbr = true;
                        }
                        dataColumn.ReadOnly = false;
                        for (int ctr = 0; ctr < dalRecord.dt.Rows.Count; ctr++)
                        {
                            if (isRnbr)
                                dalRecord.dt.Rows[ctr]["rnbr"] = ctr;
                            else
                                dalRecord.dt.Rows[ctr]["rownbr"] = ctr;
                        }

                        DataView dv = dalRecord.dt.DefaultView;
                        if (isRnbr)
                            dv.Sort = "rnbr desc";
                        else
                            dv.Sort = "rownbr desc";
                        dalRecord.dt = dv.ToTable();

                    }
                }
            }
            catch (Exception ex)
            {
                if (Transaction != null)
                    Rollback("");

                throw new ApplicationException(String.Format("DBRecordBase.SelectListRow() failed, gttype: {0} setname: {1}{2}", gttype, setname, ex.Message), ex);
            }

            if (dalRecord.dt.Rows.Count == 0)
            {
                RecordReturnCode = 307;

                if (dalRecord.CurrentList.OwnerCur != null)
                {
                    _currentDBCurrency.CurrentRecordName = dalRecord.CurrentList.OwnerCur.RecordName;
                    _currentDBCurrency.CurrentIdCol = _currentDBCurrency.GetListOwnerID(setname, true);
                }
            }
            else
            {
                RecordReturnCode = 0;
                if (gttype == RowPosition.Prior)
                {
                    dalRecord.DataTableCurrentRow = dalRecord.dt.Rows.Count - 1;
                }
                else
                {
                    //dalRecord.CurrentList.DataTableCurrentRow = 0;
                    dalRecord.DataTableCurrentRow = 0;
                }
            }
        }

        private void GetInArea(DalRecordBase dalRecord, RowPosition gttype, DBReturnData DataFlag, params DbAllow[] dbExceptions)
        {
            // Get within Area request walks the          
            _StartID = dalRecord.IDColumnValue;

            if (_StartID > 0 && gttype != RowPosition.First && gttype != RowPosition.Last)
            {
                dalRecord.IDColumnValue = _StartID;
            }
            // Set up first and last options parms
            if (gttype == RowPosition.First)
            {
                gttype = RowPosition.Next;
                _StartID = 0;
                dalRecord.IDColumnValue = 0;
            }
            else if (gttype == RowPosition.Last)
            {
                _tempDbCache = 1; //Get only 1 record so that the prior works.
                gttype = RowPosition.Prior;
                _StartID = 0;
                dalRecord.IDColumnValue = 0;
            }
            else
            {
                if (_StartID == 0)
                    dalRecord.DataTableCurrentRow = 0;
                else if (gttype == RowPosition.Next)
                    dalRecord.DataTableCurrentRow++;
                else if (gttype == RowPosition.Prior)
                    dalRecord.DataTableCurrentRow++;
            }
            dalRecord.SelectOrder = gttype;

            if (dalRecord.dt == null || _StartID == 0 || (dalRecord.DataTableCurrentRow < 0 || dalRecord.DataTableCurrentRow >= dalRecord.dt.Rows.Count))
            {
                // Retrieve data from database into datatable 
                CheckForConnection();
                SetUpSqlCommand(dalRecord, "SelectInArea");
                for (int i = _command.Parameters.Count - 1; i >= 0; i--)
                {
                    if (!_command.CommandText.Contains(_command.Parameters[i].ParameterName))
                        _command.Parameters.RemoveAt(i);
                }
                try
                {
                    dalRecord.dt.Reset();
                    using (dr = _command.ExecuteReader())
                    {
                        dalRecord.dt.Load(dr, LoadOption.OverwriteChanges);
                    }
                    dalRecord.RefreshCache = false;
                }
                catch (Exception ex)
                {
                    if (Transaction != null)
                        Rollback("");

                    throw new ApplicationException(String.Format("DBRecordBase.SelectInArea() failed, gttype: {0}: {1}", gttype, ex.Message), ex);
                }

                if (dalRecord.dt.Rows.Count == 0)
                {
                    RecordReturnCode = 307;
                }
                else
                {
                    RecordReturnCode = 0;
                    if (gttype == RowPosition.Prior)
                    {
                        dalRecord.DataTableCurrentRow = 0;
                    }
                    else
                    {
                        dalRecord.DataTableCurrentRow = 0;
                    }
                }
            }

            if (RecordReturnCode == 0)
            {
                bool canDropCurrencies = _canDropCurrencies;
                _canDropCurrencies = false;
                SetRecCurrency(dalRecord, _updateLists ? STR_All : dalRecord.LastListName);
                _canDropCurrencies = canDropCurrencies;

                if (DataFlag == DBReturnData.Yes)
                {
                    dalRecord.SetRecordData();
                }
            }
        }

        /// <summary>
        /// Call SQLCommandUtility to create dynamic SQL string
        /// </summary>
        /// <param name="dalRecord"></param>
        /// <param name="queryType"></param>
        private void SetUpSqlCommand(DalRecordBase dalRecord, string queryType, DBLockType lockType = DBLockType.None)
        {
            // if no CommandTimeout key is found set value to normal default of 30 seconds
            _commandTimeout = String.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("DBCommandTimeout")) ? 30
                : Convert.ToInt32(ConfigSettings.GetAppSettingsString("DBCommandTimeout"));

            _command = _connection.CreateCommand();
            _command.CommandTimeout = _commandTimeout;
            dalRecord.Command = _command; dalRecord.ParmPrefix = _parmPrefix;
            _commandRecordName = dalRecord.RecordName;
            _sqlCommandUtility.CurrentDbCurrency = CurrentDBCurrency;
            _sqlCommandUtility.SetUpCommandString(dalRecord, queryType, lockType, _tempDbCache, _inGetInListByKey);

            _tempDbCache = 0;

            if (PerformanceCriteria.Trim().Length > 0)
            {
                if (_command.CommandType == CommandType.Text)
                {
                    AddPerformanceCriteria();
                }
                PerformanceCriteria = "";
            }

            if (PerformanceStatement.Trim().Length > 0)
            {
                if (_command.CommandType == CommandType.Text)
                {
                    _command.CommandText = PerformanceStatement;
                    SimpleLogging.LogMandatoryMessageToFile("Performance replacement for above statement: " + PerformanceStatement);
                }
                PerformanceStatement = "";
            }

            _command.Transaction = Transaction;

            if (!CurrentDalRecords.ContainsKey(dalRecord.RecordName))
                CurrentDalRecords.Add(dalRecord.RecordName, dalRecord);

        }

        /// <summary>
        /// Add Performance Criteria to sql statement
        /// </summary>
        private void AddPerformanceCriteria()
        {
            bool cteFlag = false;
            int wherePos = -1;
            int orderPos = -1;

            if (_command.CommandText.Trim().ToUpper().StartsWith("WITH CTE AS (SELECT ROW_NUMBER()"))
            {
                cteFlag = true;
                wherePos = _command.CommandText.ToUpper().LastIndexOf(" WHERE ");
                orderPos = _command.CommandText.ToUpper().LastIndexOf(" ORDER BY ");
            }

            else if (_command.CommandText.Trim().ToUpper().Contains(" ) T2  ) T ORDER BY "))
            {
                orderPos = _command.CommandText.ToUpper().LastIndexOf(" ORDER BY ");
            }

            else if (!_command.CommandText.Trim().ToUpper().StartsWith("SELECT "))
            {
                SimpleLogging.LogMandatoryMessageToFile("Attempted Performance update on above: " + PerformanceCriteria);
                return;
            }

            else
            {
                wherePos = _command.CommandText.ToUpper().IndexOf(" WHERE ");
                orderPos = _command.CommandText.ToUpper().IndexOf(" ORDER BY ");
            }

            string newSQL = "";

            if (wherePos > -1)
            {
                if (orderPos > -1)
                {
                    newSQL = _command.CommandText.Substring(0, orderPos);
                    newSQL = string.Concat(newSQL, " AND ", PerformanceCriteria);
                    newSQL = string.Concat(newSQL, _command.CommandText.Substring(orderPos));
                }
                else
                {
                    newSQL = string.Concat(_command.CommandText, " AND ", PerformanceCriteria);
                }
            }
            else
            {
                if (orderPos > -1)
                {
                    newSQL = _command.CommandText.Substring(0, orderPos);
                    newSQL = string.Concat(newSQL, " WHERE ", PerformanceCriteria);
                    newSQL = string.Concat(newSQL, _command.CommandText.Substring(orderPos));
                }
                else
                {
                    newSQL = string.Concat(_command.CommandText, " WHERE ", PerformanceCriteria);
                }
            }

            _command.CommandText = newSQL;
            SimpleLogging.LogMandatoryMessageToFile("Performance update for above: " + newSQL);
        }

        /// <summary>
        /// Returns record ID of next row in list
        /// </summary>
        /// <param name="listName"></param>
        /// <returns></returns>
        private long GetNextKeyInList(string listName)
        {
            ListCurrency listcurrency = null; DalRecordBase dalRec = null;

            if (_currentDBCurrency.ListTable.ContainsKey(listName))
            {
                listcurrency = _currentDBCurrency.ListTable[listName];
            }
            if (listcurrency == null || listcurrency.MemberCur == null) return 0;

            long idColumnValue = 0;
            if (listcurrency.ListOrd == ListOrder.SORTED && (CheckListStatus(listName) == RowStatus.MissOnUsing))
            {
                idColumnValue = listcurrency.MissOnUsingNext;
                MDSY.Framework.Buffer.FieldEx.IdRecordName =
                    SaveKeyListStatus == ListStatus.OnOwnerRow
                        ? _currentDBCurrency.ListTable[listName.AsString().Replace('-', '_').Trim()].OwnerCur.RecordName
                        : _currentDBCurrency.ListTable[listName.AsString().Replace('-', '_').Trim()].MemberCur.RecordName;
            }
            else
            {
                if (!CurrentDalRecords.ContainsKey(listcurrency.MemberCur.RecordName))
                {
                    DalRecordBase newdalRec = GetInstanceOfDalRecord(listcurrency.MemberCur.RecordName);
                    if (newdalRec != null)
                    {
                        CurrentDalRecords.Add(listcurrency.MemberCur.RecordName, newdalRec);
                    }
                }
                dalRec = CurrentDalRecords[listcurrency.MemberCur.RecordName];
                if (dalRec == null) return 0;

                long saveDBKey = 0;
                if (listcurrency.ListPositionCode == ListStatus.OnMemberRow && listcurrency.MemberCur.RecordActionCode == RowStatus.GoodRow && listcurrency.MemberCur.CurrencyKeys.ContainsKey(listcurrency.MemberCur.IdColName))
                {
                    saveDBKey = Convert.ToInt64(listcurrency.MemberCur.CurrencyKeys[listcurrency.MemberCur.IdColName]);
                }

                if (dalRec.dt == null || dalRec.dt.Rows.Count < 2 || dalRec.DataTableCurrentRow + 1 >= dalRec.dt.Rows.Count)
                {
                    // ReQuery DB
                    GetInList(dalRec, RowPosition.Next, listName, DBReturnData.No);
                    if (RecordReturnCode == 0)
                    {
                        idColumnValue = dalRec.IDColumnValue;
                        MDSY.Framework.Buffer.FieldEx.IdRecordName = _currentDBCurrency.ListTable[listName.AsString().Replace('-', '_').Trim()].MemberCur.RecordName;
                        GetInList(dalRec, RowPosition.Prior, listName, DBReturnData.No);
                        SaveKeyListStatus = ListStatus.OnMemberRow;
                    }
                    else if (RecordReturnCode == 307)
                    {
                        if (saveDBKey != 0)
                        {
                            GetByIdCol(dalRec, saveDBKey, DBReturnData.No);
                        }
                        ResetErrorStatus();
                        if (listcurrency.OwnerCur != null)
                        {
                            idColumnValue = Convert.ToInt64(_listcurrency.OwnerCur.CurrencyKeys[_listcurrency.OwnerCur.IdColName]);
                            MDSY.Framework.Buffer.FieldEx.IdRecordName = _currentDBCurrency.ListTable[listName.AsString().Replace('-', '_').Trim()].OwnerCur.RecordName; // issue 9851
                            SaveKeyListStatus = ListStatus.OnOwnerRow;
                        }
                    }
                }
                else
                {
                    idColumnValue = Convert.ToInt64(dalRec.dt.Rows[dalRec.DataTableCurrentRow + 1][dalRec.IDColumnName]);
                    MDSY.Framework.Buffer.FieldEx.IdRecordName = dalRec.RecordName;
                    SaveKeyListStatus = ListStatus.OnMemberRow;
                }
            }

            return idColumnValue;
        }

        /// <summary>
        /// Returns record ID of the presvois row in list
        /// </summary>
        /// <param name="listName"></param>
        /// <returns></returns>
        private long GetPriorKeyInList(string listName)
        {
            ListCurrency listcurrency = null; DalRecordBase dalRec = null;

            if (_currentDBCurrency.ListTable.ContainsKey(listName))
            {
                listcurrency = _currentDBCurrency.ListTable[listName];
            }
            if (listcurrency == null || listcurrency.MemberCur == null) return 0;

            if (listcurrency.ListOrd == ListOrder.SORTED && (CheckListStatus(listName) == RowStatus.MissOnUsing))
            {
                long returnValue = listcurrency.MissOnUsingPrev;
                MDSY.Framework.Buffer.FieldEx.IdRecordName =
                    SaveKeyListStatus == ListStatus.OnOwnerRow
                        ? _currentDBCurrency.ListTable[listName.AsString().Replace('-', '_').Trim()].OwnerCur.RecordName
                        : _currentDBCurrency.ListTable[listName.AsString().Replace('-', '_').Trim()].MemberCur.RecordName;
                return returnValue;
            }
            else
            {
                dalRec = CurrentDalRecords[listcurrency.MemberCur.RecordName];
                if (dalRec == null) return 0;

                if (dalRec.dt == null || dalRec.dt.Rows.Count < 2 || dalRec.DataTableCurrentRow - 1 < 0)
                {
                    ListStatus oldListStatus = listcurrency.ListPositionCode;
                    RowStatus oldActionCode = listcurrency.ListActionCode;
                    long saveDBKey = 0;
                    if (oldListStatus == ListStatus.OnMemberRow && oldActionCode == RowStatus.GoodRow && listcurrency.MemberCur.CurrencyKeys.ContainsKey(listcurrency.MemberCur.IdColName))
                    {
                        saveDBKey = Convert.ToInt64(listcurrency.MemberCur.CurrencyKeys[listcurrency.MemberCur.IdColName]);
                    }
                    // ReQuery DB
                    GetInList(dalRec, RowPosition.Prior, listName, DBReturnData.No);
                    if (RecordReturnCode == 0)
                    {
                        long returnValue = dalRec.IDColumnValue;
                        GetInList(dalRec, RowPosition.Next, listName, DBReturnData.No);
                        SaveKeyListStatus = ListStatus.OnMemberRow;
                        MDSY.Framework.Buffer.FieldEx.IdRecordName = _currentDBCurrency.ListTable[listName.AsString().Replace('-', '_').Trim()].MemberCur.RecordName;
                        return returnValue;
                    }
                    // prior record is owner record. 
                    else if (RecordReturnCode == 307
                                && oldListStatus == ListStatus.OnMemberRow && oldActionCode == RowStatus.GoodRow
                                && listcurrency.ListPositionCode == ListStatus.OnOwnerRow)
                    {
                        ResetErrorStatus();
                        if (saveDBKey != 0)
                        {
                            GetByIdCol(dalRec, saveDBKey, DBReturnData.No);
                        }
                        SaveKeyListStatus = ListStatus.OnOwnerRow;
                        MDSY.Framework.Buffer.FieldEx.IdRecordName = _currentDBCurrency.ListTable[listName.AsString().Replace('-', '_').Trim()].OwnerCur.RecordName; // issue 9851
                        return Convert.ToInt64(listcurrency.OwnerCur.CurrencyKeys[listcurrency.OwnerCur.IdColName]);
                    }
                }
                else
                {
                    SaveKeyListStatus = ListStatus.OnMemberRow;
                    MDSY.Framework.Buffer.FieldEx.IdRecordName = dalRec.RecordName;
                    return Convert.ToInt64(dalRec.dt.Rows[dalRec.DataTableCurrentRow - 1][dalRec.IDColumnName]);
                }
            }

            return 0;
        }

        /// <summary>
        /// Returns current record ID of list owner row
        /// </summary>
        /// <param name="ownerRecName"></param>
        /// <param name="listRecCurrency"></param>
        /// <returns></returns>
        private long GetCurrentOwnerID(string ownerRecName, RecordCurrency listRecCurrency)
        {
            if (CurrentDalRecords.Keys.Contains(ownerRecName) && CurrentDalRecords[ownerRecName].IDColumnValue != 0)
                return CurrentDalRecords[ownerRecName].IDColumnValue;
            else if (listRecCurrency.CurrencyKeys[listRecCurrency.IdColName] != null)
                return Convert.ToInt64(listRecCurrency.CurrencyKeys[listRecCurrency.IdColName]);
            else return 0;
        }

        /// <summary>
        /// Check for valid current table ID
        /// </summary>
        /// <param name="dalRecord"></param>
        private void CheckCurrentTableId(DalRecordBase dalRecord)
        {
            if (dalRecord.RecordName != _currentDBCurrency.CurrentRecordName)
            {
                RecordReturnCode = 20;
                return;
            }
            if (_currentDBCurrency.CurrentIdCol == 0)
            {
                RecordReturnCode = 13;
                return;
            }
        }

        /// <summary>
        /// Set Record currency from current row
        /// </summary>
        /// <param name="dalRecord"></param>
        /// <param name="cType"></param>
        private void SetRecCurrency(DalRecordBase dalRecord, string cType)
        {
            _currentTable.SetValue(dalRecord.RecordName.Replace('_', '-').Trim());
            dalRecord.IDColumnValue = Convert.ToInt64(dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][dalRecord.IDColumnName]);
            //Update DalRecords collection
            if (CurrentDalRecords.Keys.Contains(dalRecord.RecordName))
                CurrentDalRecords[dalRecord.RecordName] = dalRecord;
            else
                CurrentDalRecords.Add(dalRecord.RecordName, dalRecord);

            RecordCurrency _reccurrency = dalRecord.CurrentRecord;
            if (_reccurrency != null)
            {
                //*** Set Conversation currency ***
                UpdateCurrentConversation(dalRecord.RecordName, Convert.ToInt64(dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][_reccurrency.IdColName]));

                //*** Set Table currencies ***
                string[] keyarray = new string[_reccurrency.CurrencyKeys.Keys.Count];
                _reccurrency.CurrencyKeys.Keys.CopyTo(keyarray, 0);
                foreach (string skey in keyarray)
                {
                    if (dalRecord.dt.Columns.Contains(skey))
                        _reccurrency.CurrencyKeys[skey] = dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][skey];

                    // added because of issue 5334. July 9, 2015
                    else
                        _reccurrency.CurrencyKeys[skey] = dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][_reccurrency.IdColName];
                }
                _reccurrency.RecordActionCode = RowStatus.GoodRow;

                //*** Set List Currencies
                if (cType == STR_All)
                {
                    foreach (string lkey in _reccurrency.ListNames.Keys)
                    {
                        SetListCurrency(dalRecord, lkey, _reccurrency.ListNames[lkey]);
                    }
                    if (dalRecord.CurrentList != null && !string.IsNullOrEmpty(dalRecord.CurrentList.ListName))
                        _currentList.SetValue(dalRecord.CurrentList.ListName);
                }
                else if (cType.Length > 0) // contains list name
                    SetListCurrency(dalRecord, cType, _reccurrency.ListNames[cType]);
            }
        }

        /// <summary>
        /// Set List currency from current row 
        /// </summary>
        /// <param name="dalRecord"></param>
        /// <param name="listname"></param>
        /// <param name="currtype"></param>
        private void SetListCurrency(DalRecordBase dalRecord, string listname, string currtype)
        {
            ListCurrency listcurrency = null;
            _currentList.SetValue(listname);

            if (_currentDBCurrency.ListTable.ContainsKey(listname))
            {
                listcurrency = _currentDBCurrency.ListTable[listname];
            }

            if (listcurrency != null)
            {
                // if record is not included in the set, do not need to update set currencies - ticket 5453
                if (listcurrency.ListFkName != null
                    && dalRecord.dt.Columns.Contains(listcurrency.ListFkName)
                    && dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][listcurrency.ListFkName] == DBNull.Value)
                {
                    return;
                }

                if (currtype == STR_Member)
                {

                    if (listcurrency.MemberCur == null || listcurrency.MemberCur.RecordName != dalRecord.RecordName)
                    {
                        listcurrency.MemberCur = (RecordCurrency)_currentDBCurrency.RecordTable[dalRecord.RecordName].Clone();
                    }
                    listcurrency.MemberCur.RecordActionCode = RowStatus.GoodRow;
                    listcurrency.ListPositionCode = ListStatus.OnMemberRow;
                    listcurrency.ListActionCode = RowStatus.GoodRow;

                    // if record points to the owner, which is not current owner of the set, 
                    // then do not update the set (for GetLatest()) as the record does not participate in the current instance of the set - ticket 6505
                    if (_getLatest && listcurrency.OwnerCur != null)
                    {
                        if (listcurrency.OwnerCur.CurrencyKeys[listcurrency.ListFkName] != null
                            && dalRecord.CurrentRecord.CurrencyKeys[listcurrency.ListFkName] != null
                            //&& listcurrency.ListOpt != ListOptions.MA    //Update for issue 9001
                            && Convert.ToInt64(listcurrency.OwnerCur.CurrencyKeys[listcurrency.ListFkName]) != Convert.ToInt64(dalRecord.CurrentRecord.CurrencyKeys[listcurrency.ListFkName]))
                            return;
                    }

                    string[] keyarray = new string[listcurrency.MemberCur.CurrencyKeys.Keys.Count];
                    listcurrency.MemberCur.CurrencyKeys.Keys.CopyTo(keyarray, 0);
                    foreach (string skey in keyarray)
                    {
                        if (dalRecord.dt.Columns.Contains(skey))
                            listcurrency.MemberCur.CurrencyKeys[skey] = dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][skey];
                    }
                    if (listcurrency.OwnerCur != null)
                    {
                        // Check for null value in Foreign key - Not connected to set 
                        if (dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][listcurrency.ListFkName] == DBNull.Value)
                        {
                            listcurrency.ListPositionCode = ListStatus.OnNone;
                            listcurrency.ListActionCode = RowStatus.NoRow;
                        }
                        else
                        {
                            listcurrency.OwnerCur.CurrencyKeys[listcurrency.ListFkName] = dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][listcurrency.ListFkName];
                            // commented out as a fix to ticket 4360
                            //listcurrency.OwnerCur.CurrencyKeys[listcurrency.OwnerCur.IdColName] = dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][listcurrency.ListFkName];
                            listcurrency.MemberCur.RecordActionCode = RowStatus.GoodRow;
                            listcurrency.ListPositionCode = ListStatus.OnMemberRow;
                            listcurrency.ListActionCode = RowStatus.GoodRow;

                            // Update junction table on sorted sets when row is modified - Issue 6234
                            if (_updateRecord && listcurrency.ListOpt != ListOptions.MA && listcurrency.ListOrd == ListOrder.SORTED
                                && !string.IsNullOrEmpty(listcurrency.JunctionTableName))
                            {
                                _updateRecord = false;
                                long ownerCurId = Convert.ToInt64(dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][listcurrency.ListFkName]);
                                ExcludeFromList(dalRecord, listname);
                                IncludeInList(dalRecord, listname, ownerCurId);
                                //If datatable rows got cleared, we obtain the row - issue 6489
                                if (dalRecord.dt.Rows.Count == 0)
                                {
                                    GetByIdCol(dalRecord, dalRecord.IDColumnValue, DBReturnData.No);
                                }
                                _updateRecord = true;
                            }
                        }
                    }
                    else
                    {
                        // System owned Optional set to see if member is connected 
                        if (listcurrency.ListOpt != ListOptions.MA)
                        {
                            string connectedFlag = string.Empty;
                            if (dalRecord.dt.Columns.Contains(string.Concat(listname, "_MEMIND")))
                            {
                                if (dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][string.Concat(listname, "_MEMIND")] != DBNull.Value)
                                    connectedFlag = (string)dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][string.Concat(listname, "_MEMIND")];
                            }
                            else if (dalRecord.dt.Columns.Contains(string.Concat(listname, "_SEQ")))
                            {
                                if (dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][string.Concat(listname, "_SEQ")] != DBNull.Value)
                                    connectedFlag = "Y"; ;
                            }

                            if (connectedFlag != "Y")
                            {
                                listcurrency.ListPositionCode = ListStatus.OnNone;
                                listcurrency.ListActionCode = RowStatus.NoRow;
                                return;
                            }
                            if (_updateRecord && listcurrency.ListOrd == ListOrder.SORTED && !string.IsNullOrEmpty(listcurrency.JunctionTableName))
                            {
                                _updateRecord = false;
                                ExcludeFromList(dalRecord, listname);
                                IncludeInList(dalRecord, listname);
                                //If datatable rows got cleared, we obtain the row - issue 6489
                                if (dalRecord.dt.Rows.Count == 0)
                                {
                                    GetByIdCol(dalRecord, dalRecord.IDColumnValue, DBReturnData.No);
                                }
                                _updateRecord = true;
                            }
                        }
                        listcurrency.MemberCur.RecordActionCode = RowStatus.GoodRow;
                        listcurrency.ListPositionCode = ListStatus.OnMemberRow;
                        listcurrency.ListActionCode = RowStatus.GoodRow;
                    }
                }
                else if (currtype == STR_Owner)
                {
                    string[] keyarray = new string[listcurrency.OwnerCur.CurrencyKeys.Keys.Count];
                    listcurrency.OwnerCur.CurrencyKeys.Keys.CopyTo(keyarray, 0);
                    foreach (string skey in keyarray)
                    {
                        if (dalRecord.dt.Columns.Contains(skey) && listcurrency.OwnerCur.CurrencyKeys.ContainsKey(skey))
                        {
                            listcurrency.OwnerCur.CurrencyKeys[skey] = dalRecord.dt.Rows[dalRecord.DataTableCurrentRow][skey];
                        }

                        // code added because of issue 5346. July 8, 2015
                        // added dropCurrencies flag because of issue 5445. July 13, 2015
                        if (_canDropCurrencies && skey == listcurrency.ListFkName)
                        {
                            if (listcurrency.ListPositionCode == ListStatus.OnMemberRow && // added for ticket 9490, 
                                listcurrency.MemberCur != null && listcurrency.MemberCur.CurrencyKeys.ContainsKey(skey)
                                //added check for int to correct issue 5507 caused by prior fix - 07-14-2015
                                && listcurrency.MemberCur.CurrencyKeys[skey] != null && listcurrency.MemberCur.CurrencyKeys[skey] is int
                                && Convert.ToInt64(listcurrency.MemberCur.CurrencyKeys[skey]) != Convert.ToInt64(listcurrency.OwnerCur.CurrencyKeys[skey]))
                            {
                                string[] mkeys = new string[listcurrency.MemberCur.CurrencyKeys.Count];
                                listcurrency.MemberCur.CurrencyKeys.Keys.CopyTo(mkeys, 0);
                                foreach (string mkey in mkeys)
                                    listcurrency.MemberCur.CurrencyKeys[mkey] = null;

                                //if (_currentDBCurrency.RecordTable.ContainsKey(listcurrency.MemberCur.RecordName))
                                //{
                                //    RecordCurrency recordCurrency = _currentDBCurrency.RecordTable[listcurrency.MemberCur.RecordName];
                                //    if (recordCurrency != null && recordCurrency.CurrencyKeys.ContainsKey(skey)
                                //        && recordCurrency.CurrencyKeys[skey] != null && recordCurrency.CurrencyKeys[skey] is int
                                //        && (int)recordCurrency.CurrencyKeys[skey] != (int)listcurrency.OwnerCur.CurrencyKeys[skey])
                                //    {
                                //        foreach (string mkey in mkeys)
                                //            recordCurrency.CurrencyKeys[mkey] = null;
                                //    }
                                //}

                                DropSubsequentListCurrencies(listcurrency.MemberCur, listname);
                            }
                        }
                    }
                    listcurrency.OwnerCur.RecordActionCode = RowStatus.GoodRow;
                    listcurrency.ListPositionCode = ListStatus.OnOwnerRow;
                    listcurrency.ListActionCode = RowStatus.GoodRow; // ticket 5805
                }

            }
        }

        private void DropSubsequentListCurrencies(RecordCurrency memberFromParentList, string dropListName)
        {
            if (memberFromParentList.ListNames == null) return;

            foreach (string listName in memberFromParentList.ListNames.Keys)
            {
                if (memberFromParentList.ListNames[listName] == "Owner")
                {
                    if (!_currentDBCurrency.ListTable.ContainsKey(listName)) continue;

                    ListCurrency listCurrency = _currentDBCurrency.ListTable[listName];
                    if (listCurrency == null) continue;

                    if (listCurrency.MemberCur == null) continue;

                    string[] mkeys = new string[listCurrency.OwnerCur.CurrencyKeys.Count];
                    listCurrency.OwnerCur.CurrencyKeys.Keys.CopyTo(mkeys, 0);
                    foreach (string mkey in mkeys)
                        listCurrency.OwnerCur.CurrencyKeys[mkey] = null;

                    if (listCurrency.ListPositionCode == ListStatus.OnMemberRow)
                    {
                        mkeys = new string[listCurrency.MemberCur.CurrencyKeys.Count];
                        listCurrency.MemberCur.CurrencyKeys.Keys.CopyTo(mkeys, 0);
                        foreach (string mkey in mkeys)
                            listCurrency.MemberCur.CurrencyKeys[mkey] = null;

                        listCurrency.OwnerCur.RecordActionCode = RowStatus.NoRow;
                        listCurrency.ListPositionCode = ListStatus.OnNone;
                        listCurrency.ListActionCode = RowStatus.NoRow;

                        DropSubsequentListCurrencies(listCurrency.MemberCur, dropListName);
                    }
                    else
                    {
                        listCurrency.OwnerCur.RecordActionCode = RowStatus.NoRow;
                        listCurrency.ListPositionCode = ListStatus.OnNone;
                        listCurrency.ListActionCode = RowStatus.NoRow;
                    }

                }
                else if (memberFromParentList.ListNames[listName] == "Member")
                {
                    if (dropListName == listName) continue;
                    if (!_currentDBCurrency.ListTable.ContainsKey(listName)) continue;

                    ListCurrency listCurrency = _currentDBCurrency.ListTable[listName];
                    if (listCurrency == null) continue;

                    if (listCurrency.MemberCur == null) continue;
                    if (listCurrency.MemberCur.RecordName != memberFromParentList.RecordName) continue;

                    string[] mkeys = new string[listCurrency.MemberCur.CurrencyKeys.Count];
                    listCurrency.MemberCur.CurrencyKeys.Keys.CopyTo(mkeys, 0);
                    foreach (string mkey in mkeys)
                        listCurrency.MemberCur.CurrencyKeys[mkey] = null;
                    //Following check added to make sure no on Owner row - issue 9227
                    if (listCurrency.ListPositionCode == ListStatus.OnMemberRow ||
                        (listCurrency.ListPositionCode == ListStatus.OnNone && listCurrency.ListActionCode == RowStatus.DeletedRow)) // ticket 9393
                    {
                        listCurrency.ListPositionCode = ListStatus.OnNone;
                        listCurrency.ListActionCode = RowStatus.NoRow;
                    }

                }
            }
        }

        /// <summary>
        /// Retrieves database configuration parms
        /// </summary>
        private void GetConfigurationData()
        {
            string defaultSchema = string.Empty;
            try
            {
                MessagePrefix = "DC";
                _providerName = ConfigSettings.GetAppSettingsString("DbConnFactory");
                // .NET Core Change
                if (_providerName.Contains("Oracle"))
                {
                    DbProviderFactories.RegisterFactory(_providerName, OracleClientFactory.Instance);
                    _parmPrefix = ":";
                    _isOracle = true;
                }
                else
                    DbProviderFactories.RegisterFactory(_providerName, SqlClientFactory.Instance);

                _dbFactory = DbProviderFactories.GetFactory(_providerName);
                _dbAccessType = DbAccessType.DynamicSql;

                if (ConfigSettings.GetAppSettingsString("DbAccessType").ToString() == "StoredProc")
                {
                    _dbAccessType = DbAccessType.StoredProcedures;
                    _storeProcedurePrefixName = ConfigSettings.GetAppSettingsString("StoreProcedurePrefixName");
                }
                // .NET Core Change
                string s = ConfigSettings.GetAppSettingsString("DefaultSchema");
                if (!String.IsNullOrEmpty(s))
                {
                    defaultSchema = ConfigSettings.GetAppSettingsString("DefaultSchema");
                }

                _isAreaSweepReverse = ConfigSettings.GetAppSettingsBool("AreaSweepReverse");
            }
            catch (Exception ex)
            {
                throw new Exception("Error with DBFactory/Connection/Config settings", ex);
            }
            if (String.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("DatabaseCacheSize")))
            {
                MaxRows = 20;
            }
            else
            {
                int tempRows = 0;
                if (!int.TryParse(ConfigSettings.GetAppSettingsString("DatabaseCacheSize"), out tempRows))
                {
                    MaxRows = 20;
                }
                else
                    MaxRows = tempRows;
            }

            //Override in JCL for a per step setting
            // //DBCACHE   DD DSN=50,DISP=SHR
            if (!(String.IsNullOrEmpty(Environment.GetEnvironmentVariable("DBCACHE"))))
                MaxRows = int.Parse(Environment.GetEnvironmentVariable("DBCACHE"));

            _sqlCommandUtility = new SQLCommandUtility(_providerName, _dbAccessType, _currentDBCurrency, _storeProcedurePrefixName, MaxRows);

            if (!string.IsNullOrEmpty(defaultSchema))
            {
                _sqlCommandUtility.SchemaName = defaultSchema;
            }

        }

        /// <summary>
        /// Delete database row
        /// </summary>
        /// <param name="dalRecord"></param>
        /// <param name="Erasetype"></param>
        /// <param name="dbExceptions"></param>
        private void DeleteDatabaseRow(DalRecordBase dalRecord, DeleteRowOption Erasetype, params DbAllow[] dbExceptions)
        {

            dalRecord.CurrentRecord = _currentDBCurrency.RecordTable[dalRecord.RecordName];
            dalRecord.DeleteType = Erasetype; dalRecord.DBOperation = DbOperation.Delete;
            CheckCurrentTableId(dalRecord);
            if (RecordReturnCode != 0)
            {
                RecordReturnCode = RecordReturnCode + 200;
                return;
            }
            long saveDbKey = dalRecord.IDColumnValue;
            bool listUpdated = false;
            bool multiMemberDelete = false;
            ListCurrency mmList = null;

            //Check for LinkList sets
            foreach (string lkey in dalRecord.CurrentRecord.ListNames.Keys)
            {
                if (dalRecord.CurrentRecord.ListNames[lkey] == "Owner")
                {
                    if (!_currentDBCurrency.ListTable.ContainsKey(lkey))
                        continue;
                    ListCurrency listcurrency = (ListCurrency)_currentDBCurrency.ListTable[lkey];
                    if (listcurrency == null)
                        continue;
                    listcurrency.ListPositionCode = ListStatus.OnOwnerRow;
                    listcurrency.OwnerCur.CurrencyKeys[listcurrency.ListFkName] = saveDbKey;
                    if (!IsListEmpty(lkey))
                    {
                        RecordReturnCode = 0;
                        if (Erasetype == DeleteRowOption.CascadeNone)
                        {
                            RecordReturnCode = 230;
                            return;
                        }
                        else
                        {
                            if (_ownerDalRecord != dalRecord)
                            {
                                _ownerDalRecord = dalRecord;
                                listcurrency.ListPositionCode = ListStatus.OnOwnerRow; //Reset to owner to ensure correct owner Id on lists
                                DeleteMembersInList(listcurrency, Erasetype);
                                RecordReturnCode = 0;

                            }
                        }
                    }
                    listcurrency.ListPositionCode = ListStatus.OnNone;
                    listcurrency.ListActionCode = RowStatus.DeletedRow;
                    listcurrency.OwnerCur.DropCurrencyKeys();
                }
                else if (dalRecord.CurrentRecord.ListNames[lkey] == "Member")
                {
                    if (!_currentDBCurrency.ListTable.ContainsKey(lkey))
                        continue;

                    ListCurrency listcurrency = (ListCurrency)_currentDBCurrency.ListTable[lkey];

                    if (listcurrency == null)
                        continue;

                    if (!IsListMember(lkey))
                    {
                        RecordReturnCode = 0;
                        continue;
                    }

                    if (listcurrency.ListOpt != ListOptions.MA || listcurrency.ListOrd == ListOrder.LinkList)
                    {
                        SetDeletedPriorNextIdColValues(dalRecord, saveDbKey, listcurrency, lkey);
                        ExcludeFromList(dalRecord, lkey);
                        listUpdated = true;
                    }
                    else if (listcurrency.ListOpt == ListOptions.MA && listcurrency.ListOrd == ListOrder.SORTED && listcurrency.ListPositionCode == ListStatus.OnMemberRow)
                    {
                        SetDeletedPriorNextIdColValues(dalRecord, saveDbKey, listcurrency, lkey);
                        UpdateListStatus(lkey, RowStatus.DeletedRow, ListStatus.OnNone);
                    }
                    else if (listcurrency.MemberList != null && listcurrency.MemberList.Count > 1)
                    {
                        multiMemberDelete = true;
                        mmList = listcurrency;
                    }
                }
            }
            if (listUpdated)
            {
                // issue 5445: can drop currencies only when GetByKey is called directly from converted code. July 13, 2015
                bool canDropCurrencies = _canDropCurrencies;
                _canDropCurrencies = false;
                _updateLists = false;
                GetByIdCol(dalRecord, saveDbKey, DBReturnData.No);
                _canDropCurrencies = canDropCurrencies;
                _updateLists = true;

                if (RecordReturnCode == 326)
                {
                    RecordReturnCode = 0;
                    return;
                }
            }

            CheckForConnection();
            SetTransaction();

            SetUpSqlCommand(dalRecord, "Delete");

            try
            {
                if (RecordReturnCode == 0)
                {
                    _returnCode = _command.ExecuteNonQuery();
                }
                //_returnCode = (int)_command.Parameters[_returnCodeName].Value;
                if (_returnCode > 0)
                {
                    RecordReturnCode = 0;
                    UpdateRecStatus(dalRecord.RecordName, RowStatus.DeletedRow);
                    _currentDBCurrency.CurrentIdCol = 0;

                    dalRecord.SetCurrentRowAsDeleted();

                    if (multiMemberDelete)
                    {
                        dalRecord.CurrentList = mmList;
                        SetUpSqlCommand(dalRecord, "DeleteMMHeader");
                        _returnCode = _command.ExecuteNonQuery();
                    }
                    //if (dalRecord.CurrentList == null) <- ??? some owner records cannot be deleted
                    _ownerDalRecord = null;


                }
                else
                {
                    RecordReturnCode = 213;
                }
                if (_isAutoStatus && !_errorStatusRecord.StatusGood.Value)
                    CheckAutostatusExceptions(dbExceptions);
                if (RecordReturnCode != 0)
                {
                    ErrorTable.SetValue(dalRecord.TableName);
                }
            }

            catch (Exception ex)
            {
                Rollback("");
                Connection.Close();

                throw new ApplicationException(String.Format("DBRecordBase.DeleteDBRow() failed, EraseType: {0}{1}", Erasetype, ex.Message), ex);
            }
        }

        /// <summary>
        /// Deletes all members in a list
        /// </summary>
        /// <param name="listCurrency"></param>
        /// <param name="eraseType"></param>
        private void DeleteMembersInList(ListCurrency listCurrency, DeleteRowOption eraseType)
        {
            GetInList(RowPosition.First, listCurrency.ListName, DBReturnData.No, null);

            while (RecordReturnCode == 0)
            {
                if ((listCurrency.ListOpt == ListOptions.OA || listCurrency.ListOpt == ListOptions.OM) && eraseType == DeleteRowOption.CascadePermanent)
                {
                    ExcludeFromList(CurrentDalRecords[listCurrency.MemberCur.RecordName], listCurrency.ListName);
                }
                else
                {
                    DeleteDatabaseRow(CurrentDalRecords[listCurrency.MemberCur.RecordName], eraseType);
                }
                if (RecordReturnCode != 0)
                    return;

                if (!(listCurrency.ListOpt == ListOptions.OM && listCurrency.JunctionTableID != null))
                    GetInList(RowPosition.First, listCurrency.ListName, DBReturnData.No, null);
            }
        }

        /// <summary>
        /// Sets Next and Prior pointers for when deletin rows
        /// </summary>
        /// <param name="dalRecord"></param>
        /// <param name="savedDbKey"></param>
        /// <param name="listCurrency"></param>
        /// <param name="listName"></param>
        private void SetDeletedPriorNextIdColValues(DalRecordBase dalRecord, long savedDbKey, ListCurrency listCurrency, string listName)
        {
            // get prior and next currency values for future navigation and mark member currency as deleted - ticket 5651
            if (listCurrency.ListPositionCode == ListStatus.OnMemberRow && listCurrency.MemberCur.RecordActionCode == RowStatus.GoodRow)
            {
                bool canDropCurrencies = _canDropCurrencies;
                _updateLists = false;
                _canDropCurrencies = false;
                try
                {
                    GetInList(RowPosition.Prior, listName, DBReturnData.No);
                    long tmpIdColVal = listCurrency.ListPositionCode != ListStatus.OnMemberRow || RecordReturnCode == 307
                        ? 0
                        : listCurrency.MemberCur.CurrencyKeys[listCurrency.MemberCur.IdColName] == null
                            ? 0
                            : Convert.ToInt64(listCurrency.MemberCur.CurrencyKeys[listCurrency.MemberCur.IdColName]);

                    dalRecord.LastListName = listName;
                    GetByIdCol(dalRecord, savedDbKey, DBReturnData.No);

                    listCurrency.MemberCur.DeletedPriorIdColValue = tmpIdColVal;

                    GetInList(RowPosition.Next, listName, DBReturnData.No);
                    tmpIdColVal = listCurrency.ListPositionCode != ListStatus.OnMemberRow || RecordReturnCode == 307
                        ? 0
                        : listCurrency.MemberCur.CurrencyKeys[listCurrency.MemberCur.IdColName] == null
                            ? 0
                            : Convert.ToInt64(listCurrency.MemberCur.CurrencyKeys[listCurrency.MemberCur.IdColName]);

                    dalRecord.LastListName = listName;
                    GetByIdCol(dalRecord, savedDbKey, DBReturnData.No);

                    listCurrency.MemberCur.DeletedNextIdColValue = tmpIdColVal;
                    listCurrency.MemberCur.RecordActionCode = RowStatus.DeletedRow;
                }
                finally
                {
                    _updateLists = true;
                    _canDropCurrencies = canDropCurrencies;
                }
            }
        }

        /// <summary>
        /// Returns new instance of Dal record
        /// </summary>
        /// <param name="recTypeName"></param>
        /// <returns></returns>
        private DalRecordBase GetInstanceOfDalRecord(string recTypeName)
        {
            string newAssemblyName = DALAssemblyName;
            string recordName = recTypeName;
            if (recordName.Contains("."))
            {
                newAssemblyName = recTypeName.Substring(0, recordName.LastIndexOf("."));
            }
            else
            {
                recordName = string.Concat(DALAssemblyName, ".DAL_", recordName);
            }
            Type type = null;
            Assembly targetAsm;

            // Go get a reference to the assembly
            targetAsm = FindAssembly(newAssemblyName);
            if (targetAsm == null)
            {
                targetAsm = FindAssembly(DALAssemblyName);
                if (targetAsm == null)
                {
                    throw new ApplicationException(String.Format("InvokeHasMainMethod: Assembly [{0}] not found", newAssemblyName));
                }
            }

            // Get the type reference using a name of "NameSpace.TypeName"
            type = targetAsm.GetType(recordName);

            DalRecordBase dalRec = (DalRecordBase)Activator.CreateInstance(type);

            return dalRec;
        }

        /// <summary>
        /// Find assembly for reflextion
        /// </summary>
        /// <param name="assemblyShortName"></param>
        /// <returns></returns>
        private static Assembly FindAssembly(string assemblyShortName)
        {
            Assembly localAssembly;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.GetName(false).Name == assemblyShortName)
                {
                    return assembly;
                }
            }
            // If not in application domain, try to load from base directory (For batch operations)
            try
            {
                localAssembly = Assembly.Load(assemblyShortName);
                return localAssembly;
            }
            catch
            {
                return null;
            }
        }

        //private bool GetUtilityDBConnection()
        //{

        //    try
        //    {
        //        string connectStringKey = "UtilityDB";
        //        connectStringKey = ConfigurationManager.AppSettings["ConnectionStringKey"].ToString();
        //        IConnectionString _connectionStringInfo = GetDBConnectionInfo();
        //        _connectionString = _connectionStringInfo.GetConnectionString(connectStringKey);
        //    }
        //    catch
        //    {
        //        _connectionString = ConfigurationManager.ConnectionStrings["UtilityDBConnectionString"].ToString();
        //    }

        //    try
        //    {
        //        if (_connection == null)
        //        {
        //            _connection = _dbFactory.CreateConnection();
        //        }
        //        _connection.ConnectionString = ConnectionString;

        //        _connection.Open();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// Initialize DBConversation fields
        /// </summary>
        private void InitFields()
        {

            _errorStatusRecord = new ErrorStatusRecord();
            _errorStatusRecord.Record.ResetToInitialValue();

            InternalRecord = BufferServices.Factory.NewRecord("CurrentDB", def =>
            {
                _currentTable = def.CreateNewField("currentTable", FieldType.String, 32, " ");
                _currentList = def.CreateNewField("currentList", FieldType.String, 32, " ");

            });
            _currentTable = InternalRecord.GetFieldByName("currentTable");
            _currentList = InternalRecord.GetFieldByName("currentList");
            CurrentDalRecords = new Dictionary<string, DalRecordBase>();

        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("The binary key cannot have an odd number of digits: {0}", hexString);
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

        /// <summary>
        /// Returns IConnectionStrng implementation
        /// </summary>
        /// <returns></returns>
        private static IConnectionString GetDBConnectionInfo()
        {
            try
            {
                IConnectionString tempConnectionString = InversionContainer.GetImplementingObject<IConnectionString>();
                return tempConnectionString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

    }
}
