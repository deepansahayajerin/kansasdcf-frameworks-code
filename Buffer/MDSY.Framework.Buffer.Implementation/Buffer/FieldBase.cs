using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Common;
using Unity;
using MDSY.Framework.Buffer.Interfaces;
using System.ComponentModel;
using MDSY.Framework.Buffer;
using System.Linq.Expressions;
using System.Globalization;
using MDSY.Framework.Buffer.Services;
using System.Text;
using System.Collections.Concurrent;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Base class for IField-implementing objects.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    internal abstract class FieldBase : BufferElementBase, IBufferElement, IField, IAssignable, IFieldInitializer
    {
        #region private flelds
        private IFieldValueSerializer serializer;
        private Dictionary<string, ICheckField> checkFields;
        [ThreadStatic]
        private static FieldComparer comparer = null;
        private static Char DecimalSeparatorChar
        {
            get { return CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]; }
        }
        #endregion

        ///// <summary>
        ///// Converts the value of the given <paramref name="source"/> field to a 
        ///// byte array of length <paramref name="outputLength"/>, serialized 
        ///// according to the given <paramref name="outputFieldType"/>
        ///// </summary>
        ///// <param name="source">The field containing the value to be converted.</param>
        ///// <param name="outputFieldType">Determines the serialization process for the result byte[].</param>
        ///// <param name="outputLength">Specifies the length of the result byte[].</param>
        ///// <returns>A byte array serialized according to the needs of <paramref name="outputFieldType"/>.</returns>
        ///// <param name="outputDecimalDigits"></param>
        //private static byte[] SerializeFromFieldValue(IField source, FieldType outputFieldType,
        //    int outputLength, int outputDecimalDigits = 0)
        //{
        //    byte[] result = default(byte[]);
        //    NumericFieldType sourceType = source.GetNumericType();
        //    NumericFieldType outputType = FieldTypeUtils.GetNumericType(outputFieldType);

        //    switch (outputType)
        //    {
        //        case NumericFieldType.Unknown:
        //        case NumericFieldType.NotNumeric:
        //            if (outputFieldType == FieldType.String)
        //            {
        //                result = ValueToBytesAsString(source.DisplayValue, outputLength);
        //            }
        //            else if (outputFieldType == FieldType.Boolean)
        //            {
        //                result = ValueToBytesAsBool(source.GetValue<bool>(), outputLength);
        //            }
        //            break;
        //        case NumericFieldType.SignedInteger:
        //            result = ValueToBytesAsSignedNumeric(source.GetValue<Int64>(), outputLength);
        //            break;
        //        case NumericFieldType.UnsignedInteger:
        //            result = ValueToBytesAsUnsignedNumeric(source.GetValue<UInt64>(), outputLength);
        //            break;
        //        case NumericFieldType.Decimal:
        //            if (outputFieldType == FieldType.UnsignedDecimal)
        //            {
        //                result = ValueToBytesAsUnsignedDecimal(source.GetValue<Decimal>(), outputLength, outputDecimalDigits);
        //            }
        //            else
        //            {
        //                result = ValueToBytesAsSignedDecimal(source.GetValue<Decimal>(), outputLength, outputDecimalDigits);
        //            }
        //            break;
        //        case NumericFieldType.PackedDecimal:
        //            PackedDecimal tempPacked = default(PackedDecimal);
        //            switch (sourceType)
        //            {
        //                case NumericFieldType.Unknown:
        //                case NumericFieldType.NotNumeric:
        //                    if (PackedDecimal.TryParse(source.DisplayValue, out tempPacked))
        //                    {
        //                        result = tempPacked.Bytes;
        //                    }
        //                    break;
        //                case NumericFieldType.SignedInteger:
        //                case NumericFieldType.UnsignedInteger:
        //                case NumericFieldType.Decimal:
        //                    decimal dec = source.GetValue<Decimal>();
        //                    //tempPacked = PackedDecimal.FromDecimal(dec, source.DecimalDigits);
        //                    tempPacked = PackedDecimal.FromDecimal(dec, outputDecimalDigits);
        //                    result = tempPacked.Bytes;
        //                    break;
        //                case NumericFieldType.PackedDecimal:
        //                    result = source.AsBytes;
        //                    break;
        //            }
        //            break;
        //    }


        //    // trim bytes as needed.

        //    return result;
        //}

        //private void AssignFromField(IField source)
        //{
        //    byte[] bytes;

        //    if ((source.FieldType == FieldType) &&
        //        (source.LengthInBuffer == LengthInBuffer))
        //    {
        //        bytes = source.AsBytes;
        //    }
        //    else
        //    {
        //        bytes = SerializationServices.Serialize(source, this.LengthInBuffer, this.FieldType, this.DecimalDigits);
        //    }
        //    AssignFrom(bytes);

        //    //if (source.FieldType == FieldType.PackedDecimal)
        //    //{
        //    //    // Temp fix - smart this up.
        //    //    AssignFrom(Convert.ToDecimal(source.DisplayValue));
        //    //}
        //    //else
        //    //{
        //    //    var sourceBytes = source.AsBytes;

        //    //    if (source.FieldType != FieldType)
        //    //    {
        //    //        Object newValue = null;
        //    //        Serializer.TryDeserialize(sourceBytes, source.FieldType, DecimalDigits, out newValue);
        //    //        if (FieldType == FieldType.String && source.FieldType != FieldType.String)
        //    //        {
        //    //            sourceBytes = Serializer.Serialize(newValue.ToString().PadLeft(sourceBytes.Length, '0'), LengthInBuffer, FieldType, DecimalDigits);
        //    //        }
        //    //        else
        //    //        {
        //    //            sourceBytes = Serializer.Serialize(newValue, LengthInBuffer, FieldType, DecimalDigits);
        //    //        }
        //    //    }

        //    //    if (IsNumericType &&
        //    //        (source.FieldType != FieldType.String) &&
        //    //        (source.FieldType != FieldType.Boolean) &&
        //    //        (source.FieldType != FieldType.ReferencePointer) &&
        //    //        (Bytes.Length < sourceBytes.Length))
        //    //    {
        //    //        sourceBytes = sourceBytes.Skip(sourceBytes.Length - Bytes.Length).ToArray();
        //    //    }

        //    //    AssignFrom(sourceBytes);
        //    //}
        //}


        #region private methods

        /// <summary>
        /// Yes, this duplicates code from StructureDefinitionCompositor. Smart this up. 
        /// </summary>
        private static IEditableArrayElementAccessor<T> CreateAccessor<T>(string arrayName,
            IDictionary<string, IArrayElementAccessorBase> accessors, T element)
            where T : IArrayElement, IBufferElement
        {
            IEditableArrayElementAccessor<T> result = ObjectFactory.Factory.NewArrayElementAccessorObject<T>(arrayName);
            result.AddElement(element);
            accessors.Add(arrayName, result.AsReadOnly());
            return result;
        }

        private static bool GetIsNumericType(FieldType fieldType)
        {
            return ((fieldType == FieldType.CompInt) ||
                    (fieldType == FieldType.CompLong) ||
                    (fieldType == FieldType.CompShort) ||
                    (fieldType == FieldType.PackedDecimal) ||
                    (fieldType == FieldType.UnsignedPackedDecimal) ||
                    (fieldType == FieldType.SignedDecimal) ||
                    (fieldType == FieldType.SignedNumeric) ||
                    (fieldType == FieldType.UnsignedDecimal) ||
                    (fieldType == FieldType.UnsignedNumeric) ||
                     (fieldType == FieldType.FloatSingle) ||
                      (fieldType == FieldType.FloatDouble) ||
                      (fieldType == FieldType.ReferencePointer)
                    );
        }

        bool enqoutedValueStringClean = false;
        bool unenqoutedValueStringClean = false;
        private string enqoutedValueString;
        private string unenqoutedValueString;
        private string GetValueString(bool enquoteStrings)
        {
            if (lastRead != Buffer.WriteCount)
                ReloadBytes();
            if (enquoteStrings)
            {
                if (enqoutedValueStringClean) return enqoutedValueString;
                enqoutedValueString = CreateValueString(true);
                enqoutedValueStringClean = true;
                return enqoutedValueString;
            }

            if (unenqoutedValueStringClean) return unenqoutedValueString;
            unenqoutedValueString = CreateValueString(false);
            unenqoutedValueStringClean = true;
            return unenqoutedValueString;

        }

        private string CreateValueString(bool enquoteStrings)
        {
            string result = string.Empty;
            bool isPositive = true;
            if (FieldType == Common.FieldType.NumericEdited)
                return BytesAsString;
            if (FieldType == Common.FieldType.String)
            {
                string tempStr = string.Empty;
                if (Serializer.TryDeserialize(Bytes, FieldType, DecimalDigits, out tempStr))
                {
                    if (tempStr.TrimStart().StartsWith("-") || tempStr.EndsWith("-"))
                        isPositive = false;

                    if (IsBlankWhenZero)
                    {
                        int length = tempStr.Replace("0", "").Replace(".", "").Replace(",", "").Replace("-", "")
                            .Replace("+", "").Replace(" ", "").Length;
                        if (tempStr.Length == 0) // value = 0
                            tempStr = new string(' ', LengthInBuffer);
                        else
                        {
                            if (!string.IsNullOrEmpty(EditMask))
                                tempStr = ApplyEditMask(tempStr, isPositive, false);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(EditMask))
                            tempStr = ApplyEditMask(tempStr, isPositive, false);
                    }

                    string fmtString = enquoteStrings ? "'{0}'" : "{0}";
                    result = String.Format(fmtString, tempStr);
                }
            }
            else
            {
                bool isZero = false;
                switch (FieldType)
                {
                    // TODO: Handle truncations.
                    case FieldType.Boolean:
                        bool tempBool = false;
                        if (Serializer.TryDeserialize(Bytes, FieldType, DecimalDigits, out tempBool))
                        {
                            result = tempBool.ToString();
                        }
                        break;

                    case FieldType.PackedDecimal:
                    case FieldType.UnsignedPackedDecimal:
                        PackedDecimal tempPacked;
                        if (Serializer.TryDeserialize(Bytes, FieldType, DecimalDigits, out tempPacked))
                        {
                            isPositive = tempPacked.Value > 0;
                            isZero = tempPacked.Value == 0;
                            result = tempPacked.ToString();
                        }
                        else
                        {
                            // tempPacked = new PackedDecimal(FieldType == Common.FieldType.PackedDecimal);
                            result = "0";
                        }
                        break;

                    case FieldType.SignedNumeric:
                        Decimal tempDec;
                        if (Serializer.TryDeserialize(Bytes, FieldType, DecimalDigits, out tempDec))
                        {
                            isPositive = tempDec > 0;
                            isZero = tempDec == 0;
                            result = tempDec.ToString();
                        }
                        break;

                    case FieldType.UnsignedNumeric:
                        Decimal tempDec2;
                        if (Serializer.TryDeserialize(Bytes, FieldType, DecimalDigits, out tempDec2))
                        {
                            isZero = tempDec2 == 0;
                            result = tempDec2.ToString();
                            if (DecimalDigits > 0)
                            {
                                string tmpBytesToString = "";
                                string tmpValue = "";
                                tmpBytesToString = System.Text.Encoding.UTF8.GetString(Bytes);
                                tmpValue = tmpBytesToString.Substring(0, tmpBytesToString.Length - DecimalDigits);
                                result = tmpValue + "." + tmpBytesToString.Substring(tmpBytesToString.Length - DecimalDigits, DecimalDigits);
                                result = result.TrimStart(new Char[] { '0' });
                                if (result.StartsWith("."))
                                    result = result.Insert(0, "0");
                            }
                        }
                        break;

                    case FieldType.CompShort:
                        Int16 tempComp16;
                        if (Serializer.TryDeserialize(Bytes, FieldType, DecimalDigits, out tempComp16))
                        {
                            isPositive = tempComp16 > 0;
                            isZero = tempComp16 == 0;
                            result = tempComp16.ToString();
                        }
                        break;
                    case FieldType.ReferencePointer:
                    case FieldType.CompInt:
                        if (DecimalDigits > 0)
                        {
                            Decimal tmpDec;
                            if (Serializer.TryDeserialize(Bytes, FieldType, DecimalDigits, out tmpDec))
                            {
                                isPositive = tmpDec > 0;
                                isZero = tmpDec == 0;
                                result = tmpDec.ToString();
                            }
                        }
                        else
                        {
                            Int32 tempComp32;
                            if (Serializer.TryDeserialize(Bytes, FieldType, DecimalDigits, out tempComp32))
                            {
                                isPositive = tempComp32 > 0;
                                isZero = tempComp32 == 0;
                                result = tempComp32.ToString();
                            }
                        }
                        break;
                    case FieldType.CompLong:
                        Int64 tempComp64;
                        if (Serializer.TryDeserialize(Bytes, FieldType, DecimalDigits, out tempComp64))
                        {
                            isPositive = tempComp64 > 0;
                            isZero = tempComp64 == 0;
                            result = tempComp64.ToString();
                        }
                        break;

                    case FieldType.SignedDecimal:
                        Decimal tempSDec;
                        if (Serializer.TryDeserialize(Bytes, FieldType, DecimalDigits, out tempSDec))
                        {
                            isPositive = tempSDec > 0;
                            isZero = tempSDec == 0;
                            result = tempSDec.ToString();
                        }
                        break;

                    case FieldType.UnsignedDecimal:
                        Decimal tempUDec;
                        if (Serializer.TryDeserialize(Bytes, FieldType, DecimalDigits, out tempUDec))
                        {
                            isZero = tempUDec == 0;
                            result = tempUDec.ToString();
                        }

                        break;
                    case FieldType.FloatSingle:
                    case FieldType.FloatDouble:
                        Decimal tempFDec;
                        if (FieldType == FieldType.FloatSingle)
                            DisplayLength = 24;
                        else
                            DisplayLength = 48;
                        if (Serializer.TryDeserialize(Bytes, FieldType, DecimalDigits, out tempFDec))
                        {
                            isPositive = tempFDec > 0;
                            isZero = tempFDec == 0;
                            string tmpStr = tempFDec.ToString().Trim();

                            // decide on truncate/round
                            if (tmpStr.Replace("-", "").Replace(DecimalSeparatorChar.ToString(), "").Length > DisplayLength)
                            {
                                int dc = tmpStr.IndexOf(DecimalSeparatorChar);
                                int integerDigits = dc;
                                bool truncateFromLeft = false;

                                if (dc == -1)
                                {
                                    integerDigits = DisplayLength;
                                    truncateFromLeft = true;
                                }
                                else
                                {
                                    if (!isPositive)
                                        integerDigits = integerDigits - 1;
                                    if (integerDigits > DisplayLength)
                                    {
                                        integerDigits = DisplayLength;
                                        truncateFromLeft = true;
                                    }
                                }

                                int decimalDigits = DisplayLength - integerDigits;

                                // truncate/round from right
                                tmpStr = (BufferServices.Directives.FieldValueMoves == FieldValueMoveType.AdsoMoves)
                                    ? Decimal.Round(tempFDec, decimalDigits).ToString()
                                    : tmpStr.Substring(0, dc + decimalDigits + 1);

                                // truncate from left
                                if (truncateFromLeft)
                                {
                                    tmpStr = isPositive
                                        ? tmpStr.Substring(tmpStr.Length - integerDigits)
                                        : "-" + tmpStr.Substring(tmpStr.Length - integerDigits);
                                }
                            }

                            result = tmpStr;
                        }
                        break;

                }

                if (IsBlankWhenZero && isZero)
                    result = new string(' ', LengthInBuffer);
                else if (!string.IsNullOrEmpty(EditMask))
                    result = ApplyEditMask(result, isPositive);
            }

            return result;
        }

        private string ApplyEditMask(string value, bool isPositive, bool replaceDecimalSeparator = true)
        {
            //*** If any changes made to this method, please run unit tests and add unit for the issue being updated **
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(EditMask))
            {
                string editMask = EditMask;
                value = value.Trim().Trim(new char[] { '\0' });
                bool maskHasSign = (EditMask.Contains('-') || EditMask.Contains('+'));

                int i = editMask.Length - 1;
                int j = i;

                if (FieldType == Common.FieldType.String && (!editMask.Contains("9") && !editMask.Contains(DecimalSeparatorChar.ToString())))    // alphanumeric mask
                {
                    if (editMask.Length > value.Length)
                        value = value.PadLeft(editMask.Length - value.Length, ' ');

                    for (; i >= 0; i--)
                    {
                        switch (editMask[i])
                        {
                            case 'X':
                                sb.Insert(0, value[j]);
                                j--;
                                break;
                            case 'B':
                                sb.Insert(0, ' ');
                                break;
                            default:
                                sb.Insert(0, editMask[i]);
                                break;
                        }
                    }
                }
                else // numeric mask
                {
                    // format the edit mask for cases such as 9(3).9(2) - issue 6087
                    if (editMask.Contains("9(") || editMask.Contains("Z("))
                    {
                        string maskValue = string.Empty;
                        string newMask = string.Empty;
                        string maskID = string.Empty;
                        int maskCount, mValue, mMask;
                        bool endFound = false;
                        for (maskCount = 0; maskCount < editMask.Length; maskCount++)
                        {
                            if (endFound && editMask.Substring(maskCount, 1) == DecimalSeparatorChar.ToString())
                                endFound = false;

                            if (endFound == false)
                            {
                                if (editMask.Substring(maskCount, 1) == "9" || editMask.Substring(maskCount, 1) == "Z")
                                {
                                    maskID = editMask.Substring(maskCount, 1);
                                    if (maskCount < editMask.Length - 1)
                                    {
                                        if (editMask.Substring(maskCount + 1, 1) == "(")
                                        {
                                            maskValue = string.Empty;
                                            for (maskCount = maskCount + 2; endFound == false; maskCount++)
                                            {
                                                if (editMask.Substring(maskCount, 1) == ")")
                                                {
                                                    mValue = Convert.ToInt32(maskValue);
                                                    for (mMask = 1; mMask <= mValue; mMask++)
                                                    {
                                                        newMask += maskID;
                                                    }
                                                    endFound = true;
                                                    maskCount--;
                                                }
                                                else
                                                    maskValue += editMask.Substring(maskCount, 1);
                                            }
                                        }
                                        else
                                        {
                                            newMask += editMask.Substring(maskCount, 1);
                                        }
                                    }
                                    else
                                    {
                                        newMask += editMask.Substring(maskCount, 1);
                                    }
                                }
                                else
                                {
                                    newMask += editMask.Substring(maskCount, 1);
                                }
                            }
                            else
                            {
                                newMask += editMask.Substring(maskCount, 1);
                            }
                        }
                        if (!(newMask == string.Empty))
                        {
                            editMask = newMask;
                            i = editMask.Length - 1;
                            j = i;
                        }
                    }

                    int index = editMask.LastIndexOf(DecimalSeparatorChar);
                    if (index == -1 && editMask.ToUpper().Contains('V'))
                        index = editMask.ToUpper().LastIndexOf('V');

                    int maskPrecision = index >= 0
                        ? editMask.Substring(index + 1).Replace("CR", "").Replace("D", "").Replace("B", "").Replace("+", "").Replace("-", "").Length
                        : 0;

                    index = value.LastIndexOf(DecimalSeparatorChar);

                    int valuePrecision = index > 0 ? value.Length - index - 1 : 0;

                    if (valuePrecision == 0 && value.StartsWith(DecimalSeparatorChar.ToString()))
                    {
                        valuePrecision = value.Length - 1;
                    }

                    if (maskPrecision > valuePrecision)
                    {
                        value = value + new string('0', maskPrecision - valuePrecision);
                    }
                    else if (maskPrecision < valuePrecision)
                    {
                        if (maskPrecision == 0)
                            value = value.Substring(0, index);
                        else
                            value = BufferServices.Directives.FieldValueMoves == FieldValueMoveType.AdsoMoves    //ADSO rounds while COBOL truncates
                                   ? Decimal.Round(decimal.Parse(value), maskPrecision).ToString()
                                   : value.Substring(0, index) + value.Substring(index, maskPrecision + 1);

                        // Issue 5010 we are taking one extra character
                        //value = value.Substring(0, index + valuePrecision - maskPrecision - 1);
                    }
                    value = value.Replace(".", "").Replace(",", "");

                    if (!maskHasSign && (value.Contains('-') || value.Contains('+')))
                    {
                        value = value.Replace('-', ' ').Replace('+', ' ');
                    }

                    // remove leading zeros
                    if (value.StartsWith("-") || value.StartsWith("+") || value.StartsWith("$"))
                    {
                        string tempValue = value.Substring(1).TrimStart('0');
                        if (tempValue.Length > 0 && tempValue.Length < maskPrecision)
                            tempValue = tempValue.PadLeft(maskPrecision, '0');
                        value = value.Substring(0, 1) + tempValue;
                    }
                    else
                        value = value.TrimStart('0');
                    //Reinsert zeros before decimal point
                    if (value.Length > 0 && value.Length < maskPrecision)
                        value = value.PadLeft(maskPrecision, '0');

                    // Bug 4567 value going through as '  ,  ' edit mask ZZ.ZZ, suppress '.' being added
                    bool zeroSuppressed = (value.Trim().Length == 0 && !editMask.Contains("9")); // && (index > 0)); 

                    // make sure that value string is long enough and does not start with zeros
                    if (editMask.Length > value.Length)
                        value = value.PadLeft(editMask.Length, ' ');

                    int allSigns = 0; // 0 - not checked, 1 - false, 2 - true
                    bool allSignsCompleted = false;
                    bool allDolSignsCompleted = false;

                    for (; i >= 0; i--)
                    {
                        switch (editMask[i])
                        {
                            case '9':
                                if (value[j] == ' ' || value[j] == '-' || value[j] == '+')
                                {
                                    sb.Insert(0, '0');
                                    if (value[j] == '-' || value[j] == '+' || value[j] == '$')
                                    {
                                        allSigns = 1;
                                    }
                                }
                                else
                                    sb.Insert(0, value[j]);
                                j--;
                                // removed because of ticket 5352, June 29, 2015
                                //zeroSuppressed = false; 
                                break;
                            case 'V':
                                //Ignore Implied Decimal
                                if (value[j] == '.')
                                    j--;
                                break;
                            case 'Z':
                                if (i == 0)
                                    sb.Insert(0, value[j] == '0' ? ' ' : value[j]);
                                else
                                    sb.Insert(0, value[j] == ' ' ? ' ' : value[j]);
                                j--;
                                // removed because of ticket 5352, June 29, 2015
                                //zeroSuppressed = true;
                                break;
                            case '.':
                                if (replaceDecimalSeparator)
                                {
                                    if (zeroSuppressed)
                                        sb.Insert(0, ' ');
                                    else
                                        sb.Insert(0, System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
                                }
                                else
                                {
                                    if (zeroSuppressed)
                                        sb.Insert(0, ' ');
                                    else
                                        sb.Insert(0, '.');
                                }
                                break;
                            case ',':
                                if (i < editMask.Length - 1 && (editMask[i - 1] == 'Z' || editMask[i - 1] == '-' || editMask[i - 1] == '+' || editMask[i - 1] == '$')) //suppression
                                {
                                    if (value[j] != ' ' && value[j] != '-' && value[j] != '+' && value[j] != '$')
                                    {
                                        sb.Insert(0, editMask[i]);
                                        //sb.Insert(0, value[j]);
                                    }
                                }
                                else
                                    sb.Insert(0, editMask[i]);
                                //    sb.Insert(0, value[j]);
                                //j--;
                                break;
                            case '-':
                                if (allSigns == 2) //  
                                {
                                    if (value[j] == ' ')
                                    {
                                        if (!allSignsCompleted)
                                        {
                                            sb.Insert(0, isPositive ? ' ' : '-');
                                            allSignsCompleted = true;
                                        }
                                        else
                                            sb.Insert(0, ' ');
                                    }
                                    else
                                    {
                                        sb.Insert(0, value[j]);
                                        if (value[j] == '-')
                                            allSignsCompleted = true;
                                    }
                                    j--;
                                }
                                else if (allSigns == 1)
                                {
                                    if (!allSignsCompleted)
                                    {
                                        if (j > 0)
                                        {
                                            string tempValue = value.Substring(0, value.Length - (value.Length - j) + 1);
                                            if (tempValue.Contains("1") || tempValue.Contains("2") || tempValue.Contains("3") || tempValue.Contains("4") || tempValue.Contains("5") || tempValue.Contains("6") || tempValue.Contains("7") || tempValue.Contains("8") || tempValue.Contains("9"))
                                            {
                                                sb.Insert(0, value[j]);
                                                j--;
                                            }
                                            else
                                            {
                                                if (!isPositive)
                                                {
                                                    sb.Insert(0, '-');
                                                    allSignsCompleted = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            sb.Insert(0, isPositive ? ' ' : '-');
                                            allSignsCompleted = true;
                                        }
                                    }
                                }
                                else // has not been checked yet
                                {
                                    if (editMask.EndsWith("-"))
                                    {
                                        sb.Insert(0, isPositive ? ' ' : '-');
                                        if (!editMask.StartsWith("$"))
                                            allSigns = 1;
                                        allSignsCompleted = true;
                                    }
                                    else
                                    {
                                        allSigns = (editMask.Substring(0, 1).Replace(",", "").Replace("-", "").Replace(" ", "").Length == 0)
                                            ? 2
                                            : 1;
                                        if (value[j] != ' ')
                                        {
                                            sb.Insert(0, value[j]);
                                            if (value[j] == '-')
                                                allSignsCompleted = true;
                                        }
                                        j--;
                                    }
                                }
                                break;
                            case '+':
                                if (allSigns == 2) // all pluses
                                {
                                    if (value[j] == ' ')
                                    {
                                        if (!allSignsCompleted)
                                        {
                                            sb.Insert(0, isPositive ? '+' : '-');
                                            allSignsCompleted = true;
                                        }
                                        else
                                            sb.Insert(0, ' ');
                                    }
                                    else
                                    {
                                        sb.Insert(0, value[j]);
                                        if (value[j] == '+' || value[j] == '-')
                                            allSignsCompleted = true;
                                    }
                                    j--;
                                }
                                else if (allSigns == 1) // not all pluses
                                {
                                    if (!allSignsCompleted)
                                    {
                                        sb.Insert(0, isPositive ? '+' : '-');
                                        allSignsCompleted = true;
                                    }
                                }
                                else // has not been checked yet
                                {
                                    allSigns = (editMask.Substring(0, 1).Replace(",", "").Replace("+", "").Replace(" ", "").Length == 0)
                                        ? 2
                                        : 1;
                                    if (value[j] != ' ')
                                    {
                                        sb.Insert(0, value[j]);
                                        if (value[j] == '+' || value[j] == '-')
                                            allSignsCompleted = true;
                                    }
                                    else if (value[j] == ' ' && i == 0)
                                    {
                                        if (!allSignsCompleted)
                                        {
                                            sb.Insert(0, isPositive ? '+' : '-');
                                            allSignsCompleted = true;
                                        }
                                        else
                                            sb.Insert(0, ' ');
                                    }
                                    j--;
                                }
                                break;
                            case '$':
                                if (allSigns == 2) // all $
                                {
                                    if (value[j] == ' ' || value[j] == '-' || value[j] == '+')
                                    {
                                        if (!allDolSignsCompleted)
                                        {
                                            sb.Insert(0, '$');
                                            allDolSignsCompleted = true;
                                        }
                                        else
                                            sb.Insert(0, ' ');
                                    }
                                    else
                                    {
                                        sb.Insert(0, value[j]);
                                        if (value[j] == '$')
                                            allDolSignsCompleted = true;
                                    }
                                    j--;
                                }
                                else if (allSigns == 1) // not all 
                                {
                                    if (!allDolSignsCompleted)
                                    {
                                        sb.Insert(0, '$');
                                        allDolSignsCompleted = true;
                                    }
                                }
                                else // has not been checked yet
                                {
                                    allSigns = (editMask.Substring(0, 1).Replace(",", "").Replace("$", "").Replace(" ", "").Replace("-", "").Replace("+", "").Length == 0)
                                        ? 2
                                        : 1;
                                    if (value[j] != ' ')
                                    {
                                        sb.Insert(0, value[j]);
                                        if (value[j] == '$')
                                            allDolSignsCompleted = true;
                                    }
                                    j--;
                                }
                                break;
                            case 'B':
                                if (i > 0 && editMask[i - 1] == 'D')
                                {
                                    sb.Insert(0, isPositive ? "  " : "DB");
                                    i--;
                                }
                                else if (i > 0 && (value[j] == '/' || value[j] == ':'))
                                {
                                    sb.Insert(0, value[j]);
                                    j--;
                                }
                                else
                                    sb.Insert(0, ' ');
                                break;
                            case 'R':
                                if (i > 0 && editMask[i - 1] == 'C')
                                {
                                    sb.Insert(0, isPositive ? "  " : "CR");
                                    i--;
                                }
                                break;
                            case 'X':
                                sb.Insert(0, value[j] == ' ' ? '0' : value[j]);
                                j--;
                                break;
                            case '*':
                                sb.Insert(0, value[j] == ' ' ? '*' : value[j]);
                                j--;
                                break;
                            default:
                                sb.Insert(0, editMask[i]);
                                break;
                        }
                    }
                }
            }

            string retString = sb.ToString();
            int firstPos = retString.IndexOf("-");
            int lastPos = retString.LastIndexOf("-");
            if (firstPos != lastPos && (firstPos != -1 && lastPos != -1))
            {
                if (firstPos == 0)
                    retString = retString.Substring(1);
                else
                    retString = retString.Substring(0, firstPos - 1) + " " + retString.Substring(firstPos + 1);
            }
            return retString;
        }

        #endregion

        #region internal
        /// <summary>
        /// Adds the given <paramref name="checkfield"/> to the internal collection of check fields. 
        /// Note: this method is not visible to consumers of IField; it's only used by FieldBase itself during duplication.
        /// </summary>
        /// <param name="checkfield"></param>
        internal void AddCheckField(ICheckField checkfield)
        {
            checkFields.Add(checkfield.Name, checkfield);
        }

        /// <summary>
        /// Gets or sets the accessor object that accesses this field object as an array element IF this object 
        /// is part of an array. 
        /// </summary>
        public IArrayElementAccessor<IField> ArrayElementAccessor { get; set; }

        /// <summary>
        /// Stores the value initially given this field object against future calls to ResetToInitialValue().
        /// </summary>
        public object InitialValue { get; set; }
        #endregion

        #region abstract
        /// <summary>
        /// Assings the value of the current Field object to the provided Field object.
        /// </summary>
        /// <param name="duplicateField">A reference to the Field object for the assignment.</param>
        protected abstract void InternalDuplicate(FieldBase duplicateField);

        /// <summary>
        /// Creates a new instance of the Field object and initializes it with the data from the current object.
        /// </summary>
        /// <param name="newName">The name of the duplicated Field object.</param>
        /// <returns>Returns a reference to the newly created Field object</returns>
        protected abstract FieldBase GetDuplicateObject(string newName);

        /// <summary>
        /// Causes the object to restore its value to its original data.
        /// </summary>
        public abstract void ResetToInitialValue();

        /// <summary>
        /// Causes the object to restore its default value or low values
        /// </summary>
        public abstract void InitializeWithLowValues();
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the FieldBase class.
        /// </summary>
        public FieldBase()
        {
            checkFields = new Dictionary<string, ICheckField>();
        }
        #endregion

        #region protected properties
        /// <summary>
        /// Gets the byte array associated with this field value.
        /// </summary>
        protected byte[] Bytes
        {
            get
            {
                if (lastRead != Buffer.WriteCount)
                {
                    ReloadBytes();
                }
                var result = new byte[bytes.Length];
                System.Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
                return result;
            }
        }

        public void ReloadBytes()
        {
            bytes = Buffer.ReadBytes(PositionInBuffer, LengthInBuffer);
            lastRead = Buffer.WriteCount;

            enqoutedValueStringClean = false;
            unenqoutedValueStringClean = false;
        }

        byte[] bytes;
        /// <summary>
        /// Assigned length of this field object.
        /// </summary>
        protected int lengthInBuffer;
        #endregion

        #region Debug properties
#if DEBUG
        /// <summary>
        /// DEBUG-ONLY PROPERTY. Returns this field's value as a string representation.
        /// </summary>
        [Category("DEBUG")]
        [Description("DEBUG ONLY - Gives r/w access to internal value as a string.")]
        public string ValueAsString
        {
            get { return GetValueString(true); }
            set
            {
                AssignFrom(value);
            }
        }


        /// <summary>
        /// DEBUG-ONLY PROPERTY. Returns this field's buffer bytes as a hex string representation.
        /// </summary>
        [Category("DEBUG")]
        [Description("DEBUG ONLY - returns a string representation of the value byte array in Hex")]
        public string ValueHexString
        {
            get { return BitConverter.ToString(Bytes); }
        }



#endif


        #endregion

        #region public properties
        /// <summary>
        /// Returns a copy of the value of this object as a byte array. 
        /// </summary>
        /// <returns>A new byte[].</returns>
        [Category("IBufferValue")]
        [Description("Gets the value of this field as bytes.")]
        [ReadOnly(true)]
        public byte[] AsBytes
        {
            get { return Bytes; }
        }

        /// <summary>
        /// Returns the string representation of this object's internal byte value, 
        /// excluding bytes referenced by IRedefinition objects.
        /// </summary>
        [Category("IBufferValue")]
        [Description("The string representation of this object's internal byte array value.")]
        [ReadOnly(true)]
        public string BytesAsString
        {
            get { return Bytes.Select(b => (AsciiChar)b).NewString(); }
        }


        /// <summary>
        /// Since Checkfields are created by their owner fields and not by StructureDefinitionCompositor
        /// (which normally handles the define-time array element accessors), field initializer
        /// needs to have a reference to the root record's define-time accessor list so the field can 
        /// add accessors for any check fields. Messy, I know.
        /// </summary>
        public IDictionary<string, IArrayElementAccessorBase> DefineTimeAccessors { get; set; }

        /// <summary>
        /// Returns the string representation of this object's internal byte value, 
        /// including bytes reference by IRedefinition objects.
        /// </summary>
        [Category("IBufferValue")]
        [Description("The string representation of this object's internal byte array value.")]
        [ReadOnly(true)]
        public string RedefinedBytesAsString
        {
            get
            {
                // for a field, there's no difference - we always return redef bytes.
                return BytesAsString;
            }
        }


        /// <summary>
        /// Gets any check fields associated with this field.
        /// </summary>
        [Category("IField")]
        [Description("The collection of check fields associated with this field.")]
        [ReadOnly(true)]
        public IEnumerable<ICheckField> CheckFields
        {
            get { return checkFields.Values; }
        }

        /// <summary>
        /// Gets a field comparison (IComparer(of IField)) object.
        /// </summary>
        public static IFieldComparer Comparer
        {
            get
            {
                // Create on demand...
                if (comparer == null)
                    comparer = new FieldComparer();
                return comparer;
            }
        }

        /// <summary>
        /// Gets the number of digits to the right of the decimal, if this is a numeric field.
        /// </summary>
        [Category("IField")]
        [Description("Number of digits to the right of the decimal, if this is a numeric field.")]
        [ReadOnly(true)]
        public int DecimalDigits { get; set; }

        [Category("IField")]
        [Description("Database Column type associated with this Field")]
        public DBColumnType DBColumnType { get; set; }

        /// <summary>
        /// Gets the number of bytes for display of the field value.
        /// </summary>
        [Category("IField")]
        [Description("The number of bytes required to display the field value.")]
        [ReadOnly(true)]
        public int DisplayLength { get; set; }

        /// <summary>
        /// Edit mask text.
        /// </summary>
        [Category("IField")]
        [Description("Edit mask text")]
        [ReadOnly(true)]
        public string EditMask { get; set; }

        /// <summary>
        /// Sets and returns the flag that indicates whether the field has a blank or zero value.
        /// </summary>
        [Category("IField")]
        [Description("IsBlankWhenZero")]
        [ReadOnly(true)]
        public bool IsBlankWhenZero { get; set; }

        /// <summary>
        /// Gets this field's value as a display-appropriate string; i.e. compressed numeric values are shown 
        /// as string representations of their number value rather than their bytes. Likewise, booleans are displayed 
        /// as "true" or "false".
        /// </summary>
        [Category("IField")]
        [Description("The display-appropritae string representation of this object's value.")]
        [ReadOnly(true)]
        public string DisplayValue
        {
            get { return GetValueString(false); }
        }

        /// <summary>
        /// Gets the type of data accessed by this field object.
        /// </summary>
        [Category("IField")]
        [Description("Type of the data stored by this field.")]
        [ReadOnly(true)]
        public FieldType FieldType
        {
            get => _fieldType;
            set
            {
                _fieldType = value;
                IsNumericType = GetIsNumericType(_fieldType);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the internal field value is null or contains an array of zero-length.
        /// </summary>
        [Category("IField")]
        [Description("Indicates whether the field contains a value or is null.")]
        [ReadOnly(true)]
        public bool IsNull
        {
            get { return (Bytes == null || Bytes.Length == 0); }
        }

        /// <summary>
        /// Gets whether this field object has been declared with a numeric FieldType.
        /// </summary>
        [Category("IField")]
        [Description("Gets whether this field object has been declared with a numeric FieldType")]
        [ReadOnly(true)]
        public bool IsNumericType { get; private set; }



        /// <summary>
        /// Gets whether this field object currently contains a value that is numeric, regardless of FieldType. 
        /// </summary>
        [Category("IField")]
        [Description("Gets whether this field object currently contains a value that is numeric, regardless of FieldType.")]
        [ReadOnly(true)]
        public bool IsNumericOnlyValue
        {
            get
            {
                Decimal temp;
                return Decimal.TryParse(DisplayValue, out temp);
            }
        }
        /// <summary>
        /// Gets the serializer object responsible for serializing the value.
        /// </summary>
        [Category("IField")]
        [Description("An IFieldValueSerializer object responsible for serializing/deserializing this object's value.")]
        public IFieldValueSerializer Serializer
        {
            get
            {
                // Create on demand...
                if (serializer == null)
                {
                    serializer = new FieldValueSerializer();
                }
                return serializer;
            }
            set
            {
                serializer = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Field Justification 
        /// </summary>
        [Category("IField")]
        [Description("A FieldFormat enum showing justification. ")]
        public MDSY.Framework.Buffer.Common.FieldFormat FieldJustification { get; set; }
        #endregion

        #region IAssignable
        /// <summary>
        /// Assigns the given value to the object.
        /// </summary>
        /// <param name="value">The new value to assign to the object.</param>
        public void Assign(object value)
        {
            if (value is IField)
            {
                AssignFrom(value as IField);
            }
            else if (value is byte[])
            {
                AssignFrom(value as byte[]);
            }
            else if (value is string)
            {
                AssignFrom(value as string);
            }
            else if (value is Boolean)
            {
                if ((bool)value)
                    AssignFrom(1);
                else
                    AssignFrom(0);
            }
            else
            {
                if (this.FieldType == Common.FieldType.NumericEdited && value != null)
                {
                    // value must be numeric and any numeric type can be coverted to decimal type
                    AssignFrom(Convert.ToDecimal(value));
                }
                else
                {
                    IEnumerable<byte> bytes = null;

                    if (IsBlankWhenZero && Convert.ToDecimal(value) == 0)
                    {
                        bytes = System.Text.Encoding.ASCII.GetBytes(new string(' ', LengthInBuffer));
                    }
                    else
                    {
                        // on rare occasion PackedDecimals.Bytes.Length can have more bytes than this field's LengthInBuffer. 
                        // in that situation, trim the serialized bytes from the left. 
                        bytes = Serializer.Serialize(value, LengthInBuffer, this.FieldType, FieldType.String, DecimalDigits);
                        if (bytes.Count() > this.LengthInBuffer)
                        {
                            bytes = bytes.Skip(bytes.Count() - this.LengthInBuffer);
                        }
                    }

                    WriteBytes(bytes.ToArray());
                }
            }
        }

        private string _recordName = "";
        private FieldType _fieldType;
        private long lastRead = -1;


        /// <summary>
        /// Associates current field with the record it belongs to if the current field is of integer type and is a structure element of the record. 
        /// Does nothing otherwise.
        /// </summary>
        /// <param name="value">The name of the record the field belongs to.</param>
        public void AssignIdRecordName(string value)
        {
            _recordName = value;
        }

        /// <summary>
        /// Associates current field with the record it belongs to if the current field is of integer type and is a structure element of the record. 
        /// Does nothing otherwise.
        /// </summary>
        /// <param name="value">A reference to the buffer value object, which contains the name of the record the field belongs to.</param>
        public void AssignIdRecordName(IBufferValue value)
        {
            // does nothing
        }

        /// <summary>
        /// Returns the name of the associated record if current object belongs to the record as a field of integer type. Returns null otherwise.
        /// </summary>
        /// <returns>Returns the name of the associated record if current object belongs to the record as a field of integer type. Returns null otherwise.</returns>
        public string GetIdRecordName()
        {
            return _recordName;
        }

        /// <summary>
        /// Assigns the given value to the object, as appropriate.
        /// </summary>
        /// <param name="value">The value to be assigned.</param>
        public void AssignFrom(string value)
        {
            string workValue = value;
            byte[] bytes = null;
            bool isNumericTypeDone = false;
            if (this.FieldType == FieldType.String && this.FieldJustification == FieldFormat.JustifyRight)
            #region Target Field has FieldJustification
            {
                value = value.TrimEnd().PadLeft(this.LengthInBuffer);
            }
            #endregion
            // - RKL 12/23/2014: this method was altered in a way that was incorrect. It changed all 
            // incoming null or empty strings to "0"; which would not be correct behavior for 
            // string-type fields. I'm changing this to only update the value to "0" if the 
            // field type is numeric. If this causes problems, please see Robert L. 

            // clean up incoming value.
            if (string.IsNullOrWhiteSpace(workValue) && this.IsNumericType)
            {
                workValue = "0";
            }

            if (IsBlankWhenZero && workValue == "0")
            {
                bytes = System.Text.Encoding.ASCII.GetBytes(new string(' ', LengthInBuffer));
                WriteBytes(bytes);
            }
            else
            {
                if (this.FieldType == Common.FieldType.NumericEdited)
                {
                    decimal dtmp = 0;
                    if (value.Contains('.'))
                    {
                        value = value.Replace('.', DecimalSeparatorChar);
                    }
                    decimal.TryParse(value, out dtmp);
                    if (!string.IsNullOrEmpty(EditMask) && !string.IsNullOrEmpty(workValue.Trim()))
                    {
                        if (dtmp == 0 && value.Contains("$"))
                            decimal.TryParse(value.Trim().Replace("$", ""), out dtmp);

                        workValue = ApplyEditMask(dtmp.ToString(), dtmp >= 0, false);
                        if (workValue.TrimStart().StartsWith("."))
                        {
                            if (EditMask.IndexOf('.') > 0)
                            {
                                if (EditMask.Substring((EditMask.IndexOf('.') - 1), 1) != "Z")
                                {
                                    if (!(EditMask.StartsWith("Z(")))
                                        workValue = workValue.Replace(" .", "0.");
                                }
                            }
                        }
                    }

                    if (DisplayLength > workValue.Length)
                        workValue = workValue.PadLeft(DisplayLength, ' ');

                    bytes = Serializer.Serialize(workValue, LengthInBuffer, FieldType.String, FieldType.String, DecimalDigits);
                    WriteBytes(bytes);
                }
                else
                {
                    // Read string values from the right for numeric values - Issues 8306, 8341, 8264, 8171, 8174, 8157
                    if (this.IsNumericType)
                    {
                        isNumericTypeDone = true;
                        int numericLength = this.FieldType == FieldType.PackedDecimal || this.FieldType == FieldType.UnsignedPackedDecimal
                            ? this.LengthInBuffer * 2 - 1
                            : this.DisplayLength;               //Issue 9393  - chabnged default to display length

                        var hasDecimal = value.Contains(DecimalSeparatorChar);
                        if (value.Length > numericLength)
                        {

                            var hasPositiveSign = value.Contains('+');
                            var hasNegativeSign = value.Contains('-');

                            value = value.Replace("+", "").Replace("-", "").TrimStart('0');

                            var substringLength = numericLength;
                            if (hasDecimal)
                                substringLength++;
                            if (value.Length < substringLength)
                                substringLength = value.Length;
                            workValue = value.Substring(0, substringLength);

                            if (hasPositiveSign)
                                workValue = "+" + workValue;

                            if (hasNegativeSign)
                                workValue = "-" + workValue;

                        }

                        if (hasDecimal)
                        {
                            var parts = workValue.Split(DecimalSeparatorChar);
                            if (parts[1].Length > DecimalDigits)
                                workValue = parts[0] + DecimalSeparatorChar + parts[1].Substring(0, DecimalDigits);
                        }

                        if ((workValue.Contains("+") || workValue.Contains("-")) && (this.FieldType == FieldType.UnsignedNumeric))
                        {
                            workValue = workValue.Replace("+", "").Replace("-", "");
                        }

                        FieldType serializeType = workValue.ContainsNumericValue() ? this.FieldType : FieldType.String;
                        bytes = Serializer.Serialize(workValue, LengthInBuffer, serializeType, FieldType.String, DecimalDigits);
                        WriteBytes(bytes);
                    }
                }
            }

            if (!isNumericTypeDone)
            {
                if (this.FieldType == Common.FieldType.String)
                {
                    if (value.Contains('.'))
                    {
                        value = value.Replace('.', DecimalSeparatorChar);
                    }

                    if (value.ContainsNumericValue())
                    {
                        decimal ntmp = 0;

                        decimal.TryParse(value, out ntmp);
                        if (!string.IsNullOrEmpty(EditMask) && !string.IsNullOrEmpty(workValue.Trim()))
                        {
                            workValue = ApplyEditMask(ntmp.ToString(), ntmp >= 0, false);
                            if (workValue.TrimStart().StartsWith("."))
                            {
                                if (EditMask.IndexOf('.') > 0)
                                {
                                    if (EditMask.Substring((EditMask.IndexOf('.') - 1), 1) != "Z")
                                    {
                                        if (!(EditMask.StartsWith("Z(")))
                                            workValue = workValue.Replace(" .", "0.");
                                    }
                                }
                            }
                        }

                        if (DisplayLength > workValue.Length)
                            if (BufferServices.Directives.FieldValueMoves == FieldValueMoveType.CobolMoves && this.FieldJustification != FieldFormat.JustifyRight)   //Following change made on 2018-09-18
                                workValue = workValue.PadRight(DisplayLength, ' ');  // COBOL move of numeric to string should left justify
                            else
                                workValue = workValue.PadLeft(DisplayLength, ' ');   // ADSO move of numeric to string should right justify

                        bytes = Serializer.Serialize(workValue, LengthInBuffer, FieldType.String, FieldType.String, DecimalDigits);
                        WriteBytes(bytes);
                    }
                    else
                    {
                        FieldType serializeType = workValue.ContainsNumericValue() ? this.FieldType : FieldType.String;
                        bytes = Serializer.Serialize(workValue, LengthInBuffer, serializeType, FieldType.String, DecimalDigits);
                        WriteBytes(bytes);
                    }
                }
                else if (this.FieldType == Common.FieldType.Binary)
                {
                    //Binary Field Update
                    WriteBytes(value.AsUTF8Bytes());
                }
            }
        }

        /// <summary>
        /// Assigns the given value to the object, as appropriate.
        /// </summary>
        /// <param name="value">The value to be assigned.</param>
        public void AssignFrom(decimal value)
        {
            byte[] bytes = null;

            if (IsBlankWhenZero && value == 0)
            {
                bytes = System.Text.Encoding.ASCII.GetBytes(new string(' ', LengthInBuffer));
            }
            else
            {
                if (FieldType == Common.FieldType.NumericEdited)
                {
                    string strValue = value.ToString();
                    strValue = ApplyEditMask(strValue, value >= 0, false);

                    if (DisplayLength > strValue.Length)
                        strValue = strValue.PadLeft(DisplayLength, ' ');

                    // ticket 9090
                    //bytes = Serializer.Serialize(strValue, LengthInBuffer, FieldType.String, FieldType.SignedDecimal, DecimalDigits);
                    bytes = Serializer.Serialize(strValue, LengthInBuffer, FieldType.String, FieldType.String, DecimalDigits);
                }
                else
                    bytes = Serializer.Serialize(value, LengthInBuffer, FieldType, FieldType.SignedDecimal, DecimalDigits);
            }

            WriteBytes(bytes);
        }

        /// <summary>
        /// Assigns the value of the given <paramref name="element"/> to this object, as appropriate.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
        /// <param name="element">A reference to the buffer value object to be assigned.</param>
        public void AssignFrom(IBufferValue element)
        {
            if (element == null)
                throw new ArgumentNullException("element", "element is null.");

            if (this.FieldType == Common.FieldType.NumericEdited)
                AssignFrom(element.DisplayValue);
            else
                AssignFrom(element.AsBytes);
        }

        /// <summary>
        /// Assigns provided value of the specified field type to the current field object.
        /// </summary>
        /// <param name="element">A reference to the buffer value to be assigned.</param>
        /// <param name="sourceFieldType">Specifies the type of the buffer value object to be assigned.</param>
        public void AssignFrom(IBufferValue element, FieldType sourceFieldType)
        {

            if (this.IsNumericType && sourceFieldType != FieldType.String && sourceFieldType != FieldType.NumericEdited && sourceFieldType != FieldType.Binary)
            {
                #region Numeric Types assigned from source element as decimal
                if (element is IField)
                {
                    IField field = (IField)element;
                    if (field.IsNumericValue())
                        this.AssignFrom(field.AsDecimal());
                }
                else if (element is IGroup)
                {
                    IGroup group = (IGroup)element;
                    if (group.IsNumericValue())
                        this.AssignFrom(group.AsDecimal());
                }
                return;
                #endregion
            }

            #region Binary type assign bytes
            if (sourceFieldType == FieldType.Binary || this.FieldType == FieldType.Binary)
            {
                AssignFrom(element.AsBytes);
                return;
            }
            #endregion

            if (this.FieldType == Common.FieldType.NumericEdited && sourceFieldType != FieldType.FloatDouble) //Issue 8414
            #region AssignNumericEdited
            {
                if (sourceFieldType == FieldType.SignedNumeric)
                {
                    byte[] bytes = element.AsBytes;
                    if (bytes[bytes.Length - 1] == 0x7B)
                    {
                        bytes[bytes.Length - 1] = 0x30;
                        int start = 0;
                        string newDisplayValue = string.Empty;
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            if (bytes[i] != 0x30)
                            {
                                start = i;
                                break;
                            }
                        }
                        if (start > 0)
                        {
                            for (int j = start; j < bytes.Length; j++)
                            {
                                newDisplayValue += (Char)bytes[j];
                            }
                        }
                        else
                        {
                            newDisplayValue = "0";
                        }

                        int dec = (((MDSY.Framework.Buffer.Implementation.FieldBase)(element)).DecimalDigits);
                        if (dec > 0)
                        {
                            if (newDisplayValue.Length <= dec)
                            {
                                for (int i = 1; i <= dec; i++)
                                {
                                    newDisplayValue = newDisplayValue.Insert(0, "0");
                                }
                            }
                            newDisplayValue = newDisplayValue.Insert(newDisplayValue.Length - dec, DecimalSeparatorChar.ToString());
                        }
                        AssignFrom(newDisplayValue);
                    }
                    else
                    {
                        AssignFrom(element.DisplayValue);
                    }
                }
                else
                {
                    AssignFrom(element.DisplayValue);
                }
            }
            #endregion
            else if ((sourceFieldType == FieldType.PackedDecimal) || (sourceFieldType == FieldType.UnsignedPackedDecimal))
            #region AssignFromPackedDecimal
            {
                if (BufferServices.Directives.FieldValueMoves == FieldValueMoveType.AdsoMoves || IsNumericType)
                    AssignFrom(Convert.ToDecimal(element.DisplayValue));
                else
                //Move PackedDecimal to string for Cobol Move type - see issue 8926
                {
                    string numericStringValue = element.DisplayValue.TrimStart().Replace("-", "").Replace(".", "").Replace(",", "").PadLeft(((IField)element).DisplayLength, '0');

                    AssignFrom(numericStringValue);
                }
            }
            #endregion
            else if ((sourceFieldType == FieldType.CompInt) || (sourceFieldType == FieldType.CompShort) || (sourceFieldType == FieldType.CompLong) || (sourceFieldType == FieldType.ReferencePointer))
            #region AssignFromComp
            {                                                 // Issue 9419
                string compValue = element.DisplayValue;
                if (((IField)element).DisplayLength < compValue.Length && sourceFieldType != FieldType)
                {
                    compValue = element.DisplayValue.Substring(compValue.Length - ((IField)element).DisplayLength);
                }
                if (sourceFieldType == FieldType.CompInt || sourceFieldType == FieldType.ReferencePointer)
                    AssignFrom(Convert.ToInt32(compValue));
                else if (sourceFieldType == FieldType.CompShort)
                    AssignFrom(Convert.ToInt16(compValue));
                else if (sourceFieldType == FieldType.CompLong)
                    AssignFrom(Convert.ToInt64(compValue));
            }
            #endregion
            else if (this.FieldType == FieldType.UnsignedNumeric && sourceFieldType == FieldType.String)
            #region  Special Rule for assigning string to Numeric field
            {
                if (IsBlankWhenZero)
                {
                    decimal testDec;
                    if (decimal.TryParse(element.BytesAsString, out testDec))
                    {
                        if (testDec == 0)
                            element.AssignFrom(System.Text.Encoding.ASCII.GetBytes(new string(' ', LengthInBuffer)));
                    }
                }
                if (LengthInBuffer < element.BytesAsString.Length)
                {
                    string nbrString = element.BytesAsString.Trim().PadLeft(LengthInBuffer, '0');
                    AssignFrom(nbrString);
                }
                else
                    AssignFrom(element.AsBytes);
            }
            #endregion
            else if (this.FieldType == FieldType.UnsignedNumeric && sourceFieldType == FieldType.NumericEdited)
            #region  AssignFromNumericEdited
            {
                decimal outd;
                int itp = 0;
                string strNumEdit = element.BytesAsString.TrimStart();
                string intString = string.Empty;
                StringBuilder sb = new StringBuilder();

                if (this.DecimalDigits > 0)
                {
                    if (decimal.TryParse(strNumEdit, out outd))
                        sb.Append(outd);
                }
                else
                {
                    for (int i = 0; i < strNumEdit.Length; i++)
                    {
                        if (int.TryParse(strNumEdit.Substring(i, 1), out itp))
                        {
                            sb.Append(strNumEdit.Substring(i, 1));
                        }
                    }
                }
                intString = sb.ToString();
                if (intString.Length == 0)
                    intString = "0";

                AssignFrom(intString);
            }
            #endregion
            else if (this.FieldType == FieldType.String && this.FieldJustification == FieldFormat.JustifyRight)
            #region Target Field has FieldJustification
            {
                string justifiedString = element.BytesAsString.TrimEnd().PadLeft(this.LengthInBuffer);
                AssignFrom(justifiedString);
            }
            #endregion
            else
            {
                if ((element is IField) && (FieldType == FieldType.PackedDecimal) || (FieldType == FieldType.UnsignedPackedDecimal))
                #region AssignToPackedDecimal
                {
                    var sourceField = element as IField;
                    string decimalString = sourceField.DisplayValue;
                    //Update to catch numeric edited values with '.' - SAAQ issue 5746
                    if (sourceFieldType == FieldType.NumericEdited && decimalString.Contains('.'))
                    {
                        decimalString = decimalString.Replace('.', DecimalSeparatorChar);
                    }
                    else
                    {
                        if (sourceField.DecimalDigits > DecimalDigits)
                        {
                            decimalString = sourceField.GetValue<decimal>().ToString(string.Format("F{0}", DecimalDigits + 1));
                            decimalString = decimalString.Substring(0, decimalString.Length - 1);
                        }
                        else
                            decimalString = sourceField.GetValue<decimal>().ToString(string.Format("F{0}", DecimalDigits));

                    }
                    AssignFrom(decimalString);
                }
                #endregion
                else
                {

                    byte[] bytes = element.AsBytes;

                    if (sourceFieldType != FieldType)
                    {
                        Object newValue = null;
                        if ((FieldType == FieldType.String && sourceFieldType != FieldType.String) &&
                            (BufferServices.Directives.FieldValueMoves != FieldValueMoveType.AdsoMoves) &&
                            (element is IField))
                        #region Assign String from non String field
                        {
                            bool checkVal = true;
                            for (int i = 0; i < bytes.Length; i++)
                            {
                                if (bytes[i] != 0)
                                    checkVal = false;
                                break;
                            }
                            if (!checkVal)
                            {
                                if (!string.IsNullOrEmpty((element as IField).EditMask))
                                    bytes = (element as IField).DisplayValue.Select(c => (byte)c).ToArray();
                                else
                                {
                                    if (Serializer.TryDeserialize(bytes, sourceFieldType, ((IField)element).DecimalDigits, out newValue))
                                    {
                                        //bytes = Serializer.Serialize(newValue.ToString().PadLeft(bytes.Length, '0'), LengthInBuffer, FieldType, DecimalDigits);
                                        bytes = Serializer.Serialize(newValue.ToString().PadLeft((element as IField).DisplayLength, '0'),
                                            ((IField)element).LengthInBuffer, FieldType, sourceFieldType, ((IField)element).DecimalDigits);
                                    }
                                }
                            }
                        }
                        #endregion
                        else
                        {
                            //Update for issue 6418  && 6504 
                            if (this.IsNumericType || this.FieldType == FieldType.NumericEdited)
                            {
                                #region AssignNUmericTypeFromStringorComporFloat
                                if (sourceFieldType == FieldType.String || sourceFieldType == FieldType.CompShort || sourceFieldType == FieldType.CompInt || sourceFieldType == FieldType.CompLong)
                                {
                                    // Update for issue 8075
                                    if (element.DisplayValue.Length > this.DisplayLength)
                                    {
                                        //Update for Issue 8487
                                        //AssignFrom(element.DisplayValue.Substring(element.DisplayValue.Length - this.DisplayLength)); return;
                                        string dispValue = element.DisplayValue.Trim();
                                        if (dispValue.Length > this.DisplayLength)
                                        {
                                            AssignFrom(dispValue.Substring(element.DisplayValue.Length - this.DisplayLength));
                                            return;
                                        }
                                        else
                                        {
                                            AssignFrom(dispValue); return;
                                        }
                                    }
                                    else
                                    {
                                        AssignFrom(element.DisplayValue); return;
                                    }
                                }
                                else if (sourceFieldType == FieldType.FloatDouble || sourceFieldType == FieldType.FloatSingle) //Added for issue 8944
                                {
                                    decimal testDec;
                                    if (decimal.TryParse(element.DisplayValue, out testDec))
                                    {
                                        AssignFrom(decimal.Round(testDec, this.DecimalDigits)); return;
                                    }
                                }
                                #endregion
                            }
                            #region Serialize value and update bytes
                            if (Serializer.TryDeserialize(bytes, sourceFieldType, ((IField)element).DecimalDigits, out newValue, FieldType == FieldType.String))
                            {
                                if (FieldType == FieldType.String || sourceFieldType == FieldType.FloatDouble)   // Issues 6479, 8414
                                {
                                    bytes = Serializer.Serialize(newValue, this.LengthInBuffer, this.FieldType, sourceFieldType, this.DecimalDigits);
                                }
                                else
                                {
                                    bytes = Serializer.Serialize(newValue, ((IField)element).LengthInBuffer, FieldType, sourceFieldType, ((IField)element).DecimalDigits);
                                }
                            }
                            #endregion
                        }
                    }

                    if (IsNumericType &&
                          sourceFieldType != FieldType.String &&
                          sourceFieldType != FieldType.Boolean &&
                          sourceFieldType != FieldType.ReferencePointer)
                    {
                        // Issue 8414 - Handle FloatDouble without interfereing with bytes positions
                        byte[] newBytes = new byte[Bytes.Length];
                        if (FieldType == FieldType.FloatDouble || sourceFieldType == FieldType.FloatDouble)
                        {
                            int k = newBytes.Length - 1;
                            int byteStop = 0;
                            if (bytes.Length > newBytes.Length)
                                byteStop = (bytes.Length - newBytes.Length);
                            for (int l = bytes.Length - 1; l >= byteStop; l--)
                            {
                                newBytes[k] = bytes[l];
                                k--;
                            }
                        }
                        else
                        {
                            int j = (bytes.Length - (element as IField).DecimalDigits) - 1;
                            // Populate and truncate left for the left side of the decimal separator according to the receiving field
                            for (int i = (Bytes.Length - DecimalDigits) - 1; i >= 0; i--)
                            {
                                if (j >= 0)
                                {
                                    //Check for space character - move zero instead 8623
                                    if (bytes[j] == 0x20 && BufferServices.Directives.FieldValueMoves != FieldValueMoveType.AdsoMoves
                                        && FieldType != FieldType.CompShort && FieldType != FieldType.CompInt && FieldType != FieldType.CompLong && FieldType != FieldType.ReferencePointer) // ticket 8744
                                        newBytes[i] = 0x30;
                                    else
                                        newBytes[i] = bytes[j];
                                }
                                else if (FieldType != FieldType.CompShort && FieldType != FieldType.CompInt && FieldType != FieldType.CompLong && FieldType != FieldType.ReferencePointer // Update to check for all comp types - issue 6504
                                     && FieldType != FieldType.FloatDouble) //FloatDouble is presented by Microsoft double format
                                {
                                    newBytes[i] = 0x30;
                                }
                                j--;
                            }
                            // Populate and truncate right side of the decimal separator according to the receiving field
                            if (DecimalDigits > 0)
                            {
                                j = (bytes.Length - (element as IField).DecimalDigits);
                                for (int i = (Bytes.Length - DecimalDigits); i < Bytes.Length; i++)
                                {
                                    if ((element as IField).DecimalDigits > 0 && j < bytes.Length)
                                    {
                                        if (bytes[j] == 0x20 && BufferServices.Directives.FieldValueMoves != FieldValueMoveType.AdsoMoves)
                                            newBytes[i] = 0x30;
                                        else
                                            newBytes[i] = bytes[j];
                                    }
                                    else if (FieldType != FieldType.CompShort)
                                    {
                                        newBytes[i] = 0x30;
                                    }
                                    j++;
                                }
                            }
                        }
                        bytes = newBytes;
                    }
                    AssignFrom(bytes);
                }
            }
        }


        /// <summary>
        /// Assigns the given <paramref name="bytes"/> to this object, as appropriate.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> is null or empty.</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> is incorrect length.</exception>
        /// <param name="bytes">The bytes to be assigned.</param>
        public void
            AssignFrom(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                //throw new ArgumentException("bytes is null or empty.", "bytes");
                return;
            // if (bytes.Length != LengthInBuffer)
            if (bytes.Length < LengthInBuffer)
            {
                List<byte> byteList = bytes.ToList();
                for (int ctr = bytes.Length; ctr < LengthInBuffer; ctr++)
                {
                    byteList.Add(0x20);
                }
                bytes = byteList.ToArray();
            }
            Buffer.WriteBytes(bytes, PositionInBuffer, LengthInBuffer);
        }

        /// <summary>
        /// Assigns the provided group object to the current field object.
        /// </summary>
        /// <param name="group">A reference to the group object to be assigned.</param>
        public void AssignFromGroup(IGroup group)
        {
            if (this.FieldJustification == FieldFormat.JustifyRight)
            {
                AssignFrom(group.AsString());
                return;
            }
            byte[] bytes = group.AsBytes;
            if (bytes.Length == 0)
                throw new ArgumentException("bytes is null or empty.", "bytes");

            if (FieldType == Common.FieldType.PackedDecimal)
            {
                decimal tmpDecimal = 0;
                if (decimal.TryParse(group.BytesAsString, out tmpDecimal))
                {
                    AssignFrom(tmpDecimal);
                    return;
                }
            }

            if (bytes.Length < LengthInBuffer)
            {
                var byteList = bytes.ToList();
                for (int ctr = 1; ctr <= (LengthInBuffer - bytes.Length); ctr++)
                {
                    byteList.Add(0x20);
                }
                bytes = byteList.ToArray();
            }

            Buffer.WriteBytes(bytes, PositionInBuffer, LengthInBuffer);
        }

        #endregion

        #region public methods


        /// <summary>
        /// Returns the field initializer object as an IField.
        /// </summary>
        public IField AsReadOnly()
        {
            return this as IField;
        }

        /// <summary>
        /// Compares the values of two specified IField objects and returns an integer that indicates their relative 
        /// position in the sort order.
        /// </summary>
        /// <param name="field1">The first field to compare.</param>
        /// <param name="field2">The second field to compare.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the ordered relationship between the two comparands. 
        /// </returns>
        public static int CompareFieldValues(IField field1, IField field2)
        {
            return Comparer.Compare(field1, field2);
        }

        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// uses the given <paramref name="check"/> expression to evaluated this field.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="check">The expression which the checkfield will use to evaluate this field.</param>
        /// <returns>The new checkfield object.</returns>
        public ICheckField CreateNewCheckField(string name, Func<IField, bool> check)
        {
            var resultInit = ObjectFactory.Factory.NewCheckFieldObject(name, check, this);
            ICheckField result = resultInit.AsReadOnly();

            checkFields.Add(result.Name, result);
            resultInit.IsInArray = IsInArray;

            if (IsInArray)
            {
                string chkName = result.Name;
                string basename;
                IEnumerable<int> idxs = ArrayElementUtils.GetElementIndexes(this.Name, out basename);
                resultInit.Name = ArrayElementUtils.MakeElementName(chkName, idxs);
                resultInit.ArrayElementIndex = 0;

                var accessor = CreateAccessor(chkName, this.DefineTimeAccessors, result);
                resultInit.ArrayElementAccessor = accessor.AsReadOnly();
            }


            if (Record != null)
            {
                Record.AddStructureElement(result);
            }
            return result;
        }

        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">A collection of values to be added to the CheckValues collection.</param>
        /// <returns>The new checkfield object.</returns>
        public ICheckField CreateNewCheckField(string name, params string[] values)
        {
            ICheckField result = CreateNewCheckField(name, (fld) => values.Contains(fld.GetValue<string>()));
            foreach (string strValue in values)
            {
                result.CheckValues.Add(strValue.PadRight(this.LengthInBuffer));
            }

            if (Record != null)
            {
                Record.AddStructureElement(result);
            }
            return result;
        }

        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">A collection of values to be added to the CheckValues collection.</param>
        /// <returns>The new checkfield object.</returns>
        public ICheckField CreateNewCheckField(string name, params Char[] values)
        {
            ICheckField result = CreateNewCheckField(name, (fld) => values.Contains(fld.GetValue<Char>()));
            foreach (Char charValue in values)
            {
                result.CheckValues.Add(charValue);
            }

            if (Record != null)
            {
                Record.AddStructureElement(result);
            }
            return result;
        }


        /// <summary>
        /// Creates and returns a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">A collection of values to be add to the CheckValues collection.</param>
        /// <returns>The new checkfield object.</returns>
        public ICheckField CreateNewCheckField(string name, params int[] values)
        {
            //SEARCHS HAS A PIC 9 WITH AN 88 LEVEL REDEFINED WITH A PIC X
            //THE X FIELD WAS THE LAST FIELD SET THEREFORE CONVERTING ints to string
            string[] stringValues = new string[values.Length];


            for (int i = 0; i < values.Length; i++)
            {
                stringValues[i] = values[i].ToString().PadLeft(Bytes.Length, '0');
            }

            ICheckField result = CreateNewCheckField(name, (fld) => stringValues.Contains(fld.GetValue<string>()));

            foreach (string strValue in stringValues)
            {
                result.CheckValues.Add(strValue);
            }
            if (Record != null)
            {
                Record.AddStructureElement(result);
            }
            return result;
        }

        /// <summary>
        /// Returns a deep copy of this element object, applying <paramref name="name"/> as the duplicate object's new 
        /// Name, and offsetting the new object's position by the amount given in <paramref name="bufferPositionOffset"/>.
        /// The new object's Parent is the same as this object's Parent.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="bufferPositionOffset">The amount by which to adjust the new object's position.</param>
        /// <param name="arrayIndexes">The indices of this element and possibly its parents if this element is part of 
        /// an array and/or nested array.</param>
        /// <returns>A new IBufferElement instance of the same type as this object.</returns>
        public IBufferElement Duplicate(string name, int bufferPositionOffset, IEnumerable<int> arrayIndexes)
        {
            return Duplicate(name, bufferPositionOffset, this.Parent, arrayIndexes);
        }

        /// <summary>
        /// Returns a deep copy of this element object, applying <paramref name="name"/> as the duplicate object's new 
        /// Name, and offsetting the new object's position by the amount given in <paramref name="bufferPositionOffset"/>.
        /// The new object is re-parented to <paramref name="newParent"/>.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="bufferPositionOffset">The amount by which to adjust the new object's position.</param>
        /// <param name="newParent">The IElementCollection which will be the new object's Parent.</param>
        /// <param name="arrayIndexes">The indices of this element and possibly its parents if this element is part of 
        /// an array and/or nested array.</param>
        /// <returns>A new IBufferElement instance of the same type as this object.</returns>
        public IBufferElement Duplicate(string name, int bufferPositionOffset, IElementCollection newParent, IEnumerable<int> arrayIndexes)
        {
            string newName;
            ArrayElementUtils.GetElementIndexes(name, out newName);

            var result = GetDuplicateObject(ArrayElementUtils.MakeElementName(newName, arrayIndexes));
            result.FieldType = FieldType;
            result.LengthInBuffer = LengthInBuffer;
            result.DisplayLength = DisplayLength;
            result.Parent = newParent;
            result.PositionInParent = PositionInParent + bufferPositionOffset;
            result.IsInArray = IsInArray;
            result.IsARedefine = IsARedefine;
            InternalDuplicate(result);
            result.DecimalDigits = DecimalDigits;
            result.ArrayElementAccessor = ArrayElementAccessor;
            result.EditMask = EditMask;
            result.InitialValue = InitialValue;
            result.DBColumnType = DBColumnType;
            foreach (ICheckField checkfld in CheckFields)
            {
                ICheckField copy = checkfld.Duplicate(checkfld.Name, 0, arrayIndexes) as ICheckField;
                copy.Field = result;
                result.AddCheckField(copy);
            }

            if (IsInArray && ArrayElementAccessor != null)
            {
                var editableAccessor = ArrayElementAccessor as IEditableArrayElementAccessor<IField>;
                if (editableAccessor != null)
                {
                    editableAccessor.AddElement(result);
                }
            }

            return result;
        }

        /// <summary>
        /// Attempts to get the value of this object converted to the given type <typeparamref name="T"/>.
        /// If the value cannot be converted, an exception is thrown.
        /// </summary>
        /// <returns>This object's value as a type <typeparamref name="T"/>.</returns>
        public T GetValue<T>()
        {
            if (lastRead != Buffer.WriteCount)
            {
                ReloadBytes();
            }

            T result;

            if ((FieldType == FieldType.PackedDecimal) || (FieldType == FieldType.UnsignedPackedDecimal))
            {
                PackedDecimal temp = PackedDecimal.FromBytes(Bytes, FieldType == FieldType.PackedDecimal, DecimalDigits);
                result = (T)Convert.ChangeType(temp, typeof(T));

            }
            else
            {
                try
                {
                    result = Serializer.Deserialize<T>(Bytes, FieldType, DecimalDigits);
                }
                catch (Exception ex)
                {
                    // if the cast failed while we're converting during a call to CompareTo()
                    // we need to let the calling code know so it can handle the exception 
                    // and just return Not Equal. 
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    for (int i = 0; i < Bytes.Length; i++)
                    {
                        if (i > 0)
                            sb.Append(",");

                        sb.Append(Bytes[i]);
                    }
                    throw new FieldValueException(string.Format("Exception while converting field {0}'s value {1} to type {2}.", Name, sb.ToString(), typeof(T)), ex);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a new check field object associated with this field object. The new check field 
        /// uses the given <paramref name="check"/> expression to evaluated this field.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="check">The expression which the checkfield will use to evaluate this field.</param>
        /// <returns>This field object.</returns>
        public IField NewCheckField(string name, Func<IField, bool> check)
        {
            CreateNewCheckField(name, check);
            return this;
        }

        /// <summary>
        /// Creates a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <returns>This field object.</returns>
        public IField NewCheckField(string name, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].PadRight(this.LengthInBuffer);
            }
            CreateNewCheckField(name, values);
            return this;
        }

        /// <summary>
        /// Creates a new check field object associated with this field object with CHAR value
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <param name="values">This field object.</param>
        /// <returns>A reference to the current field object.</returns>
        public IField NewCheckField(string name, params Char[] values)
        {
            CreateNewCheckField(name, values);
            return this;
        }

        /// <summary>
        /// Creates a new check field object associated with this field object. The new check field 
        /// evaluates this field's value for any of the given <paramref name="values"/>.
        /// </summary>
        /// <param name="name">The name of the new checkfield.</param>
        /// <returns>This field object.</returns>
        public IField NewCheckField(string name, params int[] values)
        {
            CreateNewCheckField(name, values);
            return this;
        }

        /// <summary>
        /// Creates a new check field object with a range associated with this field object. 
        /// The new check field evaluates this field's value with the given range
        /// </summary>
        /// <param name="name">The name of the new check field object.</param>
        /// <param name="loBound">Specifies the lower boundary.</param>
        /// <param name="hiBound">Specifies the upper boundary.</param>
        /// <returns>Returns a reference to the current field object.</returns>
        public IField NewCheckFieldRange(string name, string loBound, string hiBound)
        {
            CreateNewCheckFieldRange(name, loBound, hiBound);
            return this;
        }
        /// <summary>
        /// Creates a new check field object with a range associated with this field object. 
        /// The new check field evaluates this field's value with the given range
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loBound1"></param>
        /// <param name="hiBound1"></param>
        /// <param name="loBound2"></param>
        /// <param name="hiBound2"></param>
        /// <returns></returns>
        public IField NewCheckFieldRange(string name, string loBound1, string hiBound1, string loBound2, string hiBound2)
        {
            CreateNewCheckFieldRange(name, loBound1, hiBound1);
            return this;
        }

        /// <summary>
        /// Creates a new check field object with a range associated with this field object. 
        /// The new check field evaluates this field's value with the given range
        /// </summary>
        /// <param name="name">The name of the new check field object.</param>
        /// <param name="loBound">Specifies the lower boundary.</param>
        /// <param name="hiBound">Specifies the upper boundary.</param>
        /// <returns>Returns a reference to the newly created check field object.</returns>
        public ICheckField CreateNewCheckFieldRange(string name, string loBound, string hiBound)
        {
            ICheckField result = CreateNewCheckField(name, (fld) => (fld.GetValue<string>().CompareTo(loBound) >= 0) && (fld.GetValue<string>().CompareTo(hiBound) <= 0));
            return result;
        }

        public ICheckField CreateNewCheckFieldRange(string name, string loBound1, string hiBound1, string loBound2, string hiBound2)
        {
            ICheckField result = CreateNewCheckField(name, (fld) => ((fld.GetValue<string>().CompareTo(loBound1) >= 0) && (fld.GetValue<string>().CompareTo(hiBound1) >= 0)) || ((fld.GetValue<string>().CompareTo(loBound2) >= 0) && (fld.GetValue<string>().CompareTo(hiBound2)) <= 0));
            return result;
        }

        /// <summary>
        /// Creates a new check field object with a range (type int) associated with this field object. 
        /// The new check field evaluates this field's value with the given range
        /// </summary>
        /// <param name="name">The name of the new check field object.</param>
        /// <param name="loBound">Specifies the lower boundary.</param>
        /// <param name="hiBound">Specifies the upper boundary.</param>
        /// <returns>Returns a reference to the current field object.</returns>
        public IField NewCheckFieldRange(string name, int loBound, int hiBound)
        {
            CreateNewCheckFieldRange(name, loBound, hiBound);
            return this;
        }
        /// <summary>
        /// Creates a new check field object with a range (type int) associated with this field object. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loBound1"></param>
        /// <param name="hiBound1"></param>
        /// <param name="loBound2"></param>
        /// <param name="hiBound2"></param>
        /// <returns></returns>
        public IField NewCheckFieldRange(string name, int loBound1, int hiBound1, int loBound2, int hiBound2)
        {
            CreateNewCheckFieldRange(name, loBound1, hiBound1, loBound2, hiBound2);
            return this;
        }
        /// <summary>
        /// Creates a new check field object with 3 ranges (type int) associated with this field object. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loBound1"></param>
        /// <param name="hiBound1"></param>
        /// <param name="loBound2"></param>
        /// <param name="hiBound2"></param>
        /// <param name="loBound3"></param>
        /// <param name="hiBound3"></param>
        /// <returns></returns>
        public IField NewCheckFieldRange(string name, int loBound1, int hiBound1, int loBound2, int hiBound2, int loBound3, int hiBound3)
        {
            CreateNewCheckFieldRange(name, loBound1, hiBound1, loBound2, hiBound2, loBound3, hiBound3);
            return this;
        }

        /// <summary>
        /// Creates a new check field object with a range (type int) associated with this field object. 
        /// The new check field evaluates this field's value with the given range
        /// </summary>
        /// <param name="name">The name of the new check field object.</param>
        /// <param name="loBound">Specifies the lower boundary.</param>
        /// <param name="hiBound">Specifies the upper boundary.</param>
        /// <returns>Returns a reference to the current field object.</returns>
        public ICheckField CreateNewCheckFieldRange(string name, int loBound, int hiBound)
        {
            ICheckField result = CreateNewCheckField(name, (fld) => (fld.GetValue<int>() >= loBound) && (fld.GetValue<int>() <= hiBound));
            return result;
        }
        /// <summary>
        /// Creates a new check field object with a range (type int) associated with this field object. 
        /// The new check field evaluates this field's value with the given range
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loBound1"></param>
        /// <param name="hiBound1"></param>
        /// <param name="loBound2"></param>
        /// <param name="hiBound2"></param>
        /// <returns></returns>
        public ICheckField CreateNewCheckFieldRange(string name, int loBound1, int hiBound1, int loBound2, int hiBound2)
        {
            ICheckField result = CreateNewCheckField(name, (fld) => ((fld.GetValue<int>() >= loBound1) && (fld.GetValue<int>() <= hiBound1)) || ((fld.GetValue<int>() >= loBound2) && (fld.GetValue<int>() <= hiBound2)));
            return result;
        }

        public ICheckField CreateNewCheckFieldRange(string name, int loBound1, int hiBound1, int loBound2, int hiBound2, int loBound3, int hiBound3)
        {
            ICheckField result = CreateNewCheckField(name, (fld) => ((fld.GetValue<int>() >= loBound1) && (fld.GetValue<int>() <= hiBound1))
            || ((fld.GetValue<int>() >= loBound2) && (fld.GetValue<int>() <= hiBound2))
            || ((fld.GetValue<int>() >= loBound3) && (fld.GetValue<int>() <= hiBound3)));
            return result;
        }

        /// <summary>
        /// Attempts to get the value of this object converted to the given type <typeparamref name="T"/>, returns 
        /// <c>true</c> if the conversion was successful, returns <c>false</c> if it was not.
        /// </summary>
        /// <param name="value">A reference to the object that takes the result value.</param>
        /// <returns>Returns true if the value was obtained successfully.</returns>
        public bool TryGetValue<T>(out T value)
        {
            bool result = false;
            value = default(T);

            try
            {
                value = GetValue<T>();
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        #endregion

        #region overrides
        /// <summary>
        /// Returns the length (in bytes) of this buffer element in the buffer.
        /// </summary>
        /// <returns>Returns the number of bytes the current field object occupies in the buffer.</returns>
        protected override int GetLength()
        {
            return lengthInBuffer;
        }

        /// <summary>
        /// Where appropriate, sets the length of this buffer element.
        /// </summary>
        /// <param name="value">Sets the number of bytes the current field object occupies in the buffer.</param>
        protected override void SetLength(int value)
        {
            lengthInBuffer = value;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, GetValueString(true));
        }


        #endregion

        #region IEquatable<T>
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IField other)
        {
            return FieldBase.CompareFieldValues(this, other) == 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(string other)
        {
            return FieldBase.Comparer.Compare(this, other) == 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(PackedDecimal other)
        {
            return FieldBase.Comparer.Compare(this, other) == 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(bool other)
        {
            return FieldBase.Comparer.Compare(this, other) == 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(int other)
        {
            return FieldBase.Comparer.Compare(this, other) == 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(decimal other)
        {
            return FieldBase.Comparer.Compare(this, other) == 0;
        }

        /// <summary>
        /// Checks whether the current field object value equals to the provided group object value.
        /// </summary>
        /// <param name="other">A reference to the group object for comparison.</param>
        /// <returns>Returns true if the current field object value is equal to the provided group object value.</returns>
        public bool Equals(IGroup other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Checks whether the current field object value equals to the provided record object value.
        /// </summary>
        /// <param name="other">A reference to the record object for comparison.</param>
        /// <returns>Returns true if the current field object value is equal to the provided record object value.</returns>
        public bool Equals(IRecord other)
        {
            return CompareTo(other) == 0;
        }
        #endregion

        #region IComparable<T>
        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>Not implemented. Throws NotImplementedException exception.</returns>
        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">A reference to the field object for comparison.</param>
        /// <returns>A signed integer that indicates the relation between the objects.</returns>
        public int CompareTo(IField other)
        {
            return FieldBase.Comparer.Compare(this, other);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">String for comparison.</param>
        /// <returns>A signed integer that indicates the relation between the objects.</returns>
        public int CompareTo(string other)
        {
            return FieldBase.Comparer.Compare(this, other);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">A reference to the packed decimal field for comparison.</param>
        /// <returns>A signed integer that indicates the relation between the objects.</returns>
        public int CompareTo(PackedDecimal other)
        {
            return FieldBase.Comparer.Compare(this, other);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">Boolean value for comparison.</param>
        /// <returns>A signed integer that indicates the relation between the objects.</returns>
        public int CompareTo(bool other)
        {
            return FieldBase.Comparer.Compare(this, other);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">Integer value for comparison.</param>
        /// <returns>A signed integer that indicates the relation between the objects.</returns>
        public int CompareTo(int other)
        {
            return FieldBase.Comparer.Compare(this, other);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">Decimal value for comparison.</param>
        /// <returns>A signed integer that indicates the relation between the objects.</returns>
        public int CompareTo(decimal other)
        {
            return FieldBase.Comparer.Compare(this, other);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">A reference to the group object for comparison.</param>
        /// <returns>A signed integer that indicates the relation between the objects.</returns>
        public int CompareTo(IGroup other)
        {
            return ComparisonMatrix.Compare(this as IField, other);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">A reference to the field object for comparison.</param>
        /// <returns>A signed integer that indicates the relation between the objects.</returns>
        public int CompareTo(IRecord other)
        {
            return ComparisonMatrix.Compare(this as IField, other);
        }
        #endregion

        #region buffer pointer methods

        /// <summary>
        /// Retrieves the address key value for the specified record element.
        /// </summary>
        /// <param name="element">A reference to the record element, which address key should be retrieved.</param>
        /// <returns>Returns the address key value for the specified record element.</returns>
        private static int GetAddressKeyFrom(IBufferElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element", "element is null.");

            IRecord record = element.Record;
            int recordKey = BufferServices.Records.GetKeyFor(record);
            if (recordKey == 0)
            {
                recordKey = BufferServices.Records.Add(record);
            }
            IBufferAddress bufferAddress = ObjectFactory.Factory.NewBufferAddress(recordKey, element.Name);
            bufferAddress.OptionalBufferStartIndex = element.PositionInParent;
            int addressKey = BufferServices.BufferAddresses.Add(bufferAddress);
            return addressKey;
        }

        /// <summary>
        /// Creates a new buffer object for the record element based on the provided buffer address key value.
        /// </summary>
        /// <param name="bufferAddressKey">Record's element buffer address key value.</param>
        private void SetBufferAddress(int bufferAddressKey)
        {
            IBufferAddress addr = BufferServices.BufferAddresses.Get(bufferAddressKey);
            if (addr != null)
            {
                IRecord record = BufferServices.Records.Get(addr.RecordKey);
                if (record != null)
                {
                    if (!String.IsNullOrEmpty(addr.ElementName))
                    {
                        if (record.Name == addr.ElementName)
                            SetBufferAddressTo(record);
                        else
                        {
                            IBufferElement element = record.RecordElement<IBufferElement>(addr.ElementName);
                            if (element != null)
                            {
                                SetBufferAddressTo(element);
                            }
                        }
                    }
                }
                else
                    throw new DataBufferException(String.Format("The record specified with record key {0} could not be found.", addr.RecordKey));
            }
            else
                throw new DataBufferException(string.Format("Could not find buffer address element of given key: {0}", bufferAddressKey));
        }

        /// <summary>
        /// Creates a new buffer object for the provided buffer element object.
        /// </summary>
        /// <param name="element">A reference to the buffer element object for the address assignment.</param>
        private void SetBufferAddressTo(IBufferElement element)
        {
            // Buffer bytes are not the same as bytes of the group elements.  Need to fix how the record data is composed.
            byte[] tmpBytes = null;

            if (element is GroupBase)
                tmpBytes = ((GroupBase)element).AsBytes;
            else if (element is FieldBase)
                tmpBytes = ((FieldBase)element).AsBytes;
            else
                throw new Exception("SetBufferAddressTo: Not Implemented for " + element.GetType().Name + " !");

            byte[] tmpBufferBytes = new byte[LengthInBuffer];
            System.Array.Copy(tmpBytes, 0, tmpBufferBytes, 0, tmpBytes.Length);
            AssignFrom(tmpBufferBytes);
        }

        /// <summary>
        /// Creates a new buffer object for the provided buffer record object.
        /// </summary>
        /// <param name="record">A reference to the record object for the address assignment.</param>
        private void SetBufferAddressTo(IRecord record)
        {
            byte[] tmpBytes = record.AsBytes();
            byte[] tmpBufferBytes = new byte[LengthInBuffer];
            System.Array.Copy(tmpBytes, 0, tmpBufferBytes, 0, tmpBytes.Length);
            AssignFrom(tmpBufferBytes);
        }

        /// <summary>
        /// Functions as COBOL statement <c>SET ADDRESS OF C TO P</c>.
        /// Causes the field object to point its buffer to the buffer specified
        /// by the address stored in <paramref name="addressField"/>.
        /// <paramref name="addressField"/>.
        /// </summary>
        /// <param name="addressField">Specifies the buffer address for the field.</param>
        public void SetAddressFromValueOf(IField addressField)
        {
            if (addressField == null)
                throw new ArgumentNullException("addressField", "addressField is null.");

            int bufferAddressKey = addressField.GetValue<int>();
            SetBufferAddress(bufferAddressKey);
        }

        /// <summary>
        /// Functions as COBOL statement <c>SET ADDRESS OF C TO ADDRESS OF B</c>.
        /// Causes the field object to point its buffer reference to the 
        /// buffer of the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">A reference to the record element.</param>
        public void SetAddressToAddressOf<T>(T element) where T : IBufferElement, IBufferValue
        {
            SetBufferAddressTo(element);
        }

        /// <summary>
        /// Functions as COBOL statement <c>SET P TO ADDRESS OF B</c>. 
        /// Causes the field object to set its value to the "address" of the 
        /// buffer of the given <paramref name="element"/>.
        /// </summary>
        /// <remarks><para>In context of the method parameters, the COBOL would 
        /// read as <c>SET thisField TO ADDRESS OF <paramref name="element"/></c>
        /// </para></remarks>
        /// <param name="element">The IField whose buffer address will be stored in this field.</param>
        public void SetValueToAddressOf<T>(T element) where T : IBufferElement, IBufferValue
        {
            if (element == null)
                throw new ArgumentNullException("element");

            int addressKey = GetAddressKeyFrom(element);
            this.Assign(addressKey);
        }
        #endregion

        /// <summary>
        /// Creates a deep copy of the current field object.
        /// </summary>
        /// <returns>Returns a deep copy of the current field object.</returns>
        public object Clone()
        {
            FieldBase fbClone = (FieldBase)this.MemberwiseClone();
            fbClone.checkFields = new Dictionary<string, ICheckField>();
            return fbClone;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void GetAcceptData(string text)
        {
            throw new Exception("GetAcceptData - Not Implemented");
        }

        public void SetIsBlankWhenZero(bool isBlankWhenZero)
        {
            this.IsBlankWhenZero = isBlankWhenZero;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        public void SetReferenceTo(IRecord record)
        {
            this.Record = record;
            this.Buffer = record.Buffer;
        }
    }
}