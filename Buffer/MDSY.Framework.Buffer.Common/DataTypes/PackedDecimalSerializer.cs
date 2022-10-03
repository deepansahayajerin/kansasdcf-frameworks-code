using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Implements the injection interface INumericValueSerializer(of Decimal).
    /// </summary>
    [Serializable]
    [Obsolete]
    internal class PackedDecimalSerializer : INumericValueSerializer<Decimal>
    {

        #region private methods

        /// <summary>
        /// Removes the provided character object from the collection of characters.
        /// </summary>
        /// <param name="valueChars">A reference to the collection of character objects.</param>
        /// <param name="value">Character object to be removed.</param>
        private static void RemoveChar(List<AsciiChar> valueChars, AsciiChar value)
        {
            while (valueChars.Contains(value))
            {
                valueChars.Remove(value);
            }
        }

        /// <summary>
        /// Converts provided decimal value to the array of packed bytes.
        /// </summary>
        /// <param name="value">Decimal value for conversion.</param>
        /// <returns>Returns the array of packed bytes.</returns>
        private static Byte[] DecimalToPackedBytes(Decimal value)
        {
            bool isNeg = value < 0;

            // value to string -> chars so we can manipulate on char level before parsing as hex. 
            // each char will be a nybble.
            var valueChars = value.ToString().ToAsciiCharArray().ToList();
            // append signage char
            valueChars.Add(isNeg ? AsciiChar.PackedNegativeNybble : AsciiChar.PackedPositiveNybble);

            // remove negative sign
            if (isNeg)
            {
                RemoveChar(valueChars, AsciiChar.NegativeSign);
            }

            // get rid of decimal point
            RemoveChar(valueChars, AsciiChar.DecimalPoint);
            RemoveChar(valueChars, AsciiChar.Comma);

            // odd number of nybbles? Insert a 0.
            if (valueChars.Count % 2 == 1)
            {
                valueChars.Insert(0, AsciiChar.From('0'));
            }

            List<byte> result = new List<byte>();
            int i = 0;
            while (i < valueChars.Count - 1)
            {
                result.Add(valueChars.NumCharsToPackedByte(i));
                i += 2;
            }
            return result.ToArray();
        }

        /// <summary>
        /// Converts provided array of packed bytes to the string of hexadecimal characters.
        /// </summary>
        /// <param name="bytes">Array of packed bytes for conversion.</param>
        /// <param name="length">Not used. Can take any value.</param>
        /// <param name="decimalDigits">Specifies the number of digits to the right from the decimal separator.</param>
        /// <returns>Returns the string of hexadecimal characters.</returns>
        private static string PackedBytesToNumString(byte[] bytes, int length, int decimalDigits)
        {
            // ToString returns 4F-FF-07...  strip out the dashes. 
            string hex = BitConverter.ToString(bytes).Replace("-", string.Empty);

            var hexChars = hex.ToAsciiCharArray().ToList();

            bool isNeg = hexChars.Last() == AsciiChar.PackedNegativeNybble;
            // get rid of sign nybble, but add '0' at top to keep even number of chars.
            hexChars.RemoveAt(hexChars.Count - 1);

            if (decimalDigits > 0)
            {
                int decIdx = hexChars.Count - decimalDigits;
                if (Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                {
                    hexChars.Insert(decIdx, AsciiChar.Comma);
                }
                else
                {
                    hexChars.Insert(decIdx, AsciiChar.DecimalPoint);
                }
            }
            if (isNeg)
            {
                hexChars.Insert(0, AsciiChar.NegativeSign);
            }

            return hexChars.NewString();
        }

        /// <summary>
        /// Converts provided packed bytes to a decimal object.
        /// </summary>
        /// <param name="inputBytes">Array of packed bytes for conversion.</param>
        /// <param name="fieldLength">Field length</param>
        /// <param name="decimalDigits">Specifies the number of digits to the right from the decimal separator.</param>
        /// <returns>Returns a new instance of Decimal object.</returns>
        private static Decimal PackedBytesToDecimal(byte[] inputBytes, int fieldLength, int decimalDigits)
        {
            string packedString = PackedBytesToNumString(inputBytes, fieldLength, decimalDigits);

            return Decimal.Parse(packedString);
        }
        #endregion

        #region public methods
        /// <summary>
        /// Deserializes the given byte array into a value of type double.
        /// </summary>
        /// <param name="bytes">The byte array containing the serialized value to be deserialized.</param>
        /// <param name="decimalDigits">The number of digits to the right of the decimal point.</param>
        /// <returns>The <paramref name="bytes"/> deserialized as type <paramref name="T"/>.</returns>
        public Decimal Deserialize(byte[] bytes, int decimalDigits)
        {
            return PackedBytesToDecimal(bytes, bytes.Length * 2, decimalDigits);
        }

        /// <summary>
        /// Deserializes the given byte array into a value of type double.
        /// </summary>
        /// <param name="bytes">The byte array containing the serialized value to be deserialized.</param>
        /// <returns>The <paramref name="bytes"/> deserialized as type <paramref name="T"/>.</returns>
        public Decimal Deserialize(byte[] bytes)
        {
            return PackedBytesToDecimal(bytes, bytes.Length * 2, 0);
        }

        /// <summary>
        /// Serializes the given value to a byte array. 
        /// </summary>
        /// <param name="value">The value to be serialized.</param>
        /// <returns>The value converted to bytes.</returns>
        public byte[] Serialize(Decimal value)
        {
            return DecimalToPackedBytes(value);
        }
        #endregion
    }
}

