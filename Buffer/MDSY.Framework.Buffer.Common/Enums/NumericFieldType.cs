using System;
using System.Collections.Generic;
using System.Linq;


namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Specifies numeric field type.
    /// </summary>
    public enum NumericFieldType
    {
        /// <summary>
        /// Indicates that field type is unknown. Numeric value is 0.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Indicates that field type is non-numeric. Numeric value is 1.
        /// </summary>
        NotNumeric,

        /// <summary>
        /// Indicates that field type is signed integer. Numeric value is 2.
        /// </summary>
        SignedInteger,

        /// <summary>
        /// Indicates that field type is unsigned integer. Numeric value is 3.
        /// </summary>
        UnsignedInteger,

        /// <summary>
        /// Indicates that field type is decimal. Numeric value is 4.
        /// </summary>
        Decimal,

        /// <summary>
        /// Indicates that field type is packed decimal. Numeric value is 5.
        /// </summary>
        PackedDecimal
    }
}
