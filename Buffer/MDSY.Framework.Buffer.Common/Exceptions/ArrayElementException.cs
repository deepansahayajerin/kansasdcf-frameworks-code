using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Exception, which should be thrown when array related issues occur.
    /// </summary>
    [Serializable]
    public class ArrayElementException : Exception
    {
        /// <summary>
        /// Constructs a new ArrayElementExceptionException.
        /// </summary>
        public ArrayElementException() { }
        /// <summary>
        /// Constructs a new ArrayElementExceptionException.
        /// </summary>
        /// <param name="message">The exception message</param>
        public ArrayElementException(string message) : base(message) { }

        /// <summary>
        /// Constructs a new ArrayElementExceptionException.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public ArrayElementException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        protected ArrayElementException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
