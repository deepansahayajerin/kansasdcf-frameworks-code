using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Thrown when a CompressedNumericBase-descendant is given an invalid value type.
    /// </summary>
    [Serializable]
    public class InvalidCompressedNumberTypeException : Exception
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of InvalidCompressedNumberTypeException with default properties.
        /// </summary>
        public InvalidCompressedNumberTypeException() { }

        /// <summary>
        /// Initializes a new instance of InvalidCompressedNumberTypeException with a specified error <paramref name="message"/>
        /// </summary>
        /// <param name="message">The exception message</param>
        public InvalidCompressedNumberTypeException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of InvalidCompressedNumberTypeException with a specified error <paramref name="message"/> and a 
        /// reference to the <paramref name="innerException"/> that is the cause of this exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public InvalidCompressedNumberTypeException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of InvalidCompressedNumberTypeException with serialized data.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual
        /// information about the source or destination.</param>
        protected InvalidCompressedNumberTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion
    }



}

