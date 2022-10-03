using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// An exception that should be thrown for the issues with field arrays.
    /// </summary>
    public class FieldArrayException : Exception
    {
        /// <summary>
        /// Creates and initializes a new instance of the FieldArrayException class.
        /// </summary>
        public FieldArrayException()
        {

        }

        /// <summary>
        /// Creates and initializes a new instance of the FieldArrayException class.
        /// </summary>
        /// <param name="message">Message text</param>
        public FieldArrayException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates and initializes a new instance of the FieldArrayException class.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="innerException">A reference to the inner exception object.</param>
        public FieldArrayException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// Creates and initializes a new instance of the FieldArrayException class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual
        /// information about the source or destination.</param>
        protected FieldArrayException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {

        }
    }
}
