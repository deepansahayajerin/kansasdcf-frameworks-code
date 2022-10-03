using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// An exception that is thrown specific to issues with PackedDecimal values.
    /// </summary>
    [Serializable]
    public class PackedDecimalException : Exception
    {
        #region constructors
        /// <summary>
        /// Constructs a new PackedDecimalException.
        /// </summary>
        public PackedDecimalException() { }

        /// <summary>
        /// Constructs a new PackedDecimalException.
        /// </summary>
        /// <param name="message">The exception message</param>
        public PackedDecimalException(string message) : base(message) { }

        /// <summary>
        /// Constructs a new PackedDecimalException.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public PackedDecimalException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual
        /// information about the source or destination.</param>
        protected PackedDecimalException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion
    }
}
