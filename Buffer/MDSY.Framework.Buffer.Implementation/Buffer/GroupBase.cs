using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MDSY.Framework.Buffer.Common;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IGroup.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    internal abstract class GroupBase : BufferElementBase, IElementCollection,
        IStructureDefinition, IAssignable, IArrayElementInitializer
    {
        #region abstracts
        /// <summary>
        /// Calculates the length of the group object based on internal structure.
        /// </summary>
        /// <returns>Returns the calculated length of the group object.</returns>
        protected abstract int CalculateLength();

        /// <summary>
        /// Returns the starting index for the next element to be added. 
        /// </summary>
        /// <returns>Returns the starting index for the next element.</returns>
        protected abstract int GetNextElementPosition();
        #endregion

        #region private fields
        private ElementCollectionCompositor childElements;
        private StructureDefinitionCompositor structureDef;
        private IFieldValueSerializer serializer;

        #endregion

        #region private methods

        private string CreateFillWithValue(FillWith fillWith, int length)
        {
            string value = string.Empty;
            if (fillWith != FillWith.DontFill)
            {
                AsciiChar fillChar = AsciiChar.MinValue;
                switch (fillWith)
                {
                    case FillWith.Spaces:
                        fillChar = AsciiChar.From(' ');
                        break;
                    case FillWith.Zeroes:
                        fillChar = AsciiChar.From('0');
                        break;
                    case FillWith.Hashes:
                        fillChar = AsciiChar.From(' ');
                        break;
                    case FillWith.Dashes:
                        fillChar = AsciiChar.From('-');
                        break;
                    case FillWith.Equals:
                        fillChar = AsciiChar.From('=');
                        break;
                    case FillWith.Underscores:
                        fillChar = AsciiChar.From('_');
                        break;
                    case FillWith.HighValues:
                        fillChar = AsciiChar.MaxValue;
                        break;
                    case FillWith.LowValues:
                        fillChar = AsciiChar.MinValue;
                        break;
                    case FillWith.DontFill: // won't get here
                    case FillWith.Nulls:    // already null
                        break;
                }
                value = Enumerable.Repeat(fillChar, length).NewString();
            }
            return value;
        }

        private string GetBytesAsString(bool includeRedefineBytes = false)
        {
            if (RedefinedBuffer != null)
                return RedefinedBuffer.BytesAsString;

            var bytes = this.AsBytes; /* commented out because of ticket 8509 */ // childElements.GetValuesAsBytes(includeRedefineBytes);
            var chars = new List<AsciiChar>();

            for (int i = 0; i < bytes.Length; i++)
            {
                chars.Add(AsciiChar.From(bytes[i]));
            }

            return chars.NewString();
        }
        #endregion

        #region protected properties

        /// <summary>
        /// Contains the calculated length of this group object once the object is no longer IsDefining.
        /// </summary>
        protected int cachedLength = -1;

        /// <summary>
        /// Gets the ElementCollectionCompositor object that contains most of the implementation of IElementCollection
        /// for this object.
        /// </summary>
        public ElementCollectionCompositor ChildElements
        {
            get
            {
                if (childElements == null)
                {
                    childElements = new ElementCollectionCompositor(this);
                }
                return childElements;
            }
            set
            {
                childElements = value;
            }
        }


        /// <summary>
        /// Gets the StructureDefinitionCompositor object that contains most of the implementation of IStructureDefiniton
        /// for this object.
        /// </summary>
        protected StructureDefinitionCompositor StructureDef
        {
            get
            {
                if (structureDef == null)
                {
                    structureDef = new StructureDefinitionCompositor(this as IElementCollection) { Buffer = this.Buffer };
                }
                return structureDef;
            }
        }
        #endregion

        #region protected methods
        #endregion

        #region overrides
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>Returns a string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Format("Group {0}; {1} child elements; {2} ", Name, ChildElements.Count, BytesAsString);
        }

        /// <summary>
        /// Returns the length (in bytes) of this buffer element in the buffer.
        /// </summary>
        /// <returns>Returns the length (in bytes) of this buffer element in the buffer.</returns>
        protected override int GetLength()
        {
            // if we're still defining, calculate the current length.
            // if we're done defining, get the cached length.
            int result = 0;

            if (IsDefining)
            {
                result = CalculateLength();
            }
            else
            {
                if (cachedLength == -1)
                {
                    cachedLength = CalculateLength();
                }

                result = cachedLength;
            }

            return result;
        }


        #endregion

        #region IBufferValue
        /// <summary>
        /// Returns a copy of the value of this object as a byte array. 
        /// </summary>
        [Category("IBufferValue")]
        [Description("Gets the value of this group's child elements as bytes.")]
        [ReadOnly(true)]
        public byte[] AsBytes
        {

            get
            {
                if (RedefinedBuffer != null)
                    return RedefinedBuffer.AsBytes;
                else
                    // commented out because it returns duplicate bytes from the group's redefined elements.
                    //if (IsInRedefine)
                    //    return childElements.GetValuesAsBytes(true);
                    //else
                    return childElements.GetValuesAsBytes(false);
            }
        }

        /// <summary>
        /// Returns the string representation of this object's value.
        /// </summary>
        [Category("IBufferValue")]
        [Description("The string representation of this object's value.")]
        [ReadOnly(true)]
        public string BytesAsString
        {
            get { return GetBytesAsString(false); }
        }

        /// <summary>
        /// Returns a string representation of the redefined bytes.
        /// </summary>
        public string RedefinedBytesAsString
        {
            get { return GetBytesAsString(true); }
        }

        /// <summary>
        /// Returns the string representation of this object's value.
        /// </summary>
        [Category("IBufferValue")]
        [Description("The string representation of this object's value.")]
        [ReadOnly(true)]
        public string DisplayValue
        {
            //get { return GetBytesAsString(); }
            get { return string.Join("", ChildElements.GetValuesAsString()); }
        }

        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets the accessor object that accesses this group object as an array element IF this object 
        /// is part of an array. 
        /// </summary>
        public IArrayElementAccessor<IGroup> ArrayElementAccessor { get; set; }

        /// <summary>
        /// Gets the serializer object responsible for serializing the value.
        /// </summary>
        [Category("IGroup")]
        [Description("An IFieldValueSerializer object responsible for serializing/deserializing this object's value.")]
        public IFieldValueSerializer Serializer
        {
            get
            {
                // Create on demand...
                if (serializer == null)
                {
                    serializer = new FieldValueSerializer();
                }
                return serializer;
            }
            set
            {
                serializer = value;
            }
        }

        /// <summary>
        /// Sets and returns a reference to the redefined buffer.
        /// </summary>
        public IBufferValue RedefinedBuffer { get; set; }

        /// <summary>
        /// Gets or Sets the Field Justification 
        /// </summary>
        [Category("IGroup")]
        [Description("A FieldFormat enum showing justification ")]
        public MDSY.Framework.Buffer.Common.FieldFormat FieldJustification { get; set; }

        #endregion

        #region IAssignable

        /// <summary>
        /// Assigns the given value to the object.
        /// </summary>
        /// <param name="value">The new value to assign to the object.</param>
        public void Assign(object value)
        {
            if (value == null)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (value is IField)
                {
                    AssignFrom(value as IField);
                }
                else if (value is byte[])
                {
                    AssignFrom(value as byte[]);
                }
                else if (value is string)
                {
                    AssignFrom(value as string);
                }
                else if (value is int)
                {
                    string newValue = value.ToString().PadLeft(LengthInBuffer, '0');
                    AssignFrom(newValue);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Assigns the given value to the object, as appropriate.
        /// </summary>
        /// <param name="value">String to be assigned.</param>
        public void AssignFrom(string value)
        {
            if (value.Length < LengthInBuffer)
            {
                value = value.PadRight(LengthInBuffer);
            }
            var bytes = Serializer.Serialize(value, value.Length, FieldType.String, FieldType.String, 0);

            if (RedefinedBuffer != null)
                RedefinedBuffer.Buffer.WriteBytes(bytes, PositionInBuffer, LengthInBuffer);
            else
                Buffer.WriteBytes(bytes, PositionInBuffer, LengthInBuffer);
        }

        /// <summary>
        /// Assigns the given <paramref name="bytes"/> to this object, as appropriate.
        /// </summary>
        /// <param name="bytes">Bytes to be assigned.</param>
        public void AssignFrom(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                throw new ArgumentException("bytes is null or empty.", "bytes");

            if (bytes.Length < LengthInBuffer || LengthInBuffer == 0)
            {
                if (RedefinedBuffer != null)
                    RedefinedBuffer.Buffer.WriteBytes(bytes, PositionInBuffer, bytes.Length);
                else
                    Buffer.WriteBytes(bytes, PositionInBuffer, bytes.Length);
            }
            else
            {
                if (RedefinedBuffer != null)
                    RedefinedBuffer.Buffer.WriteBytes(bytes, PositionInBuffer, LengthInBuffer);
                else
                    Buffer.WriteBytes(bytes, PositionInBuffer, LengthInBuffer);
            }
        }

        /// <summary>
        /// Assigns the value of the given <paramref name="element"/> to this object, as appropriate.
        /// </summary>
        /// <param name="element">A reference to the buffer value to be assigned.</param>
        public void AssignFrom(IBufferValue element)
        {
            if (element == null)
                throw new ArgumentNullException("element", "element is null.");

            AssignFrom(element.AsBytes);
        }

        /// <summary>
        /// Assignes the provided group object to the current group object.
        /// </summary>
        /// <param name="group">A reference to the group object to be assigned.</param>
        public void AssignFromGroup(IGroup group)
        {
            if (group == null)
                throw new ArgumentNullException("group", "group is null.");
            //Update for copying group data when source is shorter than the target group - 2018-11-29
            if (group.LengthInBuffer < this.LengthInBuffer)
            {
                var tempBA = new byte[this.LengthInBuffer];
                group.AsBytes.CopyTo(tempBA, 0);
                for (int i = group.LengthInBuffer; i < tempBA.Length; i++)
                {
                    tempBA[i] = 0x20;
                }
                AssignFrom(tempBA);
            }
            else
                AssignFrom(group.AsBytes);
        }

        /// <summary>
        /// Assigns the provided buffer value object to the current group object.
        /// </summary>
        /// <param name="element">A reference to the buffer element object to be assigned.</param>
        /// <param name="sourceFieldType">The type of the object to be assigned.</param>
        public void AssignFrom(IBufferValue element, FieldType sourceFieldType)
        {
            byte[] bytes;
            bool is_SignedToUnsignedGroup = false;

            if (sourceFieldType == FieldType.SignedNumeric && this.ChildCollection.Count > 0)
            {
                is_SignedToUnsignedGroup = true;
                foreach (KeyValuePair<string, IBufferElement> kvp in this.ChildCollection)
                {
                    if (kvp.Value.IsARedefine) continue;
                    if ( kvp.Value is Group)
                    {
                        is_SignedToUnsignedGroup = false;
                        break;
                    }
                    Field child = (Field)kvp.Value;
                    if(child.FieldType != FieldType.UnsignedNumeric)
                    {
                        is_SignedToUnsignedGroup = false;
                        break;
                    }
                }
            }

            if ((element.BytesAsString.Length < LengthInBuffer) || is_SignedToUnsignedGroup)
            {
                string value;
                if (sourceFieldType != FieldType.String && sourceFieldType != FieldType.Boolean)
                {
                    //Update for issue 8028 - PAdleft with zeroes and then pad right
                    value = element.DisplayValue.PadLeft(element.AsBytes.Length, '0').PadRight(LengthInBuffer);
                }
                else
                {
                    value = element.BytesAsString.PadRight(LengthInBuffer);
                }
                if (sourceFieldType == FieldType.UnsignedNumeric)
                {
                    AssignFrom(value);
                    return;
                }
                bytes = Serializer.Serialize(value, value.Length, FieldType.String, sourceFieldType, 0);
            }
            else
            {
                bytes = element.AsBytes;
            }

            AssignFrom(bytes);
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
        #endregion

        #region public methods
        /// <summary>
        /// Causes the object to restore its value (or its children's values) to its original data.
        /// </summary>
        public void ResetToInitialValue()
        {
            if (this.IsInRedefine)
                return;
            if (RedefinedBuffer != null)
                RedefinedBuffer.InitializeWithLowValues();
            {
                if (this.Record.ResetBuffer != null && Record.Name != "WsExternals")
                {
                    byte[] resetArray = new byte[LengthInBuffer];
                    System.Buffer.BlockCopy(this.Record.ResetBuffer, this.PositionInBuffer, resetArray, 0, LengthInBuffer);
                    this.Assign(resetArray);
                }
                else
                {
                    foreach (IBufferValue item in ChildElements.Elements)
                    {
                        item.ResetToInitialValue();
                    }
                }
            }

        }

        /// <summary>
        /// Initializes value with hex 00 unless default value has been supplied
        /// </summary>
        public void InitializeWithLowValues()
        {
            if (this.IsInRedefine)
                return;
            if (RedefinedBuffer != null)
                RedefinedBuffer.InitializeWithLowValues();
            else
            {
                foreach (IBufferValue item in ChildElements.Elements)
                {
                    item.InitializeWithLowValues();
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void GetAcceptData(string text)
        {
            throw new Exception("GetAcceptData - Not Implemented");
        }

        #endregion

        #region IElementCollection
        /// <summary>
        /// Adds the given <paramref name="element"/> to this object's direct-child elements. 
        /// </summary>
        /// <remarks>
        /// Time and design constraints have forced this method to be here, in IElementCollection, which was supposed 
        /// to be basically a readonly-collection, with adding done elsewhere. I'd love to see this fixed. 
        /// </remarks>  
        public void AddChildElement(IBufferElement element)
        {
            ChildElements.Add(element);
        }

        /// <summary>
        /// Assigns the given <paramref name="buffer"/> to all children of this collection, recursively.
        /// </summary>
        /// <param name="buffer">A reference to the buffer </param>
        public void AssignDataBufferRecursive(IDataBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer", "buffer is null.");


            foreach (IBufferElement element in ChildElements.Elements)
            {
                element.Parent = this;
                element.Record = this.Record;

                if (element is IBufferValue)
                {
                    (element as IBufferValue).Buffer = buffer;
                }

                if (element is IElementCollection)
                {
                    (element as IElementCollection).AssignDataBufferRecursive(buffer);
                }

                //Commented out because not used - IBufferElement otherElement = element.Record.StructureElementByName(element.Name);
                //Commented out because not used -otherElement = element;
            }

        }

        /// <summary>
        /// Returns the number of the nested elements.
        /// </summary>
        /// <returns>Returns the number of the nested elements.</returns>
        public int GetNestedElementCount()
        {
            return ChildElements.NestedElementCount;
        }

        /// <summary>
        /// Returns <c>true</c> if some child element down the record structure tree is an IArray.
        /// </summary>
        [Category("IElementCollection")]
        [Description("Indicates whether a child element is an IArray")]
        public bool HasArrayInChildElements
        {
            get { return childElements.HasArrayInChildElements; }
        }

        /// <summary>
        /// Returns the IBufferElement with the given name.
        /// </summary>
        [Category("IElementCollection")]
        [Description("Indexer property; returns child elements by name.")]
        public IBufferElement this[string name]
        {
            get { return ChildElements[name]; }
        }

        /// <summary>
        /// Gets the collection of child element objects.
        /// </summary>
        [Category("IElementCollection")]
        [Description("Returns a collection of this group's child elements.")]
        public IEnumerable<IBufferElement> Elements
        {
            get { return ChildElements.Elements; }
        }

        /// <summary>
        /// Returns a reference to the child collection of the elements.
        /// </summary>
        public IDictionary<string, IBufferElement> ChildCollection
        {
            get { return ChildElements.ChildCollection; }
        }

        /// <summary>
        /// Returns <c>true</c> if an IBufferElement with the given <paramref name="name"/> exists in the children
        /// this collection.
        /// </summary>
        /// <param name="name">The element name for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        public bool ContainsElement(string name)
        {
            return ChildElements.ContainsElement(name);
        }

        /// <summary>
        /// Returns <c>true</c> if an IBufferElement with the given <paramref name="name"/> exists anywhere within 
        /// the descendants of this collection.
        /// </summary>
        /// <param name="name">The element name for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        public bool ContainsElementNested(string name)
        {
            return ChildElements.ContainsElementNested(name);
        }

        /// <summary>
        /// Returns <c>true</c> if this collection already contains the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The IBufferElement for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        public bool ContainsElement(IBufferElement element)
        {
            return ChildElements.ContainsElement(element);
        }

        /// <summary>
        /// Returns <c>true</c> if the given <paramref name="element"/> exists anywhere within the descendants 
        /// of this collection.
        /// </summary>
        /// <param name="element">The element for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        public bool ContainsElementNested(IBufferElement element)
        {
            return ChildElements.ContainsElementNested(element);
        }

        /// <summary>
        /// Returns an IBufferElement object if one is found with the given <paramref name="name"/> anywhere in the 
        /// collection's child hierarchy.
        /// </summary>
        /// <param name="name">The element name for which to search.</param>
        /// <returns>A matching element object, if found.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is null or empty.</exception>
        /// <exception cref="FieldCollectionException">The given <paramref name="name"/> is not found.</exception>
        public IBufferElement GetElementByNameNested(string name)
        {
            return ChildElements.GetElementByNameNested(name);
        }

        public void SetReferenceTo(IRecord record)
        {
            this.Record = record;
            this.Buffer = record.Buffer;

            foreach (IBufferElement child in ChildCollection.Values)
            {
                child.SetReferenceTo(record);
            }
        }
        #endregion

        #region IStructureDefinition
        /// <summary>
        /// Creates and returns a new IField object. Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public virtual IField CreateNewField(string name, FieldType fieldType, int displayLength)
        {
            return CreateNewField(name, fieldType, displayLength, initialValue: null);
        }

        /* This is messing up initial values like 0. The 0 is being translated to a DBColumnType.Char
        public virtual IField CreateNewField(string name, FieldType fieldType, int displayLength, DBColumnType dbColumnType)
        {
            IField newField = CreateNewField(name, fieldType, displayLength, initialValue: null);
            newField.DBColumnType = dbColumnType;
            return newField;
        }
        */

        /// <summary>
        /// Creates and returns a new IField object. Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="editMask">Edit mask text</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public virtual IField CreateNewField(string name, FieldType fieldType, string editMask, int displayLength)
        {
            return CreateNewField(name, fieldType, editMask, displayLength, initialValue: null);
        }

        /// <summary>
        /// Creates and returns a new IField object whose value is set to <paramref name="initialValue"/>.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Value to be assigned to the new field.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public virtual IField CreateNewField(string name, FieldType fieldType, int displayLength, object initialValue)
        {
            return CreateNewField(name, fieldType, displayLength, initialValue, decimalDigits: 0);
        }

        /// <summary>
        /// Creates and returns a new IField object whose value is set to <paramref name="initialValue"/>.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Value to be assigned to the new field.</param>
        /// <param name="editMask">Edit mask text</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public virtual IField CreateNewField(string name, FieldType fieldType, string editMask, int displayLength, object initialValue)
        {
            return CreateNewField(name, fieldType, editMask, displayLength, initialValue, decimalDigits: 0);
        }

        /// <summary>
        /// Creates and returns a new IField object whose numeric value is set to <paramref name="initialValue"/>
        /// and exhibits <paramref name="decimalDigits"/> number of digits to the right of the decimal. 
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Value to be assigned to the new field.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public IField CreateNewField(string name, FieldType fieldType, int displayLength, object initialValue, int decimalDigits)
        {
            return CreateNewField(name, fieldType, null, displayLength, initialValue, decimalDigits);
        }

        public IField CreateNewField(string name, FieldType fieldType, int displayLength, object initialValue, int decimalDigits, DBColumnType dbColumnType)
        {
            IField newField = CreateNewField(name, fieldType, null, displayLength, initialValue, decimalDigits);
            newField.DBColumnType = dbColumnType;
            return newField;
        }

        /// <summary>
        /// Creates and returns a new IField object whose numeric value is set to <paramref name="initialValue"/>
        /// and exhibits <paramref name="decimalDigits"/> number of digits to the right of the decimal. 
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Value to be assigned to the new field.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal.</param>
        /// <param name="editMask">Edit mask text</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public IField CreateNewField(string name, FieldType fieldType, string editMask, int displayLength, object initialValue, int decimalDigits)
        {
            int position = GetNextElementPosition();
            IField result = StructureDef.CreateNewField(name, fieldType, displayLength, initialValue, position, DefineTimeAccessors, decimalDigits);
            result.EditMask = editMask;
            AddChildElement(result);
            return result;
        }

        /// <summary>
        /// Creates and returns a new array of field objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
        /// <returns>A new instance of an IFieldArray-implementing object.</returns>
        /// <example>
        /// Creates a new record and adds a field array with 10 occurrences:
        /// <code>
        /// IRecord result = BufferServices.Factory.NewRecord("TestRecord", (rec) =>
        /// {
        ///     rec.NewFieldArray("FIELDARRAY01", 10, FieldType.String, 4);
        /// </code>
        /// </example>
        public IFieldArray CreateNewFieldArray(string arrayName,
            int numberOfOccurrances,
            FieldType fieldType,
            int fieldDisplayLength)
        {
            return CreateNewFieldArray(arrayName, numberOfOccurrances, fieldType, fieldDisplayLength, null, 0);
        }

        /// <summary>
        /// Creates and returns a new array of field objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
        /// <returns>A new instance of an IFieldArray-implementing object.</returns>
        /// <example>
        /// Creates a new record and adds a field array with 10 occurrences:
        /// <code>
        /// IRecord result = BufferServices.Factory.NewRecord("TestRecord", (rec) =>
        /// {
        ///     rec.NewFieldArray("FIELDARRAY01", 10, FieldType.String, 4);
        /// </code>
        /// </example>
        public IFieldArray CreateNewFieldArray(string arrayName,
            int numberOfOccurrances,
            FieldType fieldType, string editMask,
            int fieldDisplayLength)
        {
            return CreateNewFieldArray(arrayName, numberOfOccurrances, fieldType, editMask, fieldDisplayLength, null, 0);
        }


        /// <summary>
        /// Creates and returns a new array of field objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
        /// <returns>A new instance of an IFieldArray-implementing object.</returns>
        public IFieldArray CreateNewFieldArray(string arrayName, int numberOfOccurrances, FieldType fieldType, int fieldDisplayLength, object initialFieldValue)
        {
            return CreateNewFieldArray(arrayName, numberOfOccurrances, fieldType, fieldDisplayLength, initialFieldValue, 0);
        }

        /// <summary>
        /// Creates and returns a new array of field objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
        /// <returns>A new instance of an IFieldArray-implementing object.</returns>
        public IFieldArray CreateNewFieldArray(string arrayName, int numberOfOccurrances, FieldType fieldType, string editMask, int fieldDisplayLength, object initialFieldValue)
        {
            return CreateNewFieldArray(arrayName, numberOfOccurrances, fieldType, editMask, fieldDisplayLength, initialFieldValue, 0);
        }

        /// <summary>
        /// Creates and returns a new array of field objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
        /// <param name="initialFieldValue">Initial value of the array element.</param>
        /// <param name="decimalDigits">The number of digits to the right of the decimal separator.</param>
        /// <returns>Returns a reference to the newly created field array object.</returns>
        public IFieldArray CreateNewFieldArray(string arrayName,
            int numberOfOccurrances,
            FieldType fieldType,
            int fieldDisplayLength,
            object initialFieldValue,
            int decimalDigits
)
        {
            int position = GetNextElementPosition();
            IFieldArray result = StructureDef.CreateNewFieldArray(arrayName, numberOfOccurrances, fieldType, null,
                fieldDisplayLength, position, DefineTimeAccessors, decimalDigits, initialFieldValue);
            AddChildElement(result);
            return result;
        }

        /// <summary>
        /// Creates and returns a new array of field objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="editMask">Edit mask text for each field in the new array.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
        /// <param name="initialFieldValue">Initial value of the array element.</param>
        /// <param name="decimalDigits">The number of digits to the right of the decimal separator.</param>
        /// <returns>Returns a reference to the newly created field array object.</returns>
        public IFieldArray CreateNewFieldArray(string arrayName,
    int numberOfOccurrances,
    FieldType fieldType, string editMask,
    int fieldDisplayLength,
    object initialFieldValue,
    int decimalDigits
)
        {
            int position = GetNextElementPosition();
            IFieldArray result = StructureDef.CreateNewFieldArray(arrayName, numberOfOccurrances, fieldType, editMask,
                fieldDisplayLength, position, DefineTimeAccessors, decimalDigits, initialFieldValue);
            AddChildElement(result);
            return result;
        }

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>An new instance of an IField-implementing object.</returns>
        public virtual IField CreateNewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength)
        {
            return CreateNewFieldRedefine(name, fieldType, null, elementToRedefine, displayLength);
        }

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public virtual IField CreateNewFieldRedefine(string name, FieldType fieldType, string editMask, IBufferElement elementToRedefine, int displayLength)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");
            if (elementToRedefine == null)
                throw new ArgumentNullException("elementToRedefine", "elementToRedefine is null.");

            IField result = StructureDef.CreateNewFieldRedefine(name, fieldType, elementToRedefine, displayLength, 0,
                elementToRedefine.PositionInParent, DefineTimeAccessors);
            result.EditMask = editMask;
            result.IsARedefine = true;
            AddChildElement(result);
            return result;
        }

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Initial value for the new field object.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public virtual IField CreateNewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength, object initialValue)
        {
            return CreateNewFieldRedefine(name, fieldType, null, elementToRedefine, displayLength, initialValue, decimalDigits: 0);
        }

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="editMask">Edit mask text for the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Initial value for the new field object.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public virtual IField CreateNewFieldRedefine(string name, FieldType fieldType, string editMask, IBufferElement elementToRedefine, int displayLength, object initialValue)
        {
            return CreateNewFieldRedefine(name, fieldType, editMask, elementToRedefine, displayLength, initialValue, decimalDigits: 0);
        }

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Initial value for the new field object.</param>
        /// <param name="decimalDigits">The number of digits to the right of the decimal separator.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public virtual IField CreateNewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength, object initialValue, int decimalDigits)
        {
            return CreateNewFieldRedefine(name, fieldType, null, elementToRedefine, displayLength, initialValue, decimalDigits);
        }

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="editMask">Edit mask text for the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Initial value for the new field object.</param>
        /// <param name="decimalDigits">The number of digits to the right of the decimal separator.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public virtual IField CreateNewFieldRedefine(string name, FieldType fieldType, string editMask, IBufferElement elementToRedefine, int displayLength, object initialValue, int decimalDigits)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");
            if (elementToRedefine == null)
                throw new ArgumentNullException("elementToRedefine", "elementToRedefine is null.");

            IField result = StructureDef.CreateNewFieldRedefine(name, fieldType, elementToRedefine, displayLength, decimalDigits,
                elementToRedefine.PositionInParent, DefineTimeAccessors);
            result.EditMask = editMask;
            result.IsARedefine = true;
            AddChildElement(result);
            return result;
        }

        /// <summary>
        /// Creates a new field object marked as FILLER, and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <param name="fillWith">The type of value with which to fill the new field.</param>
        public void CreateNewFillerField(int length, FillWith fillWith)
        {
            NewFillerField(length, fillWith);
        }

        /// <summary>
        /// Creates a new field object marked as FILLER, and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <param name="defaultValue">Default value of the filler field.</param>
        public void CreateNewFillerField(int length, string defaultValue)
        {
            NewFillerField(length, defaultValue);
        }

        /// <summary>
        /// Creates a new field object marked as FILLER as a particular type
        /// </summary>
        /// <param name="fieldType">Type of the filler field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Initial value for the new field object.</param>
        /// <param name="decimalLength">The number of bytes required to display the field value.</param>
        public void CreateNewFillerField(FieldType fieldType, int displayLength, object initialValue, int decimalLength = 0)
        {
            NewFillerField(fieldType, displayLength, initialValue, decimalLength);
        }

        /// <summary>
        /// Creates a new field object marked as FILLER, and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        public void CreateNewFillerField(int length)
        {
            NewFillerField(length);
        }

        /// <summary>
        /// Creates and returns a new IGroup object. Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new group.</param>
        /// <param name="definition">A delegate which contains the logic for defining the structure of the new group.</param>
        /// <returns>A new instance of an IGroup-implementing object.</returns>
        /// <example>
        /// Creates a new record and defines the structure as having one group which contains two fields:
        /// <code>
        /// IRecord myRecord = BufferServices.Factory.NewRecord("myRecord", (rec) =>
        /// {
        ///    IGroup group01 = rec.CreateNewGroup("Group01", (grp) =>
        ///    {
        ///        grp
        ///            .NewField("Field_A", typeof(string), 10, "AAAAAAAAAA")
        ///            .NewField("Field_B", typeof(string), 10, "BBBBBBBBBB");
        ///    });
        /// </code>
        /// </example>
        public virtual IGroup CreateNewGroup(string name, Action<IStructureDefinition> definition)
        {
            int position = GetNextElementPosition();
            IGroup result = StructureDef.CreateNewGroup(name, definition, position, DefineTimeAccessors);
            AddChildElement(result);
            return result;
        }


        /// <summary>
        /// Creates and returns a new array of group objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="groupDefinition">A delegate which contains the logic for defining the structure of the first 
        /// group object, which will be the pattern for all instances.</param>
        /// <returns>A new instance of an IGroupArray-implementing object.</returns>
        /// <example>
        /// Creates a new record and adds a group array with 10 occurrences of the group which is defined by 
        /// the included <paramref name="groupDefinition"/> delegate.
        /// <code>
        /// IRecord result = BufferServices.Factory.NewRecord("TestRecord", (rec) =>
        /// {
        ///     rec.CreateNewGroupArray("GROUPARRAY01", 10, 
        ///     (grpDef) =>
        ///     {
        ///         grpDef
        ///             .NewField("ARRAYFIELD_2A", typeof(string), 5, "A1234")
        ///             .NewField("ARRAYFIELD_2B", typeof(string), 5, "B6789")
        ///             .NewGroup("GROUP_C", (g) =>
        ///             {
        ///                 g.NewField("FIELD_C_D", typeof(string), 3, "ABC");
        ///             });
        ///     });
        /// }
        /// </code>
        /// </example>
        public IGroupArray CreateNewGroupArray(string arrayName, int numberOfOccurrances, Action<IStructureDefinition> groupDefinition)
        {
            return CreateNewGroupArray(arrayName, numberOfOccurrances, null, groupDefinition, null, null);
        }

        /// <summary>
        /// Creates and returns a new IGroup object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new group object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="definition">A delegate which contains the logic for defining the structure of the new group.</param>
        /// <returns>A new instance of an IGroup-implementing object.</returns>
        /// <seealso cref="IStructureDefinition.CreateNewGroup"/>
        public virtual IGroup CreateNewGroupRedefine(string name, IBufferElement elementToRedefine, Action<IStructureDefinition> definition)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");
            if (elementToRedefine == null)
                throw new ArgumentNullException("elementToRedefine", "elementToRedefine is null.");
            if (definition == null)
                throw new ArgumentNullException("definition", "definition is null.");

            IGroup result = StructureDef.CreateNewGroupRedefine(name, elementToRedefine, definition, elementToRedefine.PositionInParent, DefineTimeAccessors);
            result.IsARedefine = true;
            AddChildElement(result);
            return result;
        }

        /// <summary>
        /// Creates and returns a new array of group objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="groupInit">A delegate which contains the logic for initializing the first group object, which 
        /// will be the pattern for all instances.</param>
        /// <param name="groupDefinition">A delegate which contains the logic for defining the structure of the first 
        /// group object, which will be the pattern for all instances.</param>
        /// <param name="arrayElementInit">A delegate which contains the logic for modifying each array element as it 
        /// is created. This delegate is called once for each element in the array. Note: the structure of the group 
        /// can not be changed at this point.</param>
        /// <param name="arrayFinal">A delegate which contains the logic for finalizing the array object. This delegate
        /// is called once, after array population is complete.</param>
        /// <returns>A new instance of an IGroupArray-implementing object.</returns>
        /// <example>
        /// Creates a new record and adds a group array with 10 occurrences of the group which is defined by 
        /// the included <paramref name="groupDefinition"/> delegate.
        /// <code>
        /// IRecord result = BufferServices.Factory.NewRecord("TestRecord", (rec) =>
        /// {
        ///     rec.NewGroupArray("GROUPARRAY01", 10, 
        ///     (grpInit) => { },
        ///     (grpDef) =>
        ///     {
        ///         // not optional, define group...
        ///         grpDef
        ///             .NewField("ARRAYFIELD_2A", typeof(string), 5, "A1234")
        ///             .NewField("ARRAYFIELD_2B", typeof(string), 5, "B6789")
        ///             .NewGroup("GROUP_C", (g) =>
        ///             {
        ///                 g.NewField("FIELD_C_D", typeof(string), 3, "ABC");
        ///             })
        ///             ;
        ///     },
        ///     (elmInit, prfx, idx) => { },
        ///     (ary) => { });
        /// </code>
        /// </example>
        public virtual IGroupArray CreateNewGroupArray(string arrayName,
            int numberOfOccurrances,
            Action<IGroupInitializer> groupInit,
            Action<IStructureDefinition> groupDefinition,
            Action<IArrayElementInitializer, string, int> arrayElementInit,
            Action<IArrayFinalizer<IGroup>> arrayFinal)
        {
            int position = GetNextElementPosition();
            IGroupArray result = StructureDef.CreateNewGroupArray(arrayName,
                                        numberOfOccurrances,
                                        groupInit,
                                        groupDefinition,
                                        arrayElementInit,
                                        arrayFinal,
                                        position, DefineTimeAccessors);
            AddChildElement(result);
            return result;
        }


        /// <summary>
        /// Gets the working list of IArrayElementAccessors during "definition-time". 
        /// Note: these are used internally by the data structure building logic and should not be 
        /// accessed by client code. 
        /// After definition-time, this property will be <c>null</c>.
        /// </summary>
        [Category("IStructureDefinition")]
        [Description("Definition-time-only collection of IArrayElementAccessors")]
        public IDictionary<string, IArrayElementAccessorBase> DefineTimeAccessors { get; set; }

        /// <summary>
        /// Gets whether this object is currently in its period of definition.
        /// </summary>
        [Category("IStructureDefinition")]
        [Description("Indicates whether this group object is currently being defined.")]
        public bool IsDefining
        {
            get { return StructureDef.IsDefining; }
            set { StructureDef.IsDefining = value; }
        }


        /// <summary>
        /// Creates a new, populated IFieldArray object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        public IStructureDefinition NewFieldArray(string arrayName,
            int numberOfOccurrances,
            FieldType fieldType,
            int fieldDisplayLength)
        {
            CreateNewFieldArray(arrayName, numberOfOccurrances, fieldType, fieldDisplayLength);
            return this;
        }

        /// <summary>
        /// Creates a new, populated IFieldArray object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
        /// <param name="decimalDigits">The number of digits to the right of the decimal.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        public IStructureDefinition NewFieldArray(string arrayName,
            int numberOfOccurrances,
            FieldType fieldType,
            int fieldDisplayLength,
            object initialFieldValue)
        {
            CreateNewFieldArray(arrayName, numberOfOccurrances, fieldType, fieldDisplayLength, initialFieldValue);
            return this;
        }

        /// <summary>
        /// Creates a new, populated IFieldArray object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
        /// <param name="decimalDigits">The number of digits to the right of the decimal.</param>
        /// <param name="initialFieldValue">The value to be applied as InitialValue for all field elements.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        public IStructureDefinition NewFieldArray(string arrayName,
            int numberOfOccurrances,
            FieldType fieldType,
            int fieldDisplayLength,
            object initialFieldValue,
            int decimalDigits
            )
        {
            CreateNewFieldArray(arrayName, numberOfOccurrances, fieldType, fieldDisplayLength, initialFieldValue, decimalDigits);
            return this;
        }

        /// <summary>
        /// Creates a new field object marked as FILLER, and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        public virtual IStructureDefinition NewFillerField(int length)
        {
            return NewFillerField(length, FillWith.DontFill);
        }


        /// <summary>
        /// Creates a new IFiller object and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <param name="fillWith">Specifies the filling character.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        public virtual IStructureDefinition NewFillerField(int length, FillWith fillWith)
        {
            int position = GetNextElementPosition();

            string value = string.Empty;
            if (fillWith != FillWith.DontFill)
            {
                AsciiChar fillChar = AsciiChar.MinValue;
                switch (fillWith)
                {
                    case FillWith.Spaces:
                        fillChar = AsciiChar.From(' ');
                        break;
                    case FillWith.Zeroes:
                        fillChar = AsciiChar.From('0');
                        break;
                    case FillWith.Hashes:
                        fillChar = AsciiChar.From(' ');
                        break;
                    case FillWith.Dashes:
                        fillChar = AsciiChar.From('-');
                        break;
                    case FillWith.Equals:
                        fillChar = AsciiChar.From('=');
                        break;
                    case FillWith.Underscores:
                        fillChar = AsciiChar.From('_');
                        break;
                    case FillWith.HighValues:
                        fillChar = AsciiChar.MaxValue;
                        break;
                    case FillWith.LowValues:
                        fillChar = AsciiChar.MinValue;
                        break;
                    case FillWith.DontFill: // won't get here
                    case FillWith.Nulls:    // already null
                        break;
                }
                value = Enumerable.Repeat(fillChar, length).NewString();
            }

            IField filler = StructureDef.CreateNewField("FILLER", FieldType.String, length, value, position, DefineTimeAccessors, 0, true);

            AddChildElement(filler);
            return this;
        }

        /// <summary>
        /// Creates a new IFiller object and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Length of the filler object.</param>
        /// <param name="stringValue">The filling string.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        public virtual IStructureDefinition NewFillerField(int length, string stringValue)
        {
            int position = GetNextElementPosition();

            IField filler = StructureDef.CreateNewField("FILLER", FieldType.String, length, stringValue, position, DefineTimeAccessors, 0, true);

            AddChildElement(filler);
            return this;
        }

        /// <summary>
        /// Creates a new IFiller object and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="fieldType">Type of the filler object.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Initial value of the filler object.</param>
        /// <param name="decimalLength">Number of digits to the right of the decimal separator.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        public virtual IStructureDefinition NewFillerField(FieldType fieldType, int displayLength, object initialValue, int decimalLength = 0)
        {

            int position = GetNextElementPosition();
            IField filler;
            if (initialValue is FillWith)
            {
                string defaultValue = CreateFillWithValue((FillWith)initialValue, displayLength);
                filler = StructureDef.CreateNewField("FILLER", fieldType, displayLength, defaultValue, position, DefineTimeAccessors, decimalLength, true);
            }
            else
            {
                filler = StructureDef.CreateNewField("FILLER", fieldType, displayLength, initialValue, position, DefineTimeAccessors, decimalLength, true);
            }


            AddChildElement(filler);
            return this;
        }

        /// <summary>
        /// Creates a new IGroup object and adds it to this IStructureDefinition object as a child. 
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">Name of the group.</param>
        /// <param name="definition">Delegate of the structure definition method.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        /// <seealso cref="CreateNewGroup"/>
        public virtual IStructureDefinition NewGroup(string name, Action<IStructureDefinition> definition)
        {
            CreateNewGroup(name, definition);
            return this;
        }

        /// <summary>
        /// Creates a new, populated IGroupArray object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="groupDefinition">A delegate which contains the logic for defining the structure of the first 
        /// group object, which will be the pattern for all instances.</param>
        /// <returns>This IStructureDefinition-implementer.</returns>
        /// <example>
        /// Creates a new record and adds a group array with 10 occurrences of the group which is defined by 
        /// the included <paramref name="groupDefinition"/> delegate.
        /// <code>
        /// IRecord result = BufferServices.Factory.NewRecord("TestRecord", (rec) =>
        /// {
        ///     rec.NewGroupArray("GROUPARRAY01", 10, 
        ///     (grpDef) =>
        ///     {
        ///         // not optional, define group...
        ///         grpDef
        ///             .NewField("ARRAYFIELD_2A", typeof(string), 5, "A1234")
        ///             .NewField("ARRAYFIELD_2B", typeof(string), 5, "B6789")
        ///             .NewGroup("GROUP_C", (g) =>
        ///             {
        ///                 g.NewField("FIELD_C_D", typeof(string), 3, "ABC");
        ///             })
        ///             ;
        ///     });
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="CreateNewGroupArray"/>
        public IStructureDefinition NewGroupArray(string arrayName, int numberOfOccurrances, Action<IStructureDefinition> groupDefinition)
        {
            return NewGroupArray(arrayName, numberOfOccurrances, null, groupDefinition, null, null);
        }

        /// <summary>
        /// Creates a new group redefine object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">Name of the group.</param>
        /// <param name="elementToRedefine">A reference to the redefinition object.</param>
        /// <param name="definition">A delegate which contains the logic for defining the structure of the group object</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        /// <seealso cref="CreateNewGroupRedefine"/>
        public virtual IStructureDefinition NewGroupRedefine(string name, IBufferElement elementToRedefine, Action<IStructureDefinition> definition)
        {
            CreateNewGroupRedefine(name, elementToRedefine, definition);
            return this;
        }

        /// <summary>
        /// Creates a new IField object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">The type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        /// <seealso cref="CreateNewField"/>
        public virtual IStructureDefinition NewField(string name, FieldType fieldType, int displayLength)
        {
            CreateNewField(name, fieldType, displayLength);
            return this;
        }

        /// <summary>
        /// Creates a new IField object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">Name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Initial value of the field.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        /// <seealso cref="CreateNewField"/>
        public virtual IStructureDefinition NewField(string name, FieldType fieldType, int displayLength, object initialValue)
        {
            CreateNewField(name, fieldType, displayLength, initialValue);
            return this;
        }

        /// <summary>
        /// Creates a new IField object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">Name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Initial value of the field.</param>
        /// <param name="decimalDigits">The number of digits to the right of the decimal separator.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        public IStructureDefinition NewField(string name, FieldType fieldType, int displayLength, object initialValue, int decimalDigits)
        {
            CreateNewField(name, fieldType, displayLength, initialValue, decimalDigits);
            return this;
        }

        /// <summary>
        /// Creates a new field redefine object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">Name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="elementToRedefine">A reference to the redefine object.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        public virtual IStructureDefinition NewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength)
        {
            CreateNewFieldRedefine(name, fieldType, elementToRedefine, displayLength);
            return this;
        }

        /// <summary>
        /// Creates a new, populated IGroupArray object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="arrayName">Name of the new group array object.</param>
        /// <param name="numberOfOccurrances">Number of elements in the array.</param>
        /// <param name="groupInit">Group initializer delegate.</param>
        /// <param name="groupDefinition">Structure definition delegate.</param>
        /// <param name="arrayElementInit">Array element initializer delegate.</param>
        /// <param name="arrayFinal">Array initializer delegate.</param>
        /// <returns>A reference to the structure definition implementer of the current object.</returns>
        public virtual IStructureDefinition NewGroupArray(string arrayName,
            int numberOfOccurrances,
            Action<IGroupInitializer> groupInit,
            Action<IStructureDefinition> groupDefinition,
            Action<IArrayElementInitializer, string, int> arrayElementInit,
            Action<IArrayFinalizer<IGroup>> arrayFinal)
        {
            CreateNewGroupArray(arrayName, numberOfOccurrances, groupInit, groupDefinition, arrayElementInit, arrayFinal);
            return this;
        }

        /// <summary>
        /// Informs the object that its period of definition has ended. 
        /// </summary>
        public void EndDefinition()
        {
            if (IsDefining)
            {
                StructureDef.EndDefinition();
                cachedLength = CalculateLength();
            }
        }
        public void RestartDefinition()
        {
        }
        #endregion

        #region IComparable<T>

        /// <summary>
        /// Compares the provided field object with the current object.
        /// </summary>
        /// <param name="other">A reference to the field object for comparison.</param>
        /// <returns>A 32-bit signed integer that indicates the ordered relationship between the two comparands.</returns>
        public int CompareTo(IField other)
        {
            // params are swapped, so flip the result
            return ComparisonMatrix.Compare(other, this as IGroup) * -1;
        }

        /// <summary>
        /// Compares current object with the provided group object.
        /// </summary>
        /// <param name="other">A reference to the group object for comparison.</param>
        /// <returns>A 32-bit signed integer that indicates the ordered relationship between the two comparands.</returns>
        public int CompareTo(IGroup other)
        {
            return ComparisonMatrix.Compare(this as IGroup, other);
        }

        /// <summary>
        /// Compares current object with the provided record object.
        /// </summary>
        /// <param name="other">A reference to the record object for comparison.</param>
        /// <returns>A 32-bit signed integer that indicates the ordered relationship between the two comparands.</returns>
        public int CompareTo(IRecord other)
        {
            return ComparisonMatrix.Compare(this as IGroup, other);
        }

        /// <summary>
        /// Compares current object with the provided string.
        /// </summary>
        /// <param name="other">A reference to the string for comparison.</param>
        /// <returns>A 32-bit signed integer that indicates the ordered relationship between the two comparands.</returns>

        public int CompareTo(string other)
        {
            return ComparisonMatrix.Compare(this as IGroup, other);
        }
        #endregion

        #region IEquatable<T>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(IField other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(IGroup other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(IRecord other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(string other)
        {
            return CompareTo(other) == 0;

        }
        #endregion
    }
}
