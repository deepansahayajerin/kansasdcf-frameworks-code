using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
//CHADusing Unity.Attributes;
using System.Text;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface ILoggingService.
    /// </summary>
    [InjectionImplementer(typeof(ILoggingService))]
    internal sealed class TextLoggingService : ILoggingService
    {
        #region private fields

       
        private StringBuilder log = null;

        private int IndentLevel
        {
            get { return indentLevel; }
            set
            {
                if (indentLevel != value)
                {
                    var chars = Enumerable.Repeat(indentChar, IndentLevel * numCharsInIndent);
                    currentIndention = new string(chars.ToArray());
                    indentLevel = value;
                }
            }
        }
        private StringBuilder Log
        {
            get
            {
                // Create on demand...
                if (log == null)
                    log = new StringBuilder();
                return log;
            }
        }

        private int indentLevel = 0;
        private char indentChar = ' ';
        private int markerCount = 0;
        private int numCharsInIndent = 2;
        private string currentIndention = string.Empty;
        #endregion


        #region private methods

        private void DecIndent(int decBy = 1)
        {
            IndentLevel = Math.Max(IndentLevel - decBy, 0);
        }

        private void IncIndent(int incBy = 1)
        {
            IndentLevel = IndentLevel + incBy;
        }

        private void InsertTextLine(string text)
        {
            // skips time stamp, indention, etc.
            Log.AppendLine(text);
        }

        private void LogText(string text, MessagePriority priority = MessagePriority.Medium)
        {
            string priorityStr = priority != MessagePriority.Medium ?
                                    String.Format("<{0}>", (int)priority) :
                                    "   ";
            string logText = string.Format("{2} {3}: {0}{1}", currentIndention, text, DateTime.Now.ToString("HH:mm:ss.fff"), priorityStr);
            Log.AppendLine(logText);
            Console.WriteLine(logText);
        }

        private void SendValue<T>(string message, T value, MessagePriority priority)
        {
            LogText(string.Format("{0} -> {1}", message, value.ToString()), priority);
        }
        #endregion

        #region public methods

        /// <summary>
        /// Sends error with the given <paramref name="errorText"/>.
        /// </summary>
        /// <param name="errorText">The error text.</param>
        public void SendError(string errorText)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Sends a warning with the give <paramref name="warningText"/>
        /// </summary>
        /// <param name="warningText"></param>
        public void SendWarning(string warningText)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Increases indent for the given <paramref name="frameName"/>.
        /// </summary>
        /// <param name="frameName">The name of the frame for the indent.</param>
        public void IndentStack(string frameName)
        {
            IncIndent();
            LogText(frameName);
        }

        /// <summary>
        /// Decreases indent for the given <paramref name="frameName"/>.
        /// </summary>
        /// <param name="frameName">The name of the frame for the indent decrease.</param>
        public void OutdentStack(string frameName)
        {
            LogText(frameName);
            DecIndent();
        }

        /// <summary>
        /// Sends the given <paramref name="value"/> to the log, with the given <paramref name="message"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to be logged.</typeparam>
        /// <param name="message">The message text to accompany the <paramref name="value"/>.</param>
        /// <param name="value">The value to be logged.</param>
        public void Send<T>(string message, T value)
        {
            Send(MessagePriority.Medium, message, value);
        }

        /// <summary>
        /// Sends the given <paramref name="value"/> to the log, with the given
        /// <paramref name="message"/> and <paramref name="priority"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to be logged.</typeparam>
        /// <param name="priority">The relative priority of the message.</param>
        /// <param name="message">The message text to accompany the <paramref name="value"/>.</param>
        /// <param name="value">The value to be logged.</param>
        public void Send<T>(MessagePriority priority, string message, T value)
        {
            SendValue(message, value, priority);
        }

        /// <summary>
        /// Sends the execption with the given <paramref name="ex"/> to the log,
        /// </summary>
        /// <param name="ex">The exception text.</param>
        public void SendException(Exception ex)
        {
            SendValue("Exception", ex.Message, MessagePriority.Higher);
            Exception innerEx = ex.InnerException;
            int i = 0;
            while (innerEx != null)
            {
                IncIndent();
                i++;
                SendValue("InnerException", innerEx.Message, MessagePriority.Higher);
                innerEx = innerEx.InnerException;
            }
            DecIndent(i);
        }

        /// <summary>
        /// Sends the given <paramref name="message"/> to the log.
        /// </summary>
        /// <param name="message"></param>
        public void SendMsg(string message)
        {
            SendMsg(MessagePriority.Medium, message);
        }

        /// <summary>
        /// Sends the given <paramref name="mesgae"/> and the given
        /// <paramref name="priority"/> to the log.
        /// </summary>
        /// <param name="priority">The relative priority of the message.</param>
        /// <param name="message">The message text to be sent.</param>
        public void SendMsg(MessagePriority priority, string message)
        {
            SendValue("<MSG>", message, priority);
        }

        /// <summary>
        /// Sends the name for the section.
        /// </summary>
        /// <param name="secName">Section name.</param>
        public void StartSection(string secName)
        {
            var leadChars = Enumerable.Repeat('_', 10);
            var trailChars = Enumerable.Repeat('_', 68 - secName.Length);

            string text = String.Format("{0} {1} {2}", new string(leadChars.ToArray()), secName, new string(trailChars.ToArray()));
            InsertTextLine(text);
            IndentLevel = 0;
        }

        /// <summary>
        /// Adds a Marker.
        /// </summary>
        public void AddMarker()
        {
            markerCount++;
            SendValue("<MKR>", String.Format("#{0:000}", markerCount), MessagePriority.Medium);
        }

        /// <summary>
        /// Adds a separator.
        /// </summary>
        public void AddSeparator()
        {
            var chars = Enumerable.Repeat('-', 80);
            string text = new string(chars.ToArray());
            InsertTextLine(text);
        }

        /// <summary>
        /// Saves the log with the given <paramref name="filename"/>.
        /// </summary>
        /// <param name="filename">The name of the log file to be saved.</param>
        public void SaveLogAs(string filename)
        {
            System.IO.File.WriteAllText(filename, Log.ToString());
        }

        /// <summary>
        /// Clears the log, indent level and the marker counter.
        /// </summary>
        public void Clear()
        {
            Log.Clear();
            IndentLevel = 0;
            markerCount = 0;
        }

        /// <summary>
        /// Sends the given <paramref name="value"/> to the log, with the given <paramref name="message"/> as medium priority.
        /// </summary>
        /// <param name="message">The message text to accompany the <paramref name="value"/>.</param>
        /// <param name="value">The value to be logged.</param>
        public void Send(string message, string value)
        {
            Send(MessagePriority.Medium, message, value);
        }

        /// <summary>
        /// Sends the given <paramref name="value"/> to the log, with the given <paramref name="message"/> as medium priority.
        /// </summary>
        /// <param name="message">The message text to accompany the <paramref name="value"/>.</param>
        /// <param name="value">The value to be logged.</param>
        public void Send(string message, bool value)
        {
            Send(MessagePriority.Medium, message, value);
        }

        /// <summary>
        /// Sends the given <paramref name="value"/> to the log, with the given <paramref name="message"/> as medium priority.
        /// </summary>
        /// <param name="message">The message text to accompany the <paramref name="value"/>.</param>
        /// <param name="value">The value to be logged.</param>
        public void Send(string message, int value)
        {
            Send(MessagePriority.Medium, message, value);
        }

        /// <summary>
        /// Sends the given <paramref name="value"/> to the log, with the given <paramref name="message"/> as medium priority.
        /// </summary>
        /// <param name="message">The message text to accompany the <paramref name="value"/>.</param>
        /// <param name="value">The value to be logged.</param>
        public void Send(string message, object value)
        {
            Send(MessagePriority.Medium, message, value);
        }

        #endregion
    }
}
