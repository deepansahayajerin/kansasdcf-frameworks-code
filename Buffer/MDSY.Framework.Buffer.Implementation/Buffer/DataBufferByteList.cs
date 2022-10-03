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
    [InjectionImplementer(typeof(IDataBuffer), "InitialDataBuffer")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    internal sealed class DataBufferByteList : DataBufferBase, IDataBuffer
    {
        #region private fields
        private List<byte> bytes;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the ListBasedDataBuffer class.
        /// </summary>
        public DataBufferByteList()
        {
            bytes = new List<byte>();
        }

        /// <summary>
        /// Resets the internal data bytes with the provided bytes.
        /// </summary>
        /// <param name="byteArray">The bytes to assign.</param>
        public void ResetByteList(byte[] byteArray)
        {
            bytes = new List<byte>(Enumerable.Repeat((byte)0x20, byteArray.Length).ToArray());
        }
        #endregion

        #region overrides
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
        public override IDataBuffer GetFinalBuffer()
        {
            return ObjectFactory.Factory.NewDataBufferByteArrayObject(bytes);
        }

        /// <summary>
        /// Retrieves the number of the data bytes in the current object.
        /// </summary>
        /// <returns>Returns the number of the data bytes in the current object.</returns>
        protected override int GetLength()
        {
            return bytes.Count;
        }

        /// <summary>
        /// Reads the specified number of the buffer bytes from the specified location.
        /// </summary>
        /// <param name="start">Specifies the start position to read from.</param>
        /// <param name="length">Specifies the number of bytes to read.</param>
        /// <returns>Returns read bytes.</returns>
        protected override byte[] InternalReadBytes(int start, int length)
        {
            //Performance update - 08-2019
            var result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = bytes[i + start];
            }
            return result;
            //return bytes.Skip(start).Take(length).ToArray();

        }

        /// <summary>
        /// Writes the provided bytes to the buffer at the specified location.
        /// </summary>
        /// <param name="value">The bytes to write.</param>
        /// <param name="startIndex">Specifies the start position.</param>
        /// <param name="count">Specifies the number of bytes to write.</param>
        protected override void InternalWriteBytes(byte[] value, int startIndex, int count)
        {
            if (bytes != null)
            {
                if (startIndex + count <= bytes.Count)
                {
                    for (int i = 0; i < count; i++)
                    {
                        bytes[startIndex + i] = value[i];
                    }
                }
                else if (startIndex == bytes.Count)
                {
                    bytes.AddRange(value.Take(count));
                }
                else
                {
                    throw new DataBufferException(string.Format("Attempted to write to non-contiguous bytes in the buffer during record definition. Trying to write at index {0}, buffer length (1).", startIndex, bytes.Count));
                }
            }

            //bytes = bytes
            //        .Take(startIndex)
            //        .Concat(value.Take(count))
            //        .Concat(bytes.Skip(startIndex + count))
            //        .ToList();
        }
        #endregion

    }
}
