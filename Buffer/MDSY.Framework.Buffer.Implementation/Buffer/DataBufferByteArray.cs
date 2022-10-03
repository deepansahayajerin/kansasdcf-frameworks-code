using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
using System.ComponentModel;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IDataBuffer.
    /// </summary>
    [InjectionImplementer(typeof(IDataBuffer))]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    internal sealed class DataBufferByteArray : DataBufferBase, IDataBuffer, IDataBufferArrayInitializer
    {
        #region private fields
        private byte[] bytes = null;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the DataBufferByteArray class.
        /// </summary>
        public DataBufferByteArray()
        {
            bytes = null;
        }
        #endregion

        #region internal
        /// <summary>
        /// Returns the bytes of this buffer.
        /// </summary>
        /// <returns>Returns the bytes of this buffer.</returns>
        internal byte[] GetBytes()
        {
            return bytes;
        }

        /// <summary>
        /// Don't expose this method; it's here only so that DataBufferByteList can inject the new byte array in. 
        /// </summary>
        /// <param name="value">The array of bytes to assing.</param>
        internal void SetBytes(byte[] value)
        {
            bytes = value;
        }
        #endregion

        #region override
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
        /// <returns>Returns an IDataBuffer object based onthe byte array.</returns>
        public override IDataBuffer GetFinalBuffer()
        {
            return this;
        }

        /// <summary>
        /// Retrieve the number of data bytes in this object. 
        /// </summary>
        /// <returns>Returns the number of data bytes in this object.</returns>
        protected override int GetLength()
        {
            return bytes.Length;
        }

        /// <summary>
        /// Reads the specified number of the buffer bytes from the specified location.
        /// </summary>
        /// <param name="start">Specifies the start position to read from.</param>
        /// <param name="length">Specifies the number of bytes to read.</param>
        /// <returns>Returns read bytes.</returns>
        protected override byte[] InternalReadBytes(int start, int length)
        {
            byte[] result = new byte[length];
            System.Buffer.BlockCopy(bytes, start, result, 0, length);

            return result;
        }

        /// <summary>
        /// Writes the provided bytes to the buffer at the specified location.
        /// </summary>
        /// <param name="value">The bytes to write.</param>
        /// <param name="startIndex">Specifies the start position.</param>
        /// <param name="count">Specifies the number of bytes to write.</param>
        protected override void InternalWriteBytes(byte[] value, int startIndex, int count)
        {
            System.Buffer.BlockCopy(value, 0, bytes, startIndex, count);
        }
        #endregion

        #region public methods

        /// <summary>
        /// Returns a reference to the current object.
        /// </summary>
        /// <returns>Returns a reference to the current object.</returns>
        public IDataBuffer AsReadOnly()
        {
            return this;
        }

        /// <summary>
        /// Initialized current object data with the provided bytes.
        /// </summary>
        /// <param name="bytes">The bytes to be assign.</param>
        public void InitializeBytes(byte[] bytes)
        {
            SetBytes(bytes);
        }
        #endregion
    }
}
