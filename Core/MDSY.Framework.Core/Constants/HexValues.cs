using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Core.Constants
{
    /// <summary>
    /// Contains string and char constans that define hexadecimal notation and some hexadecimal values.
    /// </summary>
    public static class HexValues
    {
        /// <summary>
        /// "X" string constant.
        /// </summary>
        public const string HexFormatSpecifier = "X";

        /// <summary>
        /// "H'{0}'" string constant.
        /// </summary>
        public const string HexFormatRepresentation = "H'{0}'";

        /// <summary>
        /// '\x0101' character constant.
        /// </summary>
        public const char HEX_0101 = '\x0101';

        /// <summary>
        /// '\x0000' character constant.
        /// </summary>
        public const char HEX_0000 = '\x0000';
    }
}
