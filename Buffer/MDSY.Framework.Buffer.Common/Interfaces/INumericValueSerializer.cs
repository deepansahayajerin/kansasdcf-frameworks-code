
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Defines an object which serializes or deserializes a specific numeric value type (<typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the numeric value.</typeparam>
    public interface INumericValueSerializer<T> : IValueSerializer<T>
        where T : struct
    {
        /// <summary>
        /// Deserializes the given byte array into a value of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="bytes">The byte array containing the serialized value to be deserialized.</param>
        /// <param name="digits">The number of digits to the right of the decimal point. Defaults to 0.</param>
        /// <returns>The <paramref name="bytes"/> deserialized as type <paramref name="T"/>.</returns>
        T Deserialize(byte[] bytes, int digits);
    }
}

