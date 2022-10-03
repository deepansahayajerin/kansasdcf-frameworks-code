using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MDSY.Framework.Interfaces
{
    /// <summary>
    /// Defines an object which can send messages, warnings, and errors to a log. 
    /// </summary>
    /// <remarks>
    /// The LoggerExtensions class provides some default logging operations for any classes 
    /// which implement ILogger. 
    /// </remarks>
    /// It gets prefix and suffix.
    public interface ILogger
    {
        string Prefix { get; }
        string Suffix { get; }
    }
}

