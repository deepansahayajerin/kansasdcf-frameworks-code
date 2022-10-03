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
    /// Extension methods for the integer object.
    /// </summary>
    public static class IntExtensions
    {
        #region general

        /// <summary>
        /// Returns the value of int as an Deimal if possible. 
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns the current object value as a decimal.</returns>
        public static decimal AsDecimal(this int instance)
        {
            return (decimal)instance;
        }

        /// <summary>
        /// Returns the value of the int as a string if possible.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns the current object value as a string.</returns>
        public static string AsString(this int instance)
        {
            return instance.ToString();
        }
        /// <summary>
        /// Sets the Int with Field AsInt value.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="intValue">The int value.</param>
        public static void SetValue(this int instance, IField intValue)
        {
            instance = intValue.AsInt();
        }



        #endregion

        #region simple queries
        /// <summary>
        /// Returns <c>true</c> if the object's value represents zero.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns <c>true</c> if the object's value represents zero.</returns>
        public static bool IsZeroes(this int instance)
        {
            return instance == 0;
        }

        /// <summary>
        /// Returns <c>true</c> if the object's value represents zero.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns <c>true</c> if the object's value represents zero.</returns>
        public static bool IsZeros(this int instance)
        {
            return instance.IsZeroes();
        }

        /// <summary>
        /// Checks whether the current object value is negative.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object is negative.</returns>
        public static bool IsNegative(this int instance)
        {
            return (instance < 0);
        }

        /// <summary>
        /// Checks whether the current object value is positive.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object is positive.</returns>
        public static bool IsPositive(this int instance)
        {
            return (instance  >= 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the field object's value is in range (inclusive) of the <paramref name="loBound"/> and 
        /// <paramref name="hiBound"/> values.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="loBound">Specifies the lower boundary.</param>
        /// <param name="hiBound">Specifies the upper boundary.</param>
        /// <returns>Returns true if the current object value lays withing the specified range.</returns>
        public static bool IsInRange(this int instance, int loBound, int hiBound)
        {

            return (instance >= loBound && instance <= hiBound);
        }


        #endregion

        #region IsEqualTo
        /// <summary>
        /// Returns true if the value of this int is equal to the value of the <paramref name="other"/> field.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if both fields are <c>null</c>, if both fields refer to the same object instance, or 
        /// if the two field values are functionally equivalent. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this int instance, IField other)
        {

            return (instance == other.AsInt());
        }


        /// <summary>
        /// Returns true if the value of this int is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this int instance, Decimal other)
        {

            return (instance == (decimal)other);
        }

        /// <summary>
        /// Returns true if the value of this int is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this int instance, Int64 other)
        {
            return (instance == other);
        }

        /// <summary>
        /// Returns true if the value of this int is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this int instance, int other)
        {

            return (instance ==  other);
        }

        /// <summary>
        /// Returns true if the value of this int is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this int instance, string other)
        {

            return (instance == int.Parse(other));
        }

        #endregion

        #region IsLessThan

        /// <summary>
        /// Returns <c>true</c> if the value of this int is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this int instance, IField other)
        {
            return (instance < other.AsInt());
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this int instance, string other)
        {
            return (instance < int.Parse(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this int instance, Decimal other)
        {
            return (instance.CompareTo(other) < 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this int instance, int other)
        {
            return (instance < other);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this int instance, Int64 other)
        {
            return (instance.CompareTo(other) < 0);
        }


        #endregion

        #region IsGreaterThan
        /// <summary>
        /// Returns <c>true</c> if the value of this int is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this int instance, IField other)
        {
            return (instance > other.AsInt());
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this int instance, string other)
        {
            return (instance  > int.Parse(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this int instance, Decimal other)
        {
            return (instance.CompareTo(other) > 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this int instance, int other)
        {
            return (instance > other);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this int instance, Int64 other)
        {

            return (instance.CompareTo(other) > 0);
        }


        #endregion

        #region IsLessThanOrEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this int is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this int instance, IField other)
        {
            return (instance  <= other.AsInt());
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this int instance, string other)
        {
            return (instance <= int.Parse(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this int instance, Decimal other)
        {
            return (instance.CompareTo(other) <= 0);
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this int is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this int instance, Int64 other)
        {
            return (instance <= other);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this int instance, int other)
        {
            return (instance  <= other);
        }



        #endregion

        #region IsGreaterThanOrEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this int is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this int instance, IField other)
        {
            return (instance  >= other.AsInt());
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this int instance, string other)
        {
            return (instance  >= int.Parse(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this int instance, Decimal other)
        {
            return (instance.CompareTo(other) >= 0);
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this int is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this int instance, Int64 other)
        {
            return (instance >= other);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this int instance, int other)
        {
            return (instance >= other);
        }

        #endregion

        #region IsNotEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this int is not equal to the value of the <paramref name="other"/> field.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>false</c> if both fields are <c>null</c>, if both fields refer to the same object instance, or 
        /// if the two field values are functionally equivalent. Otherwise, returns <c>true</c>.</returns>
        public static bool IsNotEqualTo(this int instance, IField other)
        {
            return !(instance.IsEqualTo(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this int instance, string other)
        {
            return !(instance.IsEqualTo(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this int instance, Decimal other)
        {
            return !(instance.IsEqualTo(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this int instance, Int64 other)
        {
            return !(instance.IsEqualTo(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this int is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this int instance, int other)
        {
            return !(instance.IsEqualTo(other));
        }
        #endregion

        #region Arithmetic
        /// <summary>
        /// Add an integer to this instance value
        /// </summary>
        /// <param name="instance">A reference to the current integer.</param>
        /// <param name="value">The integer value to be added.</param>
        /// <returns>Returns the sum of the current integer and the specified integer.</returns>
        public static int Add(this int instance, int value)
        {
            instance = instance + value;
            return instance;
        }

        /// <summary>
        /// Add a decimal to this instance value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">The decimal value to be added.</param>
        /// <returns>Returns the sum of the current integer and the specified decimal.</returns>
        public static int Add(this int instance, decimal value)
        {
            instance = instance + (int)value;
            return instance;
        }
        /// <summary>
        /// Subtracts an integer from this instance value
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Subtract(this int instance, int value)
        {
            instance = instance - value;
            return instance;
        }
        /// <summary>
        /// Subtracts a decimal from this instance value
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Subtract(this int instance, decimal value)
        {
            instance = instance - (int)value;
            return instance;
        }
        #endregion

    }
}
