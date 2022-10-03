using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Common
{

    /// <summary>
    /// An exception that is thrown in the case of an invalid Field Value operation.
    /// </summary>
    [Serializable]
    public class FieldValueException : Exception
    {
        #region constructors
        /// <summary>
        /// Constructs a new FieldValueException.
        /// </summary>
        public FieldValueException() { }

        /// <summary>
        /// Constructs a new FieldValueException.
        /// </summary>
        /// <param name="message">The exception message</param>
        public FieldValueException(string message) : base(message) { }

        /// <summary>
        /// Constructs a new FieldValueException.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public FieldValueException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Creates and initializes a new instance of teh FieldValue exception class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual
        /// information about the source or destination.</param>
        protected FieldValueException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion
    }
}

