using System;
using System.Collections.Specialized;
using System.Text;

using MDSY.Framework.Core;
using System.Collections.Generic;


namespace MDSY.Framework.Data.IDMS
{
    /// <summary>
    /// <para>
    /// ListCurrency class represents currencies of IDMS sets.
    /// All application ListCurrency instances are created in DB_SCGL0001 class, 
    /// which belongs to Service.DataStructures project and is generated during code conversion.
    /// </para>
    /// <para>
    /// ListCurrency class is a container of public properties that define the set, 
    /// its owner and member and its status. ListCurrency class has only Clone() method, 
    /// which creates and returns a copy of current ListCurrency instance.
    /// </para>
    [Serializable]
    public class ListCurrency
    {
        #region Properties
        /// <summary>
        /// Sets and returns the name of the set.
        /// </summary>
        public string ListName { get; set; }

        /// <summary>
        /// Sets and returns set's action code value.
        /// </summary>
        public RowStatus ListActionCode { get; set; }

        /// <summary>
        /// Sets and returns set's position code value.
        /// </summary>
        public ListStatus ListPositionCode { get; set; }

        /// <summary>
        /// Sets and returns the name of the junction table.
        /// </summary>
        public string JunctionTableName { get; set; }

        /// <summary>
        /// Sets and returns junctions table identifier value.
        /// </summary>
        public string JunctionTableID { get; set; }

        /// <summary>
        /// Sets and returns junction table foreign key value.
        /// </summary>
        public string JunctionFkName { get; set; }

        /// <summary>
        /// Sets and returns the database number of the current record.
        /// </summary>
        public short CurrentRecDbNbr { get; set; }

        /// <summary>
        /// Indicates whether record currency keys have been updated.
        /// </summary>
        public bool isKeysUpdated { get; set; }

        /// <summary>
        /// Sets and returns a reference to the RecordCurrency object, which represents current set's member record.
        /// </summary>
        public RecordCurrency MemberCur { get; set; }

        /// <summary>
        /// Sets and returns a reference to the RecordCurrency object, which represents set's owner record.
        /// </summary>
        public RecordCurrency OwnerCur { get; set; }

        /// <summary>
        /// Sets and returns a reference to the collection of member records that are linked to the current set.
        /// </summary>
        public List<RecordCurrency> MemberList { get; set; }

        /// <summary>
        /// Sets and returns list options value.
        /// </summary>
        public ListOptions ListOpt { get; set; }

        /// <summary>
        /// Sets and returns list order value.
        /// </summary>
        public ListOrder ListOrd { get; set; }

        /// <summary>
        /// Sets and returns a reference to the list of duplicates.
        /// </summary>
        public ListDuplicates ListDups { get; set; }

        /// <summary>
        /// Sets and returns a reference to the set's sequence object.
        /// </summary>
        public string ListSequenceObject { get; set; }

        /// <summary>
        /// Sets and returns set's foreign key value.
        /// </summary>
        public string ListFkName { get; set; }

        /// <summary>
        /// Sets and returns set keys.
        /// </summary>
        public string ListKeys { get; set; }

        /// <summary>
        /// Sets and returns multi-member sort key.
        /// </summary>
        public string MultiMemberSortKey { get; set; }

        /// <summary>
        /// Sets and returns multi-member type key.
        /// </summary>
        public string MultiMemberTypeKey { get; set; }

        /// <summary>
        /// Sets and returns the key currency value of the record, which is set's previous member.
        /// </summary>
        public long MissOnUsingPrev { get; set; }

        /// <summary>
        /// Sets and returns the key currency value of the record, which is set's next member.
        /// </summary>
        public long MissOnUsingNext { get; set; }

        public int DataTableCurrentRow { get; set; }
        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of ListCurrency class and initializes it with the parameter values.
        /// </summary>
        /// <param name="listname">The name of the set.</param>
        /// <param name="fkname">Foreign key of the set.</param>
        /// <param name="membercurrency">A reference to the RecordCurrency object, which defines currencies of the set's member record.</param>
        /// <param name="ownercurrency">A reference to the RecordCurrency object, which defines currencies of the set's owner record.</param>
        /// <param name="LOptions">Set options value.</param>
        /// <param name="LOrder">Set order value.</param>
        /// <param name="LDups">Set duplicates.</param>
        /// <param name="listKeys">Set keys.</param>
        public ListCurrency(string listname, string fkname, RecordCurrency membercurrency, RecordCurrency ownercurrency, ListOptions LOptions, ListOrder LOrder, ListDuplicates LDups, string listKeys)
        {
            ListName = listname;
            MemberCur = membercurrency == null ? null : membercurrency.Clone();
            OwnerCur = ownercurrency == null ? null : ownercurrency.Clone();
            ListPositionCode = ListStatus.OnNone;
            ListOrd = LOrder;
            ListOpt = LOptions;
            ListDups = LDups;
            ListFkName = fkname;
            ListKeys = listKeys;
            isKeysUpdated = false;
        }

        /// <summary>
        /// Creates an instance of ListCurrency class and initializes it with the parameter values.
        /// </summary>
        /// <param name="listname">The name of the set.</param>
        /// <param name="fkname">Foreign key of the set.</param>
        /// <param name="membercurrency">A reference to the RecordCurrency object, which defines currencies of the set's member record.</param>
        /// <param name="ownercurrency">A reference to the RecordCurrency object, which defines currencies of the set's member record.</param>
        /// <param name="sequenceObjectName">The name of the sequence object.</param>
        /// <param name="LOptions">Set options value.</param>
        /// <param name="LOrder">Set order value.</param>
        /// <param name="LDups">Set duplicates.</param>
        /// <param name="listKeys">Set keys.</param>
        public ListCurrency(string listname, string fkname, RecordCurrency membercurrency, RecordCurrency ownercurrency, string sequenceObjectName, ListOptions LOptions, ListOrder LOrder, ListDuplicates LDups, string listKeys)
        {
            ListName = listname;
            MemberCur = membercurrency == null ? null : membercurrency.Clone();
            OwnerCur = ownercurrency == null ? null : ownercurrency.Clone();
            ListPositionCode = ListStatus.OnNone;
            ListOrd = LOrder;
            ListOpt = LOptions;
            ListDups = LDups;
            ListFkName = fkname;
            ListKeys = listKeys;
            ListSequenceObject = sequenceObjectName;
            isKeysUpdated = false;
        }

        /// <summary>
        /// Creates an instance of ListCurrency class and initializes it with the parameter values.
        /// </summary>
        /// <param name="listname">The name of the set.</param>
        /// <param name="fkname">Foreign key of the set.</param>
        /// <param name="junctionTableName"></param>
        /// <param name="membercurrency">A reference to the RecordCurrency object, which defines currencies of the set's member record.</param>
        /// <param name="ownercurrency">A reference to the RecordCurrency object, which defines currencies of the set's member record.</param>
        /// <param name="LOptions">Set options value.</param>
        /// <param name="LOrder">Set order value.</param>
        /// <param name="LDups">Set duplicates.</param>
        /// <param name="listKeys">Set keys.</param>
        public ListCurrency(string listname, string fkname, string junctionTableName, RecordCurrency membercurrency, RecordCurrency ownercurrency, ListOptions LOptions, ListOrder LOrder, ListDuplicates LDups, string listKeys)
        {
            ListName = listname;
            MemberCur = membercurrency == null ? null : membercurrency.Clone();
            OwnerCur = ownercurrency == null ? null : ownercurrency.Clone();
            ListPositionCode = ListStatus.OnNone;
            ListOrd = LOrder;
            ListOpt = LOptions;
            ListDups = LDups;
            ListFkName = fkname;
            ListKeys = listKeys;
            JunctionTableName = junctionTableName;
            isKeysUpdated = false;
        }

        /// <summary>
        /// Creates an instance of ListCurrency class and initializes it with the parameter values.
        /// </summary>
        /// <param name="listname">The name of the set.</param>
        /// <param name="fkname">Foreign key of the set.</param>
        /// <param name="junctionTableName">The name of the junction table.</param>
        /// <param name="junctionTableID">The identifier of the junction table.</param>
        /// <param name="membercurrency">A reference to the RecordCurrency object, which defines currencies of the set's member record.</param>
        /// <param name="ownercurrency">A reference to the RecordCurrency object, which defines currencies of the set's member record.</param>
        /// <param name="LOptions">Set options value.</param>
        /// <param name="LOrder">Set order value.</param>
        /// <param name="LDups">Set duplicates.</param>
        /// <param name="listKeys">Set keys.</param>
        public ListCurrency(string listname, string fkname, string junctionTableName, string junctionTableID, RecordCurrency membercurrency, RecordCurrency ownercurrency, ListOptions LOptions, ListOrder LOrder, ListDuplicates LDups, string listKeys)
        {
            ListName = listname;
            MemberCur = membercurrency == null ? null : membercurrency.Clone();
            OwnerCur = ownercurrency == null ? null : ownercurrency.Clone();
            ListPositionCode = ListStatus.OnNone;
            ListOrd = LOrder;
            ListOpt = LOptions;
            ListDups = LDups;
            ListFkName = fkname;
            ListKeys = listKeys;
            JunctionTableName = junctionTableName;
            JunctionTableID = junctionTableID;
            isKeysUpdated = false;
        }

        /// <summary>
        /// Creates a multi-member instance of ListCurrency class and initializes it with the parameter values.
        /// </summary>
        /// <param name="listname">The name of the set.</param>
        /// <param name="fkname">Foreign key of the set.</param>
        /// <param name="junctionTableName">The name of the junction table.</param>
        /// <param name="memberList">A reference to the collection of records that are members of the set.</param>
        /// <param name="ownercurrency">A reference to the RecordCurrency object, which defines currencies of the set's member record.</param>
        /// <param name="mmSortKey">Multi-member set sort key.</param>
        /// <param name="mmTypeKey">Multi-member set type key.</param>
        /// <param name="LOptions">Set options value.</param>
        /// <param name="LOrder">Set order value.</param>
        /// <param name="LDups">Set duplicates.</param>
        /// <param name="listKeys">Set keys.</param>
        public ListCurrency(string listname, string fkname, string junctionTableName, List<RecordCurrency> memberList, RecordCurrency ownercurrency, string mmSortKey, string mmTypeKey, ListOptions LOptions, ListOrder LOrder, ListDuplicates LDups, string listKeys)
        {
            ListName = listname;
            OwnerCur = ownercurrency;
            ListPositionCode = ListStatus.OnNone;
            ListOrd = LOrder;
            ListOpt = LOptions;
            ListDups = LDups;
            ListFkName = fkname;
            ListKeys = listKeys;
            JunctionTableName = junctionTableName;
            JunctionTableID = junctionTableName;
            MultiMemberSortKey = mmSortKey;
            MultiMemberTypeKey = mmTypeKey;
            isKeysUpdated = false;
            if (MultiMemberSortKey.EndsWith("SEQ"))
            {
                ListSequenceObject = MultiMemberSortKey;
            }

            if (memberList != null)
            {
                MemberList = new List<RecordCurrency>();
                foreach (RecordCurrency recCur in memberList)
                {
                    MemberList.Add(recCur.Clone());
                }
            }
        }

        /// <summary>
        /// Creates a multi-member instance of ListCurrency class and initializes it with the parameter values.
        /// </summary>
        /// <param name="listname">The name of the set.</param>
        /// <param name="fkname">Foreign key for the set.</param>
        /// <param name="junctionTableName">The name of the junction table.</param>
        /// <param name="junctionTableID">The identifier of the junction table.</param>
        /// <param name="junctionFkName">Junction table foreign key.</param>
        /// <param name="memberList">A reference to the collection of records that are members of the set.</param>
        /// <param name="ownercurrency">A reference to the RecordCurrency object, which defines currencies of the set's member record.</param>
        /// <param name="mmSortKey">Multi-member set sort key.</param>
        /// <param name="mmTypeKey">Multi-member set type key.</param>
        /// <param name="LOptions">Set options value.</param>
        /// <param name="LOrder">Set order value.</param>
        /// <param name="LDups">Set duplicates.</param>
        /// <param name="listKeys">Set keys.</param>
        public ListCurrency(string listname, string fkname, string junctionTableName, string junctionTableID, string sequenceName, List<RecordCurrency> memberList, RecordCurrency ownercurrency, string mmSortKey, string mmTypeKey, ListOptions LOptions, ListOrder LOrder, ListDuplicates LDups, string listKeys)
        {
            ListName = listname;
            OwnerCur = ownercurrency;
            ListPositionCode = ListStatus.OnNone;
            ListOrd = LOrder;
            ListOpt = LOptions;
            ListDups = LDups;
            ListFkName = fkname;
            ListKeys = listKeys;
            JunctionTableName = junctionTableName;
            JunctionTableID = junctionTableID;
            JunctionFkName = fkname;
            ListSequenceObject = sequenceName;
            MultiMemberSortKey = mmSortKey;
            MultiMemberTypeKey = mmTypeKey;
            isKeysUpdated = false;

            if (memberList != null)
            {
                MemberList = new List<RecordCurrency>();
                foreach (RecordCurrency recCur in memberList)
                {
                    MemberList.Add(recCur.Clone());
                }
            }
        }
        
        #endregion

        #region Public Methods
        /// <summary>
        /// Create a clone of the ListCurrency instance
        /// </summary>
        /// <returns>A new instance of ListCurrency class, which contain copy of the current ListCurrency object.</returns>
        public ListCurrency Clone()
        {
             ListCurrency newListCurrency = new ListCurrency(ListName,
                   ListFkName,
                   JunctionTableName,
                   JunctionTableID,
                   JunctionFkName,
                   MemberList,
                   OwnerCur,
                   MultiMemberSortKey,
                   MultiMemberTypeKey,
                   ListOpt,
                   ListOrd,
                   ListDups,
                   ListKeys);

            newListCurrency.MemberCur = MemberCur == null ? null : MemberCur.Clone();
            newListCurrency.ListActionCode = ListActionCode;
            newListCurrency.ListPositionCode = ListPositionCode;
            newListCurrency.CurrentRecDbNbr = CurrentRecDbNbr;
            newListCurrency.isKeysUpdated = isKeysUpdated;
            newListCurrency.ListSequenceObject = ListSequenceObject;
            return newListCurrency;
        } 
        #endregion
    }
}
