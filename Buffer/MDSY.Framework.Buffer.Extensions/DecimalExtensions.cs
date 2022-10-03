using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Common;
using System.Diagnostics;

namespace MDSY.Framework.Buffer
{
    /// <summary>
    /// Extension methods for int
    /// </summary>
    public static class DecimalExtensions
    {

        #region general
        /// <summary>
        /// Returns the value of decimalas an Deimal if possible.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>An integer representation of the current decimal object.</returns>
        public static int AsInt(this decimal instance)
        {
            return (int)instance;
        }

        /// <summary>
        /// Returns the value of the int as a string if possible.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>A string representation of the current decimal object.</returns>
        public static string AsString(this decimal instance)
        {
            return instance.ToString();
        }

        #endregion

        #region simple queries
        /// <summary>
        /// Returns <c>true</c> if the current object's value represents zero. 
        /// </summary>
        public static bool IsZeroes(this decimal instance)
        {
            return instance == 0;
        }

        /// <summary>
        /// Returns <c>true</c> if the current object's value represents zero. 
        /// </summary>
        public static bool IsZeros(this decimal instance)
        {
            return instance.IsZeroes();
        }

        /// <summary>
        /// Returns true if the current decimal object's value is negative.
        /// </summary>
        /// <param name="instance">A reference to the current decimal object.</param>
        /// <returns>Returns true if the current decimal object's value is negative.</returns>
        public static bool IsNegative(this decimal instance)
        {
            return (instance < 0);
        }

        /// <summary>
        /// Returns true if the current decimal object's value is positive.
        /// </summary>
        /// <param name="instance">A reference to the current decimal object.</param>
        /// <returns>Returns true if the current decimal object's value is negative.</returns>
        public static bool IsPositive(this decimal instance)
        {
            return (instance >= 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the field object's value is in range (inclusive) of the <paramref name="loBound"/> and 
        /// <paramref name="hiBound"/> values.
        /// </summary>
        /// <param name="instance">A reference to the current decimal object.</param>
        /// <param name="loBound">Specifies the lower boundary of the range.</param>
        /// <param name="hiBound">Specifies the upper boundary of the range.</param>
        /// <returns>Returns true if the value of the current decimal object lays within the specified range.</returns>
        public static bool IsInRange(this decimal instance, int loBound, int hiBound)
        {
            return (instance >= loBound && instance <= hiBound);
        }


        #endregion

        #region IsEqualTo
        /// <summary>
        /// Returns true if the value of this decimal is equal to the value of the <paramref name="other"/> field.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if both fields are <c>null</c>, if both fields refer to the same object instance, or 
        /// if the two field values are functionally equivalent. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this decimal instance, IField other)
        {
            return (instance == other.AsDecimal());
        }


        /// <summary>
        /// Returns true if the value of this decimal is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this decimal instance, Decimal other)
        {

            return (instance == other);
        }

        /// <summary>
        /// Returns true if the value of this decimal is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this decimal instance, Int64 other)
        {
            return (instance == (decimal)other);
        }

        /// <summary>
        /// Returns true if the value of this decimal is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this decimal instance, int other)
        {

            return (instance == (decimal)other);
        }

        /// <summary>
        /// Returns true if the value of this decimal is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this decimal instance, string other)
        {

            return (instance == int.Parse(other));
        }

        #endregion

        #region IsLessThan

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this decimal instance, IField other)
        {
            return (instance < other.AsDecimal());
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this decimal instance, string other)
        {
            return (instance < decimal.Parse(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this decimal instance, Decimal other)
        {
            return (instance.CompareTo(other) < 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this decimal instance, int other)
        {
            return (instance < (decimal)other);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this decimal instance, Int64 other)
        {
            return (instance.CompareTo(other) < 0);
        }


        #endregion

        #region IsGreaterThan
        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this decimal instance, IField other)
        {
            return (instance > other.AsDecimal());
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this decimal instance, string other)
        {
            return (instance > decimal.Parse(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this decimal instance, Decimal other)
        {
            return (instance.CompareTo(other) > 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this decimal instance, int other)
        {
            return (instance > (decimal)other);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this decimal instance, Int64 other)
        {

            return (instance > (decimal)other);
        }


        #endregion

        #region IsLessThanOrEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this decimal instance, IField other)
        {
            return (instance <= other.AsDecimal());
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this decimal instance, string other)
        {
            return (instance <= decimal.Parse(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this decimal instance, Decimal other)
        {
            return (instance.CompareTo(other) <= 0);
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this decimal instance, Int64 other)
        {
            return (instance <= (decimal)other);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this decimal instance, int other)
        {
            return (instance <= (decimal)other);
        }

        #endregion

        #region IsGreaterThanOrEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this decimal instance, IField other)
        {
            return (instance >= other.AsDecimal());
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this decimal instance, string other)
        {
            return (instance >= decimal.Parse(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this decimal instance, Decimal other)
        {
            return (instance.CompareTo(other) >= 0);
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this decimal instance, Int64 other)
        {
            return (instance >= (decimal)other);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this decimal instance, int other)
        {
            return (instance >= (decimal)other);
        }

        #endregion

        #region IsNotEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is not equal to the value of the <paramref name="other"/> field.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>false</c> if both fields are <c>null</c>, if both fields refer to the same object instance, or 
        /// if the two field values are functionally equivalent. Otherwise, returns <c>true</c>.</returns>
        public static bool IsNotEqualTo(this decimal instance, IField other)
        {
            return !(instance.IsEqualTo(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this decimal instance, string other)
        {
            return !(instance.IsEqualTo(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this decimal instance, Decimal other)
        {
            return !(instance.IsEqualTo(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this decimal instance, Int64 other)
        {
            return !(instance.IsEqualTo(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this decimal is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this decimal instance, int other)
        {
            return !(instance.IsEqualTo(other));
        }
        #endregion

        #region Arithmetic
        /// <summary>
        /// Add an integer to this instance value
        /// </summary>
        /// <param name="instance">The field object that will perform the addition.</param>
        /// <param name="value">Integer value to be added.</param>
        /// <returns>Returns the sum of the provided decimal and integer values.</returns>
        public static decimal Add(this decimal instance, int value)
        {
            instance = instance + (decimal)value;
            return instance;
        }

        /// <summary>
        /// Add a decimal to this instance value
        /// </summary>
        /// <param name="instance">The field object that will perform the addition.</param>
        /// <param name="value">Decimal value to be added.</param>
        /// <returns>Returns the sum of the provided decimal values.</returns>
        public static decimal Add(this decimal instance, decimal value)
        {
            instance = instance + value;
            return instance;
        }
        #endregion

    }
}
