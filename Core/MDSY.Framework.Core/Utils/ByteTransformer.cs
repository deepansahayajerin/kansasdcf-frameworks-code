using MDSY.Framework.Buffer.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Utilities for specialized conversion to and from byte arrays. 
    /// </summary>
    public static class ByteTransformer
    {
        #region private methods
        private static string TruncateValue(string value, int length, FieldType fieldType)
        {
            string result = value;
            if (length <= 0)
            {
                result = string.Empty;
            }
            else if (value.Length > length)
            {
                if (fieldType == FieldType.String)
                {
                    result = value.Substring(0, length);
                }
                else
                {
                    result = value.Substring(value.Length - length, length);
                }
            }

            return result;
        }
        #endregion

        #region internal static
        //TODO: this method MUST be refactored.
        internal static string ProcessNonCompressedBufferValue(string value, int decimalLength, int length, FieldType fieldType)
        {
            string result = value;

            /// are we doing decimal points?
            if (decimalLength > 0)
            {
                string decimalStr = string.Empty;

                if (result.Contains(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
                {
                    decimalStr = result.Substring(result.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator) + 1);
                    if (decimalStr.Length > decimalLength) /// are we bigger? We need to truncate
                    {
                        decimalStr = decimalStr.Substring(0, decimalLength);
                    }
                    else if (decimalStr.Length < decimalLength) /// are we smaller? We need to fill with zeros
                    {
                        decimalStr = decimalStr.PadRight(decimalLength, '0');
                    }

                    result = result.Substring(0, result.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator));
                }
                else
                {
                    decimalStr = string.Empty.PadRight(decimalLength, '0');
                }

                result += decimalStr;
            }
            else
            {
                /// if we have any decimals we need to truncate them
                if (result.Contains(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
                {
                    result = result.Substring(0, result.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator));
                }
            }

            /// this is now doing what Software AG Natural does
            if (result.Contains("-"))
            {
                /// remove the -
                result = result.Replace("-", "");

                /// create the character that this is negative at the end
                char lastChar = (char)(result[result.Length - 1] + (char)DBSBaseOption.Negative);
                result = result.Remove(result.Length - 1) + lastChar;
            }

            if (result.Contains(" "))
            {
                result = result.Trim();
            }

            if (result.Length > length)
            {
                result = TruncateValue(result, length, fieldType);
                //throw new OverflowException(String.Format("The number trying to get into the buffer is too big. Expected Length:{0} Value sent:{1}", length, result));
            }

            /// now pad it to fit the right size.
            return result.PadLeft(length, '0');
        }

        internal static string CompressValue(string passedValue,
            int decimalLength,
            int length,
            int fieldBufferLength,
            FieldType fieldType)
        {
            string result = passedValue;

            /// this will make sure we truncate the number correctly.
            if (decimalLength > 0)
            {
                if (result.Contains(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
                {
                    string decimalValue = result.Substring(result.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator) + 1);
                    if (decimalValue.Length > decimalLength)
                    {
                        decimalValue = decimalValue.Substring(0, decimalLength);
                        result = result
                            .Substring(0, result.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)) +
                            NumberFormatInfo.CurrentInfo.NumberDecimalSeparator +
                            decimalValue;
                    }
                }
                /// remove '-' character and the decimal separator and check the length
                if (result
                        .Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "")
                        .Replace(NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator, "")
                        .Replace("-", "").Length > length)
                {
                    result = result.Substring(result.Length - length);
                    //throw new OverflowException(String.Format("The number is too large to fit in a compressed field of size {0}. The current number is {1}",
                    //                               length,
                    //                                result));
                }
            }

            /// Set up flags, strings, and counters
            bool isNegative = false;
            bool containsPlus = false;
            string hex;
            string valueDec = string.Empty;
            byte[] bytes = new byte[fieldBufferLength];
            int cntC = result.Length;
            int cntB = bytes.Length - 1;
            int cntD = 0;

            // Check for Compress type; Packed Decimal, CompShort, CompInt, CompLong
            if (fieldType == FieldType.PackedDecimal)
            {
                containsPlus = (result.Contains("+"));

                decimal testDec;
                if (decimal.TryParse(result, out testDec))
                {
                    isNegative = (testDec < 0);
                }

                if (result.Contains(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
                {
                    valueDec = result.Substring(result.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator) + 1);
                }

                result = result.Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "").Replace("-", "").Replace("+", "");
                if (decimalLength > valueDec.Length)
                {
                    cntD = decimalLength - valueDec.Length;
                    //result = result + "".PadRight(cntD, '0');
                    result = result.PadRight(result.Length + cntD, '0');
                }

                // Change PAcked decimal sign: + = C, - = D, unassigned = F
                if (isNegative)
                {
                    result = result + "D";
                }
                else if (containsPlus)
                {
                    result = result + "C";
                }
                else
                {
                    result = result + "F";
                }

                cntC = result.Length;
            }
            else
            // If not Packed Decimal, must be CompShort, CompInt, or CompLong; Then get hex for Binary number
            {
                long testLong;
                if (long.TryParse(result, out testLong))
                {
                    //if (testLong < 0)
                    //{
                    //    // Not needed result = result.Replace("-", "");
                    //    // Not needed isNegative = true;
                    //}
                    string hexString = testLong.ToString("X").PadLeft(16, '0');
                    result = hexString.Substring(16 - (fieldBufferLength * 2), fieldBufferLength * 2);
                    cntC = result.Length;
                }
            }

            //Iterate through Hex chracters and create byte array
            while (cntC > 0)
            {
                if (cntC == 1)
                {
                    hex = new string(new Char[] { '0', result[cntC - 1] });
                }
                else
                {
                    hex = new string(new Char[] { result[cntC - 2], result[cntC - 1] });
                }
                cntC -= 2;
                bytes[cntB] = ConvertHexToByte(hex);
                cntB--;
            }
            while (cntB > 0)
            {
                bytes[cntB] = ConvertHexToByte("00");
                cntB--;
            }
            //// *** Convert bytes to Unicode string
            //result = Encoding.Default.GetString(bytes);
            //// *****************************************

            return ByteTransformer.BytesToString(bytes);
        }

        internal static byte ConvertHexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            return byte.Parse(hex, NumberStyles.HexNumber);
        }

        #endregion

        #region FromBytes
        /// <summary>
        /// Converts the given byte array into its string representation via char[] rather than by encoding.
        /// </summary>
        /// <param name="bytes">The byte[] to transform.</param>
        /// <returns>A new string instance.</returns>
        public static string BytesToString(byte[] bytes)
        {
            return bytes.Select(b => AsciiChar.From(b)).NewString();
            //char[] chars = new char[bytes.Length];

            //for (int i = 0; i < bytes.Length; i++)
            //{
            //    chars[i] = (char)bytes[i];
            //}

            //return new string(chars);
        }
        #endregion

        #region ToBytes
        //public static byte[] ToBytes(Int32 value, int fieldLength, bool isCompressed)
        //{
        //    string valueStr;
        //    if (isCompressed)
        //    {
        //        int fieldBufferLength = 0;
        //        FieldType fieldType = FieldType.String;
        //        valueStr = CompressValue(value.ToString(), 0, fieldLength, fieldBufferLength, fieldType);
        //    }
        //    else
        //    {
        //        valueStr = ProcessNonCompressedBufferValue(value.ToString(), 0, fieldLength);
        //    }
        //    return ToBytes(value.ToString());
        //}

        //public static byte[] ToBytes(Int64 value, int fieldLength, bool isCompressed)
        //{
        //    return ToBytes(value.ToString());
        //}

        /// <summary>
        /// Converts the given string value into its bytes via char[] rather than by encoding.
        /// </summary>
        /// <param name="value">The given string value to be converted</param>
        /// <returns></returns>
        public static byte[] ToBytes(string value)
        {
            byte[] result = new byte[value.Length];
            var chars = value.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                result[i] = (byte)chars[i];
            }

            return result;
        }
        #endregion
    }
}

