using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Unity;
using System.ComponentModel;
using System.Text;
using System.Globalization;
using MDSY.Framework.Buffer.Services;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IFieldValueSerializer.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [InjectionImplementer(typeof(IFieldValueSerializer))]
    [Serializable]
    internal sealed class FieldValueSerializer : IFieldValueSerializer
    {
        #region consts
        private const byte boolByte_True = 0x31;
        private const byte boolByte_False = 0x30;
        private const byte byte_NegSign = 0x40;
        private const byte byte_EANegOffset = 0x19;     //Changed from 0x1A to 0x19 on 2021_09_16 to align with EBCDIC
        private const byte byte_EAPosOffset = 0x10;
        private const byte nullByte = 0x00;
        private const byte byte_Zero = 0x30;
        private const byte byte_NegativeZero_ZonedDec = 0x7D;
        private const byte byte_PositiveZero_ZonedDec = 0x7B;
        #endregion

        private static String DecimalSeparator
        {
            get { return CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator; }
        }

        private static Char DecimalSeparatorChar
        {
            get { return CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]; }
        }

        /// <summary>
        /// Gets the negative sign
        /// </summary>
        public static string NegativeSign
        {
            get { return CultureInfo.CurrentCulture.NumberFormat.NegativeSign; }
        }


        #region Serialize


        private static byte[] Serialize(Boolean value, int byteCount)
        {
            byte[] result = new byte[byteCount];
            byte boolByte = value ? boolByte_True : boolByte_False;
            result[byteCount - 1] = boolByte;
            return result;
        }

        private static string GetDecimalFormatString(int byteCount, int decimalDigits)
        {
            var leadingDigits = byteCount - decimalDigits;
            if (leadingDigits <= 0)
            {
                char[] result = new char[decimalDigits + 2];
                result[0] = '0';
                result[1] = '.';
                for (int i = 2; i < result.Length; i++)
                {
                    result[i] = '0';
                }
                return new string(result);
            }
            else
            {
                char[] result = new char[byteCount + 1];
                for (int i = 0; i < leadingDigits; i++)
                {
                    result[i] = '0';
                }
                result[leadingDigits] = '.';
                for (int i = leadingDigits + 1; i <= byteCount; i++)
                {
                    result[i] = '0';
                }
                return new string(result);
            }
        }

        private static byte[] Serialize(Decimal value, int byteCount, int decimalDigits, bool isSigned = false)
        {
            string fmtString = GetDecimalFormatString(byteCount, decimalDigits);
            decimal precision = (decimal)Math.Pow(10, decimalDigits);
            string decString = (Math.Truncate(precision * value) / precision).ToString(fmtString);
            decString = decString.Replace(".", string.Empty).Replace(",", string.Empty).Replace("-", string.Empty);
            if (decString.Length > byteCount)
            {
                // trim from the left. 
                decString = decString.Substring(decString.Length - byteCount, byteCount);
            }
            else if (decString.Length < byteCount)
            {
                decString = decString.PadLeft(byteCount, '0');
            }

            byte[] result = decString.Select(c => (byte)c).ToArray();

            // Signed zoned decimal
            if (isSigned)
            {
                if (value < 0)
                {
                    result = NegifyZonedDecimalBytes(result);
                }
                else 
                {
                    result = PosifyZonedDecimalBytes(result);
                }
            }

                return result;
        }

        private static byte[] Serialize(PackedDecimal packed, int byteCount)
        {
            byte[] result = new byte[byteCount];
            byte[] packedBytes = packed.Bytes;

            if (packedBytes.Count() < byteCount)
            {
                int padSize = byteCount - packedBytes.Count();
                System.Buffer.BlockCopy(packedBytes, 0, result, padSize, packedBytes.Count());
                // result = Enumerable.Repeat<byte>(nullByte, padSize).Concat(packedBytes).ToArray();
            }
            //Following code added to make sure packed Decimal bytes are truncated on the left - issue 5548
            else if (packedBytes.Count() > byteCount)
            {
                int skipByte = packedBytes.Count() - byteCount;
                System.Buffer.BlockCopy(packedBytes, skipByte, result, 0, byteCount);
                // result = packedBytes.Skip(skipByte).ToArray();
            }
            else
            {
                result = packedBytes;
            }
            return result;
        }

        private static byte[] Serialize(string value, int byteCount)
        {
            byte[] result = new byte[byteCount];
            for (int i = 0; i < value.Length; i++)
            {
                result[i] = (byte)value[i];
            }
            for (int i = value.Length; i < byteCount; i++)
            {
                result[i] = (byte)' ';
            }

            return result;
        }

        private static byte[] Serialize(Int64 value, int byteCount)
        {
            byte[] result = value.ToString().Select(c => (byte)c).ToArray();
            if (value < 0)
            {
                result = NegifyZonedDecimalBytes(value.ToString().Select(c => (byte)c).ToArray());
            }
            else
            {
                result = PosifyZonedDecimalBytes(value.ToString().Select(c => (byte)c).ToArray());
            }

            if (result.Length > byteCount)
            {
                result = result.Take(byteCount).ToArray();
            }
            else if (result.Length < byteCount)
            {
                result = Enumerable.Repeat<byte>(0x30, byteCount - result.Length).Concat(result).ToArray();
            }

            return result;
        }

        /// <summary>
        /// Returns a byte array representing the given int value as a COMP value.
        /// </summary>
        private static byte[] GetCompIntBytes(Int64 value, int byteCount)
        {
            var result = BitConverter.GetBytes(value);

            return result;
        }

        private static void PadTrimNumericBytes(int byteCount, ref byte[] bytes)
        {
            var resultBytes = new byte[byteCount];
            var offset = bytes.Length - byteCount;
            if (offset < 0)
            {
                offset = 0;
            }
            System.Buffer.BlockCopy(bytes, offset, resultBytes, 0, byteCount);
            bytes = resultBytes;

        }

        private static byte[] SerializeCompInt(Int64 value, int byteCount)
        {
            byte[] result = GetCompIntBytes(value, byteCount);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(result);
            }

            PadTrimNumericBytes(byteCount, ref result);
            return result;
        }

        private static byte[] SerializeUInt(UInt64 value, int byteCount)
        {
            var numChars = value.ToString().ToAsciiCharArray();
            if (numChars.Length > byteCount)
            {
                numChars = numChars.Skip(numChars.Length - byteCount).ToArray();
            }
            else if (numChars.Length < byteCount)
            {
                numChars = Enumerable.Repeat(AsciiChar.From('0'), byteCount - numChars.Length).Concat(numChars).ToArray();
            }

            return numChars.ToByteArray();
        }
        #endregion

        #region Deserialize


        private static bool DeserializeToBool(byte[] bytes)
        {
            bool result = default(bool);
            try
            {
                result = BytesToBool(bytes);
            }
            catch (Exception ex)
            {
                throw new FieldValueException("Unable to convert the given byte array to a bool value.", ex);
            }

            return result;
        }

        private static Decimal DeserializeToDecimal(byte[] bytes, int decimalDigits, bool isCompInt)
        {
            string s = null;
            if (isCompInt)
            {
                int i = DeserializeToInt(bytes, isCompInt);
                if (i < 0)
                {
                    s = Math.Abs(i).ToString();
                    if (bytes.Length > 0)
                    {
                        s = "-" + s.PadLeft(bytes.Length - 1, '0');
                    }
                }
                else
                    s = i.ToString().PadLeft(bytes.Length, '0');
            }
            else
            {
                long l = DeserializeToLong(bytes, isCompInt);
                if (l < 0)
                {
                    s = Math.Abs(l).ToString();
                    if (bytes.Length > 0)
                    {
                        s = "-" + s.PadLeft(bytes.Length - 1, '0');
                    }
                }
                else
                    s = l.ToString().PadLeft(bytes.Length, '0');
            }

            return Decimal.Parse(BuildDecimalString(Encoding.ASCII.GetBytes(s), decimalDigits));
        }


        private static Double DeserializeToDouble(byte[] bytes, int decimalDigits)
        {
            return Double.Parse(BuildDecimalString(bytes, decimalDigits));
        }

        private static Int32 DeserializeToInt(byte[] bytes, bool isCompInt)
        {
            // ints are stored as their string representation. 
            Int32 result = default(Int32);

            try    //rap 6/5/2014  changed to separate process for int16(CompShort), int 32 (CompINt) and fail for int 64 (CompLong)
            {
                if (isCompInt)
                {
                    if (bytes.Length == 2)
                    {
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(bytes);
                        result = BitConverter.ToInt16(bytes, 0);
                    }

                    if (bytes.Length == 4)
                    {
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(bytes);
                        result = BitConverter.ToInt32(bytes, 0);
                    }

                    if (bytes.Length == 8)
                        throw new InvalidCastException(string.Format("Buffer bytes wrong length for COMP INT. Need 4, was {0}.", bytes.Length));

                }
                else
                {
                    string temp = DeserializeToNumericString(bytes);
                    if (temp.Trim() == string.Empty)
                        result = 0;
                    else
                        result = Int32.Parse(temp);
                }
            }
            catch (Exception ex)
            {
                throw new FieldValueException("Unable to convert the given byte array to an Int32.", ex);
            }

            return result;
        }

        private static Int64 DeserializeToLong(byte[] bytes, bool isCompInt)
        {
            // ints are stored as their string representation. 
            Int64 result = default(Int64);

            try
            {
                if (isCompInt)
                {
                    if (bytes.Length > 8)
                    {
                        throw new InvalidCastException(string.Format("Buffer bytes wrong length for COMP INT. Need 8, was {0}.", bytes.Length));
                    }
                    else if (bytes.Length < 8)
                    {
                        byte[] newBytes = new byte[8];
                        newBytes.Initialize();
                        bytes.CopyTo(newBytes, 8 - bytes.Length);
                        bytes = newBytes;
                    }


                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes);
                    }

                    result = BitConverter.ToInt64(bytes, 0);
                }
                else
                {
                    string temp = DeserializeToNumericString(bytes);
                    if (temp.Trim() == string.Empty)
                        result = 0;
                    else
                        result = Int64.Parse(temp);
                }
            }
            catch (Exception ex)
            {
                throw new FieldValueException("Unable to convert the given byte array to an Int64.", ex);
            }

            return result;
        }

        private static PackedDecimal DeserializeToPackedDecimal(byte[] bytes, int decimalDigits, bool isCompInt, bool isSigned)
        {
            PackedDecimal result = new PackedDecimal(isSigned);
            try
            {
                if (isCompInt)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    result = PackedDecimal.FromBytes(bytes, isSigned, decimalDigits);
                }
            }
            catch (Exception ex)
            {
                throw new FieldValueException("Unable to convert the given byte array to a PackedDecimal.", ex);
            }

            return result;
        }

        private static Int16 DeserializeToShort(byte[] bytes, bool isCompInt)
        {
            // ints are stored as their string representation. 
            Int16 result = default(Int16);

            try
            {
                if (isCompInt)
                {
                    if (bytes.Length != 2)
                        throw new InvalidCastException(string.Format("Buffer bytes wrong length for COMP SHORT. Need 2, was {0}.", bytes.Length));

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes);
                    }

                    result = BitConverter.ToInt16(bytes, 0);
                }
                else
                {
                    string temp = DeserializeToNumericString(bytes);
                    result = Int16.Parse(temp);
                }
            }
            catch (Exception ex)
            {
                throw new FieldValueException("Unable to convert the given byte array to an Int16.", ex);
            }

            return result;
        }

        private static Single DeserializeToSingle(byte[] bytes, int decimalDigits, bool isCompInt)
        {
            return Single.Parse(BuildDecimalString(bytes, decimalDigits));
        }

        private static string DeserializeToNumericString(byte[] bytes)
        {
            string result = string.Empty;
            bool isNeg = false;
            var numBytes = DenegifyZonedDecimalBytes(bytes, ref isNeg);

            try
            {
                result = Encoding.ASCII.GetString(numBytes);
                //result = new string(numBytes.Select(b => (char) b).ToArray());                
                //result = numBytes.Select(b => (AsciiChar)b).NewString();
            }
            catch (Exception ex)
            {
                throw new FieldValueException("Unable to convert the given byte array to a string.", ex);
            }

            if (isNeg)
            {
                result = result.Insert(0, "-");
            }
            return result;
        }

        private static string DeserializeToString(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
            //return bytes.Select(b => (AsciiChar)b).NewString();
        }

        private static UInt32 DeserializeToUInt(byte[] bytes, bool isCompInt)
        {
            // ints are apparently stored as their string representation. 
            UInt32 result = default(UInt32);

            try
            {
                string temp = DeserializeToNumericString(bytes);
                if (temp.Trim() == string.Empty)
                    temp = "0";
                if (!UInt32.TryParse(temp, out result))
                {
                    //On invalid values, Do not throw exception? Move 0 instead? -Issue 5974 
                    result = 0;
                }
            }
            catch (Exception ex)
            {
                throw new FieldValueException("Unable to convert the given byte array to an UInt32.", ex);
            }

            return result;
        }

        private static UInt64 DeserializeToULong(byte[] bytes, bool isCompInt)
        {
            // ints are apparently stored as their string representation. 
            UInt64 result = default(UInt64);

            try
            {
                string temp = DeserializeToNumericString(bytes);
                if (temp.Trim() == string.Empty)
                    temp = "0";
                if (!UInt64.TryParse(temp, out result))
                {
                    //On invalid values, Do not throw exception? Move 0 instead? -Issue 5974 
                    result = 0;
                }
            }
            catch (Exception ex)
            {
                throw new FieldValueException("Unable to convert the given byte array to an UInt64.", ex);
            }

            return result;
        }

        private static UInt16 DeserializeToUShort(byte[] bytes, bool isCompInt)
        {
            // ints are apparently stored as their string representation. 
            UInt16 result = default(UInt16);

            try
            {
                string temp = DeserializeToNumericString(bytes);
                if (temp.Trim() == string.Empty)
                    temp = "0";
                if (!UInt16.TryParse(temp, out result))
                {
                    //On invalid values, Do not throw exception? Move 0 instead? -Issue 5974 
                    result = 0;
                }
            }
            catch (Exception ex)
            {
                throw new FieldValueException("Unable to convert the given byte array to an UInt16.", ex);
            }

            return result;
        }
        #endregion

        #region private methods


        private static bool BytesToBool(byte[] bytes)
        {
            // if last byte is a 0x31 and all other bytes null, it's true. Otherwise, false.
            byte[] comparer = new byte[bytes.Length]; ;
            //TODO: Check for strings that equate to TRUE/FALSE.
            comparer[comparer.Length - 1] = boolByte_True;
            return comparer.SequenceEqual(bytes);
        }

        private static byte[] DenegifyZonedDecimalBytes(byte[] bytes, ref bool isNeg)
        {
            List<byte> result = bytes.ToList();

            byte lastByte = result.Last();
            if (lastByte >= 0x70 && lastByte <= 0x79)
            {
                isNeg = true;
                result[result.Count - 1] = (byte)(lastByte - byte_NegSign);
            }
            //Handle possible EBCIDIC CHar conversion - issue 8075
            else if (lastByte >= 0x40 && lastByte <= 0x49)
            {
                isNeg = false;
                result[result.Count - 1] = (byte)(lastByte - byte_EAPosOffset);
            }
            //Look for EBCDIC negative zoned decimal character 
            else if (lastByte >= 0x4A && lastByte <= 0x53)
            {
                isNeg = true;
                result[result.Count - 1] = (byte)(lastByte - byte_EANegOffset);
            }
            else if (lastByte == byte_PositiveZero_ZonedDec)
            {
                isNeg = false;
                result[result.Count - 1] = 0x30; // zero
            }
            else if (lastByte == byte_NegativeZero_ZonedDec)
            {
                isNeg = true;
                result[result.Count - 1] = 0x30; // zero
            }

            return result.ToArray();
        }

        private static string BuildDecimalString(byte[] bytes, int decimalDigits)
        {
            bool isNeg = false;
            var temp = DenegifyZonedDecimalBytes(bytes, ref isNeg);
            var numChars = temp.Select(b => (AsciiChar)b).ToList();
            for (int i = 0; i < numChars.Count; i++)
            {
                if (numChars[i] == AsciiChar.DecimalSeparator)
                    numChars.RemoveAt(i);
            }

            while (numChars[numChars.Count - 1] == ' ')
            {
                numChars.RemoveAt(numChars.Count - 1);
                numChars.Insert(0, (AsciiChar)'0');
            }

            if (decimalDigits > 0)
            {
                numChars.Insert(numChars.Count - decimalDigits, AsciiChar.DecimalSeparator);
            }
            if (isNeg)
            {
                numChars.Insert(0, AsciiChar.CulturalNegativeSign);
            }

            return numChars.NewString();
        }

        private static bool IsCompInt(FieldType fieldType)
        {
            return (fieldType == FieldType.CompInt || fieldType == FieldType.CompLong || fieldType == FieldType.CompShort || fieldType == FieldType.ReferencePointer);
        }

        private static bool IsNumericType<T>(T value)
        {
            if (value == null)
                throw new ArgumentNullException("value", "value is null");

            return ((value is Int16) ||
                    (value is Int32) ||
                    (value is Int64) ||
                    (value is UInt16) ||
                    (value is UInt32) ||
                    (value is UInt64) ||
                    (value is Single) ||
                    (value is Double) ||
                    (value is Decimal) ||
                    (value is PackedDecimal));
        }

        /// <summary>
        /// Adjusts the final byte of the numeric byte array to properly signify a negative value. 
        /// </summary>
        /// <param name="bytes"></param>
        private static byte[] NegifyZonedDecimalBytes(byte[] bytes)
        {
            List<byte> result = bytes.ToList();
            byte signByte = result.Last();

            // 'negative' zeroes are set to 0x7D; for 1-9 just add 0x40. Insane, I know. 
            if (signByte == 0x30)
            {
                result[bytes.Length - 1] = byte_NegativeZero_ZonedDec;
            }
            else
            {
                result[bytes.Length - 1] = (byte)(signByte + byte_EANegOffset);
            }

            if (result.First() == 0x2D) // neg sign
            {
                result.RemoveAt(0);
            }

            return result.ToArray();
        }

        private static byte[] PosifyZonedDecimalBytes(byte[] bytes)
        {
            List<byte> result = bytes.ToList();
            byte signByte = result.Last();

            // 'negative' zeroes are set to 0x7D; for 1-9 just add 0x40. Insane, I know. 
            if (signByte == 0x30)
            {
                result[bytes.Length - 1] = byte_PositiveZero_ZonedDec;
            }
            else
            {
                result[bytes.Length - 1] = (byte)(signByte + byte_EAPosOffset);
            }

            if (result.First() == 0x2B) // Pos sign
            {
                result.RemoveAt(0);
            }

            return result.ToArray();
        }

        private static byte[] ValueToBytesAsSingle<T>(T value, int byteCount)
        {
            Single tempFloat = 0;

            if (value is PackedDecimal)
            {
                PackedDecimal tempPac = (PackedDecimal)Convert.ChangeType(value, typeof(PackedDecimal));
                tempFloat = Convert.ToSingle(tempPac.Value);
            }
            else if (GetIsNumericValueType(value))
            {
                tempFloat = Convert.ToSingle(value);
            }
            else if (value is string)
            {
                if (!Single.TryParse(value as string, out tempFloat))
                    throw new InvalidCastException();
            }
            else
                throw new InvalidCastException();

            byte[] result = BitConverter.GetBytes(tempFloat);
            //if (result.Length < byteCount)
            //{
            //    result = Enumerable.Repeat<byte>(0x00, byteCount - result.Length).Concat(result).ToArray();
            //}
            //PadTrimNumericBytes(byteCount, ref result);
            //if (BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(result);
            //}

            return result;
        }

        private static byte[] ValueToBytesAsDouble<T>(T value, int byteCount)
        {
            Double tempDbl = 0;

            if (value is PackedDecimal)
            {
                PackedDecimal tempPac = (PackedDecimal)Convert.ChangeType(value, typeof(PackedDecimal));
                tempDbl = Convert.ToDouble(tempPac.Value);
            }
            else if (GetIsNumericValueType(value))
            {
                tempDbl = Convert.ToDouble(value);
            }
            else if (value is string)
            {
                if (!Double.TryParse(value as string, out tempDbl))
                    throw new InvalidCastException();
            }
            else
                throw new InvalidCastException();

            byte[] result = BitConverter.GetBytes(tempDbl);
            PadTrimNumericBytes(byteCount, ref result);
            //if (BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(result);
            //}

            return result;
        }

        private static byte[] ValueToBytesAsComp<T>(T value, int byteCount)
        {
            byte[] result;
            Int64 tempInt = 0;

            if (value is Int16 || value is Int32 || value is Int64 || value is UInt16 || value is UInt32 || value is UInt64)
            {
                tempInt = Convert.ToInt64(value);
            }
            else if (value is string)
            {
                if (!Int64.TryParse(value.ToString(), out tempInt))
                    throw new InvalidCastException();
            }
            else if (value is Single || value is Double || value is Decimal)
            {
                Decimal tempDec = Convert.ToDecimal(value);
                if (tempDec % 1 == 0)
                {
                    tempInt = Decimal.ToInt64(tempDec);
                }
                else
                {
                    tempInt = Decimal.ToInt64(decimal.Floor(tempDec));
                }
            }
            else if (value is PackedDecimal)
            {
                PackedDecimal tempPac = (PackedDecimal)Convert.ChangeType(value, typeof(PackedDecimal));
                if (tempPac.DecimalValue == 0)
                {
                    tempInt = tempPac.WholeValue;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            else
            {
                throw new InvalidCastException();
            }

            result = SerializeCompInt(tempInt, byteCount);
            return result;
        }

        private static byte[] ValueToBytesAsSignedDecimal<T>(T value, int byteCount, int decimalDigits)
        {
            byte[] result;
            Decimal tempDec = 0;

            if (value is Single || value is Double || value is Decimal || value is PackedDecimal ||
                value is Int16 || value is Int32 || value is Int64 || value is UInt16 || value is UInt32 || value is UInt64)
            {
                tempDec = Convert.ToDecimal(value);
            }
            else if (value is string)
            {
                // Update for COBOL edit Masks which must contain '.' to check on move to numeric field with different separator - SAAQ issue 5584
                if (decimalDigits > 0 && DecimalSeparatorChar != '.' && value.ToString().Contains('.'))
                {
                    if (!Decimal.TryParse(value.ToString().Replace('.', DecimalSeparatorChar), out tempDec))
                        throw new InvalidCastException();
                }
                else
                    if (!Decimal.TryParse(value.ToString(), out tempDec))
                {
                    //If value not numeric, serialize string - issue 5974
                    return Serialize(value.ToString(), byteCount);
                }

            }
            else
            {
                throw new InvalidCastException();
            }

            result = Serialize(tempDec, byteCount, decimalDigits, true);
            return result;
        }

        /// <summary>
        /// Attempts to convert the given <paramref name="value"/> to a boolean and then serialize the boolean. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        private static byte[] ValueToBytesAsBool<T>(T value, int byteCount)
        {
            // attempt to store value as bool
            bool tempBool = false;

            if (value is bool)
            {
                tempBool = Convert.ToBoolean(value);
            }
            else if (value is string)
            {
                string valStr = Convert.ToString(value);
                if (valStr.IsBooleanTrue())
                {
                    tempBool = true;
                }
                else if (valStr.IsBooleanFalse())
                {
                    tempBool = false;
                }
                else
                {
                    throw new InvalidCastException(string.Format("String value {0} can't be converted to a boolean value.", valStr));
                }
            }
            else if (IsNumericType(value))
            {
                tempBool = Convert.ToBoolean(value);
            }
            else
            {
                throw new InvalidCastException(string.Format("Cannot serialize value of type {0} as Boolean.", value.GetType()));
            }

            return Serialize(tempBool, byteCount);
        }

        private static byte[] ValueToBytesAsUnsignedPackedDecimal<T>(T value, int byteCount, int decimalDigits)
        {
            PackedDecimal tempPD = GetPackedDecimalForSerialization<T>(value, byteCount, decimalDigits);
            return Serialize(tempPD.AbsoluteValue(), byteCount);
        }


        private static string BuildPackedDecimalFormatString(int byteCount, int decimalDigits)
        {
            return GetDecimalFormatString((byteCount * 2) - 1, decimalDigits);
            //return string.Concat(
            //    string.Empty.PadLeft((byteCount * 2) - 1 - decimalDigits, '0'),
            //    DecimalSeparator,
            //    string.Empty.PadLeft(decimalDigits, '0')
            //    );
        }

        private static PackedDecimal GetPackedDecimalForSerialization<T>(T value, int byteCount, int decimalDigits)
        {
            PackedDecimal tempPD;
            if (value is PackedDecimal)
            {
                tempPD = (PackedDecimal)Convert.ChangeType(value, typeof(PackedDecimal));
            }
            else if (value is string)
            {
                string strValue = Convert.ToString(value);
                if (decimalDigits > 0 && !strValue.Contains(DecimalSeparator[0]) && strValue != "0")
                {
                    strValue = strValue.Insert(strValue.Length - decimalDigits, DecimalSeparator);
                }

                try
                {
                    //string outFormat = string.Concat(string.Empty.PadLeft((byteCount * 2) - 1 - decimalDigits, '0'), ".", string.Empty.PadLeft(decimalDigits, '0'));
                    string outFormat = BuildPackedDecimalFormatString(byteCount, decimalDigits);
                    decimal tempDec = Convert.ToDecimal(strValue);
                    string decString = tempDec.ToString(outFormat);
                    if (decString[0] != '-' && decString[0] != '+')
                        decString = "+" + decString;
                    tempPD = PackedDecimal.Parse(decString);
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException(string.Format("Unable to convert string '{0}' to PackedDecimal.", value), ex);
                }
            }
            else if (IsNumericType(value))
            {
                string sValue = Convert.ToDecimal(value).ToString("G29", CultureInfo.InvariantCulture);
                int decimalsValue = sValue.IndexOf('.'); // reusing decimals variable
                decimalsValue = decimalsValue == -1 ? 0 : sValue.Length - decimalsValue - 1;

                if (decimalsValue > decimalDigits)
                {
                    //string outFormat = string.Concat(string.Empty.PadLeft((byteCount * 2) - 1 - decimalDigits, '0'), ".", string.Empty.PadLeft(decimalsValue, '0'));
                    string outFormat = BuildPackedDecimalFormatString(byteCount, decimalDigits + 1);

                    decimal tempDec = Convert.ToDecimal(value);
                    sValue = tempDec.ToString(outFormat);

                    // ToString() rounds resulted value, while it needs to be truncated
                    sValue = sValue.Substring(0, sValue.Length - 1);
                    if (sValue == "," || (sValue == "."))
                    {
                        sValue = "+0";
                    }
                    else if (sValue == "-," || sValue == "-.")
                    {
                        sValue = "-0";
                    }
                }
                else
                {
                    //string outFormat = string.Concat(string.Empty.PadLeft((byteCount * 2) - 1 - decimalDigits, '0'), ".", string.Empty.PadLeft(decimalDigits, '0'));
                    string outFormat = BuildPackedDecimalFormatString(byteCount, decimalDigits);
                    decimal tempDec = Convert.ToDecimal(value);
                    sValue = tempDec.ToString(outFormat);
                    if (!sValue.StartsWith("-") && !sValue.StartsWith("+"))
                        sValue = "+" + sValue;
                }

                tempPD = PackedDecimal.Parse(sValue);
            }
            else
            {
                throw new InvalidCastException();
            }
            return tempPD;
        }

        private static byte[] ValueToBytesAsPackedDecimal<T>(T value, int byteCount, int decimalDigits)
        {
            PackedDecimal tempPD = GetPackedDecimalForSerialization<T>(value, byteCount, decimalDigits);
            return Serialize(tempPD, byteCount);
        }

        private static byte[] ValueToBytesAsSignedNumeric<T>(T value, int byteCount)
        {
            byte[] result;
            Int64 tempInt = 0;

            if (value is Int16 || value is Int32 || value is Int64)
            {
                tempInt = Convert.ToInt64(value);
            }
            else if (value is UInt16 || value is UInt32 || value is UInt64 || value is Single || value is Double || value is Decimal || value is PackedDecimal)
            {
                try
                {
                    tempInt = Convert.ToInt64(Math.Floor(Convert.ToDecimal(value)));
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException("Unable to convert to Signed Numeric.", ex);
                }
            }
            else if (value is string)
            {
                if (!Int64.TryParse(value.ToString(), out tempInt))
                {
                    //If value not numeric, issue 5974
                    return Serialize(value.ToString(), byteCount);
                }
            }
            else
            {
                throw new InvalidCastException();
            }

            result = Serialize(tempInt, byteCount);
            return result;
        }

        //private static string LeftAdjustNumericValue(int byteCount, string value)
        //{
        //    string result = value;

        //    if (result.Length > byteCount)
        //    {
        //        result = result.Substring(result.Length - byteCount);
        //    }
        //    else
        //    {
        //        result = result.PadLeft(byteCount, '0');
        //    }

        //    return result;

        //}

        private static string RightAdjustStringValue(int byteCount, string value)
        {
            string result = value;

            if (result.Length > byteCount)
            {
                result = result.Substring(0, byteCount);
            }
            else
            {
                result = result.PadRight(byteCount, ' ');
            }

            return result;
        }

        private static bool GetIsNumericValueType<T>(T value)
        {
            return value is Single || value is Double || value is Decimal || value is PackedDecimal ||
                        value is Int16 || value is Int32 || value is Int64 ||
                        value is UInt16 || value is UInt32 || value is UInt64;
        }

        private static string ApplyCobolMoveRules(string numericString, int byteCount)
        {
            string result = numericString;

            // truncate
            if (result.Length > byteCount)
            {
                result = result.Substring(result.Length - byteCount);
            }

            // Drop any decimal point but retain the value
            result = result.Replace(DecimalSeparator, string.Empty);

            // Drop any negative sign
            result = result.Replace(NegativeSign, string.Empty);

            // left-justify the result
            result = result.PadRight(byteCount);

            return result;
        }

        private static string ApplyAdsoMoveRules<T>(T value, int byteCount)
        {
            string result = string.Empty;

            try
            {
                // round to integer value
                decimal decValue = Convert.ToDecimal(value);
                decValue = Math.Round(decValue);

                // Drop any decimal value
                result = decValue.ToString("F0");

                // Place any negative sign to the left of the result
                int idx = result.IndexOf(NegativeSign);
                if (idx > -1)
                {
                    result = result.Replace(NegativeSign, string.Empty);
                    result = result.Insert(0, NegativeSign);
                }

                // right-justify the result
                result = result.PadLeft(byteCount);
            }
            catch (Exception)
            {
                result = value.ToString();
            }

            return result;
        }

        /// <summary>
        /// Serializes the string representation of the given <paramref name="value"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        private static byte[] ValueToBytesAsString<T>(T value, int byteCount, FieldType sourceFieldType)
        {
            string tempString = value.ToString();

            if (value is FillWith)
            {
                // Added options for high values  - Issue 5270
                if (tempString == "HighValues")
                    tempString = new string(AsciiChar.MaxValue.AsChar, byteCount);
                else if (tempString == "LowValues")
                    tempString = new string(AsciiChar.MinValue.AsChar, byteCount);
                else if (tempString == "Spaces")
                    tempString = new string(' ', byteCount);
                else if (tempString == "Hashes")
                    tempString = new string('#', byteCount);
                else if (tempString == "Dashes")
                    tempString = new string('-', byteCount);
                else if (tempString == "Zeros")
                    tempString = new string('0', byteCount);
            }

            // Left pad/trim numbers, right pad/trim strings.
            if ((sourceFieldType != FieldType.String && sourceFieldType != FieldType.Boolean) || GetIsNumericValueType(value))
            {
                if (sourceFieldType == FieldType.FloatDouble || sourceFieldType == FieldType.NumericEdited) //Issue 8414 and Bugzilla 197
                {
                    tempString = RightAdjustStringValue(byteCount, tempString);
                }
                else
                {
                    // if we're moving a numeric to a string field, check for 
                    // ADSO vs. COBOL move rules:
                    switch (BufferServices.Directives.FieldValueMoves)
                    {
                        case FieldValueMoveType.Undefined:
                        case FieldValueMoveType.CobolMoves:
                            tempString = ApplyCobolMoveRules(tempString, byteCount);
                            break;
                        case FieldValueMoveType.AdsoMoves:
                            // ADSO move involves rounding the value; pass the numeric so we don't have to 
                            // convert back and forth...
                            tempString = ApplyAdsoMoveRules(value, byteCount);
                            break;
                    }
                }
            }
            else
            {
                tempString = RightAdjustStringValue(byteCount, tempString);
            }

            return Serialize(tempString, byteCount);
        }

        private static byte[] ValueToBytesAsUnsignedDecimal<T>(T value, int byteCount, int decimalDigits)
        {
            byte[] result;
            Decimal tempUDec = 0;

            if (GetIsNumericValueType(value))
            {
                tempUDec = Convert.ToDecimal(value);
            }
            else if (value is string)
            {
                // Update for COBOL edit Masks which must contain '.' to check on move to numeric field with different separator - SAAQ issue 5584
                if (decimalDigits > 0 && DecimalSeparatorChar != '.' && value.ToString().Contains('.'))
                {
                    if (!Decimal.TryParse(value.ToString().Replace('.', DecimalSeparatorChar), out tempUDec))
                        throw new InvalidCastException();
                }
                else
                    if (!Decimal.TryParse(value.ToString(), out tempUDec))
                {
                    //If value not numeric, serialize string - issue 5974
                    return Serialize(value.ToString(), byteCount);
                }
            }
            else
            {
                throw new InvalidCastException();
            }

            result = Serialize(Math.Abs(tempUDec), byteCount, decimalDigits, false);
            return result;
        }

        private static byte[] ValueToBytesAsUnsignedNumeric<T>(T value, int byteCount)
        {
            UInt64 tempUInt = 0;

            if (value is UInt16 || value is UInt32 || value is UInt64)
            {
                tempUInt = Convert.ToUInt64(value);
            }
            else if (value is Int16 || value is Int32 || value is Int64 || value is Single || value is Double || value is Decimal || value is PackedDecimal)
            {
                try
                {
                    tempUInt = Convert.ToUInt64(Math.Abs(BufferServices.Directives.FieldValueMoves == FieldValueMoveType.CobolMoves
                        ? Math.Floor(Convert.ToDecimal(value))
                        //else ADSO implmentation rounds
                        : Math.Round(Convert.ToDecimal(value),0)));
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException("Unable to convert to Unsigned Numeric", ex);
                }
            }
            else if (value is string)
            {
                string numValue = value.ToString();
                if (numValue.Trim() == string.Empty)
                    numValue = "0";
                if (!UInt64.TryParse(numValue, out tempUInt))
                {
                    //If value not numeric, serialize string - issue 5974
                    return Serialize(value.ToString(), byteCount);
                }
            }
            else
            {
                throw new InvalidCastException();
            }

            return SerializeUInt(tempUInt, byteCount);
        }
        #endregion

        #region public methods

        /// <summary>
        /// Returns the given <paramref name="bytes"/> deserialized into a value of the given 
        /// <typeparamref name="T"/> type. 
        /// </summary>
        /// <param name="bytes">Byte array value to be deserialized to value of type <typeparamref name="T"/>.</param>
        /// <param name="fieldType">The <see cref="MDSY.Framework.Buffer.Common.FieldType"/> of the field object 
        /// which stored the given <paramref name="bytes"/>. Indicates how the initial value was serialized.</param>
        /// <param name="decimalDigits">For non-integer numeric values, indicates the number of digits after the decimal. </param>
        /// <param name="isDestinationString">Specifies whether the destination is a string or not.</param>
        public T Deserialize<T>(byte[] bytes, FieldType fieldType, int decimalDigits, bool isDestinationString = false)
        {
            if (bytes == null || bytes.Length == 0)
                throw new ArgumentException("bytes is null or empty.", "bytes");

            Object result = null;
            bool isCompInt = IsCompInt(fieldType);

            if (fieldType == FieldType.UnsignedNumeric || fieldType == FieldType.UnsignedDecimal
                || fieldType == FieldType.SignedNumeric || fieldType == FieldType.SignedDecimal
                || fieldType == FieldType.PackedDecimal || fieldType == FieldType.UnsignedPackedDecimal)
            {
                bool nullValue = false;
                int j = 0;
                for (int i = 0; i < bytes.Length; i++)
                {
                    if ((Char)bytes[i] == 0)
                        j++;
                }

                if (j == bytes.Length)
                    nullValue = true;

                if (nullValue)
                {
                    if (fieldType == FieldType.PackedDecimal || fieldType == FieldType.UnsignedPackedDecimal)
                    {
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            if (i == bytes.Length - 1)
                                bytes[i] = 0x0F;
                            else
                                bytes[i] = Convert.ToByte('0');
                        }
                    }
                    else
                    {
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            bytes[i] = Convert.ToByte('0');
                        }
                    }
                }
            }

            // string
            if (typeof(String) == typeof(T) || fieldType == FieldType.String || fieldType == FieldType.NumericEdited || fieldType == FieldType.UnsignedNumeric || fieldType == FieldType.SignedNumeric)
            {
                if (isCompInt)
                {
                    if (fieldType == FieldType.CompShort)
                    {
                        result = DeserializeToShort(bytes, isCompInt).ToString();
                    }
                    else if (fieldType == FieldType.CompInt || fieldType == FieldType.ReferencePointer)
                    {
                        result = DeserializeToInt(bytes, isCompInt).ToString();
                    }
                    else if (fieldType == FieldType.CompLong)
                    {
                        result = DeserializeToLong(bytes, isCompInt).ToString();
                    }
                }
                //Update to handle Signeddecimal types - SAAQ issue 5640
                else if (fieldType == FieldType.SignedDecimal || fieldType == FieldType.SignedNumeric)
                {
                    if (decimalDigits > 0)
                        result = DeserializeToDecimal(bytes, decimalDigits, isCompInt);
                    else
                        result = DeserializeToNumericString(bytes);
                }

                else if (fieldType == FieldType.UnsignedNumeric)
                {
                    if (decimalDigits > 0)
                    {
                        result = DeserializeToDecimal(bytes, decimalDigits, isCompInt);
                    }
                    else
                    {
                        string tmp = Encoding.UTF8.GetString(bytes);

                        if (tmp.Trim() == "")
                        {
                            tmp = "0";
                            bytes[bytes.Length - 1] = 48;
                        }
                        decimal outDec;
                        if (decimal.TryParse(tmp, out outDec))
                            result = DeserializeToNumericString(bytes);
                        else
                            result = DeserializeToString(bytes);
                    }
                }

                else if (fieldType == FieldType.FloatSingle)
                {
                    result = BitConverter.ToSingle(bytes, 0).ToString();
                }
                else if (fieldType == FieldType.FloatDouble)
                {
                    result = BitConverter.ToDouble(bytes, 0).ToString();
                }
                else
                {
                    result = DeserializeToString(bytes);
                }

            }

            // PackedDecimal
            else if (typeof(PackedDecimal) == typeof(T) || fieldType == FieldType.PackedDecimal || fieldType == FieldType.UnsignedPackedDecimal)
            {
                result = DeserializeToPackedDecimal(bytes, decimalDigits, isCompInt, fieldType == FieldType.PackedDecimal);
            }

            // Boolean
            else if (typeof(Boolean) == (typeof(T)) || fieldType == FieldType.Boolean)
            {
                result = DeserializeToBool(bytes);
            }

            // ints
            else if (typeof(Int32) == typeof(T) || fieldType == FieldType.CompInt || fieldType == FieldType.ReferencePointer)
            {
                if (decimalDigits > 0)
                    result = DeserializeToDecimal(bytes, decimalDigits, isCompInt);
                else
                    result = DeserializeToInt(bytes, isCompInt);
            }
            else if (typeof(Int16) == (typeof(T)) || fieldType == FieldType.CompShort)
            {
                result = DeserializeToShort(bytes, isCompInt);
            }
            else if (typeof(Int64) == (typeof(T)) || fieldType == FieldType.CompLong || fieldType == FieldType.SignedNumeric)
            {

                //Update for issue 5753
                if (decimalDigits > 0 && fieldType != FieldType.CompLong)
                    result = DeserializeToDecimal(bytes, decimalDigits, isCompInt);
                else
                    result = DeserializeToLong(bytes, isCompInt);
            }

            // unsigned ints
            else if (typeof(UInt32) == (typeof(T)))
            {
                if (decimalDigits > 0)
                    result = DeserializeToDecimal(bytes, decimalDigits, isCompInt);
                else
                    result = DeserializeToUInt(bytes, isCompInt);
            }
            else if (typeof(UInt16) == (typeof(T)))
            {
                result = DeserializeToUShort(bytes, isCompInt);
            }
            else if (typeof(UInt64) == (typeof(T)) || fieldType == FieldType.UnsignedNumeric)
            {
                if (decimalDigits > 0)
                    result = DeserializeToDecimal(bytes, decimalDigits, isCompInt);
                else
                {
                    if (isDestinationString)
                        result = DeserializeToString(bytes); // integer value requires leading zero(s) if destination is string - ticket 5675
                    else
                        result = DeserializeToULong(bytes, isCompInt);
                }
            }

            // decimals
            else if (typeof(Single) == (typeof(T)) || fieldType == FieldType.FloatSingle)
            {
                if (fieldType == FieldType.PackedDecimal | fieldType == FieldType.UnsignedPackedDecimal)
                {
                    PackedDecimal dec = PackedDecimal.FromBytes(bytes, fieldType == FieldType.PackedDecimal, decimalDigits);
                    result = dec.ToSingle(null);
                }
                else if (fieldType == FieldType.FloatSingle | fieldType == FieldType.FloatDouble)
                {
                    //if (BitConverter.IsLittleEndian)
                    //{
                    //    Array.Reverse(bytes);
                    //}

                    result = BitConverter.ToSingle(bytes, 0);
                }
                else
                {
                    result = DeserializeToSingle(bytes, decimalDigits, isCompInt);
                }
            }
            else if (typeof(Double) == (typeof(T)) || fieldType == FieldType.FloatDouble) //Issue 8414
            {
                if (fieldType == FieldType.PackedDecimal | fieldType == FieldType.UnsignedPackedDecimal)
                {
                    PackedDecimal dec = PackedDecimal.FromBytes(bytes, fieldType == FieldType.PackedDecimal, decimalDigits);
                    result = dec.ToDouble(null);
                }
                else if (fieldType == FieldType.FloatSingle | fieldType == FieldType.FloatDouble)
                {
                    //if (BitConverter.IsLittleEndian)
                    //{
                    //    Array.Reverse(bytes);
                    //}

                    result = BitConverter.ToDouble(bytes, 0);
                }
                else
                {
                    result = DeserializeToDouble(bytes, decimalDigits);
                }

            }
            else if (typeof(Decimal) == (typeof(T)) || fieldType == FieldType.SignedDecimal)
            {
                if (fieldType == FieldType.PackedDecimal | fieldType == FieldType.UnsignedPackedDecimal)
                {
                    PackedDecimal dec = PackedDecimal.FromBytes(bytes, fieldType == FieldType.PackedDecimal, decimalDigits);
                    result = dec.ToDecimal(null);
                }
                else if (fieldType == FieldType.FloatSingle)
                {
                    //if (BitConverter.IsLittleEndian)
                    //{
                    //    Array.Reverse(bytes);
                    //}

                    Single temp = BitConverter.ToSingle(bytes, 0);
                    result = Convert.ToDecimal(temp);
                }
                else if (fieldType == FieldType.FloatDouble)
                {
                    //if (BitConverter.IsLittleEndian)
                    //{
                    //    Array.Reverse(bytes);
                    //}

                    Double temp = BitConverter.ToDouble(bytes, 0);
                    result = Convert.ToDecimal(temp);
                }
                else
                {
                    result = DeserializeToDecimal(bytes, decimalDigits, isCompInt);
                }
            }
            if (result is string && (typeof(T) == typeof(decimal) || typeof(T) == typeof(int)))
            {
                string testString = (string)result;
                if (testString.Trim() == string.Empty)
                {
                    result = "0";
                }
                else
                    result = testString.Trim().Replace(",","");
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }


        /// <summary>
        /// Returns the byte serialization of the given <paramref name="value"/> as appropriate 
        /// to the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to be serialized.</typeparam>
        /// <param name="value">The value to be serialized.</param>
        /// <param name="byteCount">Length of byte array to be returned.</param>
        /// <param name="fieldType">The <see cref="MDSY.Framework.Buffer.Common.FieldType"/> of the field object 
        /// which stored the given <paramref name="bytes"/>. Indicates how the initial value was serialized.</param>
        /// <param name="decimalDigits">For non-integer numeric values, indicates the number of digits after the decimal.</param>
        /// <returns>The serialized value.</returns>
        public byte[] Serialize<T>(T value, int byteCount, FieldType fieldType, FieldType sourceFieldType, int decimalDigits)
        {
            if (value == null) return new byte[byteCount];
            switch (fieldType)
            {
                case FieldType.String:
                case FieldType.NumericEdited:
                    return ValueToBytesAsString(value, byteCount, sourceFieldType);
                //for (int i = 0; i < result.Length; i++)
                //{
                //    if ((Char)result[i] == 0)
                //        result[i] = Convert.ToByte(' ');
                //}


                case FieldType.Boolean:
                    return ValueToBytesAsBool(value, byteCount);


                case FieldType.UnsignedPackedDecimal:
                    return ValueToBytesAsUnsignedPackedDecimal(value, byteCount, decimalDigits);


                case FieldType.PackedDecimal:
                    return ValueToBytesAsPackedDecimal(value, byteCount, decimalDigits);


                case FieldType.SignedNumeric:
                    if (decimalDigits > 0)
                        return ValueToBytesAsSignedDecimal(value, byteCount, decimalDigits);
                    else
                        return ValueToBytesAsSignedNumeric(value, byteCount);


                case FieldType.UnsignedNumeric:
                    if (decimalDigits > 0)
                        return ValueToBytesAsUnsignedDecimal(value, byteCount, decimalDigits);
                    else
                        return ValueToBytesAsUnsignedNumeric(value, byteCount);


                case FieldType.SignedDecimal:
                    return ValueToBytesAsSignedDecimal(value, byteCount, decimalDigits);


                case FieldType.UnsignedDecimal:
                    return ValueToBytesAsUnsignedDecimal(value, byteCount, decimalDigits);


                case FieldType.CompShort:
                case FieldType.CompInt:
                case FieldType.CompLong:
                case FieldType.ReferencePointer:
                    return ValueToBytesAsComp(value, byteCount);


                case FieldType.FloatSingle:
                    return ValueToBytesAsSingle(value, byteCount);

                case FieldType.FloatDouble:
                    return ValueToBytesAsDouble(value, byteCount);

                case FieldType.Binary:
                default:
                    return new byte[byteCount];
            }
        }

        /// <summary>
        /// Attempts to deserialize the given <paramref name="bytes"/> into the <paramref name="value"/> parameter.
        /// </summary>
        /// <typeparam name="T">The type the bytes will deserialized into.</typeparam>
        /// <param name="bytes">The byte array containing the value to be deserialized.</param>
        /// <param name="fieldType">The <see cref="MDSY.Framework.Buffer.Common.FieldType"/> of the field object 
        /// which stored the given <paramref name="bytes"/>. Indicates how the initial value was serialized.</param>
        /// <param name="value">The parameter to receive the deserialized value.</param>
        /// <returns><c>True</c> if the deserialization was successful, <c>false</c> otherwise.</returns>
        /// <param name="decimalDigits">For non-integer numeric values, indicates the number of digits after the decimal.</param>
        /// <param name="isDestinationString">Specifies whether the destination is a string or not.</param>
        public bool TryDeserialize<T>(byte[] bytes, FieldType fieldType, int decimalDigits, out T value, bool isDestinationString = false)
        {
            bool result;
            value = default(T);

            try
            {
                value = Deserialize<T>(bytes, fieldType, decimalDigits, isDestinationString);
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        #endregion
    }
}

