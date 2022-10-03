using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Services;
using MDSY.Framework.Buffer.BaseClasses;
using System.ComponentModel;
using Unity;
//CHADusing Unity.Attributes;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements a standard IRecord.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [InjectionImplementer(typeof(IRecord))]
    [Serializable]
    internal sealed class Record : IRecord, IStructureDefinition, IElementCollection,
        IAssignable, IDisposable
    {
        #region Injection
        /// <summary>
        /// Gets or sets the record object's data buffer object. This is injected by Unity at build-up time using a 
        /// temporary, List(of byte)-based buffer. Later it is replaced with a byte array-buffer.
        /// </summary>
        [Dependency("InitialDataBuffer")]
        [Category("IRecord")]
        [Description("Data buffer object for this Record.")]
        [ReadOnly(true)]
        public IDataBuffer Buffer { get; set; }

        public byte[] ResetBuffer { get; set; }

        #endregion

        #region internal use ONLY
        /// <summary>
        /// Seriously, don't use this field. It's only here to help work around a 
        /// DI + serialization + debug visualizer issue. 
        /// </summary>
        [NonSerialized]
        private IRecordBufferCollectionService initialRecordBuffersCollectionLink;
        /// <summary>
        /// No really, don't use this field either. 
        /// </summary>
        [NonSerialized]
        private readonly IRecordCollectionService recordCollectionLink;
        #endregion

        #region private fields
        private bool bufferPointerIsRemapped = false;
        private int cachedLength = 0;
        private Dictionary<string, IArrayElementAccessorBase> arrayElementAccessors = null;
        private IDictionary<string, IArrayElementAccessorBase> defineTimeAccessors = new Dictionary<string, IArrayElementAccessorBase>();
        private IDictionary<string, IBufferElement> structureElements = null;
        private StructureDefinitionCompositor structureDef = null;
        private ElementCollectionCompositor childElements = null;

        private IDictionary<string, IArrayElementAccessorBase> ArrayElementAccessors
        {
            get
            {
                if (arrayElementAccessors == null)
                {
                    arrayElementAccessors = new Dictionary<string, IArrayElementAccessorBase>();
                }
                return arrayElementAccessors;
            }
        }

        private ElementCollectionCompositor ChildElements
        {
            get
            {
                if (childElements == null)
                {
                    childElements = new ElementCollectionCompositor(this);
                }

                return childElements;
            }
        }

        private StructureDefinitionCompositor StructureDef
        {
            get
            {
                if (structureDef == null)
                {
                    structureDef = new StructureDefinitionCompositor(this) { Buffer = Buffer };
                }

                return structureDef;
            }
        }

        private IDictionary<string, IBufferElement> StructureElements
        {
            get
            {
                if (structureElements == null)
                {
                    structureElements = new Dictionary<string, IBufferElement>();
                }

                return structureElements;
            }
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Record"/> class.
        /// </summary>
        public Record()
        {
            Buffer = null;
            initialRecordBuffersCollectionLink = null;
            recordCollectionLink = BufferServices.Records;
            Name = null;
        }
        #endregion

        #region public methods


        /// <summary>
        /// Returns a count of all elements beneath this collection, at any level. 
        /// </summary>
        /// <returns>
        /// A recursive count of all descendant elements.
        /// </returns>
        public int GetNestedElementCount()
        {
            return ChildElements.NestedElementCount;
        }
        #endregion

        #region private methods

        private int CalculateLength()
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
        /// After definition is complete, copies over any ArrayElementAccessors generated during 
        /// definition, then clears the DefineTimeAccessors list so it's not available during run-time.  
        /// </summary>
        private void CopyDefineTimeAccessors()
        {
            if (DefineTimeAccessors == null)
                return;
            if (DefineTimeAccessors.Count > 0)
            {
                foreach (KeyValuePair<string, IArrayElementAccessorBase> kvPair in DefineTimeAccessors)
                {
                    ArrayElementAccessors.Add(kvPair);
                }
            }

            DefineTimeAccessors = null;
        }

        private static string CreateFillWithValue(FillWith fillWith, int length)
        {
            string result = string.Empty;
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
                result = Enumerable.Repeat(fillChar, length).NewString();
            }
            return result;
        }

        public void AssignDataBuffer(IDataBuffer buffer)
        {
            if (buffer.Length != Length && Length != 0)
            {
                DataBufferByteArray tempBuff = (DataBufferByteArray)buffer;
                byte[] newBuf = new byte[Length];
                if (Name == "WsExternals")
                {
                    System.Buffer.BlockCopy(tempBuff.GetBytes(), 0, newBuf, 0, tempBuff.Length);
                    tempBuff.SetBytes(newBuf);
                }
                else
                {
                tempBuff.InitializeBytes(newBuf);
            }
            }
            Buffer = buffer;
            AssignDataBufferRecursive(Buffer);
        }

        private void FinalizeDefinitionBuffer()
        {
            AssignDataBuffer(Buffer.GetFinalBuffer());
        }


        private int getLength()
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

        private int GetNextElementPosition()
        {
            return ChildElements.Elements.Sum(e => !(e is IRedefinition) ? e.LengthInBuffer : 0);
        }

        /// <summary>
        /// Assigns the specified <paramref name="record"/> to the given <paramref name="element"/> and recursivly to
        /// any child elements of <paramref name="element"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="record"></param>
        private static void SetElementRecord(IBufferElement element, IRecord record)
        {
            if (element is IBufferElementInitializer)
            {
                (element as IBufferElementInitializer).Record = record;
            }

            if (element is IElementCollection)
            {
                var collection = element as IElementCollection;
                foreach (IBufferElement el in collection.Elements)
                {
                    SetElementRecord(el, record);
                }
            }
        }
        #endregion

        #region public properties

        /// <summary>
        /// Returns the IBufferElement with the given name; applies to elements which are direct children of this record.
        /// To get an element which is owned by this record, but may be nested, use StructureElementByName().
        /// </summary>
        [Category("IRecord")]
        [Description("Returns the buffer eleent with the given name.")]
        [ReadOnly(true)]
        public IBufferElement this[string name]
        {
            get { return ChildElements[name]; }
        }

        /// <summary>
        /// Gets the length of this record (in bytes) as defined by the elements making up the record's structure.
        /// </summary>
        [Category("IRecord")]
        [Description("Length of this record object.")]
        [ReadOnly(true)]
        public int Length
        {
            get { return getLength(); }
        }

        /// <summary>
        /// Gets the name of this record object.
        /// </summary>
        [Category("IRecord")]
        [Description("Name of the record object.")]
        [ReadOnly(true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets the name of this record object.
        /// </summary>
        [Category("IRecord")]
        [Description("Address Key of the record object.")]
        [ReadOnly(true)]
        public int RecordKey { get; set; }
        #endregion

        #region IStructureDefinition
        /// <summary>
        /// Creates and returns a new IField object. Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public IField CreateNewField(string name, FieldType fieldType, int displayLength)
        {
            return CreateNewField(name, fieldType, null, displayLength);
        }

        public IField CreateNewField(string name, FieldType fieldType, int displayLength, DBColumnType dbColumnType)
        {
            IField newField = CreateNewField(name, fieldType, null, displayLength);
            newField.DBColumnType = dbColumnType;
            return newField;
        }

        /// <summary>
        /// Creates and returns a new IField object. Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="editMask">Edit mask test</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public IField CreateNewField(string name, FieldType fieldType, string editMask, int displayLength)
        {
            return CreateNewField(name, fieldType, editMask, displayLength, null);
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
        public IField CreateNewField(string name, FieldType fieldType, int displayLength, object initialValue)
        {
            return CreateNewField(name, fieldType, null, displayLength, initialValue, 0);
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
        public IField CreateNewField(string name, FieldType fieldType, string editMask, int displayLength, object initialValue)
        {
            return CreateNewField(name, fieldType, editMask, displayLength, initialValue, 0);
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
        /// <returns>A new instance of an IField-implementing object.</returns>
        public IField CreateNewField(string name, FieldType fieldType, string editMask, int displayLength, object initialValue, int decimalDigits)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");
            if (displayLength <= 0)
                throw new ArgumentException("displayLength must be greater than 0", "displayLength");

            int position = GetNextElementPosition();
            IField result = StructureDef.CreateNewField(name, fieldType, displayLength, initialValue, position, DefineTimeAccessors, decimalDigits);
            result.EditMask = editMask;
            AddChildElement(result);
            AddStructureElement(result); // <-- sets the new field's Record property.
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
        /// <param name="decimalDigits">The number of digits to the right of the decimal.</param>
        /// <returns>A new instance of an IFieldArray-implementing object.</returns>
        /// <example>
        /// Creates a new record and adds a field array with 10 occurrences:
        /// <code>
        /// IRecord result = BufferServices.Factory.NewRecord("TestRecord", (rec) =>
        /// {
        ///     rec.NewFieldArray("FIELDARRAY01", 10, FieldType.String, 4);
        /// }
        /// </code>
        /// </example>
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
        /// <param name="decimalDigits">The number of digits to the right of the decimal.</param>
        /// <param name="initialFieldValue">The value to be applied as InitialValue for all field elements.</param>
        /// <returns>A new instance of an IFieldArray-implementing object.</returns>
        /// <example>
        /// Creates a new record and adds a field array with 10 occurrences:
        /// <code>
        /// IRecord result = BufferServices.Factory.NewRecord("TestRecord", (rec) =>
        /// {
        ///     rec.NewFieldArray("FIELDARRAY01", 10, FieldType.String, 4);
        /// }
        /// </code>
        /// </example>
        public IFieldArray CreateNewFieldArray(string arrayName,
            int numberOfOccurrances,
            FieldType fieldType,
            int fieldDisplayLength,
            object initialFieldValue,
            int decimalDigits)
        {
            int position = GetNextElementPosition();

            IFieldArray result = StructureDef.CreateNewFieldArray(arrayName,
                                     numberOfOccurrances,
                                     fieldType, null,
                                     fieldDisplayLength,
                                     position,
                                     DefineTimeAccessors,
                                     decimalDigits,
                                     initialFieldValue);
            AddChildElement(result);
            AddStructureElement(result);
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
        /// <param name="decimalDigits">The number of digits to the right of the decimal.</param>
        /// <returns>A new instance of an IFieldArray-implementing object.</returns>
        /// <example>
        /// Creates a new record and adds a field array with 10 occurrences:
        /// <code>
        /// IRecord result = BufferServices.Factory.NewRecord("TestRecord", (rec) =>
        /// {
        ///     rec.NewFieldArray("FIELDARRAY01", 10, FieldType.String, 4);
        /// }
        /// </code>
        /// </example>
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
        /// <param name="decimalDigits">The number of digits to the right of the decimal separator.</param>
        /// <param name="initialFieldValue">The value to be applied as InitialValue for all field elements.</param>
        /// <returns>A new instance of an IFieldArray-implementing object.</returns>
        /// <example>
        /// Creates a new record and adds a field array with 10 occurrences:
        /// <code>
        /// IRecord result = BufferServices.Factory.NewRecord("TestRecord", (rec) =>
        /// {
        ///     rec.NewFieldArray("FIELDARRAY01", 10, FieldType.String, 4);
        /// }
        /// </code>
        /// </example>
        public IFieldArray CreateNewFieldArray(string arrayName,
            int numberOfOccurrances,
            FieldType fieldType, string editMask,
            int fieldDisplayLength,
            object initialFieldValue,
            int decimalDigits)
        {
            int position = GetNextElementPosition();

            IFieldArray result = StructureDef.CreateNewFieldArray(arrayName,
                                     numberOfOccurrances,
                                     fieldType, editMask,
                                     fieldDisplayLength,
                                     position,
                                     DefineTimeAccessors,
                                     decimalDigits,
                                     initialFieldValue);
            AddChildElement(result);
            AddStructureElement(result);
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
        /// <returns>A new instance of an IField-implementing object.</returns>
        public IField CreateNewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength)
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
        /// <param name="editMask">Edit mask text</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public IField CreateNewFieldRedefine(string name, FieldType fieldType, string editMask, IBufferElement elementToRedefine, int displayLength)
        {
            // since this is a record creating a redefine field, elementToRedefine must not be null, 
            // positionInParent and positionInBuffer both equate to elementToRedefine.PositionInBuffer, 
            // and lengthInBuffer must == elementToRedefine.Length.
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");
            if (elementToRedefine == null)
                throw new ArgumentNullException("elementToRedefine", "elementToRedefine is null.");

            IField result = StructureDef.CreateNewFieldRedefine(name, fieldType, elementToRedefine, displayLength, 0,
                elementToRedefine.PositionInParent, DefineTimeAccessors);
            result.EditMask = editMask;
            result.IsARedefine = true;
            AddChildElement(result);
            AddStructureElement(result);
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
        /// <returns>A new instance of an IField-implementing object.</returns>
        public IField CreateNewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength, object initValue)
        {
            return CreateNewFieldRedefine(name, fieldType, null, elementToRedefine, displayLength, initValue, 0);
        }

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="editMask">Edit mask text.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initValue">Initial value.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public IField CreateNewFieldRedefine(string name, FieldType fieldType, string editMask, IBufferElement elementToRedefine, int displayLength, object initValue)
        {
            return CreateNewFieldRedefine(name, fieldType, editMask, elementToRedefine, displayLength, initValue, 0);
        }

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initValue">Initial value.</param>
        /// <param name="decimalDigits">The number of digits to the right of the decimal separator.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public IField CreateNewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength, object initValue, int decimalDigits)
        {
            return CreateNewFieldRedefine(name, fieldType, null, elementToRedefine, displayLength, initValue, decimalDigits);
        }

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="editMask">Edit mask text.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initValue">Initial value.</param>
        /// <param name="decimalDigits">The number of digits to the right of the decimal separator.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public IField CreateNewFieldRedefine(string name, FieldType fieldType, string editMask, IBufferElement elementToRedefine, int displayLength, object initValue, int decimalDigits)
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
            AddStructureElement(result);
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
        /// <param name="defaultValue">The type of value with which to fill the new field.</param>
        public void CreateNewFillerField(int length, string defaultValue)
        {
            NewFillerField(length, defaultValue);
        }

        /// <summary>
        /// Creates a new field object marked as FILLER but with a specific type, length, and initial Value
        /// </summary>
        /// <param name="fieldType">The type of the filler field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Initial value.</param>
        /// <param name="decimalLength">The number of bytes to the right from the decimal separator.</param>
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
        public IGroup CreateNewGroup(string name, Action<IStructureDefinition> definition)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");
            if (definition == null)
                throw new ArgumentNullException("definition", "definition is null.");

            int position = GetNextElementPosition();
            IGroup result = StructureDef.CreateNewGroup(name, definition, position, DefineTimeAccessors);
            AddChildElement(result);
            AddStructureElement(result); // <-- sets the new group's (and all child elements') Record properties.
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
            var result = CreateNewGroupArray(arrayName, numberOfOccurrances, null, groupDefinition, null, null);

            return result;
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
        public IGroup CreateNewGroupRedefine(string name, IBufferElement elementToRedefine, Action<IStructureDefinition> definition)
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
            AddStructureElement(result);
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
        public IGroupArray CreateNewGroupArray(string arrayName,
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
                                        position,
                                        DefineTimeAccessors);
            AddChildElement(result);
            AddStructureElement(result);
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
        public IDictionary<string, IArrayElementAccessorBase> DefineTimeAccessors
        {
            get
            {
                return defineTimeAccessors;
            }
            set
            {
                defineTimeAccessors = value;
            }
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
                FinalizeDefinitionBuffer();
                CopyDefineTimeAccessors();
            }
            if (Name != "WsExternals" && WsExternals.IsInDefinition)
            {
                WsExternals.Instance.EndExternalDefinition();
            }
        }
        public void RestartDefinition()
        {
            StructureDef.RestartDefinition();
        }

        /// <summary>
        /// Creates a new IField object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>Returns a reference to the current object.</returns>
        public IStructureDefinition NewField(string name, FieldType fieldType, int displayLength)
        {
            CreateNewField(name, fieldType, displayLength);
            return this;
        }

        /// <summary>
        /// Creates a new IField object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Initial value of the field.</param>
        /// <returns>Returns a reference to the current object.</returns>
        public IStructureDefinition NewField(string name, FieldType fieldType, int displayLength, object initialValue)
        {
            CreateNewField(name, fieldType, displayLength, initialValue);
            return this;
        }

        /// <summary>
        /// Creates a new IField object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Initial value of the field.</param>
        /// <param name="decimalDigits">The number of bytes to the right of the decimal separator.</param>
        /// <returns>Returns a reference to the current object.</returns>
        public IStructureDefinition NewField(string name, FieldType fieldType, int displayLength, object initialValue, int decimalDigits)
        {
            CreateNewField(name, fieldType, displayLength, initialValue, decimalDigits);
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
        /// <returns>Returns a reference to the current object.</returns>
        public IStructureDefinition NewFieldArray(string arrayName, int numberOfOccurrances, FieldType fieldType, int fieldDisplayLength)
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
        /// <returns>This IStructureDefinition-implementer.</returns>
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
        /// <returns>This IStructureDefinition-implementer.</returns>
        public IStructureDefinition NewFieldArray(string arrayName,
            int numberOfOccurrances,
            FieldType fieldType,
            int fieldDisplayLength,
            object initialFieldValue,
            int decimalDigits)
        {
            CreateNewFieldArray(arrayName, numberOfOccurrances, fieldType, fieldDisplayLength, initialFieldValue, decimalDigits);
            return this;
        }

        /// <summary>
        /// Creates a new IFiller object and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <param name="fillWith">Number of bytes the new filler should occupy.</param>
        /// <returns>A reference to the current object.</returns>
        public IStructureDefinition NewFillerField(int length, FillWith fillWith)
        {
            int position = GetNextElementPosition();
            string value = CreateFillWithValue(fillWith, length);
            IField filler = StructureDef.CreateNewField("FILLER", FieldType.String, length, value, position, DefineTimeAccessors, 0, true);

            AddChildElement(filler);
            AddStructureElement(filler);
            return this;
        }

        /// <summary>
        /// Creates a new IFiller object and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <param name="defaultValue">The filler's default value.</param>
        /// <returns>A reference to the current object.</returns>
        public IStructureDefinition NewFillerField(int length, string defaultValue)
        {
            int position = GetNextElementPosition();

            IField filler = StructureDef.CreateNewField("FILLER", FieldType.String, length, defaultValue, position, DefineTimeAccessors, 0, true);

            AddChildElement(filler);
            AddStructureElement(filler);
            return this;
        }

        /// <summary>
        /// Creates a new IFiller object and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="fieldType">The type of the filler field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Initial value of the filler field.</param>
        /// <param name="decimalLength">The number of bytes to the right from the decimal separator.</param>
        /// <returns>A reference to the current object.</returns>
        public IStructureDefinition NewFillerField(FieldType fieldType, int displayLength, object initialValue, int decimalLength = 0)
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
            AddStructureElement(filler);
            return this;
        }

        /// <summary>
        /// Creates a new field object marked as FILLER, and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <returns>A reference to the current object.</returns>
        public IStructureDefinition NewFillerField(int length)
        {
            return NewFillerField(length, FillWith.DontFill);
        }

        /// <summary>
        /// Creates a new IGroup object and adds it to this IStructureDefinition object as a child. 
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">The name of the group.</param>
        /// <param name="definition">A delegate which contains the logic for defining the structure of the new group.</param>
        /// <returns>A reference to the current object.</returns>
        public IStructureDefinition NewGroup(string name, Action<IStructureDefinition> definition)
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
        /// <param name="name">The name of the group.</param>
        /// <param name="elementToRedefine">A reference to the record structure elements, which must be redefined.</param>
        /// <param name="definition">A delegate which contains the logic for defining the structure of the new group.</param>
        /// <returns>A reference to the current object.</returns>
        public IStructureDefinition NewGroupRedefine(string name, IBufferElement elementToRedefine, Action<IStructureDefinition> definition)
        {
            CreateNewGroupRedefine(name, elementToRedefine, definition);
            return this;
        }

        /// <summary>
        /// Creates a new field redefine object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">The name of the new field.</param>
        /// <param name="fieldType">The type of the new field.</param>
        /// <param name="elementToRedefine">A reference to the record structure elements, which must be redefined.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>A reference to the current object.</returns>
        public IStructureDefinition NewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength)
        {
            CreateNewFieldRedefine(name, fieldType, elementToRedefine, displayLength);
            return this;
        }

        /// <summary>
        /// Creates a new, populated IGroupArray object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="arrayName">The name of the array.</param>
        /// <param name="numberOfOccurrances">Number of elements in the array.</param>
        /// <param name="groupInit">A delegate which contains the logic for initializing the first group object, which 
        /// will be the pattern for all instances.</param>
        /// <param name="groupDefinition">A delegate which contains the logic for defining the structure of the first 
        /// group object, which will be the pattern for all instances.</param>
        /// <param name="arrayElementInit">A delegate which contains the logic for modifying each array element as it 
        /// is created. This delegate is called once for each element in the array. Note: the structure of the group 
        /// can not be changed at this point.</param>
        /// <param name="arrayFinal">A delegate which contains the logic for finalizing the array object. This delegate
        /// is called once, after array population is complete.</param>
        /// <returns>A reference to the current object.</returns>
        public IStructureDefinition NewGroupArray(string arrayName,
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
        /// Gets whether this object is currently in its period of definition.
        /// </summary>
        [Category("IStructureDefinition")]
        [Description("Indicates whether this record object is currently being defined.")]
        public bool IsDefining
        {
            get { return StructureDef.IsDefining; }
            set { StructureDef.IsDefining = value; }
        }
        #endregion

        #region IRecord


        /// <summary>
        /// Returns an IArrayElementAccessor of the given <typeparamref name="TItem"/> type
        /// if one is found with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Array element accessor name.</param>
        /// <returns>A reference to the array element accessor.</returns>
        public IArrayElementAccessor<TItem> GetArrayElementAccessor<TItem>(string name) where TItem : IArrayElement
        {
            IArrayElementAccessor<TItem> result = null;

            if (ArrayElementAccessors.ContainsKey(name))
            {
                var accessor = ArrayElementAccessors[name];
                if (accessor is IArrayElementAccessor<TItem>)
                {
                    result = accessor as IArrayElementAccessor<TItem>;
                }
            }

            return result;
        }

        /// <summary>
        /// Adds the given <paramref name="element"/> to a collection of all the buffer elements owned by 
        /// this record. Element names must be unique. 
        /// </summary>
        /// <remarks>
        /// <para><note>This is a separate collection from any child elements owned by this record as an 
        /// IElementCollection. The structure elements are a collection of all elements under this record 
        /// at any level. i.e. Children, grandchildren, g'grandchildren, etc.</note></para>
        /// </remarks>
        /// <param name="element">A reference to the buffer element to be added.</param>
        public void AddStructureElement(IBufferElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element", "element is null.");
            if (String.IsNullOrEmpty(element.Name))
                throw new ArgumentException("element.Name is null or empty", "element");

            try
            {
                if (!(element is ICheckField && StructureElements.ContainsKey(element.Name)))
                {
                    StructureElements.Add(element.Name, element);
                    SetElementRecord(element, this);

                    if (element is IElementCollection)
                    {
                        foreach (IBufferElement el in (element as IElementCollection).Elements)
                        {
                            AddStructureElement(el);
                        }
                    }
                    if (element is IField)
                    {
                        foreach (ICheckField checkField in (element as IField).CheckFields)
                        {
                            AddStructureElement(checkField);
                        }
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                throw new RecordStructureException(string.Format("There was a problem adding element {0} to record {1}, and element with that name already exists.",
                    element.Name, Name), ex);
            }
            catch (ArgumentException ex)
            {
                throw new RecordStructureException(string.Format("There was a problem adding the element to record {1}, check the element's Name property: ('{0}')",
                    element.Name, Name), ex);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if this record has among its structural elements the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The IBufferElement for which to search.</param>
        /// <returns><c>true</c> if the element exists in the StructuralElements collection.</returns>
        public bool ContainsStructuralElement(IBufferElement element)
        {
            return StructureElements.Values.Contains(element);
        }

        
        /// <summary>
        /// Returns <c>true</c> if this record has among its structural elements an element with the given <paramref name="elementName"/>.
        /// </summary>
        /// <param name="elementName">The name of the structure element.</param>
        /// <returns>Returns true the the requested element belongs to the collection of the structure elements.</returns>
        public bool ContainsStructuralElement(string elementName)
        {
            return StructureElements.ContainsKey(elementName);
        }

        /// <summary>
        /// Returns a readonly collection of all the IBufferElements owned by this record at any level.
        /// </summary>
        /// <returns>An unsorted, flattened collection of IBufferElements.</returns>
        public IEnumerable<IBufferElement> GetStructureElements()
        {
            return StructureElements.Values;
        }

        /// <summary>
        /// Retrieves a substring from the string representation of the current record value.
        /// </summary>
        /// <param name="startPos">Specifies the string index from which the substring should start.</param>
        /// <param name="length">Specifies the length of the substring.</param>
        /// <returns>Returns a substring from the string representation of the current record value.</returns>
        public string GetSubstring(int startPos, int length)
        {
            if (this.AsString().Length < length)
                length = this.AsString().Length;
            return (this.AsString().Substring(startPos - 1, length));
        }

        /// <summary>
        /// Restores the buffer pointer mapping of this record to its original 
        /// IDataBuffer object.
        /// </summary>
        public void RestoreInitialDataBuffer()
        {
            if (bufferPointerIsRemapped)
            {
                initialRecordBuffersCollectionLink = BufferServices.InitialRecordBuffers;
                var originalBuffer = initialRecordBuffersCollectionLink.Get(this.Name);

                if (originalBuffer == null)
                    throw new CollectionServiceException("Unable to restore initial buffer; object was not found.");

                Buffer = null;
                AssignDataBuffer(originalBuffer);
                bufferPointerIsRemapped = false;
            }
        }

        public void ResetInitialValue()
        {
            //Performance Update 08_2019
            if (ResetBuffer == null)
            {
                foreach (IBufferValue item in this.Elements)
                {
                    item.ResetToInitialValue();
                }

                if (Length > 0)
                {
                    ResetBuffer = new byte[Length];
                    System.Buffer.BlockCopy(Buffer.ReadBytes(), 0, ResetBuffer, 0, Length);
                }
            }
            else
                this.Assign(ResetBuffer);
        }

        /// <summary>
        /// Functions as COBOL statement <c>SET ADDRESS OF RECORD_A TO ADDRESS OF RECORD_B</c>.
        /// Causes the record object to point its buffer reference to the 
        /// buffer of the given <paramref name="record"/>.
        /// </summary>
        /// <param name="record">The record object whose DataBuffer this record 
        /// will now point to.</param>
        public void SetAddressToAddressOf(IRecord record)
        {
            if (record == null)
                throw new ArgumentNullException("record", "record is null.");

            // We need to store a reference to the old buffer so that we can re-assign 
            // it later, if called for. Initial buffers are stored by owning record name. 
            initialRecordBuffersCollectionLink = BufferServices.InitialRecordBuffers;

            if (initialRecordBuffersCollectionLink == null)
                throw new InvalidInjectionOperationException();

            if (initialRecordBuffersCollectionLink.GetKeyFor(Buffer) != this.Name)
            {
                // only add the buffer if we don't already have it...
                initialRecordBuffersCollectionLink.Add(this.Buffer, this);
                bufferPointerIsRemapped = true;

                Buffer = null;
                AssignDataBuffer(record.Buffer);
            }
        }

        /// <summary>
        /// Returns the structure element with the given name, if it exists within the record at any level.
        /// </summary>
        /// <param name="name">The name of the structure element.</param>
        /// <returns>Returns a reference to the requested structure element.</returns>
        public IBufferElement StructureElementByName(string name)
        {
            return StructureElements.ContainsKey(name) ? StructureElements[name] : null;
        }

        public int GetBufferAddressKey()
        {

            int recordKey = BufferServices.Records.GetKeyFor(this);
            if (recordKey == 0)
            {
                recordKey = BufferServices.Records.Add(this);
            }

            IBufferAddress bufferAddress = ObjectFactory.Factory.NewBufferAddress(recordKey, this.Name);
            int addressKey = BufferServices.BufferAddresses.Add(bufferAddress);
            return addressKey;
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
        /// <param name="element">A reference to the buffer element to be added.</param>
        public void AddChildElement(IBufferElement element)
        {
            ChildElements.Add(element);
        }

        /// <summary>
        /// Assigns the given <paramref name="buffer"/> to all children of this collection, recursively.
        /// </summary>
        /// <param name="buffer">A reference to the buffer object to be assigned.</param>
        public void AssignDataBufferRecursive(IDataBuffer buffer)
        {
            // if this is not our own buffer, we have a problem...
            //if (!Buffer.Equals(buffer))
            //    throw new NotImplementedException();

            foreach (IBufferElement element in ChildElements.Elements)
            {
                element.Record = this;
                element.Parent = this;

                if (element is IBufferValue)
                {
                    (element as IBufferValue).Buffer = buffer;
                }

                if (element is IElementCollection)
                {
                    (element as IElementCollection).AssignDataBufferRecursive(buffer);
                }

            }
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
        /// Gets the collection of child element objects.
        /// </summary>
        [Category("IElementCollection")]
        [Description("Collection of direct child elements.")]
        public IEnumerable<IBufferElement> Elements
        {
            get { return ChildElements.Elements; }
        }

        /// <summary>
        /// Returns a reference to the child collection of the record's child elements.
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
        /// Returns <c>true</c> if this collection already contains the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The IBufferElement for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        public bool ContainsElement(IBufferElement element)
        {
            return ChildElements.ContainsElement(element);
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

        /// <summary>
        /// Returns <c>true</c> if some child element down the record structure tree is an IArray.
        /// </summary>
        [Category("IElementCollection")]
        [Description("Indicates whether a child element is an IArray")]
        public bool HasArrayInChildElements
        {
            get { return childElements.HasArrayInChildElements; }
        }
        #endregion

        #region overrides
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            //return string.Format("Record {0}; {1} child elements", Name, ChildElements.Count);
            return Name;
        }
        #endregion

        #region IAssignable
        /// <summary>
        /// Assigns the given value to the object.
        /// </summary>
        /// <param name="value">The new value to assign to the object.</param>
        public void Assign(object value)
        {
            if (value is IField)
            {
                AssignFrom(value as IField);
            }
            if (value is IGroup)
            {
                AssignFrom(value as IGroup);
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
                AssignFrom(Convert.ToString((int)value));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Assigns the given string value to the object, as appropriate.
        /// </summary>
        public void AssignFrom(string value)
        {
            if (String.IsNullOrEmpty(value))
                this.ResetToInitialValue();
            else
            {
                var bytes = value.Select(c => (Byte)(AsciiChar)c).ToArray();
                AssignFrom(bytes);
            }
        }

        /// <summary>
        /// Assigns the value of the given <paramref name="element"/> to this object, as appropriate.
        /// </summary>
        /// <param name="element">A reference to the buffer element to be assigned.</param>
        public void AssignFrom(IBufferValue element)
        {
            AssignFrom(element.AsBytes);
        }

        /// <summary>
        /// Assigns the provided group to the current record.
        /// </summary>
        /// <param name="group">A reference to the group object to be assigned.</param>
        public void AssignFromGroup(IGroup group)
        {
            AssignFrom(group.AsBytes);
        }

        /// <summary>
        /// Assigns the provided element of the specified type to the current record.
        /// </summary>
        /// <param name="element">A reference to the element to be assigned.</param>
        /// <param name="sourceFieldType">Specifies the type of the element to be assigned.</param>
        public void AssignFrom(IBufferValue element, FieldType sourceFieldType)
        {
            AssignFrom(element.AsBytes);
        }

        /// <summary>
        /// Assigns the given <paramref name="bytes"/> to this object, as appropriate.
        /// </summary>
        /// <param name="bytes">A reference to the array of bytes to be assigned.</param>
        public void AssignFrom(byte[] bytes)
        {
            if (bytes != null)
            {
                if (bytes.Length > Length)
                {
                    Buffer.WriteBytes(bytes, 0, Length);
                }
                else if (bytes.Length < Length)
                {
                    Buffer.WriteBytes(bytes, 0, bytes.Length);
                }
                else
                {
                    Buffer.WriteBytes(bytes);
                }
            }
        }

        /// <summary>
        /// Does nothing, for the interface compatibility only.
        /// </summary>
        /// <param name="value">Not used, can take any value.</param>
        public void AssignIdRecordName(string value)
        {
            // does nothing
        }

        /// <summary>
        /// Does nothing, for the interface compatibility only.
        /// </summary>
        /// <param name="value">Not used, can take any value.</param>
        public void AssignIdRecordName(IBufferValue value)
        {
            // does nothing
        }

        /// <summary>
        /// Does nothing, for the interface compatibility only.
        /// </summary>
        /// <returns>Returns an empty string.</returns>
        public string GetIdRecordName()
        {
            return "";
        }

        #endregion

        #region Disposable pattern
        // Flag: Has Dispose already been called? 
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free any other managed objects here. 
                    //
                }

                // If the record's data buffer was added to the InitialRecordBuffers collection, remove it. 
                if (initialRecordBuffersCollectionLink != null)
                {
                    initialRecordBuffersCollectionLink.Remove(this.Name);
                }

                // remove the record from the system record collection.
                if (recordCollectionLink != null)
                {
                    recordCollectionLink.Remove(this);
                }
                disposed = true;
            }
        }

        ~Record()
        {
            Dispose(false);
        }
        #endregion

        #region IComparable<T>

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">A reference to the field object for comparison.</param>
        /// <returns>A signed integer that indicates the relation between the objects.</returns>
        public int CompareTo(IField other)
        {
            return ComparisonMatrix.Compare(other, this) * -1;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">A reference to the field object for comparison.</param>
        /// <returns>A signed integer that indicates the relation between the objects.</returns>
        public int CompareTo(IGroup other)
        {
            return ComparisonMatrix.Compare(other, this) * -1;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">A reference to the field object for comparison.</param>
        /// <returns>A signed integer that indicates the relation between the objects.</returns>
        public int CompareTo(IRecord other)
        {
            return ComparisonMatrix.Compare(this, other);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">A reference to the field object for comparison.</param>
        /// <returns>A signed integer that indicates the relation between the objects.</returns>
        public int CompareTo(string other)
        {
            return ComparisonMatrix.Compare(this as IRecord, other);
        }
        #endregion

        #region IEquatable<T>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IField other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IGroup other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IRecord other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(string other)
        {
            return CompareTo(other) == 0;
        }
        #endregion

        #region IClonable

        /// <summary>
        /// Creates a deep copy of the current object.
        /// </summary>
        /// <returns>Returns a deep copy of the current object.</returns>
        public object Clone()
        {
            Record cloneRecord = (Record)this.MemberwiseClone();
            cloneRecord.RecordKey = 0;
            DataBufferByteArray tempBuffer = new DataBufferByteArray();
            tempBuffer.SetBytes(new byte[cloneRecord.Length]);
            cloneRecord.Buffer = tempBuffer;

            //Clone the arrayElementAccessors
            cloneRecord.arrayElementAccessors = new Dictionary<string, IArrayElementAccessorBase>();
            foreach (string IAEBaseKey in this.ArrayElementAccessors.Keys)
            {
                cloneRecord.arrayElementAccessors.Add(IAEBaseKey, (IArrayElementAccessorBase)this.ArrayElementAccessors[IAEBaseKey].Clone());
            }

            if (this.defineTimeAccessors != null)
                cloneRecord.defineTimeAccessors = new Dictionary<string, IArrayElementAccessorBase>(this.defineTimeAccessors);

            //Clone the Record's child elements recursively
            cloneRecord.structureElements = new Dictionary<string, IBufferElement>();
            cloneRecord.childElements = this.childElements.Clone();
             foreach (IBufferElement thisElement in this.childElements.Elements)
            {
                CloneRecursive(thisElement, cloneRecord.ChildCollection[thisElement.Name], cloneRecord, cloneRecord);
            }

            return cloneRecord;
        }

        /// <summary>
        /// Clones the Record's child elements recursively
        /// </summary>
        /// <param name="oldElement">A reference to the buffer element to be cloned.</param>
        /// <param name="cloneElement">A reference to the buffer element for clonning.</param>
        /// <param name="cloneRecord">Record for cloning.</param>
        /// <param name="cloneParent">Parent element of the record for cloning.</param>
        private void CloneRecursive(IBufferElement oldElement, IBufferElement cloneElement, Record cloneRecord, IElementCollection cloneParent)
        {
            cloneElement.Record = cloneRecord;
            cloneElement.Parent = cloneParent;
            if (cloneElement is IBufferValue)
            {
                (cloneElement as IBufferValue).Buffer = cloneRecord.Buffer;
            }

            if (oldElement is IField)
            {
                IField oldField = (IField)oldElement;
                if (oldField.CheckFields.Count() > 0)
                {
                    //Clone CheckFields
                    foreach (CheckField chField in oldField.CheckFields)
                    {
                        CheckField newCheckField = (CheckField)chField.Clone();
                        newCheckField.Buffer = cloneRecord.Buffer;
                        newCheckField.Record = cloneRecord;
                        newCheckField.Parent = cloneParent;
                        newCheckField.Field = (IField)cloneElement;
                        (cloneElement as FieldBase).AddCheckField(newCheckField);
                        cloneRecord.structureElements.Add(newCheckField.Name, newCheckField);
                        if (newCheckField.HasArrayInParents)
                        {
                            ArrayElementAccessorBase<ICheckField> cloneAccessor = (ArrayElementAccessorBase<ICheckField>)cloneRecord.arrayElementAccessors[ArrayElementUtils.GetElementBaseName(newCheckField.Name)];
                            cloneAccessor.AddElement((ICheckField)newCheckField);
                        }
                    }
                }
            }

            if (cloneElement.HasArrayInParents)
            {
                if (cloneElement is IGroup)
                {
                    ArrayElementAccessorBase<IGroup> cloneAccessor = (ArrayElementAccessorBase<IGroup>)cloneRecord.arrayElementAccessors[ArrayElementUtils.GetElementBaseName(cloneElement.Name)];
                    cloneAccessor.AddElement((IGroup)cloneElement);
                }
                else if (cloneElement is IField)
                {
                    ArrayElementAccessorBase<IField> cloneAccessor = (ArrayElementAccessorBase<IField>)cloneRecord.arrayElementAccessors[ArrayElementUtils.GetElementBaseName(cloneElement.Name)];
                    cloneAccessor.AddElement((IField)cloneElement);
                }
            }

            //Check if clone element is group
            if (oldElement is IElementCollection)
            {
                IElementCollection elementCollect = (IElementCollection)oldElement;
                if (oldElement is GroupBase)
                {
                    (cloneElement as GroupBase).ChildElements = (oldElement as GroupBase).ChildElements.Clone();
                    foreach (IBufferElement element in elementCollect.Elements)
                    {
                        CloneRecursive(element, (cloneElement as IGroup).ChildCollection[element.Name], cloneRecord,(IElementCollection)cloneElement);
                    }
                }
                //Check if clone element is Field Array
                else if (oldElement is FieldArray)
                {
                    FieldArray farray = (FieldArray)oldElement;
                    FieldArray newfarray = (FieldArray)cloneElement;
                    for (int ctr = 0; ctr < farray.ArrayElements.Count; ctr++)
                    {
                        newfarray.ArrayElements[ctr] = (IField)farray.ArrayElements[ctr].Clone();
                        CloneRecursive(farray.ArrayElements[ctr], newfarray.ArrayElements[ctr], cloneRecord, (IElementCollection)cloneElement);
                    }
                }
                //Check if clone element is Group Array
                else if (oldElement is GroupArray)
                {
                    GroupArray farray = (GroupArray)oldElement;
                    GroupArray newfarray = (GroupArray)cloneElement;
                    for (int ctr = 0; ctr < farray.ArrayElements.Count; ctr++)
                    {
                        newfarray.ArrayElements[ctr] = (IGroup)farray.ArrayElements[ctr].Clone();
                        CloneRecursive(farray.ArrayElements[ctr], newfarray.ArrayElements[ctr], cloneRecord, (IElementCollection)cloneElement);
                    }
                }
            }
            //Update structure elements reference with new clone element
            cloneRecord.structureElements.Add(cloneElement.Name,cloneElement);

        }
        #endregion
    }
}
