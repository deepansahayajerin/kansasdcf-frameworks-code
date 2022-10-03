using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using System.ComponentModel;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which contains the bytes of a mainframe-style buffer. 
    /// </summary>
    [InjectionInterface]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface IDataBuffer
    {
        #region attributes
        /// <summary>
        /// Gets the length of the buffer array.
        /// </summary>
        /// <returns>Number of bytes in buffer definition.</returns>
        int Length { get; }

        long WriteCount { get; }

        #endregion

        #region operations

        /// <summary>
        /// Returns an IDataBuffer object based on a byte array, rather than one based on a List(of byte). 
        /// </summary>
        /// <remarks>
        /// <para>If this object uses a List(of byte) as its internal storage, then it is a buffer used only temporarily during record definition.
        /// Calling GetFinalBuffer() will return a new data buffer object which uses a byte array; all data will be copied over 
        /// to the new buffer object.</para>
        /// <para>If this object uses a byte array as its internal storage, then it is already a "final" buffer and does not 
        /// need to be replaced. Calling GetFinalBuffer() will simply return the current buffer object.</para>
        /// </remarks>
        /// <returns></returns>
        IDataBuffer GetFinalBuffer();

        /// <summary>
        /// Returns a copy of the byte array associated with this data buffer.
        /// </summary>
        byte[] ReadBytes();

        /// <summary>
        /// Returns a copy of the specified subset of the byte array associated with this data buffer.
        /// </summary>
        /// <param name="start">Byte start index.</param>
        /// <param name="length">Byte read length.</param>
        byte[] ReadBytes(int start, int length);

        /// <summary>
        /// Copies the values of the given bytes to the byte array associated with this data buffer. Buffer lengths must match.
        /// </summary>
        /// <param name="value">The bytes to copy.</param>
        void WriteBytes(byte[] value);

        /// <summary>
        /// Copies the values of the given bytes to the byte array associated with this data buffer, 
        /// starting at <paramref name="startIndex"/> and running to the length of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The bytes to copy.</param>
        /// <param name="startIndex">The location in the destination bytes to start copying.</param>
        void WriteBytes(byte[] value, int startIndex);

        /// <summary>
        /// Copies the values of the given bytes to the byte array associated with this data buffer, 
        /// starting at <paramref name="startIndex"/> and running for <paramref name="count"/> bytes.
        /// </summary>
        /// <param name="value">The source of the bytes to copy.</param>
        /// <param name="startIndex">The location in the destination bytes to start copying.</param>
        /// <param name="count">The number of bytes to copy from.<paramref name="value"/>.</param>
        void WriteBytes(byte[] value, int startIndex, int count);
        #endregion
    }
}
