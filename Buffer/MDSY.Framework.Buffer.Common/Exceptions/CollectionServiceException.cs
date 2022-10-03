using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// An exception that is thrown in the event of an invalid operation when 
    /// interacting with a collection service.
    /// </summary>
    public class CollectionServiceException : Exception
    {
        /// <summary>
        /// Creates and initializes a new instance of the CollectionServiceException class.
        /// </summary>
        /// <param name="message">Message text.</param>
        public CollectionServiceException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates and initializes a new instance of the CollectionServiceException class.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="innerException">A reference to the inner exception object.</param>
        public CollectionServiceException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}

