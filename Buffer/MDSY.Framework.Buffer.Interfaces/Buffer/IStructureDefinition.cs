using MDSY.Framework.Buffer.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which can create and add elements to its data structure definition.
    /// </summary>
    public interface IStructureDefinition : IDefineable
    {
        #region attributes
        /// <summary>
        /// Gets the working list of IArrayElementAccessors during "definition-time". 
        /// Note: these are used internally by the data structure building logic and should not be 
        /// accessed by client code. 
        /// After definition-time, this property will be <c>null</c>.
        /// </summary>
        IDictionary<string, IArrayElementAccessorBase> DefineTimeAccessors { get; set; }
        #endregion

        #region structure element creation operations
        /// <summary>
        /// Creates and returns a new IField object. Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        IField CreateNewField(string name, FieldType fieldType, int displayLength);

        /// <summary>
        /// Creates and returns a new IField object. Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldType"></param>
        /// <param name="displayLength"></param>
        /// <param name="dbColumnType"></param>
        /// <returns></returns>
        //IField CreateNewField(string name, FieldType fieldType, int displayLength, DBColumnType dbColumnType);

        /// <summary>
        /// Creates and returns a new IField object whose value is set to <paramref name="initialValue"/>.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Value to be assigned to the new field.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        IField CreateNewField(string name, FieldType fieldType, int displayLength, object initialValue);

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
        IField CreateNewField(string name, FieldType fieldType, int displayLength, object initialValue, int decimalDigits);

        /// <summary>
        /// Creates and returns a new IField object whose numeric value is set to <paramref name="initialValue"/>
        /// and exhibits <paramref name="decimalDigits"/> number of digits to the right of the decimal. 
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldType"></param>
        /// <param name="displayLength"></param>
        /// <param name="initialValue"></param>
        /// <param name="decimalDigits"></param>
        /// <param name="dbColumnType"></param>
        /// <returns></returns>
        IField CreateNewField(string name, FieldType fieldType, int displayLength, object initialValue, int decimalDigits, DBColumnType dbColumnType);

        /// <summary>
        /// Creates and returns a new IField object. Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="editMask">Edit mask text</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        IField CreateNewField(string name, FieldType fieldType, string editMask, int displayLength);

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
        IField CreateNewField(string name, FieldType fieldType, string editMask, int displayLength, object initialValue);

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
        IField CreateNewField(string name, FieldType fieldType, string editMask, int displayLength, object initialValue, int decimalDigits);


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
        /// }
        /// </code>
        /// </example>
        IFieldArray CreateNewFieldArray(string arrayName, int numberOfOccurrances, FieldType fieldType, int fieldDisplayLength);

        /// <summary>
        /// Creates and returns a new array of field objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
        /// <param name="initialFieldValue">The initial object value.</param>
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
        IFieldArray CreateNewFieldArray(string arrayName, int numberOfOccurrances, FieldType fieldType, int fieldDisplayLength, object initialFieldValue);

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
        IFieldArray CreateNewFieldArray(string arrayName, int numberOfOccurrances, FieldType fieldType, int fieldDisplayLength, object initialFieldValue, int decimalDigits);

        /// <summary>
        /// Creates and returns a new array of field objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        ///  <param name="editMask">Display edit mask for the field.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
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
        IFieldArray CreateNewFieldArray(string arrayName, int numberOfOccurrances, FieldType fieldType, string editMask, int fieldDisplayLength);

        /// <summary>
        /// Creates and returns a new array of field objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="editMask">Display edit mask for the field.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
        /// <param name="initialFieldValue">The initial object value.</param>
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
        IFieldArray CreateNewFieldArray(string arrayName, int numberOfOccurrances, FieldType fieldType, string editMask, int fieldDisplayLength, object initialFieldValue);

        /// <summary>
        /// Creates and returns a new array of field objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="editMask">Display edit mask for the field.</param>
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
        IFieldArray CreateNewFieldArray(string arrayName, int numberOfOccurrances, FieldType fieldType, string editMask, int fieldDisplayLength, object initialFieldValue, int decimalDigits);

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        IField CreateNewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength);

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue"> The initial value of the redefines</param>
        /// <returns></returns>
        IField CreateNewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength, object initialValue);

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">The initial value of the redefines.</param>
        /// <param name="decimalDigits">The number of digits to the right of the decimal.</param>
        /// <returns></returns>
        IField CreateNewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength, object initialValue, int decimalDigits);


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
        IField CreateNewFieldRedefine(string name, FieldType fieldType, string editMask, IBufferElement elementToRedefine, int displayLength);

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue"> The initial value of the redefines</param>
        /// <param name="editMask">Edit mask text</param>
        /// <returns></returns>
        IField CreateNewFieldRedefine(string name, FieldType fieldType, string editMask, IBufferElement elementToRedefine, int displayLength, object initialValue);

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">The initial value of the redefines.</param>
        /// <param name="decimalDigits">The number of digits to the right of the decimal.</param>
        /// <param name="editMask">Edit mask text</param>
        /// <returns></returns>
        IField CreateNewFieldRedefine(string name, FieldType fieldType, string editMask, IBufferElement elementToRedefine, int displayLength, object initialValue, int decimalDigits);

        /// <summary>
        /// Creates a new field object marked as FILLER, and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        void CreateNewFillerField(int length);

        /// <summary>
        /// Creates a new field object marked as FILLER, and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <param name="fillWith">The type of value with which to fill the new field.</param>
        void CreateNewFillerField(int length, FillWith fillWith);

        /// <summary>
        /// Creates a new field object marked as FILLER, and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <param name="defaultValue">The value the new field.</param>
        void CreateNewFillerField(int length, string defaultValue);

        /// <summary>
        /// Creates a new field object marked as FILLER. 
        /// </summary>
        void CreateNewFillerField(FieldType fieldType, int displayLength, object initialValue, int decimalLength = 0);

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
        ///{
        ///    IGroup group01 = rec.CreateNewGroup("Group01", (grp) =>
        ///    {
        ///        grp
        ///            .NewField("Field_A", typeof(string), 10, "AAAAAAAAAA")
        ///            .NewField("Field_B", typeof(string), 10, "BBBBBBBBBB");
        ///    });
        /// </code>
        /// </example>
        IGroup CreateNewGroup(string name, Action<IStructureDefinition> definition);

        /// <summary>
        /// Creates and returns a new IGroup object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new group object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="definition">A delegate which contains the logic for defining the structure of the new group.</param>
        /// <returns>A new instance of an IGroup-implementing object.</returns>
        /// <seealso cref="IStructureDefinition.CreateNewGroup"/>
        IGroup CreateNewGroupRedefine(string name, IBufferElement elementToRedefine, Action<IStructureDefinition> definition);

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
        IGroupArray CreateNewGroupArray(string arrayName, int numberOfOccurrances, Action<IStructureDefinition> groupDefinition);

        /// <summary>
        /// Creates and returns a new array of group objects. Adds the new array to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="groupInit">Optional - A delegate which contains the logic for initializing the first group object, which 
        /// will be the pattern for all instances.</param>
        /// <param name="groupDefinition">Non-optional - A delegate which contains the logic for defining the structure of the first 
        /// group object, which will be the pattern for all instances.</param>
        /// <param name="arrayElementInit">Optional - A delegate which contains the logic for modifying each array element as it 
        /// is created. This delegate is called once for each element in the array. Note: the structure of the group 
        /// can not be changed at this point.</param>
        /// <param name="arrayFinal">Optional - A delegate which contains the logic for finalizing the array object. This delegate
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
        /// }
        /// </code>
        /// </example>
        IGroupArray CreateNewGroupArray(string arrayName,
            int numberOfOccurrances,
            Action<IGroupInitializer> groupInit,
            Action<IStructureDefinition> groupDefinition,
            Action<IArrayElementInitializer, string, int> arrayElementInit,
            Action<IArrayFinalizer<IGroup>> arrayFinal);


        #endregion

        #region 'fluent-interface' methods

        /// <summary>
        /// Creates a new, populated IFieldArray object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="fieldType">Data type for each field in the new array.</param>
        /// <param name="fieldDisplayLength">Display length for each field in the new array.</param>
        /// <returns>This IStructureDefinition-implementer.</returns>
        IStructureDefinition NewFieldArray(string arrayName, int numberOfOccurrances, FieldType fieldType, int fieldDisplayLength);

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
        IStructureDefinition NewFieldArray(string arrayName, int numberOfOccurrances, FieldType fieldType, int fieldDisplayLength, object initialFieldValue);

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
        IStructureDefinition NewFieldArray(string arrayName, int numberOfOccurrances, FieldType fieldType, int fieldDisplayLength, object initialFieldValue, int decimalDigits);

        /// <summary>
        /// Creates a new field object marked as FILLER, and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <returns>This IStructureDefinition-implementer.</returns>
        IStructureDefinition NewFillerField(int length);

        /// <summary>
        /// Creates a new field object marked as FILLER, and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <param name="fillWith">The type of value with which to fill the new field.</param>
        /// <returns>This IStructureDefinition-implementer.</returns>
        IStructureDefinition NewFillerField(int length, FillWith fillWith);

        /// <summary>
        /// Creates a new field object marked as FILLER, and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <param name="defaultValue">The value the new field.</param>
        /// <returns>This IStructureDefinition-implementer.</returns>
        IStructureDefinition NewFillerField(int length, string defaultValue);

        /// <summary>
        /// Creates a new field object marked as FILLER. 
        /// </summary>
        /// <returns>This IStructureDefinition-implementer.</returns>
        IStructureDefinition NewFillerField(FieldType fieldType, int displayLength, object initialValue, int decimalLength = 0);

        /// <summary>
        /// Creates a new IGroup object and adds it to this IStructureDefinition object as a child. 
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">Name of the new group.</param>
        /// <param name="definition">A delegate which contains the logic for defining the structure of the new group.</param>
        /// <seealso cref="CreateNewGroup"/>
        /// <returns>This IStructureDefinition-implementer.</returns>
        IStructureDefinition NewGroup(string name, Action<IStructureDefinition> definition);

        /// <summary>
        /// Creates a new group redefine object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">Name of the new group object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="definition">A delegate which contains the logic for defining the structure of the new group.</param>
        /// <seealso cref="CreateNewGroupRedefine" />
        /// <returns>This IStructureDefinition-implementer.</returns>
        IStructureDefinition NewGroupRedefine(string name, IBufferElement elementToRedefine, Action<IStructureDefinition> definition);

        /// <summary>
        /// Creates a new IField object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <seealso cref="CreateNewField(string, FieldType, int)"/>
        /// <returns>This IStructureDefinition-implementer.</returns>
        IStructureDefinition NewField(string name, FieldType fieldType, int displayLength);

        /// <summary>
        /// Creates a new IField object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Value to be assigned to the new field.</param>
        /// <seealso cref="CreateNewField(string, FieldType, int, object)"/>
        /// <returns>This IStructureDefinition-implementer.</returns>
        IStructureDefinition NewField(string name, FieldType fieldType, int displayLength, object initialValue);

        /// <summary>
        /// Creates a new IField object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Value to be assigned to the new field.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal.</param>
        /// <seealso cref="CreateNewField(string, FieldType, int, object, int)"/>        
        /// <returns>This IStructureDefinition-implementer.</returns>
        IStructureDefinition NewField(string name, FieldType fieldType, int displayLength, object initialValue, int decimalDigits);


        /// <summary>
        /// Creates a new field redefine object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>This IStructureDefinition-implementer.</returns>
        /// <seealso cref="CreateNewFieldRedefine(string, FieldType, IBufferElement, int)"/>
        IStructureDefinition NewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength);



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
        IStructureDefinition NewGroupArray(string arrayName, int numberOfOccurrances, Action<IStructureDefinition> groupDefinition);

        /// <summary>
        /// Creates a new, populated IGroupArray object and adds it to this IStructureDefinition object as a child.
        /// Returns this IStructureDefinition object.
        /// </summary>
        /// <param name="arrayName">Name of the new array object.</param>
        /// <param name="numberOfOccurrances">Number of element instances in the new array object.</param>
        /// <param name="groupInit">Optional - A delegate which contains the logic for initializing the first group object, which 
        /// will be the pattern for all instances.</param>
        /// <param name="groupDefinition">Non-optional - A delegate which contains the logic for defining the structure of the first 
        /// group object, which will be the pattern for all instances.</param>
        /// <param name="arrayElementInit">Optional - A delegate which contains the logic for modifying each array element as it 
        /// is created. This delegate is called once for each element in the array. Note: the structure of the group 
        /// can not be changed at this point.</param>
        /// <param name="arrayFinal">Optional - A delegate which contains the logic for finalizing the array object. This delegate
        /// is called once, after array population is complete.</param>
        /// <returns>This IStructureDefinition-implementer.</returns>
        /// <example>
        /// Creates a new record and adds a group array with 10 occurrences of the group which is defined by 
        /// the included <paramref name="groupDefinition"/> delegate.
        /// <code>
        /// IRecord result = BufferServices.Factory.NewRecord("TestRecord", (rec) =>
        /// {
        ///     rec.NewGroupArray("GROUPARRAY01", 10, 
        ///     (grpInit) => { }, // optional delegate; can also pass null.
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
        ///     (elmInit, prfx, idx) => { }, // optional delegate; can also pass null.
        ///     (ary) => { });               // optional delegate; can also pass null.
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="CreateNewGroupArray"/>
        IStructureDefinition NewGroupArray(string arrayName,
            int numberOfOccurrances,
            Action<IGroupInitializer> groupInit,
            Action<IStructureDefinition> groupDefinition,
            Action<IArrayElementInitializer, string, int> arrayElementInit,
            Action<IArrayFinalizer<IGroup>> arrayFinal);
        #endregion


    }
}
