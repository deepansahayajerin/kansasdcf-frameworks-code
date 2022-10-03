using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{

    /// <summary>
    /// Specifies the data type contained by a field.
    /// </summary>
    public enum FieldType
    {
        /// <summary>Indicates a string value.</summary>
        String = 0,
        /// <summary>Indicates a Boolean value.</summary>
        Boolean,
        /// <summary>Indicates a numeric value of the PackedDecimal type.</summary>
        PackedDecimal,
        /// <summary>Indicates a signed whole number value.</summary>
        SignedNumeric,
        /// <summary>Indicates an unsigned whole number value.</summary>
        UnsignedNumeric,
        /// <summary>Indicates an Int16 value stored in binary format.</summary>
        CompShort,
        /// <summary>Indicates an Int32 value stored in binary format.</summary>
        CompInt,
        /// <summary>Indicates an Int64 value stored in binary format.</summary>
        CompLong,
        /// <summary>Indicates a signed decimal number value.</summary>
        SignedDecimal,
        /// <summary>Indicates an unsigned decimal number value.</summary>
        UnsignedDecimal,
        /// <summary>Indicates a single-precision floating-point number value.</summary>
        FloatSingle,
        /// <summary>Indicates a double-precision floating-point number value.</summary>
        FloatDouble, //,
        /// <summary> Indicates a field that will hold a Field/Group reference </summary>
        ReferencePointer,
        /// <summary>Indicates a field contaning an unsigned PackedDecimal value.</summary>
        UnsignedPackedDecimal,
        /// <summary>Indicates a numeric value, which is stored as string formatted with edit mask. Presence of edit mask is required for this type.</summary>
        NumericEdited,
        /// <summary>Indicates a Binary value to be handled as byte[]</summary>
        Binary
    }
}
