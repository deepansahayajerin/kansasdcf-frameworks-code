using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Array Initializer.  Gets and sets the Array element count as well as the Array element length.
    /// </summary>
    public interface IArrayBaseInitializer : IBufferElementInitializer
    {
        /// <summary>
        /// Gets and sets The array element count
        /// </summary>
        int ArrayElementCount { get; set; }

        /// <summary>
        /// Gets and sets the array element lenth.
        /// </summary>
        int ArrayElementLength { get; set; }

    }
}
