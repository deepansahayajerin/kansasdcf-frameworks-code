using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// An exception that is thrown in the event of an invalid operation when interacting with a record structure. 
    /// </summary>
    [Serializable]
    public class RecordStructureException : Exception
    {
        #region constructors
        /// <summary>
        /// Constructs a new RecordStructureException.
        /// </summary>
        public RecordStructureException() { }

        /// <summary>
        /// Constructs a new RecordStructureException.
        /// </summary>
        /// <param name="message">The exception message</param>
        public RecordStructureException(string message) : base(message) { }

        /// <summary>
        /// Constructs a new RecordStructureException.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public RecordStructureException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual
        /// information about the source or destination.</param>
        protected RecordStructureException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion

    }
}
