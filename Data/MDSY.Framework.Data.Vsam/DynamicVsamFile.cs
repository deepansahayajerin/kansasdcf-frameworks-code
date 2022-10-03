using System;
using System.Linq;
using MDSY.Framework.Buffer;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Interfaces;
using System.Data.Common;
using System.Collections.Generic;

namespace MDSY.Framework.Data.Vsam
{
    /// <summary>
    /// DynamicVsamFile represents VSAM files created thru IDCAMS without associated Segment (copybook) definition. The database table will only have columns VSAM_DATA and VSAM_KEY
    /// </summary>
    public class DynamicVsamFile : VsamDalBase
    {
        private int _recordLength = 100;
        private string _recordName = "DynamicVsamFile";
        #region Name constants
        /// <summary>
        /// Name constants.
        /// </summary>
        internal static class Names
        {
            internal const string VSAM_DATA = "VSAM_DATA";
        }
        #endregion

        #region Direct-access element properties
        public IField VSAM_DATA { get { return GetElementByName<IField>(Names.VSAM_DATA); } }

        public Dictionary<string, VsamKey> KeyDictionary = new Dictionary<string, VsamKey>();
        #endregion

        #region Data structure definition
        /// <summary>
        /// Defines the entirety of the CPY_ITUJCL IRecord structure as described by the MDSY.Framework.Buffer API.
        /// </summary>
        /// <param name="recordDef">The IStructureDefinition object to be used in defining the record structure.</param>
        protected override void DefineRecordStructure(IStructureDefinition recordDef)
        {
            recordDef.CreateNewField(Names.VSAM_DATA, FieldType.String, _recordLength);
        }

        protected override string GetRecordName()
        {
            return _recordName;
        }
        #endregion


        #region Public Constructors

        public DynamicVsamFile(DbConnection passedConnection, int dataLength)
            : base(passedConnection)
        {
            _recordLength = dataLength;
            SetUpFields();
        }
        public DynamicVsamFile(int dataLength)
            : base()
        {
            _recordLength = dataLength;
            SetUpFields();
        }
        public DynamicVsamFile(int dataLength, string recordName)
            : base() {
          _recordLength = dataLength;
          _recordName = recordName;
          SetUpFields();
        }
        private void SetUpFields()
        {
            TableName = "";
            SequenceName = "";
            IsBinaryKey = true;
            ResetToInitialValue();
        }
        #endregion

        public override void SetRecordData()
        {

            VSAM_DATA.SetValue((byte[])VsamDalDataTable.Rows[DataTableCurrentRow]["VSAM_DATA"]);
            LastKey.BinaryKey = ((byte[])VsamDalDataTable.Rows[DataTableCurrentRow]["VSAM_KEY"]);
        }

        public override void SetUpdateParameters(DbCommand command)
        {
            command.Parameters.Add(CreateParameter("VSAM_DATA", VSAM_DATA.AsBytes));
        }

        //public override void SetDataTableColumns()
        //{
        //    Command.Parameters.Add(CreateParameter(string.Concat("VSAM_DATA_",DataTableCurrentRow.ToString()) , VSAM_DATA.AsBytes));
        //    Command.Parameters.Add(CreateParameter(string.Concat(IdColumnName,"_", DataTableCurrentRow.ToString()) , VsamDalDataTable.Rows[DataTableCurrentRow][IdColumnName]));
        //    UpdateCachedQuery.AppendFormat("Update {0} SET VSAM_DATA = {1} Where {2} = {3}; ",
        //        TableName, string.Concat("@VSAM_DATA_", DataTableCurrentRow.ToString()), VsamDalDataTable.Columns[0].ColumnName, string.Concat("@", IdColumnName, "_", DataTableCurrentRow.ToString()));

        //}
        public override void SetMultiViewTable()
        {
        }
    }
}
