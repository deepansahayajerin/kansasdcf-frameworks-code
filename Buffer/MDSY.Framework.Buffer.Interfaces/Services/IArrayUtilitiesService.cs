using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Provides utilities for managing arrays and array elements.
    /// </summary>
    [InjectionInterface]
    public interface IArrayUtilitiesService
    {
        /// <summary>
        /// Returns the list of indexes built into the given <paramref name="elementName"/> via index suffixes.
        /// For nested indexes, parent indexes are first. 
        /// </summary>
        /// <example>
        /// Where <c>elementName == "myElement 3 0 12"</c>, GetElementIndexes() would return an IEnumerable(of int)
        /// thus:
        /// <code>
        /// result[0] = 3
        /// result[1] = 0
        /// result[2] = 12
        /// </code>
        /// </example>
        /// <param name="elementName">The name which is to be processed.</param>
        /// <param name="baseName">returns the "name" part of <paramref name="elementName"/>, without the indexes.</param>
        /// <returns>A collection of numeric indexes, if any are built into the given <paramref name="elementName"/>.</returns>
        IEnumerable<int> GetElementIndexes(string elementName, out string baseName);

        /// <summary>
        /// Constructs an appropriate array element name from the given <paramref name="baseName"/> and 
        /// <paramref name="singleLevelIndex"/>. Returns a string in the form "ArrayName 0"
        /// </summary>
        /// <param name="baseName">the "name" part of the element name, without the index.</param>
        /// <param name="singleLevelIndex">The "index" part of the element name.</param>
        string MakeElementName(string baseName, int singleLevelIndex);

        /// <summary>
        /// Constructs an appropriate multi-index array element name from the given <paramref name="baseName"/> and 
        /// <paramref name="indexes"/>. Returns a string in the form "ArrayName 0 1 2..."
        /// </summary>
        /// <param name="baseName">the "name" part of the element name, without the indexes.</param>
        /// <param name="indexes">The "indexes" part of the element name</param>
        string MakeElementName(string baseName, IEnumerable<int> indexes);

        /// <summary>
        /// Constructs an appropriate array element name from the given <paramref name="baseName"/> and 
        /// the specified <paramref name="indexes"/>. Returns a string in the form "ArrayName 0 1 2..."
        /// </summary>
        /// /// <param name="baseName">The "name" part of the element name, without the indexes.</param>
        /// <param name="indexes">The "indexes" array part of the element name.</param>
        string MakeElementName(string baseName, params int[] indexes);
    }
}
