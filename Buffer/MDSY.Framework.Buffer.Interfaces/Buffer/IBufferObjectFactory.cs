using MDSY.Framework.Buffer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which functions as an object factory for buffer elements.
    /// </summary>
    public interface IBufferObjectFactory
    {
        /// <summary>
        /// Returns a new, populated IBufferAddress object.
        /// </summary>
        /// <returns></returns>
        /// <param name="recordKey">The key of the record, in BufferServices.Records, whose Buffer is the new target.</param>
        /// <param name="elementName">The name of the element, in the record, whose PositionInBuffer is the new target.</param>
        IBufferAddress NewBufferAddress(int recordKey, string elementName);

        /// <summary>
        /// Returns a new, populated IBufferAddress object.
        /// </summary>
        /// <returns></returns>
        /// <param name="recordKey">The key of the record, in BufferServices.Records, whose Buffer is the new target.</param>
        /// <param name="elementName">The name of the element, in the record, whose PositionInBuffer is the new target.</param>
        /// <param name="optionalBufferStartIndex">Optional different PositionInBuffer, if <paramref name="elementName"/> is empty.</param>
        IBufferAddress NewBufferAddress(int recordKey, string elementName, int optionalBufferStartIndex);

        /// <summary>
        /// Returns a newly-instatiated FieldArray object created with the given values.
        /// </summary>
        /// <param name="name">Name of the new FieldArray object.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new object.</param>
        /// <param name="buffer">Buffer object for the new object.</param>
        /// <param name="positionInParent">Index position of the new object in its parent.</param>
        /// <param name="numberOfOccurrances">Number of array elements.</param>
        /// <param name="arrayElementLength">Length of each array element.</param>
        /// <param name="isSubArray">Indicates whether the new array object is itself within an array (i.e. nested).</param>
        /// <param name="isFiller">Indicates whether the new object is marked as <c>FILLER</c>.</param>
        /// <returns>A new FieldArray object.</returns>
        IFieldArrayInitializer NewFieldArrayObject(string name, IElementCollection parentCollection, IDataBuffer buffer, int positionInParent, int numberOfOccurrances, int arrayElementLength, bool isSubArray, bool isFiller);

        /// <summary>
        /// Returns a new DataBufferByteArray object built using the given <paramref name="bytes"/>.
        /// </summary>
        /// <param name="bytes">The bytes for the new byte array.</param>
        /// <returns>An IDataBuffer-implementing DataBufferByteArray object.</returns>
        IDataBuffer NewDataBufferByteArrayObject(IEnumerable<byte> bytes);

        IDataBuffer NewDataBufferRedefinePipelineObject(IBufferElement targetElement);

        /// <summary>
        /// Returns a newly instantiated ICheckField object with the given values.
        /// </summary>
        /// <param name="name">Name of the new check field.</param>
        /// <param name="check">Expression used to evaluate the value of the check field.</param>
        /// <returns>A new ICheckField implementer.</returns>
        ICheckFieldInitializer NewCheckFieldObject(string name, Func<IField, bool> check);


        /// <summary>
        /// Returns a newly instantiated ICheckField object with the given values.
        /// </summary>
        /// <param name="name">Name of the new check field.</param>
        /// <param name="check">Expression used to evaluate the value of the check field.</param>
        /// <param name="field">IField object that is processed by <paramref name="check"/>.</param>
        /// <returns>A new ICheckField implementer.</returns>
        ICheckFieldInitializer NewCheckFieldObject(string name, Func<IField, bool> check, IField field);

        /// <summary>
        /// Returns a newly instantiated IEditableArrayElementAccessor(of <typeparamref name="T"/>).
        /// </summary>
        /// <typeparam name="T">The type of the array elements.</typeparam>
        /// <param name="name">Name of the new object.</param>
        /// <returns>A new IEditableArrayElementAccessor(of <typeparamref name="T"/>).</returns>
        IEditableArrayElementAccessor<T> NewArrayElementAccessorObject<T>(string name) where T : IArrayElement, IBufferElement;

        /// <summary>
        /// Returns a newly instantiated GroupArray object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new group array object.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new object.</param>
        /// <param name="buffer">Buffer object for the new object.</param>
        /// <param name="positionInParent">Index position of the new object in its parent.</param>
        /// <param name="isSubArray">Indicates whether the new array object is itself within an array (i.e. nested).</param>
        /// <param name="numberOfOccurrances">Number of array elements.</param>
        /// <param name="arrayElementLength">Length of each array element.</param>
        /// <returns>A new IGroupArray object.</returns>
        IGroupArrayInitializer NewGroupArrayObject(string name, IElementCollection parentCollection, IDataBuffer buffer, int positionInParent, bool isSubArray, int numberOfOccurrances, int arrayElementLength);

        /// <summary>
        /// Returns a newly instantiated GroupArray object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new group array object.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new object.</param>
        /// <param name="buffer">Buffer object for the new object.</param>
        /// <param name="positionInParent">Index position of the new object in its parent.</param>
        /// <param name="isSubArray">Indicates whether the new array object is itself within an array (i.e. nested).</param>
        /// <param name="numberOfOccurrances">Number of array elements.</param>
        /// <returns>A new GroupArray object.</returns>
        IGroupArrayInitializer NewGroupArrayObject(string name, IElementCollection parentCollection, IDataBuffer buffer, int positionInParent, bool isSubArray, int numberOfOccurrances);

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer);

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="isRedefine">Indicates whether the new field participates in a REDEFINE.</param>
        /// <returns>An appropriate IField implementer.</returns>
        IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer, bool isRedefine);

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer, FieldType fieldType);

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer, FieldType fieldType, int lengthInBuffer);

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer, FieldType fieldType, int lengthInBuffer, int displayLength);

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer, FieldType fieldType, int lengthInBuffer, int displayLength, int positionInParent);

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer, FieldType fieldType, int lengthInBuffer, int displayLength, int positionInParent, int decimalDigits);

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer, FieldType fieldType, int lengthInBuffer, int displayLength, int positionInParent, int decimalDigits, bool isInArray);

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <param name="arrayElementIndex">The index of the new field in its array parent.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer, FieldType fieldType, int lengthInBuffer, int displayLength, int positionInParent, int decimalDigits, bool isInArray, int arrayElementIndex);

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <param name="arrayElementIndex">The index of the new field in its array parent.</param>
        /// <param name="isRedefine">Indicates whether the new field participates in a REDEFINE.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer, FieldType fieldType, int lengthInBuffer, int displayLength, int positionInParent, int decimalDigits, bool isInArray, int arrayElementIndex, bool isRedefine);

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <param name="arrayElementIndex">The index of the new field in its array parent.</param>
        /// <param name="isRedefine">Indicates whether the new field participates in a REDEFINE.</param>
        /// <param name="isFiller">Indicates whether the new field is declared FILLER.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer, FieldType fieldType, int lengthInBuffer, int displayLength, int positionInParent, int decimalDigits, bool isInArray, int arrayElementIndex, bool isRedefine, bool isFiller);

        /// <summary>
        /// Returns a newly instantiated IGroup-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new group.</param>
        /// <param name="parentCollection">>Parent IElementCollection of the new group.</param>
        /// <param name="buffer">Buffer object for the new group.</param>
        /// <param name="positionInParent">Index position of the new group in its parent.</param>
        /// <param name="accessors">Array Element Accessors.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <param name="isInRedefine">Indicates whether the new field participates in a REDEFINE.</param>
        /// <returns></returns>
        IGroupInitializer NewGroupObject(string name, IElementCollection parentCollection, IDataBuffer buffer, int positionInParent, IDictionary<string, IArrayElementAccessorBase> accessors, bool isInArray, bool isInRedefine);

        /// <summary>
        /// Returns a newly instantiated IGroup-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new group.</param>
        /// <param name="buffer">Buffer object for the new group.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new group.</param>
        /// <param name="elementToRedefine">Element to redefine.</param>
        /// <param name="positionInParent">Index position of the new group in its parent.</param>
        /// <param name="arrayElementAccessors">Array Element Accessors.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <returns></returns>
        IGroupInitializer NewRedefineGroupObject(string name, IDataBuffer buffer, IElementCollection parentCollection, IBufferElement elementToRedefine, int positionInParent, IDictionary<string, IArrayElementAccessorBase> arrayElementAccessors, bool isInArray);

        /// <summary>
        /// Returns a new field object built from the given info.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="buffer">Buffer for the new field.</param>
        /// <param name="parentCollection">Parent element collection of the new field.</param>
        /// <param name="fieldType">Data type of the new field.</param>
        /// <param name="elementToRedefine"></param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <param name="arrayElementIndex">The index of the new field in its array parent.</param>
        /// <param name="isFiller">Indicates whether the new field is declared FILLER.</param>
        /// <returns>A new RedefineField.</returns>
        IFieldInitializer NewRedefineFieldObject(string name, IDataBuffer buffer, IElementCollection parentCollection, FieldType fieldType, IBufferElement elementToRedefine, int lengthInBuffer, int displayLength, int decimalDigits, int positionInParent, bool isInArray, int arrayElementIndex, bool isFiller);

        /// <summary>
        /// Creates and returns a new IRecord-implementation using the given <paramref name="structureDefinition"/> delegate.
        /// </summary>
        /// <param name="name">Name of the new record.</param>
        /// <param name="structureDefinition">The structure definition logic to be performed on the new record.</param>
        /// <returns>A record defined by <paramref name="structureDefinition"/>.</returns>
        IRecord NewRecordObject(string name, Action<IStructureDefinition> structureDefinition);
    }
}
