using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Common
{

    /// <summary>
    /// Thrown in cases specific to DataBuffer operations.
    /// </summary>
    public class DataBufferException : Exception
    {
        /// <summary>
        /// Creates and initializes a new instance of the DataBufferException class.
        /// </summary>
        /// <param name="message">Message text.</param>
        public DataBufferException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates and initializes a new instance of the DataBufferException class.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="innerException">A reference to the inner exception object.</param>
        public DataBufferException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// Creates and initializes a new instance of the DataBufferException class.
        /// </summary>
        public DataBufferException()
        {

        }

        /// <summary>
        /// Creates and initializes a new instance of the DataBufferException class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual
        /// information about the source or destination.</param>
        protected DataBufferException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }


    }
}

