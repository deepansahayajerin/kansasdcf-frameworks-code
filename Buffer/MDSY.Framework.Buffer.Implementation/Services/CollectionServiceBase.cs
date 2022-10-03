using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
using Unity;
using MDSY.Framework.Buffer.Common;

namespace MDSY.Framework.Buffer.Implementation
{
    internal abstract class CollectionServiceBase<TKey, TValue> : ICollectionServiceBase<TKey, TValue>
    {
        #region private fields
        private Dictionary<TKey, TValue> items = null;
        #endregion

        #region protected properties
        protected Dictionary<TKey, TValue> Items
        {
            get
            {
                // Create on demand...
                if (items == null)
                    items = new Dictionary<TKey, TValue>();
                return items;
            }
        }
        #endregion

        #region protected methods
        protected TKey GetKey(TValue item)
        {
            TKey result = default(TKey);

            if (!Items.ContainsValue(item))
                throw new CollectionServiceException(String.Format("Value {0} not found in collection.", item));
            else
            {
                var kvPair = Items.Where(kv => kv.Value.Equals(item)).FirstOrDefault();
                result = kvPair.Key;
            }

            return result;
        }

        protected abstract TKey GetNewKeyValue(TValue item);

        protected bool TryGetKey(TValue item, out TKey key)
        {
            bool result = false;
            key = default(TKey);

            try
            {
                key = GetKey(item);
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        protected virtual TKey InternalAdd(TValue item)
        {
            TKey result = default(TKey);
            if (!TryGetKey(item, out result))
            {
                result = GetNewKeyValue(item);
                Items.Add(result, item);
            }

            return result;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Adds the given <paramref name="item"/> to the collection and returns 
        /// the <paramref name="item"/>'s key in the collection.
        /// </summary>
        /// <remarks>If the item already exists in the collection, it is not 
        /// added, but we return the key of the extant object.</remarks>
        /// <param name="item">The <typeparamref name="TValue"/> object to add to the collection.</param>
        /// <returns>A key value for reaccessing the <paramref name="item"/>.</returns>
        public TKey Add(TValue item)
        {
            if (item == null)
                throw new ArgumentNullException("item", "item is null.");

            return InternalAdd(item);
        }

        /// <summary>
        /// Returns the <typeparamref name="TValue"/> object with the given <paramref name="key"/> from 
        /// the collection.
        /// </summary>
        /// <param name="key">The key value for which to search.</param>
        /// <returns>The appropriate object of type <typeparamref name="TValue"/> if found, otherwise null.</returns>
        public TValue Get(TKey key)
        {
            TValue result = default(TValue);

            if (Items.ContainsKey(key))
            {
                result = Items[key];
            }

            return result;
        }

        /// <summary>
        /// If the given <paramref name="item"/> exists within the collection,
        /// returns the key of the <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The <typeparamref name="TValue"/> object for which to search.</param>
        /// <returns>The key value of <paramref name="item"/> if it is in the
        /// collection, otherwise -1.</returns>
        public TKey GetKeyFor(TValue item)
        {
            TKey result;
            if (!TryGetKey(item, out result))
            {
                result = default(TKey);
            }

            return result;
        }

        /// <summary>
        /// Removes the given <paramref name="item"/> from the collection.
        /// </summary>
        /// <param name="item">The <typeparamref name="TValue"/> object to remove.</param>
        public void Remove(TValue item)
        {
            if (Items.ContainsValue(item))
            {
                TKey key = GetKeyFor(item);
                Remove(key);
            }
        }

        /// <summary>
        /// Removes the given <paramref name="item"/> from the collection.
        /// </summary>
        /// <param name="item">The <typeparamref name="TValue"/> object to remove.</param>
        public void Remove(TKey key)
        {
            if (Items.ContainsKey(key))
            {
                Items.Remove(key);
            }
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets a count of the objects in the collection.
        /// </summary>
        public int Count
        {
            get { return Items.Count; }
        }
        #endregion
    }
}

