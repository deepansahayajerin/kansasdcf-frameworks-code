using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{

    /// <summary>
    /// Defines an object which serializes/deserializes values of a given type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of value to be serialized.</typeparam>
    public interface IValueSerializer<T>
    {
        /// <summary>
        /// Deserializes the given byte array into a value of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="bytes">The byte array containing the serialized value to be deserialized.</param>
        /// <returns>The <paramref name="bytes"/> deserialized as type <paramref name="T"/>.</returns>
        T Deserialize(byte[] bytes);

        /// <summary>
        /// Serializes the given value to a byte array. 
        /// </summary>
        /// <param name="value">The value to be serialized.</param>
        /// <returns>The value converted to bytes.</returns>
        byte[] Serialize(T value);
    }
}

