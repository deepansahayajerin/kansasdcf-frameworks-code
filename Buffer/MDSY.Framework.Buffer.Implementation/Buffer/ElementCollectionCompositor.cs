using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Interfaces;
using System.ComponentModel;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Serves as an internal provider for IFieldCollection implementers. Instances can be composited 
    /// into IElementCollection implementers for common provision of collection behavior.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    internal sealed class ElementCollectionCompositor 
    {
        #region private fields...
        private static readonly object synclock = new Object();
        private Dictionary<string, IBufferElement> elements = new Dictionary<string, IBufferElement>();
        #endregion

        #region private methods
        private IEnumerable<IBufferElement> GetElements()
        {
            return elements.Values.AsEnumerable<IBufferElement>();
        }

        private bool GetHasArrayInChildElements()
        {
            bool result = elements.Values.Any(e => e is IArrayBase);

            if (!result)
            {
                foreach (IBufferElement item in elements.Values)
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

        #region constructors
        /// <summary>
        /// Initializes a new instance of the ElementCollectionCompositor class.
        /// </summary>
        /// <param name="owner">A reference to the owner collection of the elements.</param>
        public ElementCollectionCompositor(IElementCollection owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner", "owner is null.");
            Owner = owner;
        }

        /// <summary>
        /// Initializes a new instance of the ElementCollectionCompositor class. Hidden to prevent owner-less compositors.
        /// </summary>
        private ElementCollectionCompositor()
        {

        }

        #endregion

        #region internal properties
        /// <summary>
        /// Gets the number of elements contained by this object.
        /// </summary>
        internal int Count
        {
            get { return elements.Count; }
        }

        /// <summary>
        /// Returns <c>true</c> if some child element down the record structure tree is an IArray.
        /// </summary>
        internal bool HasArrayInChildElements
        {
            get { return GetHasArrayInChildElements(); }
        }

        /// <summary>
        /// Returns the number of the nested elements.
        /// </summary>
        public int NestedElementCount
        {
            get { return GetNestedElementCount(Elements); }
        }


        /// <summary>
        /// Gets or sets the IElementCollection object that owns this object.
        /// </summary>
        internal IElementCollection Owner { get; set; }
        #endregion

        #region internal methods
        internal static int GetNestedElementCount(IEnumerable<IBufferElement> elementCollection)
        {
            int result = 0;

            foreach (IBufferElement element in elementCollection)
            {
                var collection = element as IElementCollection;
                if (collection != null)
                {
                    result++; // one for the collection + its count.
                    result += collection.GetNestedElementCount();
                }
                else
                {
                    result++;
                    if (element is IField)
                    {
                        result += (element as IField).CheckFields.Count();
                    }
                }
            }

            return result;
        }
        #endregion

        #region element collection support

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
                foreach (IBufferElement value in elements.Values)
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
                foreach (IBufferElement value in elements.Values)
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
        /// Returns a byte array made up of all the values of the elements of this collection.
        /// </summary>
        /// <param name="includeRedefinition">Specifies whether redefining values should be included.</param>
        /// <returns>Returns a byte array made up of all the values of the elements of this collection.</returns>
        public byte[] GetValuesAsBytes(bool includeRedefinition = false)
        {
            List<byte> result = new List<byte>();

            foreach (IBufferElement value in elements.Values)
            {
                if (value is IBufferValue)
                {
                    if (includeRedefinition || !value.IsARedefine)
                    {
                        result.AddRange((value as IBufferValue).AsBytes);
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns a string array made up of all the values of the elements of this collection.
        /// </summary>
        /// <returns></returns>
        /// <param name="includeRedefinition"></param>
        public string[] GetValuesAsString(bool includeRedefinition = false)
        {
            List<string> result = new List<string>();

            foreach (IBufferElement value in elements.Values)
            {
                if (value is IBufferValue)
                {
                    if (includeRedefinition || !(value is IRedefinition))
                    {
                        result.Add((value as IBufferValue).DisplayValue);
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns the IBufferElement with the given name.
        /// </summary>
        internal IBufferElement this[string name]
        {
            get { return GetElementByNameNested(name); }
        }

        /// <summary>
        /// Gets the collection of child element objects.
        /// </summary>
        internal IEnumerable<IBufferElement> Elements
        {
            get { return GetElements(); }
        }

        internal Dictionary<string, IBufferElement> ChildCollection
        {
            get { return elements; }
        }

        /// <summary>
        /// Adds the given <paramref name="element"/> of the specified IBufferElement-descendant
        /// type (<typeparamref name="T"/>) to the element collection.
        /// </summary>
        /// <typeparam name="T">The element type to be added; must implement IBufferElement.</typeparam>
        /// <param name="element">The element, of type <typeparamref name="T"/> to be added to the collection.</param>
        internal void Add<T>(T element)
            where T : IBufferElement
        {
            lock (synclock)
            {
                elements.Add(element.Name, element);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if an IBufferElement with the given <paramref name="name"/> exists in 
        /// this field collection.
        /// </summary>
        /// <param name="name">The element name for which to search.</param>
        /// <returns><c>true</c> if the element exists.</returns>
        internal bool ContainsElement(string name)
        {
            return elements.ContainsKey(name);
        }


        /// <summary>
        /// Returns <c>true</c> if this collection already contains the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The IBufferElement for which to search.</param>
        /// <returns><c>true</c> if the element exists in the collection.</returns>
        internal bool ContainsElement(IBufferElement element)
        {
            return elements.Values.Contains(element);
        }



        /// <summary>
        /// Returns an IBufferElement object if one is found with the given <paramref name="name"/> anywhere in the 
        /// collection's child hierarchy.
        /// </summary>
        /// <param name="name">The element name for which to search.</param>
        /// <returns>A matching element object, if found.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is null or empty.</exception>
        /// <exception cref="FieldCollectionException">The given <paramref name="name"/> is not found.</exception>
        internal IBufferElement GetElementByNameNested(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");
            
            IBufferElement result = null;
            if (elements.ContainsKey(name))
            {
                result = elements[name];
            }

            if (result == null)
            {
                foreach (IBufferElement value in elements.Values)
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

        internal ElementCollectionCompositor Clone()
        {
            ElementCollectionCompositor thisClone = (ElementCollectionCompositor)this.MemberwiseClone();
            thisClone.elements = new Dictionary<string, IBufferElement>();
            foreach (string key in this.elements.Keys)
            {
                thisClone.elements.Add(key, (IBufferElement)this.elements[key].Clone());
            }
            return thisClone;
        }
        #endregion


    }
}
