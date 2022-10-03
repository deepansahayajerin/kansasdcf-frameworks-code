using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
//CHADusing Unity.Attributes;

namespace MDSY.Framework.Buffer.Implementation
{


    /// <summary>
    /// Implements the injection interface IBufferAddressCollectionService.
    /// </summary>
    [InjectionImplementer(typeof(IBufferAddressCollectionService))]
    internal sealed class BufferAddressCollectionService : IBufferAddressCollectionService
    {

        #region private fields
        [ThreadStatic]
        private static Dictionary<int, IBufferAddress> items = null;
        #endregion

        #region protected properties
        private Dictionary<int, IBufferAddress> Items
        {
            get
            {
                // Create on demand...
                if (items == null)
                    items = new Dictionary<int, IBufferAddress>();
                return items;
            }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Get the IBufferAddress from Buffer Address collection
        /// </summary>
        /// <param name="RecKey">The record key used to retrieve the IBufferAddress.</param>
        /// <returns>The items belonging to the record key.</returns>
        public IBufferAddress Get(int RecKey)
        {

            return Items[RecKey];
        }

        /// <summary>
        /// Add new item to the buffer address collection
        /// </summary>
        /// <param name="item">Item to be added to the collection.</param>
        /// <returns>The item's new key.</returns>
        public int Add(IBufferAddress item)
        {
            if (item == null)
                throw new ArgumentNullException("item", "item is null.");

            int newKey = GetNewKeyValue(item);
            Items.Add(newKey, item);
            return newKey;
        } 
        #endregion

        #region private methods
        private int GetNewKeyValue(IBufferAddress item)
        {
            // don't reuse key values; this will keep incrementing new keys.
            var keys = Items.Keys.Select(i => i);
            int result = keys.Count() > 0 ? keys.Max() : -1;
            return ++result;
        } 
        #endregion
    }
}
