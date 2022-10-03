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
    /// Extension methods for String
    /// </summary>
    public static class StringExtensions
    {

        /// <summary>
        /// Returns the current string as it is.
        /// </summary>
        /// <param name="instance">A reference to the current string.</param>
        /// <returns>Returns the current string as it is.</returns>
        public static string AsString(this string instance)
        {
            return instance;
        }

        /// <summary>
        /// Checks whether the current string contains a numeric value.
        /// </summary>
        /// <param name="instance">A reference to the current string.</param>
        /// <returns>Returns true if the current string contains a numeric value.</returns>
        public static bool ContainsNumericValue(this string instance)
        {
            decimal test;
            return decimal.TryParse(instance, out test);
        }


        #region simple queries
        /// <summary>
        /// Returns <c>true</c> if the object's value represents zero.
        /// </summary>
        /// <param name="instance">A reference to the current string.</param>
        /// <returns>Returns <c>true</c> if the object's value represents zero.</returns>
        public static bool IsZeroes(this string instance)
        {
            return instance == "0".PadRight(instance.Length, '0');
        }

        /// <summary>
        /// Returns <c>true</c> if the object's value represents zero. 
        /// </summary>
        //public static bool IsZeros(this string instance)
        //{
        //    return instance.IsZeroes();
        //}

        #endregion

        #region IsEqualTo

        /// <summary>
        /// Returns true if the value of this string is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this string instance, Decimal other)
        {
            return (instance == other.ToString());
        }

        /// <summary>
        /// Returns true if the value of this string is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this string instance, int other)
        {
            return (instance.PadLeft(instance.Length, '0') == other.ToString().PadLeft(instance.Length, '0'));
        }

        /// <summary>
        /// Returns true if string contains all Hex FF
        /// </summary>
        /// <param name="instance">A reference to the current string.</param>
        /// <returns>Returns true if the string contains all 0xFF characters.</returns>
        public static bool IsMaxValue(this string instance)
        {
            return ContainsOnly(instance, AsciiChar.MaxValue);
        }

        /// <summary>
        /// Returns true if string contains all Hex 00
        /// </summary>
        /// <param name="instance">A reference to the current string.</param>
        /// <returns>Returns true if the string contains all 0x00 characters.</returns>
        public static bool IsMinValue(this string instance)
        {
            return ContainsOnly(instance, AsciiChar.MinValue);
        }

        /// <summary>
        /// Checks whether the current string contains only specified characters.
        /// </summary>
        /// <param name="instance">A reference to the current string.></param>
        /// <param name="AsciiValue">Specifies the character value for comparison.</param>
        /// <returns>Returns true if the current string contains only specified characters.</returns>
        public static bool ContainsOnly(this string instance, AsciiChar AsciiValue)
        {
            foreach (char cha in instance)
            {
                if (cha != AsciiValue)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Check to see if string contains all of one character
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool IsEqualToAll(this string instance, string other)
        {
            if (!(instance is IEquatable<string>))
                throw new NotImplementedException();

            foreach (char cha in instance)
            {
                if (cha != other[0])
                    return false;
            }
            return true;
        }

        #endregion

        #region IsLessThan

        /// <summary>
        /// Returns <c>true</c> if the value of this string is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        //Following method commented out because of duplicate extension in Framework.Core
        //public static bool IsLessThan(this string instance, string other)
        //{
        //    return (instance.CompareTo(other) < 0);
        //}

        /// <summary>
        /// Returns <c>true</c> if the value of this string is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this string instance, Decimal other)
        {
            return Decimal.Parse(instance) < other;
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this string is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this string instance, int other)
        {
            return Decimal.Parse(instance) < other;
        }

        /// <summary>
        /// Returns true if instance is less than that IField string value 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsLessThan(this string instance, IBufferValue other)
        {
            return (instance.CompareTo(other.BytesAsString) < 0);
        }


        #endregion

        #region IsGreaterThan

        /// <summary>
        /// Returns <c>true</c> if the value of this string is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        //Following method commented out because of duplicate extension in Framework.Core
        //public static bool IsGreaterThan(this string instance, string other)
        //{
        //    return (instance.CompareTo(other) > 0);
        //}

        /// <summary>
        /// Returns <c>true</c> if the value of this string is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this string instance, Decimal other)
        {
            return Decimal.Parse(instance) > other;
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this string is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this string instance, int other)
        {
            return Decimal.Parse(instance) > other;
        }
        /// <summary>
        /// Returns true if instance is greater that IField string value 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsGreaterThan(this string instance, IBufferValue other)
        {
            return (instance.CompareTo(other.BytesAsString) > 0);
        }

        #endregion

        #region IsLessThanOrEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this string is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        //public static bool IsLessThanOrEqualTo(this string instance, IField other)
        //{
        //    return (instance.CompareTo(other) <= 0);
        //}

        /// <summary>
        /// Returns <c>true</c> if the value of this string is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        //public static bool IsLessThanOrEqualTo(this string instance, string other)
        //{
        //    return (instance.CompareTo(other) <= 0);
        //}

        /// <summary>
        /// Returns <c>true</c> if the value of this string is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this string instance, Decimal other)
        {
            return Decimal.Parse(instance) <= other;
        }


        /// <summary>
        /// Compares the current string with a specified integer and indicates whether
        /// the current string precedes or appears in the same position as the integer in the sort order.
        /// </summary>
        /// <param name="instance">A reference to t current string.</param>
        /// <param name="other">Specifies the integer for comparison.</param>
        /// <returns>Returns true if the current string</returns>
        public static bool IsLessThanOrEqualTo(this string instance, int other)
        {
            return Decimal.Parse(instance) <= other;
        }



        #endregion

        #region IsGreaterThanOrEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this string is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        //public static bool IsGreaterThanOrEqualTo(this string instance, IField other)
        //{
        //    return (instance.CompareTo(other) >= 0);
        //}

        /// <summary>
        /// Returns <c>true</c> if the value of this string is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        //public static bool IsGreaterThanOrEqualTo(this string instance, string other)
        //{
        //    return (instance.CompareTo(other) >= 0);
        //}

        /// <summary>
        /// Returns <c>true</c> if the value of this string is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this string instance, Decimal other)
        {
            return Decimal.Parse(instance) >= other;
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this string is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this string instance, Int64 other)
        {
            return Decimal.Parse(instance) >= other;
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this string is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this string instance, int other)
        {
            return Decimal.Parse(instance) >= other;
        }

        #endregion

        #region IsNotEqualTo


        /// <summary>
        /// Returns <c>true</c> if the value of this string is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this string instance, Decimal other)
        {
            return !(instance.IsEqualTo(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this string is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this string instance, Int64 other)
        {
            return !(instance.IsEqualTo(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this string is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this string instance, int other)
        {
            return !(instance.IsEqualTo(other));
        }
        #endregion

        /// <summary>
        /// Replaces the first occurence found
        /// </summary>
        /// <param name="original">Current imstance of the string</param>
        /// <param name="oldValue">Value to be replaced</param>
        /// <param name="newValue">Value to replace the old value</param>
        /// <returns>
        /// Empty string if instance is null or empty
        /// If value to be replaced is null or empty the instance is returned
        /// Empty string if the value to replace the old value is null or empty
        /// The new replaced value is returned if there are values to be replaced
        /// </returns>
        public static string ReplaceFirstOccurrance(this string instance, string oldValue, string newValue)
        {
            if (String.IsNullOrEmpty(instance))
                return String.Empty;
            if (String.IsNullOrEmpty(oldValue))
                return instance;
            if (String.IsNullOrEmpty(newValue))
                newValue = String.Empty;
            int loc = instance.IndexOf(oldValue);
            if (loc < 0)
                return instance;
            else
                return instance.Remove(loc, oldValue.Length).Insert(loc, newValue);
        }

        public static bool ContainsOnly(this string instance, params string[] limitStrings)
        {
            List<char> limitChars = new List<char> { };

            for (int ctr = 0; ctr < limitStrings.Length; ctr++)
            {
                if (limitStrings[ctr] == "THROUGH" || limitStrings[ctr] == "THRU")
                {
                    int startByte = Convert.ToByte(limitChars[ctr - 1]);
                    int endByte = Convert.ToByte(limitStrings[ctr + 1][0]);
                    for (int bctr = startByte + 1; bctr <= endByte; bctr++)
                    {
                        limitChars.Add(Convert.ToChar(bctr));
                    }
                    ctr++;
                }
                else
                {
                    limitChars.Add(limitStrings[ctr][0]);
                }
            }

            for (int ctr = 0; ctr < instance.Length; ctr++)
            {
                char testchar = instance[ctr];
                if (!limitChars.Contains(testchar))
                    return false;
            }
            return true;
        }

        public static void SetValue(this string instance, string newvalue)
        {
            instance = newvalue;
        }

        public static void SetValue(this string instance, IField newFieldValue)
        {
            instance = newFieldValue.AsString();
        }

        public static void SetValue(this string instance, IGroup newGroupValue)
        {
            instance = newGroupValue.AsString();
        }

        public static int InspectGivingCount(this string instance, string searchString, string searchCondition)
        {
            int returnCount = 0;
            if (instance == null || instance == string.Empty)
                returnCount = 0;
            else
            {
                if (searchCondition == "CHARACTERS BEFORE")
                {
                    int idx = instance.IndexOf(searchString);
                    if (idx > -1)
                        returnCount = idx;
                    else
                        returnCount = instance.Length;
                }
                else if (searchCondition == "LEADING")
                {
                    int ctr = 0;
                    while (ctr < instance.Length)
                    {
                        if (instance[ctr] == searchString[0])
                        {
                            returnCount++;
                        }
                        else
                        {
                            break;
                        }
                        ctr++;
                    }
                }
                else
                // Inspect giving ALL
                {
                    int ctr = 0;
                    while (ctr < instance.Length)
                    {
                        int idx = instance.Substring(ctr).IndexOf(searchString);
                        if (idx > -1)
                        {
                            returnCount++;
                            ctr = ctr + idx + searchString.Length;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return returnCount;
        }

        public static string GetSubstring(this string instance, int startIdx, int length)
        {
            return instance.Substring(startIdx - 1, length);
        }

        public static string GetSubstring(this string instance, int startIdx)
        {
            return instance.Substring(startIdx - 1);
        }

        public static byte[] AsUTF8Bytes(this string instance)
        {
            if (string.IsNullOrEmpty(instance)) return null;
            var utf8 = new UTF8Encoding();
            byte[] bytes = utf8.GetBytes(instance);
            return bytes;
        }


    }
}
