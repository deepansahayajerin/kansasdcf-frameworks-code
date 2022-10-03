using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using Unity;
using MDSY.Framework.Buffer.Interfaces;
using System.ComponentModel;
using System.Text;
using MDSY.Framework.Buffer.Common;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IArray(of IGroup).
    /// </summary>
    [InjectionImplementer(typeof(IArray<IGroup>))]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    internal sealed class GroupArray : ArrayBase<IGroup>, IGroupArray,
        IArrayFinalizer<IGroup>, IBufferValue, IElementCollection, IGroupArrayInitializer
    {
        #region private methods
        private static IGroupInitializer NewGroupElement(bool isInRedefine,
            string name,
            IDataBuffer dataBuffer,
            GroupArray parent,
            IDictionary<string, IArrayElementAccessorBase> accessors)
        {
            return ObjectFactory.Factory.NewGroupObject(name, parent, dataBuffer, 0, accessors, true, isInRedefine);
        }
        #endregion

        #region IGroupArray
        /// <summary>
        /// Creates and returns the group object which is the first element in the array. 
        /// </summary>
        /// <param name="elementName">Name of the new group object.</param>
        /// <param name="groupInit">A delegate containing logic for setting up the new group object.</param>
        /// <param name="groupDefinition">A delegate containing the logic for defining the structure of the new group.</param>
        /// <param name="arrayElementInit">A delegate containing logic for initializing the new array element. 
        /// Note: this is here to allow you to use the same delegate code here, for the first element, as you do 
        /// for all the subsequent duplicates of the new group.</param>
        /// <returns>Returns a reference to the newly created group object.</returns>
        /// <param name="arrayElementAccessors">A reference to the collection of the array element accessors.</param>
        public IGroup CreateFirstArrayElement(string elementName,
            Action<IGroupInitializer> groupInit,
            Action<IStructureDefinition> groupDefinition,
            Action<IArrayElementInitializer, string, int> arrayElementInit,
            IDictionary<string, IArrayElementAccessorBase> arrayElementAccessors)
        {
            List<int> parentIndexes = (this as IBufferElement).IsInArray ?
                                          new List<int>(GetArrayElementIndexes()) :
                                          new List<int>();
            parentIndexes.Add(0);
            string name = ArrayElementUtils.MakeElementName(elementName, parentIndexes);
            IGroupInitializer result = NewGroupElement(IsInRedefine, name, Buffer, this, arrayElementAccessors);

            groupDefinition(result as IStructureDefinition);
            result.ArrayElementIndex = 0;

            return result.AsReadOnly();
        }

        #endregion

        #region public methods

        /// <summary>
        /// Assigns the given value to the object. (Not implemented, throws NotImplementedException exception.)
        /// </summary>
        /// <param name="value">The new value to assign to the object.</param>
        public void Assign(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assigns the given value to the object, as appropriate. (Not implemented, throws NotImplementedException exception.)
        /// </summary>
        /// <param name="value">String to be assigned.</param>
        public void AssignFrom(string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assigns the value of the given <paramref name="element"/> to this object, as appropriate. (Not implemented, throws NotImplementedException exception.)
        /// </summary>
        /// <param name="element">A reference to the buffer value to be assigned.</param>
        public void AssignFrom(IBufferValue element)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assigns the provided value to the current group array object. (Not implemented, throws NotImplementedException exception.)
        /// </summary>
        /// <param name="element">A reference to the buffer value to be assigned.</param>
        /// <param name="sourceFieldType">Specifies the type of the buffer value object.</param>
        public void AssignFrom(IBufferValue element, FieldType sourceFieldType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assigns the provided group object to the current group array object. (Not implemented, throws NotImplementedException exception.)
        /// </summary>
        /// <param name="group">A reference to the group object to be assigned.</param>
        public void AssignFromGroup(IGroup group)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assigns the given <paramref name="bytes"/> to this object, as appropriate. (Not implemented, throws NotImplementedException exception.)
        /// </summary>
        /// <param name="bytes">Bytes to be assigned.</param>
        public void AssignFrom(byte[] bytes)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region public properties
        /// <summary>
        /// Returns a copy of the value of this object as a byte array. 
        /// </summary>
        [Category("IBufferValue")]
        [Description("Gets the value of this array's elements as bytes.")]
        [ReadOnly(true)]
        public byte[] AsBytes
        {
            get { return this.GetElementsAsBytes(); }
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
                //// performance tweak: read all bytes, and convert, at once. 
                //var bytes = Buffer.ReadBytes(this.PositionInBuffer, this.LengthInBuffer);
                //var result = bytes.Select(b => (AsciiChar)b).NewString();

                StringBuilder result = new StringBuilder();

                for (int i = 0; i < arrayElements.Count; i++)
                {
                    result.Append(arrayElements[i].BytesAsString);
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Returns a string representation of the redefined bytes.
        /// </summary>
        public string RedefinedBytesAsString
        {
            get
            {
                //// performance tweak: read all bytes, and convert, at once. 
                //var bytes = Buffer.ReadBytes(this.PositionInBuffer, this.LengthInBuffer);
                //var result = bytes.Select(b => (AsciiChar)b).NewString();

                StringBuilder result = new StringBuilder();

                for (int i = 0; i < arrayElements.Count; i++)
                {
                    result.Append(arrayElements[i].RedefinedBytesAsString);
                }

                return result.ToString();
            }
        }

        #endregion

        #region overrides
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

        /// <summary>
        /// Duplicates current group array object to the new parent.
        /// </summary>
        /// <param name="name">The name of the duplicated group array object.</param>
        /// <param name="bufferPositionOffset">Specifies offset position of the duplicated group array object in the parent buffer.</param>
        /// <param name="newParent">A reference to the new parent object.</param>
        /// <param name="arrayIndexes">A reference to the collection of the array indexes.</param>
        /// <returns>Returns a reference to the newly created duplicate object.</returns>
        protected override IBufferElement InternalDuplicate(string name,
            int bufferPositionOffset,
            IElementCollection newParent,
            IEnumerable<int> arrayIndexes)
        {
            string copyName;
            ArrayElementUtils.GetElementIndexes(name, out copyName);
            IGroupArray result = ObjectFactory.Factory.NewGroupArrayObject(name: ArrayElementUtils.MakeElementName(copyName, arrayIndexes),
                parentCollection: newParent,
                buffer: Buffer,
                positionInParent: PositionInParent + bufferPositionOffset,
                isSubArray: IsInArray,
                numberOfOccurrances: ArrayElementCount,
                arrayElementLength: ArrayElementLength)  // <-- this addition fixes the GroupArray[n] element LengthInBuffer bug. 
                    .AsReadOnly();

            for (int i = 0; i < arrayElements.Count; i++)
            {
                var nestedIdx = new List<int>(arrayIndexes);
                nestedIdx.Add(i);

                // since we're duplicating to a new parent, we DON'T offset the new position.
                string elementCopyName;
                ArrayElementUtils.GetElementIndexes(arrayElements[i].Name, out elementCopyName);
                var copy = arrayElements[i].Duplicate(elementCopyName, 0, result, nestedIdx);
                result.AddChildElement(copy);
            }

            result.EndDefinition();

            return result;

        }

        /// <summary>
        /// Composes a string that contains the name of the current group array object and the number of array elements that it has.
        /// </summary>
        /// <returns>Retruns the composed string.</returns>
        public override string ToString()
        {
            return string.Format("GroupArray {0}[{1}]", Name, arrayElements.Count);
        }


        #endregion

        /// <summary>
        /// Returns a reference to the current object.
        /// </summary>
        /// <returns>Returns a reference to the current object.</returns>
        public IGroupArray AsReadOnly()
        {
            return this;
        }

        /// <summary>
        /// Does nothing, for interface compatibility only.
        /// </summary>
        /// <param name="value">Not used, can take any value.</param>
        public void AssignIdRecordName(string value)
        {
            // does nothing
        }

        /// <summary>
        /// Does nothing, for interface compatibility only.
        /// </summary>
        /// <param name="value">Not used, can take any value.</param>
        public void AssignIdRecordName(IBufferValue value)
        {
            // does nothing
        }

        /// <summary>
        /// Does nothing, for interface compatibility only.
        /// </summary>
        /// <returns>Returns an empty string.</returns>
        public string GetIdRecordName()
        {
            return "";
        }
    }

}
