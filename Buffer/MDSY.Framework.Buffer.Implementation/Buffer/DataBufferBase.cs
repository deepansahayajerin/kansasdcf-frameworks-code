using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Defines data buffer object.
    /// </summary>
    [Serializable]
    internal abstract class DataBufferBase
    {
        #region abstract/virtual

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
        public abstract IDataBuffer GetFinalBuffer();

        /// <summary>
        /// Retrieves the length of the buffer.
        /// </summary>
        /// <returns>Returns the length of the buffer.</returns>
        protected abstract int GetLength();

        /// <summary>
        /// Reads the specified number of the buffer bytes from the specified location.
        /// </summary>
        /// <param name="start">Specifies the start position to read from.</param>
        /// <param name="length">Specifies the number of bytes to read.</param>
        /// <returns>Returns read bytes.</returns>
        protected abstract byte[] InternalReadBytes(int start, int length);

        /// <summary>
        /// Writes the provided bytes to the buffer at the specified location.
        /// </summary>
        /// <param name="value">The bytes to write.</param>
        /// <param name="startIndex">Specifies the start position.</param>
        /// <param name="count">Specifies the number of bytes to write.</param>
        protected abstract void InternalWriteBytes(byte[] value, int startIndex, int count);
        #endregion

        #region public properties
        /// <summary>
        /// Gets the length of the buffer array.
        /// </summary>
        /// <returns>Number of bytes in buffer definition.</returns>
        [Category("IDataBuffer")]
        [Description("Current length of the data buffer byte list.")]
        [ReadOnly(true)]
        public int Length
        {
            get { return GetLength(); }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Returns a copy of the byte array associated with this data buffer.
        /// </summary>
        /// <returns>Returns read bytes.</returns>
        public byte[] ReadBytes()
        {
            return ReadBytes(0, Length);
        }

        /// <summary>
        /// Returns a copy of the specified subset of the byte array associated with this data buffer.
        /// </summary>
        /// <param name="start">Byte start index.</param>
        /// <param name="length">Byte read length.</param>
        public byte[] ReadBytes(int start, int length)
        {
            if (start >= Length && length != 0)
                throw new ArgumentOutOfRangeException("start", "start cannot be greater than or equal to the buffer length.");
            if ((start + length) > Length)
                throw new ArgumentOutOfRangeException("Combination of start and length is greater than the buffer length.");

            return InternalReadBytes(start, length);
        }

        /// <summary>
        /// Copies the values of the given bytes to the byte array associated with this data buffer. Buffer lengths must match.
        /// </summary>
        /// <param name="value">The bytes to copy.</param>
        public void WriteBytes(byte[] value)
        {
            if (value == null || value.Length == 0)
                throw new ArgumentException("Write Bytes Value is Null");
            if (value.Length != Length)
                throw new DataBufferException(
                    String.Format("Data Buffer Length Problem: Buffer Length: {0} Value Length: {1}", Length, value.Length));

            WriteBytes(value, 0, value.Length);
        }

        /// <summary>
        /// Copies the values of the given bytes to the byte array associated with this data buffer, 
        /// starting at <paramref name="startIndex"/> and running to the length of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The bytes to copy.</param>
        /// <param name="startIndex">The location in the destination bytes to start copying.</param>
        public void WriteBytes(byte[] value, int startIndex)
        {
            if (value == null || value.Length == 0)
                throw new ArgumentException("Write Bytes is null", "value");
            if (startIndex < 0)
                throw new IndexOutOfRangeException("startIndex must be zero or greater.");

            WriteBytes(value, startIndex, value.Length);
        }

        /// <summary>
        /// Copies the values of the given bytes to the byte array associated with this data buffer, 
        /// starting at <paramref name="startIndex"/> and running for <paramref name="count"/> bytes.
        /// </summary>
        /// <param name="value">The source of the bytes to copy.</param>
        /// <param name="startIndex">The location in the destination bytes to start copying.</param>
        /// <param name="count">The number of bytes to copy from <paramref name="value"/>.</param>
        public void WriteBytes(byte[] value, int startIndex, int count)
        {
            if (value == null || value.Length == 0)
                throw new ArgumentException("WriteBytes has null value",  "value");
            if (startIndex < 0)
                throw new IndexOutOfRangeException("startIndex must be zero or greater.");

            InternalWriteBytes(value, startIndex, count);
            WriteCount++;

        }

        public long WriteCount { get; private set; }


        #endregion
    }
}
