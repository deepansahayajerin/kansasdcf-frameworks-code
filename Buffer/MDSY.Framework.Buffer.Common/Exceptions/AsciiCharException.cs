using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Exception, which should be thrown when ASCII character issues occur.
    /// </summary>
    [Serializable]
    public class AsciiCharException : Exception
    {
        /// <summary>
        /// Default constructor. Creates a new instance of the AsciiCharException class.
        /// </summary>
        public AsciiCharException()
        {

        }

        /// <summary>
        /// Creats and initializes a new instance of the AsciiCharException class.
        /// </summary>
        /// <param name="message">Message text.</param>
        public AsciiCharException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creats and initializes a new instance of the AsciiCharException class.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="innerException">A reference to the inner exception object.</param>
        public AsciiCharException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }

        /// <summary>
        /// Creats and initializes a new instance of the AsciiCharException class.
        /// </summary>
        /// <param name="info">A reference to the serialization information object.</param>
        /// <param name="context">A reference to the streaming context object.</param>
        protected AsciiCharException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }

        /// <summary>
        /// Creats and initializes a new instance of the AsciiCharException class.
        /// </summary>
        /// <param name="innerException">A reference to the inner exception object.</param>
        /// <param name="formatMsg">Formatting message.</param>
        /// <param name="args">Message parameters.</param>
        public AsciiCharException(Exception innerException, string formatMsg, params object[] args)
            : base(string.Format(formatMsg, args), innerException)
        {
        }


    }
}
