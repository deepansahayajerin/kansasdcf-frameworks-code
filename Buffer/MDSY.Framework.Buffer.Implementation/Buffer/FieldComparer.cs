using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements a type that compares a field with various types of values.
    /// </summary>
    [Serializable]
    public class FieldComparer : IFieldComparer
    {
        #region public methods
        /// <summary>
        /// Compares the values of two specified IField objects and returns an integer that indicates their relative 
        /// position in the sort order.
        /// </summary>
        /// <param name="x">The first field to compare.</param>
        /// <param name="y">The second field to compare.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the ordered relationship between the two comparands. 
        /// <list type="table">
        /// <listheader><term>Value</term><description>Condition</description></listheader>
        /// <item>
        ///     <term>Less than zero</term>
        ///     <description>The value of <paramref name="x"/> is less than the value of <paramref name="y"/>, 
        ///     or <paramref name="x"/> is <c>null</c> while <paramref name="y"/> is not.</description>
        /// </item>
        /// <item>
        ///     <term>Zero</term>
        ///     <description>The values of <paramref name="x"/> and <paramref name="y"/> are equivalent, 
        ///     or both <paramref name="x"/> and <paramref name="y"/> refer to the same instance,
        ///     or both <paramref name="x"/> and <paramref name="y"/> are <c>null</c>.</description>
        /// </item>
        /// <item>
        ///     <term>Greater than zero</term>
        ///     <description>The value of <paramref name="x"/> is greater than the value of <paramref name="y"/>, 
        ///     or <paramref name="y"/> is <c>null</c> while <paramref name="x"/> is not.</description>
        /// </item>
        /// </list>
        /// </returns>
        public int Compare(IField x, IField y)
        {
            return ComparisonMatrix.Compare(x, y);
        }

        /// <summary>
        /// Compares the value of a specified <paramref name="field"/> object with a string <paramref name="value"/> 
        /// and returns an integer that indicates their relative position in the sort order.
        /// </summary>
        /// <param name="field">The field value to compare.</param>
        /// <param name="value">The value to compare.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the ordered relationship between the two comparands. 
        /// <list type="table">
        /// <listheader><term>Value</term><description>Condition</description></listheader>
        /// <item>
        ///     <term>Less than zero</term>
        ///     <description>The value of <paramref name="field"/> is less than the value of <paramref name="value"/>, 
        ///     or <paramref name="field"/> is <c>null</c> while <paramref name="value"/> is not.</description>
        /// </item>
        /// <item>
        ///     <term>Zero</term>
        ///     <description>The values of <paramref name="field"/> and <paramref name="value"/> are equivalent, 
        ///     or <paramref name="field"/> is <c>null</c> while <paramref name="value"/> is null or empty.</description>
        /// </item>
        /// <item>
        ///     <term>Greater than zero</term>
        ///     <description>The value of <paramref name="field"/> is greater than the value of <paramref name="value"/>, 
        ///     or <paramref name="value"/> is <c>null</c> or empty while <paramref name="field"/> is not <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </returns>
        public int Compare(IField field, string value)
        {
            if (field == null)
                throw new ArgumentNullException("field", "field is null.");

            // we flip the result's sign because we swapped the order of the comparands...
            return ComparisonMatrix.CompareFieldValuesAsString(value, field) * -1;
        }

        /// <summary>
        /// Compares the value of a specified <paramref name="field"/> object with a bool <paramref name="value"/> 
        /// and returns an integer that indicates their relative position in the sort order.
        /// </summary>
        /// <param name="field">The field value to compare.</param>
        /// <param name="value">The value to compare.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the ordered relationship between the two comparands. 
        /// <list type="table">
        /// <listheader><term>Value</term><description>Condition</description></listheader>
        /// <item>
        ///     <term>Less than zero</term>
        ///     <description>The value of <paramref name="field"/> is less than the value of <paramref name="value"/>, 
        ///     or <paramref name="field"/> is <c>null</c> while <paramref name="value"/> is not.</description>
        /// </item>
        /// <item>
        ///     <term>Zero</term>
        ///     <description>The values of <paramref name="field"/> and <paramref name="value"/> are equivalent.</description>
        /// </item>
        /// <item>
        ///     <term>Greater than zero</term>
        ///     <description>The value of <paramref name="field"/> is greater than the value of <paramref name="value"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        public int Compare(IField field, bool value)
        {
            if (field == null)
                throw new ArgumentNullException("field", "field is null.");

            // we flip the result's sign because we swapped the order of the comparands...
            return ComparisonMatrix.CompareFieldValuesAsBool(value, field) * -1;
        }


        /// <summary>
        /// Compares the value of a specified <paramref name="field"/> object with a PackedDecimal <paramref name="value"/> 
        /// and returns an integer that indicates their relative position in the sort order.
        /// </summary>
        /// <param name="field">The field value to compare.</param>
        /// <param name="value">The value to compare.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the ordered relationship between the two comparands. 
        /// <list type="table">
        /// <listheader><term>Value</term><description>Condition</description></listheader>
        /// <item>
        ///     <term>Less than zero</term>
        ///     <description>The value of <paramref name="field"/> is less than the value of <paramref name="value"/>, 
        ///     or <paramref name="field"/> is <c>null</c> while <paramref name="value"/> is not.</description>
        /// </item>
        /// <item>
        ///     <term>Zero</term>
        ///     <description>The values of <paramref name="field"/> and <paramref name="value"/> are equivalent.</description>
        /// </item>
        /// <item>
        ///     <term>Greater than zero</term>
        ///     <description>The value of <paramref name="field"/> is greater than the value of <paramref name="value"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        public int Compare(IField field, PackedDecimal value)
        {
            if (field == null)
                throw new ArgumentNullException("field", "field is null.");
            if (field.IsNumericType && !field.IsNumericValue())
            {
                return -1;
            }

            // we flip the result's sign because we swapped the order of the comparands...
            return ComparisonMatrix.CompareFieldValuesAsPackedDecimal(value, field) * -1;
        }

        /// <summary>
        /// Compares the value of a specified <paramref name="field"/> object with a Decimal <paramref name="value"/> 
        /// and returns an integer that indicates their relative position in the sort order.
        /// </summary>
        /// <param name="field">The field value to compare.</param>
        /// <param name="value">The value to compare.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the ordered relationship between the two comparands. 
        /// <list type="table">
        /// <listheader><term>Value</term><description>Condition</description></listheader>
        /// <item>
        ///     <term>Less than zero</term>
        ///     <description>The value of <paramref name="field"/> is less than the value of <paramref name="value"/>, 
        ///     or <paramref name="field"/> is <c>null</c> while <paramref name="value"/> is not.</description>
        /// </item>
        /// <item>
        ///     <term>Zero</term>
        ///     <description>The values of <paramref name="field"/> and <paramref name="value"/> are equivalent.</description>
        /// </item>
        /// <item>
        ///     <term>Greater than zero</term>
        ///     <description>The value of <paramref name="field"/> is greater than the value of <paramref name="value"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        public int Compare(IField field, Decimal value)
        {
            if (field == null)
                throw new ArgumentNullException("field", "field is null.");
            if (!field.IsNumericValue())  
            {
                // If field is string and contains all spaces or trimmed string not numeric, return -1 else try to compare 
                decimal testDec = 0;
                if (!field.IsNumericType && field.IsNotSpaces() && decimal.TryParse(field.AsString().Trim(), out testDec))
                {
                    return ComparisonMatrix.CompareFieldValuesAsDecimal(value, field) * -1;
                }
                return -1;
            }
            // we flip the result's sign because we swapped the order of the comparands...
            return ComparisonMatrix.CompareFieldValuesAsDecimal(value, field) * -1;
        }

        /// <summary>
        /// Compares the value of a specified <paramref name="field"/> object with an Int64 <paramref name="value"/> 
        /// and returns an integer that indicates their relative position in the sort order.
        /// </summary>
        /// <param name="field">The field value to compare.</param>
        /// <param name="value">The value to compare.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the ordered relationship between the two comparands. 
        /// <list type="table">
        /// <listheader><term>Value</term><description>Condition</description></listheader>
        /// <item>
        ///     <term>Less than zero</term>
        ///     <description>The value of <paramref name="field"/> is less than the value of <paramref name="value"/>, 
        ///     or <paramref name="field"/> is <c>null</c> while <paramref name="value"/> is not.</description>
        /// </item>
        /// <item>
        ///     <term>Zero</term>
        ///     <description>The values of <paramref name="field"/> and <paramref name="value"/> are equivalent.</description>
        /// </item>
        /// <item>
        ///     <term>Greater than zero</term>
        ///     <description>The value of <paramref name="field"/> is greater than the value of <paramref name="value"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        public int Compare(IField field, Int64 value)
        {
            if (field == null)
                throw new ArgumentNullException("field", "field is null.");
            if (field.IsNumericType && !field.IsNumericValue())
            {
                return -1;
            }

            // we flip the result's sign because we swapped the order of the comparands...
            return ComparisonMatrix.CompareFieldValuesAsInt64(value, field) * -1;
        }

        #endregion
    }
}
