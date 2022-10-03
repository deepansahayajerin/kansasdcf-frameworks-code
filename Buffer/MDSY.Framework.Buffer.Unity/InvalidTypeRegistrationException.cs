using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Unity
{


    /// <summary>
    /// This exception is thrown in the event of an issue during dynamic type registration loading. 
    /// </summary>
    public class InvalidTypeRegistrationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the InvalidTypeRegistrationException class.
        /// </summary>
        public InvalidTypeRegistrationException(string message, string assemblyName)
            : base(message)
        {
            RegistrationAssembly = assemblyName;
        }

        /// <summary>
        /// Initializes a new instance of the InvalidTypeRegistrationException class.
        /// </summary>
        public InvalidTypeRegistrationException()
        {

        }

        /// <summary>
        /// Initializes a new instance of the InvalidTypeRegistrationException class.
        /// </summary>
        public InvalidTypeRegistrationException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the InvalidTypeRegistrationException class.
        /// </summary>
        public InvalidTypeRegistrationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the InvalidTypeRegistrationException class.
        /// </summary>
        protected InvalidTypeRegistrationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        /// <summary>
        /// Gets or sets the name of the assembly being interacted with at the time of the exception. 
        /// </summary>
        public string RegistrationAssembly { get; set; }


    }
}

