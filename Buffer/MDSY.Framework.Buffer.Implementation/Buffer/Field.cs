using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using Unity;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Common;


namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IField.
    /// </summary>
    [InjectionImplementer(typeof(IField))]
    [Serializable]
    internal sealed class Field : FieldBase, IField, IBufferValue, IArrayElementInitializer<IField>
    {
        #region overrides
        /// <summary>
        /// Creates a new instance of the Field object and initializes it with the data from the current object.
        /// </summary>
        /// <param name="newName">The name of the duplicated Field object.</param>
        /// <returns>Returns a reference to the newly created Field object</returns>
        protected override FieldBase GetDuplicateObject(string newName)
        {
            return new Field()
            {
                //Name = IsFiller ? ObjectFactory.GetTimeBasedName(newName) : newName,
                Name = newName,
                FieldType = this.FieldType,
                LengthInBuffer = this.LengthInBuffer,
                DisplayLength = this.DisplayLength,
                Parent = this.Parent,
                Buffer = this.Buffer,
                PositionInParent = Constants.Defaults.PositionInParent,
                DecimalDigits = this.DecimalDigits,
                IsInArray = this.IsInArray,
                IsFiller = this.IsFiller,
                ArrayElementIndex = Constants.Defaults.ArrayElementIndex
            };
        }

        /// <summary>
        /// Assings the value of the current Field object to the provided Field object.
        /// </summary>
        /// <param name="duplicateField">A reference to the Field object for the assignment.</param>
        protected override void InternalDuplicate(FieldBase duplicateField)
        {
            duplicateField.AssignFrom(this);
        }

        //public object Clone()
        //{
        //    return this.MemberwiseClone();
        //}
        /// <summary>
        /// Causes the object to restore its value to its original data.
        /// </summary>
        public override void ResetToInitialValue()
        {
            if (InitialValue != null)
            {
                Assign(InitialValue);
            }
            else
            {
                if (this.IsInRedefine)
                    return;
                if (this.FieldType == Common.FieldType.String || this.FieldType == Common.FieldType.NumericEdited)
                {
                    Assign(" ");
                }
                else if (this.FieldType == Common.FieldType.Boolean)
                {
                    Assign(false);
                }
                else
                {
                    Assign(0);
                }
            }
        }

        /// <summary>
        /// Causes the object to be initialized to low values or default value
        /// /// </summary>
        public override void InitializeWithLowValues()
        {
            if (InitialValue != null)
            {
                Assign(InitialValue);
            }
            else
            {
                if (this.IsInRedefine)
                    return;

                Buffer.WriteBytes(Enumerable.Repeat<byte>(byte.MinValue, this.LengthInBuffer).ToArray(), PositionInBuffer, LengthInBuffer);
            }
        }
        #endregion
    }

}
