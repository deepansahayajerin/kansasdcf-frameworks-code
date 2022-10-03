using System;
using System.Collections.Generic;
using System.Linq;

using System.ComponentModel;
using MDSY.Framework.Buffer.Common;
using System.Linq.Expressions;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines a type that compares a field with various types of values.
    /// </summary>
    public interface IFieldComparer : IComparer<IField>
    {
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
        /// <term>Less than zero</term>
        /// <description>The value of <paramref name="field"/> is less than the value of <paramref name="value"/>, 
        /// or <paramref name="field"/> is <c>null</c> while <paramref name="value"/> is not.</description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>The values of <paramref name="field"/> and <paramref name="value"/> are equivalent, 
        /// or <paramref name="field"/> is <c>null</c> while <paramref name="value"/> is null or empty.</description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>The value of <paramref name="field"/> is greater than the value of <paramref name="value"/>, 
        /// or <paramref name="value"/> is <c>null</c> or empty while <paramref name="field"/> is not <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </returns>
        int Compare(IField field, string value);
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
        /// <term>Less than zero</term>
        /// <description>The value of <paramref name="field"/> is less than the value of <paramref name="value"/>, 
        /// or <paramref name="field"/> is <c>null</c> while <paramref name="value"/> is not.</description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>The values of <paramref name="field"/> and <paramref name="value"/> are equivalent.</description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>The value of <paramref name="field"/> is greater than the value of <paramref name="value"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        int Compare(IField field, bool value);
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
        /// <term>Less than zero</term>
        /// <description>The value of <paramref name="field"/> is less than the value of <paramref name="value"/>, 
        /// or <paramref name="field"/> is <c>null</c> while <paramref name="value"/> is not.</description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>The values of <paramref name="field"/> and <paramref name="value"/> are equivalent.</description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>The value of <paramref name="field"/> is greater than the value of <paramref name="value"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        int Compare(IField field, PackedDecimal value);
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
        /// <term>Less than zero</term>
        /// <description>The value of <paramref name="field"/> is less than the value of <paramref name="value"/>, 
        /// or <paramref name="field"/> is <c>null</c> while <paramref name="value"/> is not.</description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>The values of <paramref name="field"/> and <paramref name="value"/> are equivalent.</description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>The value of <paramref name="field"/> is greater than the value of <paramref name="value"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        int Compare(IField field, Decimal value);
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
        /// <term>Less than zero</term>
        /// <description>The value of <paramref name="field"/> is less than the value of <paramref name="value"/>, 
        /// or <paramref name="field"/> is <c>null</c> while <paramref name="value"/> is not.</description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>The values of <paramref name="field"/> and <paramref name="value"/> are equivalent.</description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>The value of <paramref name="field"/> is greater than the value of <paramref name="value"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        int Compare(IField field, Int64 value);
    }
}
