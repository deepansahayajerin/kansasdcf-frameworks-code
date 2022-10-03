using System;
using System.Web;
using System.Data;
using System.Text;

using System.Collections;
using System.Data.Common;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer;
using MDSY.Framework.Buffer.BaseClasses;
using System.Collections.Generic;
using MDSY.Framework.Buffer.Common;


namespace MDSY.Framework.Data.IDMS
{
    public abstract class DalRecordBase : PredefinedRecordBase
    {

        #region Private Members
        private const string STR_Comma = ",";
        private byte[] saveBuffer;
        private ErrorStatusRecord _errorStatusRecord;

        #endregion

        #region Public Members

        protected internal DataTable dt = new DataTable();
        protected internal int DataTableCurrentRow = 0;
        protected internal PredefinedRecordBase BindRecord;

        public bool RefreshCache = false;
        /// <summary>
        /// Number of the start row.
        /// </summary>
        public int StartRow = 0;
        protected internal long StartID = 0;
        protected internal long InsertID = 0;
        protected string DalUserID { get; set; }
        protected string DalCallingProgram { get; set; }

        /// <summary>
        /// Sets and returns the error message text.
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// Sets and returns the name of the DAL record's key currency.
        /// </summary>
        public string IDColumnName { get; set; }

        /// <summary>
        /// Sets and returns the value of the DAL record's key value.
        /// </summary>
        public long IDColumnValue { get; set; }

        /// <summary>
        /// Sets and returns a reference to a ListCurrency object, which will be treated as a current set.
        /// </summary>
        public ListCurrency CurrentList { get; set; }

        /// <summary>
        /// Sets and returns a reference to a RecordCurrency object, which will be treated as a current record.
        /// </summary>
        public RecordCurrency CurrentRecord { get; set; }

        /// <summary>
        /// Sets and returns which record should be processed. 
        /// </summary>
        public RowPosition SelectOrder { get; set; }

        /// <summary>
        /// Sets and returns start position for SelectInListByKey
        /// </summary>
        public RowPosition SelectUsingPosition { get; set; }

        /// <summary>
        /// Sets and returns the text for WHERE criteria for SQL statements.
        /// </summary>
        public string WhereCriteria { get; set; }

        /// <summary>
        /// Sets and returns the name of the currency key, which will be used for search.
        /// </summary>
        public string SearchKey { get; set; }

        /// <summary>
        /// Sets and returns set update type. Can take "Exclude" and "Include" values.
        /// </summary>
        public string ListUpdateType { get; set; }

        /// <summary>
        /// Sets and returns parameter prefix. Can take value "@".
        /// </summary>
        public string ParmPrefix { get; set; }

        /// <summary>
        /// Sets and returns the name of the table that corresponds to the current DAL record.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Sets and returns the name of the last used set.
        /// </summary>
        public string LastListName { get; set; }

        /// <summary>
        /// Sets and returns record delete option.
        /// </summary>
        public DeleteRowOption DeleteType { get; set; }

        /// <summary>
        /// Sets and returns a reference to a DbCommand object.
        /// </summary>
        public DbCommand Command { get; set; }

        /// <summary>
        /// Sets and returns a reference to a DbOperation object.
        /// </summary>
        public DbOperation DBOperation { get; set; }

        /// <summary>
        /// Returns column names for the current SQL statement.
        /// </summary>
        public StringBuilder ColumnList { get; protected set; }

        /// <summary>
        /// Returns parameter names for the current SQL statement.
        /// </summary>
        public StringBuilder ColumnParms { get; protected set; }

        /// <summary>
        /// Returns column/value pairs for the current UPDATE SQL statement.
        /// </summary>
        public StringBuilder ColumnUpdateSets { get; protected set; }

        /// <summary>
        /// Returns column names and their set order for the current SQL statement.
        /// </summary>
        public List<string> SortKeyList { get;  set; }

        /// <summary>
        /// Specifies lengths of the key fields. 
        /// </summary>
        public Dictionary<string, int> ListKeyLengths { get; set; }

        /// <summary>
        /// Sets and returns portions that compose current SQL statement.
        /// </summary>
        public List<string> MultipleKeysSqlList { get; set; }

        /// <summary>
        /// Specifies whether one or multiple key fields must be used for processing current command.
        /// </summary>
        public bool UsingOneFieldOnly { get; set; }

        /// <summary>
        /// ALternate View Name to be used on Select queries
        /// </summary>
        public string AlternateViewName { get; set; }

        /// <summary>
        /// If true, use Union type query for Paging Set Data instead of CTE
        /// </summary>
        public bool UseAlternateQueryForLargeTable { get; set; }

        /// <summary>
        /// If true, use the binary view to read data.
        /// </summary>
        public bool UseBinaryViewToReadData { get; set; }
        /// <summary>
        /// List of Sets and columns that require EBCIDIC sorting
        /// </summary>  
        public Dictionary<string,string> ListsWithEBCDICSort { get; set; }

        /// <summary>
        /// Saves and returns a copy of the record's buffer.
        /// </summary>
        public byte[] SaveBuffer
        {
            get
            {
                if (saveBuffer == null) return null;
                else
                {
                    byte [] tmpBuffer = new byte [saveBuffer.Length];
                    Array.Copy(saveBuffer, tmpBuffer, saveBuffer.Length);
                    return tmpBuffer;
                }
            }
            set
            {
                if (value == null) saveBuffer = null;
                else
                {
                    saveBuffer = new byte[value.Length];
                    Array.Copy(value, saveBuffer, value.Length);
                }
            }
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

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates an instance of DalRecorBase class, sets table name and the record's (primary) key column name.
        /// </summary>
        /// <param name="recname"></param>
        /// <param name="idColumn"></param>
        public DalRecordBase(string recname, string idColumn)
            : base()
        {
            TableName = recname;
            IDColumnName = idColumn;
            ColumnList = new StringBuilder();
            ColumnParms = new StringBuilder();
            ColumnUpdateSets = new StringBuilder();
            LastListName = string.Empty;
            UsingOneFieldOnly = false;
            ListKeyLengths = new Dictionary<string, int>();
            SelectUsingPosition = RowPosition.All;
            _errorStatusRecord = new ErrorStatusRecord();
            _errorStatusRecord.Record.ResetToInitialValue();
            UseAlternateQueryForLargeTable = false;
            UseBinaryViewToReadData = false;
            DalUserID = GlobalVariables.UserID;
            DalCallingProgram = GlobalVariables.ProgramName;
            ListsWithEBCDICSort = new Dictionary<string, string>();
        }

        #endregion

        #region virtual  Methods

        /// <summary>
        /// Virtual method. If not overridden, assings BindRecord. 
        /// </summary>
        public virtual void GetColumnData()
        {
            if (BindRecord != null)
            {
                this.AssignFrom(BindRecord);
            }
        }

        /// <summary>
        /// Virtual method. If not overridden, does nothing.
        /// </summary>
        /// <param name="KeyType">type of the key</param>
        public virtual void SetKeyParameters(string KeyType)
        {
        }

        /// <summary>
        /// Virtual method. If not overridden, does nothing.
        /// </summary>
        /// <param name="KeyType">type of the key</param>
        public virtual void GetKeyParameters(string KeyType)
        {
        }

        /// <summary>
        /// Virtual method. If not overridden, does nothing.
        /// </summary>
        /// <param name="KeyType">type of the key</param>
        /// <param name="KeyString">key string</param>
        public virtual void SetKeyUsingParameters(string KeyType, string KeyString)
        {
        }

        /// <summary>
        /// Virtual method. If not overridden, does nothing.
        /// </summary>
        /// <param name="KeyType">type of the key</param>
        /// <param name="KeyString">key string</param>
        public virtual void SetKeyValue(string KeyType, string KeyString)
        {
        }

        /// <summary>
        /// Virtual method. If not overridden, returns false.
        /// </summary>
        /// <param name="KeyType">type of the key</param>
        /// <param name="KeyString">key string</param>
        /// <returns>false</returns>
        public virtual bool IsKeyMatch(string KeyType, string KeyString)
        {
            return false;
        }

        /// <summary>
        /// Virtual method. If not overridden, returns empyt string.
        /// </summary>
        /// <param name="KeyType">type of the key</param>
        /// <returns>empty string</returns>
        public virtual string GetKeySelectLogic(string KeyType)
        {
            return string.Empty;
        }

        /// <summary>
        /// Virtual method. If not overridden, sets buffert bytes to BindRecord (if BindRecord is not null).
        /// </summary>
        public virtual void SetRecordData()
        {
            if (BindRecord != null)
            {
                BindRecord.SetValue(this.Buffer.ReadBytes());
            }
        }

        public virtual void SetRecordBinaryData()
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Attaches provided record instance to CurrentDalRecords collection.
        /// </summary>
        /// <param name="db">record to be bound</param>
        public void BindDBRecord(DBConversation db)
        {
            if (BindRecord != null && saveBuffer != null)
            {
                this.Buffer.WriteBytes(saveBuffer);
                saveBuffer = null;
            }
            BindRecord = null;
            db.SetReturnCode(0);
            if (db.CurrentDalRecords.ContainsKey(RecordName))
            {
                db.CurrentDalRecords[RecordName] = this;
            }
            else
            {
                db.CurrentDalRecords.Add(RecordName, this);
            }
        }

        /// <summary>
        /// Attaches provided record instance to CurrentDalRecords collection.
        /// </summary>
        /// <param name="db">Associates current record object with the DBConversation object.</param>
        /// <param name="record">References a record where the buffer content will be copied.</param>
        public void BindDBRecord(DBConversation db, PredefinedRecordBase record)
        {
            if (this.Record.Name == record.Record.Name)
            {
                BindRecord = null;
                if (saveBuffer != null)
                {
                    this.Buffer.WriteBytes(saveBuffer);
                    saveBuffer = null;
                }
            }
            else
            {
                saveBuffer = this.Buffer.ReadBytes();
                //this.SetBufferReference(record.Record);
                BindRecord = record;
            }
            db.SetReturnCode(0);
            if (!db.CurrentDalRecords.ContainsKey(RecordName))
                db.CurrentDalRecords.Add(RecordName, this);
        }

        /// <summary>
        ///  Deletes current row from local data table
        /// </summary>
        public void SetCurrentRowAsDeleted()
        {
            if (dt.Rows.Count > 0 && (DataTableCurrentRow + 1) <= dt.Rows.Count )
                dt.Rows[DataTableCurrentRow].Delete();
        }

        /// <summary>
        /// Returns substring from Record text
        /// </summary>
        /// <param name="startPos">Start position of the substring within a string.</param>
        /// <param name="length">Lenght of the substring.</param>
        /// <returns></returns>
        public string GetSubstring(int startPos, int length)
        {
            return Record.GetSubstring(startPos, length);
        }

        /// <summary>
        /// Returns a key string after handling binary data
        /// </summary>
        /// <param name="keyString">Key string.</param>
        /// <param name="fieldpos">Position of the field.</param>
        /// <param name="fieldlength">Length of the field.</param>
        /// <param name="fieldtype">Type of the field.</param>
        /// <param name="sortOrder">Optional parameter, which specifies sorting order. Default value is "ASC".</param>
        /// <returns>Key string.</returns>
        public string SetKeyByteData(string keyString, short fieldpos, short fieldlength, string fieldtype, string sortOrder = "ASC")
        {
            StringBuilder hexstring = new StringBuilder();

            byte[] bytes = new byte[fieldlength * sizeof(char)];
            //If keystring is long enough for the value, get substring of contents, else return zeros
            if (keyString.Length >= fieldpos + fieldlength)
            {
                //Updated for issue 9863
                char[] ByteChars = keyString.Substring(fieldpos, fieldlength).ToCharArray();
                string hex = "";
                foreach (char c in ByteChars)
                {
                    int tmp = Convert.ToInt32(c);
                    hexstring.Append(String.Format("{0:x2}", tmp).ToUpper());
                }
                int ctr = 0;
                foreach (char c in hex)
                {
                    bytes[ctr] = Convert.ToByte(c);
                    ctr++;
                }
  //              System.Buffer.BlockCopy(keyString.Substring(fieldpos, fieldlength).ToCharArray(), 0, bytes, 0, bytes.Length);
            }
            else
            {
                return "0".PadRight(fieldlength,'0');
            }

            if (fieldtype == "PackedDecimal" && !UsingOneFieldOnly)
            {
                //Following lines commented out for issue 9863
                //for (int ctr = 0; ctr < bytes.Length; ctr = ctr + 2)
                //{
                //    if (String.Format("{0:X}", bytes[ctr]) == "F" || String.Format("{0:X}", bytes[ctr]) == "D" || String.Format("{0:X}", bytes[ctr]) == "C")
                //        hexstring.Append("0");
                //    hexstring.Append(String.Format("{0:X}", bytes[ctr]));
                //}
                if (hexstring[hexstring.Length - 1] != 'D' && hexstring[hexstring.Length - 1] != 'C' & hexstring[hexstring.Length - 1] != 'F' )
                    return keyString.Substring(fieldpos, fieldlength);
                if (hexstring[hexstring.Length - 1] == 'D')
                {
                    hexstring.Insert(0, "-");
                }
                hexstring.Remove(hexstring.Length - 1, 1);
                return hexstring.ToString();
            }
            else
                return keyString;
        }

        /// <summary>
        /// Returns Key string from multi member key field
        /// </summary>
        /// <param name="keystring">Key string.</param>
        /// <param name="fieldpos">Position of the field.</param>
        /// <param name="fieldlength">Lenth of the field.</param>
        /// <param name="fieldtype">Type of the field.</param>
        /// <param name="sortOrder">Optional parameter, which specifies sorting order. Default value is "ASC".</param>
        /// <returns></returns>
        public string SetKeyData(string keystring, short fieldpos, short fieldlength, string fieldtype, string sortOrder = "ASC")
        {
            string thisKey = string.Empty;
            if (keystring.Length >= fieldpos + fieldlength)
                thisKey = keystring.Substring(fieldpos, fieldlength);
            else if (keystring.Length > fieldpos)
                thisKey = keystring.Substring(fieldpos);
            //else
            //    thisKey = keystring.Substring(keystring.Length - fieldlength);

            // Following logic added for sending string parms to DB2 through DB2Connect 
            if (fieldtype == "String")
            {
                if (thisKey.Contains(AsciiChar.MaxValue.AsChar.ToString()))
                {
                    thisKey= thisKey.Replace(AsciiChar.MaxValue.AsChar, AsciiChar.Db2ConnectHighValue.AsChar);
                }
            }

            if (fieldtype == "Integer" || fieldtype == "Short" || fieldtype == "Decimal" || fieldtype == "PackedDecimal")
            {
                //For numerics, trim the keystring so there are no spaces. This will result
                //in a keystring of "   " returning from SetKeyData as "000", which is what we want.
                //Note keystring is not passed by ref, so it's value is unchanged externally.

                thisKey = thisKey.Trim();
            }
            int currkeypos = fieldpos;
            int currfieldpos = 0;
            StringBuilder sbField = new StringBuilder(thisKey.Length);
            sbField.Insert(0, thisKey);

            currfieldpos = thisKey.Length;
            while (currfieldpos < fieldlength)
            {
                if (sortOrder == "DESC")
                {
                    // Following is for comparing DB2 values when key is descending
                    sbField.Insert(currfieldpos, "9");
                }
                else
                {
                    if (fieldtype == "String")
                    {
                        sbField.Insert(currfieldpos, " ");
                    }
                    else if (fieldtype == "Integer" || fieldtype == "Short" || fieldtype == "Decimal")
                    {
                        sbField.Insert(0, "0");
                    }
                    else
                    {
                        sbField.Insert(currfieldpos, " ");
                    }
                }
                currfieldpos++;
            }

            if (fieldtype == "Date")
            {
                if (sbField.ToString() == "00000000")
                    sbField.Replace("00000000", "10010101");
                if (sbField.ToString() == "        ")
                    sbField.Replace("        ", "10010101");
                sbField.Insert(6, "-");
                sbField.Insert(4, "-");
            }

            return sbField.ToString();
        }

        /// <summary>
        /// Create database Command paramter  
        /// </summary>
        /// <param name="parmName">Parameter name.</param>
        /// <param name="parmValue">Parameter value.</param>
        /// <param name="skipColumnBuild">Optional parameter for INSERT and UPDATE commands. Default value is false.</param>
        /// <returns></returns>
        public DbParameter CreateParameter(string parmName, object parmValue, bool skipColumnBuild = false)
        {
            DbParameter param = Command.CreateParameter();
            param.ParameterName = string.Concat(ParmPrefix, parmName);
            if (parmValue is IField)
            {
                IField tempField = (IField)parmValue;
                if (tempField.IsNumericType)
                {
                    if (tempField.IsNumericValue())
                    {
                        param.Value = tempField.GetValue<decimal>();
                    }
                    else
                    {
                        param.Value = 0;
                    }
                }
                else if (tempField.AsBytes[0] == 0x00)
                {
                    tempField.Assign(" ");
                }
                else
                {
                    param.Value = tempField.AsString();
                }
            }
            else
            {
                param.Value = parmValue;
            }

            // Create Insert and Update lists
            if (parmName != "RowID" && parmName != "TblRowID" && !skipColumnBuild)
            {
                if (DBOperation == DbOperation.Update || DBOperation == DbOperation.Insert)
                {
                    if (ColumnList.Length > 0)
                    {
                        ColumnList.Append(STR_Comma);
                        ColumnParms.Append(STR_Comma);
                        if (DBOperation == DbOperation.Update)
                            ColumnUpdateSets.Append(STR_Comma);
                    }
                    ColumnList.Append(parmName);
                    ColumnParms.Append(param.ParameterName);
                    if (DBOperation == DbOperation.Update)
                        ColumnUpdateSets.Append(string.Concat(parmName, " = ", param.ParameterName));
                }
            }

            return param;
        }

        /// <summary>
        /// Sets Buffer data from another Record or initializes buffer
        /// </summary>
        /// <param name="recordData">A reference to another IRecord object, which buffer data must be copied to the buffer of the current object.
        /// Record's buffer is initialized to the initial value if this parameter is null of if it contains all nulls in its buffer.</param>
        /// <param name="isNewCopy">Indicates whether a new copy of buffer data should be created. Record's buffer is initialized to the initial value if this parameter is true.</param>
        public void SetBufferData(IRecord recordData, bool isNewCopy)
        {
            if (isNewCopy || recordData == null || recordData.AsBytes() == null)
            {
                this.Record.ResetToInitialValue();
            }
            else 
            {
                this.Record.AssignFrom(recordData);
            }
        }

        /// <summary>
        /// Returns string of Record buffer contents
        /// </summary>
        /// <returns>Returns a string representation of the record's buffer contents.</returns>
        public new string AsString()
        {
            return Record.AsString();
        }

        /// <summary>
        /// Initializes Record buffer
        /// </summary>
        public override void Initialize()
        {
            ResetToInitialValue();
            DataTableCurrentRow = 0; StartRow = 0; StartID = 0; IDColumnValue = 0;
            dt = new DataTable();
        }

        /// <summary>
        /// Sets return paramter for DalRecord parms
        /// </summary>
        /// <param name="arg">A reference to another DAL record, which takes contents of the current record.</param>
        public void SetDalReturnParm(IBufferValue arg)
        {
            arg.SetValue((IBufferValue)this);
            if (arg is DalRecordBase)
            {
                DalRecordBase tempRec = (DalRecordBase)arg;
                tempRec.CurrentList = this.CurrentList;
                tempRec.CurrentRecord = this.CurrentRecord;
                tempRec.LastListName = this.LastListName;
                tempRec.dt = this.dt;
                tempRec.DataTableCurrentRow = this.DataTableCurrentRow;
                tempRec.IDColumnValue = this.IDColumnValue;
            }

        }

        /// <summary>
        /// Sets local DalRecord from passed DalRecord parm
        /// </summary>
        /// <param name="arg">A reference to another DAL record, which content is copied to the current record.</param>
        public void SetDalPassedParm(IBufferValue arg)
        {
            this.SetValue((IBufferValue)arg);
            if (arg is DalRecordBase)
            {
                DalRecordBase tempRec = (DalRecordBase)arg;
                this.CurrentList = tempRec.CurrentList;
                this.CurrentRecord = tempRec.CurrentRecord;
                this.LastListName = tempRec.LastListName;
                this.dt = tempRec.dt;
                this.DataTableCurrentRow = tempRec.DataTableCurrentRow;
                this.IDColumnValue = tempRec.IDColumnValue;
            }
        }

        public void SetDataTable(DataTable newDataTable)
        {
            dt = newDataTable;
        }

        public void SetCurrentRowCounter(int currentRow)
        {
            DataTableCurrentRow = currentRow;
        }

        public void SetReturnCode(int returnCode)
        {
            _errorStatusRecord.ReturnCode.SetValue(returnCode);
            _errorStatusRecord.ErrStatSave.Assign(returnCode);
        }
        #endregion
    }
}
