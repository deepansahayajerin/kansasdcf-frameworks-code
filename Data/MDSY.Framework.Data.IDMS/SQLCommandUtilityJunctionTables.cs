using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using MDSY.Framework.Core;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Data.IDMS
{
    /// <summary>
    /// SQLCommandUtility class is a support class for DBConversation class. 
    /// It generates SQL statements for IDMS DAL commands and creates debug log messages with SQL text and parameter values. 
    /// Instance of this class is created when DBConversation instance is created.
    /// </summary>
    public class SQLCommandUtilityJunctionTables
    {
        #region private variables
        private const string STR_RowID = "TblRowID";
        private const string STR_CurrentRowID = "CurrentRowID";
        private const string STR_ListOwnerID = "ListOwnerID";
        private const string STR_JunctionTableID = "JunctionTableID";
        private const string STR_GreaterThan = ">";
        private const string STR_GreaterThanEqualTo = ">=";
        private const string STR_LessThan = "<";
        private const string STR_LessThanEqualTo = "<=";
        private const string STR_EqualTo = " = ";
        private const string STR_MaxRows = "MaxRows";
        private const string STR_Desc = " Desc";
        private const string STR_StartRow = "StartRow";
        private const string STR_EndRow = "EndRow";
        private const string STR_StartID = "StartID";
        private const string STR_RowNumber = "RowNumber";
        private const string STR_ListName = "ListName";
        private const string STR_ListKeyName = "ListKeyName";
        private const string STR_ListMemberName = "ListMemberName";
        private const string STR_SelectType = "SelectType";
        private const string STR_UpdateType = "UpdateType";
        private const string STR_DeleteType = "DelType";
        private const string STR_OwnerID = "OwnerID";
        private const string STR_WhereCriteria = "WhereCriteria";
        private const string STR_PriorPointerSuffix = "_P";
        private const string STR_NextPointerSuffix = "_N";
        private const string STR_BusinessKeys = "BusinessKeys";
        private const string STR_LatestID_SQLServer = "; Select Scope_Identity();";
        private const string STR_LatestID_Oracle = "; SELECT {0}.CURRVAL as id From DUAL;";
        private const string STR_LatestID_DB2 = "; SELECT IDENTITY_VAL_LOCAL() FROM SYSIBM.SYSDUMMY1;";
        private const string STR_AREAID = "AREAID";
        private const string STR_RECID = "RECID";
        private const string STR_NEXTVALUEFOR = "NEXT VALUE FOR ";
        private const string STR_DBS_ID_COL = "DBS_ID_COL";
        private const string STR_Period = ".";
        private const string STR_Comma = ",";
        private const string STR_InnerJoin = " inner join ";
        private const string STR_On = " on ";
        private const string STR_KeyProgression = "**KEYPROGRESSION**";
        private string parmRowID = "@TblRowID";
        private string parmListOwnerID = "@ListOwnerID";
        private string parmJunctionTableID = "@JunctionTableID";
        private string parmStartID = "@StartID";
        private string parmRowNumber = "@RowNumber";
        // private string parmMaxRows = "@MaxRows";
        private string parmQueueData = "@QueueData";
        private string parmTop = "Top({0})";
        private string parmTop1 = "Top(1)";
        private string parmFetch = " Fetch First {0} rows only ";
        private string parmFetch1 = " Fetch First 1 rows only ";
        private string parmWhereRowNum = " and RowNum <= {0} ";
        private string parmWhereRowMatchesID = "{0} (Select row from crow where crow.{1} = {2}) ";
        private string parmWhereRowStart = ">= 0 ";
        private string parmTable = "table";
        private string parmOrder = ">";
        private string parmSortOrder = "ASC";
        private string parmSortOrderDesc = "DESC";
        private string parmSortOrderAsc = "ASC";
        private string parmListPointerNext = "@Next";
        private string parmListPointerPrior = "@Prior";
        private string junctionTableIDColumn = "DBS_ID_COL";
        private string parmPrefix = "@";
        private string _dbProviderName;
        private DbAccessType _dBAccessType;
        private string _spPrefix;
        private string _linkListKey;
        private string _linkListKeyOpp;
        private int _maxRows;
        private string _queryType;
        private string _utilitiesSchemaName;
        private string _viewName;
        private DalRecordBase _dalRecord;
        bool _doLogging = false;
        bool _logErrorsOnly = false;
        private static IIdentityColumnInfo _IdentityColumnInfoHandler;
        private static string _dateTimeStampName;

        #endregion

        #region Public Properties
        public DBCurrency CurrentDbCurrency { get; set; }
        public static IIdentityColumnInfo IdentityColumnInfoHandler
        {
            get
            {
                if (_IdentityColumnInfoHandler == null)
                {
                    _IdentityColumnInfoHandler = GetIdentityColumnInfoHandlerObject();
                }

                return _IdentityColumnInfoHandler;
            }
        }

        public static string DateTimeStampName
        {
            get
            {
                if (string.IsNullOrEmpty(_dateTimeStampName))
                    _dateTimeStampName = ConfigSettings.GetAppSettingsString("DateTimeStampName");
                return _dateTimeStampName;
            }
        }

        public string SchemaName { get; set; }
        #endregion

        #region Constructor
        public SQLCommandUtilityJunctionTables(string dbProviderName, DbAccessType dbAccessType, DBCurrency dBCurrency, string storedProcPrefix, int maxRows)
        {
            _dBAccessType = dbAccessType;
            _spPrefix = storedProcPrefix;
            _maxRows = maxRows;
            CurrentDbCurrency = dBCurrency;
            _dbProviderName = dbProviderName;
            //if (dbProviderName.Contains("Oracle"))
            //{
            //    parmFetch = string.Empty; parmFetch1 = string.Empty; parmTable = string.Empty;
            //    parmTop = string.Empty; parmTop1 = string.Empty;
            //    parmWhereRowNum = string.Format(parmWhereRowNum, _maxRows.ToString());
            //}
            if (dbProviderName.ToUpper().Contains("ORACLE") || dbProviderName.Contains("DB2"))
            {
                parmTop = string.Empty; parmTop1 = string.Empty;
                parmWhereRowNum = string.Empty;
                parmFetch = string.Format(parmFetch, _maxRows.ToString());
                if (dbProviderName.ToUpper().Contains("ORACLE"))
                {
                    parmRowID = parmRowID.Replace('@', ':');
                    parmListOwnerID = parmListOwnerID.Replace('@', ':');
                    parmJunctionTableID = parmJunctionTableID.Replace('@', ':');
                    parmStartID = parmStartID.Replace('@', ':');
                    parmRowNumber = parmRowNumber.Replace('@', ':');
                    parmQueueData = parmQueueData.Replace('@', ':');
                    parmListPointerNext = parmListPointerNext.Replace('@', ':');
                    parmListPointerPrior = parmListPointerPrior.Replace('@', ':');
                    parmTable = string.Empty;
                }
            }
            else
            {
                parmFetch = string.Empty; parmFetch1 = string.Empty; parmTable = string.Empty;
                parmWhereRowNum = string.Empty;
                parmTop = string.Format(parmTop, _maxRows.ToString());
            }
            _utilitiesSchemaName = ConfigSettings.GetAppSettingsString("UtilitiesSchemaName");

            _doLogging = ConfigSettings.GetAppSettingsBool("LogFileEnabled");

            _logErrorsOnly = ConfigSettings.GetAppSettingsBool("LogFileErrorOnly");

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a SQL command text based on the provided query type.
        /// </summary>
        /// <param name="dalRecord">Specifies an instance of record, which command object needs to be updated with SQL command text.</param>
        /// <param name="sqlType">specifies type of SQL command. It can take the following values:
        ///	            SelectByByskey, SelectByID,	SelectInList, SelectInListByRowNumber, SelectInListUsing, GetLastInsertedRow, Update, UpdateList, Delete and Insert</param>
        public void SetUpCommandString(DalRecordBase dalRecord, string sqlType, bool inGetInListByKey = false)
        {
            _dalRecord = dalRecord; _queryType = sqlType;
            if (_dBAccessType == DbAccessType.StoredProcedures)
            {
                _dalRecord.Command.CommandType = CommandType.StoredProcedure;
                _dalRecord.Command.CommandText = string.Concat(_spPrefix, _dalRecord.TableName, "_", _queryType);

            }
            else
            {
                _dalRecord.Command.CommandType = CommandType.Text;
            }

            switch (sqlType)
            {
                case "SelectByBuskey":
                    SetViewName(); SelectByBuskey(); break;
                case "SelectByID":
                    SetViewName(); SelectByID(); break;
                case "SelectInList":
                    SetViewName(); SelectInList(inGetInListByKey); break;
                case "SelectInListByRowNumber":
                    SetViewName(); SelectInListByRowNumber(); break;
                case "SelectInListUsing":
                    SetViewName(); SelectInListUsing(); break;
                case "GetLastInsertedRow":
                    SetViewName(); GetLastInsertedRow(); break;
                case "Update":
                    Update(); break;
                case "UpdateList":
                    UpdateList(); break;
                case "Delete":
                    Delete(); break;
                case "Insert":
                    Insert(); break;
                default:
                    break;
            }

            #region Create Debug Log Message
            if (_doLogging && !_logErrorsOnly)
            {
                string commandtext = string.Empty;
                if (!string.IsNullOrEmpty(_dalRecord.Command.CommandText))
                    commandtext = _dalRecord.Command.CommandText;
                else
                {
                    foreach (string sqlString in _dalRecord.MultipleKeysSqlList)
                    {
                        commandtext += string.Concat(sqlString, "\r\n");
                    }
                }

                StringBuilder sbParms = new StringBuilder();
                foreach (DbParameter param in _dalRecord.Command.Parameters)
                {
                    if ((param.ParameterName != null))
                        sbParms.AppendFormat("       {0}={1},\r\n", param.ParameterName, param.Value);
                }
                commandtext += "\r\n" + sbParms.ToString();
                LogMessageToFile(commandtext);
            }
            #endregion
        }

        /// <summary>
        /// Create a sql commandText string for query against utilities database.
        /// </summary>
        /// <param name="command">Specifies an instance of command object, which should be updated with SQL text.</param>
        /// <param name="sqlType">Specifies type of SQL statement. It can take the following values: SelectQueue, PutQueue, DeleteQueue</param>
        /// <param name="queueName">Specifies queue name.</param>
        /// <param name="queueData">Specifies queue data.</param>
        /// <param name="queuePosition">Specifies position of the queue.</param>
        /// <param name="queueRowID">Specifies queue row ID.</param>
        /// <param name="queueRetention">Specifies queue retention.</param>
        public void SetUpUtilityCommandString(DbCommand command, string sqlType, string queueName, string queueData,
            RowPosition queuePosition, int queueRowID, int queueRetention)
        {
            string utilityTablePrefix = string.Empty;
            if (_utilitiesSchemaName == null || _utilitiesSchemaName.Trim() == string.Empty)
            {
                utilityTablePrefix = string.Empty;
            }
            else
            {
                utilityTablePrefix = string.Concat(_utilitiesSchemaName, STR_Period);
            }
            if (sqlType == "SelectQueue")
            {
                command.CommandText = string.Format("SELECT *  FROM {0}UTL_QUE_DET where AREAID = '{1}'  order by RecID", utilityTablePrefix, queueName);
            }
            else if (sqlType == "PutQueue")
            {

                DbParameter param = command.CreateParameter();
                param.ParameterName = parmQueueData;
                //param.Value = queueData.Replace("'", "''"); issue 6035 an additional single quote is added.
                param.Value = queueData;
                command.Parameters.Add(param);

                command.CommandText = string.Format(string.Concat("Insert into {0}UTL_QUE_HDR ( AREAID, USERID, TERMID, SYSID, USAGE_IND, CURR_RECID, RETENTION_DAYS, DATE_CREATED) ",
                    "Values ( '{1}', '{2}', '0', '0', '0', {3}, {4}, '{5}') ;  ",
                    " Insert into {0}UTL_QUE_DET (AREAID, USERID, TERMID, SYSID, RECID, KEY_NUM, REC_DATA) ",
                    "Values ( '{1}', '{2}', '0', '0', {3}, {6}, {7} ) ;"),
                utilityTablePrefix, queueName, "QUEUE", queueRowID, queueRetention, DateTime.Now.ToShortDateString(), "0", parmQueueData);
            }
            else if (sqlType == "DeleteQueue")
            {
                command.CommandText = string.Format("Delete FROM {0}UTL_QUE_DET where AREAID = '{1}' ; Delete FROM {0}UTL_QUE_HDR where AREAID = '{1}' ; ", utilityTablePrefix, queueName);
            }

            if (_doLogging && !_logErrorsOnly)
            {
                string commandtext = command.CommandText;

                StringBuilder sbParms = new StringBuilder();

                foreach (DbParameter param in command.Parameters)
                {
                    if ((param.ParameterName != null))
                        sbParms.AppendFormat("       {0}={1},\r\n", param.ParameterName, param.Value);
                }
                commandtext += "\r\n" + sbParms.ToString();
                LogMessageToFile(commandtext);
            }
        }

        /// <summary>
        /// Create sql commandText for list check commands
        /// </summary>
        /// <param name="listCurrency"></param>
        /// <param name="command"></param>
        public void SetUpCommandForListCheck(ListCurrency listCurrency, DbCommand command)
        {
            long ownerID = CurrentDbCurrency.GetListOwnerID(listCurrency.ListName);
            SetUpCommandForListCheck(listCurrency, command, ownerID);
        }

        /// <summary>
        /// Creates SQL statements for set’s check commands.
        /// </summary>
        /// <param name="listCurrency">Specifies current instance of the set.</param>
        /// <param name="command">Specifies an instance of database command, which should take generated SQL statement.</param>
        public void SetUpCommandForListCheck(ListCurrency listCurrency, DbCommand command, long ownerID)
        {

            if (_dBAccessType == DbAccessType.StoredProcedures)
            {
                AddCommandParameter(STR_ListName, listCurrency.ListName);
                AddCommandParameter(STR_ListKeyName, listCurrency.ListFkName);
                AddCommandParameter(STR_ListMemberName, listCurrency.MemberCur.RecordName);
                command.CommandText = string.Concat(_spPrefix, "AUtil_ListEmptyCheck");
                command.CommandType = CommandType.StoredProcedure;
            }
            else
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add(CreateParameter(command, STR_ListOwnerID, ownerID));
                if (listCurrency.MemberList == null || listCurrency.MemberList.Count == 0)
                {
                    //command.CommandText =
                    //    string.Format(" WITH cRow AS (Select Row_Number() over (order by  {0}) as row from {1} where {0} = {2} ) Select *  From  cRow WHERE cRow.row = 1  ",
                    //    listCurrency.ListFkName, listCurrency.MemberCur.TableName, parmListOwnerID);
                    command.CommandText =
                        string.Format(" Select {0} 1 as rownbr from {1} where {2} = {3}  {4} ", parmTop1,
                          CheckForSchema(listCurrency.MemberCur.TableName), listCurrency.ListFkName, parmListOwnerID, parmFetch1);
                }
                else
                //Check List for multi member set
                {
                    //command.CommandText =
                    //string.Format(" WITH cRow AS (Select Row_Number() over (order by  {0}) as row from {1} where {0} = {2} ) Select *  From  cRow WHERE cRow.row = 1  ",
                    // listCurrency.JunctionFkName, listCurrency.JunctionTableName, parmListOwnerID);
                    command.CommandText =
                         string.Format(" Select {0} 1 as rownbr from {1} where {2} = {3}  {4} ", parmTop1,
                         CheckForSchema(listCurrency.JunctionTableName), listCurrency.JunctionFkName, parmListOwnerID, parmFetch1);
                }
            }
            if (_doLogging && !_logErrorsOnly)
            {
                string commandtext = command.CommandText;
                StringBuilder sbParms = new StringBuilder();
                foreach (DbParameter param in command.Parameters)
                {
                    if ((param.ParameterName != null))
                        sbParms.AppendFormat("       {0}={1},\r\n", param.ParameterName, param.Value);
                }
                commandtext += "\r\n" + sbParms.ToString();
                LogMessageToFile(commandtext);
            }
        }

        /// <summary>
        /// Create a sql commandText for query against a list that has multiple members
        /// </summary>
        /// <param name="listCurrency">specifies current instance of the set.</param>
        /// <param name="rowPosition">Specifies position of the queue.</param>
        /// <param name="command">Specifies an instance of database command, which should take generated SQL statement.</param>
        public void SetUpCommandFindMemberInMultiMemberList(ListCurrency listCurrency, RowPosition rowPosition, DbCommand command)
        {
            long ownerID = CurrentDbCurrency.GetListOwnerID(listCurrency.ListName);
            string orderBy = string.Empty;
            string currentKeyWhere = string.Empty;

            if (rowPosition == RowPosition.Prior)
            {
                parmOrder = (_dalRecord.CurrentRecord.RecordActionCode == RowStatus.DeletedRow
                    && (_dalRecord.CurrentList.ListPositionCode == ListStatus.OnNone ||
                       (_dalRecord.CurrentList.ListPositionCode == ListStatus.OnMemberRow &&
                       Convert.ToInt64(_dalRecord.CurrentList.MemberCur.CurrencyKeys[_dalRecord.CurrentList.MemberCur.IdColName])
                        == Convert.ToInt64(_dalRecord.CurrentRecord.CurrencyKeys[_dalRecord.CurrentRecord.IdColName]))))
                ? STR_LessThanEqualTo : STR_LessThan;

            }
            else if (rowPosition == RowPosition.Current)
            {
                parmOrder = STR_GreaterThanEqualTo;
            }
            else  // Default is RowPosition.Next
            {
                parmOrder = (_dalRecord.CurrentRecord.RecordActionCode == RowStatus.DeletedRow
                    && (_dalRecord.CurrentList.ListPositionCode == ListStatus.OnNone
                        || (_dalRecord.CurrentList.ListPositionCode == ListStatus.OnMemberRow
                            && Convert.ToInt64(_dalRecord.CurrentList.MemberCur.CurrencyKeys[_dalRecord.CurrentList.MemberCur.IdColName]) == Convert.ToInt64(_dalRecord.CurrentRecord.CurrencyKeys[_dalRecord.CurrentRecord.IdColName]))))
                ? STR_GreaterThanEqualTo : STR_GreaterThan;
            }

            if (listCurrency.ListOrd == ListOrder.FIRST)
            {
                orderBy = string.Concat(listCurrency.JunctionTableID, " Desc ");
                if (parmOrder.Contains(STR_LessThan))
                    parmOrder = parmOrder.Replace(STR_LessThan, STR_GreaterThan);
                else if (parmOrder.Contains(STR_GreaterThan))
                    parmOrder = parmOrder.Replace(STR_GreaterThan, STR_LessThan);
            }
            else if (listCurrency.ListOrd == ListOrder.LAST)
            {
                orderBy = string.Concat(listCurrency.JunctionTableID, " Asc ");
            }
            else if (listCurrency.ListOrd == ListOrder.SORTED)
            {
                orderBy = listCurrency.MultiMemberSortKey;
                if (listCurrency.ListKeys.Trim().EndsWith(" DESC"))
                {
                    orderBy = string.Concat(orderBy, " Desc ");
                    if (parmOrder.Contains(STR_LessThan))
                        parmOrder = parmOrder.Replace(STR_LessThan, STR_GreaterThan);
                    else if (parmOrder.Contains(STR_GreaterThan))
                        parmOrder = parmOrder.Replace(STR_GreaterThan, STR_LessThan);
                }
            }

            if (rowPosition == RowPosition.Last || rowPosition == RowPosition.Prior)
            {

                if (orderBy.EndsWith(" Desc "))
                {
                    orderBy = orderBy.Replace(" Desc ", " Asc ");
                }
                else if (orderBy.EndsWith(" Asc "))
                {
                    orderBy = orderBy.Replace(" Asc ", " Desc ");
                }
                else
                {
                    orderBy = string.Concat(orderBy, " Desc ");
                }
            }

            if (_dBAccessType == DbAccessType.StoredProcedures)
            {
                AddCommandParameter(STR_ListName, listCurrency.ListName);
                AddCommandParameter(STR_ListKeyName, listCurrency.ListFkName);
                command.CommandText = string.Concat(_spPrefix, "AUtil_ListFindMember");
                command.CommandType = CommandType.StoredProcedure;
            }
            else
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add(CreateParameter(command, STR_ListOwnerID, ownerID));
                //if ((rowPosition == RowPosition.Next || rowPosition == RowPosition.Prior) && listCurrency.MemberCur != null)
                if ((rowPosition == RowPosition.Next || rowPosition == RowPosition.Prior) && listCurrency.ListPositionCode == ListStatus.OnMemberRow)
                {

                    command.Parameters.Add(CreateParameter(command, STR_JunctionTableID, listCurrency.MemberCur.CurrencyKeys[listCurrency.JunctionTableID]));
                    //                   command.CommandText =
                    //                        string.Format(
                    //                            @" WITH cRow AS (Select {0}, {5}, Row_Number() over (order by {1}, {0}) as row from {2} where {3} = {4}) 
                    //                              Select *  From  cRow WHERE cRow.row = (Select cRow.row from cRow where cRow.{5} = {6}) + 1",
                    //                            orderBy.EndsWith("Desc ") ? listCurrency.MultiMemberTypeKey + " Desc " : listCurrency.MultiMemberTypeKey,
                    //                            orderBy, listCurrency.JunctionTableName, listCurrency.JunctionFkName, parmListOwnerID,
                    //                            listCurrency.JunctionTableID, parmJunctionTableID);
                    if (listCurrency.ListOrd == ListOrder.SORTED)
                    {
                        command.CommandText = string.Format(" Select {0} {1}, {2} from {3} T, ( Select {4} from {3} where {2} = {5} ) B " +
                            "where T.{6} = {7} and T.{4} {8} B.{4} order by T.{9}, T.{10} {11} ", parmTop1,
                               listCurrency.MultiMemberTypeKey, listCurrency.JunctionTableID, CheckForSchema(listCurrency.JunctionTableName),
                               listCurrency.MultiMemberSortKey, parmJunctionTableID,
                               listCurrency.JunctionFkName, parmListOwnerID, parmOrder,
                               orderBy, orderBy.EndsWith("Desc ") ? listCurrency.MultiMemberTypeKey + " Desc " : listCurrency.MultiMemberTypeKey, parmFetch1);
                    }
                    // else if LinkList
                    #region Select from Linklist (NEXT and PRIOR pointers)
                    else if (_dalRecord.CurrentList.ListOrd == ListOrder.LinkList)
                    {

                        if (_dalRecord.SelectOrder == RowPosition.Next)
                        {
                            _linkListKey = string.Concat(_dalRecord.CurrentList.ListName, STR_NextPointerSuffix);
                            _linkListKeyOpp = string.Concat(_dalRecord.CurrentList.ListName, STR_PriorPointerSuffix);
                            parmSortOrder = string.Empty;
                        }
                        else
                        {
                            _linkListKey = string.Concat(_dalRecord.CurrentList.ListName, STR_PriorPointerSuffix);
                            _linkListKeyOpp = string.Concat(_dalRecord.CurrentList.ListName, STR_NextPointerSuffix);
                            parmSortOrder = STR_Desc;
                        }

                        AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetListOwnerID(_dalRecord.CurrentList.ListName));
                        _dalRecord.Command.CommandText = string.Format(string.Concat("with LinkList ({0}, {1}, Cnt) as (Select MB1.{0}, MB1.{1}, 0 as Cnt from {2} as MB1 where {3} = {4} and MB1.{5} = {6}",
                            " union all Select MB1.{0}, MB1.{1}, Cnt + 1 from {2} MB1  inner join LinkList L on  MB1.{0} = L.{1} where MB1.{3} = {4} and Cnt < {7} ) ",
                            " Select cnt as RowNbr, {2}.* from {2} inner join  Linklist on Linklist.{0} = {2}.{0} order by Linklist.cnt {8}"),
                            _dalRecord.IDColumnName, _linkListKey, CheckForSchema(listCurrency.JunctionTableName), listCurrency.JunctionFkName, parmListOwnerID, _linkListKeyOpp, parmStartID, _maxRows.ToString(), parmSortOrder);
                    }
                    #endregion
                    else
                    {

                        command.CommandText = string.Format(" Select {0} {1}, {2} from {3} where {4} = {5} and {2} {6} {7} order by {8}, {9} {10}", parmTop1,
                       listCurrency.MultiMemberTypeKey, listCurrency.JunctionTableID, CheckForSchema(listCurrency.JunctionTableName),
                       listCurrency.JunctionFkName, parmListOwnerID, parmOrder, parmJunctionTableID,
                       orderBy, orderBy.EndsWith("Desc ") ? listCurrency.MultiMemberTypeKey + " Desc " : listCurrency.MultiMemberTypeKey, parmFetch1);
                    }

                }
                else if (rowPosition == RowPosition.Current && listCurrency.ListPositionCode == ListStatus.OnMemberRow)
                {
                    command.Parameters.Add(CreateParameter(command, STR_JunctionTableID, listCurrency.MemberCur.CurrencyKeys[listCurrency.JunctionTableID]));
                    //command.CommandText = string.Format(" WITH cRow AS (Select {0}, {5}, Row_Number() over (order by {1}, {0}) as row from {2} where {5} = {6} ) Select *  From  cRow WHERE cRow.row = 1  ",
                    //    listCurrency.MultiMemberTypeKey, orderBy, listCurrency.JunctionTableName, listCurrency.JunctionFkName, parmListOwnerID,
                    //    listCurrency.JunctionTableID, parmJunctionTableID);

                    command.CommandText = string.Format(" Select {0} {1}, {2} from {3} where {2} = {4} order by {5}, {6} {7} ", parmTop1,
                           listCurrency.MultiMemberTypeKey, listCurrency.JunctionTableID, CheckForSchema(listCurrency.JunctionTableName),
                            parmJunctionTableID, orderBy, listCurrency.MultiMemberTypeKey, parmFetch1);

                }
                else
                {
                    //command.CommandText =
                    //    string.Format(" WITH cRow AS (Select {0}, {5}, Row_Number() over (order by {1}, {0}) as row from {2} where {3} = {4} ) Select *  From  cRow WHERE cRow.row = 1  ",
                    //    listCurrency.MultiMemberTypeKey, orderBy, listCurrency.JunctionTableName, listCurrency.JunctionFkName,
                    //    parmListOwnerID, listCurrency.JunctionTableID);

                    command.CommandText = string.Format(" Select {0} {1}, {2} from {3} where {4} = {5} order by {6}, {7} {8} ", parmTop1,
                           listCurrency.MultiMemberTypeKey, listCurrency.JunctionTableID, CheckForSchema(listCurrency.JunctionTableName),
                           listCurrency.JunctionFkName, parmListOwnerID, orderBy, listCurrency.MultiMemberTypeKey, parmFetch1);

                }
            }

            if (_doLogging && !_logErrorsOnly)
            {
                string commandtext = command.CommandText;

                StringBuilder sbParms = new StringBuilder();

                foreach (DbParameter param in command.Parameters)
                {
                    if ((param.ParameterName != null))
                        sbParms.AppendFormat("       {0}={1},\r\n", param.ParameterName, param.Value);
                }
                commandtext += "\r\n" + sbParms.ToString();
                LogMessageToFile(commandtext);
            }
        }

        /// <summary>
        /// Create sql commnadText for querying against other keys
        /// </summary>
        /// <param name="dalRecord">Specifies an instance of current record, which database command object should be updated with generated SQL statement text.</param>
        /// <param name="keyName">Specifies key name.</param>
        /// <param name="keyValue">Specifies key value.</param>
        public void SetUpSelectByOtherKey(DalRecordBase dalRecord, string keyName, long keyValue)
        {
            dalRecord.Command.CommandType = CommandType.Text;
            if (_dBAccessType == DbAccessType.DynamicSql)
            {
                dalRecord.Command.CommandText = string.Format("Select * from {0} where {1} = {2} ",
                    CheckForSchema(dalRecord.TableName), keyName, keyValue);
            }
        }

        #endregion

        #region Private Methods

        private void SetViewName()
        {
            if (string.IsNullOrEmpty(_dalRecord.AlternateViewName))
            {
                _viewName = _dalRecord.TableName;
            }
            else
            {
                _viewName = _dalRecord.AlternateViewName;
            }
            _viewName = CheckForSchema(_viewName);

        }
        /// <summary>
        /// Create Sql Syntax for selecting row(s) by business key
        /// </summary>
        private void SelectByBuskey()
        {
            _dalRecord.SetKeyParameters(STR_BusinessKeys);
            if (_dBAccessType == DbAccessType.DynamicSql)
            {
                _dalRecord.Command.CommandText = string.Format("Select * from {0} where {1} ",
                    _viewName, _dalRecord.GetKeySelectLogic(STR_BusinessKeys));
            }

        }

        /// <summary>
        /// Create sql syntax for selecting row by ID column
        /// </summary>
        private void SelectByID()
        {
            AddCommandParameter(STR_RowID, _dalRecord.IDColumnValue);
            if (_dBAccessType == DbAccessType.DynamicSql)
            {
                _dalRecord.Command.CommandText = string.Format("Select * from {0} where {1} = {2} ",
                    _viewName, _dalRecord.IDColumnName, parmRowID);
            }
        }

        /// <summary>
        /// Create sql syntax for selecting rows from a list (index or foreign key relationship)
        /// </summary>
        private void SelectInList(bool inGetInListByKey = false)
        {
            long _startID = 0;

            #region Set up Select Parms
            if (_dalRecord.IDColumnValue == 0)
            {
                //AddCommandParameter(STR_StartID, 0);
            }
            else
            {
                if (_dalRecord.CurrentRecord.RecordActionCode == RowStatus.DeletedRow && (_dalRecord.CurrentList.ListPositionCode == ListStatus.OnNone ||
                       (_dalRecord.CurrentList.ListPositionCode == ListStatus.OnMemberRow &&
                           Convert.ToInt64(_dalRecord.CurrentList.MemberCur.CurrencyKeys[_dalRecord.CurrentList.MemberCur.IdColName])
                                  == Convert.ToInt64(_dalRecord.CurrentRecord.CurrencyKeys[_dalRecord.CurrentRecord.IdColName]))))
                {
                    if (_dalRecord.SelectOrder == RowPosition.Prior)
                        _startID = _dalRecord.CurrentList.MemberCur.DeletedPriorIdColValue;
                    else if (_dalRecord.SelectOrder == RowPosition.Next)
                        _startID = _dalRecord.CurrentList.MemberCur.DeletedNextIdColValue;
                    else
                        _startID = CurrentDbCurrency.GetListCurrentID(_dalRecord.CurrentList.ListName);
                }
                else
                    _startID = CurrentDbCurrency.GetListCurrentID(_dalRecord.CurrentList.ListName);

                if (_dalRecord.CurrentList.ListOrd == ListOrder.LinkList)
                {
                    string currencyPrior = _dalRecord.CurrentList.ListName + "_P";
                    if (_dalRecord.CurrentRecord.RecordActionCode == RowStatus.DeletedRow)
                    {
                        _startID = CurrentDbCurrency.GetListCurrentID(currencyPrior);
                    }
                }
                AddCommandParameter(STR_StartID, _startID);
                // AddCommandParameter(STR_StartID, CurrentDbCurrency.GetListCurrentID(_dalRecord.CurrentList.ListName));
            }

            if (_dBAccessType == DbAccessType.StoredProcedures)
            {
                AddCommandParameter(STR_ListName, _dalRecord.CurrentList.ListName);
                AddCommandParameter(STR_SelectType, _dalRecord.SelectOrder.ToString());
                AddCommandParameter(STR_WhereCriteria, _dalRecord.WhereCriteria);
                AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetListOwnerID(_dalRecord.CurrentList.ListName));
            }
            else
            {
                if (parmFetch.StartsWith(STR_Desc))
                {
                    parmFetch = parmFetch.Remove(0, STR_Desc.Length);
                }
                if (_dalRecord.SelectOrder == RowPosition.Prior)
                {
                    parmOrder = (_dalRecord.CurrentRecord.RecordActionCode == RowStatus.DeletedRow
                        && (_dalRecord.CurrentList.ListPositionCode == ListStatus.OnNone ||
                           (_dalRecord.CurrentList.ListPositionCode == ListStatus.OnMemberRow &&
                           Convert.ToInt64(_dalRecord.CurrentList.MemberCur.CurrencyKeys[_dalRecord.CurrentList.MemberCur.IdColName])
                            == Convert.ToInt64(_dalRecord.CurrentRecord.CurrencyKeys[_dalRecord.CurrentRecord.IdColName]))))
                    ? STR_LessThanEqualTo : STR_LessThan;

                }
                else if (_dalRecord.SelectOrder == RowPosition.Current)
                {
                    parmOrder = STR_GreaterThanEqualTo;
                }
                else  // Default is RowPosition.Next
                {
                    parmOrder = (_dalRecord.CurrentRecord.RecordActionCode == RowStatus.DeletedRow
                        && (_dalRecord.CurrentList.ListPositionCode == ListStatus.OnNone
                            || (_dalRecord.CurrentList.ListPositionCode == ListStatus.OnMemberRow
                                && Convert.ToInt64(_dalRecord.CurrentList.MemberCur.CurrencyKeys[_dalRecord.CurrentList.MemberCur.IdColName]) == Convert.ToInt64(_dalRecord.CurrentRecord.CurrencyKeys[_dalRecord.CurrentRecord.IdColName]))))
                    ? STR_GreaterThanEqualTo : STR_GreaterThan;
                }
                //Not Used string parmWhereStart;
                //if ((_dalRecord.IDColumnValue == 0) || (_startID == 0))
                //{
                //    parmWhereStart = parmWhereRowStart;
                //}
                //else
                //    parmWhereStart = string.Format(parmWhereRowMatchesID, parmOrder, _dalRecord.IDColumnName, parmStartID);
            #endregion

                #region Database type documentation
                //SQL Server: with crow as (select idkey, ROW_NUMBER() over (order by sortkey) as rnum from tablename)
                //select top(5) * from crow inner join tablename t on t.idkey = crow.idkey
                //where crow.rnum >= (select rnum from crow where crow.iskey = 12)
                //order by crow.rnum
                //
                //Oracle: with crow as (select idkey, ROW_NUMBER() over (order by sortkey) as rnum from tablename)
                //select * from crow inner join tablename t on t.idkey = crow.idkey
                //where crow.rnum >= (select rnum from crow where crow.iskey = 12)
                //and rownum <= 5
                //order by crow.rnum
                //
                //DB2: with crow as (select idkey, ROW_NUMBER() over (order by sortkey) as rnum from tablename)
                //select * from crow inner join tablename t on t.idkey = crow.idkey
                //where crow.rnum >= (select rnum from crow where crow.iskey = 12)
                //order by crow.rnum
                //fetch 5 rows only 
                #endregion

                // If INDX or AREA set
                #region Select from system owned index or area (No foreign key)
                if (_dalRecord.CurrentList.ListFkName == null)
                {

                    // Optimized query
                    string keyColumns = _dalRecord.CurrentList.ListKeys.Replace(" ASC", "").Replace(" DESC", "");
                    string orderBy = _dalRecord.CurrentList.ListKeys;
                    if (_dalRecord.SelectOrder == RowPosition.Prior || _dalRecord.SelectOrder == RowPosition.Last)
                    {
                        orderBy = _dalRecord.CurrentList.ListKeys.Replace(" DESC", " D*SC");
                        orderBy = orderBy.Replace(" ASC", " DESC");
                        orderBy = orderBy.Replace(" D*SC", " ASC");
                    }
                    string outsideOrderBy = orderBy;
                    orderBy = string.Concat("A.", orderBy);
                    orderBy = orderBy.Replace(STR_Comma, string.Concat(STR_Comma, "A."));

                    // Not used parmWhereStart = parmWhereStart.Replace("crow", _viewName);

                    if (_dalRecord.CurrentList.ListOpt == ListOptions.MA)
                    {
                        if ((_dalRecord.IDColumnValue == 0) || (_startID == 0))
                        {
                            _dalRecord.Command.CommandText = string.Format(" select {0} 0 as rownbr, A.* from {1} A order by {2} {3} ", parmTop,
                             _viewName, orderBy, parmFetch);
                        }
                        else
                        {
                            string startQuery = string.Format("Select {0} from {1} where {2} = {3} ",
                                keyColumns, _viewName, _dalRecord.IDColumnName, parmStartID);

                            string queryText = string.Format(" select {0} 0 as rownbr, A.* from {1} A, ({2}) B where ({3}) order by {4} {5} ", parmTop,
                            _viewName, startQuery, STR_KeyProgression, orderBy, parmFetch);

                            _dalRecord.Command.CommandText = string.Format(" Select {0} * from {1}( {2} ) T order by {3} {4} ", parmTop, parmTable,
                                GetKeyProgressionUnionSql(queryText), outsideOrderBy, parmFetch);
                        }
                    }
                    else
                    {
                        if ((_dalRecord.IDColumnValue == 0) || (_startID == 0))
                        {
                            _dalRecord.Command.CommandText = string.Format(" select {0} 0 as rownbr, A.* from {1} A where A.{2} = 'Y'  order by {3} {4} ", parmTop,
                             _viewName, _dalRecord.CurrentList.ListName, orderBy, parmFetch);
                        }
                        else
                        {
                            string startQuery = string.Format("Select {0} from {1} where {2} = {3} ",
                             keyColumns, _viewName, _dalRecord.IDColumnName, parmStartID);

                            string queryText = string.Format(" select {0} 0 as rownbr, A.* from {1} A, ({2}) B where {3} = 'Y' and ({4}) order by {5} {6} ", parmTop,
                            _viewName, startQuery, _dalRecord.CurrentList.ListName, STR_KeyProgression, orderBy, parmFetch);

                            _dalRecord.Command.CommandText = string.Format(" Select {0} * from {1}( {2} ) T order by {3} {4} ", parmTop, parmTable,
                                  GetKeyProgressionUnionSql(queryText), outsideOrderBy, parmFetch);

                        }
                    }
                }
                #endregion
                // else if LinkList
                #region Select from Linklist (NEXT and PRIOR pointers)
                else if (_dalRecord.CurrentList.ListOrd == ListOrder.LinkList)
                {

                    if (_dalRecord.SelectOrder == RowPosition.Next)
                    {
                        _linkListKey = string.Concat(_dalRecord.CurrentList.ListName, STR_NextPointerSuffix);
                        _linkListKeyOpp = string.Concat(_dalRecord.CurrentList.ListName, STR_PriorPointerSuffix);
                        parmSortOrder = string.Empty;
                    }
                    else
                    {
                        _linkListKey = string.Concat(_dalRecord.CurrentList.ListName, STR_PriorPointerSuffix);
                        _linkListKeyOpp = string.Concat(_dalRecord.CurrentList.ListName, STR_NextPointerSuffix);
                        parmSortOrder = STR_Desc;
                    }
                    if (!_dalRecord.Command.Parameters.Contains(string.Concat(_dalRecord.ParmPrefix, STR_StartID)))
                        AddCommandParameter(STR_StartID, 0);

                    if (_dalRecord.CurrentList.MemberList != null && _dalRecord.CurrentList.MemberList.Count > 0)
                    {
                        //Set up multi member linklist query
                        AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetListOwnerID(_dalRecord.CurrentList.ListName));
                        _dalRecord.Command.CommandText = string.Format(string.Concat("with LinkList ({0}, {1}, Cnt) as (Select MB1.{0}, MB1.{1}, 0 as Cnt from {2} MB1 where {3} = {4} and MB1.{5} = {6}",
                            " union all Select MB1.{0}, MB1.{1}, Cnt + 1 from {2} MB1  inner join LinkList L on  MB1.{0} = L.{1} where MB1.{3} = {4} and Cnt < {7} ) ",
                            " Select cnt as RowNbr, {8}.* from {8} inner join  Linklist on Linklist.{0} = {8}.{0} order by Linklist.cnt {9}"),
                           _dalRecord.CurrentList.JunctionTableID, _linkListKey, CheckForSchema(_dalRecord.CurrentList.JunctionTableName),
                            _dalRecord.CurrentList.JunctionFkName, parmListOwnerID, _linkListKeyOpp, parmStartID, _maxRows.ToString(), _viewName, parmSortOrder);
                    }
                    else
                    {
                        AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetListOwnerID(_dalRecord.CurrentList.ListName));
                        _dalRecord.Command.CommandText = string.Format(string.Concat("with LinkList ({0}, {1}, Cnt) as (Select MB1.{0}, MB1.{1}, 0 as Cnt from {2} MB1 where {3} = {4} and MB1.{5} = {6}",
                            " union all Select MB1.{0}, MB1.{1}, Cnt + 1 from {2} MB1  inner join LinkList L on  MB1.{0} = L.{1} where MB1.{3} = {4} and Cnt < {7} ) ",
                            " Select cnt as RowNbr, {2}.* from {2} inner join  Linklist on Linklist.{0} = {2}.{0} order by Linklist.cnt {8}"),
                            _dalRecord.IDColumnName, _linkListKey, _viewName,
                            _dalRecord.CurrentList.ListFkName, parmListOwnerID, _linkListKeyOpp, parmStartID, _maxRows.ToString(), parmSortOrder);
                    }
                }
                #endregion
                //else if Sorted
                #region Select from Sorted Lists
                else if (_dalRecord.CurrentList.ListOrd == ListOrder.SORTED)
                {
                    string joinSql = string.Empty;
                    string listKeys = "";
                    if (inGetInListByKey && _dalRecord.CurrentList.MemberList != null && _dalRecord.CurrentList.MemberList.Count > 1)
                    {
                        string[] lstKeys = _dalRecord.CurrentList.ListKeys.Trim().Split(',');
                        foreach (string key in _dalRecord.Record.ChildCollection.Keys)
                        {
                            foreach (string lstKey in lstKeys)
                            {
                                if (lstKey.StartsWith(key))
                                    listKeys = listKeys + (listKeys.Length > 0 ? "," : "") + lstKey;
                            }
                        }
                    }
                    else
                        listKeys = _dalRecord.CurrentList.ListKeys;
                    string keyColumns = listKeys.Replace(" ASC", "").Replace(" DESC", "");

                    if (_dalRecord.CurrentList.ListOpt != ListOptions.MA)
                    {
                        AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetJunctionListOwnerID(_dalRecord.CurrentList.ListName));

                        //if (_dalRecord.CurrentList.ListDups == ListDuplicates.First || _dalRecord.CurrentList.ListDups == ListDuplicates.Last)
                        //{
                        string junctionTableSuffix = string.Concat("A", STR_Period);
                        joinSql = string.Concat(STR_InnerJoin, CheckForSchema(_dalRecord.CurrentList.JunctionTableName), " A", STR_On,
                            junctionTableSuffix, _dalRecord.IDColumnName, STR_EqualTo,
                            "T", STR_Period, _dalRecord.IDColumnName);
                        if (listKeys.Trim() != string.Empty)
                        {
                            listKeys = string.Concat(junctionTableSuffix, listKeys);
                            listKeys = listKeys.Replace(STR_Comma, string.Concat(STR_Comma, junctionTableSuffix));
                        }
                        //}
                    }
                    else
                    {
                        AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetListOwnerID(_dalRecord.CurrentList.ListName));
                    }

                    string orderBy = listKeys;

                    if (_dalRecord.SelectOrder == RowPosition.Prior || _dalRecord.SelectOrder == RowPosition.Last)
                    {
                        orderBy = listKeys.Replace(" DESC", " D*SC");
                        orderBy = orderBy.Replace(" ASC", " DESC");
                        orderBy = orderBy.Replace(" D*SC", " ASC");

                    }
                    string outsideOrderBy = orderBy;

                    if (_dalRecord.CurrentList.ListOpt == ListOptions.MA)
                    {
                        orderBy = string.Concat("A.", orderBy);
                        orderBy = orderBy.Replace(STR_Comma, string.Concat(STR_Comma, "A."));
                    }

                    if (_dalRecord.CurrentList.ListOpt != ListOptions.MA)
                    {
                        if ((_dalRecord.IDColumnValue == 0) || (_startID == 0))
                        {
                            _dalRecord.Command.CommandText = string.Format(" select {0} T.{1}, 0 as rownbr, T.* from {2} T {3} where T.{4} = {5}  order by {6} {7} ", parmTop,
                            _dalRecord.IDColumnName, _viewName, joinSql, _dalRecord.CurrentList.ListFkName, parmListOwnerID, orderBy, parmFetch);
                        }
                        else
                        {
                            //string keyProgression = GetKeyProgression("A");
                            //_dalRecord.Command.CommandText = string.Format(string.Concat(" select {0} T.{1}, 0 as row, T.* from {2} T {3}, ( Select {4} from {5} where {1} = {6} ) B",
                            //" where T.{7} = {8} and ({9}) ",
                            //" order by {10} {11} "), parmTop,
                            //_dalRecord.IDColumnName, _viewName, joinSql, keyColumns, _dalRecord.CurrentList.JunctionTableName, parmStartID,
                            //_dalRecord.CurrentList.ListFkName, parmListOwnerID, keyProgression, orderBy, parmFetch);

                            string keyProgression = GetKeyProgression("cRow");
                            _dalRecord.Command.CommandText = string.Format(string.Concat(" with cRow as (select Row_Number() over (order by {0}) as rownbr, A.*",
                            "   from {1} T inner join {2} A on A.{3} = T.{3} where T.{4} = {5}) ",
                            " Select {6} cRow.{3}, cRow.rownbr, ST.* from cRow inner join {1} ST on ST.{3} = cRow.{3}",
                            "   inner join {2} B on B.{3} = {7} where ({8}) order by cRow.rownbr  "),
                            orderBy, _viewName, CheckForSchema(_dalRecord.CurrentList.JunctionTableName), _dalRecord.IDColumnName, _dalRecord.CurrentList.ListFkName,
                            parmListOwnerID, parmTop, parmStartID, keyProgression);

                        }
                    }
                    else
                    {
                        if ((_dalRecord.IDColumnValue == 0) || (_startID == 0))
                        {
                            _dalRecord.Command.CommandText = string.Format(" select {0} A.{1}, 0 as rownbr, A.* from {2} A where A.{3} = {4}  order by {5} {6} ", parmTop,
                            _dalRecord.IDColumnName, _viewName, _dalRecord.CurrentList.ListFkName, parmListOwnerID, orderBy, parmFetch);
                        }
                        else
                        {
                            //      ******************************************************************************************************************
                            string startQuery = string.Format("Select {0} from {1} where {2} = {3} ",
                             keyColumns, _viewName, _dalRecord.IDColumnName, parmStartID);

                            string queryText = string.Format(" select {0} 0 as rownbr, A.* from {1} A, ({2}) B where A.{3} = {4} and ({5}) order by {6} {7} ", parmTop,
                            _viewName, startQuery, _dalRecord.CurrentList.ListFkName, parmListOwnerID, STR_KeyProgression, orderBy, parmFetch);

                            _dalRecord.Command.CommandText = string.Format(" Select {0} * from {1}( {2} ) TS order by {3} {4} ", parmTop, parmTable,
                                GetKeyProgressionUnionSql(queryText, listKeys), outsideOrderBy, parmFetch);
                        }
                    }

                }
                #endregion
                //ELSE if FIRST or LAST
                #region Select from FIRST or LAST ordered Lists
                else
                {
                    string SequenceKey = string.Empty;
                    string SequenceOrder = string.Empty;
                    string sequenceParm = string.Empty;
                    string startQuery = string.Empty;
                    if (_dalRecord.CurrentList.ListOrd == ListOrder.FIRST)
                    {
                        SequenceOrder = STR_Desc;
                        if (parmOrder.Contains(STR_LessThan))
                            parmOrder = parmOrder.Replace(STR_LessThan, STR_GreaterThan);
                        else if (parmOrder.Contains(STR_GreaterThan))
                            parmOrder = parmOrder.Replace(STR_GreaterThan, STR_LessThan);
                    }
                    if (_dalRecord.SelectOrder == RowPosition.Prior || _dalRecord.SelectOrder == RowPosition.Last)
                    {
                        if (SequenceOrder == STR_Desc)
                            SequenceOrder = string.Empty;
                        else
                            SequenceOrder = STR_Desc;

                    }

                    if (_dalRecord.CurrentList.ListOpt != ListOptions.MA)
                    {
                        //Added check for List duplicates not allowed - issue 6158
                        if (_dalRecord.CurrentList.ListDups != ListDuplicates.NotAllowed)
                        {
                            sequenceParm = string.Concat(_dalRecord.CurrentList.ListName, "_S");
                            SequenceKey = string.Concat("J.", sequenceParm, SequenceOrder, ", ", "J.", _dalRecord.CurrentList.JunctionTableID, SequenceOrder);
                        }
                        else
                        {
                            SequenceKey = string.Concat(_dalRecord.CurrentList.JunctionTableID, SequenceOrder);
                        }
                    }
                    else
                    {
                        SequenceKey = string.Concat(_dalRecord.IDColumnName, SequenceOrder);
                    }

                    string startKeyLogic = string.Empty;
                    if ((_dalRecord.IDColumnValue == 0) || (_startID == 0))
                    {
                        startKeyLogic = string.Empty;
                    }
                    else
                    {
                        if (_dalRecord.CurrentList.ListDups == ListDuplicates.NotAllowed || sequenceParm == string.Empty)
                        {
                            startKeyLogic = string.Concat(" and ( T.", _dalRecord.IDColumnName, " ", parmOrder, " ", parmStartID, "  )");
                        }
                        else
                        {
                            startKeyLogic = string.Format("and ((J.{0} = S.{0} and J.{1} {2} S.{1}) or (J.{0} {2} S.{0}))  ",
                                sequenceParm, _dalRecord.CurrentList.JunctionTableID, parmOrder);
                            startQuery = string.Format(", (Select * from {0} where {1} = {2} ) S",
                                CheckForSchema(_dalRecord.CurrentList.JunctionTableName), _dalRecord.IDColumnName, parmStartID);
                        }
                    }

                    if (_dalRecord.CurrentList.ListOpt != ListOptions.MA)
                    {
                        //Create FIRST/LAST query for Optional sets 

                        AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetJunctionListOwnerID(_dalRecord.CurrentList.ListName));
                        _dalRecord.Command.CommandText = string.Format(string.Concat(" select {0} J.{1}, 0 as rownbr, T.* from {2} J inner join {3} T on T.{1} = J.{1} {4} ",
                        "where J.{5} = {6} {7} ",
                        " order by J.{8} {9} "), parmTop,
                         _dalRecord.IDColumnName, CheckForSchema(_dalRecord.CurrentList.JunctionTableName), _viewName, startQuery,
                         _dalRecord.CurrentList.OwnerCur.IdColName, parmListOwnerID, startKeyLogic, SequenceKey, parmFetch);
                    }
                    else
                    {
                        //Create FIRST/LAST query for Mandatory sets
                        AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetListOwnerID(_dalRecord.CurrentList.ListName));

                        _dalRecord.Command.CommandText = string.Format(string.Concat(" select {0} {1}, 0 as rownbr, T.* from {2} T where {3} = {4} {5} ",
                        " order by T.{6} {7} "), parmTop,
                        _dalRecord.IDColumnName, _viewName, _dalRecord.CurrentList.ListFkName, parmListOwnerID, startKeyLogic,
                        SequenceKey, parmFetch);
                    }
                }
                #endregion
            }

        }

        /// <summary>
        /// Create sql syntax for selecting rows from a list (index or foreign key relationship)
        /// </summary>
        private void SelectInListByRowNumber()
        {

            AddCommandParameter(STR_RowNumber, _dalRecord.StartRow);

            if (parmFetch.StartsWith(STR_Desc))
            {
                parmFetch = parmFetch.Remove(0, STR_Desc.Length);
            }
            if (_dalRecord.SelectOrder == RowPosition.Prior)
            {
                parmOrder = STR_LessThan;
                parmFetch = string.Concat(STR_Desc, parmFetch);
            }
            else
            {
                parmOrder = STR_GreaterThan;
            }

            // Not used string parmWhereStart = string.Format(parmWhereRowMatchesID, parmOrder, _dalRecord.IDColumnName, parmStartID);

            // If INDX or AREA set
            if (_dalRecord.CurrentList.ListFkName == null)
            {
                if (_dalRecord.CurrentList.ListOpt == ListOptions.MA)
                {
                    _dalRecord.Command.CommandText = string.Format(string.Concat("with crow as ( select {0}, row_number() over (order by {1} ) as rownbr from {2} ) ",
                "select crow.rownbr, {2}.* from crow inner join {2} T on T.{0} = crow.{0} where rownbr = {3} "),
                _dalRecord.IDColumnName, _dalRecord.CurrentList.ListKeys, _viewName, parmRowNumber);
                }
                else
                {
                    _dalRecord.Command.CommandText = string.Format(string.Concat("with crow as ( select {0}, row_number() over (order by {1} ) as rownbr from {2} where {3} = 'Y') ",
                    "select crow.rownbr, {2}.* from crow inner join {2} T on T.{0} = crow.{0} where rownbr = {4} "),
                    _dalRecord.IDColumnName, _dalRecord.CurrentList.ListKeys, _viewName, _dalRecord.CurrentList.ListName, parmRowNumber);
                }

            }
            // else if LinkList
            else if (_dalRecord.CurrentList.ListOrd == ListOrder.LinkList)
            {


                _linkListKey = string.Concat(_dalRecord.CurrentList.ListName, STR_NextPointerSuffix);
                _linkListKeyOpp = string.Concat(_dalRecord.CurrentList.ListName, STR_PriorPointerSuffix);
                parmSortOrder = string.Empty;


                AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetListOwnerID(_dalRecord.CurrentList.ListName));
                _dalRecord.Command.CommandText = string.Format(string.Concat("with LinkList ({0}, {1}, Cnt) as (Select MB1.{0}, MB1.{1}, 1 as Cnt from {2} MB1 where {3} = {4} and MB1.{5} = 0",
                    " union all Select MB1.{0}, MB1.{1}, Cnt + 1 from {2} MB1  inner join LinkList L on  MB1.{0} = L.{1} where MB1.{3} = {4} ) ",
                    " Select cnt, {2}.* from {2} inner join  Linklist on Linklist.{0} = {2}.{0} and cnt = {6}"),
                    _dalRecord.IDColumnName, _linkListKey, _viewName,
                    _dalRecord.CurrentList.ListFkName, parmListOwnerID, _linkListKeyOpp, parmRowNumber);
            }
            //else if Sorted
            else if (_dalRecord.CurrentList.ListOrd == ListOrder.SORTED)
            {
                if (_dalRecord.CurrentList.ListOpt != ListOptions.MA)
                {
                    AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetJunctionListOwnerID(_dalRecord.CurrentList.ListName));
                }
                else
                {
                    AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetListOwnerID(_dalRecord.CurrentList.ListName));
                }

                _dalRecord.Command.CommandText = string.Format(string.Concat("with crow as ( select {0}, row_number() over (order by {1} ) as rownbr from {2} where {3} = {4} ) ",
                "select * from crow inner join {2} T on T.{0} = crow.{0} where rownbr = {5} "),
                _dalRecord.IDColumnName, _dalRecord.CurrentList.ListKeys, _viewName, _dalRecord.CurrentList.ListFkName,
                parmListOwnerID, parmRowNumber);
            }
            else
            {
                string SequenceKey = _dalRecord.IDColumnName;
                if (_dalRecord.CurrentList.ListOrd == ListOrder.FIRST)
                {
                    SequenceKey = string.Concat(SequenceKey, STR_Desc);
                }

                if (_dalRecord.CurrentList.ListOpt != ListOptions.MA)
                {
                    AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetJunctionListOwnerID(_dalRecord.CurrentList.ListName));
                }
                else
                {
                    AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetListOwnerID(_dalRecord.CurrentList.ListName));
                }

                _dalRecord.Command.CommandText = string.Format(string.Concat("with crow as ( select {0}, row_number() over (order by {1} ) as rownbr from {2} where {3} = {4} ) ",
                "select * from crow inner join {2} T on T.{0} = crow.{0} where rownbr = {5} ",
                " order by crow.rownbr  "),
                _dalRecord.IDColumnName, SequenceKey, _viewName, _dalRecord.CurrentList.ListFkName,
                parmListOwnerID, parmRowNumber);

            }

        }

        /// <summary>
        /// Create sql syntax for selecting row in list based on key or partial key
        /// </summary>
        private void SelectInListUsing()
        {
            string joinSql = string.Empty;
            string startQuery = string.Empty;
            string keyProgression = string.Empty;
            string usingTableAlias = "A";

            //Exclude list table columns that don't belong to the record
            string listKeys = "";
            string[] lstKeys = _dalRecord.CurrentList.ListKeys.Trim().Split(',');
            if (_dalRecord.CurrentList.MemberList != null && _dalRecord.CurrentList.MemberList.Count > 1)
            {
                foreach (string key in _dalRecord.Record.ChildCollection.Keys)
                {
                    foreach (string lstKey in lstKeys)
                    {
                        if (lstKey.StartsWith(key))
                            listKeys = listKeys + (listKeys.Length > 0 ? "," : "") + lstKey;
                    }
                }
            }
            else
            {
                foreach (string lstKey in lstKeys)
                {
                    listKeys = listKeys + (listKeys.Length > 0 ? "," : "") + lstKey;
                }
            }

            if (listKeys.Trim() != string.Empty)
            {
                listKeys = string.Concat(usingTableAlias, STR_Period, listKeys);
                listKeys = listKeys.Replace(STR_Comma, string.Concat(STR_Comma, usingTableAlias, STR_Period));
            }
            if (_dalRecord.CurrentList.ListOpt != ListOptions.MA)
            {
                AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetJunctionListOwnerID(_dalRecord.CurrentList.ListName));

                if (_dalRecord.CurrentList.ListDups == ListDuplicates.First || _dalRecord.CurrentList.ListDups == ListDuplicates.Last)
                {
                    if (_dalRecord.CurrentList.JunctionTableName != null)
                    {

                        string junctionTableSuffix = string.Concat(CheckForSchema(_dalRecord.CurrentList.JunctionTableName), STR_Period);
                        joinSql = string.Concat(STR_InnerJoin, CheckForSchema(_dalRecord.CurrentList.JunctionTableName), STR_On,
                            junctionTableSuffix, _dalRecord.IDColumnName, STR_EqualTo,
                            usingTableAlias, STR_Period, _dalRecord.IDColumnName);
                        if (listKeys.Trim() != string.Empty)
                        {
                            listKeys = listKeys.Replace(string.Concat(usingTableAlias, STR_Period), junctionTableSuffix);
                        }
                    }
                }
            }
            else
            {
                if (_dalRecord.CurrentList.ListFkName != null)
                {
                    AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetListOwnerID(_dalRecord.CurrentList.ListName));
                }
            }
            //***********************************
            //** Logic for Current positioning
            if (_dalRecord.SelectUsingPosition == RowPosition.Current)
            {
                long startID = CurrentDbCurrency.GetListCurrentID(_dalRecord.CurrentList.ListName);
                AddCommandParameter(STR_StartID, startID);
                string keyColumns = _dalRecord.CurrentList.ListKeys.Replace(" ASC", "").Replace(" DESC", "");
                startQuery = string.Format(" ,( Select {0} from {1} where {2} = {3} ) B ",
                keyColumns, _viewName, _dalRecord.IDColumnName, parmStartID);
                keyProgression = string.Format(" ({0}) and ", GetKeyProgression("A"));
            }

            //***********************************

            _dalRecord.SetKeyUsingParameters(_dalRecord.CurrentList.ListName, _dalRecord.SearchKey);
            _dalRecord.GetKeySelectLogic(_dalRecord.CurrentList.ListName);

            if (_dBAccessType == DbAccessType.StoredProcedures)
            {
                AddCommandParameter(STR_ListName, _dalRecord.CurrentList.ListName);
                AddCommandParameter(STR_SelectType, _dalRecord.SelectOrder.ToString());
                //   AddCommandParameter(STR_ListOwnerID, CurrentDbCurrency.GetListOwnerID(_dalRecord.CurrentList.ListName));
            }
            else
            {

                _dalRecord.MultipleKeysSqlList = new List<string>();
                StringBuilder sqlString = new StringBuilder();
                int ctr = 0;

                string[] strArray = _dalRecord.CurrentList.ListKeys.Split(',');
                List<string> listKeySortOrder = new List<string>();
                foreach (string stringKey in strArray)
                {
                    if (stringKey.EndsWith(" DESC"))
                    {
                        listKeySortOrder.Add(parmSortOrderDesc);
                    }
                    else
                    {
                        listKeySortOrder.Add(parmSortOrderAsc);
                    }
                }

                #region Build Select Using Key Sql

                string listKeyLogic = string.Empty;
                if (_dalRecord.CurrentList.ListFkName == null)
                {
                    if (_dalRecord.CurrentList.ListOpt != ListOptions.MA)
                    {
                        listKeyLogic = string.Format(" {0} = 'Y' and   ", _dalRecord.CurrentList.ListName);
                    }
                }
                else
                {
                    listKeyLogic = string.Format(" {0} = {1} and   ", _dalRecord.CurrentList.ListFkName, parmListOwnerID);
                }

                string cteString1 = string.Format("select {0} 'exact' as rowType, {1}.* from {2} {1}{3}{4} where {5}{6} ", parmTop1,
                    usingTableAlias, _viewName, startQuery, joinSql, listKeyLogic, keyProgression);
                string cteString2 = string.Format(" Order by {0} {1}   ", listKeys, parmFetch1);

                // Add key selection logic
                sqlString.Append(cteString1);
                foreach (string keyLogic in _dalRecord.SortKeyList)
                {
                    if (ctr > 0)
                        sqlString.Append(" and ");
                    sqlString.Append(string.Concat(usingTableAlias, ".", keyLogic));
                    ctr++;
                }

                sqlString.Append(cteString2);
                _dalRecord.MultipleKeysSqlList.Add(sqlString.ToString());

                // Create On Missing Next/Prior row Sql statements                  
                foreach (string keyLogic in _dalRecord.SortKeyList)
                {
                    sqlString.Clear();
                    sqlString.Append(string.Format("Select * from {0}( ", parmTable));

                    sqlString.Append(cteString1.Replace("'exact' as rowType", "'next' as rowType"));
                    for (int ctr2 = 0; ctr2 < ctr; ctr2++)
                    {
                        if (ctr2 > 0)
                            sqlString.Append(" and ");
                        if (ctr2 == (ctr - 1))
                        {

                            if (listKeySortOrder[ctr2] == parmSortOrderDesc)
                                sqlString.Append(string.Concat(usingTableAlias, ".", _dalRecord.SortKeyList[ctr2].Replace(" = ", " < ")));
                            else
                                sqlString.Append(string.Concat(usingTableAlias, ".", _dalRecord.SortKeyList[ctr2].Replace(" = ", " > ")));
                        }
                        else
                        {
                            sqlString.Append(string.Concat(usingTableAlias, ".", _dalRecord.SortKeyList[ctr2]));
                        }
                    }
                    sqlString.Append(cteString2);

                    sqlString.Append(string.Format(") TableA union Select * from {0}(", parmTable));

                    if (_dalRecord.SelectUsingPosition == RowPosition.Current)
                    {
                        cteString1 = cteString1.Replace(" < ", " << ").Replace(" > ", " >> ");
                        cteString1 = cteString1.Replace(" << ", " > ").Replace(" >> ", " < ");
                    }
                    sqlString.Append(cteString1.Replace("'exact' as rowType", "'prev' as rowType"));


                    for (int ctr2 = 0; ctr2 < ctr; ctr2++)
                    {
                        if (ctr2 > 0)
                            sqlString.Append(" and ");
                        if (ctr2 == (ctr - 1))
                        {
                            if (listKeySortOrder[ctr2] == parmSortOrderDesc)
                                sqlString.Append(string.Concat(usingTableAlias, ".", _dalRecord.SortKeyList[ctr2].Replace(" = ", " > ")));
                            else
                                sqlString.Append(string.Concat(usingTableAlias, ".", _dalRecord.SortKeyList[ctr2].Replace(" = ", " < ")));
                        }
                        else
                        {
                            sqlString.Append(string.Concat(usingTableAlias, ".", _dalRecord.SortKeyList[ctr2]));
                        }
                    }
                    sqlString.Append(cteString2.Replace(" DESC ", " AASSCC ").Replace(" ASC ", " DESC ").Replace(" AASSCC ", " ASC ")
                      .Replace(" DESC,", " AASSCC,").Replace(" ASC,", " DESC,").Replace(" AASSCC,", " ASC,"));
                    sqlString.Append(") TableB ");
                    _dalRecord.MultipleKeysSqlList.Add(sqlString.ToString());
                    ctr--;
                }

                //}
                #endregion

            }
        }

        /// <summary>
        /// Create sql syntax for updating row in table
        /// </summary>
        private void Update()
        {

            AddCommandParameter(STR_RowID, _dalRecord.IDColumnValue);
            _dalRecord.GetColumnData();
            GetDateTimeStamp();
            if (_dBAccessType == DbAccessType.DynamicSql)
            {
                _dalRecord.Command.CommandText = string.Format("Update {0} Set {1} where {2} = {3} ",
                    CheckForSchema(_dalRecord.TableName), _dalRecord.ColumnUpdateSets.ToString(), _dalRecord.IDColumnName, parmRowID);
            }
        }

        /// <summary>
        /// Create sql syntax to update rows for including & excluding from lists
        /// </summary>
        private void UpdateList()
        {
            bool isSystemOwnedList = false;
            bool hasJunctionTable = true;
            if (_dalRecord.CurrentList.OwnerCur == null)
            {
                isSystemOwnedList = true;
                if (string.IsNullOrEmpty(_dalRecord.CurrentList.JunctionTableName))
                {
                    hasJunctionTable = false;
                }
            }
            else
            {
                if (_dalRecord.ListUpdateType == "Include")
                    AddCommandParameter(_dalRecord.CurrentList.OwnerCur.IdColName, CurrentDbCurrency.GetJunctionListOwnerID(_dalRecord.CurrentList.ListName));
                else
                {
                    // member can point to the owner, which is different from OwnerCur - ticket 5734

                    long junctionListOwnerID = 0;
                    if (CurrentDbCurrency.ListTable.ContainsKey(_dalRecord.CurrentList.ListName))
                    {
                        ListCurrency listcurrency = CurrentDbCurrency.ListTable[_dalRecord.CurrentList.ListName];

                        if (listcurrency.MemberCur != null
                            && listcurrency.MemberCur.CurrencyKeys != null
                            && listcurrency.MemberCur.CurrencyKeys.Contains(listcurrency.ListFkName)
                            && listcurrency.MemberCur.CurrencyKeys[listcurrency.ListFkName] != null
                            //Added check for empty string - issue 6010
                            && listcurrency.MemberCur.CurrencyKeys[listcurrency.ListFkName].ToString().Trim() != string.Empty)
                        {
                            junctionListOwnerID = Convert.ToInt64(listcurrency.MemberCur.CurrencyKeys[listcurrency.ListFkName]);
                        }
                        else
                        {
                            junctionListOwnerID = CurrentDbCurrency.GetJunctionListOwnerID(_dalRecord.CurrentList.ListName);
                        }
                    }
                    AddCommandParameter(_dalRecord.CurrentList.OwnerCur.IdColName, junctionListOwnerID);
                }
            }

            if ((_dalRecord.CurrentList.ListOrd == ListOrder.SORTED || _dalRecord.CurrentList.ListOrd == ListOrder.FIRST || _dalRecord.CurrentList.ListOrd == ListOrder.LAST) && _dalRecord.CurrentList.JunctionTableID != null)
            {
                junctionTableIDColumn = _dalRecord.CurrentList.JunctionTableID;
            }
            else
            {
                junctionTableIDColumn = STR_DBS_ID_COL;
            }

            // setting CurrencyKey to IDColumnValue is for ticket 4580. 
            if ( //_dalRecord.IDColumnValue == 0 &&  <-- commented out because of ticket 8753
                _dalRecord.CurrentRecord.CurrencyKeys.ContainsKey(_dalRecord.IDColumnName) && _dalRecord.CurrentRecord.CurrencyKeys[_dalRecord.IDColumnName] != null)
                _dalRecord.IDColumnValue = Convert.ToInt64(_dalRecord.CurrentRecord.CurrencyKeys[_dalRecord.IDColumnName]);

            if (_dalRecord.CurrentList.MemberList == null || _dalRecord.CurrentList.MemberList.Count == 0)
                AddCommandParameter(_dalRecord.IDColumnName, _dalRecord.IDColumnValue);

            if (_dBAccessType == DbAccessType.StoredProcedures)
            {
                AddCommandParameter(STR_ListName, _dalRecord.CurrentList.ListName);
                AddCommandParameter(STR_UpdateType, _dalRecord.ListUpdateType);
            }
            else
            {
                if (_dalRecord.CurrentList.ListOrd == ListOrder.LinkList)
                {
                    _linkListKey = string.Concat(_dalRecord.CurrentList.ListName, STR_NextPointerSuffix);
                    parmListPointerNext = string.Concat(_dalRecord.ParmPrefix, _linkListKey);
                    _linkListKeyOpp = string.Concat(_dalRecord.CurrentList.ListName, STR_PriorPointerSuffix);
                    parmListPointerPrior = string.Concat(_dalRecord.ParmPrefix, _linkListKeyOpp);
                    if (_dalRecord.CurrentList.MemberCur.CurrencyKeys.ContainsKey(_linkListKey))
                        AddCommandParameter(_linkListKey, _dalRecord.CurrentList.MemberCur.CurrencyKeys[_linkListKey] == null ? 0 : _dalRecord.CurrentList.MemberCur.CurrencyKeys[_linkListKey]);
                    else
                        AddCommandParameter(_linkListKey, 0);
                    if (_dalRecord.CurrentList.MemberCur.CurrencyKeys.ContainsKey(_linkListKeyOpp))
                        AddCommandParameter(_linkListKeyOpp, _dalRecord.CurrentList.MemberCur.CurrencyKeys[_linkListKeyOpp] == null ? 0 : _dalRecord.CurrentList.MemberCur.CurrencyKeys[_linkListKeyOpp]);
                    else
                        AddCommandParameter(_linkListKeyOpp, 0);
                    if (_dalRecord.CurrentList.OwnerCur.CurrencyKeys.ContainsKey(_dalRecord.CurrentList.ListName))
                        AddCommandParameter(_dalRecord.CurrentList.ListName, _dalRecord.CurrentList.OwnerCur.CurrencyKeys[_dalRecord.CurrentList.ListName] == null ? 0 : _dalRecord.CurrentList.OwnerCur.CurrencyKeys[_dalRecord.CurrentList.ListName]);
                    else
                        AddCommandParameter(_dalRecord.CurrentList.ListName, 0);
                }

                if (_dalRecord.ListUpdateType == "Include")
                {
                    // Connect the row to the list
                    if (_dalRecord.CurrentList.ListOrd == ListOrder.LinkList)
                    {
                        // Linklist Update
                        string identityColumnName = string.Empty; string identityColumnValue = string.Empty;
                        if (IdentityColumnInfoHandler != null)
                        {
                            if ((_dalRecord.CurrentList.ListOpt != ListOptions.MA) && !string.IsNullOrEmpty(IdentityColumnInfoHandler.GetName(_dalRecord.CurrentList.JunctionTableName, junctionTableIDColumn)))
                            {
                                identityColumnName = string.Concat(IdentityColumnInfoHandler.GetName(_dalRecord.CurrentList.JunctionTableName, junctionTableIDColumn), ",");

                                //Undoing the following fix -Must get the value from the sequence object. Issue 5785.  If there is a problem with this identity column as a duplicate - the sequence object must be checked
                                // ticket 5680, 5759: for linked lists, modified from identityColumnValue to a parameter
                                //if (_dalRecord.CurrentList.ListOrd == ListOrder.LinkList)
                                //    identityColumnValue = "(select max(" + IdentityColumnInfoHandler.GetName(_dalRecord.CurrentList.JunctionTableName, junctionTableIDColumn) + ") from " + _dalRecord.CurrentList.JunctionTableName + ") + 1,";
                                //else
                                identityColumnValue = string.Concat(IdentityColumnInfoHandler.GetValue(_dalRecord.CurrentList.JunctionTableName), ",");
                            }
                        }
                        AddCommandParameter(STR_CurrentRowID, CurrentDbCurrency.GetListCurrentID(_dalRecord.CurrentList.ListName));
                        _dalRecord.SetKeyParameters(_dalRecord.CurrentList.ListName);
                        if (_dalRecord.CurrentList.ListOpt == ListOptions.MA)
                        // Set linklist for MA sets
                        {
                            _dalRecord.Command.CommandText = string.Format(string.Concat(
                                //Set Pointers on current record
                            "Update {0} set {1} = {2}, {3} = {4}, {5} = {6} where {7} = {8} ;  ",
                                //Set Next pointer on Prior Record 
                            "Update {0} set {3} = {8} where {7} = {6} ;  ",
                                //Set Prior pointer on Next Record 
                            "Update {0} set {5} = {8} where {7} = {4} ;  "),
                             CheckForSchema(_dalRecord.TableName), _dalRecord.CurrentList.OwnerCur.IdColName, _dalRecord.Command.Parameters[0].ParameterName,
                             _linkListKey, parmListPointerNext, _linkListKeyOpp, parmListPointerPrior,
                            _dalRecord.IDColumnName, _dalRecord.Command.Parameters[1].ParameterName);
                        }
                        else
                        // Set linklist for Optional sets
                        {
                            _dalRecord.Command.CommandText = string.Format(string.Concat(
                            "Insert into {0} ({1} {2} , {3}) Values ({4} {5} , {6});",
                                //Set Pointers on current record
                            "Update {7} set {8} = {5}, {9} = {10}, {11} = {12} where {13} = {6} ;  ",
                                //Set Next pointer on Prior Record 
                            "Update {7} set {9} = {6} where {13} = {12} ;  ",
                                //Set Prior pointer on Next Record 
                            "Update {7} set {11} = {6} where {13} = {10} ;  "),
                                CheckForSchema(_dalRecord.CurrentList.JunctionTableName), identityColumnName, _dalRecord.CurrentList.OwnerCur.IdColName, _dalRecord.CurrentList.MemberCur.IdColName,
                                identityColumnValue, _dalRecord.Command.Parameters[0].ParameterName, _dalRecord.Command.Parameters[1].ParameterName,
                                CheckForSchema(_dalRecord.TableName), _dalRecord.CurrentList.ListName, _linkListKey,
                                parmListPointerNext, _linkListKeyOpp, parmListPointerPrior,
                                _dalRecord.IDColumnName);
                        }

                    }
                    else
                    {
                        // Sorted List update
                        string identityColumnName = string.Empty; string identityColumnValue = string.Empty;
                        string junctionSColumn = string.Empty; string junctionSParm = string.Empty;
                        if ((_dalRecord.CurrentList.ListDups != ListDuplicates.NotAllowed &&
                            (_dalRecord.CurrentList.ListOrd == ListOrder.FIRST || _dalRecord.CurrentList.ListOrd == ListOrder.LAST || _dalRecord.CurrentList.ListOrd == ListOrder.SORTED))
                            ||
                            (_dalRecord.CurrentList.ListOrd == ListOrder.FIRST && _dalRecord.CurrentList.ListOpt != ListOptions.MA)
                            )
                        {
                            string parmName = string.Concat(_dalRecord.CurrentList.ListName, "_S");

                            if (string.IsNullOrEmpty(_dalRecord.CurrentList.ListSequenceObject))
                            {
                                if (_dalRecord.CurrentList.ListOrd == ListOrder.LAST || _dalRecord.CurrentList.ListDups == ListDuplicates.Last
                                    //Added check for LIstOrder.First to for Max Sequence - issue 6003
                                    || (_dalRecord.CurrentList.ListOrd == ListOrder.FIRST && _dalRecord.CurrentList.ListDups == ListDuplicates.First))
                                {
                                    AddCommandParameter(_dalRecord.CurrentList.ListName + "_S", int.MaxValue);
                                }
                                else
                                {
                                    AddCommandParameter(parmName, _dalRecord.Command.Parameters[1].Value);
                                }
                            }
                            else
                            {
                                AddCommandParameter(_dalRecord.CurrentList.ListName + "_S", string.Concat(STR_NEXTVALUEFOR, _dalRecord.CurrentList.ListSequenceObject));
                            }


                            junctionSColumn = parmName;
                            // Not used ? junctionSParm = _dalRecord.Command.Parameters[1].ParameterName;
                        }
                        if (IdentityColumnInfoHandler != null)
                        {
                            if (!string.IsNullOrEmpty(_dalRecord.CurrentList.JunctionTableName) && !string.IsNullOrEmpty(IdentityColumnInfoHandler.GetName(_dalRecord.CurrentList.JunctionTableName, junctionTableIDColumn)))
                            {
                                identityColumnName = string.Concat(IdentityColumnInfoHandler.GetName(_dalRecord.CurrentList.JunctionTableName, junctionTableIDColumn), ",");
                                identityColumnValue = string.Concat(IdentityColumnInfoHandler.GetValue(_dalRecord.CurrentList.JunctionTableName), ",");
                            }
                        }

                        _dalRecord.SetKeyParameters(_dalRecord.CurrentList.ListName);

                        //Check For Multi Member set
                        if (_dalRecord.CurrentList.MemberList != null && _dalRecord.CurrentList.MemberList.Count > 0)
                        {
                            //if (_dalRecord.CurrentList.ListOrd == ListOrder.SORTED)
                            //    _dalRecord.ColumnList.Replace(_dalRecord.Command.Parameters[_dalRecord.Command.Parameters.Count - 1].ParameterName.Substring(1),
                            //string.Concat(_dalRecord.CurrentList.ListName, STR_SORT));
                            //SAAQ Rule for table views - Needs updated for other table names
                            AddCommandParameter(_dalRecord.CurrentList.MultiMemberTypeKey, _dalRecord.TableName.Substring(1, 4));

                            _dalRecord.Command.CommandText = string.Format(string.Concat(
                            "Insert into {0} ({1} {2} ) Values ({3} {4} ); ",
                            "Update {5} set {6} = {7} where {8} = {9} ;  "),
                            CheckForSchema(_dalRecord.CurrentList.JunctionTableName), identityColumnName, _dalRecord.ColumnList,
                            identityColumnValue, _dalRecord.ColumnParms,
                            CheckForSchema(_dalRecord.TableName), identityColumnName.Replace(",", ""), identityColumnValue.Replace("NEXT ", "PREVIOUS ").Replace(",", ""),
                           _dalRecord.IDColumnName, _dalRecord.IDColumnValue);
                        }
                        else if (!isSystemOwnedList)
                        {
                            _dalRecord.Command.CommandText = string.Format(string.Concat(
                            "Insert into {0} ({1} {2} ) Values ({3} {4} ); ",
                            "Update {5} set {6} = {7} where {8} = {9} ;  "),
                            CheckForSchema(_dalRecord.CurrentList.JunctionTableName), identityColumnName, _dalRecord.ColumnList,
                            identityColumnValue, _dalRecord.ColumnParms, CheckForSchema(_dalRecord.TableName),
                             _dalRecord.CurrentList.ListName, _dalRecord.Command.Parameters[0].ParameterName,
                             _dalRecord.IDColumnName, _dalRecord.IDColumnValue);

                        }
                        else
                        {
                            if (hasJunctionTable)
                            {
                                _dalRecord.Command.CommandText = string.Format(string.Concat(
                               "Insert into {0} ({1} {2} ) Values ({3} {4} ); ",
                               "Update {5} set {6} = 'Y' where {7} = {8} ;  "),
                               CheckForSchema(_dalRecord.CurrentList.JunctionTableName), identityColumnName, _dalRecord.ColumnList, identityColumnValue, _dalRecord.ColumnParms,
                               CheckForSchema(_dalRecord.TableName), _dalRecord.CurrentList.ListName, _dalRecord.IDColumnName, _dalRecord.IDColumnValue);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(junctionSColumn))
                                {
                                    _dalRecord.Command.CommandText = string.Format("Update {0} set {1} = 'Y'  where {2} = {3} ;  ",
                                    CheckForSchema(_dalRecord.TableName), _dalRecord.CurrentList.ListName, _dalRecord.IDColumnName, _dalRecord.Command.Parameters[0].ParameterName);
                                }
                                else
                                {
                                    _dalRecord.Command.CommandText = string.Format("Update {0} set {1} = 'Y' , {2} = {3} where {4} = {5} ;  ",
                                    CheckForSchema(_dalRecord.TableName), _dalRecord.CurrentList.ListName, junctionSColumn, _dalRecord.Command.Parameters[1].Value,
                                    _dalRecord.IDColumnName, _dalRecord.Command.Parameters[0].ParameterName);
                                }
                            }
                        }

                    }
                }
                else
                // Exclude Logic
                {
                    if (_dalRecord.CurrentList.ListOrd == ListOrder.LinkList)
                    {
                        AddCommandParameter(STR_CurrentRowID, CurrentDbCurrency.GetListCurrentID(_dalRecord.CurrentList.ListName));
                        _dalRecord.SetKeyParameters(_dalRecord.CurrentList.ListName);
                        if (_dalRecord.CurrentList.ListOpt == ListOptions.MA)
                        // Exlcude linklist member from MA sets
                        {
                            _dalRecord.Command.CommandText = string.Format(string.Concat(
                                //Set current record pointers to null
                                //    "Update {0} set {1} = 0, {2} = 0 where {3} = {4} ;  ",
                                //Set Next pointer on prior Record 
                            "Update {0} set {1} = {5} where {3} = {6} ;  ",
                                //Set Prior pointer on Next Record 
                            "Update {0} set {2} = {6} where {3} = {5} ;  "),
                             CheckForSchema(_dalRecord.TableName), _linkListKey, _linkListKeyOpp,
                             _dalRecord.IDColumnName, _dalRecord.Command.Parameters[1].ParameterName, parmListPointerNext, parmListPointerPrior);
                        }
                        else
                        // Exclude linklist for Optional sets
                        {
                            _dalRecord.Command.CommandText = string.Format(string.Concat(
                            "Delete from {0} where {1} = {2} and {3} = {4}; ",
                                //Set current record pointers to null
                            "Update {5} set {6} = null, {7} = 0, {10} = 0 where {3} = {4} ;  ",
                                //Set Next pointer on prior Record 
                            "Update {5} set {7} = {8} where {3} = {9} ;  ",
                                //Set Prior pointer on Next Record 
                            "Update {5} set {10} = {9} where {3} = {8} ;  "),
                            CheckForSchema(_dalRecord.CurrentList.JunctionTableName), _dalRecord.CurrentList.OwnerCur.IdColName, _dalRecord.Command.Parameters[4].ParameterName,
                             _dalRecord.IDColumnName, _dalRecord.Command.Parameters[1].ParameterName,
                            CheckForSchema(_dalRecord.TableName), _dalRecord.CurrentList.ListName, _linkListKey,
                             parmListPointerNext, parmListPointerPrior, _linkListKeyOpp);
                        }
                        //TBD Add logic for SYSTEM owned Link lists (Rare)
                    }
                    else
                    {
                        _dalRecord.SetKeyParameters(_dalRecord.CurrentList.ListName);
                        if (!isSystemOwnedList)
                        {
                            if (_dalRecord.CurrentList.MemberList != null && _dalRecord.CurrentList.MemberList.Count > 0 && _dalRecord.CurrentList.ListOpt == ListOptions.MA)
                            {
                                _dalRecord.Command.CommandText = string.Format("Delete from {0} where {1} = {2} ; ",
                                 CheckForSchema(_dalRecord.CurrentList.JunctionTableName), _dalRecord.CurrentList.JunctionTableID, _dalRecord.CurrentList.MemberCur.CurrencyKeys[_dalRecord.CurrentList.JunctionTableID]);
                                //  _dalRecord.Command.CommandText = string.Format("Delete from {0} where {1} = {2} and {3} = {4}; ",
                                //_dalRecord.CurrentList.JunctionTableName, _dalRecord.CurrentList.OwnerCur.IdColName, _dalRecord.Command.Parameters[0].ParameterName,
                                // _dalRecord.IDColumnName, _dalRecord.Command.Parameters[1].ParameterName);
                            }
                            else
                            {
                                _dalRecord.Command.CommandText = string.Format(string.Concat(
                                "Delete from {0} where {1} = {2} and {3} = {4}; ",
                                "Update {5} set {6} = NULL where {3} = {4} ;  "),
                               CheckForSchema(_dalRecord.CurrentList.JunctionTableName), _dalRecord.CurrentList.OwnerCur.IdColName, _dalRecord.Command.Parameters[0].ParameterName,
                               _dalRecord.IDColumnName, _dalRecord.Command.Parameters[1].ParameterName, CheckForSchema(_dalRecord.TableName), _dalRecord.CurrentList.ListName);
                            }
                        }
                        else
                        {
                            if (hasJunctionTable)
                            {
                                _dalRecord.Command.CommandText = string.Format(string.Concat(
                                "Delete from {0} where {1} = {2} ; ",
                                "Update {3} set {4} = '' where {1} = {2} ;  "),
                                 CheckForSchema(_dalRecord.CurrentList.JunctionTableName), _dalRecord.IDColumnName, _dalRecord.IDColumnValue,
                                 CheckForSchema(_dalRecord.TableName), _dalRecord.CurrentList.ListName);
                            }
                            else
                            {
                                _dalRecord.Command.CommandText = string.Format("Update {0} set {1} = '' where {2} = {3} ;  ",
                                CheckForSchema(_dalRecord.TableName), _dalRecord.CurrentList.ListName, _dalRecord.IDColumnName, _dalRecord.Command.Parameters[0].ParameterName);
                            }
                        }

                    }
                }
            }

        }

        /// <summary>
        /// Create sql syntax for Deleting a row from a table
        /// </summary>
        private void Delete()
        {
            AddCommandParameter(STR_RowID, _dalRecord.IDColumnValue);
            if (_dBAccessType == DbAccessType.StoredProcedures)
            {
                switch (_dalRecord.DeleteType)
                {
                    case DeleteRowOption.CascadePermanent:
                        AddCommandParameter(STR_DeleteType, "PERM");
                        break;
                    case DeleteRowOption.CascadeSelective:
                        AddCommandParameter(STR_DeleteType, "SEL");
                        break;
                    case DeleteRowOption.CascadeAll:
                        AddCommandParameter(STR_DeleteType, "ALL");
                        break;
                    case DeleteRowOption.CascadeNone:
                        AddCommandParameter(STR_DeleteType, "NONE");
                        break;
                    default:
                        AddCommandParameter(STR_DeleteType, _dalRecord.DeleteType.ToString());
                        break;
                }
            }
            else
            {
                _dalRecord.Command.CommandText = string.Format("Delete from {0} where {1} = {2} ",
                    CheckForSchema(_dalRecord.TableName), _dalRecord.IDColumnName, parmRowID);
            }
        }

        /// <summary>
        /// Create sql syntax for Inserting new rows into a table
        /// </summary>
        private void Insert()
        {
            string identityColumnName = string.Empty; string identityColumnValue = string.Empty;
            string sequenceColumnName = string.Empty; string sequenceColumnValue = string.Empty;
            if (IdentityColumnInfoHandler != null)
            {
                if (!string.IsNullOrEmpty(IdentityColumnInfoHandler.GetName(_dalRecord.TableName, _dalRecord.IDColumnName)))
                {
                    identityColumnName = string.Concat(IdentityColumnInfoHandler.GetName(_dalRecord.TableName, _dalRecord.IDColumnName), ",");
                    identityColumnValue = string.Concat(IdentityColumnInfoHandler.GetValue(_dalRecord.TableName), ",");
                }
            }

            _dalRecord.GetColumnData();

            GetListKeyData(ref sequenceColumnName, ref sequenceColumnValue);

            GetDateTimeStamp();

            if (_dBAccessType == DbAccessType.DynamicSql)
            {
                if (sequenceColumnName == string.Empty)
                {
                    _dalRecord.Command.CommandText = string.Format("Insert into {0} ( {1} {2} ) Values ( {3} {4} ) {5} ",
                   CheckForSchema(_dalRecord.TableName), identityColumnName, _dalRecord.ColumnList.ToString(), identityColumnValue, _dalRecord.ColumnParms.ToString(), GetLastInsertedRow());
                }
                else
                {
                    _dalRecord.Command.CommandText = string.Format("Insert into {0} ( {1} {2} {6} ) Values ( {3} {4} {7} ) {5} ",
                    CheckForSchema(_dalRecord.TableName), identityColumnName, _dalRecord.ColumnList.ToString(), identityColumnValue, _dalRecord.ColumnParms.ToString(), GetLastInsertedRow(), sequenceColumnName, sequenceColumnValue);
                }
            }
        }

                /// <summary>
        /// Create sql syntax for selecting last inserted row
        /// </summary>
        private string GetLastInsertedRow()
        {

            string parmLatestID = STR_LatestID_SQLServer;
            if (_dbProviderName.Contains("Oracle"))
                parmLatestID = string.Empty;
            //string.Format(STR_LatestID_Oracle, string.Concat(_dalRecord.TableName, "_seq"));
            else if (_dbProviderName.Contains("DB2"))
                parmLatestID = STR_LatestID_DB2;
            if (IdentityColumnInfoHandler != null)
            {
                if (!string.IsNullOrEmpty(IdentityColumnInfoHandler.GetInsertedValue(_dalRecord.TableName)))
                {
                    parmLatestID = IdentityColumnInfoHandler.GetInsertedValue(_dalRecord.TableName);
                }
            }

            return parmLatestID;

        }

        /// <summary>
        /// Create message in log file
        /// </summary>
        /// <param name="msg"></param>
        private static void LogMessageToFile(string msg)
        {
            SimpleLogging.LogMandatoryMessageToFile(msg);
        }

        private void AddCommandParameter(string parmName, object parmValue)
        {
            if (!_dalRecord.Command.Parameters.Contains(string.Concat(_dalRecord.ParmPrefix, parmName)))
                _dalRecord.Command.Parameters.Add(_dalRecord.CreateParameter(parmName, parmValue));
        }

        /// <summary>
        /// Create DbParameter
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parmName"></param>
        /// <param name="parmValue"></param>
        /// <returns></returns>
        private DbParameter CreateParameter(DbCommand command, string parmName, object parmValue)
        {
            DbParameter param = command.CreateParameter();
            param.ParameterName = string.Concat(parmPrefix, parmName);
            param.Value = parmValue;
            return param;
        }

        private void GetListKeyData(ref string seqColName, ref string seqColVal)
        {
            RecordCurrency reccurrency = _dalRecord.CurrentRecord;
            ListCurrency listcurrency;
            if (reccurrency != null)
            {
                foreach (string lkey in reccurrency.ListNames.Keys)
                {
                    if (reccurrency.ListNames[lkey] == "Member")
                    {
                        if (CurrentDbCurrency.ListTable.ContainsKey(lkey))
                        {
                            listcurrency = (ListCurrency)CurrentDbCurrency.ListTable[lkey];
                            if (listcurrency == null) continue;
                        }
                        else
                        {
                            continue;
                        }

                        if (listcurrency.MemberCur == null || listcurrency.MemberCur.RecordName != _dalRecord.TableName)
                        {
                            listcurrency.MemberCur = reccurrency.Clone();
                        }
                        if (((listcurrency.ListDups == ListDuplicates.First) || (listcurrency.ListDups == ListDuplicates.Last)) &&
                             listcurrency.MemberCur.CurrencyKeys.ContainsKey(listcurrency.ListName + "_S"))
                        {
                            if (listcurrency.ListOpt == ListOptions.MM || listcurrency.ListOpt == ListOptions.OM)
                            {
                                AddCommandParameter(listcurrency.ListName + "_S", 0);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(listcurrency.ListSequenceObject))
                                {
                                    AddCommandParameter(listcurrency.ListName + "_S", int.MaxValue);
                                }
                                else
                                {
                                    //AddCommandParameter(listcurrency.ListName + "_S", string.Concat(STR_NEXTVALUEFOR, listcurrency.ListSequenceObject));
                                    seqColName = seqColName + "," + listcurrency.ListName + "_S";
                                    seqColVal = seqColVal + "," + string.Concat(STR_NEXTVALUEFOR, listcurrency.ListSequenceObject);
                                }
                            }
                        }

                        if (listcurrency.ListFkName != null)
                        {

                            if (listcurrency.ListOpt == ListOptions.MA || listcurrency.ListOpt == ListOptions.OA)
                                AddCommandParameter(listcurrency.ListFkName, CurrentDbCurrency.GetTableCurrentID(listcurrency.OwnerCur.RecordName));
                            else
                            {
                                AddCommandParameter(listcurrency.ListFkName, null);
                                //sqlcommand.Parameters[sqlcommand.Parameters.Count - 1].IsNullable = true;
                            }

                            if (listcurrency.ListOrd == ListOrder.LinkList)
                            {
                                if (listcurrency.ListOpt == ListOptions.MA || listcurrency.ListOpt == ListOptions.OA)
                                {
                                    // Update to check for null key - issue 5451
                                    if (listcurrency.MemberCur.RecordActionCode.ToString() == "DeletedRow" && listcurrency.MemberCur.CurrencyKeys[string.Concat(lkey, STR_PriorPointerSuffix)] != null)
                                        AddCommandParameter(string.Concat(lkey, STR_PriorPointerSuffix), Convert.ToInt64(listcurrency.MemberCur.CurrencyKeys[string.Concat(lkey, STR_PriorPointerSuffix)]));
                                    else
                                        AddCommandParameter(string.Concat(lkey, STR_PriorPointerSuffix), CurrentDbCurrency.GetListCurrentID(lkey));
                                }
                                else
                                {
                                    AddCommandParameter(string.Concat(lkey, STR_PriorPointerSuffix), 0);
                                    //sqlcommand.Parameters[sqlcommand.Parameters.Count - 1].IsNullable = true;
                                }
                                if (listcurrency.MemberCur.CurrencyKeys[string.Concat(lkey, STR_NextPointerSuffix)] == null || (listcurrency.ListPositionCode != ListStatus.OnMemberRow && listcurrency.ListActionCode != RowStatus.DeletedRow))
                                    AddCommandParameter(string.Concat(lkey, STR_NextPointerSuffix), 0);
                                //sqlcommand.Parameters[sqlcommand.Parameters.Count - 1].IsNullable = true;
                                else
                                    AddCommandParameter(string.Concat(lkey, STR_NextPointerSuffix), Convert.ToInt64(listcurrency.MemberCur.CurrencyKeys[string.Concat(lkey, STR_NextPointerSuffix)]));
                            }
                        }
                        else
                        {
                            // Check for System Owned Optional INdex
                            if (listcurrency.OwnerCur == null)
                            {
                                if (listcurrency.ListOpt != ListOptions.MA)
                                {
                                    if (listcurrency.ListOpt == ListOptions.OA)
                                        AddCommandParameter(listcurrency.ListName, "Y");
                                    else
                                        AddCommandParameter(listcurrency.ListName, "");

                                }
                            }

                        }
                    }

                }
            }
        }

        private void GetDateTimeStamp()
        {
            if (!string.IsNullOrEmpty(DateTimeStampName))
            {
                string updateDateTimeName = string.Format(DateTimeStampName, _dalRecord.TableName);
                AddCommandParameter(updateDateTimeName, DateTime.Now);
            }
        }

        /// <summary>
        /// Build key progression logic
        /// </summary>
        /// <returns></returns>
        private string GetKeyProgression(string firstPrefix)
        {
            StringBuilder progString = new StringBuilder();
            progString.Append("(");
            string extraKeys = string.Empty;
            string[] strArray = _dalRecord.CurrentList.ListKeys.Split(',');
            List<string> listKeySortOrder = new List<string>();
            List<string> listSortParms = new List<string>();
            foreach (string stringKey in strArray)
            {
                if (_dalRecord.SelectOrder == RowPosition.Prior || _dalRecord.SelectOrder == RowPosition.Last)
                {
                    if (stringKey.EndsWith(" ASC"))
                    {
                        listKeySortOrder.Add(parmSortOrderDesc);
                    }
                    else
                    {
                        listKeySortOrder.Add(parmSortOrderAsc);
                    }
                }
                else
                {
                    if (stringKey.EndsWith(" DESC"))
                    {
                        listKeySortOrder.Add(parmSortOrderDesc);
                    }
                    else
                    {
                        listKeySortOrder.Add(parmSortOrderAsc);
                    }
                }
                listSortParms.Add(stringKey.Replace(" ASC", "").Replace(" DESC", ""));
            }

            //Create progression key where logic 
            int ctr = 0;
            int keyCtr = listSortParms.Count;
            foreach (string keyParm in listSortParms)
            {
                for (int ctr2 = 0; ctr2 < keyCtr; ctr2++)
                {
                    if (ctr2 > 0) progString.Append(" and ");
                    if (keyCtr > ctr2 + 1)
                    {
                        progString.Append(string.Format("{0}.{1} = B.{1}", firstPrefix, listSortParms[ctr2]));
                    }
                    else
                    {
                        progString.Append(string.Format("{0}.{1} {2} B.{1}", firstPrefix, listSortParms[ctr2], GetFinalOperator(listKeySortOrder[ctr2])));
                    }
                }
                if (listSortParms.Count == ctr + 1)
                {
                    progString.Append(" ) ");
                }
                else
                {
                    progString.Append(" ) or ( ");
                }
                keyCtr--; ctr++;
            }

            return progString.ToString();
        }

        /// <summary>
        /// Builds a query string for Union logic of progression keys
        /// </summary>
        /// <param name="queryText"></param>
        /// <returns></returns>
        private string GetKeyProgressionUnionSql(string queryText, string listKeys = null)
        {
            StringBuilder progString = new StringBuilder();
            StringBuilder queryString = new StringBuilder();
            string[] strArray = listKeys == null ? _dalRecord.CurrentList.ListKeys.Split(',') : listKeys.Split(',');
            List<string> listKeySortOrder = new List<string>();
            List<string> listSortParms = new List<string>();
            string junctionTableColumns = string.Empty;
            if (queryText.Contains("inner join"))
            {
                junctionTableColumns = ",A.*";
            }
            foreach (string stringKey in strArray)
            {
                if (_dalRecord.SelectOrder == RowPosition.Prior || _dalRecord.SelectOrder == RowPosition.Last)
                {
                    if (stringKey.EndsWith(" ASC"))
                    {
                        listKeySortOrder.Add(parmSortOrderDesc);
                    }
                    else
                    {
                        listKeySortOrder.Add(parmSortOrderAsc);
                    }
                }
                else
                {
                    if (stringKey.EndsWith(" DESC"))
                    {
                        listKeySortOrder.Add(parmSortOrderDesc);
                    }
                    else
                    {
                        listKeySortOrder.Add(parmSortOrderAsc);
                    }
                }
                listSortParms.Add(stringKey.Replace(" ASC", "").Replace(" DESC", ""));
            }

            queryString.Append(string.Format(" Select T1.* {0} from {1}(", junctionTableColumns, parmTable));
            //Create progression key where logic 
            int ctr = 0;
            int keyCtr = listSortParms.Count;
            foreach (string keyParm in listSortParms)
            {
                progString.Clear();
                for (int ctr2 = 0; ctr2 < keyCtr; ctr2++)
                {
                    if (ctr2 > 0) progString.Append(" and ");
                    if (keyCtr > ctr2 + 1)
                    {
                        progString.Append(string.Format("A.{0} = B.{0}", listSortParms[ctr2]));
                    }
                    else
                    {
                        progString.Append(string.Format("A.{0} {1} B.{0}", listSortParms[ctr2], GetFinalOperator(listKeySortOrder[ctr2])));
                    }
                }
                if (ctr > 0)
                {
                    queryString.Append(string.Format(" union Select T{0}.* {1} from {2}(", (ctr + 1).ToString(), junctionTableColumns, parmTable));
                }
                queryString.Append(queryText.Replace(STR_KeyProgression, progString.ToString()));
                queryString.Append(string.Format(" ) T{0} ", (ctr + 1).ToString()));
                keyCtr--; ctr++;
            }

            return queryString.ToString();
        }

        private string GetFinalOperator(string sortOrder)
        {
            string returnOper;
            if (parmOrder.Contains("="))
            {
                if (sortOrder == "ASC")
                    returnOper = STR_GreaterThanEqualTo;
                else
                    returnOper = STR_LessThanEqualTo;
                parmOrder = parmOrder.Replace("=", "");
            }
            else
            {
                if (sortOrder == "ASC")
                    returnOper = STR_GreaterThan;
                else
                    returnOper = STR_LessThan;
            }
            return returnOper;
        }

        private string CheckForSchema(string tableName)
        {
            string returnName = tableName;
            if (!string.IsNullOrEmpty(SchemaName))
                returnName = string.Format("{0}.{1}", SchemaName, tableName);
            return returnName;
        }

        private static IIdentityColumnInfo GetIdentityColumnInfoHandlerObject()
        {
            try
            {
                return InversionContainer.GetImplementingObject<IIdentityColumnInfo>();
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
