using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Represents a character as an old-style single-byte value. Use <c>AsciiChar</c> in place of <c>char</c> when 
    /// converting between chars and bytes to avoid UTF-16 resultant overruns.
    /// </summary>
    [Serializable]
    public struct AsciiChar : IComparable, IConvertible, IComparable<char>, IEquatable<char>,
        IComparable<AsciiChar>, IEquatable<AsciiChar>
    {
        #region constants
        private const byte byte_decimalPoint = 0x2E;
        private const byte byte_comma = 0x2C;
        private const byte byte_HexCChar = 0x43;
        private const byte byte_HexDChar = 0x44;
        private const byte byte_NegativeSign = 0x2D;
        private const byte byte_Db2ConnectHighValue = 0x9F;

        private static byte[] trueBytes { get { return new byte[2] { 0x54, 0x74 }; } }
        private static byte[] falseBytes { get { return new byte[2] { 0x46, 0x66 }; } }
        #endregion

        #region private fields
        private byte value;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the AsciiChar structure initialized to the given byte value.
        /// </summary>
        public AsciiChar(byte value)
        {
            this.value = value;
        }

        /// <summary>
        /// Initializes a new instance of the AsciiChar structure initialized to the given char value, if possible.
        /// </summary>
        public AsciiChar(char value)
        {
            try
            {
                byte val = (byte)value;
                this.value = val;
            }
            catch (Exception ex)
            {
                throw new AsciiCharException(ex, "Unable to convert given value {0} to AsciiChar.", value);
            }
        }
        #endregion

        #region instatiators
        /// <summary>
        /// Returns a new AsciiChar from the given char value.
        /// </summary>
        public static AsciiChar From(char value)
        {
            return new AsciiChar(value);
        }

        /// <summary>
        /// Returns a new AsciiChar from the given byte value.
        /// </summary>
        public static AsciiChar From(byte value)
        {
            return new AsciiChar(value);
        }
        #endregion

        #region operator overloads
        /// <summary>
        /// Implicitly converts an instance of type AsciiChar to a new instance of type byte.
        /// </summary>
        /// <param name="obj">An instance of type AsciiChar to convert.</param>
        /// <returns>Returns a new instance of type byte, derived from the specified AsciiChar instance.</returns>
        public static implicit operator byte(AsciiChar obj)
        {
            return obj.AsByte;
        }

        /// <summary>
        /// Implicitly converts an instance of type byte to a new instance of type AsciiChar.
        /// </summary>
        /// <param name="byteValue">An instance of type byte to convert.</param>
        /// <returns>Returns a new instance of type AsciiChar, derived from the specified byteValue instance.</returns>
        public static implicit operator AsciiChar(byte byteValue)
        {
            return new AsciiChar(byteValue);
        }

        /// <summary>
        /// Explicitly converts an instance of type AsciiChar to a new instance of type char.
        /// </summary>
        /// <param name="obj">An instance of type AsciiChar to convert.</param>
        /// <returns>Returns a new instance of type char, derived from the specified AsciiChar instance.</returns>
        public static explicit operator char(AsciiChar obj)
        {
            return obj.AsChar;
        }

        /// <summary>
        /// Explicitly converts an instance of type char to a new instance of type AsciiChar.
        /// </summary>
        /// <param name="charValue">An instance of type char to convert.</param>
        /// <returns>Returns a new instance of type AsciiChar, derived from the specified charValue instance.</returns>
        public static explicit operator AsciiChar(char charValue)
        {
            AsciiChar result;

            if (charValue > byte.MaxValue)
            {
                result = MaxValue;
            }
            else
            {
                result = From(charValue);
            }
            return result;
        }

        /// <summary>
        /// Equality operator overload.
        /// </summary>
        public static bool operator ==(AsciiChar left, AsciiChar right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator overload.
        /// </summary>
        public static bool operator !=(AsciiChar left, AsciiChar right)
        {
            return !(left.Equals(right));
        }

        /// <summary>
        /// Greater than operator overload.
        /// </summary>
        public static bool operator >(AsciiChar left, AsciiChar right)
        {
            return left.AsByte > right.AsByte;
        }

        /// <summary>
        /// Less than operator overload.
        /// </summary>
        public static bool operator <(AsciiChar left, AsciiChar right)
        {
            return left.AsByte < right.AsByte;
        }

        /// <summary>
        /// Greater than or equal to operator overload.
        /// </summary>
        public static bool operator >=(AsciiChar left, AsciiChar right)
        {
            return left.AsByte >= right.AsByte;
        }

        /// <summary>
        /// Less than or equal to operator overload.
        /// </summary>
        public static bool operator <=(AsciiChar left, AsciiChar right)
        {
            return left.AsByte <= right.AsByte;
        }

        #endregion

        #region overrides


        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj is char)
            {
                result = Equals((char)obj);
            }
            else if (obj is AsciiChar)
            {
                result = Equals((AsciiChar)obj);
            }

            return result;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return AsChar.ToString();
        }


        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        #endregion

        #region public properties
        /// <summary>
        /// Returns a new AsciiChar that contains the current culture's decimal separator char. 
        /// </summary>
        public static AsciiChar DecimalSeparator
        {
            get { return new AsciiChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]); }
        }

        /// <summary>
        /// Returns a new AsciiChar that contains the current culture's negative sign. 
        /// </summary>
        public static AsciiChar CulturalNegativeSign
        {
            get { return new AsciiChar(CultureInfo.CurrentCulture.NumberFormat.NegativeSign[0]); }
        }

        /// <summary>
        /// Returns a new AsciiChar that contains the period character('.'). 
        /// </summary>
        public static AsciiChar DecimalPoint
        {
            get { return new AsciiChar(byte_decimalPoint); }
        }

        /// <summary>
        ///  Returns a new AsciiChar that contains the comma character(',').
        /// </summary>
        public static AsciiChar Comma
        {
            get { return new AsciiChar(byte_comma); }
        }

        /// <summary>
        /// Returns a new AsciiChar that contains the 'D' char/nybble used to indicate a negative value in a PackedDecimal.
        /// </summary>
        public static AsciiChar PackedNegativeNybble
        {
            get { return new AsciiChar(byte_HexDChar); }
        }

        /// <summary>
        /// Returns a new AsciiChar that contains the 'C' char/nybble used to indicate a positive value in a PackedDecimal.
        /// </summary>
        public static AsciiChar PackedPositiveNybble
        {
            get { return new AsciiChar(byte_HexCChar); }
        }

        /// <summary>
        /// Returns a new AsciiChar that contains the negative sign ('-'). 
        /// </summary>
        [Obsolete("Use CulturalNegativeSign instead.", false)]
        public static AsciiChar NegativeSign
        {
            get { return new AsciiChar(byte_NegativeSign); }
        }

        /// <summary>
        /// Returns a value representing the largest possible value of an AsciiChar. 
        /// </summary>
        public static AsciiChar MaxValue
        {
            get { return new AsciiChar(byte.MaxValue); }
        }

        /// <summary>
        /// Returns a value representing the smallest possible value of an AsciiChar. 
        /// </summary>
        public static AsciiChar MinValue
        {
            get { return new AsciiChar(byte.MinValue); }
        }

        /// <summary>
        /// Returns the value of this AsciiChar cast to a <c>byte</c> value.
        /// </summary>
        public byte AsByte
        {
            get { return value; }
        }

        /// <summary>
        /// Returns the value of this AsciiChar cast to a UTF-16 <c>char</c>.
        /// </summary>
        public char AsChar
        {
            get { return ToChar(null); }
        }

        /// <summary>
        ///  Returns a new AsciiChar that contains hex 9F
        /// </summary>
        public static AsciiChar Db2ConnectHighValue
        {
            get { return new AsciiChar(byte_Db2ConnectHighValue); }
        }


        #endregion

        #region IComparable
        /// <summary>
        /// Compares the current instance with another object and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        public int CompareTo(object other)
        {
            if (!(other is char) || !(other is AsciiChar))
                throw new ArgumentException("Argument must be char or AsciiChar");

            int result = 1;

            if (other != null)
            {
                if (other is AsciiChar)
                {
                    result = CompareTo((AsciiChar)other);
                }
                else
                {
                    result = CompareTo((char)other);
                }
            }

            return result;
        }

        /// <summary>
        /// Compares the current object with another object <c>char</c> type.
        /// </summary>
        public int CompareTo(char other)
        {
            return AsChar.CompareTo(other);
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        public int CompareTo(AsciiChar other)
        {
            return value.CompareTo(other.AsByte);
        }
        #endregion

        #region IEquatable
        /// <summary>
        /// Indicates whether the current object is equal to another object of type <c>char</c>.
        /// </summary>
        public bool Equals(char other)
        {
            return AsChar.Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        public bool Equals(AsciiChar other)
        {
            return object.ReferenceEquals(this, other) ?
                       true :
                       value.Equals(other.AsByte);
        }
        #endregion

        #region IConvertible
        /// <summary>
        /// Returns the <see cref="T:System.TypeCode" /> for this instance.
        /// </summary>
        /// <returns>
        /// The enumerated constant that is the <see cref="T:System.TypeCode" /> of the class or value type that implements this interface.
        /// </returns>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Byte;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent Boolean value.
        /// </summary>
        /// <returns>
        /// A Boolean value equivalent to the value of this instance.
        /// </returns>
        /// <exception cref="AsciiCharException">Value of AsciiChar cannot be converted.</exception>
        public bool ToBoolean(IFormatProvider provider)
        {
            bool result = default(bool);
            if (trueBytes.Contains(value))
            {
                result = true;
            }
            else if (falseBytes.Contains(value))
            {
                result = false;
            }
            else
            {
                throw new AsciiCharException(null, "Unable to convert value {0:x} to Boolean", value);
            }
            return result;
        }



        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit unsigned integer.
        /// </summary>
        /// <returns>
        /// An 8-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        public byte ToByte(IFormatProvider provider)
        {
            return value;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent Unicode character.
        /// </summary>
        /// <returns>
        /// A Unicode character equivalent to the value of this instance.
        /// </returns>
        public char ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(value);
        }

        /// <summary>
        /// Throws an InvalidCastException if called.
        /// </summary>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.Decimal" /> number.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Decimal" /> number equivalent to the value of this instance.
        /// </returns>
        public decimal ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(value);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent double-precision floating-point number.
        /// </summary>
        /// <returns>
        /// A double-precision floating-point number equivalent to the value of this instance.
        /// </returns>
        public double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(value);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit signed integer.
        /// </summary>
        /// <returns>
        /// An 16-bit signed integer equivalent to the value of this instance.
        /// </returns>
        public short ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(value);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit signed integer.
        /// </summary>
        /// <returns>
        /// An 32-bit signed integer equivalent to the value of this instance.
        /// </returns>
        public int ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit signed integer.
        /// </summary>
        /// <returns>
        /// An 64-bit signed integer equivalent to the value of this instance.
        /// </returns>
        public long ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(value);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit signed integer.
        /// </summary>
        /// <returns>
        /// An 8-bit signed integer equivalent to the value of this instance.
        /// </returns>
        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(value);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent single-precision floating-point number.
        /// </summary>
        /// <returns>
        /// A single-precision floating-point number equivalent to the value of this instance.
        /// </returns>
        public float ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.String" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String" /> instance equivalent to the value of this instance.
        /// </returns>
        public string ToString(IFormatProvider provider)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts the value of this instance to an <see cref="T:System.Object" /> of the specified <see cref="T:System.Type" /> 
        /// that has an equivalent value, using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object" /> instance of type <paramref name="conversionType" /> whose value is equivalent to the value of this instance.
        /// </returns>
        /// <param name="conversionType">
        /// The <see cref="T:System.Type" /> to which the value of this instance is converted. 
        /// </param>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            object result = null;

            if (conversionType.Equals(typeof(AsciiChar)))
            {
                result = new AsciiChar(value);
            }
            else if (conversionType.Equals(typeof(string)))
            {
                result = ToString(provider);
            }
            else if (conversionType.Equals(typeof(bool)))
            {
                result = ToBoolean(provider);
            }
            else if (conversionType.Equals(typeof(float)))
            {
                result = ToSingle(provider);
            }
            else if (conversionType.Equals(typeof(double)))
            {
                result = ToDouble(provider);
            }
            else if (conversionType.Equals(typeof(decimal)))
            {
                result = ToDecimal(provider);
            }
            else if (conversionType.Equals(typeof(short)))
            {
                result = ToInt16(provider);
            }
            else if (conversionType.Equals(typeof(int)))
            {
                result = ToInt32(provider);
            }
            else if (conversionType.Equals(typeof(long)))
            {
                result = ToInt64(provider);
            }
            else if (conversionType.Equals(typeof(ushort)))
            {
                result = ToUInt16(provider);
            }
            else if (conversionType.Equals(typeof(uint)))
            {
                result = ToUInt32(provider);
            }
            else if (conversionType.Equals(typeof(ulong)))
            {
                result = ToUInt64(provider);
            }
            else if (conversionType.Equals(typeof(byte)))
            {
                result = ToByte(provider);
            }
            else if (conversionType.Equals(typeof(sbyte)))
            {
                result = ToSByte(provider);
            }
            else
            {
                throw new InvalidCastException();
            }

            return result;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit unsigned integer.
        /// </summary>
        /// <returns>
        /// An 16-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        public UInt16 ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(value);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit unsigned integer.
        /// </summary>
        /// <returns>
        /// An 32-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        public UInt32 ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(value);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit unsigned integer.
        /// </summary>
        /// <returns>
        /// An 64-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        public UInt64 ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(value);
        }
        #endregion





    }
}

