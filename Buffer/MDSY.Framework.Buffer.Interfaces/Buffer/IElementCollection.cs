using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which contains a readonly list of IBufferElements.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface IElementCollection
    {
        #region attributes
        /// <summary>
        /// Returns <c>true</c> if some child element down the record structure tree is an IArray.
        /// </summary>
        bool HasArrayInChildElements { get; }

        /// <summary>
        /// Returns the IBufferElement with the given name.
        /// </summary>
        IBufferElement this[string name] { get; }

        /// <summary>
        /// Gets the collection of child element objects.
        /// </summary>
        IEnumerable<IBufferElement> Elements { get; }

        /// <summary>
        /// Gets the collection of child elements.
        /// </summary>
        IDictionary<string, IBufferElement> ChildCollection { get; }

        #endregion

        #region operations
        /// <summary>
        /// Assigns the given <paramref name="buffer"/> to all children of this collection, recursively.
        /// </summary>
        /// <param name="buffer">Buffer to be assigned to all children.</param>
        void AssignDataBufferRecursive(IDataBuffer buffer);

        /// <summary>
        /// Returns <c>true</c> if an IBufferElement with the given <paramref name="name"/> exists in the children
        /// this collection.
        /// </summary>
        /// <param name="name">The element name for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        bool ContainsElement(string name);

        /// <summary>
        /// Returns <c>true</c> if an IBufferElement with the given <paramref name="name"/> exists anywhere within 
        /// the descendants of this collection.
        /// </summary>
        /// <param name="name">The element name for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        bool ContainsElementNested(string name);

        /// <summary>
        /// Returns <c>true</c> if this collection already contains the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The IBufferElement for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        bool ContainsElement(IBufferElement element);

        /// <summary>
        /// Returns <c>true</c> if the given <paramref name="element"/> exists anywhere within the descendants 
        /// of this collection.
        /// </summary>
        /// <param name="element">The element for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        bool ContainsElementNested(IBufferElement element);

        /// <summary>
        /// Returns an IBufferElement object if one is found with the given <paramref name="name"/> anywhere in the 
        /// collection's child hierarchy.
        /// </summary>
        /// <param name="name">The element name for which to search.</param>
        /// <returns>A matching element object, if found.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is null or empty.</exception>
        /// <exception cref="FieldCollectionException">The given <paramref name="name"/> is not found.</exception>
        IBufferElement GetElementByNameNested(string name);

        /// <summary>
        /// Adds the given <paramref name="element"/> to this object's direct-child elements. 
        /// </summary>
        /// <remarks>
        /// Time and design constraints have forced this method to be here, in IElementCollection, which was supposed 
        /// to be basically a readonly-collection, with adding done elsewhere. I'd love to see this fixed. 
        /// </remarks>
        void AddChildElement(IBufferElement element);

        /// <summary>
        /// Returns a count of all elements beneath this collection, at any level. 
        /// </summary>
        /// <returns>A recursive count of all descendant elements.</returns>
        int GetNestedElementCount();
        #endregion

    }
}
