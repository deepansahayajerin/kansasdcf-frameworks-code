#region Using
using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

using System.Collections;
using MDSY.Framework.Core;
#endregion

namespace MDSY.Framework.Data.IDMS
{
    public class LogicalRecordBase// : Record_Old
    {
        #region Private Members
        private string _logicalRecordName;
        private string _logicalRecordStatus;
        #endregion

        #region Protected Members
        protected internal int StartRow = 0;
        protected internal int StartID = 0;
        protected internal int InsertID = 0;
        protected internal Dictionary<string, string> WhereParms = new Dictionary<string, string>();
        #endregion

        #region Public Properties

        /// <summary>
        /// Sets and returns a reference to teh DBConversation object, which is associated with current record.
        /// </summary>
        public DBConversation DBConv
        {
            get;
            set;
        }

        /// <summary>
        /// Sets and returns record's status value. 
        /// </summary>
        public string LogicalRecordStatus
        {
            get { return _logicalRecordStatus; }
            set { _logicalRecordStatus = value; }
        }

        /// <summary>
        /// Sets and returns a reference to the collection of logical record path info objects.
        /// </summary>
        public List<LogicalRecordPathInfo> LogicalRecordParmInfoList
        {
            get;
            set;
        }

        /// <summary>
        /// Sets and returns a reference to the collection of logical record parameters.
        /// </summary>
        public List<LogicalRecordFilterParm> LogicalRecordFilterParmList
        {
            get;
            set;
        }

        /// <summary>
        /// Sets and returns passed in direction value.
        /// </summary>
        public RowPosition PassedInDirection
        {
            get;
            set;
        }
        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates an instance of LogicalRecordBase class and initializes it with the parameter values.
        /// </summary>
        /// <param name="lrname">The name of the logical record.</param>
        /// <param name="_dbconv">A reference to the DBConversation object, which is associated with this instance of LogicalRecordBase.</param>
        public LogicalRecordBase(string lrname, DBConversation _dbconv)
            : base()
        {
            _logicalRecordName = lrname;
            DBConv = _dbconv;
            LogicalRecordParmInfoList = new List<LogicalRecordPathInfo>();
            LogicalRecordFilterParmList = new List<LogicalRecordFilterParm>();
        }

        #endregion

        #region virtual  Methods

        /// <summary>
        /// Virtual method. Does nothing if not overridden.
        /// </summary>
        public virtual void SetRecordData()
        {
        }

        /// <summary>
        /// Composed and returns a key data string.
        /// </summary>
        /// <param name="keystring">The name of the key field.</param>
        /// <param name="fieldpos">Position of the key field.</param>
        /// <param name="fieldlength">Length of the key field.</param>
        /// <param name="fieldtype">Type of the key field.</param>
        /// <returns>Key data string</returns>
        public string SetKeyData(string keystring, short fieldpos, short fieldlength, string fieldtype)
        {
            if (fieldtype == "Integer" || fieldtype == "Short" || fieldtype == "Decimal")
            {
                //For numerics, trim the keystring so there are no spaces. This will result
                //in a keystring of "   " returning from SetKeyData as "000", which is what we want.
                //Note keystring is not passed by ref, so it's value is unchanged externally.
                keystring = keystring.Trim();
            }

            short currkeypos = fieldpos;
            short currfieldpos = 0;
            StringBuilder sbField = new StringBuilder(fieldlength);
            while (currkeypos < keystring.Length && currfieldpos < fieldlength)
            {
                sbField.Insert(currfieldpos, keystring[currkeypos]);
                currkeypos++;
                currfieldpos++;
            }
            while (currfieldpos < fieldlength)
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
                currfieldpos++;
            }

            return sbField.ToString();
        }

        /// <summary>
        /// Virtual method. Does nothing if not overridden.
        /// </summary>
        /// <param name="selectedPathName">Selected path</param>
        /// <param name="dbExceptions">Auto state exceptions.</param>
        public virtual void ExecuteSelectedPath(string selectedPathName, params DbAllow[] dbExceptions)
        {
        }

        /// <summary>
        /// Checks action and where conditions for the best path.
        /// </summary>
        /// <param name="Action">Action value.</param>
        /// <param name="WhereClause">Where clause condtions text.</param>
        /// <returns>Best path.</returns>
        public string FindBestPath(string Action, string WhereClause)
        {
            WhereParms = new Dictionary<string, string>();
            LogicalRecordFilterParmList = new List<LogicalRecordFilterParm>();

            ParseWhereForCompare(WhereClause);

            string BestPath = "";
            foreach (LogicalRecordPathInfo logicalRecordPathInfo in this.LogicalRecordParmInfoList)
            {
                if (logicalRecordPathInfo.Action == Action)
                {
                    if (GoodForPath(logicalRecordPathInfo.LogicalRecordParmList, WhereParms))
                    {
                        BestPath = logicalRecordPathInfo.PathGroupName;
                        break;
                    }
                }
            }

            return BestPath;
        }

        #endregion

        #region Private Methods

        private void ParseWhereForCompare(string whereClause)
        {
            whereClause = whereClause.ToUpper();

            string[] Splitters = { " AND ", " OR " };
            string[] TestParms = whereClause.Split(Splitters, System.StringSplitOptions.RemoveEmptyEntries);

            for (int x = 0; x < TestParms.Length; x++)
            {
                string[] Operators = { " EQ ", "=", " GT ", ">", " LT ", "<", " GE ", ">=", " LE ", "<=" };
                string[] Operands = TestParms[x].Split(Operators, System.StringSplitOptions.RemoveEmptyEntries);

                if (Operands.Length == 1)
                {
                    WhereParms.Add(Operands[0], "KEYWORD");
                }
                else if (TestParms[x].Contains("=") || TestParms[x].Contains(" EQ "))
                {
                    if (!WhereParms.ContainsKey(Operands[0]))
                    {
                        WhereParms.Add(Operands[0], "FIELDNAME-EQ");
                    }
                    AddToFilterParmList(TestParms[x]);
                }
                else
                {
                    if (!WhereParms.ContainsKey(Operands[0]))
                    {
                        WhereParms.Add(Operands[0], "FIELDNAME");
                    }
                    AddToFilterParmList(TestParms[x]);
                }
            }
        }

        private bool GoodForPath(IDictionary<string, string> PathParms, IDictionary<string, string> WhereParms)
        {
            bool retValue = true;
            foreach (KeyValuePair<string, string> pathParm in PathParms)
            {
                if (!(WhereParms.ContainsKey(pathParm.Key)))
                {
                    retValue = false;
                    break;
                }
            }
            return retValue;
        }
        private void AddToFilterParmList(string PartOfWhere)
        {
            string[] Splitters = { " " };
            string[] Words = PartOfWhere.Split(Splitters, StringSplitOptions.RemoveEmptyEntries);
            LogicalRecordFilterParm logicalRecordFilterParm = new LogicalRecordFilterParm();
            logicalRecordFilterParm.ParmOperator = "AND";
            logicalRecordFilterParm.RecordName = (Words[0].Contains(".") ? Words[0].Substring(0, Words[0].IndexOf(".")) : Words[0]);
            logicalRecordFilterParm.FieldName = (Words[0].Contains(".") ? Words[0].Substring(Words[0].IndexOf(".") + 1) : Words[0]);
            logicalRecordFilterParm.Operator = Words[1];
            logicalRecordFilterParm.Operator = (logicalRecordFilterParm.Operator == "EQ" ? "=" : logicalRecordFilterParm.Operator);
            logicalRecordFilterParm.Operator = (logicalRecordFilterParm.Operator == "NE" ? "!=" : logicalRecordFilterParm.Operator);
            logicalRecordFilterParm.Operator = (logicalRecordFilterParm.Operator == "GT" ? ">" : logicalRecordFilterParm.Operator);
            logicalRecordFilterParm.Operator = (logicalRecordFilterParm.Operator == "LT" ? "<" : logicalRecordFilterParm.Operator);
            logicalRecordFilterParm.Operator = (logicalRecordFilterParm.Operator == "GE" ? ">=" : logicalRecordFilterParm.Operator);
            logicalRecordFilterParm.Operator = (logicalRecordFilterParm.Operator == "LE" ? "<=" : logicalRecordFilterParm.Operator);
            logicalRecordFilterParm.FieldValue = Words[2];
            //logicalRecordFilterParm.FieldValue = logicalRecordFilterParm.FieldValue.Replace("'", "");
            LogicalRecordFilterParmList.Add(logicalRecordFilterParm);
        }

        public string GetFilterParmValue(string recordName, string fieldName)
        {
            string returnValue = "";
            foreach (LogicalRecordFilterParm logicalRecordFilterParm in LogicalRecordFilterParmList)
            {
                if (logicalRecordFilterParm.RecordName == recordName && logicalRecordFilterParm.FieldName == fieldName)
                {
                    returnValue = logicalRecordFilterParm.FieldValue;
                    returnValue = returnValue.Replace("'", "");
                    break;
                }
            }
            return returnValue;
        }
        public void SetLRFilter(string recordName, string extraWhere)
        {
            string returnWhere = "";
            returnWhere += extraWhere;

            foreach (LogicalRecordFilterParm logicalRecordFilterParm in LogicalRecordFilterParmList)
            {
                if (logicalRecordFilterParm.RecordName == recordName)
                {
                    returnWhere += String.Format(" AND [{0}].[{1}] {2} {3}",
                        recordName, logicalRecordFilterParm.FieldName, logicalRecordFilterParm.Operator, logicalRecordFilterParm.FieldValue);
                }
            }
            DBConv.WhereCriteria = returnWhere;
        }
        #endregion
    }
}
