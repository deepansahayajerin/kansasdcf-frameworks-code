using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Services;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IGroup.
    /// </summary>
    [InjectionImplementer(typeof(IGroup))]
    [Serializable]
    internal sealed class Group : GroupBase, IGroup, IGroupInitializer, IBufferValue,
        IStructureDefinition, IElementCollection
    {
        #region public methods
        /// <summary>
        /// Returns a deep copy of this element object, applying <paramref name="name"/> as the duplicate object's new 
        /// Name, and offsetting the new object's position by the amount given in <paramref name="bufferPositionOffset"/>.
        /// The new object's Parent is the same as this object's Parent.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="bufferPositionOffset">The amount by which to adjust the new object's position.</param>
        /// <param name="arrayIndexes">The indices of this element and possibly its parents if this element is part of 
        /// an array and/or nested array.</param>
        /// <returns>A new IBufferElement instance of the same type as this object.</returns>
        public IBufferElement Duplicate(string name, int bufferPositionOffset, IEnumerable<int> arrayIndexes)
        {
            IBufferElement result = Duplicate(name, bufferPositionOffset, this.Parent, arrayIndexes);
            return result;
        }

        /// <summary>
        /// Returns a deep copy of this element object, applying <paramref name="name"/> as the duplicate object's new 
        /// Name, and offsetting the new object's position by the amount given in <paramref name="bufferPositionOffset"/>.
        /// The new object is re-parented to <paramref name="newParent"/>.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="bufferPositionOffset">The amount by which to adjust the new object's position.</param>
        /// <param name="newParent">The IElementCollection which will be the new object's Parent.</param>
        /// <param name="arrayIndexes">The indices of this element and possibly its parents if this element is part of 
        /// an array and/or nested array.</param>
        /// <returns>A new IBufferElement instance of the same type as this object.</returns>
        public IBufferElement Duplicate(string name, int bufferPositionOffset, IElementCollection newParent, IEnumerable<int> arrayIndexes)
        {
            string newName;
            ArrayElementUtils.GetElementIndexes(name, out newName);

            string groupName = ArrayElementUtils.MakeElementName(newName, arrayIndexes);

            var result = ObjectFactory.Factory.NewGroupObject(groupName, newParent, Buffer,
                PositionInParent + bufferPositionOffset,
                DefineTimeAccessors, IsInArray, IsInRedefine);
            result.ArrayElementAccessor = ArrayElementAccessor;

            IGroup groupResult = result.AsReadOnly();
            groupResult.IsARedefine = IsARedefine;

            foreach (IBufferElement element in Elements)
            {
                // since we're duplicating to a new parent, we DON'T offset the new position.
                var copy = element.Duplicate(element.Name, 0, groupResult, arrayIndexes);
                groupResult.AddChildElement(copy);
            }


            if (IsInArray && ArrayElementAccessor != null)
            {
                var editableAccessor = ArrayElementAccessor as IEditableArrayElementAccessor<IGroup>;
                if (editableAccessor != null)
                {
                    editableAccessor.AddElement(groupResult);
                }
            }

            (result as IStructureDefinition).EndDefinition();
            return groupResult;
        }
        #endregion

        #region overrides
        /// <summary>
        /// Calculates the length of the group object based on internal structure.
        /// </summary>
        /// <returns>Returns the length of the group object.</returns>
        protected override int CalculateLength()
        {
            int result = 0;

            // for length calculation, never include IRedefinition children.
            foreach (IBufferElement element in ChildElements.Elements.Where(e => !(e is IRedefinition)))
            {
                result += element.LengthInBuffer;
            }

            return result;
        }

        /// <summary>
        /// Returns the starting index for the next element to be added. 
        /// </summary>
        /// <returns>Returns the starting index for the next element to be added.</returns>
        protected override int GetNextElementPosition()
        {
            return ChildElements.Elements.Sum(e => !(e is IRedefinition) ? e.LengthInBuffer : 0);
        }
        #endregion

        public int BufferAddress
        {
            get
            {
                //TBD not implemented
                return 0;

            }
        }

        /// <summary>
        /// Returns a reference to the current group object.
        /// </summary>
        /// <returns>Returns a reference to the current group object.</returns>
        public IGroup AsReadOnly()
        {
            return this as IGroup;
        }

        /// <summary>
        /// Functions as COBOL statement <c>SET ADDRESS OF C TO ADDRESS OF B</c>.
        /// Causes the current group object to point its buffer reference to the 
        /// buffer of the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">A reference to the group element.</param>
        public void SetAddressToAddressOf<T>(T element) where T : IBufferElement, IBufferValue
        {
            bool isReferencePointer = false; IField tField = null;
            if (element is IField)
            {
                tField = (IField)element;
                if (tField.FieldType == Common.FieldType.ReferencePointer)
                    isReferencePointer = true;
            }

            if (isReferencePointer)
            {
                if (tField.AsInt() == -1)
                    return;

                IBufferAddress bufferAddress = BufferServices.BufferAddresses.Get(tField.AsInt());
                IRecord bufferRecord = BufferServices.Records.Get(bufferAddress.RecordKey);
                if (this.LengthInBuffer > bufferRecord.Length)
                    throw new ArgumentOutOfRangeException("Buffer length is shorter than the length of the group");

                this.AssignDataBufferRecursive(bufferRecord.Buffer);
                if (bufferAddress.OptionalBufferStartIndex > 0)
                {
                    this.PositionInParent = bufferAddress.OptionalBufferStartIndex;
                }
            }
            else
            {
                this.AssignDataBufferRecursive(ObjectFactory.Factory.NewDataBufferRedefinePipelineObject(element));
                this.PositionInParent = element.PositionInBuffer;
            }

            //element.ResetToInitialValue();
            // this.RedefinedBuffer = element;
        }

        /// <summary>
        /// Functions as COBOL statement <c>SET ADDRESS OF C TO ADDRESS OF B</c>.
        /// Causes the current group object to point its buffer reference to the 
        /// buffer of the given record.
        /// </summary>
        /// <param name="recordBuffer">A reference to the record object.</param>
        public void SetAddressToAddressOf(IRecord recordBuffer)
        {

            this.AssignDataBufferRecursive(recordBuffer.Buffer);

            this.PositionInParent = 0;
            //recordBuffer.ResetToInitialValue();
            // this.RedefinedBuffer = element;
        }

        /// <summary>
        /// Does nothing, for interface compatibility only.
        /// </summary>
        /// <param name="value">Not used, can take any value.</param>
        public new void AssignIdRecordName(string value)
        {
            // does nothing
        }

        /// <summary>
        /// Does nothing, for interface compatibility only.
        /// </summary>
        /// <param name="value">Not used, can take any value.</param>
        public new void AssignIdRecordName(IBufferValue value)
        {
            // does nothing
        }

        /// <summary>
        /// Does nothing, for interface compatibility only.
        /// </summary>
        /// <returns>Returns an empty string.</returns>
        public new string GetIdRecordName()
        {
            return "";
        }

        /// <summary>
        /// Creates a deep copy of the current group object.
        /// </summary>
        /// <returns>Returns a deep copy of the current group object.</returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
