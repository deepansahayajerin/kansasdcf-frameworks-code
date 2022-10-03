using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Provides support for comparison of IFields, IGroups, and IRecords.
    /// </summary>
    public static class ComparisonMatrix
    {
        #region private methods


        /// <summary>
        /// Compares two byte arrays specifically with buffer object comparison rules.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns><para>0, if both objects are null, or if they are the same.
        /// -1, if <paramref name="x"/> is null or less than <paramref name="y"/>, or
        /// 1 if <paramref name="y"/> is null or <paramref name="x"/> is greater than <paramref name="y"/> </para>
        /// </returns>
        /// <param name="xAsNumeric">Specifies whether first object should be as numeric.</param>
        /// <param name="yAsNumeric">Specifies whether second object should be as numeric.</param>
        private static Nullable<int> CompareBufferBytes(byte[] x, byte[] y, bool xAsNumeric, bool yAsNumeric)
        {
            var xStr = x.Select(b => (AsciiChar)b).NewString();
            var yStr = y.Select(b => (AsciiChar)b).NewString();

            return xStr.Length == yStr.Length ?
                                    string.Compare(xStr, yStr) :
                                    CompareBufferValueStrings(xStr, yStr, xAsNumeric, yAsNumeric);
        }

        /// <summary>
        /// Compares two buffer objects as objects (i.e. by reference) rather
        /// than by values.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns><para>0, if both object are null, or if they are the same object 
        /// instance. 
        /// Otherwise, if <paramref name="x"/> is null, -1; or 
        /// if <paramref name="y"/> is null, 1. </para>
        /// <para>If none of these conditions are met, returns null.</para></returns>
        private static Nullable<int> CompareBufferObjects(object x, object y)
        {
            Nullable<int> result = null;

            if ((x == null && y == null) || object.ReferenceEquals(x, y))
            {
                result = 0;
            }
            else
            {
                if (x == null)
                {
                    result = -1;
                }
                else if (y == null)
                {
                    result = 1;
                }
            }

            return result;
        }

        private static Nullable<int> CompareFieldObjects(IField x, IField y)
        {
            return CompareBufferObjects(x, y);
        }

        private static Nullable<int> CompareGroupObjects(IGroup x, IGroup y)
        {
            return CompareBufferObjects(x, y);
        }

        private static Nullable<int> CompareRecordObjects(IRecord x, IRecord y)
        {
            return CompareBufferObjects(x, y);
        }


        private static Nullable<int> CompareFieldsOfDifferingTypes(IField x, IField y)
        {
            Nullable<int> result = null;
            switch (x.FieldType)
            {
                case FieldType.String:
                    result = CompareFieldValuesAsString(x.GetValue<string>(), y);
                    break;

                case FieldType.Boolean:
                    result = CompareFieldValuesAsBool(x.GetValue<bool>(), y);
                    break;

                case FieldType.PackedDecimal:
                case FieldType.UnsignedPackedDecimal:
                    result = CompareFieldValuesAsDecimal(x.GetValue<Decimal>(), y);
                    break;

                case FieldType.SignedNumeric:
                case FieldType.CompShort:
                case FieldType.CompInt:
                case FieldType.CompLong:
                case FieldType.ReferencePointer:
                    result = CompareFieldValuesAsInt64(x.GetValue<Int64>(), y);
                    break;

                case FieldType.UnsignedNumeric:
                    result = CompareFieldValuesAsUInt64(x.GetValue<UInt64>(), y);
                    break;

                case FieldType.FloatSingle:
                case FieldType.FloatDouble:
                case FieldType.SignedDecimal:
                case FieldType.UnsignedDecimal:
                    result = CompareFieldValuesAsDecimal(x.GetValue<Decimal>(), y);
                    break;

                default:
                    result = CompareFieldValuesAsString(x.GetValue<string>(), y);
                    break;
            }
            return result;
        }

        private static Nullable<int> CompareBufferValueStrings(string xStrValue, string yStrValue, bool xAsNumeric, bool yAsNumeric)
        {
            if (xStrValue.Length < yStrValue.Length)
            {
                // pad x's length to match y.
                if (xAsNumeric)
                {
                    xStrValue = xStrValue.PadLeft(yStrValue.Length, '0');
                }
                else
                {
                    xStrValue = xStrValue.PadRight(yStrValue.Length, ' ');
                }
            }
            else if (xStrValue.Length > yStrValue.Length)
            {
                // pad y's length to match x.
                if (yAsNumeric)
                {
                    yStrValue = yStrValue.PadLeft(xStrValue.Length, '0');
                }
                else
                {
                    yStrValue = yStrValue.PadRight(xStrValue.Length, ' ');
                }
            }

            return string.Compare(xStrValue, yStrValue);
        }
        #endregion

        #region internal methods


        internal static int CompareFieldValuesAsBool(bool xValue, IField y)
        {
            int result;

            try
            {
                result = xValue.CompareTo(y.GetValue<bool>());
            }
            catch (FieldValueException)
            {
                // we can't convert the y value to a bool. 
                result = 1;
            }
            return result;
        }

        internal static int CompareFieldValuesAsDecimal(Decimal xValue, IField y)
        {
            return xValue.CompareTo(y.GetValue<Decimal>());
        }

        internal static int CompareFieldValuesAsPackedDecimal(PackedDecimal xValue, IField y)
        {
            PackedDecimal yValue = y.GetValue<PackedDecimal>(); // <-- this will throw if we can't convert.
            return xValue.CompareTo(yValue);
        }

        internal static int CompareFieldValuesAsInt64(Int64 xValue, IField y)
        {
            Int64 testValue = 0;
            if (Int64.TryParse(y.DisplayValue, out testValue))
                testValue = y.GetValue<Int64>();
            else
                return 1;
            return xValue.CompareTo(testValue);
        }

        internal static int CompareFieldValuesAsString(string xValue, IField y)
        {
            if (y == null)
                throw new ArgumentNullException("y", "y is null.");

            Int64 testValue = 0;
            bool xAsNumeric = Int64.TryParse(xValue, out testValue);
            var result = CompareBufferValueStrings(xValue, y.GetValue<string>(), xAsNumeric, y.IsNumericType);
            if (!result.HasValue)
                throw new FieldValueException("Comparison failed; result was null.");
            return result.Value;
        }

        internal static int CompareFieldValuesAsUInt64(UInt64 xValue, IField y)
        {
            UInt64 testValue = 0;
            if (UInt64.TryParse(y.DisplayValue, out testValue))
                testValue = y.GetValue<UInt64>();
            else
                return 1;
            return xValue.CompareTo(testValue);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Compares two IField objects. 
        /// </summary>
        /// <param name="x">First object to compare.</param>
        /// <param name="y">Second object to compare.</param>
        /// <returns>0, if both objects are null, or if they are the same.
        /// -1, if <paramref name="x"/> is null or less than <paramref name="y"/>.
        /// 1, if <paramref name="y"/> is null or <paramref name="x"/> is greater than <paramref name="y"/> </returns>
        public static int Compare(IField x, IField y)
        {
            Nullable<int> result = null;

            // first compare objects...
            result = CompareFieldObjects(x, y);

            // ... then compare values. 
            if (!result.HasValue)
            {
                NumericFieldType xType = x.GetNumericType();

                if (x.FieldType == y.FieldType)
                {
                    decimal testDec = 0;
                    //Following check added for SAAQ issue 5450
                    if (decimal.TryParse(x.DisplayValue, out testDec) && decimal.TryParse(y.DisplayValue, out testDec))
                    {
                        switch (xType)
                        {
                            case NumericFieldType.Unknown:
                            case NumericFieldType.NotNumeric:
                                result = CompareBufferBytes(x.AsBytes, y.AsBytes, x.IsNumericType, y.IsNumericType);
                                break;
                            case NumericFieldType.SignedInteger:
                                result = x.GetValue<Int64>().CompareTo(y.GetValue<Int64>());
                                break;
                            case NumericFieldType.UnsignedInteger:
                                result = x.GetValue<UInt64>().CompareTo(y.GetValue<UInt64>());
                                break;
                            case NumericFieldType.Decimal:
                                result = x.GetValue<Decimal>().CompareTo(y.GetValue<Decimal>());
                                break;
                            case NumericFieldType.PackedDecimal:
                                result = x.GetValue<PackedDecimal>().CompareTo(y.GetValue<PackedDecimal>());
                                break;
                        }
                    }
                    else
                    {
                        result = CompareBufferBytes(x.AsBytes, y.AsBytes, x.IsNumericType, y.IsNumericType);
                    }

                }
                else
                {
                    // modified because of ticket 5265. June 19, 2015
                    // Modified because of ticket 5413. Julu 07, 2015
                    if (x.IsNumericType && y.IsNumericType)
                    {
                        decimal xVal = 0;
                        decimal yVal = 0;
                        if (x.BytesAsString.Trim().Length > 0 || x.FieldType == FieldType.PackedDecimal)
                            xVal = x.GetValue<Decimal>();
                        if (y.BytesAsString.Trim().Length > 0 || y.FieldType == FieldType.PackedDecimal)
                            yVal = y.GetValue<Decimal>();

                        result = xVal.CompareTo(yVal);
                        //result = x.GetValue<Decimal>().CompareTo(y.GetValue<Decimal>());
                    }
                    else if (x.IsNumericType || y.IsNumericType)
                    {
                        decimal d = 0;
                        if (!x.IsNumericType && decimal.TryParse(x.BytesAsString, out d))
                            result = d.CompareTo(y.GetValue<Decimal>());
                        else if (!y.IsNumericType && decimal.TryParse(y.BytesAsString, out d))
                        {
                            if (x.IsMaxValue())
                                result = 1;
                            else
                                result = x.GetValue<Decimal>().CompareTo(d);
                        }                            
                        else
                            result = CompareFieldsOfDifferingTypes(x, y);
                    }
                    else
                    {
                        result = CompareFieldsOfDifferingTypes(x, y);
                    }

                    //// if the field types don't match, things get more complicated. 

                    //NumericFieldType yType = y.GetNumericType();
                    //// if either is non-numeric, compare as bytes/strings
                    //if (xType == NumericFieldType.Unknown || xType == NumericFieldType.NotNumeric ||
                    //    yType == NumericFieldType.Unknown || yType == NumericFieldType.NotNumeric)
                    //{
                    //    result = CompareFieldsOfDifferingTypes(x, y);
                    //}
                    //else // both are numeric of some sort
                    //{
                    //    result = x.GetValue<Decimal>().CompareTo(y.GetValue<Decimal>());
                    //}
                }
            }

            if (!result.HasValue)
                throw new FieldValueException("Field comparison failed; result was null.");

            return result.Value;
        }

        /// <summary>
        /// Compares two IGroup objects.
        /// </summary>
        /// <param name="x">First object to be compared.</param>
        /// <param name="y">Second object to be compared.</param>
        /// <returns>0, if both objects are null, or if they are the same.
        /// -1, if <paramref name="x"/> is null or less than <paramref name="y"/>.
        /// 1, if <paramref name="y"/> is null or <paramref name="x"/> is greater than <paramref name="y"/></returns>
        public static int Compare(IGroup x, IGroup y)
        {
            Nullable<int> result = null;

            result = CompareGroupObjects(x, y);

            if (!result.HasValue)
            {
                result = CompareBufferBytes(x.AsBytes, y.AsBytes, false, false);
            }

            if (!result.HasValue)
                throw new FieldValueException("Group comparison failed; result was null.");

            return result.Value;
        }

        /// <summary>
        /// Compares the buffer values of an IGroup and an IRecord.
        /// </summary>
        /// <param name="x">First object to be compared.</param>
        /// <param name="y">Second object to be compared.</param>
        /// <returns>0, if both objects are null, or if they are the same.
        /// -1, if <paramref name="x"/> is null or less than <paramref name="y"/>.
        /// 1, if <paramref name="y"/> is null or <paramref name="x"/> is greater than <paramref name="y"/></returns>
        public static int Compare(IGroup x, IRecord y)
        {
            Nullable<int> result = CompareBufferBytes(x.AsBytes, y.Buffer.ReadBytes(), false, false);

            if (!result.HasValue)
                throw new FieldValueException("Comparison failed; result was null.");
            return result.Value;
        }

        /// <summary>
        /// Compares the buffer values of an IField and an IRecord.
        /// </summary>
        /// <param name="x">First object to be compared.</param>
        /// <param name="y">Second object to be compared.</param>
        /// <returns>0, if both objects are null, or if they are the same.
        /// -1, if <paramref name="x"/> is null or less than <paramref name="y"/>.
        /// 1, if <paramref name="y"/> is null or <paramref name="x"/> is greater than <paramref name="y"/></returns>
        public static int Compare(IField x, IRecord y)
        {
            Nullable<int> result = CompareBufferBytes(x.AsBytes, y.Buffer.ReadBytes(), x.IsNumericType, false);

            if (!result.HasValue)
                throw new FieldValueException("Comparison failed; result was null.");
            return result.Value;
        }


        /// <summary>
        /// Compares the buffer values of an IField and an IGroup.
        /// </summary>
        /// <param name="x">First object to be compared.</param>
        /// <param name="y">Second object to be compared.</param>
        /// <returns>0, if both objects are null, or if they are the same.
        /// -1, if <paramref name="x"/> is null or less than <paramref name="y"/>.
        /// 1, if <paramref name="y"/> is null or <paramref name="x"/> is greater than <paramref name="y"/></returns>
        public static int Compare(IField x, IGroup y)
        {
            Nullable<int> result = 0;
            if (x.IsNumericType)
            {
                result = x.CompareTo(y.AsDecimal());
            }
            else
               result = CompareBufferBytes(x.AsBytes, y.AsBytes, x.IsNumericType, false);

            if (!result.HasValue)
                throw new FieldValueException("Comparison failed; result was null.");
            return result.Value;
        }

        /// <summary>
        /// Compares two IRecord objects.
        /// </summary>
        /// <param name="x">First object to be compared.</param>
        /// <param name="y">Second object to be compared.</param>
        /// <returns>0, if both objects are null, or if they are the same.
        /// -1, if <paramref name="x"/> is null or less than <paramref name="y"/>.
        /// 1, if <paramref name="y"/> is null or <paramref name="x"/> is greater than <paramref name="y"/></returns>
        public static int Compare(IRecord x, IRecord y)
        {
            Nullable<int> result = null;

            result = CompareRecordObjects(x, y);

            if (!result.HasValue)
            {
                result = CompareBufferBytes(x.Buffer.ReadBytes(), y.Buffer.ReadBytes(), false, false);
            }

            if (!result.HasValue)
                throw new FieldValueException("Record comparison failed; result was null.");

            return result.Value;
        }

        /// <summary>
        /// Compares the value of a field and a string.
        /// </summary>
        /// <param name="x">First value to be compared.</param>
        /// <param name="y">Second value to be compared.</param>
        /// <returns>0, if both objects are null, or if they are the same.
        /// -1, if <paramref name="x"/> is null or less than <paramref name="y"/>.
        /// 1, if <paramref name="y"/> is null or <paramref name="x"/> is greater than <paramref name="y"/></returns>
        public static int Compare(IField x, string y)
        {
            Nullable<int> result = ComparisonMatrix.CompareBufferValueStrings(x.BytesAsString, y, x.IsNumericType, false);

            if (!result.HasValue)
                throw new FieldValueException("Comparison failed; result was null.");

            return result.Value;
        }

        /// <summary>
        /// Compares the value of a grooup and a string.
        /// </summary>
        /// <param name="x">First value to be compared.</param>
        /// <param name="y">Second value to be compared.</param>
        /// <returns>0, if both objects are null, or if they are the same.
        /// -1, if <paramref name="x"/> is null or less than <paramref name="y"/>.
        /// 1, if <paramref name="y"/> is null or <paramref name="x"/> is greater than <paramref name="y"/></returns>
        public static int Compare(IGroup x, string y)
        {
            Nullable<int> result = ComparisonMatrix.CompareBufferValueStrings(x.BytesAsString, y, false, false);

            if (!result.HasValue)
                throw new FieldValueException("Comparison failed; result was null.");

            return result.Value;
        }

        /// <summary>
        /// Compares the value of a record and a string.
        /// </summary>
        /// <param name="x">First value to be compared.</param>
        /// <param name="y">Second value to be compared.</param>
        /// <returns>0, if both objects are null, or if they are the same.
        /// -1, if <paramref name="x"/> is null or less than <paramref name="y"/>.
        /// 1, if <paramref name="y"/> is null or <paramref name="x"/> is greater than <paramref name="y"/></returns>
        public static int Compare(IRecord x, string y)
        {
            Nullable<int> result = ComparisonMatrix.CompareBufferValueStrings(x.AsString(), y, false, false);

            if (!result.HasValue)
                throw new FieldValueException("Comparison failed; result was null.");

            return result.Value;
        }


        #endregion

    }
}

