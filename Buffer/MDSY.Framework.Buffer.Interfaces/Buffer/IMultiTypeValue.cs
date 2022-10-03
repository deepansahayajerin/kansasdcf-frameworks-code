using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which can get or assign its value to or from the given type.
    /// </summary>
    public interface IMultiTypeValue
    {
        /// <summary>
        /// Attempts to get the value of this object converted to the given type <typeparamref name="T"/>.
        /// If the value cannot be converted, an exception is thrown.
        /// </summary>
        /// <returns>This object's value as a type <typeparamref name="T"/>.</returns>
        T GetValue<T>();

        /// <summary>
        /// Attempts to get the value of this object converted to the given type <typeparamref name="T"/>, returns 
        /// <c>true</c> if the conversion was successful, returns <c>false</c> if it was not.
        /// </summary>
        /// <param name="value">Specifies the given type to convert to.</param>
        /// <returns></returns>
        bool TryGetValue<T>(out T value);

    }
}
