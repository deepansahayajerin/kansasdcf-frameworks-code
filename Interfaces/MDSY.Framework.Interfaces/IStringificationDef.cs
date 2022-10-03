using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Interfaces
{
    /// <summary>
    /// Defines an object that can concatenate a single string from a list of
    /// strings and a delimiter string.
    /// </summary>
    /// <remarks>
    /// The delimiter string is used by GetStringification() to define how much 
    /// of each string will be taken in building the concatenation. 
    /// </remarks>
    public interface IStringificationDef
    {
        /// <summary>
        /// gets the list of texts
        /// </summary>
        IList<string> Texts { get; }
        /// <summary>
        /// gets the delimiter string
        /// </summary>
        string Delimiter { get; }
        /// <summary>
        /// returns the concatination of the different sections of the strings 
        /// </summary>
        /// <returns></returns>
        string GetStringification();
    }
}

