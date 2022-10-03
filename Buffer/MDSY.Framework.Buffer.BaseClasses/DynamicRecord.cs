using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer;

namespace MDSY.Framework.Buffer.BaseClasses
{
    /// <summary>
    /// Record filled with hashes. Defauld record length is 100 bytes.
    /// </summary>
    public class DynamicRecord : PredefinedRecordBase
    {
        /// <summary>
        /// Keeps "DynamicRecord" value.
        /// </summary>
        public string DynamicRecordName  = "DynamicRecord";
        private int _recordLength = 100;

        #region Data structure definition
        /// <summary>
        /// Fills provided record definition with hashes.
        /// </summary>
        /// <param name="recordDef">A reference to the record definition object.</param>
        protected override void DefineRecordStructure(IStructureDefinition recordDef)
        {
            recordDef.CreateNewFillerField(_recordLength, FillWith.Hashes);
        }

        /// <summary>
        /// Returns record name.
        /// </summary>
        /// <returns>Returns "DynamicRecord" value. </returns>
        protected override string GetRecordName()
        {
            return DynamicRecordName;
        }
        #endregion


        /// <summary>
        /// Creates a new instance of the DynamicRecord class and initializes it with the provided record length value.
        /// </summary>
        /// <param name="recordLength">Record length.</param>
        public DynamicRecord(int recordLength) : base()
        {
            _recordLength = recordLength;
        }
    }
}
