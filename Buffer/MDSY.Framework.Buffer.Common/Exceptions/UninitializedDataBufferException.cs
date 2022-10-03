using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Thrown in cases where an attempt is made to access a Data Buffer that has not yet been initialized.
    /// </summary>
    public class UninitializedDataBufferException : DataBufferException
    {
        /// <summary>
        /// Creates and initializes a new instance of the UnitializedDataBufferException class.
        /// </summary>
        /// <param name="message">Message text.</param>
        public UninitializedDataBufferException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates and initializes a new instance of the UnitializedDataBufferException class.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="innerException">A reference to the inner exception object.</param>
        public UninitializedDataBufferException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}

