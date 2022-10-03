using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Interfaces;
using System.ComponentModel;
using MDSY.Framework.Buffer.Common;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Base class for objects which would implement IBufferElement.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    internal abstract class BufferElementBase : IArrayElement, IBufferElementInitializer
    {
        #region private fields
        private bool isFiller = false;
        private int level = -1;
        private int positionInBuffer = -1;
        #endregion

        #region abstracts
        /// <summary>
        /// Returns the length (in bytes) of this buffer element in the buffer.
        /// </summary>
        protected abstract int GetLength();

        /// <summary>
        /// Where appropriate, sets the length of this buffer element.
        /// </summary>
        protected virtual void SetLength(int value)
        {
            // do nada - override to actually set length.
        }
        #endregion

        #region private methods

        private int GetBufferElementLevel()
        {
            int result = 0;
            var recursParent = Parent;

            while (!(recursParent is IRecord))
            {
                result++;
                recursParent = (recursParent as IBufferElement).Parent;
            }

            return result;
        }

        private bool GetHasArrayInAncestors()
        {
            return (GetNextArrayParent() != null);
        }

        private IArrayBase GetNextArrayParent()
        {
            IArrayBase result = null;
            var parent = this.Parent;
            while ((result == null) && (parent != null))
            {
                if (parent is IArrayBase)
                {
                    result = parent as IArrayBase;
                }
                else
                {
                    if (parent is IBufferElement)
                    {
                        parent = (parent as IBufferElement).Parent;
                    }
                    else
                    {
                        parent = null;
                    }
                }
            }
            return result;
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets the index of the element within its array. Do not set ArrayElementIndex; its setter is used 
        /// only for internal initialization. 
        /// </summary>
        [Category("IArrayElement")]
        [Description("Index of this element within its array, if applicable.")]
        public int ArrayElementIndex { get; set; }

        /// <summary>
        /// The buffer object to and from which values are normally stored and retrieved. 
        /// </summary>
        [Category("IBufferValue")]
        [Description("Buffer object for this element.")]
        public IDataBuffer Buffer { get; set; }

        /// <summary>
        /// Returns <c>true</c> if some parent object up the record structure tree is also an IArray. 
        /// </summary>
        [Category("IBufferElement")]
        [Description("Indicates whether a parent is an IArray")]
        public bool HasArrayInParents
        {
            get { return GetHasArrayInAncestors(); }
        }

        /// <summary>
        /// Gets a value indicating whether this element has been declared as 'FILLER'. 
        /// </summary>
        /// <remarks>
        /// If an element is decorated as FILLER, it means that while the element takes up space in the 
        /// buffer, the program will not be referencing the element by name. In COBOL, items marked FILLER
        /// are not given names.
        /// </remarks>
        [Category("IBufferElement")]
        [Description("Indicates whether this element is marked 'FILLER'.")]
        public bool IsFiller
        {
            get { return isFiller; }
            set { isFiller = value; }
        }

        /// <summary>
        /// Gets a cached (non-calculated) value indicating whether this element resides beneath an array.
        /// </summary>
        [Category("IBufferElement")]
        [Description("Indicates whether this element is in an array.")]
        public bool IsInArray { get; set; }

        /// <summary>
        /// Returns <c>true</c> if this element or any parent element implements IRedefine.
        /// </summary>
        [Category("IBufferElement")]
        [Description("Indicates whether this element exists within a redefinition.")]
        public bool IsInRedefine
        {
            get
            {
                bool result = this is IRedefinition;

                if (!result && (Parent != null) && (Parent is IBufferElement))
                {
                    result = (Parent as IBufferElement).IsInRedefine;
                }

                return result;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if this element is a redefines
        /// </summary>
        [Category("IBufferElement")]
        [Description("Indicates whether this element is a redefines element")]
        public bool IsARedefine { get; set; }

        /// <summary>
        /// Gets the number of bytes occupied in the buffer by this element.
        /// </summary>
        [Category("IBufferElement")]
        [Description("Indicates number of bytes this element occupies in the buffer.")]
        public int LengthInBuffer
        {
            get { return GetLength(); }
            set { SetLength(value); }
        }

        /// <summary>
        /// Gets the nesting level for this buffer element. i.e. how many parents between this element and the record.
        /// A direct child of the record is Level=0. 
        /// </summary>
        [Category("IBufferElement")]
        [Description("Nesting level (number of parent elements between this element and the record) for this element.")]
        public int Level
        {
            get { return GetLevel(); }
        }


        /// <summary>
        /// Gets the name of this buffer element.
        /// </summary>
        [Category("IBufferElement")]
        [Description("Name of this element")]
        public string Name { get; set; }

        /// <summary>
        /// If HasArrayInAncestors is <c>true</c>, returns the closest ancestor IArray object, bypassing intervening 
        /// non-array parents. 
        /// </summary>
        [Category("IBufferElement")]
        [Description("Gets the closest parent IArray object")]
        public IArrayBase NextArrayParent
        {
            get { return GetNextArrayParent(); }
        }

        /// <summary>
        /// Gets the <see cref="IElementCollection"/> implementation that is the immediate parent of this element.
        /// </summary>
        [Category("IBufferElement")]
        [Description("Gets the parent element of this element.")]
        public IElementCollection Parent { get; set; }

        /// <summary>
        /// Gets the byte index of this element within the Buffer.
        /// </summary>
        [Category("IBufferElement")]
        [Description("Byte index of this element within the buffer.")]
        public int PositionInBuffer
        {
            get
            {
                //return GetPositionInBuffer();
                if (positionInBuffer == -1)
                    positionInBuffer = GetPositionInBuffer();
                return positionInBuffer;
            }

        }

        /// <summary>
        /// Gets the byte index of this element within its Parent.
        /// </summary>
        [Category("IBufferElement")]
        [Description("Byte index of this element within its Parent object.")]
        public int PositionInParent { get; set; }

        /// <summary>
        /// Gets the IRecord object which is the root owner of this object.
        /// </summary>
        [Category("IBufferElement")]
        [Description("The IRecord object which is the root owner of this element.")]
        public IRecord Record { get; set; }
        #endregion

        #region protected methods
        /// <summary>
        /// Returns a string representation of the given <paramref name="bytes"/>.
        /// </summary>
        /// <param name="bytes">An array of bytes for conversion to string.</param>
        /// <returns>Returns a string that contains the value obtained from the provided array of bytes.</returns>
        protected static string GetBytesAsString(byte[] bytes)
        {
            return (bytes == null || bytes.Length == 0) ?
                       string.Empty :
                       bytes.Select(b => (AsciiChar)b).NewString();
        }

        /// <summary>
        /// Retrieves an integer value representing how deep in the data structure tree this element resides.
        /// </summary>
        /// <returns>Returns an integer value representing how deep in the data structure tree this element resides.</returns>
        protected virtual int GetLevel()
        {
            if (level == -1)
            {
                level = GetBufferElementLevel();
            }
            return level;
        }

        /// <summary>
        /// Calculates and returns the start index of this object within the buffer, based on parent position. 
        /// </summary>
        /// <remarks>Redefine objects should override this method to return a proper position.</remarks>
        /// <returns>Returns the start index of the current object within the buffer.</returns>
        protected virtual int GetPositionInBuffer()
        {
            int result = PositionInParent;

            if (Parent is IBufferElement)
            {
                result += (Parent as IBufferElement).PositionInBuffer;
            }

            return result;
        }

        /// <summary>
        /// Writes the given <paramref name="bytes"/> to the buffer at this object's position. 
        /// </summary>
        /// <param name="bytes">An array of bytes to be written to the buffer.</param>
        protected void WriteBytes(byte[] bytes)
        {
            //
            //if (bytes.Length < LengthInBuffer)
            //    Buffer.WriteBytes(bytes, PositionInBuffer, bytes.Length);
            //else
            Buffer.WriteBytes(bytes, PositionInBuffer, Math.Min(bytes.Length, LengthInBuffer));
        }
        #endregion

        #region public methods
        /// <summary>
        /// Sets the field value object's internal bytes all to the given <paramref name="clearByte"/>. 
        /// </summary>
        /// <param name="clearByte">Specifies the byte value to be written to the buffer.</param>
        public void Clear(byte clearByte)
        {
            WriteBytes(Enumerable.Repeat<byte>(clearByte, LengthInBuffer).ToArray());
        }

        /// <summary>
        /// Sets the field value object's internal bytes to null bytes (0x00). 
        /// </summary>
        public void Clear()
        {
            Clear(0x00);
        }

        [Obsolete("This is redundant with IAssignable.AssignFrom(bytes); if you need this, talk to Robert about why.", false)]
        public void SetBytes(byte[] valueBytes)
        {
            WriteBytes(valueBytes);
        }


        /// <summary>
        /// Returns the collection of indexes of the element within multiple nested arrays, if present.
        /// </summary>
        public IEnumerable<int> GetArrayElementIndexes()
        {
            List<int> result = new List<int>();
            if (IsInArray)
            {
                if (Parent is IArrayBase)
                {
                    result.Insert(0, this.ArrayElementIndex);
                }

                var penElement = this.GetNextPenultimateArrayParent();
                while (penElement != null)
                {
                    result.Insert(0, (penElement as IArrayElement).ArrayElementIndex);
                    penElement = penElement.GetNextPenultimateArrayParent();
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the number of IArrayBase-implementing parents this element has, indicating how many nested
        /// array levels deep.
        /// </summary>
        public int GetArrayParentCount()
        {
            int result = 0;
            if (IsInArray)
            {
                var arrParent = NextArrayParent;
                while (arrParent != null)
                {
                    result++;
                    arrParent = arrParent.NextArrayParent;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the IArrayElement parent of this element that is directly below the next array above, not counting 
        /// this object itself. 
        /// If this object is not in an array, returns null.
        /// See example.
        /// </summary>
        /// <remarks>Traverses up the tree to find the parent element that is the direct child of the next array parent.</remarks>
        /// <example>
        /// Given the following structure, where this object is Field_A:
        /// <code>
        /// RECORD
        ///  - GROUPARRAY[10]
        ///    - GROUP01
        ///      - GROUP02
        ///        - Field_A
        /// </code>
        /// Calling <c>GetPenultimateArrayParent()</c> would return <c>GROUP01</c> since that is the direct child of 
        /// <c>GROUPARRAY</c>.
        /// </example>
        /// <returns>Returns the IArrayElement parent of this element.</returns>
        public IArrayElement GetNextPenultimateArrayParent()
        {
            IArrayElement result = null;

            if (IsInArray)
            {
                var nextParent = Parent as IBufferElement;
                while ((result == null) && (nextParent != null) && (nextParent is IBufferElement))
                {
                    if (nextParent.Parent is IArrayBase)
                    {
                        result = nextParent as IArrayElement;
                    }

                    nextParent = (nextParent as IBufferElement).Parent as IBufferElement;
                }
            }

            return result;
        }
        #endregion
    }
}
