using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Extension methods for collections of AsciiChars. 
    /// </summary>
    public static class IEnumerableOfAsciiCharExtensions
    {
        /// <summary>
        /// Returns a new <c>string</c> instance built from the collection of <c>AsciiChar</c>.
        /// </summary>
        public static string NewString(this IEnumerable<AsciiChar> instance)
        {
            return new string(instance.Select(ac => ac.AsChar).ToArray());
        }

        /// <summary>
        /// Converts current collection of AsciiChar objects into an array of bytes.
        /// </summary>
        /// <param name="instance">Current instance of the IEnumerableOfAsciiChar object.</param>
        /// <returns>Returns an array of bytes that carries the content or the current IEnumerableOfAsciiChar instance.</returns>
        public static byte[] ToByteArray(this IEnumerable<AsciiChar> instance)
        {
            return instance.Select(ac => ac.AsByte).ToArray();
        }

        /// <summary>
        /// Returns a "packed" byte value built by treating the numeric characters found at <paramref name="index"/> and 
        /// <paramref name="index"/>+1 as nybbles for the new byte.
        /// </summary>
        /// <param name="instance">The character collection.</param>
        /// <param name="index">The position in the collection from which to get the two chars used in creating the byte.</param>
        /// <returns>Returns a "packed" byte value.</returns>
        public static byte NumCharsToPackedByte(this IEnumerable<AsciiChar> instance, int index)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");
            if (index + 1 >= instance.Count())
                throw new ArgumentException("index", "index + 1 would exceed range.");

            var byteStr = instance.Skip(index).Take(2).NewString();
            return byte.Parse(byteStr, NumberStyles.HexNumber);
        }


    }
}

