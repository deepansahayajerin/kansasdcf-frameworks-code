using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Interfaces;
using System.ComponentModel;
using MDSY.Framework.Buffer.Common;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Generic base class for classes which implement IArray(of TItem).
    /// </summary>
    /// <remarks>
    /// Descendants can close the generic type by declaring a specific type for <typeparamref name="TItem"/>.
    /// </remarks>
    /// <typeparam name="TItem"></typeparam>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    internal abstract class ArrayBase<TItem> : BufferElementBase, IArray<TItem>
        where TItem : IBufferElement
    {
        #region private fields
        private int cachedLength;

        private StructureDefinitionCompositor structureDef = null;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the ArrayBase class.
        /// </summary>
        public ArrayBase()
        {
            IsDefining = true;
            cachedLength = -1;
        }
        #endregion

        #region private methods
        private int CalculateLength()
        {
            return ArrayElementLength * ArrayElementCount;
        }

        private bool GetHasArrayInChildElements()
        {
            bool result = arrayElements.Any(e => e is IArrayBase);

            if (!result)
            {
                foreach (IBufferElement item in arrayElements)
                {
                    if (item is IElementCollection && (item as IElementCollection).HasArrayInChildElements)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }
        #endregion

        #region protected properties
        /// <summary>
        /// The internal collection of array elements. 
        /// </summary>
        protected List<TItem> arrayElements = new List<TItem>();

        /// <summary>
        /// Gets a StructureDefinitionCompositor object.
        /// </summary>
        protected StructureDefinitionCompositor StructureDef
        {
            get
            {
                if (structureDef == null)
                {
                    structureDef = new StructureDefinitionCompositor(this as IElementCollection) { Buffer = this.Buffer };
                }
                return structureDef;
            }
        }
        #endregion

        #region protected methods

        /// <summary>
        /// Returns a byte array concatenated from the values of all contained elements.
        /// </summary>
        /// <returns>Returns and array of bytes that contains the contents of the current array object.</returns>
        protected byte[] GetElementsAsBytes()
        {
            List<byte> result = new List<byte>();

            foreach (IBufferElement value in arrayElements)
            {
                if (value is IBufferValue)
                {
                    result.AddRange((value as IBufferValue).AsBytes);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Duplicates current array object to the new parent. Override this method to provide custom duplication behavior.
        /// </summary>
        /// <param name="name">Array name.</param>
        /// <param name="bufferPositionOffset">Array's offset position in the buffer.</param>
        /// <param name="newParent">Specifies a reference to the new parent object.</param>
        /// <param name="arrayIndexes">Collection of the array indexes.</param>
        /// <returns>Returns a reference to the duplicated array instance.</returns>
        protected abstract IBufferElement InternalDuplicate(string name, int bufferPositionOffset, IElementCollection newParent, IEnumerable<int> arrayIndexes);

        /// <summary>
        /// Adds the given <paramref name="element"/> to the internal array element collection.
        /// </summary>
        /// <param name="element">Array element to be added.</param>
        protected void AddArrayElement(TItem element)
        {
            arrayElements.Add(element);
        }
        #endregion

        #region overrides
        /// <summary>
        /// Returns the length (in bytes) of this buffer element in the buffer.
        /// </summary>
        protected override int GetLength()
        {
            return IsDefining ? CalculateLength() : cachedLength;
        }

        #endregion

        #region public properties
        /// <summary>
        /// Gets the length of each element in the array.
        /// </summary>
        [Category("IArrayBase")]
        [Description("The length in bytes of each element in the array.")]
        [ReadOnly(true)]
        public int ArrayElementLength { get; set; }

        /// <summary>
        /// Gets the total number of elements in the array.
        /// </summary>
        [Category("IArrayBase")]
        [Description("The total number of elements in the array.")]
        [ReadOnly(true)]
        public int ArrayElementCount { get; set; }


        /// <summary>
        /// Gets the collection of indexes for this array and arrays above. 
        /// </summary>
        /// <remarks>
        /// If this array is a nested array, ArrayIndexes will include the array element index for this
        /// array (as buffer element) and the array indexes for arrays above this one. 
        /// </remarks>
        [Category("IArrayBase")]
        [Description("The collection of indexes for this array and any arrays above.")]
        [ReadOnly(true)]
        public IEnumerable<int> GetArrayIndexes()
        {
            return this.GetArrayElementIndexes();
        }

        /// <summary>
        /// Gets the collection of child element objects.
        /// </summary>
        [Category("IElementCollection")]
        [Description("Returns a collection of this array's array elements.")]
        [ReadOnly(true)]
        public IEnumerable<IBufferElement> Elements
        {
            get { return arrayElements.Select<TItem, IBufferElement>(x => x as IBufferElement); }
        }

        /// <summary>
        /// Returns a new and empty instance of the array elements collection.
        /// </summary>
        public IDictionary<string, IBufferElement> ChildCollection
        {
            get { return new Dictionary<string, IBufferElement>(); }
        }

        /// <summary>
        /// Returns a reference to the collection of the array elements.
        /// </summary>
        public List<TItem> ArrayElements
        {
            get { return arrayElements; }
        }

        /// <summary>
        /// Returns <c>true</c> if some child element down the record structure tree is an IArray.
        /// </summary>
        [Category("IElementCollection")]
        [Description("Indicates whether a child element is also an IArray")]
        [ReadOnly(true)]
        public bool HasArrayInChildElements
        {
            get { return GetHasArrayInChildElements(); }
        }

        /// <summary>
        /// Gets whether this object is currently in its period of definition.
        /// </summary>
        [Category("IDefinable")]
        [Description("Indicates whether this object is in its period of definition.")]
        [ReadOnly(true)]
        public bool IsDefining { get; set; }

        /// <summary>
        /// Gets an occurrence of SourceElement as indicated by <paramref name="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [Category("IArray(of T)")]
        [Description("Indexer property; returns array elements by numeric index.")]
        [ReadOnly(true)]
        public TItem this[int index]
        {
            get { return arrayElements[index]; }
        }

        /// <summary>
        /// Returns the IBufferElement with the given name.
        /// </summary>
        [Category("IElementCollection")]
        [Description("Indexer property; returns array elements by name.")]
        [ReadOnly(true)]
        public IBufferElement this[string name]
        {
            get { return arrayElements.Where(e => e.Name == name).Single(); }
        }

        public FieldFormat FieldJustification { get; set; }
        #endregion

        #region public methods
        /// <summary>
        /// Assigns the given <paramref name="buffer"/> to all children of this collection, recursively.
        /// </summary>
        /// <param name="buffer">Specifies a reference to the buffer that should be assing.</param>
        public void AssignDataBufferRecursive(IDataBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer", "buffer is null.");

            foreach (IBufferElement element in Elements)
            {
                if (element is IBufferValue)
                {
                    (element as IBufferValue).Buffer = buffer;
                }

                if (element is IElementCollection)
                {
                    (element as IElementCollection).AssignDataBufferRecursive(buffer);
                }
            }
        }

        /// <summary>
        /// Returns <c>true</c> if this collection already contains the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The IBufferElement for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        public bool ContainsElement(IBufferElement element)
        {
            return Elements.Contains(element);
        }

        /// <summary>
        /// Returns <c>true</c> if an IBufferElement with the given <paramref name="name"/> exists in the children
        /// this collection.
        /// </summary>
        /// <param name="name">The element name for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        public bool ContainsElement(string name)
        {
            return Elements.Where(e => (String.Compare(e.Name, name, true) == 0)).Count() > 0;
        }

        /// <summary>
        /// Returns <c>true</c> if the given <paramref name="element"/> exists anywhere within the descendants 
        /// of this collection.
        /// </summary>
        /// <param name="element">The element for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        public bool ContainsElementNested(IBufferElement element)
        {
            bool result = ContainsElement(element);

            if (!result)
            {
                foreach (IBufferElement value in Elements)
                {
                    if (value is IElementCollection)
                    {
                        result = (value as IElementCollection).ContainsElementNested(element);
                    }

                    if (result)
                    {
                        break;
                    }
                }

            }

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> if an IBufferElement with the given <paramref name="name"/> exists anywhere within 
        /// the descendants of this collection.
        /// </summary>
        /// <param name="name">The element name for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        public bool ContainsElementNested(string name)
        {
            bool result = ContainsElement(name);

            if (!result)
            {
                foreach (IBufferElement value in Elements)
                {
                    if (value is IElementCollection)
                    {
                        result = (value as IElementCollection).ContainsElementNested(name);
                    }

                    if (result)
                    {
                        break;
                    }
                }

            }

            return result;
        }




        /// <summary>
        /// Returns a deep copy of this element object, applying <paramref name="name"/> as the duplicate object's new 
        /// Name, and offsetting the new object's position by the amount given in <paramref name="bufferPositionOffset"/>.
        /// The new object's Parent is the same as this object's Parent.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="bufferPositionOffset">The amount by which to adjust the new object's position.</param>
        /// <param name="arrayIndexes">The indices of this element and possibly its parents if this element is part of 
        /// an array and/or nested array.</param>
        /// <returns>A new IBufferElement instance of the same type as this object.</returns>
        public IBufferElement Duplicate(string name, int bufferPositionOffset, IEnumerable<int> arrayIndexes)
        {
            return Duplicate(name, bufferPositionOffset, this.Parent, arrayIndexes);
        }


        /// <summary>
        /// Returns a deep copy of this element object, applying <paramref name="name"/> as the duplicate object's new 
        /// Name, and offsetting the new object's position by the amount given in <paramref name="bufferPositionOffset"/>.
        /// The new object is re-parented to <paramref name="newParent"/>.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="bufferPositionOffset">The amount by which to adjust the new object's position.</param>
        /// <param name="newParent">The IElementCollection which will be the new object's Parent.</param>
        /// <param name="arrayIndexes">The indices of this element and possibly its parents if this element is part of 
        /// an array and/or nested array.</param>
        /// <returns>A new IBufferElement instance of the same type as this object.</returns>
        public IBufferElement Duplicate(string name, int bufferPositionOffset, IElementCollection newParent, IEnumerable<int> arrayIndexes)
        {
            return InternalDuplicate(name, bufferPositionOffset, newParent, arrayIndexes);
        }

        /// <summary>
        /// Informs the object that its period of definition has ended. 
        /// </summary>
        public void EndDefinition()
        {
            if (IsDefining)
            {
                cachedLength = CalculateLength();
                IsDefining = false;
            }
        }
        public void RestartDefinition()
        {
            IsDefining = true;
        }

        /// <summary>
        /// Returns an IBufferElement object if one is found with the given <paramref name="name"/> anywhere in the 
        /// collection's child hierarchy.
        /// </summary>
        /// <param name="name">The element name for which to search.</param>
        /// <returns>A matching element object, if found.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is null or empty.</exception>
        /// <exception cref="FieldCollectionException">The given <paramref name="name"/> is not found.</exception>
        public IBufferElement GetElementByNameNested(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");

            IBufferElement result = arrayElements.Where(e => e.Name == name).Single();

            if (result == null)
            {
                foreach (IBufferElement value in arrayElements)
                {
                    if (value is IElementCollection)
                    {
                        result = (value as IElementCollection).GetElementByNameNested(name);
                    }

                    if (result != null)
                        break;
                }
            }

            return result;

        }

        /// <summary>
        /// Adds the given <paramref name="element"/> to this object's direct-child elements. 
        /// </summary>
        /// <remarks>
        /// <para>Time and design constraints have forced this method to be here, in IElementCollection, which was supposed 
        /// to be basically a readonly-collection, with adding done elsewhere. I'd love to see this fixed.</para>
        /// <para><note><paramref name="element"/> is declared here as an IBufferElement
        /// rather than as a <typeparamref name="TItem"/> because of casting issues elsewhere.</note></para>
        /// </remarks>
        public void AddChildElement(IBufferElement element)
        {
            AddArrayElement((TItem)element);
        }

        /// <summary>
        /// Returns a count of all elements beneath this collection, at any level. 
        /// </summary>
        /// <returns>
        /// A recursive count of all descendant elements.
        /// </returns>
        public int GetNestedElementCount()
        {
            return ElementCollectionCompositor.GetNestedElementCount(Elements);
        }

        /// <summary>
        /// Causes the object to restore its value (or its children's values) to its original data.
        /// </summary>
        public void ResetToInitialValue()
        {
            foreach (IBufferValue item in Elements)
            {
                item.ResetToInitialValue();
            }
        }

        /// <summary>
        /// Initializes value with hex 00 unless default value has been supplied
        /// </summary>
        public void InitializeWithLowValues()
        {
            if (this.IsInRedefine)
                return;
            foreach (IBufferValue item in Elements)
            {
                item.InitializeWithLowValues();
            }
        }

        /// <summary>
        /// Creates a deep copy of the current array object.
        /// </summary>
        /// <returns>Returns a new instance of the ArrayBase object.</returns>
        public object Clone()
        {
            ArrayBase<TItem> aBase = (ArrayBase<TItem>)this.MemberwiseClone();

            aBase.arrayElements = new List<TItem>(aBase.ArrayElements);

            return aBase;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        public void SetReferenceTo(IRecord record)
        {
            this.Record = record;
            this.Buffer = record.Buffer;

            foreach (IBufferElement item in this.ArrayElements)
            {
                item.SetReferenceTo(record);
            }
        }

        #endregion
    }
}
