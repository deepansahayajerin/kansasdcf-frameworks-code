using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using Unity;
using MDSY.Framework.Buffer.Interfaces;
using System.ComponentModel;
using MDSY.Framework.Buffer.Common;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IField.
    /// </summary>
    [InjectionImplementer(typeof(IField), "Redefine")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    internal sealed class RedefineField : FieldBase, IField, IBufferValue, IRedefinition
    {
        #region overrides

        /// <summary>
        /// Descendant objects should implement GetDuplicateObject so as to return a newly created field object of their same type.
        /// </summary>
        /// <returns>Returns a reference to the newly created duplicate object.</returns>
        /// <param name="newName">Name of the duplicate object.</param>
        protected override FieldBase GetDuplicateObject(string newName)
        {
            return new RedefineField()
            {
               // Name = IsFiller ? ObjectFactory.GetTimeBasedName(newName) : newName,
                Name =  newName,
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
        /// Calculates and returns the start index of this object within the buffer, based on parent position. 
        /// </summary>
        /// <remarks>Redefine objects should override this method to return a proper position.</remarks>
        /// <returns>Returns the start index of the current object withing the buffer.</returns>
        protected override int GetPositionInBuffer()
        {
            int result;

            if (RedefinedElement != null)
            {
                result = RedefinedElement.PositionInBuffer;
            }
            else
            {
                result = (Parent is IBufferElement) ?
                            (Parent as IBufferElement).PositionInBuffer + PositionInParent :
                            PositionInParent;
            }

            return result;
        }


        /// <summary>
        /// Descendant objects should implement InternalDuplicate so as to return a deep-copied object of their same type.
        /// </summary>
        /// <param name="duplicateField">A reference to the Field object for the assignment.</param>
        protected override void InternalDuplicate(FieldBase duplicateField)
        {
            UnitySingleton.Container.BuildUp(duplicateField as RedefineField);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return string.Format("[Redef]{0}", base.ToString());
        }
        #endregion

        #region private methods
        private IRedefinition GetRootLevelRedefinition()
        {
            IRedefinition result = this;

            if (!(Parent is IRecord))
            {
                var parent = Parent;
                while (parent != null)
                {
                    if (parent is IRedefinition && (parent as IRedefinition).RedefinedElement != null)
                    {
                        result = parent as IRedefinition;
                        break;
                    }
                    else
                    {
                        if (parent is IBufferElement)
                        {
                            parent = (parent as IBufferElement).Parent;
                        }
                        else
                        {
                            break;
                        }
                    }
                }


            }

            return result;
        }
        #endregion

        #region IRedefininion


        /// <summary>
        /// Returns <c>true</c> if any parent above this element implements IRedefinition.
        /// </summary>
        [Category("IRedefinition")]
        [Description("Indicates whether a parent that implements IRedefinition is above this element.")]
        [ReadOnly(true)]
        public bool HasRedefineInParents
        {
            get { return (GetRootLevelRedefinition() != this); }
        }

        /// <summary>
        /// Gets or sets the (optional) buffer element that this redefinition object redefines.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If RedefinedElement is null, this object is participating in a REDEFINE at a level lower 
        /// than the root level, thus it is redefining only a sub-section of the Redefinition root's RedefinedElement.
        /// </para>
        /// <para>To get the root-level RedefinedElement, access RootLevelRedefinition.</para>
        /// </remarks>
        [Category("IRedefinition")]
        [Description("The buffer element which this redef object redefines.")]
        [ReadOnly(true)]
        public IBufferElement RedefinedElement { get; set; }

        /// <summary>
        /// Gets the parent element which is the root of the REDEFINE.
        /// </summary>
        [Category("IRedefinition")]
        [Description("The parent element which is the root of the REDEFINE.")]
        [ReadOnly(true)]
        public IRedefinition RootLevelRedefinition
        {
            get { return GetRootLevelRedefinition(); }
        }
        #endregion


        #region public methods

        #endregion

        /// <summary>
        /// Causes the object to restore its value to its original data.
        /// </summary>
        public override void ResetToInitialValue()
        {
            // do nothing - we're a redefine. 
        }

        /// <summary>
        /// Initializes value with hex 00 unless default value has been supplied
        /// </summary>
        public override void InitializeWithLowValues()
        {
            // do nothing - we're a redefine. 
        }
    }
}
