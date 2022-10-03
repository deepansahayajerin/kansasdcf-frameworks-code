#region Using Directives
using System.Collections.Generic;
using System;
using System.Collections.Specialized;
using System.Collections;
using System.Text;

using MDSY.Framework.Core;
#endregion

namespace MDSY.Framework.Data.IDMS
{
    [Serializable]
    public class RecordCurrency
    {
        /// <summary>
        /// RecordCurrency object contains a set of integers, which represent its currency values. 
        /// Those are key currency and foreign key currencies. Key currency corresponds to primary key in record’s database table. 
        /// Foreign key currencies correspond to database foreign keys that link current record table with other record tables 
        /// in member-to-owner relationship. It contains a list of IDMS set in which it participates as an owner or member.

        /// </summary>
        #region Private Objects
        private Hashtable _CurrencyKeys = new Hashtable();
        private IDictionary<string, string> _ListNames = new Dictionary<string, string> { };
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor of RecordCurrency class.
        /// </summary>
        public RecordCurrency(string recordName, string tableName, string recordTypeName, params string[] Keyarray)
        {
            RecordName = recordName;
            TableName = tableName;
            IdColName = Keyarray[0];
            foreach (string strKey in Keyarray)
            {
                if (!_CurrencyKeys.ContainsKey(strKey))
                    _CurrencyKeys.Add(strKey, null);
            }
            RecordActionCode = RowStatus.NoAction;
            RecordTypeName = recordTypeName;
            isKeysUpdated = false;
        }
        #endregion

        #region  Public Properties
        public string RecordName { get; set; }
        public string TableName { get; set; }
        public RowStatus RecordActionCode { get; set; }
        public string ErrorStatus { get; set; }
        public bool isKeysUpdated { get; set; }
        public string IdColName { get; set; }
        public string RecordTypeName { get; set; }

        public long DeletedNextIdColValue { get; set; }
        public long DeletedPriorIdColValue { get; set; }

        public Hashtable CurrencyKeys
        {
            get
            {
                if (_CurrencyKeys == null)
                {
                    _CurrencyKeys = new Hashtable();
                }
                return _CurrencyKeys;
            }
        }

        public IDictionary<string, string> ListNames
        {
            get
            {
                return _ListNames;
            }
            set
            {
                _ListNames = value;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds IDMS set name and type to the list of sets in which current record participates as an owner or member.
        /// </summary>
        /// <param name="ListName">Specifies set name.</param>
        /// <param name="ListType">Specifies how the current record participates in the set. It can take values “Owner” or “Member”.</param>
        public void SetListName(string ListName, string ListType)
        {
            if (!_ListNames.ContainsKey(ListName))
                _ListNames.Add(ListName, ListType);
        }

        /// <summary>
        /// Creates a clone of the RecordCurrency instance
        /// </summary>
        /// <returns>a new instance of RecordCurrency, which is a copy of current RecordCurrency object.</returns>
        public RecordCurrency Clone()
        {
            string[] keyarray = new string[_CurrencyKeys.Count];
            _CurrencyKeys.Keys.CopyTo(keyarray,0);

            RecordCurrency newRecordCurrency = new RecordCurrency(RecordName, TableName, RecordTypeName, keyarray);

            newRecordCurrency.RecordActionCode = RecordActionCode;
            newRecordCurrency.ErrorStatus = ErrorStatus;
            newRecordCurrency.isKeysUpdated = isKeysUpdated;
            newRecordCurrency.IdColName = IdColName;
                 
            foreach (string keyString in CurrencyKeys.Keys)
            {
                newRecordCurrency.CurrencyKeys[keyString] = CurrencyKeys[keyString];
            }

            foreach (string listKey in _ListNames.Keys)
            {
                newRecordCurrency.ListNames.Add(listKey, _ListNames[listKey]);
            }

            return newRecordCurrency;
        }

        /// <summary>
        /// Sets currency keys of current RecordCurrency to null.
        /// </summary>
        /// <returns></returns>
        public void DropCurrencyKeys()
        {
            if (CurrencyKeys == null || CurrencyKeys.Count == 0 ) return;

            RecordActionCode = RowStatus.NoRow;

            string[] mkeys = new string[CurrencyKeys.Count];
            CurrencyKeys.Keys.CopyTo(mkeys, 0);
            foreach (string mkey in mkeys)
                CurrencyKeys[mkey] = null;
        }

        /// <summary>
        /// Sets the specified currency to null. If currency is not specified then sets record’s key currency to null.
        /// </summary>
        /// <param name="listKeyName">Specifies currency that needs to be set to null.</param>
        public void DropRecordSetCurrencyKeys(string listKeyName)
        {
            if (listKeyName != null)
            {
                if (CurrencyKeys.ContainsKey(listKeyName))
                    CurrencyKeys[listKeyName] = null;
            }
            //if (CurrencyKeys.ContainsKey(IdColName))
            //     CurrencyKeys[IdColName] = null;
        }
        #endregion
    }
}
