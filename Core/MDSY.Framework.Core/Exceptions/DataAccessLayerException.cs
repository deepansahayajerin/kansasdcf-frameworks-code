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
    public class DataAccessLayerException: Exception
    {
        /// <summary>
        /// Constructs a new DataAccessLayerException.
        /// </summary>
        public DataAccessLayerException() { }
        /// <summary>
        /// Constructs a new DataAccessLayerException.
        /// </summary>
        /// <param name="message">The exception message</param>
        public DataAccessLayerException(string message) : base(message) { }

        /// <summary>
        /// Constructs a new DataAccessLayerException.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public DataAccessLayerException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Constructs a new DataAccessLayerException for serialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DataAccessLayerException(SerializationInfo info, StreamingContext context) : base(info, context) { }


    }
}
