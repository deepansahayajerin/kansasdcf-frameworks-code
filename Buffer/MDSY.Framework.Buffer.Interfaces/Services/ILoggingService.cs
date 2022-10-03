using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Provides access to logging services.
    /// </summary>
    [InjectionInterface]
    public interface ILoggingService
    {
        /// <summary>
        /// Sends the given error text.
        /// </summary>
        /// <param name="errorText">The error text to be sent.</param>
        void SendError(string errorText);

        /// <summary>
        /// Sends the given warning text.
        /// </summary>
        /// <param name="warningText">The warning text to be sent.</param>
        void SendWarning(string warningText);

        /// <summary>
        /// Indents the current message stack one level, with the given text.
        /// </summary>
        void IndentStack(string frameName);

        /// <summary>
        /// Outdents the current message stack one level, with the given text.
        /// </summary>
        void OutdentStack(string frameName);

        /// <summary>
        /// Sends the given <paramref name="value"/> to the log, with the given <paramref name="message"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to be logged.</typeparam>
        /// <param name="message">The message text to accompany the <paramref name="value"/>.</param>
        /// <param name="value">The value to be logged.</param>
        [Obsolete("Use overloads instead", true)]
        void Send<T>(string message, T value);

        /// <summary>
        /// Sends the given <paramref name="value"/> to the log, with the given <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message text to accompany the <paramref name="value"/>.</param></param>
        /// <param name="value">The value to be logged.</param>
        void Send(string message, string value);

        /// <summary>
        /// Sends the given <paramref name="value"/> to the log, with the given <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message text to accompany the <paramref name="value"/>.</param></param>
        /// <param name="value">The value to be logged.</param>
        void Send(string message, bool value);

        /// <summary>
        /// Sends the given <paramref name="value"/> to the log, with the given <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message text to accompany the <paramref name="value"/>.</param></param>
        /// <param name="value">The value to be logged.</param>
        void Send(string message, int value);

        /// <summary>
        /// Sends the given <paramref name="value"/> to the log, with the given <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message text to accompany the <paramref name="value"/>.</param></param>
        /// <param name="value">The value to be logged.</param>
        void Send(string message, object value);


        /// <summary>
        /// Sends the given <paramref name="value"/> to the log, with the given
        /// <paramref name="message"/> and <paramref name="priority"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to be logged.</typeparam>
        /// <param name="priority">The relative priority of the message.</param>
        /// <param name="message">The message text to accompany the <paramref name="value"/>.</param>
        /// <param name="value">The value to be logged.</param>
        void Send<T>(MessagePriority priority, string message, T value);

        void SendException(Exception ex);

        /// <summary>
        /// Sends the given <paramref name="message"/> to the log.
        /// </summary>
        /// <param name="message">The text to be logged.</param>
        void SendMsg(string message);

        /// <summary>
        /// Sends the given <paramref name="message"/> to the log with the specified
        /// <paramref name="priority"/>.
        /// </summary>
        /// <param name="priority">The relative priority of the message.</param>
        /// <param name="message">The text to be logged.</param>
        void SendMsg(MessagePriority priority, string message);

        /// <summary>
        /// Starts the section with the given <paramref name="secName"/>.
        /// </summary>
        /// <param name="secName">Section name</param>
        void StartSection(string secName);

        /// <summary>
        /// Adds a marker
        /// </summary>
        void AddMarker();

        /// <summary>
        /// Adds a separator
        /// </summary>
        void AddSeparator();

        /// <summary>
        /// Clears log entries.
        /// </summary>
        void Clear();

        /// <summary>
        /// Save the log with the give file name.
        /// </summary>
        /// <param name="filename">The file name used to save the log.</param>
        void SaveLogAs(string filename);
    }
}
