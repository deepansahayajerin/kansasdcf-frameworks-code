using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Provides Message text
    /// </summary>
    public class LogMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Returns the message text
        /// </summary>
        /// <param name="messageText">Message text</param>
        public LogMessageEventArgs(string messageText)
        {
            MessageText = messageText;

        }
        /// <summary>
        /// Initializes a new instance of LogMessageEventArgs class.
        /// </summary>
        public LogMessageEventArgs()
        {

        }
        /// <summary>
        /// Returns the message text
        /// </summary>
        public string MessageText { get; set; }

    }
}
