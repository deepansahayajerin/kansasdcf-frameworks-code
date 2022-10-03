using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
//CHADusing Unity.Attributes;
using MDSY.Framework.Buffer.Common;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IRecordBufferCollectionService.
    /// </summary>
    [InjectionImplementer(typeof(IRecordBufferCollectionService))]
    internal sealed class RecordBufferCollectionService : IRecordBufferCollectionService
    {
        private IRecord currentRecord = null;
        #region private fields
        [ThreadStatic]
        private static Dictionary<string, IDataBuffer> items = null;
        #endregion

        #region protected properties
        private Dictionary<string, IDataBuffer> Items
        {
            get
            {
                // Create on demand...
                if (items == null)
                    items = new Dictionary<string, IDataBuffer>();
                return items;
            }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Adds the given IDatabuffer item to the record buffer collection.
        /// </summary>
        /// <param name="item">The IDataBuffer item to be added to the collection.</param>
        /// <returns>The key of the newly added item.</returns>
        public string Add(IDataBuffer item)
        {
            if (item == null)
                throw new ArgumentNullException("item", "item is null.");

            string newKey = GetNewKeyValue(item);
            Items.Add(newKey, item);
            return newKey;
        }
        /// <summary>
        /// Adds the given <paramref name="dataBuffer"/> to the collection and returns 
        /// the <paramref name="dataBuffer"/>'s key in the collection. 
        /// The returned key is built from the given <paramref name="owningRecord"/>.Name.
        /// </summary>
        /// <remarks>If the item already exists in the collection, it is not 
        /// added, but we return the key of the extant object.</remarks>
        /// <param name="dataBuffer">The IDataBuffer to add to the collection.</param>
        /// <param name="owningRecord">The IRecord which owns the given <paramref name="dataBuffer"/>.</param>
        /// <returns>A key value for reaccessing the <paramref name="dataBuffer"/>.</returns>
        public string Add(IDataBuffer dataBuffer, IRecord owningRecord)
        {
            string result = string.Empty;

            if (!TryGetKey(dataBuffer, out result))
            {
                currentRecord = owningRecord;
                result = GetNewKeyValue(dataBuffer);
                if (Items.ContainsKey(result))
                {
                    Items[result] = dataBuffer;
                }
                else
                {
                    Items.Add(result, dataBuffer);
                }
            }

            currentRecord = null;
            return result;
        }

        /// <summary>
        /// Gets the IDataBuffer item
        /// </summary>
        /// <param name="RecKey">The record key to get the IDatabuffer item.</param>
        /// <returns>The IDataBuffer item.</returns>
        public IDataBuffer Get(string RecKey)
        {

            return Items[RecKey];
        }

        /// <summary>
        /// REturns the current record name.
        /// </summary>
        /// <param name="item">The IDtabuffer item.</param>
        /// <returns>The current record name.</returns>
        public string GetNewKeyValue(IDataBuffer item)
        {
            if (item == null)
                throw new ArgumentNullException("item", "item is null.");
            if (currentRecord == null)
                throw new CollectionServiceException("currentRecord must contain a record object.");

            return currentRecord.Name;
        }

        /// <summary>
        /// Returns true if the key for the item is valid.
        /// </summary>
        /// <param name="item">The IDataBuffer item.</param>
        /// <param name="key">The item collection key.</param>
        /// <returns>True if the key for the item is valid.</returns>
        public bool TryGetKey(IDataBuffer item, out string key)
        {
            bool result = false;
            key = string.Empty;

            try
            {
                key = GetKeyFor(item);
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Returns the item's key.
        /// </summary>
        /// <param name="item">The IDataBuffer item.</param>
        /// <returns>The item's key.</returns>
        public string GetKeyFor(IDataBuffer item)
        {
            string result = string.Empty;

            var kvPair = Items.Where(kv => kv.Value.Equals(item)).FirstOrDefault();
            result = kvPair.Key;

            return result;
        }

        /// <summary>
        /// Removes the item from the collection.
        /// </summary>
        /// <param name="key">The key for the item to be removed.</param>
        public void Remove(string key)
        {
            if (Items.ContainsKey(key))
            {
                Items.Remove(key);
            }
        }

        /// <summary>
        /// Clears all items in the collection.
        /// </summary>
        public void Clear()
        {
            Items.Clear();
        }
        #endregion
    }
}

