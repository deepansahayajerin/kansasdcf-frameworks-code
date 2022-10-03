using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Interfaces;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Provides access to the array elements.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    [Serializable]
    internal abstract class ArrayElementAccessorBase<TItem> : IArrayElementAccessor<TItem>, IEditableArrayElementAccessor<TItem>
        where TItem : IArrayElement, IBufferElement
    {
        #region private fields
        private Dictionary<string, TItem> elements = new Dictionary<string, TItem>();
        private int indexOffset;

        /// <summary>If indexOffset is set to this value, it's not yet initialized.</summary>
        private const int INT_UnitializedIndexOffset = -99;
        #endregion

        #region private methods
        private TItem GetIndexedItem(params int[] index)
        {
            var idx = index.ToList();
            TItem result = default(TItem);

            string key = GetIndexKeyString(idx.ToArray());

            if (Elements.ContainsKey(key))
            {
                result = Elements[key];
            }

            return result;
        }

        private static string GetIndexKeyString(IEnumerable<int> indices)
        {
            return GetIndexKeyString(indices.ToArray());
        }

        private static string GetIndexKeyString(int[] indices)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];  // don't use IndexOffset here - use what's passed in;
                builder.AppendFormat("{0}:", index);
            }

            return builder.ToString().TrimEnd(':');
        }
        #endregion
        #region public methods

        /// <summary>
        /// Adds the given <paramref name="element"/> to the internal elements list.
        /// </summary>
        /// <param name="element">A reference to the element to be added.</param>
        public void AddElement(TItem element)
        {
            int[] key;
            string temp;
            key = ArrayElementUtils.GetElementIndexes((element as IBufferElement).Name, out temp).ToArray();

            //adjust key value(s) for elements to reflect zero based array indexed to 1 based collection
            for (int i = 0; i <= key.GetUpperBound(0); i++)
            {
                key[i] = key[i] + IndexOffset;
            }

            string keyStr = GetIndexKeyString(key);
            Elements.Add(keyStr, (TItem)element);
        }

        /// <summary>
        /// Returns a reference to the current array element accessor object.
        /// </summary>
        /// <returns>Returns a reference to the current array element accessor object.</returns>
        public IArrayElementAccessor<TItem> AsReadOnly()
        {
            return this as IArrayElementAccessor<TItem>;
        }

        /// <summary>
        /// Creates a deep copy of the current array element accessor object.
        /// </summary>
        /// <returns>Returns a new instance of the current object.</returns>
        public object Clone()
        {
            ArrayElementAccessorBase<TItem> thisclone = (ArrayElementAccessorBase<TItem>)this.MemberwiseClone();
            thisclone.elements = new Dictionary<string, TItem>();
            //foreach (string key in this.elements.Keys)
            //{
            //    thisclone.elements.Add(key, (TItem)this.elements[key].Clone());
            //}
            return thisclone;
        }

        public void SetIsBlankWhenZero(bool isBlankWhenZero)
        {
            if (Count == 0 || !(this[0] is IField)) return;

            foreach (string key in elements.Keys)
                ((IField)elements[key]).IsBlankWhenZero = isBlankWhenZero;
        }

        #endregion

        #region abstract
        /// <summary>
        /// Indicates the amount to offset from the normal .net zero-based index. 
        /// </summary>
        /// <returns>Returns the offset value.</returns>
        protected abstract int GetIndexOffset();
        #endregion

        #region constructor
        /// <summary>
        /// Initializes a new instance of the ArrayElementAccessorBase class.
        /// </summary>
        public ArrayElementAccessorBase()
        {
            // indexOffset will be lazy loaded after construction
            // besides, don't call a virtual in an abstract constructor...
            indexOffset = INT_UnitializedIndexOffset;
        }
        #endregion

        #region  public properties

        /// <summary>
        /// Gets an array of all array elements represented by this accessor.
        /// </summary>
        public TItem[] All
        {
            get { return Elements.Values.ToArray(); }
        }

        /// <summary>
        /// Gets the number of array elements to which this object has access.
        /// </summary>
        public int Count
        {
            get { return Elements.Count; }
        }

        /// <summary>
        /// Returns a reference to the collection of the array elements.
        /// </summary>
        public Dictionary<string, TItem> Elements
        {
            get
            {
                return elements;
            }
        }

        /// <summary>
        /// Returns the number of bytes that are occupied in the buffer by one element of the current array.
        /// </summary>
        public int LengthInBuffer
        {
            get { return this[1].LengthInBuffer; }
        }

        /// <summary>
        /// Gets the amount by which the client code-given array index will 
        /// be adjusted in order to match the zero-based arrays of .NET.
        /// </summary>
        /// <remarks>
        /// <para>A one-based array in converted code would pass in 1 for the first 
        /// element of an array, but a .NET array is zero based, so unadjusted, 
        /// the second element of the array would be returned. </para>
        /// <para>When the array index is passed in from the converted client 
        /// code, the value of IndexOffset will be SUBTRACTED from the given 
        /// index. </para>
        /// </remarks>
        public int IndexOffset
        {
            get
            {
                if (indexOffset == INT_UnitializedIndexOffset)
                    indexOffset = GetIndexOffset();
                return indexOffset;
            }
        }

        /// <summary>
        /// Multidimensional indexer property for array elements.
        /// </summary>
        public TItem this[params int[] index]
        {
            get { return GetIndexedItem(index.ToList().ToArray()); }
        }

        /// <summary>
        /// Multidimensional indexer property int and Ifield
        /// </summary>
        public TItem this[int index1, IField index2]
        {
            get
            {
                if (index2 == null)
                    throw new ArgumentNullException("index2");

                return GetIndexedItem(index1, index2.GetValue<int>());
            }
        }

        /// <summary>
        /// Multidimensional indexer property IField and Int
        /// </summary>
        public TItem this[IField index1, int index2]
        {
            get
            {
                if (index1 == null)
                    throw new ArgumentNullException("index1");

                return GetIndexedItem(index1.GetValue<int>(), index2);
            }
        }

        /// <summary>
        /// Multidimensional indexer property for array elements using numeric fields for indices.
        /// </summary>
        public TItem this[params IField[] index]
        {
            get
            {
                TItem result = default(TItem);
                if (index.All(f => f.IsNumericOnlyValue))
                {
                    var idx = index.Select(f => f.GetValue<int>()).ToArray();
                    result = GetIndexedItem(idx);
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the name of this accessor object; this is also the name the accessor
        /// will use when accessing array elements.
        /// </summary>
        public string Name { get; set; }
        #endregion
    }
}
