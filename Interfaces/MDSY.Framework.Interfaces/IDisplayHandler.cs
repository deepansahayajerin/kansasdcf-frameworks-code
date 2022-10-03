using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Interfaces
{
    /// <summary>
    /// Defines an object which provides implementation for handling DISPLAY statements.
    /// </summary>
    public interface IDisplayHandler
    {
        /// <summary>
        /// Writes the given text to the implementing object's log. 
        /// Analogous to the DISPLAY TO LOG statement. 
        /// </summary>
        /// <param name="text"></param>
        void DisplayToLog(string text);

        /// <summary>
        /// Writes the given text to the SYSOUT
        /// Analogous to the DISPLAY   statement. 
        /// </summary>
        /// <param name="text"></param>
        void Display(string text);

        /// <summary>
        /// Sends exception information to the log
        /// </summary>
        /// <param name="ex"></param>
        void DisplayExceptionToLog(Exception ex);

    }
}
