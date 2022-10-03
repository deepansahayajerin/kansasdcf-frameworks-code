using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Provides methods for the conversion between Comp3 value and array of bytes.
    /// </summary>
    public static class Comp3Serializer
    {
        #region private

        // Packed decimal Byte to digits Conversion Table. nulls represent invalid values.
        private static readonly string[] PackedByteToChars =
        {
            "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", null, null, null, null, null, null,
            "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", null, null, null, null, null, null,
            "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", null, null, null, null, null, null,
            "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", null, null, null, null, null, null,
            "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", null, null, null, null, null, null,
            "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", null, null, null, null, null, null,
            "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", null, null, null, null, null, null,
            "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", null, null, null, null, null, null,
            "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", null, null, null, null, null, null,
            "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null
        };

        // Packed decimal Byte to digits and sign Conversion Table. nulls represent invalid values.
        private static readonly string[] PackedByteWithSignToChars =
        {
            null, null, null, null, null, null, null, null, null, null, null, null, "0C", "0D", null, "0F",
            null, null, null, null, null, null, null, null, null, null, null, null, "1C", "1D", null, "1F",
            null, null, null, null, null, null, null, null, null, null, null, null, "2C", "2D", null, "2F",
            null, null, null, null, null, null, null, null, null, null, null, null, "3C", "3D", null, "3F",
            null, null, null, null, null, null, null, null, null, null, null, null, "4C", "4D", null, "4F",
            null, null, null, null, null, null, null, null, null, null, null, null, "5C", "5D", null, "5F",
            null, null, null, null, null, null, null, null, null, null, null, null, "6C", "6D", null, "6F",
            null, null, null, null, null, null, null, null, null, null, null, null, "7C", "7D", null, "7F",
            null, null, null, null, null, null, null, null, null, null, null, null, "8C", "8D", null, "8F",
            null, null, null, null, null, null, null, null, null, null, null, null, "9C", "9D", null, "9F",
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null
        };

        private static int nybble(byte[] bytes, int nybbleIndex)
        {
            int b = bytes[bytes.Length - 1 - nybbleIndex / 2];
            return (nybbleIndex % 2 == 0) ? (b & 0x0000000F) : (b >> 4);
        }

        /// <summary>
        /// Returns <c>true</c> if the 0th nybble (first on right) is 
        /// a proper PackedDecimal signage value (i.e. 0x0C, 0x0D, or0x0F).
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static bool NybbleZeroIsSign(byte[] bytes)
        {
            int[] validNybbles = new int[] { 0x0C, 0x0D, 0x0F };
            int n = nybble(bytes, 0);
            return validNybbles.Contains(n);
        }

        private static bool IsPackedBytes(byte[] bytes)
        {
            bool result = true;
            long nybble;

            result = NybbleZeroIsSign(bytes);

            int i = 1;
            while (result && i < (bytes.Length * 2) - 1)
            {
                result = TryGetPackedNybble(bytes, i, out nybble);
                i++;
            }
            return result;
        }

        private static bool TryGetPackedNybble(byte[] bytes, int nybbleIndex, out long digit)
        {
            digit = nybble(bytes, nybbleIndex);
            return (digit <= 9);
        }
        #endregion

        #region public methods

        /// <summary>
        /// Returns a COMP-3 type packed byte array for the given <paramref name="numericText"/>..
        /// </summary>
        /// <param name="numericText">A string that contains numeric value.</param>
        /// <returns>Returns a COMP-3 type packed byte array.</returns>
        public static byte[] GetPackedBytes(string numericText)
        {
            var bytes = new List<byte>();
            byte signByte = 0x3F;
            var negativeSign = NumberFormatInfo.NegativeSign;
            var positiveSign = NumberFormatInfo.PositiveSign;

            for (var i = 0; i < numericText.Length; i++)
            {
                char c = numericText[i];
                if (i == 0)
                {
                    bool isNegative = true;
                    bool isPositive = true;

                    if (c == negativeSign[0])
                    {
                        for (int k = 1; k < negativeSign.Length; k++)
                        {
                            if (numericText[k] == negativeSign[k]) continue;
                            isNegative = false;
                            break;
                        }
                        if (isNegative)
                        {
                            signByte = 0x3D;
                            i += negativeSign.Length - 1;
                            continue;
                        }
                    }


                    if (c == positiveSign[0])
                    {
                        for (int k = 1; k < positiveSign.Length; k++)
                        {
                            if (numericText[k] == positiveSign[k]) continue;
                            isPositive = false;
                            break;
                        }
                        if (isPositive)
                        {
                            signByte = 0x3C;
                            i += positiveSign.Length - 1;
                            continue;
                        }
                    }


                }

                if (c == DecimalSeparatorChar)
                    continue;

                bytes.Add((byte)c);
            }


            bytes.Add(signByte);
            if (bytes.Count % 2 != 0) // odd number of bytes.
            {
                bytes.Insert(0, 0xF0);
            }

            var result = new byte[bytes.Count / 2];
            int j = 0;
            while (j < bytes.Count - 1)
            {
                byte packedByte = (byte)(((bytes[j] & 0x0f) << 4) | (bytes[j + 1] & 0x0f));
                result[j / 2] = packedByte;
                j += 2;
            }

            return result;
        }

        /// <summary>
        /// Returns a Decimal value for the given packed <paramref name="bytes"/>; <paramref name="digits"/>
        /// specifies how many digits should be to the right of the inserted decimal separator.
        /// </summary>
        /// <remarks><note>This code was largely taken as-is from online sample code; it 
        /// needs some clean-up and renaming.</note></remarks>
        /// <param name="bytes">A byte array that contains packed numeric value.</param>
        /// <param name="digits">Specifies how many digits should be to the right of the inserted decimal separator.</param>
        /// <returns>Returns a Decimal value that corresponds to the provided packed value.</returns>
        public static Decimal GetDecimalFromPackedBytes(byte[] comp3, int scale)
        {

            // populate an array with the digits of the packed number

            int digitArrayLength = comp3.Length * 2 - 1;

            int[] digitArray = new int[digitArrayLength];


            // populate the array with all but the last byte of the comp 3 array
            int comp3Index = 0;
            int digitArrayIndex = 0;
            for (; comp3Index < comp3.Length - 1; comp3Index++)
            {
                string twoChars = PackedByteToChars[comp3[comp3Index]];

                if (twoChars == null)
                {
                    //PackedDecimalException("The given packed byte was not in proper format.");
                    return 0M;
                }

                for (int j = 0; j < 2; j++)
                {
                    digitArray[digitArrayIndex++] = twoChars[j] - '0';
                }

            }

            //handle the last byte of the comp3, 1 digit and the sign

            string lastChars = PackedByteWithSignToChars[comp3[comp3Index]];

            if (lastChars == null)
            {
                //PackedDecimalException("The given packed byte was not in proper format.");
                return 0M;
            }

            digitArray[digitArrayIndex] = lastChars[0] - '0';


            // the sign
            bool isNegative = (lastChars[1] == 'D');


            // Building the actual decimal components: 96 bit integer, Sign and scale 
            long lo = 0;
            long mid = 0;
            long hi = 0;
            const uint bitFlip = 0xffffffff;
            long intermediate;
            long carry;

            for (digitArrayIndex = 0; digitArrayIndex < digitArrayLength; digitArrayIndex++)
            {
                // multiply by 10
                intermediate = lo * 10;
                lo = intermediate & bitFlip;
                carry = intermediate >> 32;
                intermediate = mid * 10 + carry;
                mid = intermediate & bitFlip;
                carry = intermediate >> 32;
                intermediate = hi * 10 + carry;
                hi = intermediate & bitFlip;
                //Cleanup carry = intermediate >> 32;
                // By limiting input length to 14, we ensure overflow will never occur

                intermediate = lo + digitArray[digitArrayIndex];
                lo = intermediate & bitFlip;
                carry = intermediate >> 32;
                if (carry > 0)
                {
                    intermediate = mid + carry;
                    mid = intermediate & bitFlip;
                    carry = intermediate >> 32;
                    if (carry > 0)
                    {
                        intermediate = hi + carry;
                        hi = intermediate & bitFlip;
                        // Duplicate assignment carry = intermediate >> 32;
                        // carry should never be non-zero. Back up with validation
                    }
                }
            }

            // return the constructed Decimal value
            return new Decimal((int)lo, (int)mid, (int)hi, isNegative, (byte)scale);

        }

        #endregion

        #region Culture-based

        /// <summary>
        /// Returns the number format information that defines the culturally
        ///  appropriate format of displaying numbers, currency, and percentage.
        /// </summary>
        public static readonly NumberFormatInfo NumberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;

        /// <summary>
        /// Returns a decimal separator character as a string.
        /// </summary>
        public static readonly string DecimalSeparator = NumberFormatInfo.NumberDecimalSeparator;

        /// <summary>
        /// Returns a decimal separator character.
        /// </summary>
        public static readonly char DecimalSeparatorChar = DecimalSeparator[0];


        #endregion

    }
}
