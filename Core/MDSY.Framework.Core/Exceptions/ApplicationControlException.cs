using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Exception thrown with Application Control error
    /// </summary>
    [Serializable]
    public class ApplicationControlException: Exception
    {
        /// <summary>
        /// Constructs a new ApplicationControlException.
        /// </summary>
        public ApplicationControlException() { }
        /// <summary>
        /// Constructs a new ApplicationControlException.
        /// </summary>
        /// <param name="message">The exception message</param>
        public ApplicationControlException(string message) : base(message) { }

        /// <summary>
        /// Constructs a new ApplicationControlException.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public ApplicationControlException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Constructs a new ApplicationControlException for serialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ApplicationControlException(SerializationInfo info, StreamingContext context) : base(info, context) { }


    }
}
