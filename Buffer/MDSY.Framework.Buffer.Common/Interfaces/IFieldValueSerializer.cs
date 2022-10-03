
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Defines an object which serializes given data into a byte arary. 
    /// </summary>

    public interface IFieldValueSerializer
    {
        /// <summary>
        /// Returns the byte serialization of the given <paramref name="value"/> as appropriate 
        /// to the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to be serialized.</typeparam>
        /// <param name="value">The value to be serialized.</param>
        /// <param name="byteCount">Length of byte array to be returned.</param>
        /// <returns>The serialized value.</returns>
        /// <param name="fieldType">Field type.</param>
        /// <param name="decimalDigits">Number of digits to the right from the decimal separator.</param>
        byte[] Serialize<T>(T value, int byteCount, FieldType fieldType, FieldType sourceFieldType, int decimalDigits);

        /// <summary>
        /// Returns the given <paramref name="bytes"/> deserialized into a value of the given 
        /// <typeparamref name="T"/> type. 
        /// </summary>
        /// <param name="decimalDigits">Number of digits to the right from the decimal separator.</param>
        /// <param name="fieldType">Field type.</param>
        T Deserialize<T>(byte[] bytes, FieldType fieldType, int decimalDigits, bool IsDestinationString = false);

        //[Obsolete("Use Deserialize with decimal digits", true)]
        //T Deserialize<T>(byte[] bytes, FieldType fieldType);


        /// <summary>
        /// Attempts to deserialize the given <paramref name="bytes"/> into the <paramref name="value"/> parameter.
        /// </summary>
        /// <typeparam name="T">The type the bytes will deserialized into.</typeparam>
        /// <param name="bytes">The byte array containing the value to be deserialized.</param>
        /// <param name="value">The parameter to receive the deserialized value.</param>
        /// <returns><c>True</c> if the deserialization was successful, <c>false</c> otherwise.</returns>
        /// <param name="decimalDigits">Number of digits to the right from the decimal separator.</param>
        bool TryDeserialize<T>(byte[] bytes, FieldType fieldType, int decimalDigits, out T value, bool isDestinationString = false);
    }
}

