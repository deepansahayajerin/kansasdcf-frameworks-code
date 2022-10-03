
//using MDSY.Framework.Buffer.BaseClasses;
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
    /// The global scope structure that provides a temporary storage for the record name value, which is associated with the current ID column value.
    /// </summary>
    public static class FieldEx
    {
        public static string IdRecordName = "";
    }

    /// <summary>
    /// Extension methods for objects with implement IBufferValue 
    /// </summary>
    public static class IBufferValueExtensions
    {
        /// <summary>
        /// Fills the buffer value object with the given <paramref name="fillByte"/>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="fillByte">Byte to fill the buffer.</param>
        public static void FillWithByte(this IBufferValue instance, byte fillByte)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");

            if (instance is IBufferElement)
            {
                var bytes = Enumerable.Repeat(fillByte, (instance as IBufferElement).LengthInBuffer).ToArray();
                instance.AssignFrom(bytes);
            }
            else
            {
                var bytes = Enumerable.Repeat(fillByte, instance.Buffer.Length).ToArray();
                instance.AssignFrom(bytes);
            }

        }

        /// <summary>
        /// Fills the buffer value object with the given <paramref name="fillChar"/>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="fillChar">A reference to the AsciiChar object, which value should fill the buffer.</param>
        public static void FillWithChar(this IBufferValue instance, AsciiChar fillChar)
        {
            if (instance == null) return;
            instance.FillWithByte(fillChar.AsByte);
        }


        /// <summary>
        /// Fills the buffer value object with the given <paramref name="fillChar"/>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="fillChar">A reference to the char value that should fill the buffer.</param>
        public static void FillWithChar(this IBufferValue instance, char fillChar)
        {
            if (instance == null) return;
            instance.FillWithChar(AsciiChar.From(fillChar));
        }

        /// <summary>
        /// Fills the buffer value object with the given <paramref name="fillChar"/>.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="fillChar">A string for filling the buffer.</param>
        public static void FillWithChar(this IBufferValue instance, string fillChar)
        {
            if (instance == null) return;
            instance.FillWithChar(fillChar[0]);
        }


        /// <summary>
        /// Fills the object's buffer location with space characters.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        public static void SetValueWithSpaces(this IBufferValue instance)
        {
            if (instance == null) return;
            instance.FillWithChar(' ');
        }

        /// <summary>
        /// Fill all with a particular byte value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">A byte, which is used to fill the buffer of the current object.</param>
        public static void SetValueAll(this IBufferValue instance, byte value)
        {
            if (instance == null) return;
            instance.FillWithByte(value);
        }
        /// <summary>
        /// Fill all with a particular value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">A string, which first character is used to fill the buffer of the current object.</param>
        public static void SetValueAll(this IBufferValue instance, string value)
        {
            if (instance == null) return;
            instance.FillWithChar(value[0]);
        }
        /// <summary>
        /// Fill all with a particular value
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">Char value to fill the buffer of the current object.</param>
        public static void SetValueAll(this IBufferValue instance, char value)
        {
            if (instance == null) return;
            instance.FillWithChar(value);
        }

        /// <summary>
        /// Inspects the value of the current object and replaces specified characters with the provided characters.
        /// </summary>
        /// <param name="instance">A reference to the curren object.</param>
        /// <param name="valueToSearch">The value against which the search should be provided.</param>
        /// <param name="searchType">Specifies search option. Can take values "LEADING", "ALL" or "FIRST".</param>
        /// <param name="searchFor">Specifies the value to search for.</param>
        /// <param name="searchReplace">Specifies the value that should be used for replacement.</param>
        /// <param name="beforeOption">Not used. Can take any value. </param>
        /// <param name="beforeParm">Not used. Can take any value.</param>
        /// <param name="afterOption">Not used. Can take any value.</param>
        /// <param name="afterParm">Not used. Can take any value.</param>
        public static void SetValueInspectReplacing(this IBufferValue instance, IBufferValue valueToSearch, string searchType, string searchFor, string searchReplace, string beforeOption, string beforeParm, string afterOption, string afterParm)
        {
            if (instance == null) return;

            if (searchType == "LEADING")
            {
                byte[] bytes = valueToSearch.AsBytes;
                bool stopSearch = false;
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (!stopSearch)
                    {
                        char searchChar = Convert.ToChar(searchFor);
                        char replaceChar = Convert.ToChar(searchReplace);
                        if ((searchChar == (Char)bytes[i]))
                            bytes[i] = Convert.ToByte(replaceChar);
                        else
                            stopSearch = true;
                    }
                }
                instance.Assign(bytes);
            }
            else if (searchType == "ALL")
            {
                //string search = valueToSearch.BytesAsString;
                //string replacedSearch = "";
                //for (int i = 0; i < search.Length; i++)
                //{
                //    string searchChar = search.Substring(i, 1);
                //    int idx = 0;
                //    idx = searchFor.IndexOf(searchChar);
                //    if (idx == -1)
                //    {
                //        replacedSearch += searchChar;
                //    }
                //    else
                //    {
                //        replacedSearch += searchReplace.Substring(idx, 1);
                //    }
                //}

                string replacedSearch = valueToSearch.BytesAsString.Replace(searchFor, searchReplace);
                instance.Assign(replacedSearch);
            }
            else if (searchType == "FIRST")
            {
                instance.Assign(valueToSearch.BytesAsString.ReplaceFirstOccurrance(searchFor, searchReplace));
            }
            else
                throw new ApplicationException("Unknown search type in INSPECT ... REPLACING statement. (searchType = '" + searchType + "')");

        }


        /// <summary>
        /// Set Substring value with repeating bytes
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="fillWith">A byte value to fill the substring with.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the number of repeating bytes to be set.</param>
        public static void SetValueOfSubstringAll(this IBufferValue instance, int startPosition, int length, byte fillWith)
        {
            if (instance == null) return;

            byte[] workBytes = instance.AsBytes;
            if (workBytes.Length < (startPosition + length - 1))
            {
                throw new ApplicationException("SetValueOfSubstringAll: 'Fill With' length would overflow target substring.");
            }

            int ixOut = startPosition - 1;

            for (int i = 0; i < length; i++)
            {
                workBytes[ixOut + i] = fillWith;
            }

            instance.Assign(workBytes);
        }

        /// <summary>
        /// Set Substring value with repeating bytes
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="fillWith">A char value to fill the substring with.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the number of repeating chars to be set.</param>
        public static void SetValueOfSubstringAll(this IBufferValue instance, int startPosition, int length, char fillWith)
        {
            if (instance == null) return;

            byte fillWithByte = (byte)fillWith;
            byte[] workBytes = instance.AsBytes;
            if (workBytes.Length < (startPosition + length - 1))
            {
                throw new ApplicationException("SetValueOfSubstringAll: 'Fill With' length would overflow target substring.");
            }
            int ixOut = startPosition - 1;

            for (int i = 0; i < length; i++)
            {
                workBytes[ixOut + i] = fillWithByte;
            }

            instance.Assign(workBytes);
        }

        /// <summary>
        /// Set Substring value with repeating string
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="fillWith">A byte value to fill the substring with.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the size in bytes of the new value to be set.</param>
        public static void SetValueOfSubstringAll(this IBufferValue instance, int startPosition, int length, string fillWith)
        {
            if (instance == null) return;

            StringBuilder sb = new StringBuilder("");
            while (sb.Length < length)
            {
                sb.Append(fillWith);
            }
            string newValue = sb.ToString().Substring(0, length);

            byte[] workBytes = instance.AsBytes;
            if (workBytes.Length < (startPosition + length))
            {
                throw new ApplicationException("SetValueOfSubstringAll: 'Fill With' length would overflow target substring.");
            }
            byte[] fillBytes = newValue.ToAsciiCharArray().ToByteArray();
            int ixOut = startPosition - 1;

            for (int i = 0; i < length; i++)
            {
                workBytes[ixOut + i] = fillBytes[i];
            }

            instance.Assign(workBytes);
        }

        /// <summary>
        /// Assigns provided value to the current object.
        /// This is semantically the same as calling <c><paramref name="instance"/>.Assign(<paramref name="value"/>);</c>
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">Specifies the value to be assigned.</param>
        public static void SetValue(this IBufferValue instance, int value)
        {
            if (instance == null) return;

            instance.AssignIdRecordName(FieldEx.IdRecordName);
            FieldEx.IdRecordName = "";

            instance.Assign(value);
        }

        /// <summary>
        /// Assigns provided value to the current object.
        /// This is semantically the same as calling <c><paramref name="instance"/>.Assign(<paramref name="value"/>);</c>
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">Specifies the value to be assigned.</param>
        public static void SetValue(this IBufferValue instance, int? value)
        {
            if (instance == null) return;

            if (value == null)
                value = 0;
            instance.AssignIdRecordName(FieldEx.IdRecordName);
            FieldEx.IdRecordName = "";

            instance.Assign(value);
        }
        /// <summary>
        /// Set Substring value with spaces
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the number of spaces to be set.</param>
        public static void SetValueWithSpacesOfSubstring(this IField instance, int startPosition, int length)
        {
            if (instance == null) return;

            instance.Assign(instance.AsString().Remove(startPosition - 1, length).Insert(startPosition - 1, string.Empty.PadRight(length)));
        }
        /// <summary>
        /// Set Substring value with spaces
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="startPosition"></param>
        public static void SetValueWithSpacesOfSubstring(this IField instance, int startPosition)
        {
            if (instance == null) return;

            int length = instance.AsString().Length - startPosition + 1;
            instance.Assign(instance.AsString().Remove(startPosition - 1, length).Insert(startPosition - 1, string.Empty.PadRight(length)));
        }

        /// <summary>
        /// Set a Substring value with all zeroes
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="startPosition"></param>
        /// <param name="length"></param>
        public static void SetValueWithZeroesOfSubstring(this IField instance, int startPosition, int length)
        {
            if (instance == null) return;

            instance.Assign(instance.AsString().Remove(startPosition - 1, length).Insert(startPosition - 1, string.Empty.PadRight(length, '0')));
        }

        /// <summary>
        /// Assigns provided value to the current object.
        /// This is semantically the same as calling <c><paramref name="instance"/>.Assign(<paramref name="value"/>);</c>
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">Specifies the value to be assigned.</param>
        public static void SetValue(this IBufferValue instance, decimal value)
        {
            if (instance == null) return;

            instance.Assign(value);
        }

        /// <summary>
        /// Assigns provided value to the current object.
        /// This is semantically the same as calling <c><paramref name="instance"/>.Assign(<paramref name="value"/>);</c>
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">Specifies the value to be assigned.</param>
        public static void SetValue(this IBufferValue instance, decimal? value)
        {
            if (instance == null) return;

            if (value == null)
                value = 0;
            instance.Assign(value);
        }
        /// <summary>
        /// Assigns provided value to the current object. 
        /// Resets current object to its initial value is the provided string is null or empty.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">Specifies the value to be assigned.</param>
        public static void SetValue(this IBufferValue instance, string value)
        {
            if (instance == null) return;

            if (string.IsNullOrEmpty(value))
                instance.ResetToInitialValue();
            else
                instance.AssignFrom(value);
        }

        /// <summary>
        /// Assigns provided value to the current object.
        /// This is semantically the same as calling <c><paramref name="instance"/>.AssignFrom(<paramref name="value"/>);</c>
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">Specifies the value to be assigned.</param>
        public static void SetValue(this IBufferValue instance, byte[] value)
        {
            if (instance == null) return;

            instance.AssignFrom(value);
        }

        /// <summary>
        /// Assigns provided value to the current object.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">Specifies the value to be assigned.</param>
        public static void SetValue(this IBufferValue instance, IBufferValue value)
        {
            if (instance == null) return;
            instance.AssignFrom(value);
        }

        /// <summary>
        /// Assigns provided date value to the current object.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="dateValue">Specifies the date value to be assigned.</param>
        public static void SetValue(this IBufferValue instance, DateTime dateValue)
        {
            if (instance == null) return;

            string value = dateValue.ToString("yyyyMMdd");
            if (value == "00010101")
                value = "00000000";
            instance.AssignFrom(value);
        }

        /// <summary>
        /// Sets the value of the buffer value object to the given byte array of a Record buffer <paramref name="value"/>. 
        /// </summary>
        //public static void SetValue(this IBufferValue instance, IRecord value)
        //{
        //    instance.AssignFrom(value.AsBytes());
        //}

        /// <summary>
        /// Sets the value of the buffer value object to the value of the given <paramref name="other"/> object. 
        /// This is semantically the same as calling <c><paramref name="instance"/>.AssignFrom(<paramref name="other"/>);</c>
        /// </summary>
        //public static void SetValue(this IBufferValue instance, IBufferValue other)
        //{
        //    instance.AssignFrom(other);
        //}

        /// <summary>
        /// Assigns provided value to the current object.
        /// </summary>
        /// <param name="instance">A reference to the current objec.</param>
        /// <param name="field">A reference to the IField object, which should be assigned to the current object.</param>
        public static void SetValue(this IBufferValue instance, IField field)
        {
            if (field == null) return;
            if (instance == null) return;
            instance.AssignIdRecordName(field.GetIdRecordName());
            instance.AssignFrom(field, field.FieldType);
        }

        /// <summary>
        /// Assigns provided value to the current object.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="group">A reference to the IGroup object, which value should be assigned to the current object.</param>
        public static void SetValue(this IBufferValue instance, IGroup group)
        {
            if (instance == null || group == null) return;
            //Change for specific AssignFromGroup method - issue 5781
            instance.AssignFromGroup(group);
        }

        /// <summary>
        /// Assigns provided value to the current object.
        /// </summary>
        /// <param name="instance">A reference to the current object.</param>
        /// <param name="value">A reference to the IRecord object, which value should be assigned to the current object.</param>
        public static void SetValue(this IBufferValue instance, IRecord value)
        {
            if (instance == null) return;
            instance.AssignFrom(value.AsBytes());
        }
    }
}
