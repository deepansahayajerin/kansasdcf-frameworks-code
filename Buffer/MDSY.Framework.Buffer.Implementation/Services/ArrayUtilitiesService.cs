using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
//CHADusing Unity.Attributes;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IArrayUtilitiesService.
    /// </summary>
    [InjectionImplementer(typeof(IArrayUtilitiesService))]
    internal sealed class ArrayUtilitiesService : IArrayUtilitiesService
    {
        /// <summary>
        /// Returns the list of indexes built into the given <paramref name="elementName"/> via index suffixes.
        /// For nested indexes, parent indexes are first. 
        /// </summary>
        /// <param name="elementName">The name which is to be processed.</param>
        /// <param name="baseName">The "name" part of <paramref name="elementName"/>, without the indexes.</param>
        /// <returns>A collection of numeric indexes, if any are built into the given <paramref name="elementName"/>.</returns>
        public IEnumerable<int> GetElementIndexes(string elementName, out string baseName)
        {
            return ArrayElementUtils.GetElementIndexes(elementName, out baseName);
        }
        
        /// <summary>
        /// Constructs an appropriate array element name from the given <paramref name="baseName"/> and 
        /// <paramref name="singleLevelIndex"/>. 
        /// </summary>
        /// <param name="baseName">the "name" part of the element name, without the index.</param>
        /// <param name="singleLevelIndex">The "index" part of the element name.</param>
        /// <returns>A string in the form "ArrayName 0".</returns>
        public string MakeElementName(string baseName, int singleLevelIndex)
        {
            return ArrayElementUtils.MakeElementName(baseName, singleLevelIndex);
        }

        /// <summary>
        /// Constructs an appropriate multi-index array element name from the given <paramref name="baseName"/> and 
        /// <paramref name="indexes"/>.
        /// </summary>
        /// <param name="baseName">the "name" part of the element name, without the index.</param>
        /// <param name="indexes">The "indexes" part of the element name.</param>
        /// <returns>A string in the form "ArrayName 0 1 2...".</returns>
        public string MakeElementName(string baseName, IEnumerable<int> indexes)
        {
            return ArrayElementUtils.MakeElementName(baseName, indexes);
        }

        /// <summary>
        /// Constructs an appropriate array element name from the given <paramref name="baseName"/> and 
        /// the specified <paramref name="indexes"/>.
        /// </summary>
        /// <param name="baseName">The "name" part of the element name, without the indexes.</param>
        /// <param name="indexes">The "indexes" array part of the element name.</param>
        /// <returns>A string in the form "ArrayName 0 1 2..."</returns>
        public string MakeElementName(string baseName, params int[] indexes)
        {
            var idx = new List<int>(indexes);
            return MakeElementName(baseName, idx);
        }
    }
}
