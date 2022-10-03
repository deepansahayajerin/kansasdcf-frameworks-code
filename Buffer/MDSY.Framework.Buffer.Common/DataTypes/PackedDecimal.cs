using System;
using System.Collections.Generic;
using System.Linq;

using System.Globalization;
using System.Text;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Encapsulates an immutable PackedDecimal value, providing access to either the numeric value or to the compressed 
    /// byte representation. 
    /// </summary>
    [Serializable]
    public struct PackedDecimal :
        IComparable,
        IComparable<PackedDecimal>, IEquatable<PackedDecimal>,
        IComparable<Double>, IEquatable<Double>,
        IComparable<Decimal>, IEquatable<Decimal>,
        IConvertible
    {
        #region private

        private static PackedDecimal FromWholeNumberText(string numericText, int decimalDigitsCount, bool isSigned)
        {
            if (numericText.Length <= decimalDigitsCount)
            {
                numericText.PadRight(decimalDigitsCount + 1, '0');
            }

            numericText = numericText.Insert(numericText.Length - (decimalDigitsCount), DecimalSeparator);
            Decimal value;
            try
            {
                value = Decimal.Parse(numericText);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(string.Format("Unable to convert value {0} to PackedDecimal.", numericText), ex);
            }
            return new PackedDecimal(value, isSigned, decimalDigitsCount);
        }

        private static Decimal Deserialize(byte[] bytes, int digits)
        {
            return Comp3Serializer.GetDecimalFromPackedBytes(bytes, digits);
        }
        private Byte[] cachedPackedBytes;
        private readonly Int64 decimalValue;


        private static string ConditionallyPadDecimalDigits(string numText, int digits)
        {
            string result = numText;
            int decIdx = result.IndexOf(DecimalSeparator);

            if (decIdx >= 0)
            {
                int actualDigitsToRight = (result.Length - 1) - decIdx;
                int correctedTotalLength = decIdx + 1 + digits;

                if ((actualDigitsToRight) < digits)
                {
                    result = result.PadRight(correctedTotalLength, '0');
                }
            }

            return result;
        }

        private long GetRightOfDecimalDigits()
        {
            long result = 0;
            if (Digits > 0)
            {
                return Math.Abs((long)(Value % 1 * (decimal)Math.Pow(10, Digits)));
            }
            return result;
        }

        private bool isSigned;
        public bool IsSigned
        {
            get { return isSigned; }
            private set { this.isSigned = value; }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Converts provided decimal value to an array of packed bytes.
        /// </summary>
        /// <param name="value">Decimal value for conversion.</param>
        /// <returns>Returns an array of packed bytes.</returns>
        public byte[] GetPackedBytes(decimal value)
        {
            string numText = value.ToString();
            if (isSigned)
            {
                if (numText[0] != ('-') && numText[0] != ('+'))
                    numText = "+" + numText;
            }
            // double-check number of digits to right of decimal.
            numText = ConditionallyPadDecimalDigits(numText, Digits);
            return Comp3Serializer.GetPackedBytes(numText);
        }
        #endregion

        #region constructors

        public PackedDecimal(bool isSigned)
            :this()
        {
            this.isSigned = isSigned;
        }

        /// <summary>
        /// Initializes a new instance of the PackedDecimal structure.
        /// </summary>
        /// <param name="value">The decimal value immutably contained by this instance.</param>
        /// <param name="digits">The number of digits to the right of the decimal point.</param>
        public PackedDecimal(Decimal value, bool isSigned, int digits = 2)
            : this(isSigned)
        {
            Digits = digits;
            Value = value;
            decimalValue = GetRightOfDecimalDigits();

            // since PackedDecimal is immutable, calculate and cache the packed bytes once.
            cachedPackedBytes = GetPackedBytes(value);
        }

        /// <summary>
        /// Initializes a new instance of the PackedDecimal structure.
        /// </summary>
        /// <param name="value">The decimal value immutably contained by this instance.</param>
        /// <param name="digits">The number of digits to the right of the decimal point.</param>
        public PackedDecimal(Double value, bool isSigned, int digits = 2)
            : this(Convert.ToDecimal(value), isSigned, digits)
        {
        }
        #endregion

        #region IComparable
        /// <summary>
        /// Compares the current instance with another object of the same type and returns an 
        /// integer that indicates whether the current instance precedes, 
        /// follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. 
        /// </returns>
        /// <param name="obj">
        /// An object to compare with this instance. 
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="obj" /> is not the same type as this instance. 
        /// </exception>
        public int CompareTo(object obj)
        {
            if (obj.GetType().Equals(typeof(PackedDecimal)))
            {
                return CompareTo((PackedDecimal)obj);
            }
            else
            {
                throw new ArgumentException("Argument is not a PackedDecimal", "obj");
            }
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. 
        /// </returns>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        public int CompareTo(PackedDecimal other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <summary>
        /// Compares the current object with another object.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. 
        /// </returns>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        public int CompareTo(Double other)
        {
            return Value.CompareTo(Convert.ToDecimal(other));
        }

        /// <summary>
        /// Compares the current object with another object.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. 
        /// </returns>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        public Int32 CompareTo(Decimal other)
        {
            return Value.CompareTo(other);
        }
        #endregion

        #region IEquatable
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        public bool Equals(PackedDecimal other)
        {
            bool result = false;

            if (object.ReferenceEquals(this, other))
            {
                result = true;
            }
            else
            {
                result = Value.Equals(other.Value);
            }

            return result;

        }

        /// <summary>
        /// Indicates whether the current object is equal to another object.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        public bool Equals(Double other)
        {
            return Value.Equals(Convert.ToDecimal(other));
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        public Boolean Equals(Decimal other)
        {
            return Value.Equals(other);
        }
        #endregion

        #region new PackedDecimal instantiators
        /// <summary>
        /// Returns a new PackedDecimal instance containing the value given by <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <param name="decimalDigits"></param>
        public static PackedDecimal From(object value, int decimalDigits, bool isSigned)
        {
            PackedDecimal result = new PackedDecimal(isSigned);

            if (value != null)
            {
                //decimal decValue;
                if (value is decimal)
                {
                    result = new PackedDecimal((decimal)value, isSigned, decimalDigits);
                }
                else if (value is double || value is float ||
                         value is UInt16 || value is UInt32 || value is UInt64 ||
                         value is Int16 || value is Int32 || value is Int64)
                {
                    string specifier = string.Format("F{0}", decimalDigits);
                    var decValue = Convert.ToDecimal(value);
                    result = Parse(decValue.ToString(specifier));
                }
                else
                {
                    result = Parse(value.ToString());
                }
                //result = new PackedDecimal(decValue, decimalDigits);
            }
            return result;
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing the value represented by the given <paramref name="bytes"/>.
        /// </summary>
        /// <param name="bytes">The byte array containing the serialized decimal value.</param>
        /// <param name="digits">The number of digits to the right of the decimal for the deserialized value.</param>
        /// <returns>A new PackedDecimal.</returns>
        public static PackedDecimal FromBytes(byte[] bytes, bool isSigned, int digits = 2)
        {
            Decimal value = Deserialize(bytes, digits);
            return FromDecimal(value, isSigned, digits);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing the given <paramref name="value"/>.
        /// </summary>
        /// <returns>A new PackedDecimal.</returns>
        //public static PackedDecimal FromInt16(Int16 value, int digits = 0)
        public static PackedDecimal FromInt16(Int16 value, bool isSigned)
        {
            return FromInt64(value, isSigned);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing the given <paramref name="value"/>.
        /// </summary>
        /// <returns>A new PackedDecimal.</returns>
        //public static PackedDecimal FromInt32(Int32 value, int digits = 0)
        public static PackedDecimal FromInt32(Int32 value, bool isSigned)
        {
            return FromInt64(value, isSigned);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing the given <paramref name="value"/>.
        /// </summary>
        /// <returns>A new PackedDecimal.</returns>
        //public static PackedDecimal FromInt64(Int64 value, int digits = 0)
        public static PackedDecimal FromInt64(Int64 value, bool isSigned)
        {
            return FromDecimal(value, isSigned, 0);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing the given <paramref name="value"/>.
        /// </summary>
        /// <returns>A new PackedDecimal.</returns>
        public static PackedDecimal FromSingle(Single value, bool isSigned, int digits = 2)
        {
            return new PackedDecimal(Convert.ToDouble(value), isSigned, digits);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing the given value.
        /// </summary>
        /// <param name="value">The decimal value immutably contained by the new instance.</param>
        /// <param name="digits">The number of digits to the right of the decimal point.</param>
        /// <returns>A new PackedDecimal.</returns>
        public static PackedDecimal FromDouble(Double value, bool isSigned, int digits = 2)
        {
            return new PackedDecimal(value, isSigned, digits);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing the given value.
        /// </summary>
        /// <param name="value">The decimal value immutably contained by the new instance.</param>
        /// <param name="digits">The number of digits to the right of the decimal point.</param>
        /// <returns>A new PackedDecimal.</returns>
        public static PackedDecimal FromDecimal(Decimal value, bool isSigned, int digits = 2)
        {
            return new PackedDecimal(value, isSigned, digits);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing the given value.
        /// </summary>
        /// <param name="value">The UInt16 value immutably contained by the new instance.</param>
        /// <returns>A new PackedDecimal.</returns>
        //public static PackedDecimal FromUInt16(UInt16 value, int digits = 0)
        public static PackedDecimal FromUInt16(UInt16 value, bool isSigned)
        {
            return FromUInt64(value, isSigned);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing the given value.
        /// </summary>
        /// <param name="value">The UInt32 value immutably contained by the new instance.</param>
        /// <returns>A new PackedDecimal.</returns>
        //public static PackedDecimal FromUInt32(UInt32 value, int digits = 0)
        public static PackedDecimal FromUInt32(UInt32 value, bool isSigned)
        {
            return FromUInt64(value, isSigned);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing the given value.
        /// </summary>
        /// <param name="value">The UInt64 value immutably contained by the new instance.</param>
        /// <returns>A new PackedDecimal.</returns>
        //public static PackedDecimal FromUInt64(UInt64 value, int digits = 2)
        public static PackedDecimal FromUInt64(UInt64 value, bool isSigned)
        {
            return new PackedDecimal((decimal)value, isSigned, 0);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing the absolute value of this PackedDecimal.
        /// </summary>
        public PackedDecimal AbsoluteValue()
        {
            return new PackedDecimal(Math.Abs(Value), false, Digits);
        }

        public static PackedDecimal FromWholeNumberRepresentation(Int64 wholeNumber, int decimalDigitsCount, bool isSigned)
        {
            string numericText = wholeNumber.ToString();
            if (isSigned)
            {
                if (!numericText.StartsWith("-") && !numericText.StartsWith("+"))
                    numericText = "+" + numericText;
            }
            return FromWholeNumberText(numericText, decimalDigitsCount, isSigned);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing a <c>double</c> value rehydrated from the 
        /// combination of the given <paramref name="wholeNumber"/> and the value of <paramref name="digits"/>.
        /// </summary>
        /// <param name="wholeNumber">A whole number for conversion.</param>
        /// <param name="digits">Specifies the number of digits to the right from the decimal separator.</param>
        /// <example>
        /// Calling:
        /// <code>
        /// PackedDecimal dec = PackedDecimal.FromWhole(1414, 3);
        /// </code>
        /// would result in: <para>dec.Value == 1.414</para><para>dec.Digits == 3</para>
        /// </example>
        /// <returns>Returns a new PackedDecimal instance.</returns>
        [Obsolete("The purpose for this method appears to have been misunderstood; it was only for use with the unpacking process and would therefore never have a decimal point or be negative. This method will be replaced with FromWholeNumberRepresentation(). If you meant to create a new PackedDecimal from an Int64, use FromInt64() instead.", true)]
        public static PackedDecimal FromWhole(Int64 wholeNumber, bool isSigned, int digits = 2)
        {
            bool isNegative = (wholeNumber < 0);
            StringBuilder strValue = new StringBuilder();
            strValue.Append(wholeNumber.ToString().Replace("-", ""));
            if (digits > 0)
            {
                strValue.Append((DecimalSeparatorChar));
                while (strValue.Length < digits + wholeNumber.ToString().Length + 1)
                {
                    strValue.Append('0');
                }
            }
            string stringValue = strValue.ToString();

            Decimal value = Decimal.Parse(stringValue);
            if (isNegative)
            {
                value = value * -1;
            }
            return new PackedDecimal(value, isSigned, digits);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance containing a <c>double</c> value rehydrated from the 
        /// combination of the given <paramref name="wholeNumber"/> and the value of <paramref name="digits"/>.
        /// </summary>
        /// <param name="wholeNumber">A whole number for conversion.</param>
        /// <param name="digits">Specifies the number of digits to the right from the decimal separator.</param>
        /// <example>
        /// Calling:
        /// <code>
        /// PackedDecimal dec = PackedDecimal.FromWhole(1414, 3);
        /// </code>
        /// would result in: <para>dec.Value == 1.414</para><para>dec.Digits == 3</para>
        /// </example>
        /// <returns>Returns a new PackedDecimal instance.</returns>
        [Obsolete("This method should be unneccessary (a whole number cannot have decimal digits), and appears to duplicate FromWhole(), and will be removed - replaced with FromWholeNumberRepresentation().", true)]
        public static PackedDecimal FromWholeWithDec(Int64 wholeNumber, bool isSigned, int digits = 2)
        {
            bool isNegative = (wholeNumber < 0);

            var chars = wholeNumber.ToString().Replace("-", "").Reverse().ToList();
            while (chars.Count() < digits)
            {
                chars.Add('0');
            }

            var hydrated = chars.Take(digits).ToList();
            hydrated.Add(DecimalSeparatorChar);
            var finalChars = hydrated.Concat(chars.Skip(digits)).Reverse().ToArray();

            string stringValue = new string(finalChars);
            if (stringValue[stringValue.Length - 1] == DecimalSeparatorChar)
            {
                stringValue = stringValue.Remove(stringValue.Length - 1);
            }

            Decimal value = Decimal.Parse(stringValue);
            if (isNegative)
            {
                value = value * -1;
            }
            return new PackedDecimal(value, isSigned, digits);
        }

        /// <summary>
        /// Returns a new PackedDecimal instance using this instance's Bytes, but with a new Digits value, essentially 
        /// moving the value's decimal point.
        /// </summary>
        /// <param name="newDigitsValue">The value for the new PackedDecimal's Digits property.</param>
        /// <returns>A new PackedDecimal.</returns>
        public PackedDecimal MoveDecimalPoint(int newDigitsValue, bool isSigned)
        {
            return FromBytes(Bytes, isSigned, newDigitsValue);
        }

        /// <summary>
        /// Converts the string representation of a number to its PackedDecimal equivalent.
        /// </summary>
        /// <param name="numericText"></param>
        /// <returns>A new PackedDecimal containing the value represented by <paramref name="numericText"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="numericText"/> is null or empty.</exception>
        /// <exception cref="InvalidCastException"><paramref name="numericText"/> is non-numeric or there was a problem parsing.</exception>
        public static PackedDecimal Parse(string numericText)
        {
            if (string.IsNullOrEmpty(numericText))
                throw new ArgumentNullException("numericText");

            string workText = numericText;
            int digits = 0;
            try
            {
                int decimalIdx = workText.IndexOf(DecimalSeparator);
                if ((decimalIdx > -1) && (decimalIdx < workText.Length - 1))
                {
                    // how many digits to right of decimal?
                    digits = (workText.Length - 1) - decimalIdx;
                    workText = workText.Remove(decimalIdx, DecimalSeparator.Length);
                }
                else
                {
                    if (decimalIdx > -1)
                    {
                        workText = workText.Remove(decimalIdx, DecimalSeparator.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(string.Format("Unable to convert value {0} to PackedDecimal.", numericText), ex);
            }

            bool isSigned = workText[0] == ('-') || workText[0] == ('+');

            return PackedDecimal.FromWholeNumberText(workText, digits, isSigned);
        }



        /// <summary>
        /// Attempts to convert the string representation of a number to its PackedDecimal equivalent. 
        /// </summary>
        /// <param name="numericText">A string that contains numeric value for parsing.</param>
        /// <param name="value">Receives the new PackedDecimal value.</param>
        /// <returns><c>true</c> if the string value can be converted, otherwise <c>false</c>.</returns>
        public static bool TryParse(string numericText, out PackedDecimal value)
        {
            bool isSigned = numericText.StartsWith("-") || numericText.StartsWith("+");

            value = new PackedDecimal(isSigned);
            try
            {
                value = Parse(numericText);
                return true;
            }
            catch
            {
                return false;
            }
        }


        #endregion

        #region public properties
        /// <summary>
        /// Returns the DecimalSeparator string specific to the current culture. 
        /// </summary>
        public static string DecimalSeparator
        {
            get { return Comp3Serializer.DecimalSeparator; }
        }

        /// <summary>
        /// Returns the DecimalSeparator char specific to the current culture.
        /// </summary>
        public static char DecimalSeparatorChar
        {
            get { return Comp3Serializer.DecimalSeparatorChar; }
        }

        /// <summary>
        /// Gets the number of digits to the right of the decimal point.
        /// </summary>
        public Int32 Digits { get; private set; }


        /// <summary>
        /// Returns the NumberFormatInfo for the current culture.
        /// </summary>
        public static NumberFormatInfo NumberFormatInfo
        {
            get { return Comp3Serializer.NumberFormatInfo; }
        }

        /// <summary>
        /// Gets the immutable value of this PackedDecimal instance. 
        /// </summary>
        public Decimal Value { get; private set; }

        /// <summary>
        /// Returns the portion of this PackedDecimal instance to the left of the decimal sign. 
        /// </summary>
        public Int64 WholeValue
        {
            get { return (Int64)Math.Truncate(Value); }
        }

        /// <summary>
        /// Returns the portion of this PackedDecimal instance to the right of the decimal sign.
        /// </summary>
        public Int64 DecimalValue
        {
            get { return decimalValue; }
        }


        /// <summary>
        /// Gets the value of the compressed numeric in its compressed-to-bytes form. 
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                if (cachedPackedBytes == null)
                {
                    cachedPackedBytes = GetPackedBytes(Value);
                }
                return cachedPackedBytes;
            }
        }
        #endregion

        #region overrides

        /// <summary>
        /// Returns a string representation of the current uncompressed, 
        /// or "true" decimal value.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Value.ToString();
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
            return TypeCode.Double;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent Boolean value using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A Boolean value equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public Boolean ToBoolean(IFormatProvider provider)
        {
            return Value == 0;
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 8-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public Byte ToByte(IFormatProvider provider)
        {
            if (Value > byte.MaxValue || Value < byte.MinValue)
                throw new InvalidCastException();
            if (DecimalValue != 0)
                throw new InvalidCastException();

            return Convert.ToByte(WholeValue, provider);
        }

        /// <summary>
        /// Calling this method will throw a System.InvalidCastException.
        /// </summary>
        public Char ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Calling this method will throw a System.InvalidCastException.
        /// </summary>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.Decimal" /> number using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Decimal" /> number equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public Decimal ToDecimal(IFormatProvider provider)
        {
            return Value;
        }

        /// <summary>
        /// Returns the value of the PackedDecimal instance as a Double. 
        /// </summary>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.
        /// </param>
        /// <returns></returns>
        public Double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(Value, provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 16-bit signed integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public Int16 ToInt16(IFormatProvider provider)
        {
            if (Value > Int16.MaxValue || Value < Int16.MinValue)
                throw new InvalidCastException();
            //if (DecimalValue != 0)
            //    throw new InvalidCastException();

            return Convert.ToInt16(WholeValue, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 32-bit signed integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public Int32 ToInt32(IFormatProvider provider)
        {
            if (Value > Int32.MaxValue || Value < Int32.MinValue)
                throw new InvalidCastException();
            //if (DecimalValue != 0)
            //    throw new InvalidCastException();

            return Convert.ToInt32(WholeValue, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 64-bit signed integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public Int64 ToInt64(IFormatProvider provider)
        {
            if (Value > Int64.MaxValue || Value < Int64.MinValue)
                throw new InvalidCastException();
            //if (DecimalValue != 0)
            //    throw new InvalidCastException();

            return Convert.ToInt64(WholeValue, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 8-bit signed integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public SByte ToSByte(IFormatProvider provider)
        {
            if (Value > SByte.MaxValue || Value < SByte.MinValue)
                throw new InvalidCastException();
            if (DecimalValue != 0)
                throw new InvalidCastException();

            return Convert.ToSByte(WholeValue, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent single-precision floating-point number using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A single-precision floating-point number equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public Single ToSingle(IFormatProvider provider)
        {
            decimal max = Convert.ToDecimal(Single.MaxValue);
            decimal min = Convert.ToDecimal(Single.MinValue);

            if (Value > max || Value < min)
                throw new InvalidCastException();

            return Convert.ToSingle(Value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.String" /> using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String" /> instance equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public string ToString(IFormatProvider provider)
        {
            return Value.ToString(provider);
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

            if (conversionType.Equals(typeof(PackedDecimal)))
            {
                result = this;
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
        /// Converts the value of this instance to an equivalent 16-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 16-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public UInt16 ToUInt16(IFormatProvider provider)
        {
            if (Value > UInt16.MaxValue || Value < UInt16.MinValue)
                throw new InvalidCastException();
            if (DecimalValue != 0)
                throw new InvalidCastException();

            return Convert.ToUInt16(WholeValue, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 32-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public UInt32 ToUInt32(IFormatProvider provider)
        {
            if (Value > UInt32.MaxValue || Value < UInt32.MinValue)
                throw new InvalidCastException();
            if (DecimalValue != 0)
                throw new InvalidCastException();

            return Convert.ToUInt32(WholeValue, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 64-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information. 
        /// </param>
        public UInt64 ToUInt64(IFormatProvider provider)
        {
            if (Value > UInt64.MaxValue || Value < UInt64.MinValue)
                throw new InvalidCastException();
            if (DecimalValue != 0)
                throw new InvalidCastException();

            return Convert.ToUInt64(WholeValue, provider);
        }
        #endregion
    }
}

