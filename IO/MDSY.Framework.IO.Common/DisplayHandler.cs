using MDSY.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MDSY.Framework.IO.Common
{
    /// <summary>
    /// Handles display to the map and display to the log file actions.
    /// </summary>
    public class DisplayHandler : IDisplayHandler
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="text">Text to be displayed.</param>
        public void Display(string text)
        {
            Console.WriteLine(text);
        }

        /// <summary>
        /// Writes exception message to the log file.
        /// </summary>
        /// <param name="ex">A reference to the current exception object.</param>
        public void DisplayExceptionToLog(Exception ex)
        {
            //EventLog.WriteEntry("ConversionCode", ex.Message, EventLogEntryType.Error);
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="text">Text to be written to the log file.</param>
        public void DisplayToLog(string text)
        {
            Console.WriteLine(text);
            //EventLog.WriteEntry("ConversionCode", text, EventLogEntryType.Information);
        }

    }
}
