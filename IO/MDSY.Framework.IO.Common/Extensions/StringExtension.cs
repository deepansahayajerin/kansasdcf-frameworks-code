using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.IO.Common
{

    public static class StringExtension
    {
        public static List<byte> ToByteList(this string value)
        {
            // Performance tweak - RKL
            char[] chars = value.ToCharArray();
            byte[] result = new byte[chars.Length];

            for (int i = 0; i < chars.Length; i++)
            {
                result[i] = (byte)chars[i];
            }

            return result.ToList();
        }

        /// <summary>
        /// Returns a new string that right-aligns the string in this instance by
        ///  padding them on the left with a specified Unicode character, for a specified
        ///  total length.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="totalWidth">
        /// The number of characters in the resulting string, equal to the number of
        ///  original characters plus any additional padding characters.
        /// </param>
        /// <param name="paddingString">
        /// A string padding.
        /// </param>
        /// <returns>
        /// A new string that is equivalent to this instance, but right-aligned and padded
        ///  on the left with as many paddingChar characters as needed to create a length
        ///  of totalWidth. Or, if totalWidth is less than the length of this instance,
        ///  a new string that is identical to this instance.
        /// </returns>
        public static string PadLeft(this string value, int totalWidth, string paddingString)
        {
            StringBuilder returnValue = new StringBuilder(value);

            while (returnValue.Length < totalWidth)
            {
                returnValue.Insert(0, paddingString);
            }

            while (returnValue.Length > totalWidth)
            {
                returnValue.Remove(0, 1);
            }

            return returnValue.ToString();
        }
        /// <summary>
        /// Gets a formated string in ASCII representation, from ASCII
        /// hex to string.
        /// </summary>
        /// <param name="strASCIIHex"></param>
        /// <param name="format">If set to "X" will return a hex decimal format of this string</param>
        /// <returns></returns>
        public static string ParseASCIIHex(this string value)
        {
            string hexDecodedValue = string.Empty;

            for (int count = 0; count < value.Length; count += 2)
            {
                byte byteValue = Convert.ToByte(value[count].ToString() + value[count + 1].ToString(), 16);
                hexDecodedValue += (char)byteValue;
            }

            return hexDecodedValue;
        }
        /// <summary>
        /// Gets a formated hex ASCII code from this string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToASCIIHex(this string value)
        {
            string asciiHex = string.Empty;

            for (int count = 0; count < value.Length; count++)
            {
                asciiHex += ((byte)value[count]).ToString("X");
            }

            return asciiHex;
        }
        /// <summary>
        /// This will pad the field in both sides to leave
        /// the string on the middle.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Pad(this string value, int totalWidth, char paddingChar = ' ')
        {
            StringBuilder returnValue = new StringBuilder();

            int padMissing = totalWidth - value.Length;

            if (padMissing > 0)
            {
                returnValue.Append(string.Empty.PadLeft(padMissing / 2, paddingChar));
                returnValue.Append(value);
                returnValue.Append(string.Empty.PadLeft(padMissing / 2, paddingChar));

                /// add one more to the end if this was a odd number length.
                if (padMissing % 2 > 0)
                {
                    returnValue.Append(string.Empty.PadLeft(1, paddingChar));
                }
            }
            else
            {
                returnValue.Append(value);
            }

            return returnValue.ToString();
        }
        public static string PadRight(this string value, int totalWidth, string paddingString = " ")
        {
            StringBuilder returnString = new StringBuilder(value.TrimEnd());

            for (int index = 0; returnString.Length < totalWidth; index++)
            {
                returnString.Append(paddingString);
            }

            /// remove the extra
            if (returnString.Length > totalWidth)
            {
                returnString = returnString.Remove(totalWidth, returnString.Length - totalWidth);
            }

            return returnString.ToString();
        }

/*
        // Following Method was commented out as it already exists in MDSY.Framework.Core.String Extensions
        /// <summary>
        /// Returns <c>true</c> if the instance string is greater than another string 
        /// </summary>
        public static bool IsGreaterThan(this string instance, string value)
        {
            return instance.CompareTo(value) > 0;
        }
*/
        /// <summary>
         /// Returns <c>true</c> if the instance string is greater than or equal to than another string 
        /// </summary>
        //public static bool IsGreaterThanOrEqualTo(this string instance, string value)
        //{
        //    return instance.CompareTo(value) >= 0;
        //}

/*
        // Following Method was commented out as it already exists in MDSY.Framework.Core.String Extensions
        /// <summary>
        /// Returns <c>true</c> if the instance string is less than another string 
        /// </summary>
        public static bool IsLessThan(this string instance, string value)
        {
            return instance.CompareTo(value) < 0;
        }
*/
        /// <summary>
        /// Returns <c>true</c> if the instance string is less or equal to than another string 
        /// </summary>
        //public static bool IsLessThanOrEqualTo(this string instance, string value)
        //{
        //    return instance.CompareTo(value) <= 0;
        //}

    }
}
