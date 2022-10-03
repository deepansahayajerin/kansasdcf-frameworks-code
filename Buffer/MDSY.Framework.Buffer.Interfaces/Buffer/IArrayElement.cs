using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which can be an element of an IArray(of T). 
    /// </summary>
    public interface IArrayElement
    {
        /// <summary>
        /// Returns the IArrayElement parent of this element that is directly below the next array above, not counting 
        /// this object itself. 
        /// If this object is not in an array, returns null.
        /// See example.
        /// </summary>
        /// <remarks>Traverses up the tree to find the parent element that is the direct child of the next array parent.</remarks>
        /// <example>
        /// Given the following structure, where this object is Field_A:
        /// <code>
        /// RECORD
        ///  - GROUPARRAY[10]
        ///    - GROUP01
        ///      - GROUP02
        ///        - Field_A
        /// </code>
        /// Calling <c>GetPenultimateArrayParent()</c> would return <c>GROUP01</c> since that is the direct child of 
        /// <c>GROUPARRAY</c>.
        /// </example>
        IArrayElement GetNextPenultimateArrayParent();

        /// <summary>
        /// Returns the index of the element within its array.
        /// </summary>
        int ArrayElementIndex { get; }

        /// <summary>
        /// Returns the collection of indexes of the element within multiple nested arrays, if present.
        /// </summary>
        IEnumerable<int> GetArrayElementIndexes();

        /// <summary>
        /// Gets the number of IArrayBase-implementing parents this element has, indicating how many nested
        /// array levels deep.
        /// </summary>
        int GetArrayParentCount();
    }
}
