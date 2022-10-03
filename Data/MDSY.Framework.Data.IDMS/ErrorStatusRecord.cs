using System;

using System.Collections;
using System.Text;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.BaseClasses;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer;
namespace MDSY.Framework.Data.IDMS
{


    public class ErrorStatusRecord : PredefinedRecordBase
    {

        #region Name constants
        /// <summary>
        /// Name constants.
        /// </summary>
        internal static class Names
        {
            internal const string RecordName = "ErrorStatus";
            internal const string ReturnCode = "ReturnCode";
            internal const string StatusGood = "StatusGood";
            internal const string RowNotFound = "RowNotFound";
            internal const string EndOfList = "EndOfList";
            internal const string AnyError = "AnyError";
            internal const string EndOfIndex = "EndOfIndex";
            internal const string IndexRowNotFound = "IndexRowNotFound";
            internal const string QueueIDNotFound = "QueueIDNotFound";
            internal const string QueueRecordNotFound = "QueueRecordNotFound";
            internal const string ScratchAreaNotFound = "ScratchAreaNotFound";
            internal const string ScratchRecordNotFound = "ScratchRecordNotFound";
            internal const string ScratchRecordReplaced = "ScratchRecordReplaced";
            internal const string AnyStatus = "AnyStatus";
            internal const string CallSave = "CallSave";
            internal const string ErrStatSave = "ErrStatSave";
            internal const string ErrorRecord = "ErrorRecord";
            internal const string ErrorSet = "ErrorSet";

        }
        #endregion

        #region Direct-access element properties

        /// <summary>
        /// Returns a reference to the IField object, which contains return code value.
        /// </summary>
        public IField ReturnCode { get { return GetElementByName<IField>(Names.ReturnCode); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains StatusGood value.
        /// </summary>
        public ICheckField StatusGood { get { return GetElementByName<ICheckField>(Names.StatusGood); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains RowNotFound value.
        /// </summary>
        public ICheckField RowNotFound { get { return GetElementByName<ICheckField>(Names.RowNotFound); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains EndOfList value.
        /// </summary>
        public ICheckField EndOfList { get { return GetElementByName<ICheckField>(Names.EndOfList); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains AnyError value.
        /// </summary>
        public ICheckField AnyError { get { return GetElementByName<ICheckField>(Names.AnyError); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains EndOfIndex value.
        /// </summary>
        public ICheckField EndOfIndex { get { return GetElementByName<ICheckField>(Names.EndOfIndex); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains IndexRowNotFound value.
        /// </summary>
        public ICheckField IndexRowNotFound { get { return GetElementByName<ICheckField>(Names.IndexRowNotFound); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains QueueIDNotFound value.
        /// </summary>
        public ICheckField QueueIDNotFound { get { return GetElementByName<ICheckField>(Names.QueueIDNotFound); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains QueueRecordNotFound value.
        /// </summary>
        public ICheckField QueueRecordNotFound { get { return GetElementByName<ICheckField>(Names.QueueRecordNotFound); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains ScratchAreaNotFound value.
        /// </summary>
        public ICheckField ScratchAreaNotFound { get { return GetElementByName<ICheckField>(Names.ScratchAreaNotFound); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains ScratchRecordNotFound value.
        /// </summary>
        public ICheckField ScratchRecordNotFound { get { return GetElementByName<ICheckField>(Names.ScratchRecordNotFound); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains ScratchRecordReplaced value.
        /// </summary>
        public ICheckField ScratchRecordReplaced { get { return GetElementByName<ICheckField>(Names.ScratchRecordReplaced); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains AnyStatus value.
        /// </summary>
        public ICheckField AnyStatus { get { return GetElementByName<ICheckField>(Names.AnyStatus); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains CallSave value.
        /// </summary>
        public IField CallSave { get { return GetElementByName<IField>(Names.CallSave); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains ErrStatSave value.
        /// </summary>
        public IField ErrStatSave { get { return GetElementByName<IField>(Names.ErrStatSave); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains ErrorSet value.
        /// </summary>
        public IField ErrorSet { get { return GetElementByName<IField>(Names.ErrorSet); } }

        /// <summary>
        /// Returns a reference to the ICheckField object, which contains ErrorRecord value.
        /// </summary>
        public IField ErrorRecord { get { return GetElementByName<IField>(Names.ErrorRecord); } }
        

        #endregion

        /// <summary>
        /// Returns record name.
        /// </summary>
        /// <returns>Record name.</returns>
        protected override string GetRecordName()
        {
            return Names.RecordName;
        }

        /// <summary>
        /// Creates record's field objects and attaches them to the provided record definition object. 
        /// </summary>
        /// <param name="recordDef">A reference to the record definition object.</param>
        protected override void DefineRecordStructure(IStructureDefinition recordDef)
        {
            recordDef
                .CreateNewField(Names.ReturnCode, FieldType.SignedNumeric, 4, 0)
                .NewCheckField(Names.StatusGood, f => f.AsInt() == ReturnCodes.StatusGood)
                .NewCheckField(Names.RowNotFound, f => f.AsInt() == ReturnCodes.RowNotFound)
                .NewCheckField(Names.EndOfList, f => f.AsInt() == ReturnCodes.EndOfList)
                .NewCheckField(Names.AnyError, f => f.IsInRange(ReturnCodes.ErrorLoBound, ReturnCodes.ErrorHiBound))
                .NewCheckField(Names.EndOfIndex, f => f.AsInt() == ReturnCodes.EndOfIndex)
                .NewCheckField(Names.IndexRowNotFound, f => f.AsInt() == ReturnCodes.IndexRowNotFound)
                .NewCheckField(Names.QueueIDNotFound, f => f.AsInt() == ReturnCodes.QueueIdNotFound)
                .NewCheckField(Names.QueueRecordNotFound, 4405, 4305)
                .NewCheckField(Names.ScratchAreaNotFound, f => f.AsInt() == ReturnCodes.ScratchAreaNotFound)
                .NewCheckField(Names.ScratchRecordNotFound, f => f.AsInt() == ReturnCodes.ScratchRecordNotFound)
                 .NewCheckField(Names.ScratchRecordReplaced, f => f.AsInt() == ReturnCodes.ScratchRecordReplaced)
                .NewCheckField(Names.AnyStatus, f => f.IsInRange(ReturnCodes.StatusGood, ReturnCodes.ErrorHiBound));

            recordDef
                .NewField(Names.CallSave, FieldType.SignedNumeric, 8)
                .NewField(Names.ErrStatSave, FieldType.SignedNumeric, 4)
                .NewField(Names.ErrorRecord, FieldType.String, 32)
                .NewField(Names.ErrorSet, FieldType.String, 32);
        }


    }
}
