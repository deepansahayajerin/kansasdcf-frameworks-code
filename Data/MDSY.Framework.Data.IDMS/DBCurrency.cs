using System;
using System.Collections.Specialized;
using System.Text;

using System.Collections.Generic;
using MDSY.Framework.Core;

namespace MDSY.Framework.Data.IDMS
{
    /// <summary>
    /// DBCurrency class represents IDMS currencies and provides API methods for working with currencies. 
    /// It contains ListCurrency instance as a private member and has one public constructor, 
    /// which is called when a new DBConversation instance is created or when DBCurrency object is cloned.
    /// </summary>
    [Serializable]
    public class DBCurrency
    {
        #region Private Properties
        //private HybridDictionary recordTable = new HybridDictionary();
        //private HybridDictionary listTable = new HybridDictionary();
        //private int currentIdCol;
        //private string currentRecordName;
        //private string _conversationName;
        private ListCurrency _listcurrency;
        #endregion

        #region Public Properties

        private long _currentIdCol = 0;
        public long CurrentIdCol
        {
            get
            {
                MDSY.Framework.Buffer.FieldEx.IdRecordName = CurrentRecordName;
                return _currentIdCol;
            }
            set { _currentIdCol = value; }
        }

        public string CurrentRecordName
        {
            get;
            set;
        }
        public string ConversationName
        {
            get;
            set;
        }
        public Dictionary<string, RecordCurrency> RecordTable
        {
            get;
            set;
        }
        public Dictionary<string, ListCurrency> ListTable
        {
            get;
            set;
        }
        #endregion

        #region Public Constructors
        /// <summary>
        /// Constructor of DBCurrency class.
        /// </summary>
        public DBCurrency()
        {
            CurrentIdCol = 0;
            CurrentRecordName = string.Empty;
            ConversationName = string.Empty;
            ListTable = new Dictionary<string, ListCurrency> { };
            RecordTable = new Dictionary<string, RecordCurrency> { };
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Clear the currency key tables
        /// </summary>
        public void ClearCurrency()
        {
            CurrentIdCol = 0;
            CurrentRecordName = "";

            foreach (RecordCurrency rCurrency in RecordTable.Values)
            {
                string[] keyarray = new string[rCurrency.CurrencyKeys.Count];
                rCurrency.CurrencyKeys.Keys.CopyTo(keyarray, 0);
                foreach (string keyString in keyarray)
                {
                    rCurrency.CurrencyKeys[keyString] = null;
                }
            }
            foreach (ListCurrency lCurrency in ListTable.Values)
            {
                if (lCurrency == null)
                    continue;
                if (lCurrency.OwnerCur != null)
                {
                    string[] keyarray = new string[lCurrency.OwnerCur.CurrencyKeys.Count];
                    lCurrency.OwnerCur.CurrencyKeys.Keys.CopyTo(keyarray, 0);
                    foreach (string keyString in keyarray)
                    {
                        lCurrency.OwnerCur.CurrencyKeys[keyString] = null;
                    }
                }
                if (lCurrency.MemberCur != null)
                {
                    string[] keyarray = new string[lCurrency.MemberCur.CurrencyKeys.Count];
                    lCurrency.MemberCur.CurrencyKeys.Keys.CopyTo(keyarray, 0);
                    foreach (string keyString in keyarray)
                    {
                        lCurrency.MemberCur.CurrencyKeys[keyString] = null;
                    }
                }
                lCurrency.ListActionCode = RowStatus.NoRow;
                lCurrency.ListPositionCode = ListStatus.OnNone;
            }
        }

        /// <summary>
        /// Returns the value of key column from the specified record currency table. 
        /// </summary>
        /// <param name="TableName">Specifies table name</param>
        /// <returns>0 if table is not found or currency is not defined.</returns>
        public long GetTableCurrentID(string TableName)
        {
            if (RecordTable.ContainsKey(TableName))
            {
                if (RecordTable[TableName].CurrencyKeys[RecordTable[TableName].IdColName] != null)
                    return Convert.ToInt64(RecordTable[TableName].CurrencyKeys[RecordTable[TableName].IdColName]);
            }
            return 0;
        }

        /// <summary>
        /// Returns the value of owner key column (foreign key) from the specified list (set) currency table. 
        /// </summary>
        /// <param name="ListName">Specifies list (set) name.</param>
        /// <param name="isGetOwner">Bool parameter is a flag, which indicates that this method is called from GetOwner() method. 
        /// Default value is “false”. </param>
        /// <returns>0 if table is not found or currency is not defined.</returns>
        public long GetListOwnerID(string ListName, bool isGetOwner = false)
        {
            long returnID = 0;
            if (ListTable.ContainsKey(ListName) && ListTable[ListName].OwnerCur != null)
            {
                _listcurrency = ListTable[ListName];
                if (_listcurrency.ListPositionCode == ListStatus.OnMemberRow && _listcurrency.ListActionCode != RowStatus.MissOnUsing)
                {
                    if (_listcurrency.MemberCur != null && _listcurrency.MemberCur.CurrencyKeys[_listcurrency.ListFkName] != null && _listcurrency.MemberCur.CurrencyKeys[_listcurrency.ListFkName] != DBNull.Value)
                    {
                        returnID = Convert.ToInt64(_listcurrency.MemberCur.CurrencyKeys[_listcurrency.ListFkName]);
                    }
                }
                else
                {
                    if (_listcurrency.OwnerCur == null && _listcurrency.ListFkName != null)
                    {
                        if (_listcurrency.MemberCur != null && _listcurrency.MemberCur.CurrencyKeys[_listcurrency.ListFkName] != null && _listcurrency.MemberCur.CurrencyKeys[_listcurrency.ListFkName] != DBNull.Value)
                        {
                            returnID = Convert.ToInt64(_listcurrency.MemberCur.CurrencyKeys[_listcurrency.ListFkName]);
                        }
                        else
                            return 0;
                    }
                    else if (_listcurrency.OwnerCur.CurrencyKeys[_listcurrency.OwnerCur.IdColName] != null)
                    {
                        if (_listcurrency.OwnerCur.CurrencyKeys[_listcurrency.OwnerCur.IdColName] != DBNull.Value)
                            returnID = Convert.ToInt64(_listcurrency.OwnerCur.CurrencyKeys[_listcurrency.OwnerCur.IdColName]);
                    }
                }
                // If there is a second set, override the ID with that value
                if (_listcurrency.OwnerCur.CurrencyKeys[ListName] != null)
                {
                    if (returnID != Convert.ToInt64(_listcurrency.OwnerCur.CurrencyKeys[ListName]) && _listcurrency.ListPositionCode != ListStatus.OnOwnerRow)
                    {
                        //Updated for SAAQ-FA issue 3910  and issue 4003 and issue 4670
                        //if ((_listcurrency.MemberCur.CurrencyKeys[_listcurrency.OwnerCur.IdColName] != null) && isGetOwner)
                        //Check for returnID == 0 for issue 8665
                        if (isGetOwner || returnID == 0)
                            returnID = Convert.ToInt64(_listcurrency.OwnerCur.CurrencyKeys[ListName]);
                    }
                }
            }
            return returnID;
            //return 0;
        }

        /// <summary>
        /// Returns value of owner key column (foreign key) from the specified list (set) currency table.
        /// </summary>
        /// <param name="ListName">Specifies list (set) name.</param>
        /// <returns>0 if table is not found or currency is not defined.</returns>
        public long GetJunctionListOwnerID(string ListName)
        {
            long returnID = 0;
            if (ListTable.ContainsKey(ListName))
            {
                _listcurrency = ListTable[ListName];

                //Following Added in for handling BOM complicated structures - Tenneco - 2019-07-11
                if (_listcurrency.ListPositionCode == ListStatus.OnMemberRow)
                {
                    if (_listcurrency.MemberCur.CurrencyKeys.ContainsKey(ListName) && _listcurrency.MemberCur.CurrencyKeys[ListName] != null)
                    {
                        returnID = Convert.ToInt64(_listcurrency.MemberCur.CurrencyKeys[ListName]);
                    }
                }
                else if (_listcurrency.OwnerCur == null)
                    {
                        returnID = 0;
                    }
                else
                {

                    if (_listcurrency.OwnerCur.CurrencyKeys[_listcurrency.OwnerCur.IdColName] != null)
                    {
                        if (_listcurrency.OwnerCur.CurrencyKeys[_listcurrency.OwnerCur.IdColName] != DBNull.Value)
                            returnID = Convert.ToInt64(_listcurrency.OwnerCur.CurrencyKeys[_listcurrency.OwnerCur.IdColName]);
                    }
                }
            }
            return returnID;
        }

        /// <summary>
        /// Returns value of member key column from the specified list (set) currency table.
        /// </summary>
        /// <param name="ListName">Specifies list (set) name.</param>
        /// <returns>0 if table is not found or list is empty or member currency is not defined.</returns>
        public long GetListCurrentID(string ListName)
        {
            if (ListTable.ContainsKey(ListName))
            {
                _listcurrency = ListTable[ListName];
                if (_listcurrency.MemberCur != null
                        && (_listcurrency.ListPositionCode == ListStatus.OnMemberRow || (_listcurrency.ListPositionCode == ListStatus.OnNone && _listcurrency.MemberCur.RecordActionCode == RowStatus.DeletedRow))
                        && _listcurrency.MemberCur.CurrencyKeys.ContainsKey(_listcurrency.MemberCur.IdColName))
                {
                    if (_listcurrency.MemberCur.CurrencyKeys[_listcurrency.MemberCur.IdColName] != null && _listcurrency.MemberCur.CurrencyKeys[_listcurrency.MemberCur.IdColName].ToString() != string.Empty)
                        return Convert.ToInt64(_listcurrency.MemberCur.CurrencyKeys[_listcurrency.MemberCur.IdColName]);
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns member record name of the specified set.
        /// </summary>
        /// <param name="ListName">Specifies list (set) name.</param>
        /// <returns>empty string if set is not found or member is not defined.</returns>
        public string GetListMemberName(string ListName)
        {
            if (ListTable.ContainsKey(ListName))
            {
                _listcurrency = ListTable[ListName];

                if (_listcurrency.MemberCur == null)
                    return string.Empty;
                else
                    return _listcurrency.MemberCur.RecordName;
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns owner record name of the specified set.
        /// </summary>
        /// <param name="ListName">Specifies list (set) name.</param>
        /// <returns>empty string if set is not found.</returns>
        public string GetListOwnerName(string ListName)
        {
            if (ListTable.ContainsKey(ListName))
            {
                _listcurrency = ListTable[ListName];
                return _listcurrency.OwnerCur.RecordName;
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the foreign key name of the specified set.
        /// </summary>
        /// <param name="ListName">Specifies list (set) name.</param>
        /// <returns>empty string if set is not found.</returns>
        public string GetListKeyName(string ListName)
        {
            if (ListTable.ContainsKey(ListName))
            {
                _listcurrency = ListTable[ListName];

                return _listcurrency.ListFkName;
            }
            return string.Empty;
        }

        /// <summary>
        /// Creates a clone of the DBCurrency instance
        /// </summary>
        /// <returns>a new instance of DBCurrency, which fields are populated by data from current instance of DBCurrency.</returns>
        public DBCurrency Clone()
        {
            DBCurrency newCurrency = new DBCurrency();
            newCurrency.CurrentIdCol = CurrentIdCol;
            newCurrency.CurrentRecordName = CurrentRecordName;
            newCurrency.ConversationName = ConversationName;
            foreach (string listName in ListTable.Keys)
            {
                if (ListTable[listName] == null)
                    newCurrency.ListTable.Add(listName, null);
                else
                    newCurrency.ListTable.Add(listName, ListTable[listName].Clone());
            }
            foreach (string recName in RecordTable.Keys)
            {
                if (RecordTable[recName] == null)
                    newCurrency.RecordTable.Add(recName, null);
                else
                    newCurrency.RecordTable.Add(recName, RecordTable[recName].Clone());
            }

            return newCurrency;
        }
        #endregion
    }
}
