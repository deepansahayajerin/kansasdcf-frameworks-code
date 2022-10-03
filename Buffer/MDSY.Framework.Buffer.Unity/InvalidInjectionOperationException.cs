using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Unity
{



    /// <summary>
    /// Thrown in the case of improper interaction with Unity. 
    /// </summary>
    [Serializable]
    public class InvalidInjectionOperationException : Exception
    {
        /// <summary>
        /// Constructs a new InvalidInjectionOperationException.
        /// </summary>
        public InvalidInjectionOperationException() { }
        /// <summary>
        /// Constructs a new InvalidInjectionOperationException.
        /// </summary>
        /// <param name="message">The exception message</param>
        public InvalidInjectionOperationException(string message) : base(message) { }
        /// <summary>
        /// Constructs a new InvalidInjectionOperationException.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public InvalidInjectionOperationException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>
        /// Serialization constructor.
        /// </summary>
        protected InvalidInjectionOperationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

