using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using MDSY.Framework.Interfaces;
using MDSY.Framework.Core.Constants;
using System.Globalization;
using MDSY.Framework.Buffer.Interfaces;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Extends string objects with the methods that are required by the framework needs.
    /// </summary>
    public static class StringExtensions
    {
        #region private fields
        /// <summary>
        /// Contains the upper- and lower-case characters of the English alphabet. 
        /// </summary>
        private static char[] alphaOnlyChars = new char[] { 'a', 'b', 'c', 'd', 
            'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 
            's', 't', 'y', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F',
            'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
            'U', 'V', 'W', 'X', 'Y', 'Z' };
        private static char[] numericOnlyChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        #endregion

        #region simple queries

        /// <summary>
        /// Returns <c>true</c> if the instance string and another string 
        /// object have the same value.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A string for comparison.</param>
        /// <returns>Returns true if current string matches with the provided string.</returns>
        public static bool IsEqualTo(this string instance, string value)
        {
            return instance.TrimEnd().Equals(value.TrimEnd() as string);  // Issue 8580
        }

        /// <summary>
        /// Checks whether the current string does not match the provided string.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A string for comparison.</param>
        /// <returns>Returns true if current string does not match the provided string.</returns>
        public static bool IsNotEqualTo(this string instance, string value)
        {
            return !(instance.Equals(value as string));
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string and the <c>Text</c>
        /// property of the given field have the same value.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A reference to a IField object, which has a string value for comparison.</param>
        public static bool IsEqualTo(this string instance, IField value)
        //public static bool IsEqualTo(this string instance, IFieldBase_Old value)
        {
            return instance.IsEqualTo(value.GetValue<string>());
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string and the <c>Text</c>
        /// property of the given group have the same value.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A reference to an IGroup object, which string representation needs to be compared with the current string object.</param>
        /// <returns>Returns true if the instance string matches with the string representation of the provided group object.</returns>
        public static bool IsEqualTo(this string instance, IGroup value)
        {
            return instance.IsEqualTo(value.BytesAsString);
        }

        /// <summary>
        /// Checks whether the instance string does not match with the string value of the provided IField object.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A reference to an IField object, which has a string value for comparison.</param>
        /// <returns>Returns true if the instance string does not match with the string value of the provided IField object.</returns>
        public static bool IsNotEqualTo(this string instance, IField value)
        {
            return instance.IsNotEqualTo(value.GetValue<string>());
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string is equal to string.Empty,
        /// but not if is <c>null</c>.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <returns>Returns true if the provided string is an empty string.</returns>
        public static bool IsEmpty(this string instance)
        {
            return (instance == string.Empty);
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string is greater than another string 
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A string for comparison.</param>
        /// <returns>Returns true if the instance string is greated than the provided string.</returns>
        public static bool IsGreaterThan(this string instance, string value)
        {
            return instance.CompareTo(value) > 0;
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string is greater than another  Field value
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A reference to an IField object, which has a string value for comparison.</param>
        /// <returns>Returns true if the instance string is greater that the string value of the provided IField object.</returns>
        public static bool IsGreaterThan(this string instance, IField value)
        {
            return instance.CompareTo(value.GetValue<string>()) > 0;
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string is greater than or equal to than another string 
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A string for comparison.</param>
        /// <remarks>Returns true if the instance string is greater than or equal to the provided string.</remarks>
        public static bool IsGreaterThanOrEqualTo(this string instance, string value)
        {
            return instance.CompareTo(value) >= 0;
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string is greater than or equal to another  Field value
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A reference to an IField object, which has a string value for comparison.</param>
        /// <returns>Returns true if the instance string is greater than or equal to the string value of the provided IField object.</returns>
        public static bool IsGreaterThanOrEqualTo(this string instance, IField value)
        {
            return instance.CompareTo(value.GetValue<string>()) >= 0;
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string is less than another string 
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A string for comparison.</param>
        /// <returns>Returns true if the instance string is less than the provided string.</returns>
        public static bool IsLessThan(this string instance, string value)
        {
            return instance.CompareTo(value) < 0;
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string is less than another  Field value
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A reference to an IField object, which has a string value for comparison.</param>
        /// <returns>Returns true if the instance string is less than the string value of the provided IField object.</returns>
        public static bool IsLessThan(this string instance, IField value)
        {
            return instance.CompareTo(value.GetValue<string>()) < 0;
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string is less or equal to than another string 
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A string for comparison.</param>
        /// <returns>Returns true if the instance string is less than or equal to the provided string.</returns>
        public static bool IsLessThanOrEqualTo(this string instance, string value)
        {
            return instance.CompareTo(value) <= 0;
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string is less than or equal to another  Field value
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A reference to the IField object, which has a string for comparison.</param>
        /// <returns>Returns true if the instance string is less than or equeal to the string value of the provided IField object.</returns>
        public static bool IsLessThanOrEqualTo(this string instance, IField value)
        {
            return instance.CompareTo(value.GetValue<string>()) <= 0;
        }

        /// <summary>
        /// Returns <c>true</c> if the string contains characters only from 
        /// the standard English alphanumeric character set. 
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <returns> Returns <c>true</c> if the string contains characters only from 
        /// the standard English alphanumeric character set.
        /// </returns>
        public static bool IsAlphaNumeric(this string instance)
        {
            var alphaNumericChars = alphaOnlyChars.Concat(numericOnlyChars);
            return instance
                .Where(c => !alphaNumericChars.Contains(c))
                .Count()
                .Equals(0);
        }

        /// <summary>
        /// Returns <c>true</c> if the string instance contains a value that can 
        /// be converted to a numeric value.
        /// </summary>
        /// <remarks>Provided as a pass-through to simplify code generation.</remarks>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <returns>Returns <c>true</c> if the string instance contains a value that can 
        /// be converted to a numeric value.
        /// </returns>
        public static bool IsNumericValue(this string instance)
        {
            return IsNumeric(instance);
        }

        /// <summary>
        /// Returns <c>true</c> if the string instance contains a value that can 
        /// be converted to either an <c>int</c>, a <c>byte</c>, or a <c>decimal</c>.
        /// </summary>
        /// <remarks>
        /// <note>
        /// <para>Changes made to IsNumeric() on 9/4/2012 are Breaking Changes, in that numeric 
        /// strings with leading white space were previously considered numeric by the default TryParse() 
        /// methods. Calls below now pass in NumberStyles.None to indicate that no style elements (such as whitespace)
        /// can be present in the parsed string.</para>
        /// <para>To restore the previous behavior, pass in NumberStyles.AllowLeadingWhite | NumberStyles.AllowTralingWhite.</para>
        /// </note>
        /// </remarks>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <returns>Returns <c>true</c> if the string instance contains a value that can 
        /// be converted to either an <c>int</c>, a <c>byte</c>, or a <c>decimal</c>.</returns>
        public static bool IsNumeric(this string instance)
        {
            bool result = false;

            byte byteTest = 0;
            if (byte.TryParse(instance, NumberStyles.None, null as IFormatProvider, out byteTest))
            {
                result = true;
            }
            else
            {
                long longIntTest = 0;
                if (long.TryParse(instance, NumberStyles.None, null as IFormatProvider, out longIntTest))
                {
                    result = true;
                }
                else
                {
                    decimal decTest = 0;
                    result = decimal.TryParse(instance, NumberStyles.None, null as IFormatProvider, out decTest);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> if the string contains characters only from 
        /// the standard English alphabetic character set. 
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <returns>Returns <c>true</c> if the string contains characters only from 
        /// the standard English alphabetic character set.</returns>
        public static bool IsAlphabetic(this string instance)
        {
            return instance
                .Where(c => !alphaOnlyChars.Contains(c))
                .Count()
                .Equals(0);
        }


        /// <summary>
        /// Returns <c>true</c> if the instance string contains only space characters.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <returns>Returns <c>true</c> if the instance string contains only space characters.</returns>
        public static bool IsSpaces(this string instance)
        {
            return instance.ContainsOnly(new char[] { ' ' });
        }


        /// <summary>
        /// Returns <c>true</c> if the instance string contains only space characters.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <returns>Returns <c>true</c> if the instance string contains only space characters.</returns>
        public static bool IsNotSpaces(this string instance)
        {
            return !instance.IsSpaces();
        }


        /// <summary>
        /// Returns <c>true</c> if the instance string contains only zero ('0') characters.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <returns>Returns <c>true</c> if the instance string contains only zero ('0') characters.</returns>
        //public static bool IsZeroes(this string instance)
        //{
        //    return instance.ContainsOnly(new char[] { '0' });
        //}
        /// <summary>
        /// Returns <c>true</c> if the instance string contains only zero ('0') characters.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <returns>Returns <c>true</c> if the instance string contains only zero ('0') characters.</returns>
        public static bool IsZeros(this string instance)
        {
            return instance.ContainsOnly(new char[] { '0' });
        }
        #endregion

        #region public methods
        /// <summary>
        /// Returns <c>true</c> if the instance string contains only
        /// chars that are listed in <paramref name="limitChars"/>.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="limitChars">Characters for comparison.</param>
        /// <returns>Returns <c>true</c> if the instance string contains only
        /// chars that are listed in <paramref name="limitChars"/>.</returns>
        public static bool ContainsOnly(this string instance, char[] limitChars)
        {
            return instance
                .Where(c => (!limitChars.Contains(c)))
                .Count()
                .Equals(0);
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string contains another  Field value
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="value">A reference to an IField object, which contains a string value for comparison.</param>
        /// <returns>Returns <c>true</c> if the instance string contains another  Field value.</returns>
        public static bool Contains(this string instance, IField value)
        {
            return instance.Contains(value.GetValue<string>());
        }

        /// <summary>
        /// Returns a new string object, the same length as the instance string, 
        /// filled with the given fillerValue; repeating fillerValue, if necessary, 
        /// to fill the entire length.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <param name="fillerValue"></param>
        /// <returns>A new string instance.</returns>
        /// <example>
        /// string myString = "0123456789";
        /// string newValue = myString.FillWith("Abc");
        /// // newValue is "AbcAbcAbcA"
        /// </example>
        public static string FillWith(this string instance, string fillerValue)
        {
            int index = 0;
            int subStringIndex = 0;
            int length = instance.Length;
            StringBuilder builder = new StringBuilder(length, length);

            while (index < length)
            {
                int remaining = length - index;
                if (fillerValue.Length <= remaining)
                {
                    builder.Append(fillerValue);
                    index += fillerValue.Length;
                }
                else
                {
                    builder.Append(fillerValue[subStringIndex]);
                    subStringIndex++;
                    index++;
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Returns the index of the first occurrence of any of the given delimiters 
        /// within the source string.
        /// </summary>
        /// <param name="source">The string object, which is extended with the current method.</param>
        /// <param name="delimiters">A collection of the string delimiters.</param>
        /// <param name="firstDelimiterIdx">Output parameter, which contains index of the first occurrence of any of the provided delimiters found in the string instance.</param>
        /// <returns>Returns the infex of the first occurrence of any fo the provided delimiters withing the instance string.</returns>
        public static int FirstIndexOfAny(this string source, IEnumerable<string> delimiters, out int firstDelimiterIdx)
        {
            int result = source.Length;
            int delimCount = delimiters.Count();
            int idx = 0;
            IList<string> delims = delimiters.ToList();
            int currentLowestDelimIdx = -1;
            firstDelimiterIdx = -1;

            while ((result != 0) && (idx < delimCount))
            {
                int sourceIndexOf = source.IndexOf(delims[idx]);
                if ((sourceIndexOf >= 0) && (sourceIndexOf < result))
                {
                    currentLowestDelimIdx = idx;
                    result = sourceIndexOf;
                }

                idx++;
            }

            // if we didn't find any, return -1
            if (result.Equals(source.Length))
            {
                result = -1;
            }
            else
            {
                firstDelimiterIdx = currentLowestDelimIdx;
            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> if the instance string is the same as one of the 
        /// string values contained in <c>MDSY.Common.Constants.Booleans.TrueStrings</c>.
        /// </summary>
        /// <param name="instance">The string object, which is extended with the current method.</param>
        /// <returns><c>true</c>, if the string value represents "true".</returns>
        public static bool AsBoolean(this string instance)
        {
            return (Booleans.TrueStrings.Contains(instance.ToUpper()));
        }

        /// <summary>
        /// Creates decimal value from string
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static decimal ToNumberValue(this string source)
        {
            decimal testDec = 0;
            decimal.TryParse(source.Replace("$", "").Trim(), out testDec);
            return testDec;
        }

        #endregion

        #region obsolete
        /// <summary>
        /// Provides a pass-through method to the <c>string.SubString()</c> method,
        /// correcting for 1-based COBOL strings. 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [Obsolete("Use BaseField.SubString() instead.", true)]
        public static string CobolSubstring(this string instance, int startIndex, int length)
        {
            return instance.Substring(startIndex - 1, length);
        }

        #endregion

    }
}
