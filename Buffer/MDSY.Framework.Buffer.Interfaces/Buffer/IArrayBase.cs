using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Non-generic ancestor for IArray(of T).
    /// </summary>
    public interface IArrayBase : IBufferElement, IElementCollection, IDefineable
    {
        /// <summary>
        /// Gets the total number of elements in the array.
        /// </summary>
        int ArrayElementCount { get; }

        /// <summary>
        /// Gets the length of each element in the array.
        /// </summary>
        int ArrayElementLength { get; }

        /// <summary>
        /// Gets the collection of indexes for this array and arrays above. 
        /// </summary>
        /// <remarks>
        /// If this array is a nested array, ArrayIndexes will include the array element index for this
        /// array (as buffer element) and the array indexes for arrays above this one. 
        /// </remarks>
        /// <example>
        /// 1 - If this array has no arrays above it, it is a singular instance, thus ArrayIndexes returns an empty collection.
        /// 2 - If this array is the child of an array, thus in this case:
        /// <code>
        /// RECORD
        ///  - GROUPARRAY[10]        // 10 occurrences
        ///     - GROUP[3]           // the 3rd group array element
        ///        - FIELDARRAY[5]   // this object - 5 field occurrences, but 10 instances of this array.
        /// </code>
        /// <c>GROUP[3].FIELDARRAY.ArrayIndexes</c> would return a collection with the value <c>3</c>.
        /// </example>
        IEnumerable<int> GetArrayIndexes();
    }
}
