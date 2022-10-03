using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which maintains a value within the buffer.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface IBufferValue : IAssignable
    {
        #region attributes
        /// <summary>
        /// Returns a copy of the value of this object as a byte array. 
        /// </summary>
        /// <returns>A new byte[].</returns>
        byte[] AsBytes { get; }

        /// <summary>
        /// The buffer object to and from which values are normally stored and retrieved. 
        /// </summary>
        IDataBuffer Buffer { get; set; }

        /// <summary>
        /// Returns the string representation of this object's internal byte value, 
        /// excluding bytes referenced by IRedefinition objects.
        /// </summary>
        string BytesAsString { get; }

        /// <summary>
        /// Returns the string representation of this object's internal byte value, 
        /// including bytes reference by IRedefinition objects.
        /// </summary>
        string RedefinedBytesAsString { get; }

        /// <summary>
        /// Returns the string representation of this object's stringvalue.
        /// </summary>
        string DisplayValue { get; }

        #endregion

        #region operations

        /// <summary>
        /// Sets the field value object's internal bytes to null bytes (0x00). 
        /// </summary>
        void Clear();

        /// <summary>
        /// Sets the field value object's internal bytes all to the given <paramref name="clearByte"/>. 
        /// </summary>
        /// <param name="clearByte">Specifies the value of the byte to be used</param>
        void Clear(byte clearByte);

        /// <summary>
        /// Causes the object to restore its value (or its children's values) to its original data.
        /// </summary>
        void ResetToInitialValue();

        /// <summary>
        /// Initialize value with hex 00 unless default value has been supplied
        /// </summary>
        void InitializeWithLowValues();

        /// <summary>
        /// Set the object's internal bytes to the passed bytes
        /// </summary>
        /// <param name="valueBytes"></param>
        [Obsolete("This is redundant with IAssignable.AssignFrom(bytes); if you need this, talk to Robert about why.", false)]
        void SetBytes(byte[] valueBytes);


        #endregion


    }
}
