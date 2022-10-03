using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// An exception that is thrown in the event of an invalid operation when interacting with a field
    /// collection (IElementCollection). 
    /// </summary>
    public class ElementCollectionException : Exception
    {
        /// <summary>
        /// Creates and initializes a new instance of the ElementCollectionException class.
        /// </summary>
        /// <param name="message">Message text.</param>
        public ElementCollectionException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates and initializes a new instance of the ElementCollectionException class. 
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="innerException">A reference to the inner exception object.</param>
        public ElementCollectionException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

    }
}

