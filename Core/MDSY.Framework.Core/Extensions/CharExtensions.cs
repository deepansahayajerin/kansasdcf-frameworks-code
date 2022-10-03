using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Extension methods for the Char type. 
    /// </summary>
    public static class CharExtensions
    {
        /// <summary>
        /// Obsolete method. Returns 255 value casted to a char.
        /// </summary>
        /// <param name="instance">A reference to the CharExtensions object, which is extended with the current method.</param>
        /// <returns></returns>
        [Obsolete("char types in byte operations should be replaced with AsciiChar", false)]
        public static char MaxAsciiValue(this Char instance)
        {
            return (char)byte.MaxValue;
        }

        /// <summary>
        /// Obsolete method. Returns <c>true</c> if the <c>char</c> value equals <c>Char.MaxValue</c>.
        /// </summary>
        [Obsolete("char types in byte operations should be replaced with AsciiChar", false)]
        public static bool IsMaxAsciiValue(this Char instance)
        {
            return instance.Equals(instance.MaxAsciiValue());
        }

        /// <summary>
        /// Returns <c>true</c> if the <c>char</c> value equals <c>Char.MinValue</c>.
        /// </summary>
        [Obsolete("char types in byte operations should be replaced with AsciiChar", false)]
        public static bool IsMinValue(this Char instance)
        {
            return instance.Equals(Char.MinValue);
        }
    }
}
