using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Extension methods for string objects. 
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if current string contains a string representation of the boolean true value.
        /// </summary>
        /// <param name="instance">Current string instance.</param>
        /// <returns>Returns true if current string contains a string representation of the boolean true value.</returns>
        public static bool IsBooleanTrue(this string instance)
        {
            if (String.IsNullOrEmpty(instance))
                throw new ArgumentException("instance is null or empty.", "instance");

            return Constants.BooleanStrings.TrueStrings.Contains(instance);
        }

        /// <summary>
        /// Checks if current string contains a string representation of the boolean false value.
        /// </summary>
        /// <param name="instance">Current string instance.</param>
        /// <returns>Returns true if current string contains a string representation of the boolean false value.</returns>
        public static bool IsBooleanFalse(this string instance)
        {
            if (String.IsNullOrEmpty(instance))
                throw new ArgumentException("instance is null or empty.", "instance");

            return Constants.BooleanStrings.FalseStrings.Contains(instance);
        }

        /// <summary>
        /// Returns an array of AsciiChar built from the char elements of the string object. Similar to String.ToCharArray().
        /// </summary>
        /// <param name="instance">Current string instance.</param>
        /// <returns>Returns an array of AsciiChar built from the char elements of the string object.</returns>
        public static AsciiChar[] ToAsciiCharArray(this string instance)
        {
            return instance.Select(c => (AsciiChar)c).ToArray();
        }
    }
}
