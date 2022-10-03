using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Unity;
using System.ComponentModel;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which provides access to the underlying buffer array containing data. 
    /// </summary>
    [InjectionInterface]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface IDataBufferAccess
    {

        #region attributes
        /// <summary>
        /// Gets the underlying data buffer object.
        /// </summary>
        IDataBuffer Buffer { get; }

        /// <summary>
        /// Gets the buffer start index (0-based) of the byte sub-set stored by this object.
        /// </summary>
        int ElementStartIndex { get; }

        /// <summary>
        /// Gets the length (in bytes) of the byte sub-set stored by this object.
        /// </summary>
        int ElementLength { get; }
        #endregion

        #region operations
        /// <summary>
        /// Writes the given bytes to the underlying data buffer starting at <see cref="ElementStartIndex"/>
        /// and running through <see cref="ElementLength"/>.
        /// </summary>
        /// <param name="value">The bytes to be written.</param>
        void SetElementBytes(byte[] value);

        /// <summary>
        /// Returns the subset of bytes from the underlying data buffer starting at <see cref="ElementStartIndex"/>
        /// and running through <see cref="ElementLength"/>.
        /// </summary>
        /// <returns>The specified byte subset.</returns>
        byte[] GetElementBytes();
        #endregion

    }
}
