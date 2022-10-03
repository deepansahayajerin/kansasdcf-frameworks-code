using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
using System.ComponentModel;
//CHADusing Unity.Attributes;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IDataBuffer for SetBufferPointer()
    /// operations.
    /// </summary>
    /// <remarks>
    /// <para>Supports the Framework's IField extension methods: SetBufferPointer() and
    /// SetBufferReference() by providing a IDataBuffer implementation that 
    /// manages pointing a IField's buffer to a buffer that is not owns by the 
    /// field's own record.</para>
    /// <para>This is in support of the COBOL calls like <c>SET A TO ADDRESS OF B</c>.
    /// </para>
    /// </remarks>
    [InjectionImplementer(typeof(IDataBuffer))]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    internal sealed class DataBufferRedirectionPipeline : DataBufferBase, IDataBuffer
    {
        #region private fields
        private int positionInTargetBuffer = -1;
        private IDataBuffer targetBuffer = null;
        #endregion

        #region overrides

        /// <summary>
        /// Returns a reference to the current object.
        /// </summary>
        /// <returns>Returns a reference to the current object.</returns>
        public override IDataBuffer GetFinalBuffer()
        {
            return this;
        }

        /// <summary>
        /// Returns the number of bytes in the TargetBuffer.
        /// </summary>
        /// <returns>Returns the number of bytes in the TargetBuffer.</returns>
        protected override int GetLength()
        {
            return TargetBuffer.Length;
        }

        /// <summary>
        /// Reads the specified number of the buffer bytes from the specified location.
        /// </summary>
        /// <param name="start">Specifies the start position to read from.</param>
        /// <param name="length">Specifies the number of bytes to read.</param>
        /// <returns>Returns read bytes.</returns>
        protected override byte[] InternalReadBytes(int start, int length)
        {
            // start is overridden
            //return TargetBuffer.ReadBytes(PositionInTargetBuffer, length);
            return TargetBuffer.ReadBytes(start, length);
        }

        /// <summary>
        /// Writes the provided bytes to the buffer at the specified location.
        /// </summary>
        /// <param name="value">The bytes to write.</param>
        /// <param name="startIndex">Specifies the start position.</param>
        /// <param name="count">Specifies the number of bytes to write.</param>
        protected override void InternalWriteBytes(byte[] value, int startIndex, int count)
        {
            // startIndex is overridden
           // TargetBuffer.WriteBytes(value, PositionInTargetBuffer, count);
            TargetBuffer.WriteBytes(value, startIndex, count);
        }
        #endregion

        #region private methods
        private static IDataBuffer GetTargetBuffer(IBufferElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element", "element is null.");
            if (!(element is IBufferValue))
                throw new ArgumentException("element is not of type IBufferValue - cannot get buffer.", "element");

            return (element as IBufferValue).Buffer;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Sets and returns a reference to the buffer element object.
        /// </summary>
        public IBufferElement TargetElement { get; set; }

        /// <summary>
        /// Returns a position of the target element in the buffer.
        /// </summary>
        public int PositionInTargetBuffer
        {
            // This buffer can't really function as a true IDataBuffer replacement. 
            // It implements IDataBuffer only for individual fields whose buffers 
            // have been readdressed. 
            get
            {
                if (positionInTargetBuffer < 0)
                {
                    positionInTargetBuffer = TargetElement.PositionInBuffer;
                }
                return positionInTargetBuffer;
            }
        }

        /// <summary>
        /// Returns a reference to the target data buffer.
        /// </summary>
        public IDataBuffer TargetBuffer
        {
            get
            {
                if (targetBuffer == null)
                {
                    targetBuffer = GetTargetBuffer(TargetElement);
                }
                return targetBuffer;
            }
        }
        
        #endregion

    }
}
