using System;
using System.Collections;
using System.Collections.Specialized;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// A collection that holds a maximum number of items. First-in-Last-out: FILO.
    /// </summary>
    /// <description>
    /// When a new item is added and the total count of the collection is at the 
    /// maximum number of allowed items, the oldest (first) element is removed 
    /// and the newest is appended at the end.
    /// </description>
    public class FILOCollectionBase
    {
        #region private fields
        private int _maxCount;
        private NameValueCollection _keys;
        private HybridDictionary _items;
        #endregion

        #region public properties
        /// <summary>
        /// Gets the items in the collection.
        /// </summary>
        public HybridDictionary Items
        {
            get
            {
                if (_items == null)
                    _items = new HybridDictionary(_maxCount);
                return _items;
            }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Adds an object to the Items collection.  
        /// </summary>
        /// <description>
        /// Before object is added, an assessment is done to see if the
        /// collection's MaxCount has been met.  If so then oldest object is
        /// dropped and newest is appended.
        /// </description>
        /// <param name="hashKey">System.String used to reference the object in the collection.</param>
        /// <param name="obj">The object being added to the collection.</param>
        public void Add(string hashKey, object item)
        {
            // if at our max count then drop the oldest
            if (_items.Count >= _maxCount)
            {

                _items.Remove(_keys[0]);
                _keys.Remove(_keys[0]);

            }

            // add our object to the collection.
            if (item != null)
            {
                _items.Add(hashKey, item);
                _keys.Add(hashKey, hashKey);
            }
        }

        /// <summary>
        /// Initializes the collection with its maximum size.
        /// </summary>
        /// <param name="maxCount">
        /// The maximum number of items allowed in our collection.
        /// </param>
        public FILOCollectionBase(int maxCount)
        {
            _maxCount = maxCount;
            _keys = new NameValueCollection(maxCount);
            _items = new HybridDictionary(maxCount);
        }
        #endregion
    }
}
