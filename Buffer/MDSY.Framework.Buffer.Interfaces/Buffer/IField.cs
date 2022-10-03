using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using MDSY.Framework.Buffer.Unity;
using System.ComponentModel;
using MDSY.Framework.Buffer.Common;
using System.Linq.Expressions;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which represents a single field value in an IRecord or IGroup.
    /// Like a common DB field, IFields have types, data lengths, etc.
    /// </summary>
    [InjectionInterface]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface IField : IBufferElement, IBufferValue, IMultiTypeValue, IArrayElement,

        IEquatable<IField>, IEquatable<IGroup>, IEquatable<IRecord>,
        IEquatable<string>,
        IEquatable<PackedDecimal>, IEquatable<Decimal>,
        IEquatable<bool>, IEquatable<int>,

        IComparable, IComparable<IField>, IComparable<IGroup>, IComparable<IRecord>,
        IComparable<string>, IComparable<PackedDecimal>, IComparable<Decimal>,
        IComparable<bool>, IComparable<int>
    {
        #region attributes

        /// <summary>
        /// Gets any check fields associated with this field.
        /// </summary>
        IEnumerable<ICheckField> CheckFields { get; }

        /// <summary>
        /// Gets the number of digits to the right of the decimal, if this is a numeric field.
        /// </summary>
        int DecimalDigits { get; }

        /// <summary>
        /// Gets the number of bytes for display of the field value.
        /// </summary>
        int DisplayLength { get; }

        /// <summary>
        /// Gets the type of data accessed by this field object.
        /// </summary>
        FieldType FieldType { get; }

        /// <summary>
        /// Gets whether this field object has been declared with a numeric FieldType.
        /// </summary>
        bool IsNumericType { get; }

        //[Obsolete("Use IsNumericValue() extension method, or IsNumericOnlyValue property instead.", true)]
        //bool IsNumericValue { get; }

        /// <summary>
        /// Gets whether this field object currently contains a value that is numeric, regardless of FieldType. 
        /// </summary>
        /// <remarks>
        /// For issues in converted code, an IsNumericValue() extension method was added,
        /// so to avoid confusion, the IsNumericValue property has been renamed IsNumericOnlyValue.
        /// </remarks>
        bool IsNumericOnlyValue { get; }

        /// <summary>
        /// Gets this field's value as a display-appropriate string; i.e. compressed numeric values are shown 
        /// as string representations of their number value rather than their bytes. Likewise, booleans are displayed 
        /// as "true" or "false".
        /// </summary>
        new string DisplayValue { get; }

        /// <summary>
        /// Returns <c>true</c> if the internal value of this object is null or contains an array of zero-length.
        /// </summary>
        bool IsNull { get; }

        /// <summary>
        /// Gets or sets the edit mask.
        /// </summary>
        string EditMask { get; set; }

        /// <summary>
        /// Gets or sets whether this field should be blanked if it's value is zero.
        /// </summary>
        bool IsBlankWhenZero { get; set; }

        void SetIsBlankWhenZero(bool isBlankWhenZero);
        /// <summary>
        /// Represents Database column type field is used for.  
        /// </summary>
        DBColumnType DBColumnType { get; set; }

        #endregion

        #region operations

        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">The string values for which to check the field value.</param>
        /// <returns>The new checkfield object.</returns>
        ICheckField CreateNewCheckField(string name, params string[] values);
        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">The int values for which to check the field value.</param>
        /// <returns>The new checkfield object.</returns>
        ICheckField CreateNewCheckField(string name, params int[] values);
        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// uses the given <paramref name="check"/> expression to evaluated this field.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="check">The expression which the checkfield will use to evaluate this field.</param>
        /// <returns>The new checkfield object.</returns>
        ICheckField CreateNewCheckField(string name, Func<IField, bool> check);

        /// <summary>
        /// Creates a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">The values(string) for which to check the field value.</param>
        /// <returns>This field object.</returns>
        IField NewCheckField(string name, params string[] values);
        /// <summary>
        /// Creates a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">The values (int) for which to check the field value.</param>
        /// <returns>This field object.</returns>
        IField NewCheckField(string name, params int[] values);

        /// <summary>
        /// Creates a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">The values (Char) for which to check the field value.</param>
        /// <returns>This field object.</returns>
        IField NewCheckField(string name, params Char[] values);

        /// <summary>
        /// Creates a new check field range object associated with this field object. The new check field 
        /// evaluates this field's range for any of the given <paramref name="loBound"/> and <paramref name="hiBound"/> values.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="loBound">The lower bound string value of the field range.</param>
        /// <param name="hiBound">The higher bound string value of the field range.</param>
        /// <returns>This field object.</returns>
        IField NewCheckFieldRange(string name, string loBound, string hiBound);

        IField NewCheckFieldRange(string name, string loBound1, string hiBound1, string loBound2, string hiBound2);

        /// <summary>
        /// Creates a new check field range object associated with this field object. The new check field 
        /// evaluates this field's range for any of the given <paramref name="loBound"/> and <paramref name="hiBound"/> values.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="loBound">The lower bound int value of the field range.</param>
        /// <param name="hiBound">The higher bound int value of the field range.</param>
        /// <returns></returns>
        IField NewCheckFieldRange(string name, int loBound, int hiBound);

        IField NewCheckFieldRange(string name, int loBound1, int hiBound1, int loBound2, int hiBound2);

        IField NewCheckFieldRange(string name, int loBound1, int hiBound1, int loBound2, int hiBound2, int loBound3, int hiBound3);


        /// <summary>
        /// Creates a new check field object associated with this field object. The new check field 
        /// uses the given <paramref name="check"/> expression to evaluated this field.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="check">The expression which the checkfield will use to evaluate this field.</param>
        /// <returns>This field object.</returns>
        IField NewCheckField(string name, Func<IField, bool> check);

        /// <summary>
        /// Functions as COBOL statement <c>SET P TO ADDRESS OF B</c>. 
        /// Causes the field object to set its value to the "address" of the 
        /// buffer of the given <paramref name="element"/>.
        /// </summary>
        /// <remarks><para>In context of the method parameters, the COBOL would 
        /// read as <c>SET thisField TO ADDRESS OF <paramref name="element"/></c>
        /// </para></remarks>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="element">The IField whose buffer address will be stored in this field.</param>
        void SetValueToAddressOf<T>(T element) where T : IBufferElement, IBufferValue;

        /// <summary>
        /// Functions as COBOL statement <c>SET ADDRESS OF C TO ADDRESS OF B</c>.
        /// Causes the field object to point its buffer reference to the 
        /// buffer of the given <paramref name="element"/>.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="element">The IField whose buffer address will be stored in this field.</param>
        void SetAddressToAddressOf<T>(T element) where T : IBufferElement, IBufferValue;

        /// <summary>
        /// Functions as COBOL statement <c>SET ADDRESS OF C TO P</c>.
        /// Causes the field object to point its buffer to the buffer specified
        /// by the address stored in <paramref name="addressField"/>.
        /// <paramref name="addressField"/>.
        /// </summary>
        /// <param name="addressField">Specifies the address to be pointed to.</param>
        void SetAddressFromValueOf(IField addressField);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        void GetAcceptData(string text);

        #endregion

    }
}
