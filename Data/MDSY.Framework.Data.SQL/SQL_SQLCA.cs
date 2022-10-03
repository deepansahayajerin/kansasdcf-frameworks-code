using MDSY.Framework.Buffer;
using MDSY.Framework.Buffer.BaseClasses;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSY.Framework.Data.SQL
{
    public class SQL_SQLCA : PredefinedRecordBase
    {
        #region Name constants
        /// <summary>
        /// Name constants.
        /// </summary>
        internal static class Names
        {
            internal const string CPY_SQLCA = "SQL_SQLCA";
            internal const string SQLCA = "SQLCA";
            internal const string SQLCAID = "SQLCAID";
            internal const string SQLCABC = "SQLCABC";
            internal const string SQLCODE = "SQLCODE";
            internal const string SQLERRM = "SQLERRM";
            internal const string SQLERRML = "SQLERRML";
            internal const string SQLERRMC = "SQLERRMC";
            internal const string SQLERRP = "SQLERRP";
            internal const string SQLERRD = "SQLERRD";
            internal const string SQLWARN = "SQLWARN";
            internal const string SQLWARN0 = "SQLWARN0";
            internal const string SQLWARN1 = "SQLWARN1";
            internal const string SQLWARN2 = "SQLWARN2";
            internal const string SQLWARN3 = "SQLWARN3";
            internal const string SQLWARN4 = "SQLWARN4";
            internal const string SQLWARN5 = "SQLWARN5";
            internal const string SQLWARN6 = "SQLWARN6";
            internal const string SQLWARN7 = "SQLWARN7";
            internal const string SQLEXT = "SQLEXT";
            internal const string SQLWARN8 = "SQLWARN8";
            internal const string SQLWARN9 = "SQLWARN9";
            internal const string SQLWARNA = "SQLWARNA";
            internal const string SQLSTATE = "SQLSTATE";
        }
        #endregion

        #region Direct-access element properties
        public IGroup SQLCA { get { return GetElementByName<IGroup>(Names.SQLCA); } }
        public IField SQLCAID { get { return GetElementByName<IField>(Names.SQLCAID); } }
        public IField SQLCABC { get { return GetElementByName<IField>(Names.SQLCABC); } }
        public IField SQLCODE { get { return GetElementByName<IField>(Names.SQLCODE); } }
        public IGroup SQLERRM { get { return GetElementByName<IGroup>(Names.SQLERRM); } }
        public IField SQLERRML { get { return GetElementByName<IField>(Names.SQLERRML); } }
        public IField SQLERRMC { get { return GetElementByName<IField>(Names.SQLERRMC); } }
        public IField SQLERRP { get { return GetElementByName<IField>(Names.SQLERRP); } }
        public IArrayElementAccessor<IField> SQLERRD { get { return GetArrayElementAccessor<IField>(Names.SQLERRD); } }
        public IGroup SQLWARN { get { return GetElementByName<IGroup>(Names.SQLWARN); } }
        public IField SQLWARN0 { get { return GetElementByName<IField>(Names.SQLWARN0); } }
        public IField SQLWARN1 { get { return GetElementByName<IField>(Names.SQLWARN1); } }
        public IField SQLWARN2 { get { return GetElementByName<IField>(Names.SQLWARN2); } }
        public IField SQLWARN3 { get { return GetElementByName<IField>(Names.SQLWARN3); } }
        public IField SQLWARN4 { get { return GetElementByName<IField>(Names.SQLWARN4); } }
        public IField SQLWARN5 { get { return GetElementByName<IField>(Names.SQLWARN5); } }
        public IField SQLWARN6 { get { return GetElementByName<IField>(Names.SQLWARN6); } }
        public IField SQLWARN7 { get { return GetElementByName<IField>(Names.SQLWARN7); } }
        public IGroup SQLEXT { get { return GetElementByName<IGroup>(Names.SQLEXT); } }
        public IField SQLWARN8 { get { return GetElementByName<IField>(Names.SQLWARN8); } }
        public IField SQLWARN9 { get { return GetElementByName<IField>(Names.SQLWARN9); } }
        public IField SQLWARNA { get { return GetElementByName<IField>(Names.SQLWARNA); } }
        public IField SQLSTATE { get { return GetElementByName<IField>(Names.SQLSTATE); } }

        #endregion

        #region Data structure definition
        /// <summary>
        /// Defines the entirety of the CPY_SQLCA IRecord structure as described by the MDSY.Framework.Buffer API.
        /// </summary>
        /// <param name="recordDef">The IStructureDefinition object to be used in defining the record structure.</param>
        protected override void DefineRecordStructure(IStructureDefinition recordDef)
        {

            recordDef.CreateNewGroup(Names.SQLCA, (SQLCA) =>
            {
                SQLCA.CreateNewField(Names.SQLCAID, FieldType.String, 8);
                SQLCA.CreateNewField(Names.SQLCABC, FieldType.CompInt, 9);
                SQLCA.CreateNewField(Names.SQLCODE, FieldType.CompInt, 9);
                SQLCA.CreateNewGroup(Names.SQLERRM, (SQLERRM) =>
                {
                    SQLERRM.CreateNewField(Names.SQLERRML, FieldType.CompShort, 4);
                    SQLERRM.CreateNewField(Names.SQLERRMC, FieldType.String, 70);
                });
                SQLCA.CreateNewField(Names.SQLERRP, FieldType.String, 8);
                SQLCA.CreateNewFieldArray(Names.SQLERRD, 6, FieldType.CompInt, 9);
                SQLCA.CreateNewGroup(Names.SQLWARN, (SQLWARN) =>
                {
                    SQLWARN.CreateNewField(Names.SQLWARN0, FieldType.String, 1);
                    SQLWARN.CreateNewField(Names.SQLWARN1, FieldType.String, 1);
                    SQLWARN.CreateNewField(Names.SQLWARN2, FieldType.String, 1);
                    SQLWARN.CreateNewField(Names.SQLWARN3, FieldType.String, 1);
                    SQLWARN.CreateNewField(Names.SQLWARN4, FieldType.String, 1);
                    SQLWARN.CreateNewField(Names.SQLWARN5, FieldType.String, 1);
                    SQLWARN.CreateNewField(Names.SQLWARN6, FieldType.String, 1);
                    SQLWARN.CreateNewField(Names.SQLWARN7, FieldType.String, 1);
                });
                SQLCA.CreateNewGroup(Names.SQLEXT, (SQLEXT) =>
                {
                    SQLEXT.CreateNewField(Names.SQLWARN8, FieldType.String, 1);
                    SQLEXT.CreateNewField(Names.SQLWARN9, FieldType.String, 1);
                    SQLEXT.CreateNewField(Names.SQLWARNA, FieldType.String, 1);
                    SQLEXT.CreateNewField(Names.SQLSTATE, FieldType.String, 5);
                });
            });

        }

        protected override string GetRecordName()
        {
            return Names.CPY_SQLCA;
        }
        #endregion

        #region Public Constructors

        public SQL_SQLCA(IRecord recordBuffer, bool isNewCopy)
            : base()
        {
            if (isNewCopy || recordBuffer.AsBytes() == null)
                this.Record.ResetToInitialValue();
            else
                this.Record.AssignFrom(recordBuffer.AsBytes());
        }
        public SQL_SQLCA()
            : base()
        {

            this.Record.ResetToInitialValue();
        }
        #endregion
    }
}
