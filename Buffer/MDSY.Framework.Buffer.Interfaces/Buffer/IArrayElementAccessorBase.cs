using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Non-generic baseclass for IArrayElementAccessor(of TItem).
    /// </summary>
    public interface IArrayElementAccessorBase: ICloneable  
    {
        /// <summary>
        /// Gets the name of this accessor object; this is also the name the accessor
        /// will use when accessing array elements.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the number of array elements to which this object has access.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns the length of one element of the array
        /// </summary>
        int LengthInBuffer { get; }

    }
}
