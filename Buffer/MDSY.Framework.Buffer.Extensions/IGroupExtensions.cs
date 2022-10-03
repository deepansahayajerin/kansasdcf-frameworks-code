using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MDSY.Framework.Buffer
{
    /// <summary>
    /// Extension methods for objects with implement IGroup 
    /// </summary>
    public static class IGroupExtensions
    {
        /// <summary>
        /// Applies the given criteria to the instance object and returns the result.
        /// </summary>
        /// <param name="instance">The buffer value object which will be passed to <paramref name="criteria"/>.</param>
        /// <param name="criteria">The Func&lt;IBufferValue, bool&gt; to be invoked on <paramref name="instance"/>.</param>
        /// <returns>The result of the applied criteria.</returns>
        public static bool MeetsCriteria(this IGroup instance, Expression<Func<IGroup, bool>> criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria", "criteria is null.");

            return criteria.Compile().Invoke(instance);
        }

        /// <summary>
        /// Checks whether the current object contains only the specified byte value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="byteValue">Specifies byte value.</param>
        /// <returns>Returns true if the current object contains only the specified byte value.</returns>
        public static bool ContainsOnly(this IGroup instance, byte byteValue)
        {
            return instance.AsBytes.All(b => b.Equals(byteValue));
        }

        /// <summary>
        /// Returns <c>true</c> if the IBaseField object's text value contains 
        /// only characters that are in the given char array. 
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="limitChars">Specifies characters for comparison.</param>
        /// <returns>Returns true if the current object contains only the specified characters.</returns>
        public static bool ContainsOnly(this IGroup instance, params char[] limitChars)
        {
            bool result = true;
            string value = instance.BytesAsString.Trim();
            for (int ctr = 0; ctr < value.Length; ctr++)
            {
                char testchar = value[ctr];
                if (!limitChars.Contains(testchar))
                {
                    result = false;
                    break;
                }
            }
            return result;
            // checks by getting a list of chars from the field's Text property
            // that are NOT in limitChars and making sure that list's count is zero. 
            //return instance
            //    .MeetsCriteria(grp => grp.AsBytes
            //        .Select(b => (char)b)
            //        .Any(c => !limitChars.Contains(c)));
        }

        /// <summary>
        /// Returns <c>true</c> if the group object contains only space characters.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object contains only space characters.</returns>
        public static bool IsSpaces(this IGroup instance)
        {
            if (instance == null)
                return false;
            return (instance.AsString().Trim() == string.Empty);
        }

        /// <summary>
        /// Returns <c>true</c> if the group object does not contain only space characters.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object does not contain only space characters.</returns>
        public static bool IsNotSpaces(this IGroup instance)
        {
            if (instance == null)
                return false;
            return(instance.AsString().Trim() != string.Empty);
        }

        /// <summary>
        /// Checks whether the current object contains only a numeric value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object contains only a numeric value</returns>
        public static bool IsNumericValue(this IGroup instance)
        {
            if (instance == null)
                return false;
            return instance.ContainsOnly('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
        }


        /// <summary>
        /// Returns <c>true</c> if the group object contains only '0' characters.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object contains only zeroes.</returns>
        public static bool IsZeroes(this IGroup instance)
        {
            if (instance == null)
                return false;
            return instance.ContainsOnly('0');
        }

        /// <summary>
        /// Returns <c>true</c> if the group object contains only '0' characters.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object contains only zeros.</returns>
        public static bool IsZeros(this IGroup instance)
        {
            if (instance == null)
                return false;
            return instance.ContainsOnly('0');
        }

        public static int InspectGivingCount(this IGroup instance, string searchString, string searchCondition)
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
        /// Returns the value of the Group object as a string if possible.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns the string representation of the current object value.</returns>
        public static string AsString(this IGroup instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            return instance is IRedefinition ?
                       instance.RedefinedBytesAsString :
                       instance.BytesAsString;
        }
        /// <summary>
        /// Returns the current object value as a date string in yyyy-DD-mm format.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns the current object value as a date string in yyyy-DD-mm format</returns>
        public static string AsDateString(this IGroup instance)
        {
            if (instance.AsString() == "00000000")
                instance.SetValue("00010101");

            return instance.AsString().Insert(6, "-").Insert(4, "-");
        }

        /// <summary>
        /// Returns the value of the Group object as an Int64 if possible. Throws
        /// if conversion is not possible.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns the integer representation of the current object value.</returns>
        public static Int32 AsInt(this IGroup instance)
        {
            if (instance.Elements.Count() == 1)
            {
                int val = 0;
                foreach (IField field in instance.Elements)
                {
                    if (field.IsNumericType)
                    {
                        val = Int32.Parse(field.DisplayValue);
                    }
                    else
                    {
                        val = Int32.Parse(instance.BytesAsString);
                    }
                }
                return val;
            }
            else
                return Int32.Parse(instance.BytesAsString);

        }

        /// <summary>
        /// Converts the current object value into a long integer.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns the long integer representation of the current object value.</returns>
        public static Int64 AsLong(this IGroup instance)
        {
            return Int64.Parse(instance.BytesAsString);
        }

        /// <summary>
        /// Sets string Value coming from DB2 database data after changing hex 9F to hex FF
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetValueFromDB2(this IGroup instance, string value)
        {
            if (value.Contains(AsciiChar.Db2ConnectHighValue.AsChar))
            {
                value = value.Replace(AsciiChar.Db2ConnectHighValue.AsChar, AsciiChar.MaxValue.AsChar);
            }
            instance.SetValue(value);
        }

        /// <summary>
        /// Sets all numeric fields in a group to an Integer value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void ResetNumericsWith(this IGroup instance, int value)
        {
            foreach (KeyValuePair<string, IBufferElement> kvp in instance.ChildCollection)
            {
                if (kvp.Value.IsARedefine) continue;
                if (kvp.Value is IGroup)
                {
                    ResetNumericsWith((IGroup)kvp.Value, value);
                    continue;
                }
                if (kvp.Value is IGroupArray)
                {
                    int arrayOccurs = ((IGroupArray)kvp.Value).ArrayElementCount;
                    for (int i = 0; i < arrayOccurs; i++)
                    {
                        ResetNumericsWith(((IGroupArray)kvp.Value)[i], value);
                        continue;
                    }
                    continue;
                }
                if (kvp.Value is IFieldArray)
                {
                    var array = (IFieldArray)kvp.Value;
                    int arrayOccurs = array.ArrayElementCount;
                    for (int i = 0; i < arrayOccurs; i++)
                    {
                        var element = array[i];
                        IField arrayChild = element;
                        if (arrayChild.IsNumericType)
                        {
                            arrayChild.SetValue(value);
                        }
                    }
                    continue;
                }
                IField child = (IField)kvp.Value;
                if (child.IsNumericType)
                {
                    child.SetValue(value);
                }
            }
        }

        /// <summary>
        /// Sets all alphanumeric fields in a group to a string value composed of repeating bytes
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void ResetAlphaNumericDataWithAll(this IGroup instance, string value)
        {
            foreach (KeyValuePair<string, IBufferElement> kvp in instance.ChildCollection)
            {
                if (kvp.Value.IsARedefine) continue;
                if (kvp.Value is IGroup)
                {
                    ResetAlphaNumericDataWithAll((IGroup)kvp.Value, value);
                    continue;
                }
                if (kvp.Value is IGroupArray)
                {
                    int arrayOccurs = ((IGroupArray)kvp.Value).ArrayElementCount;
                    for (int i = 0; i < arrayOccurs; i++)
                    {
                        ResetAlphaNumericDataWithAll(((IGroupArray)kvp.Value)[i], value);
                        continue;
                    }
                    continue;
                }
                if (kvp.Value is IFieldArray)
                {
                    var array = (IFieldArray)kvp.Value;
                    int arrayOccurs = array.ArrayElementCount;
                    for (int i = 0; i < arrayOccurs; i++)
                    {
                        var element = array[i];
                        IField arrayChild = element;
                        if (arrayChild.FieldType == FieldType.String)
                        {
                            arrayChild.SetValue(BuildStringFromValueToLength(value, arrayChild.DisplayLength));
                        }
                    }
                    continue;
                }
                IField child = (IField)kvp.Value;
                if (child.FieldType == FieldType.String)
                {
                    child.SetValue(BuildStringFromValueToLength(value, child.DisplayLength));
                }
            }
        }

        public static string BuildStringFromValueToLength(string value, int len)
        {
            StringBuilder sb = new StringBuilder("");
            for (int i = 0; i < len; i++)
            {
                sb.Append(value);
            }
            return sb.ToString();
        }


        /// <summary>
        /// Returns string for sending to DB2 database after changing hex FF to Hex 9F
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns the current object value as a string.</returns>
        public static string AsStringForDB2(this IGroup instance)
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
        /// Returns the value of the Group object as a Decimal if possible. Throws
        /// if conversion is not possible.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns the current object value as a decimal.</returns>
        public static Decimal AsDecimal(this IGroup instance)
        {
            return Decimal.Parse(instance.BytesAsString);
        }

        /// <summary>
        /// Checks whether the current object value contains maximum value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object value is the maximum value.</returns>
        public static bool IsMaxValue(this IGroup instance)
        {
            return ContainsOnly(instance, byte.MaxValue);
        }

        /// <summary>
        /// Checks whether the current object value contains minimum value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object value is the minimum value.</returns>
        public static bool IsMinValue(this IGroup instance)
        {
            return ContainsOnly(instance, byte.MinValue);
        }

        /// <summary>
        /// Checks whether the current object value does not contain zeroes.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object does not contain zeroes.</returns>
        public static bool IsNotZeroes(this IGroup instance)
        {
            return !(IsZeroes(instance));
        }

        /// <summary>
        /// Checks whether the current object does not contain the maximum value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object does not contain the maximum value.</returns>
        public static bool IsNotMaxValue(this IGroup instance)
        {
            return !(IsMaxValue(instance));
        }

        /// <summary>
        /// Checks whether the current object does not contain the minimum value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <returns>Returns true if the current object does not contain the minimum value.s</returns>
        public static bool IsNotMinValue(this IGroup instance)
        {
            return !(IsMinValue(instance));
        }


        #region IsEqualTo
        /// <summary>
        /// Returns true if the value of this group is equal to the value of the <paramref name="other"/> field.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if both fields are <c>null</c>, if both fields refer to the same object instance, or 
        /// if the two field values are functionally equivalent. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this IGroup instance, IField other)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            return (instance.Equals(other));
        }

        /// <summary>
        /// Returns true if the value of this group is equal to the value of the <paramref name="other"/> group.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="other">A reference to another IGroup object for comparison.</param>
        /// <returns>Returns true if the current object value is equal to the specified object value.</returns>
        public static bool IsEqualTo(this IGroup instance, IGroup other)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            return (instance.Equals(other));
        }

        /// <summary>
        /// Returns true if the value of this group is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this IGroup instance, string other)
        {
            if (instance == null)
                return false;
            return instance.Equals(other);
        }

        /// <summary>
        /// Returns true if the value of this group is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this IGroup instance, Int64 other)
        {
            decimal testDec;
            if (decimal.TryParse(instance.BytesAsString, out testDec))
            {
                return (testDec == (decimal)other);
            }
            else
                return false;
        }

        /// <summary>
        /// Returns true if all characters in this field match the passed character
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsEqualToAll(this IGroup instance, string other)
        {
            if (!(instance is IEquatable<string>))
                throw new NotImplementedException();

            return instance.ContainsOnly(other[0]);
        }



        #endregion

        #region IsNotEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this group is not equal to the value of the <paramref name="other"/> field.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>false</c> if both fields are <c>null</c>, if both fields refer to the same object instance, or 
        /// if the two field values are functionally equivalent. Otherwise, returns <c>true</c>.</returns>
        public static bool IsNotEqualTo(this IGroup instance, IField other)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            return (!instance.Equals(other));
        }

        /// <summary>
        ///  Returns <c>true</c> if the value of this group is not equal to the value of the <paramref name="other"/> group.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="other">A reference to another IGroup object for comparison.</param>
        /// <returns>Returns true if the current object value does not match with the specified object value.</returns>
        public static bool IsNotEqualTo(this IGroup instance, IGroup other)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            return (!instance.Equals(other));
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this field is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this IGroup instance, string other)
        {
            return (!instance.Equals(other));
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this group is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this IGroup instance, Int64 other)
        {
            decimal testDec;
            if (decimal.TryParse(instance.BytesAsString, out testDec))
            {
                return (testDec != (decimal)other);
            }
            else
                return true;
        }


        #endregion

        #region IsLessThan

        /// <summary>
        /// Returns <c>true</c> if the value of this Group is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this IGroup instance, IField other)
        {
            return (instance.CompareTo(other) < 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this Group is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this IGroup instance, IGroup other)
        {
            return (instance.CompareTo(other) < 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this Group is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this IGroup instance, string other)
        {
            return (instance.CompareTo(other) < 0);
            //return (instance.AsString().CompareTo(other) < 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this Group is less than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThan(this IGroup instance, Int64 other)
        {
            decimal testDec;
            if (decimal.TryParse(instance.BytesAsString, out testDec))
            {
                return (testDec < (decimal)other);
            }
            else
                return false;
        }



        #endregion

        #region IsGreaterThan
        /// <summary>
        /// Returns <c>true</c> if the value of this Group is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The Group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this IGroup instance, IField other)
        {
            return (instance.CompareTo(other) > 0);
            //return (instance.AsString().CompareTo(other.AsString()) > 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this Group is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The Group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this IGroup instance, IGroup other)
        {
            return (instance.CompareTo(other) > 0);
            //return (instance.AsString().CompareTo(other.AsString()) > 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this Group is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The Group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this IGroup instance, string other)
        {
            return (instance.CompareTo(other) > 0);
            //return (instance.AsString().CompareTo(other) > 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this Group is greater than the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt; 0);</c>
        /// </summary>
        /// <param name="instance">The Group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThan(this IGroup instance, Int64 other)
        {
            decimal testDec;
            if (decimal.TryParse(instance.BytesAsString, out testDec))
            {
                return (testDec > (decimal)other);
            }
            else
                return false;
        }

        public static bool IsGreaterThanMinValue(this IGroup instance)
        {
            if (instance.IsGreaterThan(byte.MinValue))
                return true;
            else
                return false;
        }
        #endregion

        #region IsLessThanOrEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this Group is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The Group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this IGroup instance, IField other)
        {
            return (instance.CompareTo(other) <= 0);
            //return (instance.AsString().CompareTo(other.AsString()) <= 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this Group is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The Group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this IGroup instance, IGroup other)
        {
            return (instance.CompareTo(other) <= 0);
            //return (instance.AsString().CompareTo(other.AsString()) <= 0);
        }


        /// <summary>
        /// Returns <c>true</c> if the value of this Group is less than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt;= 0);</c>
        /// </summary>
        /// <param name="instance">The Group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this IGroup instance, string other)
        {
            return (instance.CompareTo(other) <= 0);
            //return (instance.AsString().CompareTo(other) <= 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this Group is less than or equal the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsLessThanOrEqualTo(this IGroup instance, Int64 other)
        {
            decimal testDec;
            if (decimal.TryParse(instance.BytesAsString, out testDec))
            {
                return (testDec <= (decimal)other);
            }
            else
                return false;
        }


        #endregion

        #region IsGreaterThanOrEqualTo
        /// <summary>
        /// Returns <c>true</c> if the value of this Group is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The Group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this IGroup instance, IField other)
        {
            return (instance.CompareTo(other) >= 0);
            //return (instance.AsString().CompareTo(other.AsString()) >= 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this Group is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The Group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally greater than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this IGroup instance, IGroup other)
        {
            return (instance.CompareTo(other) >= 0);
            //return (instance.AsString().CompareTo(other.AsString()) >= 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this Group is greater than or equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &gt;= 0);</c>
        /// </summary>
        /// <param name="instance">The Group object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this IGroup instance, string other)
        {
            return (instance.CompareTo(other) >= 0);
            //return (instance.AsString().CompareTo(other) >= 0);
        }

        /// <summary>
        /// Returns <c>true</c> if the value of this Group is less than or equal the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>(<paramref name="instance"/>.CompareTo(<paramref name="other"/>) &lt; 0);</c>
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally less than
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsGreaterThanOrEqualTo(this IGroup instance, Int64 other)
        {
            decimal testDec;
            if (decimal.TryParse(instance.BytesAsString, out testDec))
            {
                return (testDec >= (decimal)other);
            }
            else
                return false;
        }
        #endregion

        #region Misc
        /// <summary>
        /// Replaces the current object value with another string value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="searchValue">Specifies the string value to search for.</param>
        /// <param name="replaceValue">Specifies the string value for the replacement.</param>
        /// <returns></returns>
        public static IGroup Replace(this IGroup instance, string searchValue, string replaceValue)
        {
            instance.Assign(instance.AsString().Replace(searchValue, replaceValue));
            return instance;
        }
        public static IGroup Replace(this IGroup instance, IBufferValue searchValue, IBufferValue replaceValue)
        {
            instance.Assign(instance.AsString().Replace(searchValue.BytesAsString, replaceValue.BytesAsString));
            return instance;
        }

        /// <summary>
        /// Set the value of a Substring (position based)
        /// </summary>
        /// <param name="instance">A referenceto the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the number of characters for the substring.</param>
        /// <param name="replaceValue">String value for the replacement.</param>
        public static void SetValueOfSubstring(this IGroup instance, int startPosition, int length, string replaceValue)
        {
            instance.Assign(instance.AsString().Remove(startPosition - 1, length).Insert(startPosition - 1, replaceValue.PadRight(length)));
        }

        /// <summary>
        /// Set the value of a Substring from IField value (position based)
        /// </summary>
        /// <param name="instance">A referenceto the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the number of characters for the substring.</param>
        /// <param name="replaceValue">A reference to the IField object that contains a string value for the replacement.</param>
        public static void SetValueOfSubstring(this IGroup instance, int startPosition, int length, IField replaceField)
        {
            instance.Assign(instance.AsString().Remove(startPosition - 1, length).Insert(startPosition - 1, replaceField.AsString().PadRight(length)));
        }

        /// <summary>
        /// Inserts a string value into a group based on pointer position and updates the pointer with ending position
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="stringValue"></param>
        /// <param name="pointer"></param>
        public static void SetStringValueWithPointer(this IGroup instance, string stringValue, IField pointer)
        {
            SetValueOfSubstring(instance, pointer.GetValue<int>(), stringValue.Length, stringValue);
            pointer.Add(stringValue.Length);
        }

        /// <summary>
        /// Sets the current object value with zeroes.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetValueWithZeroes(this IGroup instance)
        {
            instance.FillWithChar('0');
        }

        /// <summary>
        /// Sets the current object value with zeroes.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetValueWithZeros(this IGroup instance)
        {
            instance.FillWithChar('0');
        }

        /// <summary>
        /// Sets the current object with the minimum value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetMinValue(this IGroup instance)
        {
            instance.FillWithByte(byte.MinValue);
        }

        /// <summary>
        /// Sets the current object with the maximum value.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetMaxValue(this IGroup instance)
        {
            instance.FillWithByte(byte.MaxValue);
        }

        /// <summary>
        /// Set a buffer refence from buffer pointer
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="bufferField">Specifies an IField object for the reference.</param>
        public static void SetBufferReference(this IGroup instance, IField bufferField)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");
            if (bufferField == null)
                throw new ArgumentNullException("element", "element is null.");

            instance.SetAddressToAddressOf(bufferField);
        }

        /// <summary>
        /// Set Buffer refrence from another group buffer
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="bufferGroup">Specifies an IGroup object for the reference.</param>
        public static void SetBufferReference(this IGroup instance, IGroup bufferGroup)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");
            if (bufferGroup == null)
                throw new ArgumentNullException("element", "element is null.");

            instance.SetAddressToAddressOf(bufferGroup);
        }

        /// <summary>
        /// Set Buffer refrence from another record buffer
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="bufferGroup">Specifies an IRecord object for the reference.</param>
        public static void SetBufferReference(this IGroup instance, IRecord bufferRecord)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");
            if (bufferRecord == null)
                throw new ArgumentNullException("element", "element is null.");

            instance.SetAddressToAddressOf(bufferRecord);
        }

        /// <summary>
        /// Returns a substring of IGroup value (position based)
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the number of characters in the substring.</param>
        /// <returns>Returns a substring that is extracted from the current object value.</returns>
        public static string GetSubstring(this IGroup instance, int startPosition, int length)
        {
            if ((startPosition - 1) + length < instance.LengthInBuffer)
                return instance.AsString().Substring(startPosition - 1, length);
            else
                return instance.AsString().Substring(startPosition - 1);
        }

        /// <summary>
        /// Returns a substring of IGroup value (position based)
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <returns>Returns a substring that is extracted from the current object value.</returns>
        public static string GetSubstring(this IGroup instance, int startPosition)
        {
                return instance.AsString().Substring(startPosition - 1);
        }

        /// <summary>
        /// Returns corresponding group elements values
        /// </summary>
        /// <param name="instance">The instance of the receiving field.</param>
        /// <param name="value">The group's elements values that need to be moved to the corresponding elements in the receiving group.</param>
        /// <returns>Retuns an instance with the updated elemets values.</returns>
        public static IGroup SetValueCorresponding(this IGroup instance, IGroup value)
        {
            string name1 = "";
            string name2 = "";
            int resultIndex;

            foreach (IBufferElement target in instance.Elements)
            {
                resultIndex = target.Name.IndexOf("_d");
                if (resultIndex != -1)
                    name1 = target.Name.Substring(0, resultIndex);
                else
                    name1 = target.Name;
                
                foreach (IBufferElement input in value.Elements)
                {
                    resultIndex = input.Name.IndexOf("_d");
                    if (resultIndex != -1)
                        name2 = input.Name.Substring(0, resultIndex);
                    else
                        name2 = input.Name;

                    var tempInput = (IBufferValue)input;

                    if (name1 == name2)
                    {
                        IBufferValue tempTarget = (IBufferValue)target;
                        tempTarget.SetValue(tempInput);
                        
                        break;
                    }
                }
            }
            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IGroup GetAcceptData(string text)
        {
            throw new Exception("GetAcceptData - Not Implemented");
        }

        public static string ReverseCharacters(this IGroup instance)
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
        /// Inserts a string at the specified start position.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="checkInsertString">The check insert string.</param>
        public static string Insert(this IGroup instance, IField startPosition, IField checkInsertString)
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
        public static string Insert(this IGroup instance, IField startPosition, string checkInsertString)
        {
            return instance.Insert(startPosition.AsInt(), checkInsertString.AsString());
        }
        /// <summary>
        /// Inserts a string at the specified start position.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="checkInsertString">The check insert string.</param>
        public static string Insert(this IGroup instance, int startPosition, IField checkInsertString)
        {
            return instance.Insert(startPosition, checkInsertString.AsString());
        }
        /// <summary>
        /// Inserts a string at the specified start position.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="checkInsertString">The check insert string.</param>
        public static string Insert(this IGroup instance, int startPosition, string checkInsertString)
        {
            return instance.AsString().Insert(startPosition - 1, checkInsertString);
        }
        /// <summary>
        /// Set Group with bytes if byte array is not null
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="bytes"></param>
        public static void SetValueWithNullCheck(this IGroup instance, byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
                instance.Assign(bytes);
        }

        #endregion
    }
}
