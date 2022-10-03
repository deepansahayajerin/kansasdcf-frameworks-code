using System;
using System.Collections.Generic;
using System.Linq;


namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Specifies what sort of <c>AsciiChar</c> values with which to fill a field.
    /// </summary>
    public enum FillWith
    {
        /// <summary>Indicates that the field should not be filled.</summary>
        DontFill = 0,
        /// <summary>Indicates that the field should be filled with bytes of value <c>0x00</c>.</summary>
        Nulls,
        /// <summary>Indicates that the field should be filled with <c>SPACE</c> characters (hex <c>0x20</c>).</summary>
        Spaces,
        /// <summary>Indicates that the field should be filled with zero (<c>0</c>) characters (hex <c>0x30</c>).</summary>
        Zeroes,
        /// <summary>Indicates that the field should be filled with octothorpe (<c>#</c>) characters (hex <c>0x23</c>).</summary>
        Hashes,
        /// <summary>Indicates that the field should be filled with hyphen (<c>-</c>) characters (hex <c>0x2D</c>).</summary>
        Dashes,
        /// <summary>Indicates that the field should be filled with  (hex <c>0xFF</c>).</summary>
        HighValues,
        /// <summary>Indicates that the field should be filled with  (hex <c>0x00</c>).</summary>
        LowValues,
        /// <summary>Indicates that the field should be filled with Equal sign characters (hex <c>0x3D</c>).</summary>
        Equals,
        /// <summary>Indicates that the field should be filled with Underscore characters (hex <c>0x5F</c>).</summary>
        Underscores
    }
}

