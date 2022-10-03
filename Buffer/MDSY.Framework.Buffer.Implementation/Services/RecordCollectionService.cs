using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
//CHADusing Unity.Attributes;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IRecordCollectionService.
    /// </summary>
    [InjectionImplementer(typeof(IRecordCollectionService))]
    internal sealed class RecordCollectionService : IRecordCollectionService
    {
        #region private fields
        [ThreadStatic]
        private static Dictionary<int, IRecord> items = null;
        #endregion

        #region protected properties
        private Dictionary<int, IRecord> Items
        {
            get
            {
                // Create on demand...
                if (items == null)
                    items = new Dictionary<int, IRecord>();
                return items;
            }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Returns the IRecord object with the given <paramref name="name"/> 
        /// if it is found within the collection.
        /// </summary>
        /// <param name="name">The name for which to search.</param>
        /// <returns>The appropriate IRecord object if found, otherwise null.</returns>
        public IRecord Get(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");

            return Items.Where(kv => kv.Value.Name == name).FirstOrDefault().Value;
        }

        /// <summary>
        /// Returns the IRecord object with the given <paramref name="RecKey"/> 
        /// if it is found within the collection.
        /// </summary>
        /// <param name="RecKey">The Key for which to search.</param>
        /// <returns>The appropriate IRecord object if found, otherwise null.</returns>
        public IRecord Get(int RecKey )
        {

            return Items[RecKey];
        }

        /// <summary>
        /// Returns the key for the added item to the collection.
        /// </summary>
        /// <param name="item">The item to be added to the collection.</param>
        /// <returns>The appropriate key for the added item.</returns>
        public int Add(IRecord item)
        {
            if (item == null)
                throw new ArgumentNullException("item", "item is null.");

            int newKey = GetNewKeyValue(item);
            item.RecordKey = newKey;
            Items.Add(newKey, item);
            return newKey;
        }

        /// <summary>
        /// Removes the given IRecord item from the collection.
        /// </summary>
        /// <param name="item">Item to be removed from the collection.</param>
        public void Remove(IRecord item)
        {
            if (Items.ContainsValue(item))
            {
                int key = GetKeyFor(item);
                Remove(key);
            }
        }

        /// <summary>
        /// Removes the IRecord item associated to the given key from the collection.
        /// </summary>
        /// <param name="key">Key for the items to be removed.</param>
        public void Remove(int key)
        {
            if (Items.ContainsKey(key))
            {
                Items.Remove(key);
            }
        }

        /// <summary>
        /// Gets the key for the given <paramref name="item"/>
        /// </summary>
        /// <param name="item">The item to get the key for.</param>
        /// <returns>The item's key if found, otherwise 0.</returns>
        public int GetKeyFor(IRecord item)
        {
            int result = 0;

            //var kvPair = Items.Where(kv => kv.Value.RecordKey.Equals(item.RecordKey)).FirstOrDefault(); - this statement returns weird result, BZ 643
            //result = kvPair.Key;

            if (Items.ContainsKey(item.RecordKey))
                result = item.RecordKey;

            return result;
        }

        /// <summary>
        /// Clears the items in the collection.
        /// </summary>
        public void Clear()
        {
            Items.Clear();
        }
        #endregion

        #region private methods
        private int GetNewKeyValue(IRecord item)
        {
            // don't reuse key values; this will keep incrementing new keys.
            var keys = Items.Keys.Select(i => i);
            int result = keys.Count() > 0 ? keys.Max() : -1;
            return ++result;
        } 
        #endregion
    }
}
