using MDSY.Framework.Buffer.BaseClasses;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSY.Framework.Buffer.BaseClasses
{
    public class WsExternals : PredefinedRecordBase
    {
        public string WsExternalsName = "WsExternals";
        #region private static fields
        /// <summary>
        /// Singleton instanciation
        /// </summary>
        [ThreadStatic]
        private static WsExternals _instance;
        [ThreadStatic]
        private static bool _isInDefinition;
        #endregion

        #region public static properties
        public static WsExternals Instance
        {
            get
            {
                if (WsExternals._instance == null)
                {
                    WsExternals._instance = new WsExternals();
                }
                return WsExternals._instance;
            }
        }

        public static bool IsInDefinition
        {
            get { return _isInDefinition; }
        }
        #endregion

        #region constructors
        public WsExternals()
            : base()
        {
            this.Record.ResetToInitialValue();
        }
        #endregion

        protected override string GetRecordName()
        {
            return WsExternalsName;
        }

        protected override void DefineRecordStructure(IStructureDefinition recordDef)
        {
            recordDef.CreateNewFillerField(10, FillWith.Hashes);
        }
        /// <summary>
        /// Add new External Field to WsExternals Record
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fieldtype">The fieldtype.</param>
        /// <param name="fieldlength">The fieldlength.</param>
        /// <returns></returns>
        public IField CreateNewField(string name, FieldType fieldtype, int fieldlength)
        {
            IField field = this.GetElementByName<IField>(name);

            if ((object)field == null)
            {
                IStructureDefinition rec = (IStructureDefinition)Record;
                rec.IsDefining = true; _isInDefinition = true;
                 field = rec.CreateNewField(name, fieldtype, fieldlength);
                field.InitializeWithLowValues();
                //rec.IsDefining = false;
            }

            return field;
        }
        /// <summary>
        /// Add new External Field to WsExternals Record
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldtype"></param>
        /// <param name="fieldlength"></param>
        /// <param name="declength"></param>
        /// <returns></returns>
        public IField CreateNewField(string name, FieldType fieldtype, int fieldlength, object defaultValue, int declength)
        {
            IField field = this.GetElementByName<IField>(name);

            if ((object)field == null)
            {

                IStructureDefinition rec = (IStructureDefinition)Record;
                rec.IsDefining = true; _isInDefinition = true;
                field = rec.CreateNewField(name, fieldtype, fieldlength, defaultValue, declength);
                field.InitializeWithLowValues();
                //rec.IsDefining = false;
            }

            return field;
        }

        /// <summary>
        /// Add new Group to WsExternals record
        /// </summary>
        /// <param name="name"></param>
        /// <param name="AddFieldSyntax"></param>
        /// <returns></returns>
        public IGroup CreateNewGroup(string name, Action<IStructureDefinition> AddFieldSyntax)
        {
            IGroup group = this.GetElementByName<IGroup>(name);

            if ((object)group == null)
            {
                
                IStructureDefinition rec = (IStructureDefinition)Record;
                rec.IsDefining = true; _isInDefinition = true;
                rec.DefineTimeAccessors = new Dictionary<string, IArrayElementAccessorBase>();
                group = rec.CreateNewGroup(name, AddFieldSyntax);
                rec.EndDefinition();
            }

            return group;
        }

        public void EndExternalDefinition()
        {
            _isInDefinition = false;
            IStructureDefinition rec = (IStructureDefinition)Record;
            rec.EndDefinition();
        }

    }
}
