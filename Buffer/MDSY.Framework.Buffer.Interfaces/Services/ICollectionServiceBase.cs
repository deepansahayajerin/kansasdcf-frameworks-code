using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines a baseclass for Collection Service.
    /// </summary>
    /// <typeparam name="TKey">The key value of type T.</typeparam>
    /// <typeparam name="TValue">The appropriate object of type T.</typeparam>
    public interface ICollectionServiceBase<TKey, TValue>
    {

        #region attributes
        /// <summary>
        /// Gets a count of the objects in the collection.
        /// </summary>
        int Count { get; }
        #endregion

        #region operations
        /// <summary>
        /// Adds the given <paramref name="item"/> to the collection and returns 
        /// the <paramref name="item"/>'s key in the collection.
        /// </summary>
        /// <remarks>If the item already exists in the collection, it is not 
        /// added, but we return the key of the extant object.</remarks>
        /// <param name="item">The <typeparamref name="TValue"/> object to add to the collection.</param>
        /// <returns>A key value for reaccessing the <paramref name="item"/>.</returns>
        TKey Add(TValue item);

        /// <summary>
        /// Returns the <typeparamref name="TValue"/> object with the given <paramref name="key"/> from 
        /// the collection.
        /// </summary>
        /// <param name="key">The key value for which to search.</param>
        /// <returns>The appropriate object of type <typeparamref name="TValue"/> if found, otherwise null.</returns>
        TValue Get(TKey key);

        /// <summary>
        /// If the given <paramref name="item"/> exists within the collection,
        /// returns the key of the <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The <typeparamref name="TValue"/> object for which to search.</param>
        /// <returns>The key value of <paramref name="item"/> if it is in the
        /// collection, otherwise -1.</returns>
        TKey GetKeyFor(TValue item);

        /// <summary>
        /// Removes the given <paramref name="item"/> from the collection.
        /// </summary>
        /// <param name="item">The <typeparamref name="TValue"/> object to remove.</param>
        void Remove(TValue item);

        /// <summary>
        /// Removes the item with the given <paramref name="key"/> from the 
        /// collection.
        /// </summary>
        /// <param name="key">The key value for which to search.</param>
        void Remove(TKey key);
        #endregion
    }
}

