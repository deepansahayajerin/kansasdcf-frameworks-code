using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using MDSY.Framework.Interfaces;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Extension methods for ILogger.
    /// </summary>
    public static class LoggerExtensions
    {
        #region constants
        private const string STR_Debug = "Debug";
        private const string STR_Error = "Error";
        private const string STR_Warning = "Warning";
        private const string STR_Message = "Info";
        #endregion

        #region private methods
        /// <summary>
        /// Returns a string conditionally built in the format: 
        /// "{<paramref name="prefx"/>}: {<paramref name="msgType"/>} - {<paramref name="message"/>} - {<paramref name="suffix"/>}"
        /// </summary>
        private static string BuildMessage(string message, string msgType, string prefix, string suffix)
        {
            var result = new StringBuilder();

            if (!String.IsNullOrEmpty(prefix))
            {
                result.AppendFormat("{0}: ", prefix);
            }

            if (!String.IsNullOrEmpty(msgType))
            {
                result.AppendFormat("{0} - ", msgType);
            }

            result.Append(message);

            if (!String.IsNullOrEmpty(suffix))
            {
                result.AppendFormat(" - {0}", suffix);
            }

            return result.ToString();
        }

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="message">Message to send</param>
        private static void Send(string message)
        {
            Console.WriteLine(message);
        }

        #endregion


        #region public methods
        /// <summary>
        /// Sends the debug message <paramref name="message"/> to the log. If <paramref name="args"/>
        /// contains values, those values are formatted into <paramref name="message"/> before sending.
        /// </summary>
        /// <param name="instance">The log instance</param>
        /// <param name="message">Debug message to be sent</param>
        /// <param name="args">Values to be included in the message</param>
        [Conditional("DEBUG")]
        public static void SendDebugMessage(this ILogger instance, string message, params object[] args)
        {
            //if (instance.IsDebugging)
            //{
            string msg = args.Length > 0 ?
                             String.Format(message, args) :
                             message;
            msg = BuildMessage(msg, STR_Debug, instance.Prefix, instance.Suffix);
            Send(msg);
            //}
        }

        /// <summary>
        /// Sends the given error <paramref name="message"/> to the log. If <paramref name="args"/>
        /// contains values, those values are formatted into <paramref name="message"/> before sending.
        /// </summary>
        /// <param name="instance">The log instance</param>
        /// <param name="message">Error message to be sent</param>
        /// <param name="args">Values to be included in the message</param>
        public static void SendError(this ILogger instance, string message, params object[] args)
        {
            string msg = args.Length > 0 ?
                             String.Format(message, args) :
                             message;
            msg = BuildMessage(msg, STR_Error, instance.Prefix, instance.Suffix);
            Send(msg);
        }

        /// <summary>
        /// Sends the given warning <paramref name="message"/> to the log. If <paramref name="args"/>
        /// contains values, those values are formatted into <paramref name="message"/> before sending.
        /// </summary>
        /// <param name="instance">The log instance</param>
        /// <param name="message">Warning message to be sent</param>
        /// <param name="args">Values to be included in the message</param>
        public static void SendWarning(this ILogger instance, string message, params object[] args)
        {
            string msg = args.Length > 0 ?
                             String.Format(message, args) :
                             message;
            msg = BuildMessage(msg, STR_Warning, instance.Prefix, instance.Suffix);
            Send(msg);
        }

        /// <summary>
        /// Sends the given info <paramref name="message"/> to the log. If <paramref name="args"/>
        /// contains values, those values are formatted into <paramref name="message"/> before sending.
        /// </summary>
        /// <param name="instance">The log instance</param>
        /// <param name="message">Message to be sent</param>
        /// <param name="args">Values to be included in the message</param>
        public static void SendMessage(this ILogger instance, string message, params object[] args)
        {
            string msg = args.Length > 0 ?
                             String.Format(message, args) :
                             message;
            msg = BuildMessage(msg, STR_Message, instance.Prefix, instance.Suffix);
            Send(msg);
        }
        #endregion
    }
}

