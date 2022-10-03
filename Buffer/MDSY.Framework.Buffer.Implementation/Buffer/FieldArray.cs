using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
//CHADusing Unity.Attributes;
using MDSY.Framework.Buffer.Common;
using System.ComponentModel;
using System.Text;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IArray(of IGroup).
    /// </summary>
    [InjectionImplementer(typeof(IArray<IField>))]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    internal sealed class FieldArray : ArrayBase<IField>, IFieldArray,
        IArrayFinalizer<IField>, IBufferValue, IFieldArrayInitializer
    {
        #region public methods

        /// <summary>
        /// Assigns the given value to the object.
        /// </summary>
        /// <param name="value">The new value to assign to the object.</param>
        public void Assign(object value)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Assigns the given value to the object, as appropriate. (Not implemented, throws NotImplementedException exception.)
        /// </summary>
        /// <param name="value">String value to be assigned.</param>
        public void AssignFrom(string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assigns the value of the given <paramref name="element"/> to this object, as appropriate. (Not implemented, throws NotImplementedException exception.)
        /// </summary>
        /// <param name="element">Buffer value to be assinged.</param>
        public void AssignFrom(IBufferValue element)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assgins the provided value to the current object. (Not implemented, throws NotImplementedException exception.)
        /// </summary>
        /// <param name="element">Buffer value to be assigned.</param>
        /// <param name="sourceFieldType">Specifies the type of the provided buffer value.</param>
        public void AssignFrom(IBufferValue element, FieldType sourceFieldType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assigns the given <paramref name="bytes"/> to this object, as appropriate. (Not implemented, throws NotImplementedException exception.)
        /// </summary>
        /// <param name="bytes">The bytes to be assigned.</param>
        public void AssignFrom(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assigns the given <paramref name="group"/> to this object, as appropriate. (Not implemented, throws NotImplementedException exception.)
        /// </summary>
        /// <param name="group">A reference to the group object to be assigned.</param>
        public void AssignFromGroup(IGroup group)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new instance of the field initialization object and initializes it with the provided data.
        /// </summary>
        /// <param name="fieldType">Specifies the type for the new field.</param>
        /// <param name="fieldDisplayLength">Specifies the length for the new field.</param>
        /// <param name="decimalDigits">Specifies the number of digits to the right from the decimal point in the new field.</param>
        /// <param name="name">Specifies the name for the new field.</param>
        /// <param name="fieldBufferLength">Specifies how many bytes in the buffer the new field should occupy.</param>
        /// <returns>Returns a reference to the newly created field initialization object.</returns>
        private IFieldInitializer NewFieldObject(FieldType fieldType, int fieldDisplayLength, int decimalDigits, string name, int fieldBufferLength)
        {
            return ObjectFactory.Factory.NewFieldObject(name,
                        this,
                        Buffer,
                        fieldType,
                        fieldBufferLength,
                        fieldDisplayLength,
                        positionInParent: 0,
                        decimalDigits: decimalDigits,
                        isInArray: true,
                        arrayElementIndex: 0,
                        isRedefine: IsInRedefine);
        }

        /// <summary>
        /// Creates and returns the field object which is the first element in the array. 
        /// </summary>
        /// <param name="elementName">Name of the new field object.</param>
        /// <param name="fieldType">The type fo the field object.</param>
        /// <param name="fieldDisplayLength">Display length of the field.</param>
        /// <param name="decimalDigits">The number of digits to the right from the decimal separator in the field.</param>
        /// <param name="initialValue">Initial value of the field.</param>
        /// <returns>Returns a reference to the newly created object.</returns>
        public IField CreateFirstArrayElement(string elementName,
            FieldType fieldType,
            int fieldDisplayLength,
            int decimalDigits,
            object initialValue)
        {
            List<int> parentIndexes = (this as IBufferElement).IsInArray ?
                                        new List<int>(GetArrayElementIndexes()) :
                                        new List<int>();

            parentIndexes.Add(0);
            string name = ArrayElementUtils.MakeElementName(elementName, parentIndexes);

            int fieldBufferLength = ObjectFactory.GetFieldBufferLength(fieldType, fieldDisplayLength);
            IFieldInitializer result = NewFieldObject(fieldType, fieldDisplayLength, decimalDigits, name, fieldBufferLength);
            result.ArrayElementIndex = 0;

            if (!IsInRedefine)
            {
                // we need to populate the new field with something to build its space in the buffer.
                if (initialValue != null)
                {
                    result.Assign(initialValue);
                }
                else
                {
                    result.AssignFrom(Enumerable.Repeat<byte>(0x00, fieldBufferLength).ToArray());
                }
            }

            return result.AsReadOnly();
        }

        #endregion

        #region public properties
        /// <summary>
        /// Returns a copy of the value of this object as a byte array. 
        /// </summary>
        /// <returns>A new byte[].</returns>
        [Category("IBufferValue")]
        [Description("Gets the value of this array's elements as bytes.")]
        [ReadOnly(true)]
        public byte[] AsBytes
        {
            get { return GetElementsAsBytes(); }
        }

        /// <summary>
        /// Returns the string representation of this object's value.
        /// </summary>
        [Category("IBufferValue")]
        [Description("The string representation of this object's value.")]
        [ReadOnly(true)]
        public string BytesAsString
        {
            get
            {
                StringBuilder result = new StringBuilder();

                for (int i = 0; i < arrayElements.Count; i++)
                {
                    result.Append(arrayElements[i].BytesAsString);
                }

                return result.ToString();
            }
        }

        [Category("IBufferValue")]
        [Description("The string representation of this object's value.")]
        [ReadOnly(true)]
        public string RedefinedBytesAsString
        {
            get
            {
                StringBuilder result = new StringBuilder();

                for (int i = 0; i < arrayElements.Count; i++)
                {
                    result.Append(arrayElements[i].RedefinedBytesAsString);
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Returns the string representation of this object's value.
        /// </summary>
        [Category("IBufferValue")]
        [Description("The string representation of this object's value.")]
        [ReadOnly(true)]
        public string DisplayValue
        {
            get
            {
                StringBuilder result = new StringBuilder();

                for (int i = 0; i < arrayElements.Count; i++)
                {
                    result.Append(arrayElements[i].DisplayValue);
                }

                return result.ToString();
            }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Duplicates current array object to the new parent. Override this method to provide custom duplication behavior.
        /// </summary>
        /// <param name="name">Array name.</param>
        /// <param name="bufferPositionOffset">Array's offset position in the buffer.</param>
        /// <param name="newParent">Specifies a reference to the new parent object.</param>
        /// <param name="arrayIndexes">Collection of the array indexes.</param>
        /// <returns>Returns a reference to the duplicated array instance.</returns>
        protected override IBufferElement InternalDuplicate(string name, int bufferPositionOffset, IElementCollection newParent, IEnumerable<int> arrayIndexes)
        {
            string newName;
            // just get the base name
            ArrayElementUtils.GetElementIndexes(name, out newName);

            IFieldArray result = ObjectFactory.Factory.NewFieldArrayObject(ArrayElementUtils.MakeElementName(newName, arrayIndexes),
                            newParent, Buffer, PositionInParent + bufferPositionOffset, ArrayElementCount, ArrayElementLength,
                            IsInArray, IsFiller)
                            .AsReadOnly();

            for (int i = 0; i < arrayElements.Count; i++)
            {
                var nestedIdx = new List<int>(arrayIndexes);
                nestedIdx.Add(i);

                // since we're duplicating to a new parent, we DON'T offset the new position.
                var copy = arrayElements[i].Duplicate(arrayElements[i].Name, 0, result, nestedIdx);
                result.AddChildElement(copy);
            }

            result.EndDefinition();
            return result;
        }

        /// <summary>
        /// Generates a string that contains the array name and the number of the array elements.
        /// </summary>
        /// <returns>Returns the generated string.</returns>
        public override string ToString()
        {
            return string.Format("FieldArray {0}[{1}]", Name, arrayElements.Count);
        }

        #endregion

        /// <summary>
        /// Returns a reference to the current object.
        /// </summary>
        /// <returns>Returns a reference to the current object.</returns>
        public IFieldArray AsReadOnly()
        {
            return this;
        }

        /// <summary>
        /// Does nothing. For interface compatibility only.
        /// </summary>
        /// <param name="value">Not used. Can take any value.</param>
        public void AssignIdRecordName(string value)
        {
            // does nothing
        }

        /// <summary>
        /// Does nothing. For interface compatibility only.
        /// </summary>
        /// <param name="value">Not used. Can take any value.</param>
        public void AssignIdRecordName(IBufferValue value)
        {
            // does nothing
        }

        /// <summary>
        /// Does nothing. For interface compatibility only.
        /// </summary>
        /// <returns>Returns an empty string.</returns>
        public string GetIdRecordName()
        {
            return "";
        }
    }
}
