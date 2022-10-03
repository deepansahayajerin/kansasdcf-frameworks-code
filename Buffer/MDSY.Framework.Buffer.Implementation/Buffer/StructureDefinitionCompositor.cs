using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Unity;
using System.ComponentModel;


namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// A composition object that provides support for IStructureDefinition implementers. 
    /// </summary>
    [Serializable]
    internal class StructureDefinitionCompositor
    {
        #region private fields

        /// <summary>
        /// Returns true if the owner collection of the current object belongs to the array object.
        /// </summary>
        private bool IsInArray
        {
            get
            {
                bool result = false;

                if (ownerCollection is IBufferElement)
                {
                    result = (ownerCollection as IBufferElement).IsInArray;
                }

                return result;
            }
        }

        /// <summary>
        /// This is the element collection which has this StructureDefinitionCompositor.
        /// </summary>
        private readonly IElementCollection ownerCollection;
        #endregion

        #region private methods
        /// <summary>
        /// Verifies that the object is in Defining mode
        /// </summary>
        /// <exception cref="RecordStructureException">IsDefining is <c>false</c>.</exception>
        private void CheckDefining()
        {
            if (!IsDefining)
            {
                throw new RecordStructureException(string.Format("The current element collection ({0}) is closed for defining; you can only add to the structure of a record while IsDefining is true.", ownerCollection));
            }
        }

        /// <summary>
        /// Creates and initializes a new field object.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new object.</param>
        /// <param name="fieldDisplayLength">The number of bytes that are required to display the value of the field.</param>
        /// <param name="positionInParent">Byte index of the field within its Parent.</param>
        /// <param name="isFiller">Specifies whether the field is a filler field.</param>
        /// <param name="digits">Number of digits to the right of the decimal separator.</param>
        /// <param name="accessors">Field accessors.</param>
        /// <returns>Returns a reference to the new field initializer.</returns>
        private IFieldInitializer NewField(string name, FieldType fieldType, int fieldDisplayLength,
            int positionInParent, bool isFiller, int digits, IDictionary<string, IArrayElementAccessorBase> accessors)
        {
            IFieldInitializer result = ObjectFactory.Factory.NewFieldObject(name,
                parentCollection: ownerCollection,
                buffer: Buffer,
                fieldType: fieldType,
                lengthInBuffer: ObjectFactory.GetFieldBufferLength(fieldType, fieldDisplayLength),
                displayLength: fieldDisplayLength,
                positionInParent: positionInParent,
                decimalDigits: digits,
                isInArray: IsInArray,
                arrayElementIndex: Constants.Defaults.ArrayElementIndex,
                isRedefine: Constants.Defaults.IsRedefine,
                isFiller: isFiller);

            result.DefineTimeAccessors = accessors;

            return result;

        }

        private IFieldInitializer NewRedefineField(string name, FieldType fieldType,
            IBufferElement elementToRedefine, int fieldDisplayLength, int fieldDecimalDigits,
            int positionInParent, bool isFiller, IDictionary<string, IArrayElementAccessorBase> accessors)
        {
            IFieldInitializer result = ObjectFactory.Factory.NewRedefineFieldObject(name,
                Buffer,
                ownerCollection,
                fieldType,
                elementToRedefine,
                lengthInBuffer: ObjectFactory.GetFieldBufferLength(fieldType, fieldDisplayLength),
                displayLength: fieldDisplayLength,
                decimalDigits: fieldDecimalDigits,
                positionInParent: positionInParent,
                isInArray: IsInArray,
                arrayElementIndex: Constants.Defaults.ArrayElementIndex,
                isFiller: isFiller);

            result.DefineTimeAccessors = accessors;

            return result;
        }

        private IGroupInitializer NewGroup(string name, int positionInParent, IDictionary<string, IArrayElementAccessorBase> accessors)
        {
            return ObjectFactory.Factory.NewGroupObject(name,
                ownerCollection,
                Buffer,
                positionInParent,
                accessors,
                IsInArray,
                Constants.Defaults.IsRedefine);
        }

        private IEnumerable<int> GetParentIndexes(out bool isUnderArray)
        {
            IBufferElement parentElement = ownerCollection as IBufferElement;
            isUnderArray = (parentElement != null) && ((parentElement is IArrayBase) || (parentElement.IsInArray));
            List<int> result = (isUnderArray && parentElement is IArrayElement) ?
                            new List<int>((parentElement as IArrayElement).GetArrayElementIndexes()) :
                            new List<int>();
            return result;
        }

        private static void CreateGroupArrayOccurrances(string givenInitName,
            string groupName,
            int numberOfOccurrances,
            IEnumerable<int> parentIndexes,
            IGroupArray array,
            IGroup group,
            IEditableArrayElementAccessor<IGroup> accessor,
            Action<IArrayElementInitializer, string, int> arrayElementInit)
        {
            for (int i = 1; i < numberOfOccurrances; i++)
            {
                var indexes = new List<int>(parentIndexes);
                indexes.Add(i);
                var copy = group.Duplicate(groupName, group.LengthInBuffer * i, indexes);
                if (copy is IArrayElementInitializer)
                {
                    IArrayElementInitializer init = copy as IArrayElementInitializer;
                    init.ArrayElementIndex = i;
                    if (arrayElementInit != null)
                    {
                        arrayElementInit(init, givenInitName, i);
                    }
                }

                array.AddChildElement(copy);
            }
        }

        private static void CreateFieldArrayOccurrances(string fieldName,
            int numberOfOccurrances,
            IEnumerable<int> parentIndexes,
            IFieldArray array,
            IField field,
            IEditableArrayElementAccessor<IField> accessor)
        {
            for (int i = 1; i < numberOfOccurrances; i++)
            {
                var idx = new List<int>(parentIndexes);
                idx.Add(i);
                var copy = field.Duplicate(fieldName, field.LengthInBuffer * i, idx);
                if (copy is IArrayElementInitializer)
                {
                    IArrayElementInitializer init = copy as IArrayElementInitializer;
                    init.ArrayElementIndex = i;
                }

                array.AddChildElement(copy);
            }
        }

        private static IEditableArrayElementAccessor<T> CreateAccessor<T>(string arrayName,
            IDictionary<string, IArrayElementAccessorBase> accessors, T element)
            where T : IArrayElement, IBufferElement
        {
            IEditableArrayElementAccessor<T> result = ObjectFactory.Factory.NewArrayElementAccessorObject<T>(arrayName);
            result.AddElement(element);
            accessors.Add(arrayName, result.AsReadOnly());
            return result;
        }

        private static IEditableArrayElementAccessor<T> CreateArrayElementAccessor<T>(string arrayName,
            IDictionary<string, IArrayElementAccessorBase> accessors, IArray<T> array, T element)
            where T : IArrayElement, IBufferElement
        {
            IEditableArrayElementAccessor<T> result = CreateAccessor<T>(arrayName, accessors, element);
            array.AddChildElement(element);
            return result;
        }

        private void AssignArrayDetails<T>(IDictionary<string, IArrayElementAccessorBase> accessors,
            IArrayElementInitializer<T> element)
            where T : IArrayElement, IBufferElement
        {
            // If we're in an array, then this new element is being created within the group definition of of the 
            // first group of a GroupArray (otherwise we'd be creating this element via Duplicate()); 
            // thus we'll need to correct the name of the element.
            string elementName = element.Name;
            List<int> parentIdxs = new List<int>((ownerCollection as IArrayElement).GetArrayElementIndexes());
            element.Name = ArrayElementUtils.MakeElementName(elementName, parentIdxs);
            element.ArrayElementIndex = 0;

            var accessor = CreateAccessor<T>(elementName, accessors, element.AsReadOnly());
            element.ArrayElementAccessor = accessor.AsReadOnly();
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the StructureDefinitionCompositor class.
        /// </summary>
        /// <param name="collection">A reference to the collaction, which should be set a the owner collection for the current compositor.</param>
        public StructureDefinitionCompositor(IElementCollection collection)
        {
            ownerCollection = collection;
            IsDefining = true;
        }

        /// <summary>
        /// Hides the parameter-less constructor so that we can't have a compositor without an ownerCollection.
        /// </summary>
        private StructureDefinitionCompositor()
        {

        }
        #endregion

        #region internal properties
        /// <summary>
        /// The buffer object that will be assigned to new buffer structure elements as they are created.
        /// </summary>
        [Category("Structure definition")]
        [Description("Pass-thru buffer object")]
        [ReadOnly(true)]
        internal IDataBuffer Buffer { get; set; }

        /// <summary>
        /// Gets whether this object is currently in Defining mode.
        /// </summary>
        [Category("Structure definition")]
        [Description("Is the object in Defining mode")]
        internal bool IsDefining { get; set; }
        #endregion

        #region internal methods
        /// <summary>
        /// Returns a new IField-implementing object.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of data the new field contains.</param>
        /// <param name="fieldDisplayLength">Number of bytes required to display the value of the new field. Effectively
        /// the same as the length in the buffer unless the value is of COMP int or PackedDecimal type.</param>
        /// <param name="initialValue">Default, or initial value of the new field.</param>
        /// <param name="positionInParent">Byte index of the new field in its parent collection.</param>
        /// <param name="accessors">Collection of array element accessors owned by the root record.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal. Defaults to 0.</param>
        /// <param name="isFiller">Indicates whether the new field is marked as <c>FILLER</c>. Defaults to <c>false</c>.</param>
        /// <returns>A new IField-implementer.</returns>
        internal IField CreateNewField(string name, FieldType fieldType, int fieldDisplayLength,
            object initialValue, int positionInParent, IDictionary<string, IArrayElementAccessorBase> accessors,
            int decimalDigits = 0, bool isFiller = false)
        {
            CheckDefining();

            object value = initialValue;

            if ((fieldType == FieldType.PackedDecimal) && !(initialValue is PackedDecimal))
            {
                value = PackedDecimal.From(initialValue, decimalDigits, true);
            }

            if (fieldType == FieldType.UnsignedPackedDecimal)
            {
                PackedDecimal tempPD = (initialValue is PackedDecimal) ? (PackedDecimal)initialValue : PackedDecimal.From(initialValue, decimalDigits, false);
                value = tempPD.AbsoluteValue();
            }

            int digits = decimalDigits;
            IFieldInitializer result = NewField(name, fieldType, fieldDisplayLength, positionInParent, isFiller, digits, accessors);
            result.DefineTimeAccessors = accessors;

            if (IsInArray)
            {
                AssignArrayDetails(accessors, result);
            }

            if (fieldType == FieldType.String && value is Int32 && ((Int32)value).Equals(0)) {
              value = "0".PadLeft(fieldDisplayLength, '0');
            }

            result.InitialValue = value;
            result.Assign(value);
            return result.AsReadOnly();
        }


        /// <summary>
        /// Returns a new IFieldArray-implementing object.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType"></param>
        /// <param name="fieldDisplayLength"></param>
        /// <param name="positionInParent">Byte index of the new array in its parent collection.</param>
        /// <param name="accessors">Collection of array element accessors owned by the root record.</param>
        /// <param name="decimalDigits"></param>
        /// <param name="initialFieldValue"></param>
        /// <param name="isFiller">Indicates whether the field occurrances should be marked as FILLER.</param>
        internal IFieldArray CreateNewFieldArray(string arrayName, int numberOfOccurrances,
            FieldType fieldType, string editMask, int fieldDisplayLength, int positionInParent,
            IDictionary<string, IArrayElementAccessorBase> accessors, int decimalDigits,
            object initialFieldValue, bool isFiller = false)
        {
            if (String.IsNullOrEmpty(arrayName))
                throw new ArgumentException("arrayName is null or empty.", "arrayName");

            CheckDefining();

            bool isUnderArray;
            var parentIndexes = GetParentIndexes(out isUnderArray);
            string name = ArrayElementUtils.MakeElementName(arrayName, parentIndexes);

            IFieldArrayInitializer result = ObjectFactory.Factory.NewFieldArrayObject(name, ownerCollection, Buffer, positionInParent, numberOfOccurrances, 0, isUnderArray, isFiller);
            IFieldArray resultArray = result.AsReadOnly();

            IField firstField = resultArray.CreateFirstArrayElement(arrayName, fieldType, fieldDisplayLength, decimalDigits, initialFieldValue);
            firstField.EditMask = editMask;
            result.ArrayElementLength = firstField.LengthInBuffer;

            var accessor = CreateArrayElementAccessor(arrayName, accessors, resultArray, firstField);
            (firstField as IFieldInitializer).ArrayElementAccessor = accessor.AsReadOnly();

            CreateFieldArrayOccurrances(arrayName, numberOfOccurrances, parentIndexes, resultArray, firstField, accessor);
            resultArray.EndDefinition();

            return result.AsReadOnly();
        }

        /// <summary>
        /// Returns a new IGroupArray-implementing object.
        /// </summary>
        /// <param name="arrayName">Name of the new IGroupArray.</param>
        /// <param name="numberOfOccurrances">The number of copies of the first group element, as defined by <paramref name="groupDefinition"/>.</param>
        /// <param name="groupInit">Optional initialization action for first group element.</param>
        /// <param name="groupDefinition">Action which defines the group structure.</param>
        /// <param name="arrayElementInit"></param>
        /// <param name="arrayFinal"></param>
        /// <param name="positionInParent"></param>
        /// <param name="accessors">The list of IArrayElementAccessors maintained by the parent record. This is passed
        /// down through all element instantiations so that new accessors can be added at the time that their first 
        /// elements are created. Record needs this list later.</param>
        /// <returns>A newly created IGroupArray-implementing object, completely populated with all its child
        /// array element objects.</returns>
        internal IGroupArray CreateNewGroupArray(string arrayName, int numberOfOccurrances,
            Action<IGroupInitializer> groupInit, Action<IStructureDefinition> groupDefinition,
            Action<IArrayElementInitializer, string, int> arrayElementInit, Action<IArrayFinalizer<IGroup>> arrayFinal,
            int positionInParent, IDictionary<string, IArrayElementAccessorBase> accessors)
        {
            CheckDefining();

            bool isUnderArray;
            var parentIndexes = GetParentIndexes(out isUnderArray);
            string name = ArrayElementUtils.MakeElementName(arrayName, parentIndexes);

            IGroupArrayInitializer result = ObjectFactory.Factory.NewGroupArrayObject(name, ownerCollection,
                            Buffer, positionInParent, isUnderArray, numberOfOccurrances);
            IGroupArray resultArray = result.AsReadOnly();

            // this makes sure we're passing the record's list of accessors all the way down through the tree so that 
            // we can add IArrayElementAccessors to it when necessary. 
            IGroup firstGroup = resultArray.CreateFirstArrayElement(arrayName, groupInit, groupDefinition, arrayElementInit, accessors);
            result.ArrayElementLength = firstGroup.LengthInBuffer;

            var accessor = CreateArrayElementAccessor(arrayName, accessors, resultArray, firstGroup);
            (firstGroup as IGroupInitializer).ArrayElementAccessor = accessor.AsReadOnly();

            // they might have changed the group name in groupInit().
            string temp = ArrayElementUtils.GetElementBaseName(firstGroup.Name);
            string groupName = !String.IsNullOrEmpty(temp) ? temp : name;

            CreateGroupArrayOccurrances(name, groupName, numberOfOccurrances, parentIndexes, resultArray, firstGroup, accessor, arrayElementInit);

            if (arrayFinal != null)
            {
                arrayFinal(result as IArrayFinalizer<IGroup>);
            }

            resultArray.EndDefinition();
            return result.AsReadOnly();
        }



        /// <summary>
        /// Creates a new REDEF-specific instance of IField.
        /// </summary>
        /// <remarks>
        /// <para>There are only two valid cases for creation of field redefines:</para>
        /// <para>1 - the redefine field is a child of the record or of a non-redefine group.</para>
        /// <para>In this case, the redefine field must redefine the entirety of <paramref name="elementToRedefine"/>. 
        /// <paramref name="fieldDisplayLength"/> must be the same as <paramref name="elementToRedefine"/>'s, and 
        /// <paramref name="positionInParent"/> and the new field's PositionInBuffer must be the same as 
        /// <paramref name="elementToRedefine"/>'s.</para>
        /// <para>2 - the redefine field is a child of a redefine group.</para>
        /// <para>In this case, <paramref name="elementToRedefine"/> should be <c>null</c> since a parent-redefine will 
        /// be pointing to an element to redefine, while this new field redefines only a portion. <paramref name="positionInParent"/>
        /// will be calculated and passed in as with a normal field, and the new field's PositionInBuffer will be 
        /// calculated based on the position of the parent redefine's elementToRedefine, plus <paramref name="positionInParent"/>.
        /// </para>
        /// </remarks>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Data type of the new field.</param>
        /// <param name="elementToRedefine">The buffer element which this new field will redefine.</param>
        /// <param name="fieldDisplayLength">Number of bytes in the buffer the new field will occupy.</param>
        /// <param name="positionInParent">Byte offset within the new field's parent.</param>
        /// <returns>A new IField-implementing object.</returns>
        /// <param name="accessors"></param>
        internal IField CreateNewFieldRedefine(string name, FieldType fieldType,
            IBufferElement elementToRedefine, int fieldDisplayLength, int fieldDecimalDigits,
            int positionInParent, IDictionary<string, IArrayElementAccessorBase> accessors, bool isFiller = false)
        {
            if (elementToRedefine != null && (positionInParent != elementToRedefine.PositionInParent))
                throw new ArgumentException("positionInParent and elementToRedefine.PositionInParent must match.", "positionInParent");

            CheckDefining();

            IFieldInitializer result = NewRedefineField(name, fieldType, elementToRedefine, fieldDisplayLength, fieldDecimalDigits, positionInParent, isFiller, accessors);

            // If we're in an array, then this new element is being created within the group definition of of the 
            // first group of a GroupArray (otherwise we'd be creating this element via Duplicate()); 
            // thus we'll need to correct the name of the element.
            if (IsInArray)
            {
                string elementName = result.Name;
                List<int> parentIdxs = new List<int>((ownerCollection as IArrayElement).GetArrayElementIndexes());
                result.Name = ArrayElementUtils.MakeElementName(elementName, parentIdxs);
                result.ArrayElementIndex = 0;

                IEditableArrayElementAccessor<IField> accessor = ObjectFactory.Factory.NewArrayElementAccessorObject<IField>(elementName);
                accessor.AddElement(result.AsReadOnly());

                IArrayElementAccessor<IField> readonlyAccessor = accessor.AsReadOnly();
                accessors.Add(elementName, readonlyAccessor);
                result.ArrayElementAccessor = readonlyAccessor;
            }

            return result.AsReadOnly();
        }


        /// <summary>
        /// Returns a new IGroup-Implementing object.
        /// </summary>
        /// <param name="name">Name of the new group object.</param>
        /// <param name="definition">Action which defines the group structure.</param>
        /// <param name="positionInParent"></param>
        /// <returns></returns>
        /// <param name="accessors"></param>
        internal IGroup CreateNewGroup(string name, Action<IStructureDefinition> definition,
            int positionInParent, IDictionary<string, IArrayElementAccessorBase> accessors)
        {
            CheckDefining();

            IGroupInitializer result = NewGroup(name, positionInParent, accessors);

            if (IsInArray)
            {
                AssignArrayDetails(accessors, result);
            }

            IStructureDefinition groupDef = result as IStructureDefinition;
            definition(groupDef);          // <-- defines group structure. 
            groupDef.EndDefinition();

            return result.AsReadOnly();
        }


        /// <summary>
        /// Returns a new IGroup-implementing object for use in Redefines.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elementToRedefine"></param>
        /// <param name="definition"></param>
        /// <param name="positionInParent"></param>
        /// <returns></returns>
        /// <param name="accessors"></param>
        internal IGroup CreateNewGroupRedefine(string name,
            IBufferElement elementToRedefine,
            Action<IStructureDefinition> definition,
            int positionInParent, IDictionary<string, IArrayElementAccessorBase> accessors)
        {
            if (elementToRedefine != null && (positionInParent != elementToRedefine.PositionInParent))
                throw new ArgumentException("positionInParent and elementToRedefine.PositionInParent must match.", "positionInParent");

            CheckDefining();

            var result = ObjectFactory.Factory.NewRedefineGroupObject(name, Buffer, ownerCollection, elementToRedefine, positionInParent, accessors, IsInArray);
            var groupResult = result.AsReadOnly();

            // If we're in an array, then this new element is being created within the group definition of of the 
            // first group of a GroupArray (otherwise we'd be creating this element via Duplicate()); 
            // thus we'll need to correct the name of the element.
            if (IsInArray)
            {
                string elementName = result.Name;

                List<int> parentIdxs = new List<int>((ownerCollection as IArrayElement).GetArrayElementIndexes());
                result.Name = ArrayElementUtils.MakeElementName(elementName, parentIdxs);

                IEditableArrayElementAccessor<IGroup> accessor = ObjectFactory.Factory.NewArrayElementAccessorObject<IGroup>(elementName);
                accessor.AddElement(groupResult);

                IArrayElementAccessor<IGroup> readonlyAccessor = accessor.AsReadOnly();
                accessors.Add(elementName, readonlyAccessor);
                result.ArrayElementAccessor = readonlyAccessor;
            }

            definition(result as IStructureDefinition);
            (result as IStructureDefinition).EndDefinition();
            return groupResult;
        }

        /// <summary>
        /// Informs this object that it is done with Defining mode.
        /// </summary>
        internal void EndDefinition()
        {
            if (IsDefining)
            {
                IsDefining = false;
            }
        }

        public void RestartDefinition()
        {
            IsDefining = true;
        }

        #endregion
    }



}