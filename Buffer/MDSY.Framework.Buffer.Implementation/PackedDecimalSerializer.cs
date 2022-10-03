using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Unity;
using System.ComponentModel;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface INumericValueSerializer(of Decimal).
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [InjectionImplementer(typeof(INumericValueSerializer<Decimal>))]
    [Serializable]
    internal class PackedDecimalSerializer : INumericValueSerializer<Decimal>
    {

        #region private methods


        private static void RemoveChar(List<AsciiChar> valueChars, AsciiChar value)
        {
            while (valueChars.Contains(value))
            {
                valueChars.Remove(value);
            }
        }

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

        private static string PackedBytesToNumString(byte[] bytes, int length, int decimalDigits)
        {
            // ToString returns 4F-FF-07...  strip out the dashes. 
            string hex = BitConverter.ToString(bytes).Replace("-", string.Empty);

            var hexChars = hex.ToAsciiCharArray().ToList();

            bool isNeg = hexChars.Last() == AsciiChar.PackedNegativeNybble;
            // get rid of sign nybble, but add '0' at top to keep even number of chars.
            hexChars.RemoveAt(hexChars.Count - 1);

            int decIdx = hexChars.Count - decimalDigits;
            hexChars.Insert(decIdx, AsciiChar.DecimalPoint);
            if (isNeg)
            {
                hexChars.Insert(0, AsciiChar.NegativeSign);
            }

            return hexChars.NewString();
        }




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

