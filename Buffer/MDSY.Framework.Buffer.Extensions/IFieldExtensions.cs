using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Common;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using MDSY.Framework.Buffer.Services;
using System.Text.RegularExpressions;
using MDSY.Framework.Configuration.Common;
using System.Web;

namespace MDSY.Framework.Buffer
{
    /// <summary>
    /// Extension methods for objects with implement IField 
    /// </summary>
    public static class IFieldExtensions
    {
        #region private
        /// <summary>
        /// Contains the upper- and lower-case characters of the English alphabet. 
        /// </summary>
        private readonly static char[] alphaOnlyChars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'y', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        private readonly static char[] alphaUpperOnlyChars = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        private readonly static char[] numericOnlyChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private readonly static string standardDateFormat = "yyyy-MM-dd";

        private static String DecimalSeparator
        {
            get { return CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator; }
        }

        private static Char DecimalSeparatorChar
        {
            get { return CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]; }
        }
        private static string DefaultDateFormat
        {
            get
            {
                if (!(String.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("DefaultDateFormat"))))
                {
                    return ConfigSettings.GetAppSettingsString("DefaultDateFormat");
                }
                else
                    return standardDateFormat;
            }
        }
        private static IIndexBaseServices Indexes
        {
            get { return BufferServices.Indexes; }
        }
        #endregion

        #region private methods
        private static IList<byte> GetBytesByLength(byte value, int length)
        {
            return Enumerable.Repeat<byte>(value, length).ToList();
        }
        #endregion

        #region general
        /// <summary>
        /// Applies the given criteria to the instance object and returns the result.
        /// </summary>
        /// <param name="instance">The field object which will be passed to <paramref name="criteria"/>.</param>
        /// <param name="criteria">The Func&lt;IField, bool&gt; to be invoked on <paramref name="instance"/>.</param>
        /// <returns>The result of the applied criteria.</returns>
        public static bool MeetsCriteria(this IField instance, Expression<Func<IField, bool>> criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria", "criteria is null.");

            //Logging.LogExpression<IField>(criteria, instance);
            return criteria.Compile().Invoke(instance);
        }

        /// <summary>
        /// Returns the string representation of the maximum possible hex value for the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The field object whose length will be used to calculate the max value.</param>
        /// <returns>A string representation of a hex value similar to "FFFFFFFF"</returns>
        public static string MaxHex(this IField instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            return new string(Enumerable.Repeat<char>('F', instance.LengthInBuffer * 2).ToArray());
        }

        /// <summary>
        /// Returns the string representation of the minimum possible hex value for the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The field object whose length will be used to calculate the min value.</param>
        /// <returns>A string representation of a hex value similar to "00000000"</returns>
        public static string MinHex(this IField instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            return new string(Enumerable.Repeat<char>('0', instance.LengthInBuffer * 2).ToArray());
        }

        /// <summary>
        /// Returns a byte array containing the maximum possible binary value for the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The field object whose length will be used to calculate the max value.</param>
        public static byte[] MaxBytes(this IField instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            var bytes = GetBytesByLength(byte.MaxValue, instance.LengthInBuffer);
            return bytes.ToArray();
        }

        /// <summary>
        /// Returns a byte array containing the minimum possible binary value for the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The field object whose length will be used to calculate the min value.</param>
        public static byte[] MinBytes(this IField instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            var bytes = GetBytesByLength(byte.MinValue, instance.LengthInBuffer);
            return bytes.ToArray();
        }

        /// <summary>
        /// Gets the maximum possible integer value for the given <paramref name="instance"/>. 
        /// Note, field length for <paramref name="instance"/> must be 8 or less or MaxInt will return <c>false</c>.
        /// </summary>
        /// <param name="instance">The field object whose length will be used to calculate the max value.</param>
        /// <param name="max">The max integer value.</param>
        /// <returns><c>true</c> if max can be calculated for the given field's length, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        public static bool MaxInt(this IField instance, out Int64 max)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            bool result = false;
            max = 0;
            // a long is only 8 bytes. 
            if (instance.LengthInBuffer <= 8)
            {
                //var bytes = GetBytesByLength(byte.MaxValue, instance.LengthInBuffer);
                var bytes = GetBytesByLength((byte)(sbyte.MaxValue), instance.LengthInBuffer);
                // byte array for ToInt64 has to be 8 bytes long.
                while (bytes.Count < 8)
                {
                    bytes.Insert(0, 0);
                }

                if (BitConverter.IsLittleEndian)
                {
                    bytes = bytes.Reverse().ToList();
                }

                max = BitConverter.ToInt64(bytes.ToArray(), 0);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Gets the maximum possible unsigned integer value for the given <paramref name="instance"/>.
        /// Note, field length for <paramref name="instance"/> must be 8 or less or MaxUInt will return <c>false</c>.
        /// </summary>
        /// <param name="instance">The field object whose length will be used to calculate the max value.</param>
        /// <param name="max">The max unsigned integer value.</param>
        /// <returns><c>true</c> if max can be calculated for the given field's length, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        public static bool MaxUInt(this IField instance, out UInt64 max)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            bool result = false;
            max = 0;

            // a ulong is only 8 bytes. 
            if (instance.LengthInBuffer <= 8)
            {
                var bytes = instance.MaxBytes();
                max = BitConverter.ToUInt64(bytes, 0);
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Gets the minimum possible unsigned integer value for the given <paramref name="instance"/>.
        /// Note, no matter the length of <paramref name="instance"/>, min will be zero (0). 
        /// </summary>
        /// <remarks>
        /// Yes, this really is a thing. Balance in all things, Grasshopper. There's a MaxUInt() so...
        /// </remarks>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="min">Takes the result value.</param>
        /// <returns>Returns true.</returns>
        public static bool MinUInt(this IField instance, out UInt64 min)
        {
            min = 0;
            return true;
        }

        /// <summary>
        /// Returns the value of the field object as an Int32 if possible. Semantically the same as 
        /// <c>myField.GetValue&lt;int&gt;()</c>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns the value of the field object as an Int32 if possible.</returns>
        public static int AsInt(this IField instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            int tmp = 0;
            if (instance.FieldType == FieldType.FloatSingle)
            {
                try { tmp = (int)instance.GetValue<float>(); }
                catch (FieldValueException) { /* do nothing */ }
            }
            else if (instance.FieldType == FieldType.FloatDouble)
            {
                try { tmp = (int)instance.GetValue<double>(); }
                catch (FieldValueException) { /* do nothing */ }
            }
            else
            {
                try { tmp = instance.GetValue<int>(); }
                catch (FieldValueException) { /* do nothing */ }
            }

            return tmp;
        }


        /// <summary>
        /// Returns the value of the field object as an Int64 if possible. Semantically the same as 
        /// <c>myField.GetValue&lt;int&gt;()</c>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns the value of the field object as an Int32 if possible.</returns>
        public static Int64 AsInt64(this IField instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            Int64 tmp = 0;
            try { tmp = instance.GetValue<Int64>(); }
            catch (FieldValueException) { /* do nothing */ }

            return tmp;
        }

        /// <summary>
        /// Returns the value of the field object as an Int16
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static Int16 AsInt16(this IField instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            Int16 tmp = 0;
            try { tmp = instance.GetValue<short>(); }
            catch (FieldValueException) { /* do nothing */ }

            return tmp;
        }

        /// <summary>
        /// Returns integer value. If value is null or non numeric and returns 0
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns an interger representation of the current object's value.</returns>
        public static int AsSafeInt(this IField instance)
        {
            if (instance == null)
                return 0;

            if (!instance.IsNumericValue())
                return 0;

            return instance.GetValue<int>();
        }


        /// <summary>
        /// Returns the value of the field object as an Deimal if possible. Semantically the same as 
        /// <c>myField.GetValue&lt;decimal&gt;()</c>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns a decimal representation of the current object's value.</returns>
        public static decimal AsDecimal(this IField instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            bool isEmptyString = true;
            foreach (byte b in instance.AsBytes)
            {
                if (b != ' ')
                {
                    isEmptyString = false;
                    break;
                }
            }

            return isEmptyString ? 0 : instance.GetValue<decimal>();
        }

        /// <summary>
        /// Return decimal with specified precision
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="totalLength"></param>
        /// <param name="decimalLength"></param>
        /// <returns></returns>
        public static decimal AsDecimal(this IField instance, int totalLength, int decimalLength)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            string tempNumber = instance.BytesAsString.Trim();

            if (tempNumber.Length > decimalLength && !tempNumber.Contains(DecimalSeparator))
            {
                tempNumber = tempNumber.Insert(tempNumber.Length - decimalLength, DecimalSeparator);
            }
            Decimal returnDec = 0;

            decimal.TryParse(tempNumber, out returnDec);
            return returnDec;
        }

        /// <summary>
        /// Returns the value of the field object as a string if possible. Semantically the same as 
        /// <c>myField.GetValue&lt;string&gt;()</c>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns a string representation of the current object's value.</returns>
        public static string AsString(this IField instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");
            if (instance.IsNumericType)
            {
                string stringValue;
                //Following check added for invalid numeric va;lues needing to get passed on a map
                if (!instance.IsNumericValue())
                    return instance.BytesAsString;

                if (instance.IsMinValue())
                {
                    stringValue = "0".PadLeft(instance.DisplayLength, '0');
                }
                else
                {
                    stringValue = instance.GetValue<string>().PadLeft(instance.DisplayLength, '0');
                }

                if (instance.DecimalDigits > 0 && !stringValue.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                {
                    stringValue = stringValue.Insert(stringValue.Length - instance.DecimalDigits, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                }
                if (stringValue.Contains('-'))
                    stringValue = string.Concat("-", stringValue.Replace("-", ""));
                return stringValue;
            }
            else
                return instance.GetValue<string>();


        }

        /// <summary>
        /// Returns a string representation of the date value in yyyy-MM-dd format.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns a string representation of the date value.</returns>
        public static string AsDateString(this IField instance)
        {
            if (instance.AsString() == "00000000")
                instance.SetValue("00010101");
            string dateString;
            if (instance.AsString().Contains(".") || instance.AsString().Contains("-") || instance.AsString().Trim() == string.Empty)
            {
                dateString = instance.AsString();
            }
            else
            {
                dateString = instance.AsString().Insert(6, "-").Insert(4, "-");
            }

            return dateString;
        }

        public static DateTime AsDateTime(this IField instance)
        {
            if (instance.AsString() == "00000000")
                instance.SetValue("00010101");
            DateTime tempDT;
            if (instance.AsString().Contains(".") || instance.AsString().Contains("-"))
            {
                string dateFormat = standardDateFormat;
                if (instance.AsString().Contains(".") && DefaultDateFormat.Contains("."))
                    dateFormat = DefaultDateFormat;
                int day = 0, month = 0, year = 0;
                for (int ctr = 0; ctr < dateFormat.Length; ctr++)
                {
                    if (dateFormat[ctr] == 'd')
                    {
                        day = Convert.ToInt32(instance.GetSubstring(ctr + 1, 2));
                        ctr++;
                    }
                    else if (dateFormat[ctr] == 'M')
                    {
                        month = Convert.ToInt32(instance.GetSubstring(ctr + 1, 2));
                        ctr++;
                    }
                    else if (dateFormat[ctr] == 'y')
                    {
                        year = Convert.ToInt32(instance.GetSubstring(ctr + 1, 4));
                        ctr = ctr + 3;
                    }
                }
                tempDT = new DateTime(year, month, day);
            }
            else
                tempDT = new DateTime(Convert.ToInt32(instance.GetSubstring(1, 4)), Convert.ToInt32(instance.GetSubstring(4, 2)), Convert.ToInt32(instance.GetSubstring(6, 2)));
            return tempDT;
        }

        /// <summary>
        /// Returns a byte array of the Field contents
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static byte[] AsBytes(this IField instance)
        {
            return instance.AsBytes;
        }

        /// <summary>
        /// Returns a boolean from the Field value
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static bool AsBool(this IField instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            return (instance.AsInt() == 1);
        }


        /// <summary>
        /// Attempts to increment the value of the field object, if numeric, by 1.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void IncInt(this IField instance)
        {
            IncInt(instance, 1);
        }

        /// <summary>
        /// Attempts to increment the value of the field object, if numeric, by the given <paramref name="incBy"/> amount.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="incBy">Incrementation value.</param>
        public static void IncInt(this IField instance, int incBy)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            if (instance.IsNumericValue())
            {
                int value = instance.AsInt();
                value += incBy;
                instance.Assign(value);
            }
        }


        #endregion

        /// <summary>
        /// Returns a generalized indication of field type if the given <paramref name="instance"/>
        /// is a numeric FieldType.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns a generalized indication of field type if current object is numeric.
        /// Returns NumericFieldType.Unknown if current object is not numeric.</returns>
        public static NumericFieldType GetNumericType(this IField instance)
        {
            NumericFieldType result = NumericFieldType.Unknown;

            switch (instance.FieldType)
            {
                case FieldType.String:
                case FieldType.Boolean:
                    result = NumericFieldType.NotNumeric;
                    break;

                case FieldType.PackedDecimal:
                case FieldType.UnsignedPackedDecimal:
                    result = NumericFieldType.PackedDecimal;
                    break;

                case FieldType.SignedNumeric:
                case FieldType.CompShort:
                case FieldType.CompInt:
                case FieldType.CompLong:
                case FieldType.ReferencePointer:
                    result = NumericFieldType.SignedInteger;
                    break;

                case FieldType.SignedDecimal:
                case FieldType.UnsignedDecimal:
                case FieldType.FloatSingle:
                case FieldType.FloatDouble:
                    result = NumericFieldType.Decimal;
                    break;

                case FieldType.UnsignedNumeric:
                    result = NumericFieldType.UnsignedInteger;
                    break;
            }

            return result;
        }

        #region simple queries
        /// <summary>
        /// Returns <c>true</c> if the IBaseField object's text value contains 
        /// only characters that are in the given char array.
        /// </summary>
        /// <param name="instance">A referenence to the current object.</param>
        /// <param name="limitChars">An array fo characters for comparison.</param>
        /// <returns>Returns true if current object's value contains only specified characters.</returns>
        public static bool ContainsOnly(this IField instance, params char[] limitChars)
        {
            // RKL: note, replacing calls to AsBytes with local instance for performance. 
            // AsBytes calls Buffer.ReadBytes everytime. Call and cache the bytes instead.
            // for (int ctr = 0; ctr < instance.AsBytes.Length; ctr++)

            byte[] bytes = instance.AsBytes;
            for (int ctr = 0; ctr < bytes.Length; ctr++)
            {
                char testchar = (char)bytes[ctr];
                if (!limitChars.Contains(testchar))
                    return false;
            }
            return true;
        }

        public static bool ContainsOnly(this IField instance, params string[] limitStrings)
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

            byte[] bytes = instance.AsBytes;
            for (int ctr = 0; ctr < bytes.Length; ctr++)
            {
                char testchar = (char)bytes[ctr];
                if (!limitChars.Contains(testchar))
                    return false;
            }
            return true;
        }



        /// <summary>
        /// Returns <c>true</c> if the object's value text is made up of only spaces.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns <c>true</c> if the object's value text is made up of only spaces.</returns>
        public static bool IsSpaces(this IField instance)
        {
            if (instance == null)
                return false;
            if (instance.IsNumericType)
                return instance.AsString().Trim().CompareTo("0") == 0;
            else if (instance.FieldType == FieldType.String)
            {
                bool retFlag = true;
                for (int i = 0; i < instance.AsBytes.Length; i++)
                {
                    if (instance.AsBytes[i] != 0X00 && instance.AsBytes[i] != 0X20)
                    {
                        retFlag = false;
                        break;
                    }
                }
                return retFlag;
            }
            else
                return instance.AsString().Trim().Length == 0;
        }

        /// <summary>
        /// Returns <c>true</c> if the object's value text contains a character 
        /// that is a non-space.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns <c>true</c> if the object's value text contains a character 
        /// that is a non-space.</returns>
        public static bool IsNotSpaces(this IField instance)
        {
            return !instance.IsSpaces();
        }

        /// <summary>
        /// Returns <c>true</c> if the object's value represents zero. 
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns <c>true</c> if the object's value represents zero.</returns>
        public static bool IsZeroes(this IField instance)
        {
            bool result = false;
            if (instance == null)
                return result;
            if (instance.IsNumericType || instance.FieldType == FieldType.NumericEdited)
            {
                string testString = instance.AsString();
                Decimal testDec;
                if (Decimal.TryParse(testString, out testDec))
                {
                    result = (testDec == 0);
                }
                else
                {
                    result = false;
                }
                //Following code added to return false if field does not contan decimal digits but a decimal is included in the test value - Bug 969
                if (instance.DecimalDigits == 0 && testString.Contains("."))
                {
                    result = false;
                }
            }
            else if (instance.FieldType == FieldType.String)
            {
                result = true;
                byte[] instanceBytes = instance.AsBytes;
                for (int x = 0; x < instance.LengthInBuffer; x++)
                {
                    if (instanceBytes[x] != '0')
                    {
                        result = false;
                        break;
                    }
                }
            }
            else if (instance.FieldType == FieldType.Boolean)
            {
                throw new NotImplementedException();
            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> if the object's value represents zero.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns <c>true</c> if the object's value represents zero.</returns>
        public static bool IsZeros(this IField instance)
        {
            return instance.IsZeroes();
        }

        ///// <summary>
        ///// Returns <c>true</c> if the IField object contains a text value 
        ///// that contains only the standard English alphabetic character set. 
        ///// </summary>
        //public static bool IsLetters(this IField instance)
        //{
        //    bool returnValue = true;
        //    foreach (char c in instance.AsString())
        //    {
        //        // We can't logically extend "IsLetters" to allow for spaces. 
        //        // additionally, the unit test for this case was failing. It should have raised flags. It should have been updated.
        //        if (c == ' ')
        //            throw new Exception("IsLetters() was improperly extended to allow space characters. To allow for spaces, call IsLettersOrSpace() instead.");
        //        if (!alphaOnlyChars.Contains(c))
        //        {
        //            returnValue = false;
        //            break;
        //        }
        //    }
        //}

        //TODO: this is an issue, by adding support for spaces, we now have a method which behaves
        // contrary to its name, xml doc, and unit test. We really should find a different approach 
        // if allowing spaces is required. (see IsLettersOrSpace()).
        // Additionally, the default includeSpaces param is provided to begin addressing this issue 
        // without breaking existing code, but it really should not default to true.
        /// <summary>
        /// Returns <c>true</c> if the IField object contains a text value 
        /// that contains only the standard English alphabetic character set. 
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="includeSpaces = true">Optional parameter that specifies whether space characters should be checked as well. Default value is true.</param>
        /// <returns>Returns <c>true</c> if the IField object contains a text value 
        /// that contains only the standard English alphabetic character set and/or optionally contains spaces.</returns>
        public static bool IsLetters(this IField instance, bool includeSpaces = true)
        {
            if (instance == null)
                return false;
            bool returnValue = true;
            foreach (char c in instance.AsString())
            {
                if (includeSpaces && c == ' ')
                    continue;

                if (!alphaOnlyChars.Contains(c))
                {
                    returnValue = false;
                    break;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Returns <c>true</c> if the IField object contains a text value 
        /// that contains only the standard English alphabetic character set - 
        /// or a space character.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns <c>true</c> if the IField object contains a text value 
        /// that contains only the standard English alphabetic character set - 
        /// or a space character.</returns>
        public static bool IsLettersOrSpace(this IField instance)
        {
            bool returnValue = true;
            string fieldText = instance.AsString();

            for (int i = 0; i < fieldText.Length; i++)
            {
                if (fieldText[i] == ' ')
                    continue;
                if (!alphaOnlyChars.Contains(fieldText[i]))
                {
                    returnValue = false;
                    break;
                }
            }
            return returnValue;
        }

        ///// <summary>
        ///// Returns <c>true</c> if the IField object contains a text value 
        ///// that contains only the standard English upper alphabetic character set. 
        ///// </summary>
        //public static bool IsAlphaUpper(this IField instance)
        //{
        //    bool returnValue = true;
        //    foreach (char c in instance.AsString())
        //    {
        //        if (c == ' ')
        //            throw new Exception("IsAlphaUpper() was improperly extended to allow space characters. To allow for spaces, call IsAlphaUpperOrSpace() instead.");
        //        if (!alphaUpperOnlyChars.Contains(c))
        //        {
        //            returnValue = false;
        //            break;
        //        }
        //    }
        //    return returnValue;
        //}

        //TODO: this is an issue, by adding support for spaces, we now have a method which behaves
        // contrary to its name, xml doc, and unit test. We really should find a different approach 
        // if allowing spaces is required. (see IsAlphaUpperOrSpace()).
        // Additionally, the default includeSpaces param is provided to begin addressing this issue 
        // without breaking existing code, but it really should not default to true.
        /// <summary>
        /// Returns <c>true</c> if the IField object contains a text value 
        /// that contains only the standard English upper alphabetic character set. 
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="includeSpaces = true">Optional paramter. Specifies whether the valuse shoud be also examined for spaces. Default value is true.</param>
        /// <returns>Returns Returns <c>true</c> if the IField object contains a text value 
        /// that contains only the standard English upper alphabetic character set and/or space characters.</returns>
        public static bool IsAlphaUpper(this IField instance, bool includeSpaces = true)
        {
            bool returnValue = true;
            foreach (char c in instance.AsString())
            {
                if (includeSpaces && c == ' ')
                    continue; // - talk to Robert about this; we can't logically extend "IsAlphaUpper" to allow for spaces. 
                if (!alphaUpperOnlyChars.Contains(c))
                {
                    returnValue = false;
                    break;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Returns <c>true</c> if the IField object contains a text value 
        /// that contains only the standard English upper alphabetic character set -
        /// or a space character.. 
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns <c>true</c> if the IField object contains a text value 
        /// that contains only the standard English upper alphabetic character set -
        /// or a space character.</returns>
        public static bool IsAlphaUpperOrSpace(this IField instance)
        {
            bool returnValue = true;
            foreach (char c in instance.AsString())
            {
                if (c == ' ')
                    continue;
                if (!alphaUpperOnlyChars.Contains(c))
                {
                    returnValue = false;
                    break;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Returns <c>true</c> if the IField object contains a text value 
        /// that contains only letters and numbers (no special characters). 
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns <c>true</c> if the IField object contains a text value 
        /// that contains only letters and numbers (no special characters).</returns>
        public static bool IsAlphanumeric(this IField instance)
        {
            var alphaNumChars = alphaOnlyChars.Concat(numericOnlyChars);
            return instance
                .GetValue<string>()
                .Where(c => !alphaNumChars.Contains(c))
                .Count()
                .Equals(0);
        }

        /// <summary>
        /// Returns <c>true</c> if the IField object contains a text value 
        /// that contains only letters and numbers (no special characters). 
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="includeSpaces = true">Optional paramter. Specifies whether the valuse shoud be also examined for spaces. Default value is true.</param>
        /// <returns>Returns <c>true</c> if the IField object contains a text value 
        /// that contains only letters and numbers (no special characters) and/or space characters.</returns>
        public static bool IsAlphabetic(this IField instance, bool includeSpaces = true)
        {
            bool returnValue = true;
            foreach (char c in instance.AsString())
            {
                if (c == ' ')
                    continue; //- talk to Robert about this; we can't logically extend "IsAlphabetic" to allow for spaces. 
                //throw new Exception("IsAlphabetic() was improperly extended to allow space characters. To allow for spaces, call IsAlphabeticOrSpace() instead.");
                if (!alphaOnlyChars.Contains(c))
                {
                    returnValue = false;
                    break;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Returns <c>true</c> if the IField object contains a text value 
        /// that contains only letters and numbers (no special characters) - 
        /// or a space character. 
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns <c>true</c> if the IField object contains a text value 
        /// that contains only letters and numbers (no special characters) - 
        /// or a space character.</returns>
        public static bool IsAlphabeticOrSpace(this IField instance)
        {
            bool returnValue = true;
            foreach (char c in instance.AsString())
            {
                if (c == ' ')
                    continue;
                if (!alphaOnlyChars.Contains(c))
                {
                    returnValue = false;
                    break;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Checks whether the current object value is numeric.
        /// </summary>
        /// <param name="instance">A reference to the current value.</param>
        /// <returns>Returns true if the current object value is numeric.</returns>
        public static bool IsNumericValue(this IField instance)
        {
            bool result = false;

            if (instance == null)
                result = false;
            else
            {
                switch (BufferServices.Directives.FieldValueMoves)
                {
                    case FieldValueMoveType.Undefined:
                    case FieldValueMoveType.CobolMoves:
                        //if (instance.GetValue<string>().EndsWith(" "))
                        bool instanceValidated = false;
                        if (instance.FieldType == FieldType.Binary)
                        {
                            var input = instance.BytesAsString.Trim();
                            result = Regex.IsMatch(input, $@"\d{{{input.Length}}}");
                        }
                        else
                        {
                            if (instance.BytesAsString.EndsWith(" ")
                                  && ((instance.FieldType != FieldType.CompInt && instance.FieldType != FieldType.CompLong && instance.FieldType != FieldType.CompShort
                                  && instance.FieldType != FieldType.PackedDecimal) && instance.IsNumericType))
                            {
                                string st = instance.BytesAsString;
                                if (instance.FieldType == FieldType.SignedNumeric || instance.FieldType == FieldType.SignedDecimal)
                                {
                                    if (st.Contains("+") || st.Contains("-"))
                                    {
                                        st = st.Replace("+", " ").Replace("-", " ");
                                    }
                                }
                                if (!st.Replace(DecimalSeparator, "").Trim().All(Char.IsDigit))
                                    return false;
                                else
                                    instanceValidated = true;
                            }
                        }
                        if (!instanceValidated && instance.BytesAsString.EndsWith(" ") && (instance.FieldType != FieldType.CompInt && instance.FieldType != FieldType.CompLong && instance.FieldType != FieldType.CompShort))
                            return false;
                        else
                        {
                            if (instance.FieldType == FieldType.String)
                            {
                                var input = instance.BytesAsString;
                                result = Regex.IsMatch(input, $@"\d{{{input.Length}}}");
                            }
                            else
                            {
                                if (instance.FieldType == FieldType.PackedDecimal || instance.FieldType == FieldType.UnsignedPackedDecimal)
                                {
                                    if (instance.IsMinValue())
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        decimal pDec;
                                        result = decimal.TryParse(instance.GetValue<string>(), out pDec);
                                        break;
                                    }
                                }
                                else if (instance.FieldType == FieldType.UnsignedDecimal || instance.FieldType == FieldType.UnsignedNumeric)  //Check added for Unsigned Numeric if '-' or '+' is present - JETRO issue 1079
                                {
                                    if (instance.BytesAsString.Contains("-") || instance.BytesAsString.Contains("+"))
                                    {
                                        return false;
                                    }
                                }

                                decimal num;
                                result = decimal.TryParse(instance.GetValue<string>(), out num);
                            }
                        }
                        break;
                    case FieldValueMoveType.AdsoMoves:
                        if (instance.FieldType == FieldType.Binary)
                        {
                            var input = instance.BytesAsString.Trim();
                            result = Regex.IsMatch(input, $@"\d{{{input.Length}}}");
                        }
                        else
                        {
                            decimal tmp;
                            result = decimal.TryParse(instance.GetValue<string>(), out tmp);
                        }
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> if the field's value can be converted to a boolean true. This is semantically the same 
        /// as <c>myField.GetValue&lt;bool&gt;()</c>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object value corresponds to the boolean true value.</returns>
        public static bool IsTrue(this IField instance)
        {
            return instance.GetValue<bool>();
        }

        /// <summary>
        /// Checks whether the current object value is negative.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object value is negative.</returns>
        public static bool IsNegative(this IField instance)
        {
            return (instance.GetValue<Decimal>() < 0);
        }

        /// <summary>
        /// Checks whether the current object is positive.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object value is positive.</returns>
        public static bool IsPositive(this IField instance)
        {
            return (instance.GetValue<Decimal>() >= 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the field object's value is in range (inclusive) of the <paramref name="loBound"/> and 
        /// <paramref name="hiBound"/> values.
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="loBound">Specifies the low boundary value.</param>
        /// <param name="hiBound">Specifies the high boundary value.</param>
        /// </summary>
        /// <returns>Returns true if the current object value is in the specified range.</returns>
        public static bool IsInRange(this IField instance, int loBound, int hiBound)
        {
            int value = instance.AsInt();
            return (value >= loBound && value <= hiBound);
        }

        /// <summary>
        /// Checks whether the current object value matches the specified mask.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="mask">Specifies mask value.</param>
        /// <returns>Returns true if the current object value matches the specified mask.</returns>
        public static bool Matches(this IField instance, string mask)
        {
            bool result = true;

            string fieldValue = instance.GetValue<string>();
            int length = Math.Min(fieldValue.Length, mask.Length);
            int i = 0;

            while (result && (i < length))
            {
                char maskChar = mask[i];
                char fieldChar = fieldValue[i];

                // '*' matches anything. 
                if (maskChar != '*')
                {
                    if (maskChar == '@')
                    {
                        result = (fieldChar == ' ' || Char.IsLetter(fieldChar));
                        // if IsLetter() causes trouble due to unicode issues, replace with: 
                        //result = ((fieldChar >= 65 && fieldChar <= 90) || (fieldChar >= 97 && fieldChar <= 122));
                    }
                    else if (maskChar == '#')
                    {
                        result = Char.IsDigit(fieldChar);
                        // if IsDigit() causes trouble due to unicode issues, replace with:
                        //result = (fieldChar >= 48 && fieldChar <= 57);
                    }
                    else
                    {
                        result = maskChar.Equals(fieldChar);
                    }
                }

                i++;
            }

            return result;
        }

        /// <summary>
        /// Checks whether the current object value matches the specified mask.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="maskField"></param>
        /// <returns></returns>
        public static bool Matches(this IField instance, IField maskField)
        {
            return Matches(instance, maskField.AsString());
        }
        /// <summary>
        /// Checks whether the current object value contains only the specified byte value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="byteValue">Specifies byte value for comparison.</param>
        /// <returns>Returns true if the current object value contains only specifed byte value.</returns>
        public static bool ContainsOnly(this IField instance, byte byteValue)
        {
            if (instance != null && instance.AsBytes != null && instance.AsBytes.Length > 0)
            {
                return instance.AsBytes.All(b => b.Equals(byteValue));
            }
            else
                return false;
        }

        /// <summary>
        /// Checks whether the current object value contains maximum value.
        /// </summary>
        /// <param name="instance">A reference to the current value.</param>
        /// <returns>Returns true if the current object value contains maximum value.</returns>
        public static bool IsMaxValue(this IField instance)
        {
            return ContainsOnly(instance, byte.MaxValue);
        }

        /// <summary>
        /// Checks whether the current object value contains minimum value.
        /// </summary>
        /// <param name="instance">A reference to the current value.</param>
        /// <returns>Returns true if the current object value contains minimum value.</returns>
        public static bool IsMinValue(this IField instance)
        {
            return ContainsOnly(instance, byte.MinValue);
        }

        /// <summary>
        /// Checks whether the current object value does not contain maximum value.
        /// </summary>
        /// <param name="instance">A reference to the current value.</param>
        /// <returns>Returns true if the current object value does not contain maximum value.</returns>
        public static bool IsNotMaxValue(this IField instance)
        {
            return !(IsMaxValue(instance));
        }

        /// <summary>
        /// Checks whether the current object value does not contain minimum value.
        /// </summary>
        /// <param name="instance">A reference to the current value.</param>
        /// <returns>Returns true if the current object value does not contain minimum value.</returns>
        public static bool IsNotMinValue(this IField instance)
        {
            return !(IsMinValue(instance));
        }


        #endregion

        #region new CheckFields
        /// <summary>
        /// Creates a new check field object associated with this field object. The new check field 
        /// evaluates whether the field's value is within the range defined by <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// </summary>
        /// <param name="instance">The field object.</param>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="loBound">Indicates the low boundary of the range.</param>
        /// <param name="hiBound">Indicates the high boundary of the range.</param>
        /// <param name="inclusive">If <c>true</c> (the default value), the check evaluation includes the <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// See example.</param>
        /// <example>if <c>inclusive == true</c>, the evaluation is performed as 
        /// <c>(value &gt;= loBonud &amp;&amp; value &lt;= hiBound)</c>.
        /// If <c>inclusive == false</c>, the evaluation is performed as 
        /// <c>(value &gt; loBonud &amp;&amp; value &lt; hiBound)</c>.</example>
        /// <returns>The IField object.</returns>
        public static IField NewCheckFieldRange(this IField instance, string name, int loBound, int hiBound, bool inclusive = true)
        {
            CreateNewCheckFieldRange(instance, name, loBound, hiBound, inclusive);
            return instance;
        }

        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// evaluates whether the field's value is within the range defined by <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// </summary>
        /// <param name="instance">The field object.</param>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="loBound">Indicates the low boundary of the range.</param>
        /// <param name="hiBound">Indicates the high boundary of the range.</param>
        /// <param name="inclusive">If <c>true</c> (the default value), the check evaluation includes the <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// See example.</param>
        /// <example>if <c>inclusive == true</c>, the evaluation is performed as 
        /// <c>(value &gt;= loBonud &amp;&amp; value &lt;= hiBound)</c>.
        /// If <c>inclusive == false</c>, the evaluation is performed as 
        /// <c>(value &gt; loBonud &amp;&amp; value &lt; hiBound)</c>.</example>
        /// <returns>The new checkfield object.</returns>
        public static ICheckField CreateNewCheckFieldRange(this IField instance, string name, int loBound, int hiBound, bool inclusive = true)
        {
            int lo = inclusive ? loBound : loBound + 1;
            int hi = inclusive ? hiBound : hiBound - 1;

            return instance.CreateNewCheckField(name, f => f.IsInRange(lo, hi));
        }


        /// <summary>
        /// Creates a new check field object associated with this field object. The new check field 
        /// evaluates whether the field's value is within the range defined by <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// </summary>
        /// <param name="instance">The field object.</param>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="loBound">Indicates the low boundary of the range.</param>
        /// <param name="hiBound">Indicates the high boundary of the range.</param>
        /// <param name="inclusive">If <c>true</c> (the default value), the check evaluation includes the <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// See example.</param>
        /// <example>if <c>inclusive == true</c>, the evaluation is performed as 
        /// <c>(value &gt;= loBonud &amp;&amp; value &lt;= hiBound)</c>.
        /// If <c>inclusive == false</c>, the evaluation is performed as 
        /// <c>(value &gt; loBonud &amp;&amp; value &lt; hiBound)</c>.</example>
        /// <returns>The IField object.</returns>
        public static IField NewCheckFieldRange(this IField instance, string name, decimal loBound, decimal hiBound, bool inclusive = true)
        {
            CreateNewCheckFieldRange(instance, name, loBound, hiBound, inclusive);
            return instance;
        }

        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// evaluates whether the field's value is within the range defined by <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// </summary>
        /// <param name="instance">The field object.</param>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="loBound">Indicates the low boundary of the range.</param>
        /// <param name="hiBound">Indicates the high boundary of the range.</param>
        /// <param name="inclusive">If <c>true</c> (the default value), the check evaluation includes the <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// See example.</param>
        /// <example>if <c>inclusive == true</c>, the evaluation is performed as 
        /// <c>(value &gt;= loBonud &amp;&amp; value &lt;= hiBound)</c>.
        /// If <c>inclusive == false</c>, the evaluation is performed as 
        /// <c>(value &gt; loBonud &amp;&amp; value &lt; hiBound)</c>.</example>
        /// <returns>The new checkfield object.</returns>
        public static ICheckField CreateNewCheckFieldRange(this IField instance, string name, decimal loBound, decimal hiBound, bool inclusive = true)
        {
            Func<IField, bool> expr;
            if (inclusive)
                expr = (f => (f.GetValue<decimal>() >= loBound) && (f.GetValue<decimal>() <= hiBound));
            else
                expr = (f => (f.GetValue<decimal>() > loBound) && (f.GetValue<decimal>() < hiBound));

            return instance.CreateNewCheckField(name, expr);
        }

        /// <summary>
        /// Creates a new check field object associated with this field object. The new check field 
        /// evaluates whether the field's value is within the range defined by <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// </summary>
        /// <param name="instance">The field object.</param>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="loBound">Indicates the low boundary of the range.</param>
        /// <param name="hiBound">Indicates the high boundary of the range.</param>
        /// <param name="inclusive">If <c>true</c> (the default value), the check evaluation includes the <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// See example.</param>
        /// <example>if <c>inclusive == true</c>, the evaluation is performed as 
        /// <c>(value &gt;= loBonud &amp;&amp; value &lt;= hiBound)</c>.
        /// If <c>inclusive == false</c>, the evaluation is performed as 
        /// <c>(value &gt; loBonud &amp;&amp; value &lt; hiBound)</c>.</example>
        /// <returns>The IField object.</returns>
        public static IField NewCheckFieldRange(this IField instance, string name, string loBound, string hiBound, bool inclusive = true)
        {
            CreateNewCheckFieldRange(instance, name, loBound, hiBound, inclusive);
            return instance;
        }

        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// evaluates whether the field's value is within the range defined by <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// </summary>
        /// <param name="instance">The field object.</param>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="loBound">Indicates the low boundary of the range.</param>
        /// <param name="hiBound">Indicates the high boundary of the range.</param>
        /// <param name="inclusive">If <c>true</c> (the default value), the check evaluation includes the <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// See example.</param>
        /// <example>if <c>inclusive == true</c>, the evaluation is performed as 
        /// <c>(value &gt;= loBonud &amp;&amp; value &lt;= hiBound)</c>.
        /// If <c>inclusive == false</c>, the evaluation is performed as 
        /// <c>(value &gt; loBonud &amp;&amp; value &lt; hiBound)</c>.</example>
        /// <returns>The new checkfield object.</returns>
        public static ICheckField CreateNewCheckFieldRange(this IField instance, string name, string loBound, string hiBound, bool inclusive = true)
        {
            Func<IField, bool> expr;
            if (inclusive)
                expr = (f => (f.GetValue<string>().CompareTo(loBound) >= 0) && (f.GetValue<string>().CompareTo(hiBound) <= 0));
            else
                expr = (f => (f.GetValue<string>().CompareTo(loBound) > 0) && (f.GetValue<string>().CompareTo(hiBound) < 0));

            return instance.CreateNewCheckField(name, expr);
        }


        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// evaluates whether the field's value is within the range defined by <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// </summary>
        /// <param name="instance">The field object.</param>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="loBonud">Indicates the low boundary of the range.</param>
        /// <param name="hiBound">Indicates the high boundary of the range.</param>
        /// <returns>The new checkfield object.</returns>
        public static IField NewCheckFieldRange(this IField instance, string name, char loBonud, char hiBound)
        {
            CreateNewCheckFieldRange(instance, name, loBonud, hiBound);
            return instance;
        }

        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// evaluates whether the field's value is within the range defined by <paramref name="loBound"/> and <paramref name="hiBound"/>.
        /// </summary>
        /// <param name="instance">The field object.</param>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="loBonud">Indicates the low boundary of the range.</param>
        /// <param name="hiBound">Indicates the high boundary of the range.</param>
        /// <returns>The new checkfield object.</returns>
        public static ICheckField CreateNewCheckFieldRange(this IField instance, string name, char loBonud, char hiBound)
        {
            if (instance.DisplayLength > 1)
                throw new ArgumentException("For this check field, field length must be 1.", "instance");

            AsciiChar lo = AsciiChar.From(loBonud);
            AsciiChar hi = AsciiChar.From(hiBound);

            return instance.CreateNewCheckField(name, f => ((f.AsBytes[0] >= lo.AsByte) && (f.AsBytes[0] <= hi.AsByte)));
        }


        /// <summary>
        /// Creates a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="instance">The field object.</param>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">The values for which to check the field value.</param>
        /// <returns>This field object.</returns>
        public static IField NewCheckField(this IField instance, string name, params int[] values)
        {
            CreateNewCheckField(instance, name, values);
            return instance;
        }

        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="instance">The field object.</param>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">The values for which to check the field value.</param>
        /// <returns>The new checkfield object.</returns>
        public static ICheckField CreateNewCheckField(this IField instance, string name, params int[] values)
        {
            return instance.CreateNewCheckField(name, f => values.Contains(f.AsInt()));
        }

        /// <summary>
        /// Creates a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="instance">The field object.</param>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">The values for which to check the field value.</param>
        /// <returns>This field object.</returns>
        public static IField NewCheckField(this IField instance, string name, params decimal[] values)
        {
            CreateNewCheckField(instance, name, values);
            return instance;
        }

        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="instance">The field object.</param>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">The values for which to check the field value.</param>
        /// <returns>The new checkfield object.</returns>
        public static ICheckField CreateNewCheckField(this IField instance, string name, params decimal[] values)
        {
            return instance.CreateNewCheckField(name, f => values.Contains(f.AsInt()));
        }


        #endregion

        #region IsEqualTo
        /// <summary>
        /// Returns true if the value of this field is equal to the value of the <paramref name="other"/> field.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if both fields are <c>null</c>, if both fields refer to the same object instance, or 
        /// if the two field values are functionally equivalent. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this IField instance, IField other)
        {
            if (instance == null)
                return false;
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");
            if (instance.IsNumericType && other.IsNumericValue() && instance.IsNumericValue())
            {
                return (instance.AsDecimal() == other.AsDecimal());
            }
            else
                return instance.Equals(other);
        }


        /// <summary>
        /// Returns true if the value of this field is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this IField instance, string other)
        {
            if (instance == null)
                return false;
            if (!(instance is IEquatable<string>))
                throw new NotImplementedException();

            return instance.Equals(other);
        }

        /// <summary>
        /// Returns true if the value of this field is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this IField instance, IGroup other)
        {
            if (instance == null)
                return false;
            if (!(instance is IEquatable<string>))
                throw new NotImplementedException();
            if (instance.IsNumericType && other.IsNumericValue() && instance.IsNumericValue())
            {
                return (instance.AsDecimal() == other.AsDecimal());
            }
            else
                return instance.Equals(other);
            //return (instance.BytesAsString == other.BytesAsString);
        }


        /// <summary>
        /// Returns true if the value of this field is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this IField instance, PackedDecimal other)
        {
            if (!(instance is IEquatable<PackedDecimal>))
                throw new NotImplementedException();

            return instance.Equals(other);
        }

        /// <summary>
        /// Returns true if the value of this field is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this IField instance, bool other)
        {
            if (!(instance is IEquatable<bool>))
                throw new NotImplementedException();

            return instance.Equals(other);
        }


        /// <summary>
        /// Returns true if the value of this field is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this IField instance, Decimal other)
        {
            //if (!(instance is IEquatable<Decimal>))
            //    throw new NotImplementedException();
            try
            {
                return instance.Equals(other);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the value of this field is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this IField instance, Int64 other)
        {
            //if (!(instance is IEquatable<Int64>))
            //    throw new NotImplementedException();
            try
            {
                return instance.Equals(other);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the value of this field is equal to the value of <paramref name="other"/>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="other">Specifies a reference to another IRecord object for comparison.</param>
        /// <returns>Returns true if the value of this field is equal to the value of <paramref name="other"/>.</returns>
        public static bool IsEqualTo(this IField instance, IRecord other)
        {
            if (instance == null)
                return false;
            if (!(instance is IEquatable<string>))
                throw new NotImplementedException();

            return instance.Equals(other);
            //return instance.Equals(other.AsString());
        }

        /// <summary>
        /// Returns true if all characters in this field match the passed character.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="other">Specifies a string for comparison.</param>
        /// <returns>Returns true if all characters in this field match the passed character.</returns>
        public static bool IsEqualToAll(this IField instance, string other)
        {
            if (!(instance is IEquatable<string>))
                throw new NotImplementedException();

            return instance.ContainsOnly(other[0]);
        }

        #endregion

        #region IsLessThan

        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this IField instance, IField other)
        {
            if (instance == null || other == null)
                return false;
            if (other.IsNumericType && other.IsNumericValue() && instance.IsNumericValue())
            {
                return (instance.AsDecimal() < other.AsDecimal());
            }
            else
                return (instance.CompareTo(other) < 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this IField instance, IGroup other)
        {
            if (instance == null || other == null)
                return false;
            if (instance.IsNumericType && other.IsNumericValue() && instance.IsNumericValue())
            {
                return (instance.AsDecimal() < other.AsDecimal());
            }
            else
                return (instance.CompareTo(other) < 0);
            //return (instance.CompareTo(other.BytesAsString) < 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this IField instance, string other)
        {
            if (instance == null)
                return false;
            if (!(instance is IComparable<string>))
                throw new NotImplementedException();

            return (instance.CompareTo(other) < 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this IField instance, PackedDecimal other)
        {
            if (!(instance is IComparable<PackedDecimal>))
                throw new NotImplementedException();

            return (instance.CompareTo(other) < 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this IField instance, Decimal other)
        {
            if (instance.IsNumericOnlyValue)
            {
                return (instance.AsDecimal() < other);
            }
            else
            {
                return IsLessThan(instance, other.ToString());
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this IField instance, Int64 other)
        {

            if (instance.IsNumericOnlyValue)
            {
                return (instance.AsDecimal() < (decimal)other);
            }
            else
            {
                return IsLessThan(instance, other.ToString());
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than MaxValue
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// MaxValue. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanMaxValue(this IField instance)
        {
            return instance.CompareTo(byte.MaxValue) < 0;
        }

        /// <summary>
        /// Returns true if instance value is less than a string fill with all of a certain character
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="allValue"></param>
        /// <returns></returns>
        public static bool IsLessThanAll(this IField instance, string allValue)
        {
            return instance.CompareTo(string.Empty.PadLeft(instance.LengthInBuffer, allValue[0])) < 0;
        }

        #endregion

        #region IsGreaterThan
        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this IField instance, IField other)
        {
            if (instance == null)
                return false;
            bool result;
            if (other.IsNumericType && other.IsNumericValue() && instance.IsNumericValue())
            {
                result = (instance.AsDecimal() > other.AsDecimal());
            }
            else
            {
                result = (instance.CompareTo(other) > 0);
            }
            return result;
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this IField instance, IGroup other)
        {
            if (instance == null)
                return false;
            if (instance.IsNumericType && other.IsNumericValue() && instance.IsNumericValue())
            {
                return (instance.AsDecimal() > other.AsDecimal());
            }
            else
                return (instance.CompareTo(other) > 0);
            //return (instance.CompareTo(other.BytesAsString) > 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this IField instance, string other)
        {
            if (instance == null)
                return false;
            if (!(instance is IComparable<string>))
                throw new NotImplementedException();

            return (instance.CompareTo(other) > 0);
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this IField instance, PackedDecimal other)
        {
            if (!(instance is IComparable<PackedDecimal>))
                throw new NotImplementedException();

            return (instance.CompareTo(other) > 0);
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this IField instance, Decimal other)
        {
            if (instance.IsNumericOnlyValue)
            {
                return (instance.AsDecimal() > other);
            }
            else
            {
                return IsGreaterThan(instance, other.ToString());
            }
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this IField instance, Int64 other)
        {
            if (instance.IsNumericOnlyValue)
            {
                if (instance.DecimalDigits > 0)
                    return (instance.AsDecimal() > (decimal)other);
                else
                    return (((Int64)instance.AsDecimal()) > other);
            }
            else
            {
                return IsGreaterThan(instance, other.ToString());
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than MinValue
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// MinValue. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanMinValue(this IField instance)
        {
            return instance.CompareTo(byte.MinValue) > 0;
        }
        /// <summary>
        /// Returns true if instance value is greater than a string fill with all of a certain character
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="allValue"></param>
        /// <returns></returns>
        public static bool IsGreaterThanAll(this IField instance, string allValue)
        {
            return instance.CompareTo(string.Empty.PadLeft(instance.LengthInBuffer, allValue[0])) > 0;
        }

        #endregion

        #region IsLessThanOrEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this IField instance, IField other)
        {
            if (instance == null)
                return true;
            if (other.IsNumericType && other.IsNumericValue() && instance.IsNumericValue())
            {
                return (instance.AsDecimal() <= other.AsDecimal());
            }
            else
                return (instance.CompareTo(other) <= 0);
        }
        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this IField instance, IGroup other)
        {
            if (instance == null)
                return true;
            if (instance.IsNumericType && other.IsNumericValue() && instance.IsNumericValue())
            {
                return (instance.AsDecimal() <= other.AsDecimal());
            }
            else
                return (instance.CompareTo(other) <= 0);
            //return (instance.CompareTo(other.BytesAsString) <= 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this IField instance, string other)
        {
            if (instance == null)
                return true;
            return (instance.CompareTo(other) <= 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this IField instance, PackedDecimal other)
        {
            if (!(instance is IComparable<PackedDecimal>))
                throw new NotImplementedException();

            return (instance.CompareTo(other) <= 0);
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this IField instance, Decimal other)
        {
            if (instance.IsNumericOnlyValue)
            {
                return (instance.AsDecimal() <= other);
            }
            else
            {
                return IsLessThanOrEqualTo(instance, other.ToString());
            }

        }


        /// <summary>
        /// Returns <c>true</c> if the value of this field is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this IField instance, Int64 other)
        {
            if (instance.IsNumericOnlyValue)
            {
                return (instance.AsDecimal() <= (decimal)other);
            }
            else
            {
                return IsLessThanOrEqualTo(instance, other.ToString());
            }
        }




        #endregion

        #region IsGreaterThanOrEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this IField instance, IField other)
        {
            if (instance == null)
                return false;

            if (other.IsNumericValue() && instance.IsNumericValue() && (other.IsNumericType || instance.IsNumericType))
            {
                return (instance.AsDecimal() >= other.AsDecimal());
            }
            else
                return (instance.CompareTo(other) >= 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this IField instance, IGroup other)
        {
            if (instance == null)
                return false;
            if (instance.IsNumericType && other.IsNumericValue() && instance.IsNumericValue())
            {
                return (instance.AsDecimal() >= other.AsDecimal());
            }
            else
                return (instance.CompareTo(other) >= 0);
            //return (instance.CompareTo(other.BytesAsString) >= 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this IField instance, string other)
        {
            if (instance == null)
                return false;
            if (!(instance is IComparable<string>))
                throw new NotImplementedException();

            return (instance.CompareTo(other) >= 0);
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this IField instance, PackedDecimal other)
        {
            if (!(instance is IComparable<PackedDecimal>))
                throw new NotImplementedException();

            if (instance.IsNumericType && instance.IsNumericValue())
            {
                return (instance.AsDecimal() >= other.Value);
            }
            else
                return (instance.CompareTo(other) >= 0);
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this IField instance, Decimal other)
        {

            if (instance.IsNumericOnlyValue)
            {
                return (instance.AsDecimal() >= other);
            }
            else
            {
                return IsGreaterThanOrEqualTo(instance, other.ToString());
            }
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this field is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this IField instance, Int64 other)
        {
            if (instance == null)
                return false;
            if (instance.IsNumericOnlyValue)
            {
                return (instance.AsDecimal() >= (decimal)other);
            }
            else
            {
                return IsGreaterThanOrEqualTo(instance, other.ToString());
            }
        }

        #endregion

        #region IsNotEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this field is not equal to the value of the <paramref name="other"/> field.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>false</c> if both fields are <c>null</c>, if both fields refer to the same object instance, or 
        /// if the two field values are functionally equivalent. Otherwise, returns <c>true</c>.</returns>
        public static bool IsNotEqualTo(this IField instance, IField other)
        {
            if (instance == null)
                return true;
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            return !(instance.Equals(other));
        }

        /// <summary>
        /// Checks whether the current object value is not equal to the provided IGroup object value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="other">A reference to the IGroup object for comparison.</param>
        /// <returns>Returns true if the current object value is not equal to the provided IGroup object value.</returns>
        public static bool IsNotEqualTo(this IField instance, IGroup other)
        {
            if (instance == null)
                return true;
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            return !(instance.Equals(other));
            //return !(instance.BytesAsString == other.BytesAsString);
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this field is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this IField instance, string other)
        {
            if (!(instance is IEquatable<string>))
                throw new NotImplementedException();

            return !(instance.Equals(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this IField instance, PackedDecimal other)
        {
            if (!(instance is IEquatable<PackedDecimal>))
                throw new NotImplementedException();

            return !(instance.Equals(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this IField instance, Decimal other)
        {
            return !(instance.Equals(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this IField instance, Int64 other)
        {
            // Fixed to same as IsEqualTo version for Issue CT-241, CT-243, CT-425 and CT-455
            try
            {
                return !(instance.Equals(other));
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Returns true if the value of this field is not equal to the value of <paramref name="other"/>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="other">A reference to the IRecord object for comparison.</param>
        /// <returns>Returns true if the value of this field is not equal to the value of <paramref name="other"/>.</returns>
        public static bool IsNotEqualTo(this IField instance, IRecord other)
        {
            if (instance == null)
                return false;
            if (!(instance is IEquatable<string>))
                throw new NotImplementedException();

            return !instance.Equals(other);
            //return !instance.Equals(other.AsString());
        }

        #endregion

        #region Arithmetic
        /// <summary>
        /// Add an integer to this instance value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">Integer value to be added.</param>
        /// <returns>Returns a reference to the current object with the resulted sum value.</returns>
        public static IField Add(this IField instance, int value)
        {
            if (instance == null)
                return instance;

            if (instance.DecimalDigits > 0)
                instance.Assign(instance.GetValue<Decimal>() + value);
            else
                instance.Assign(instance.GetValue<Int64>() + value);
            return instance;
        }

        /// <summary>
        /// Add a string number to this instance value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">String representation of the numeric value to be added.</param>
        /// <returns>Returns a reference to the current object with the resulted sum value.</returns>
        public static IField Add(this IField instance, string value)
        {
            instance.Assign(instance.GetValue<decimal>() + Convert.ToDecimal(value));
            return instance;
        }

        /// <summary>
        /// Add a decimal to this instance value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">Decimal value to be added.</param>
        /// <returns>Returns a reference to the current object with the resulted sum value.</returns>
        public static IField Add(this IField instance, decimal value)
        {
            instance.Assign(instance.GetValue<decimal>() + value);
            return instance;
        }

        /// <summary>
        /// Add an IField value to this instance value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">A reference to the IField object with the decimal value to be added.</param>
        /// <returns>Returns a reference to the current object with the resulted sum value.</returns>
        public static IField Add(this IField instance, IField value)
        {
            if (instance.DecimalDigits > 0)
                instance.Assign(instance.GetValue<decimal>() + value.AsDecimal());
            else
                instance.Assign(instance.GetValue<int>() + value.AsInt());
            return instance;
        }

        /// <summary>
        /// Subtract an integer to this instance value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">Integer value to be subtracted.</param>
        /// <returns>Returns a reference to the current object with the resulted subtraction value.</returns>
        public static IField Subtract(this IField instance, int value)
        {
            instance.Assign(instance.GetValue<int>() - value);
            return instance;
        }

        /// <summary>
        /// Subtract a decimal to this instance value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">Decimal value to be subtracted.</param>
        /// <returns>Returns a reference to the current object with the resulted subtraction value.</returns>
        public static IField Subtract(this IField instance, decimal value)
        {
            instance.Assign(instance.GetValue<decimal>() - value);
            return instance;
        }

        /// <summary>
        /// Subtract a string number to this instance value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">String representation of the numeric value to be subtracted.</param>
        /// <returns>Returns a reference to the current object with the resulted subtraction value.</returns>
        public static IField Subtract(this IField instance, string value)
        {
            instance.Assign(instance.GetValue<decimal>() - Convert.ToDecimal(value));
            return instance;
        }

        /// <summary>
        /// Subtract an IField value to this instance value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">A reference to the IField object with the numeric value for subtraction.</param>
        /// <returns>Returns a reference to the current object with the resulted subtraction value.</returns>
        public static IField Subtract(this IField instance, IField value)
        {
            if (instance.DecimalDigits > 0)
                instance.Assign(instance.GetValue<decimal>() - value.AsDecimal());
            else
                instance.Assign(instance.GetValue<int>() - value.AsInt());
            return instance;
        }
        #endregion

        #region set value
        /// <summary>
        /// Set field with High values
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetMaxValue(this IField instance)
        {
            instance.AssignFrom(MaxBytes(instance));
        }

        /// <summary>
        /// Set field with low Values
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetMinValue(this IField instance)
        {
            instance.AssignFrom(MinBytes(instance));
        }

        /// <summary>
        /// Set Computed Value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetComputeValue(this IField instance, decimal computedDecimal)
        {
            //decimal newDec = Math.Floor((decimal)computedDecimal * (decimal)Math.Pow(10, instance.DecimalDigits)) / (decimal)Math.Pow(10, instance.DecimalDigits);

            string strDec = computedDecimal.AsString();
            if (strDec.IndexOf(".") > 0)
            {
                if (instance.DecimalDigits == 0)
                    strDec = strDec.Substring(0, strDec.IndexOf("."));
                else if (strDec.Length >= (strDec.IndexOf(".") + instance.DecimalDigits + 1))
                {
                    strDec = strDec.Substring(0, strDec.IndexOf(".") + instance.DecimalDigits + 1);
                }
            }
            instance.SetValue(strDec);
        }

        /// <summary>
        /// Set Computed Value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetComputeValue(this IField instance, IField computedField)
        {
            //decimal newDec = Math.Floor((decimal)computedField.AsDecimal() * (decimal)Math.Pow(10, instance.DecimalDigits)) / (decimal)Math.Pow(10, instance.DecimalDigits);
            instance.SetValue(computedField.AsDecimal());
        }

        public static void SetComputeValue(this IField instance, string computedString)
        {
            instance.SetValue(computedString);
        }

        /// <summary>
        /// Set Computed RoundedValue
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetComputeRoundedValue(this IField instance, decimal computedDecimal)
        {
            instance.SetValue(decimal.Round(computedDecimal, instance.DecimalDigits, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// Set Computed RoundedValue
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetComputeRoundedValue(this IField instance, IField computedField)
        {
            instance.SetValue(decimal.Round(computedField.AsDecimal(), instance.DecimalDigits, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// Sets the value of a substring of the given field's value.
        /// </summary>
        /// <param name="instance">The field whose value will be altered.</param>
        /// <param name="startPosition">The beginning index of the field's substring to be assigned.</param>
        /// <param name="length">The length of the substring to be assigned.</param>
        /// <param name="replaceValue">The value from whom the new substring value will be retrieved.</param>
        /// <remarks>
        /// The value of <paramref name="instance"/>[<paramref name="startPosition"/>] 
        /// will be re-assigned from <paramref name="replaceValue"/>[0] for 
        /// <paramref name="length"/> number of characters.
        /// See example.
        /// </remarks>
        /// <example>
        /// Given that the value of <c>myField</c> is "0123456789", if we call:
        /// <code>
        /// // assume 0-based C#
        /// myField.SetValueOfSubstring(3, 4, "abcdefghij")
        /// </code>
        /// The value of <c>myField</c> should now be "012abcd789".
        /// </example>
        public static void SetValueOfSubstring(this IField instance, int startPosition, int length, string replaceValue)
        {
            try
            {
                // note: rather than just subtracting/adding 1 to values throughout the 
                // Framework, I'm encapsulating this behavior in the IIndexBaseServices service.
                // Additionally, the existing logic did not protect against replaceValue
                // being too long.
                //instance
                //    .Assign(instance.AsString()
                //        .Remove(startPosition - 1, length)
                //        .Insert(startPosition - 1, replaceValue.PadRight(length)));

                int idx = Indexes.ConvertedCodeIndexToCSharpIndex(startPosition);

                string value = replaceValue;
                if (replaceValue.Length > length)
                {
                    value = replaceValue.Substring(0, length);
                }
                else if (replaceValue.Length < length)
                {
                    value = replaceValue.PadRight(length);
                }

                string newValue = instance
                    .AsString()
                    .Remove(idx, length)
                    .Insert(idx, value);
                instance.Assign(newValue);
            }
            catch (Exception ex)
            {
                string exString = ex.Message;
                //exString += Environment.NewLine + ex.StackTrace;
                exString += Environment.NewLine + "      String='" + instance.AsString() + "'";
                exString += Environment.NewLine + "          At=" + startPosition;
                exString += Environment.NewLine + "         For=" + length;
                exString += Environment.NewLine + "Replace with='" + replaceValue + "'";

                throw new Exception(exString, ex);
            }
        }

        public static void SetValueOfSubstring(this IField instance, int startPosition, int length, int replaceValue)
        {
            try
            {

                int idx = Indexes.ConvertedCodeIndexToCSharpIndex(startPosition);
                string value = replaceValue.AsString();

                value = value.PadRight(length, replaceValue.AsString()[0]);

                string newValue = instance
                    .AsString()
                    .Remove(idx, length)
                    .Insert(idx, value);
                instance.Assign(newValue);
            }
            catch (Exception ex)
            {
                string exString = ex.Message;
                //exString += Environment.NewLine + ex.StackTrace;
                exString += Environment.NewLine + "      String='" + instance.AsString() + "'";
                exString += Environment.NewLine + "          At=" + startPosition;
                exString += Environment.NewLine + "         For=" + length;
                exString += Environment.NewLine + "Replace with='" + replaceValue + "'";

                throw new Exception(exString, ex);
            }
        }


        /// <summary>
        /// Sets the value of a substring of the given field's value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifiels start index.</param>
        /// <param name="length">Specifies the length of the substring.</param>
        /// <param name="replaceValue">A reference to the IField object with the value for the replacement.</param>
        public static void SetValueOfSubstring(this IField instance, int startPosition, int length, IField replaceValue)
        {
            // don't adjust index base here - it'll be done in the root SetValueOfSubstring().
            SetValueOfSubstring(instance, startPosition, length, replaceValue.AsString());
        }

        /// <summary>
        /// Sets the value of a substring of the given field's value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the length of the substring.</param>
        /// <param name="replaceValue">A reference to the IGroup object with the value for the replacement.</param>
        public static void SetValueOfSubstring(this IField instance, int startPosition, int length, IGroup replaceValue)
        {
            SetValueOfSubstring(instance, startPosition, length, replaceValue.AsString());
        }

        /// <summary>
        /// Sets the value of a substring of the given field's value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="lengthField">Specifies the length of the substring.</param>
        /// <param name="replaceField">A reference to the IField object with the value for the replacement.</param>
        public static void SetValueOfSubstring(this IField instance, int startPosition, IField lengthField, IField replaceField)
        {
            SetValueOfSubstring(instance, startPosition, lengthField.GetValue<int>(), replaceField.AsString());
        }

        /// <summary>
        /// Sets the value of a substring of the given field's value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="lengthField">Specifies the length of the substring.</param>
        /// <param name="replaceGroup">A reference to the IGroup object with the value for the replacement.</param>
        public static void SetValueOfSubstring(this IField instance, int startPosition, IField lengthField, IGroup replaceGroup)
        {
            SetValueOfSubstring(instance, startPosition, lengthField.GetValue<int>(), replaceGroup.AsString());
        }

        /// <summary>
        /// Sets the value of a substring of the given field's value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the length of the substring.</param>
        /// <param name="replaceField">A reference to the IField object with the value for the replacement.</param>
        public static void SetValueOfSubstring(this IField instance, IField startPosition, int length, IField replaceField)
        {
            SetValueOfSubstring(instance, startPosition.GetValue<int>(), length, replaceField.AsString());
        }

        /// <summary>
        /// Sets the value of a substring of the given field's value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the length of the substring.</param>
        /// <param name="replaceGroup">A reference to the IGroup object with the value for the replacement.</param>
        public static void SetValueOfSubstring(this IField instance, IField startPosition, int length, IGroup replaceGroup)
        {
            SetValueOfSubstring(instance, startPosition.GetValue<int>(), length, replaceGroup.AsString());
        }

        /// <summary>
        /// Sets the value of a substring of the given field's value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the length of the substring.</param>
        /// <param name="replaceGroup">A reference to the IField object with the value for the replacement.</param>
        public static void SetValueOfSubstring(this IField instance, IField startPosition, IField lengthField, IField replaceField)
        {
            SetValueOfSubstring(instance, startPosition.GetValue<int>(), lengthField.GetValue<int>(), replaceField.AsString());
        }

        /// <summary>
        /// Sets the value of a substring of the given field's value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the length of the substring.</param>
        /// <param name="replaceGroup">A reference to the IGroup object with the value for the replacement.</param>
        public static void SetValueOfSubstring(this IField instance, IField startPosition, IField lengthField, IGroup replaceGroup)
        {
            SetValueOfSubstring(instance, startPosition.GetValue<int>(), lengthField.GetValue<int>(), replaceGroup.AsString());
        }

        public static void SetValueOfSubstring(this IField instance, int startPosition, string replaceValue)
        {
            string newValue = string.Concat(instance.GetSubstring(1, startPosition - 1), replaceValue);
            instance.Assign(newValue);
        }

        public static void SetValueOfSubstringInspectReplacing(this IField instance, int startPosition, int length, string searchType, string searchFor, string searchReplace, string beforeOption, string beforeParm, string afterOption, string afterParm)
        {
            string valueToSearch = instance.GetSubstring(startPosition, length);
            string replaceValue = valueToSearch;
            #region InspectReplacing
            if (searchType == "LEADING")
            {
                char[] chars = valueToSearch.ToCharArray();
                bool stopSearch = false;
                for (int i = 0; i < chars.Length; i++)
                {
                    if (!stopSearch)
                    {
                        char searchChar = Convert.ToChar(searchFor);
                        char replaceChar = Convert.ToChar(searchReplace);
                        if ((searchChar == chars[i]))
                            chars[i] = replaceChar;
                        else
                            stopSearch = true;
                    }
                }
                replaceValue = Convert.ToString(chars);
            }
            else if (searchType == "ALL")
            {
                string search = valueToSearch;
                StringBuilder replacedSearch = new StringBuilder();
                for (int i = 0; i < search.Length; i++)
                {
                    string searchChar = search.Substring(i, 1);
                    int charIdx = 0;
                    charIdx = searchFor.IndexOf(searchChar);
                    if (charIdx == -1)
                    {
                        replacedSearch.Append(searchChar);
                    }
                    else
                    {
                        replacedSearch.Append(searchReplace.Substring(charIdx, 1));
                    }
                }
                replaceValue = replacedSearch.ToString();
            }
            else if (searchType == "FIRST")
            {
                instance.Assign(valueToSearch.ReplaceFirstOccurrance(searchFor, searchReplace));
            }
            #endregion

            string value = replaceValue;
            if (replaceValue.Length > length)
            {
                value = replaceValue.Substring(0, length);
            }
            else if (replaceValue.Length < length)
            {
                value = replaceValue.PadRight(length);
            }

            int idx = Indexes.ConvertedCodeIndexToCSharpIndex(startPosition);
            string newValue = instance
                .AsString()
                .Remove(idx, length)
                .Insert(idx, value);
            instance.Assign(newValue);
        }

        /// <summary>
        /// Set field Value with zeroes 
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetValueWithZeroes(this IField instance)
        {
            if (instance == null) return;
            if (instance.IsNumericType || instance.FieldType == FieldType.NumericEdited)
            {   // Issue 8245 - Reset IdRecordName when setting value to 0
                FieldEx.IdRecordName = "";
                instance.SetValue(0);
            }
            else
                instance.FillWithChar('0');
        }

        /// <summary>
        /// Set field Value with zeroes 
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetValueWithZeros(this IField instance)
        {
            SetValueWithZeroes(instance);
        }

        /// <summary>
        /// Sets string Value coming from DB2 database data after changing hex 9F to hex FF
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">String value to be assigned.</param>
        public static void SetValueFromDB2(this IField instance, string value)
        {
            if (value.Contains(AsciiChar.Db2ConnectHighValue.AsChar))
            {
                value = value.Replace(AsciiChar.Db2ConnectHighValue.AsChar, AsciiChar.MaxValue.AsChar);
            }
            instance.SetValue(value);
        }

        /// <summary>
        /// Returns string for sending to DB2 database after changing hex FF to Hex 9F
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static string AsStringForDB2(this IField instance)
        {
            if (instance.AsString().Contains(AsciiChar.MaxValue.AsChar))
            {
                return instance.AsString().Replace(AsciiChar.MaxValue.AsChar, AsciiChar.Db2ConnectHighValue.AsChar);
            }
            else
            {
                return instance.AsString();
            }
        }

        /// <summary>
        /// Inserts a string value into field based on pointer position and updates the pointer with ending position
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="stringValue"></param>
        /// <param name="pointer"></param>
        public static void SetStringValueWithPointer(this IField instance, string stringValue, IField pointer)
        {
            SetValueOfSubstring(instance, pointer.GetValue<int>(), stringValue);
            pointer.Add(stringValue.Length);
        }
        /// <summary>
        /// Sets the value of boolean fieldtype
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="boolValue">if set to <c>true</c> [bool value].</param>
        public static void SetValue(this IField instance, bool boolValue)
        {
            instance.Assign(boolValue);
        }
        /// <summary>
        /// Set Value if byte array is not null
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="bytes"></param>
        public static void SetValueWithNullCheck(this IField instance, byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
                instance.Assign(bytes);
        }
        #endregion

        #region Misc
        /// <summary>
        /// Replaces current object string value with the provided string value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="searchValue">The value to search for.</param>
        /// <param name="replaceValue">The value for the replacement.</param>
        /// <returns>Returns a reference to the current object that contains the updated value.</returns>
        public static IField Replace(this IField instance, string searchValue, string replaceValue)
        {
            instance.Assign(instance.AsString().Replace(searchValue, replaceValue));
            return instance;
        }

        public static IField Replace(this IField instance, string searchValue, IField replaceValue)
        {
            instance.Assign(instance.AsString().Replace(searchValue, replaceValue.AsString()));
            return instance;
        }

        public static IField Replace(this IField instance, IField searchValue, IField replaceValue)
        {
            instance.Assign(instance.AsString().Replace(searchValue.AsString(), replaceValue.AsString()));
            return instance;
        }

        /// <summary>
        /// Returns a substring of IField value (position based)
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the length of the substring.</param>
        /// <returns>Returns a reference to the current object that contains the updated value.</returns>
        public static string GetSubstring(this IField instance, int startPosition, int length)
        {
            if (instance == null) return "";

            string result = "";

            if ((startPosition - 1) + length < instance.DisplayLength)
                result = instance.AsString().Substring(startPosition - 1, length);
            else
                result = instance.AsString().Substring(startPosition - 1);

            //if (instance.FieldType == FieldType.UnsignedNumeric && result.Replace("0", "").Length == 0)  //MHM - Commented this out be uecause it is incorrect. Unsigned Numeric fields should always have numric digits
            //    result = result.Replace('0', ' ');

            return result;
        }

        /// <summary>
        /// Returns a substring of IField value (position based)
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <returns>Returns a reference to the current object that contains the updated value.</returns>
        public static string GetSubstring(this IField instance, int startPosition)
        {
            if (instance == null) return "";

            return instance.AsString().Substring(startPosition - 1);

        }

        /// <summary>
        /// Trims the specified Field string and returns a string
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public static string Trim(this IField instance)
        {
            if (instance == null) return "";

            return instance.AsString().Trim();

        }

        public static int Length(this IField instance)
        {
            if (instance == null) return 0;

            return instance.LengthInBuffer;
        }

        /// <summary>
        /// Converts string to decimal
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static decimal ToNumberValue(this IField instance)
        {
            if (instance == null) return 0;

            decimal tempDecimal = 0m;

            decimal.TryParse(instance.AsString().Replace("$", "").Trim(), out tempDecimal);

            return tempDecimal;

        }
        /// <summary>
        /// Returns a string of reversed characters form IField value
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string ReverseCharacters(this IField instance)
        {
            if (instance == null) return string.Empty;

            string tempString = instance.BytesAsString;
            StringBuilder sbString = new StringBuilder();
            for (int ctr = tempString.Length - 1; ctr >= 0; ctr--)
            {
                sbString.Append(tempString[ctr]);
            }
            return sbString.ToString();
        }

        /// <summary>
        /// Ispects the field for all occurences of the search string and returns a count
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="searchString">String value to search for.</param>
        /// <param name="searchCondition">Specifies the search condition. Can take value, for example, "CHARACTERS BEFORE".</param>
        /// <returns>Returns the number of occurences found in the current object value.</returns>
        public static int InspectGivingCount(this IField instance, string searchString, string searchCondition)
        {
            int returnCount = 0;
            if (instance == null || instance.AsString() == string.Empty)
                returnCount = 0;
            else
            {
                if (searchCondition == "CHARACTERS BEFORE")
                {
                    int idx = instance.AsString().IndexOf(searchString);
                    if (idx > -1)
                        returnCount = idx;
                    else
                        returnCount = instance.LengthInBuffer;
                }
                else if (searchCondition == "LEADING")
                {
                    for (int i = 0; i < instance.AsString().Length - searchString.Length; i++)
                    {
                        if (instance.AsString().Substring(i, searchString.Length) == searchString)
                        {
                            returnCount++;
                            i = i + searchString.Length - 1;
                        }
                        else
                            break;
                    }
                }
                else
                // Inspect giving ALL
                {
                    int ctr = 0;
                    while (ctr < instance.AsString().Length)
                    {
                        int idx = instance.AsString().Substring(ctr).IndexOf(searchString);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IField GetAcceptData(string text)
        {
            throw new Exception("GetAcceptData - Not Implemented");
        }

        /// <summary>
        /// GEts teh length of a Field trimming low values from the end
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static int GetValueLength(this IField instance)
        {
            string valueString = instance.DisplayValue.Replace(AsciiChar.MinValue.ToString(), "");
            return valueString.Length;
        }

        /// <summary>
        /// Sets the format type for a field. Left or right justify
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fieldFormat"></param>
        public static void SetFormat(this IField instance, FieldFormat fieldFormat)
        {
            instance.FieldJustification = fieldFormat;
        }

        /// <summary>
        /// Emulate COBOL XML GENERATE function - Create XML from Data structure 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="strutureToConvert"></param>
        /// <param name="xmlCount"></param>
        /// <returns></returns>

        private static bool _isArray = false;
        private static string _prefixValue = "";
        private static bool _withAttributes = false;
        private static Dictionary<string, TypeOf> _typeOfs = new Dictionary<string, TypeOf>();

        private static string _suppressName = "";
        private static Dictionary<Every, List<When>> _suppressConditions = new Dictionary<Every, List<When>>();
        private static bool _suppressIsSet = false;
        private static string _suppressIsSetFor = "";

        public static byte[] CreateXMLFromDataStructure(this IField instance, IBufferElement structureToConvert, IField xml_code, params object[] options)
        {
            bool countIn = false;
            IField countInField = null;
            Encoding specifiedEncoding = null;
            Encoding currentEncoding = null;
            bool withXmlDeclaration = false;
            bool namespaceIs = false;
            IField namespaceValue = null;
            Dictionary<string, string> names = new Dictionary<string, string>();
            _suppressConditions.Clear();
            byte[] bytes = new byte[0];

            try
            {
                xml_code.SetValue(0);

                for (int i = 0; i < options.Length; i++)
                {
                    if (options[i] is XmlGenerate)
                    {
                        XmlGenerate xmlOption = (XmlGenerate)options[i];
                        if (xmlOption == XmlGenerate.CountIn)
                        {
                            if (options.Length > i + 1 && options[i + 1] is IField)
                            {
                                i++;
                                countIn = true;
                                countInField = (IField)options[i];
                            }
                            else
                                throw new Exception("XML GENERATE: COUNT IN parameter value is missing");
                        }
                        else if (xmlOption == XmlGenerate.WithAttributes)
                        {
                            _withAttributes = true;
                        }
                        else if (xmlOption == XmlGenerate.WithEncoding)
                        {
                            specifiedEncoding = Encoding.UTF8;
                            //if (options.Length > i + 1 && options[i + 1] is IField)
                            //{
                            //    i++;
                            //    int codePage = int.Parse(((IField)options[i]).DisplayValue);
                            //    if (codePage == 1208)
                            //    {
                            //        specifiedEncoding = Encoding.UTF8;
                            //    }
                            //    else
                            //    {
                            //        foreach (EncodingInfo ei in Encoding.GetEncodings())
                            //        {
                            //            Encoding e = ei.GetEncoding();
                            //            if (e.CodePage == codePage)
                            //            {
                            //                specifiedEncoding = e;
                            //                break;
                            //            }
                            //        }

                            //        if (specifiedEncoding == null)
                            //            throw new Exception("XML GENERATE: could not find encoding for code page " + codePage);
                            //    }
                            //}
                        }
                        else if (xmlOption == XmlGenerate.WithXmlDeclaration)
                        {
                            withXmlDeclaration = true;
                        }
                        else if (xmlOption == XmlGenerate.NamespaceIs)
                        {
                            if (options.Length > i + 1 && options[i + 1] is IField)
                            {
                                i++;
                                namespaceIs = true;
                                namespaceValue = (IField)options[i];
                            }
                            else
                                throw new Exception("XML GENERATE: namespace parameter value is missing");
                        }
                        else if (xmlOption == XmlGenerate.NamespacePrefixIs)
                        {
                            if (options.Length > i + 1 && options[i + 1] is IField)
                            {
                                i++;
                                _prefixValue = ((IField)options[i]).DisplayValue.Trim() + ":";
                            }
                            else
                                throw new Exception("XML GENERATE: namespace prefix parameter value is missing");
                        }
                        else if (xmlOption == XmlGenerate.NameOf)
                        {
                            for (i++; i < options.Length; i++)
                            {
                                if (options[i] is IBufferElement)
                                {
                                    string oldName = ((IBufferElement)options[i]).Name.Replace('_', '-');
                                    i++;
                                    if (options[i] is string)
                                        names.Add(oldName, (string)options[i]);
                                    else
                                    {
                                        i--;
                                        throw new Exception("XML GENERATE: NAME OF parameter value is missing");
                                    }
                                }
                                else
                                {
                                    i--;
                                    break;
                                }
                            }
                        }
                        else if (xmlOption == XmlGenerate.Suppress)
                        {
                            if (options.Length > i + 1)
                            {
                                if (options[i + 1] is IField)
                                    _suppressName = ((IBufferElement)options[++i]).Name;

                                Every every = Every.None;
                                for (i++; i < options.Length; i++)
                                {
                                    if (options[i] is Every)
                                    {
                                        every = (Every)options[i];
                                        if (!_suppressConditions.Keys.Contains<Every>(every))
                                            _suppressConditions.Add(every, new List<When>());
                                    }
                                    else if (options[i] is When)
                                    {
                                        if (!_suppressConditions.Keys.Contains<Every>(every))
                                            _suppressConditions.Add(every, new List<When>());

                                        _suppressConditions[every].Add((When)options[i]);
                                    }
                                    else
                                    {
                                        i--;
                                        break;
                                    }
                                }
                            }
                            else
                                throw new Exception("XML GENERATE: SUPPRESS parameters are missing");
                        }
                        else if (xmlOption == XmlGenerate.TypeOf)
                        {
                            for (i++; i < options.Length; i++)
                            {
                                if (options[i] is IBufferElement)
                                {
                                    string fieldName = ((IBufferElement)options[i]).Name;
                                    i++;
                                    if (options[i] is TypeOf)
                                        _typeOfs.Add(fieldName, (TypeOf)options[i]);
                                    else
                                    {
                                        i--;
                                        throw new Exception("XML GENERATE: TypeOf parameter is missing");
                                    }
                                }
                                else
                                {
                                    i--;
                                    break;
                                }
                            }
                        }
                    }
                    else
                        throw new Exception("XML GENERATE: options do not start with a XmlGenerate parameter");
                }

                StringBuilder xmlString = new StringBuilder();

                CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
                //int cp = cultureInfo.TextInfo.ANSICodePage;
                //currentEncoding = cp.Equals(0)
                //    ? Encoding.UTF8
                //    : Encoding.GetEncoding(cp);
                currentEncoding = Encoding.UTF8;

                if (specifiedEncoding == null)
                    specifiedEncoding = currentEncoding;

                if (withXmlDeclaration)
                    xmlString.Append("<?xml version=\"1.0\" encoding=\"" + specifiedEncoding.EncodingName + "\"?>");

                if (structureToConvert is IGroup)
                {
                    StringBuilder xmlAttributes = new StringBuilder();
                    StringBuilder xmlSubElements = new StringBuilder();

                    if (!_suppressIsSet) SetSuppress(structureToConvert);

                    foreach (IBufferElement element in ((IGroup)structureToConvert).Elements)
                    {
                        if (element.IsFiller || element.Name.StartsWith("FILLER") || element.IsARedefine || element.IsInRedefine /* TODO: || element.IsRenames */ ) continue;

                        if (!_suppressIsSet) SetSuppress(element);

                        if (element is IGroup)
                        {
                            xmlSubElements.Append(CreateXMLFromGroup((IGroup)element));
                        }
                        else if (element is IField || element is ICheckField)
                        {
                            if (_withAttributes)
                            {
                                if (_typeOfs.Keys.Contains<string>(element.Name))
                                {
                                    if (_typeOfs[element.Name] == TypeOf.IsAttribute)
                                        xmlAttributes.Append(CreateXmlAttribute(element));
                                    else
                                        xmlSubElements.Append(CreateXmlElement(element));
                                }
                                else
                                    xmlAttributes.Append(CreateXmlAttribute(element));
                            }
                            else
                            {
                                if (_typeOfs.Keys.Contains<string>(element.Name))
                                {
                                    if (_typeOfs[element.Name] == TypeOf.IsAttribute)
                                        xmlAttributes.Append(CreateXmlAttribute(element));
                                    else
                                        xmlSubElements.Append(CreateXmlElement(element));
                                }
                                else
                                    xmlSubElements.Append(CreateXmlElement(element));
                            }
                        }
                        else if (element is IGroupArray)
                        {
                            _isArray = true;
                            foreach (IGroup group in ((IGroupArray)element).Elements)
                            {
                                xmlSubElements.Append(CreateXMLFromGroup(group));
                            }
                            _isArray = false;
                        }
                        else if (element is IFieldArray)
                        {
                            _isArray = true;
                            foreach (IField field in ((IFieldArray)element).Elements)
                            {
                                xmlSubElements.Append(CreateXmlElement(element));
                            }
                            _isArray = false;
                        }

                        if (_suppressIsSet) DropSuppress(element);
                    }

                    if (_suppressIsSet) DropSuppress(structureToConvert);

                    xmlString.Append("<" + _prefixValue + GetName(structureToConvert));
                    if (namespaceIs) xmlString.Append(" xmlns=\"" + namespaceValue.DisplayValue.Trim() + "\"");
                    xmlString.Append(xmlAttributes + ">" + xmlSubElements + "</" + _prefixValue + GetName(structureToConvert) + ">");
                }
                else if (structureToConvert is IField)
                {
                    if (!_suppressIsSet) SetSuppress(structureToConvert);
                    xmlString.Append(CreateXmlElement(structureToConvert));
                    if (_suppressIsSet) DropSuppress(structureToConvert);
                }

                string strXML = ReplaceNames(xmlString.ToString(), names);
                strXML = System.Web.HttpUtility.HtmlDecode(strXML);
                bytes = Encoding.Convert(currentEncoding, specifiedEncoding, currentEncoding.GetBytes(strXML));
                instance.Assign(bytes);
                _suppressConditions.Clear();

                if (countIn)
                    countInField.Add(bytes.Length);
            }
            catch
            {
                xml_code.SetValue(-1);
            }

            return bytes;
        }

        public static byte[] CreateXMLFromDataStructure(this IField instance, IRecord structureToConvert, IField xml_code, params object[] options)
        {
            bool countIn = false;
            IField countInField = null;
            Encoding specifiedEncoding = null;
            Encoding currentEncoding = null;
            bool withXmlDeclaration = false;
            bool namespaceIs = false;
            IField namespaceValue = null;
            Dictionary<string, string> names = new Dictionary<string, string>();
            _suppressConditions.Clear();
            byte[] bytes = new byte[0];

            try
            {
                xml_code.SetValue(0);

                for (int i = 0; i < options.Length; i++)
                {
                    if (options[i] is XmlGenerate)
                    {
                        XmlGenerate xmlOption = (XmlGenerate)options[i];
                        if (xmlOption == XmlGenerate.CountIn)
                        {
                            if (options.Length > i + 1 && options[i + 1] is IField)
                            {
                                i++;
                                countIn = true;
                                countInField = (IField)options[i];
                            }
                            else
                                throw new Exception("XML GENERATE: COUNT IN parameter value is missing");
                        }
                        else if (xmlOption == XmlGenerate.WithAttributes)
                        {
                            _withAttributes = true;
                        }
                        else if (xmlOption == XmlGenerate.WithEncoding)
                        {
                            specifiedEncoding = Encoding.UTF8;
                            //if (options.Length > i + 1 && options[i + 1] is IField)
                            //{
                            //    i++;
                            //    int codePage = int.Parse(((IField)options[i]).DisplayValue);
                            //    if (codePage == 1208)
                            //    {
                            //        specifiedEncoding = Encoding.UTF8;
                            //    }
                            //    else
                            //    {
                            //        foreach (EncodingInfo ei in Encoding.GetEncodings())
                            //        {
                            //            Encoding e = ei.GetEncoding();
                            //            if (e.CodePage == codePage)
                            //            {
                            //                specifiedEncoding = e;
                            //                break;
                            //            }
                            //        }

                            //        if (specifiedEncoding == null)
                            //            throw new Exception("XML GENERATE: could not find encoding for code page " + codePage);
                            //    }
                            //}
                        }
                        else if (xmlOption == XmlGenerate.WithXmlDeclaration)
                        {
                            withXmlDeclaration = true;
                        }
                        else if (xmlOption == XmlGenerate.NamespaceIs)
                        {
                            if (options.Length > i + 1 && options[i + 1] is IField)
                            {
                                i++;
                                namespaceIs = true;
                                namespaceValue = (IField)options[i];
                            }
                            else
                                throw new Exception("XML GENERATE: namespace parameter value is missing");
                        }
                        else if (xmlOption == XmlGenerate.NamespacePrefixIs)
                        {
                            if (options.Length > i + 1 && options[i + 1] is IField)
                            {
                                i++;
                                _prefixValue = ((IField)options[i]).DisplayValue.Trim() + ":";
                            }
                            else
                                throw new Exception("XML GENERATE: namespace prefix parameter value is missing");
                        }
                        else if (xmlOption == XmlGenerate.NameOf)
                        {
                            for (i++; i < options.Length; i++)
                            {
                                if (options[i] is IBufferElement)
                                {
                                    string oldName = ((IBufferElement)options[i]).Name.Replace('_', '-');
                                    i++;
                                    if (options[i] is string)
                                        names.Add(oldName, (string)options[i]);
                                    else
                                    {
                                        i--;
                                        throw new Exception("XML GENERATE: NAME OF parameter value is missing");
                                    }
                                }
                                else
                                {
                                    i--;
                                    break;
                                }
                            }
                        }
                        else if (xmlOption == XmlGenerate.Suppress)
                        {
                            if (options.Length > i + 1)
                            {
                                if (options[i + 1] is IField)
                                    _suppressName = ((IBufferElement)options[++i]).Name;

                                Every every = Every.None;
                                for (i++; i < options.Length; i++)
                                {
                                    if (options[i] is Every)
                                    {
                                        every = (Every)options[i];
                                        if (!_suppressConditions.Keys.Contains<Every>(every))
                                            _suppressConditions.Add(every, new List<When>());
                                    }
                                    else if (options[i] is When)
                                    {
                                        if (!_suppressConditions.Keys.Contains<Every>(every))
                                            _suppressConditions.Add(every, new List<When>());

                                        _suppressConditions[every].Add((When)options[i]);
                                    }
                                    else
                                    {
                                        i--;
                                        break;
                                    }
                                }
                            }
                            else
                                throw new Exception("XML GENERATE: SUPPRESS parameters are missing");
                        }
                        else if (xmlOption == XmlGenerate.TypeOf)
                        {
                            for (i++; i < options.Length; i++)
                            {
                                if (options[i] is IBufferElement)
                                {
                                    string fieldName = ((IBufferElement)options[i]).Name;
                                    i++;
                                    if (options[i] is TypeOf)
                                        _typeOfs.Add(fieldName, (TypeOf)options[i]);
                                    else
                                    {
                                        i--;
                                        throw new Exception("XML GENERATE: TypeOf parameter is missing");
                                    }
                                }
                                else
                                {
                                    i--;
                                    break;
                                }
                            }
                        }
                    }
                    else
                        throw new Exception("XML GENERATE: options do not start with a XmlGenerate parameter");
                }

                StringBuilder xmlString = new StringBuilder();

                currentEncoding = Encoding.UTF8;
                specifiedEncoding = currentEncoding;
                //CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
                // int cp = cultureInfo.TextInfo.ANSICodePage;
                // currentEncoding = cp.Equals(0)
                //     ? Encoding.UTF8
                //     : Encoding.GetEncoding(cp);
                // if (specifiedEncoding == null)
                //     specifiedEncoding = currentEncoding;

                if (withXmlDeclaration)
                    xmlString.Append("<?xml version=\"1.0\" encoding=\"" + specifiedEncoding.EncodingName + "\"?>");


                StringBuilder xmlAttributes = new StringBuilder();
                StringBuilder xmlSubElements = new StringBuilder();

                // if (!_suppressIsSet) SetSuppress(structureToConvert);

                foreach (IBufferElement element in (structureToConvert).Elements)
                {
                    if (element.IsFiller || element.Name.StartsWith("FILLER") || element.IsARedefine || element.IsInRedefine /* TODO: || element.IsRenames */ ) continue;

                    if (!_suppressIsSet) SetSuppress(element);

                    if (element is IGroup)
                    {
                        xmlSubElements.Append(CreateXMLFromGroup((IGroup)element));
                    }
                    else if (element is IField || element is ICheckField)
                    {
                        if (_withAttributes)
                        {
                            if (_typeOfs.Keys.Contains<string>(element.Name))
                            {
                                if (_typeOfs[element.Name] == TypeOf.IsAttribute)
                                    xmlAttributes.Append(CreateXmlAttribute(element));
                                else
                                    xmlSubElements.Append(CreateXmlElement(element));
                            }
                            else
                                xmlAttributes.Append(CreateXmlAttribute(element));
                        }
                        else
                        {
                            if (_typeOfs.Keys.Contains<string>(element.Name))
                            {
                                if (_typeOfs[element.Name] == TypeOf.IsAttribute)
                                    xmlAttributes.Append(CreateXmlAttribute(element));
                                else
                                    xmlSubElements.Append(CreateXmlElement(element));
                            }
                            else
                                xmlSubElements.Append(CreateXmlElement(element));
                        }
                    }
                    else if (element is IGroupArray)
                    {
                        _isArray = true;
                        foreach (IGroup group in ((IGroupArray)element).Elements)
                        {
                            xmlSubElements.Append(CreateXMLFromGroup(group));
                        }
                        _isArray = false;
                    }
                    else if (element is IFieldArray)
                    {
                        _isArray = true;
                        foreach (IField field in ((IFieldArray)element).Elements)
                        {
                            xmlSubElements.Append(CreateXmlElement(element));
                        }
                        _isArray = false;
                    }

                    if (_suppressIsSet) DropSuppress(element);
                }

                // if (_suppressIsSet) DropSuppress(structureToConvert);

                xmlString.Append("<" + _prefixValue + structureToConvert.Name.Replace("_", "-"));
                if (namespaceIs) xmlString.Append(" xmlns=\"" + namespaceValue.DisplayValue.Trim() + "\"");
                xmlString.Append(xmlAttributes + ">" + xmlSubElements + "</" + _prefixValue + structureToConvert.Name.Replace("_", "-") + ">");



                string strXML = ReplaceNames(xmlString.ToString(), names);
                bytes = Encoding.Convert(currentEncoding, specifiedEncoding, currentEncoding.GetBytes(strXML));
                instance.Assign(bytes);
                _suppressConditions.Clear();

                if (countIn)
                    countInField.Add(bytes.Length);
            }
            catch
            {
                xml_code.SetValue(-1);
            }

            return bytes;
        }
        private static string CreateXMLFromGroup(IGroup groupToConvert)
        {
            StringBuilder xmlAttributes = new StringBuilder();
            StringBuilder xmlSubElements = new StringBuilder();

            foreach (IBufferElement element in groupToConvert.Elements)
            {
                if (element.IsFiller || element.Name.StartsWith("FILLER") || element.IsARedefine || element.IsInRedefine /* TODO: || element.IsRenames */ ) continue;

                if (!_suppressIsSet) SetSuppress(element);

                if (element is IGroup)
                {
                    xmlSubElements.Append(CreateXMLFromGroup((IGroup)element));
                }
                else if (element is IField || element is ICheckField)
                {
                    if (_withAttributes)
                    {
                        if (_typeOfs.Keys.Contains<string>(element.Name))
                        {
                            if (_typeOfs[element.Name] == TypeOf.IsAttribute)
                                xmlAttributes.Append(CreateXmlAttribute(element));
                            else
                                xmlSubElements.Append(CreateXmlElement(element));
                        }
                        else
                            xmlAttributes.Append(CreateXmlAttribute(element));
                    }
                    else
                    {
                        if (_typeOfs.Keys.Contains<string>(element.Name))
                        {
                            if (_typeOfs[element.Name] == TypeOf.IsAttribute)
                                xmlAttributes.Append(CreateXmlAttribute(element));
                            else
                                xmlSubElements.Append(CreateXmlElement(element));
                        }
                        else
                            xmlSubElements.Append(CreateXmlElement(element));
                    }
                }
                else if (element is IGroupArray)
                {
                    _isArray = true;
                    foreach (IGroup group in ((IGroupArray)element).Elements)
                    {
                        xmlSubElements.Append(CreateXMLFromGroup(group));
                    }
                    _isArray = false;
                }
                else if (element is IFieldArray)
                {
                    _isArray = true;
                    foreach (IField field in ((IFieldArray)element).Elements)
                    {
                        xmlSubElements.Append(CreateXmlElement(element));
                    }
                    _isArray = false;
                }

                if (_suppressIsSet) DropSuppress(element);
            }
            //return xmlSubElements.ToString();
            return CreateXmlElement(groupToConvert, xmlAttributes.ToString(), xmlSubElements.ToString());
        }

        private static string GetName(IBufferElement element)
        {
            string name = element.Name.Replace('_', '-');

            if (_isArray)
            {
                int index = name.IndexOf(' ');
                if (index > 0)
                    name = name.Substring(0, index);
            }

            return name;
        }

        private static string CreateXmlAttribute(IBufferElement element)
        {
            if (SuppressAttribute(element)) return "";

            string value = element is IField
                ? ((IField)element).DisplayValue
                : element is ICheckField
                    ? ((ICheckField)element).DisplayString
                    : null;

            return " " + GetName(element) + "=\"" + HttpUtility.HtmlEncode(value) + "\"";
        }

        private static string CreateXmlElement(IBufferElement element, string xmlAttributes = "", string xmlValue = "")
        {
            if ((element is IGroup && SuppressGroup(element))
                || (element is IField && SuppressElement(element))) return "";

            string name = _prefixValue + GetName(element);
            string value = element is IGroup
                                ? xmlValue
                                : element is IField
                                    ? ((IField)element).DisplayValue
                                    : element is ICheckField
                                        ? ((ICheckField)element).DisplayString
                                        : null;

            value = value.TrimEnd();
            if (string.IsNullOrEmpty(value))
                value = " ";
            if (element is IGroup)
                return "<" + name + xmlAttributes + ">" + value + "</" + name + ">";
            else
                return "<" + name + xmlAttributes + ">" + HttpUtility.HtmlEncode(value) + "</" + name + ">";
        }

        private static void SetSuppress(IBufferElement element)
        {
            if (_suppressName.Length == 0 || ((IBufferElement)element).Name == _suppressName)
            {
                _suppressIsSet = true;
                _suppressIsSetFor = ((IBufferElement)element).Name;
            }
        }

        private static void DropSuppress(IBufferElement element)
        {
            if (((IBufferElement)element).Name == _suppressIsSetFor)
            {
                _suppressIsSet = false;
                _suppressIsSetFor = "";
            }
        }

        private static bool SuppressGroup(IBufferElement element)
        {
            bool suppress = false;
            foreach (Every every in _suppressConditions.Keys)
            {
                if (every == Every.Content)
                {
                    if (_suppressConditions[every].Count == 0)
                    {
                        suppress = true;
                        break;
                    }
                }
            }

            return suppress;
        }

        private static bool SuppressElement(IBufferElement element)
        {
            if (!(element is IField)) return false;

            bool suppress = false;
            if (_suppressIsSet)
            {
                bool isNumeric = ((IField)element).IsNumericType;

                foreach (Every every in _suppressConditions.Keys)
                {
                    bool canSuppress = false;
                    if (every == Every.Element) canSuppress = true;
                    else if (isNumeric && (every == Every.Numeric || every == Every.NumericElement) || every == Every.NumericContent) canSuppress = true;
                    else if (!isNumeric && (every == Every.NonNumeric || every == Every.NonNumericElement || every == Every.NonNumericContent)) canSuppress = true;

                    if (canSuppress)
                    {
                        if (_suppressConditions[every].Count == 0)
                        {
                            suppress = true;
                            break;
                        }

                        if (isNumeric)
                        {

                            foreach (When when in _suppressConditions[every])
                            {
                                suppress = (((IField)element).AsDecimal() == 0 && (when == When.Zero || when == When.Zeroes || when == When.Zeros))
                                    || (((IField)element).IsMaxValue() && (when == When.HighValue || when == When.HighValues))
                                    || (((IField)element).IsMinValue() && (when == When.LowValue || when == When.LowValues));

                                if (suppress) break;
                            }
                        }
                        else
                        {
                            foreach (When when in _suppressConditions[every])
                            {
                                suppress = (((IField)element).DisplayValue.Trim().Length == 0 && ((IField)element).DisplayValue.Length > 1 && when == When.Spaces)
                                    || (((IField)element).DisplayValue == " " && when == When.Space)
                                    || (((IField)element).IsMaxValue() && (when == When.HighValue || when == When.HighValues))
                                    || (((IField)element).IsMinValue() && (when == When.LowValue || when == When.LowValues));

                                if (suppress) break;
                            }
                        }
                    }

                    if (suppress) break;
                }
            }

            return suppress;
        }

        private static bool SuppressAttribute(IBufferElement element)
        {
            if (!(element is IField)) return false;

            bool suppress = false;
            if (_suppressIsSet)
            {
                bool isNumeric = ((IField)element).IsNumericType;

                foreach (Every every in _suppressConditions.Keys)
                {
                    bool canSuppress = false;
                    if (every == Every.Attribute) canSuppress = true;
                    else if (isNumeric && (every == Every.Numeric || every == Every.NumericAttribute)) canSuppress = true;
                    else if (!isNumeric && (every == Every.NonNumeric || every == Every.NonNumericAttribute)) canSuppress = true;

                    if (canSuppress)
                    {
                        if (_suppressConditions[every].Count == 0)
                        {
                            suppress = true;
                            break;
                        }

                        if (isNumeric)
                        {
                            foreach (When when in _suppressConditions[every])
                            {
                                suppress = (((IField)element).AsDecimal() == 0 && (when == When.Zero || when == When.Zeroes || when == When.Zeros))
                                    || (((IField)element).IsMaxValue() && (when == When.HighValue || when == When.HighValues))
                                    || (((IField)element).IsMinValue() && (when == When.LowValue || when == When.LowValues));

                                if (suppress) break;
                            }
                        }
                        else
                        {
                            foreach (When when in _suppressConditions[every])
                            {
                                suppress = (((IField)element).DisplayValue.Trim().Length == 0 && ((IField)element).DisplayValue.Length > 1 && when == When.Spaces)
                                    || (((IField)element).DisplayValue == " " && when == When.Space)
                                    || (((IField)element).IsMaxValue() && (when == When.HighValue || when == When.HighValues))
                                    || (((IField)element).IsMinValue() && (when == When.LowValue || when == When.LowValues));

                                if (suppress) break;
                            }
                        }
                    }

                    if (suppress) break;
                }
            }

            return suppress;
        }

        private static string ReplaceNames(string xmlString, Dictionary<string, string> names)
        {

            foreach (string oldName in names.Keys)
            {
                if (_prefixValue.Length > 0)
                {
                    xmlString = xmlString
                        .Replace(":" + oldName + " ", ":" + names[oldName] + " ")
                        .Replace(":" + oldName + ">", ":" + names[oldName] + ">");
                }
                else
                {
                    xmlString = xmlString
                       .Replace("<" + oldName + ">", "<" + names[oldName] + ">")
                       .Replace("<" + oldName + " ", "<" + names[oldName] + " ")
                       .Replace("</" + oldName + ">", "</" + names[oldName] + ">");
                }

                if (_withAttributes)
                    xmlString = xmlString.Replace(" " + oldName + "=", " " + names[oldName] + "=");
            }

            return xmlString;
        }

        private static string ToUpper(this IField instance)
        {

            return instance.AsString().ToUpper();
        }

        /// <summary>
        /// Emulates COBOL function DATE-OF-INTEGER - returns date(YYYYMMDD) based on number of days since Jan 1. 1601
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string AsDateFromInteger(this IField instance)
        {
            DateTime dtTemp = new DateTime(1600, 12, 31);
            DateTime dtnew = dtTemp.AddDays(instance.AsInt());
            return dtnew.ToString("yyyyMMdd");
        }
        /// <summary>
        /// /// Emulates COBOL function INTEGER-OF-DATE - returns number based on difference bewteen date and Jan 1. 1601
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static int AsIntegerFromDate(this IField instance)
        {
            DateTime dtfield = new DateTime(Convert.ToInt32(instance.GetSubstring(1, 4)), Convert.ToInt32(instance.GetSubstring(4, 2)), Convert.ToInt32(instance.GetSubstring(6, 2)));
            DateTime dtTemp = new DateTime(1600, 12, 31);
            TimeSpan ts = dtfield.Subtract(dtTemp);
            return ts.Days;
        }
        /// <summary>
        /// Determines whether this instance contains the specified string.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="checkString">The check string.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified check string]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(this IField instance, string checkString)
        {
            if (instance.AsString().Contains(checkString))
                return true;
            else
                return false;
        }
        /// <summary>
        /// Inserts a string at the specified start position.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="checkInsertString">The check insert string.</param>
        public static string Insert(this IField instance, IField startPosition, IField checkInsertString)
        {
            return instance.Insert(startPosition.AsInt(), checkInsertString.AsString());
        }
        /// <summary>
        /// Inserts a string at the specified start position.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="checkInsertString">The check insert string.</param>
        /// <returns></returns>
        public static string Insert(this IField instance, IField startPosition, string checkInsertString)
        {
            return instance.Insert(startPosition.AsInt(), checkInsertString.AsString());
        }
        /// <summary>
        /// Inserts a string at the specified start position.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="checkInsertString">The check insert string.</param>
        public static string Insert(this IField instance, int startPosition, IField checkInsertString)
        {
            return instance.Insert(startPosition, checkInsertString.AsString());
        }
        /// <summary>
        /// Inserts a string at the specified start position.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="checkInsertString">The check insert string.</param>
        public static string Insert(this IField instance, int startPosition, string checkInsertString)
        {
            return instance.AsString().Insert(startPosition - 1, checkInsertString);
        }
        /// <summary>
        /// Reset Field with int value
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        public static void ResetNumericsWith(this IField instance, int value)
        {
            instance.SetValue(value);
        }

        #endregion

        #region SetBufferPointer
        /// <summary>
        /// Emulates COBOL statement <c>SET ADDRESS OF <paramref name="instance"/> TO ADDRESS OF <paramref name="element"/></c>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="element">A reference to the buffer element or buffer value object.</param>
        public static void SetBufferReference<T>(this IField instance, T element)
        where T : IBufferElement, IBufferValue
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");
            if (element == null)
                throw new ArgumentNullException("element", "element is null.");

            bool isReferencePointer = false; IField tField = null;
            if (element is IField)
            {
                tField = (IField)element;
                if (tField.FieldType == Common.FieldType.ReferencePointer)
                    isReferencePointer = true;
            }

            if (isReferencePointer)
                instance.SetAddressFromValueOf(tField);
            else
                instance.SetAddressToAddressOf(element);
        }

        /// <summary>
        /// This is an obsolete method. Use Generic version of SetBufferPointer.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="field">A reference to the IFIeld object.</param>
        [Obsolete("Use Generic version of SetBufferPointer instead.", false)]
        public static void SetBufferPointer(this IField instance, IField field)
        {
            // this method should go away in favor of: 
            SetBufferPointer<IField>(instance, field);
        }

        /// <summary>
        /// This is an obsolete method. Use Generic version of SetBufferPointer.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="field">A reference to the IGroup object.</param>
        [Obsolete("Use Generic version of SetBufferPointer instead.", false)]
        public static void SetBufferPointer(this IField instance, IGroup group)
        {
            // this method should go away in favor of: 
            SetBufferPointer<IGroup>(instance, group);
        }

        /// <summary>
        /// Functions as COBOL statement <c>SET P TO ADDRESS OF B</c>. 
        /// Causes the field object to set its value to the "address" of the 
        /// buffer of the given <paramref name="element"/>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="element">A reference to the buffer element or buffer value object.</param>
        public static void SetBufferPointer<T>(this IField instance, T element)
            where T : IBufferValue, IBufferElement
        {
            instance.SetValueToAddressOf(element);
        }

        /// <summary>
        /// This is an obsolete method. Use Generic version of SetBufferPointer.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="field">A reference to the IBufferValue object.</param>
        [Obsolete("Use Generic version of SetBufferPointer instead.", true)]
        public static void SetBufferPointer(this IField instance, IBufferValue bufferValue)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Functions as COBOL statement <c>SET P TO ADDRESS OF B</c>. 
        /// Causes the field object to set its value to the "address" of the 
        /// buffer of the given <paramref name="bufferRecord"/> object.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="bufferRecord">A reference to the IRecord object.</param>
        public static void SetRecordBufferPointer(this IField instance, IRecord bufferRecord)
        {
            int recordKey = BufferServices.Records.GetKeyFor(bufferRecord);
            if (recordKey == 0)
            {
                BufferServices.Records.Add(bufferRecord);
                recordKey = BufferServices.Records.GetKeyFor(bufferRecord);
            }
            //IBufferAddress bufferAddress = ObjectFactory.Factory.NewBufferAddress(recordKey, element.Name);
            //int addressKey = BufferServices.BufferAddresses.Add(bufferAddress);
            instance.SetValue(recordKey);
        }



        #endregion

    }
}
