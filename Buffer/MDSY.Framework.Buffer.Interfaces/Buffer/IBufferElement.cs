using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MDSY.Framework.Buffer.Common;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which exists within a buffer definition. 
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface IBufferElement: ICloneable
    {

        #region attributes
        /// <summary>
        /// Returns <c>true</c> if some parent object up the record structure tree is also an IArray. 
        /// </summary>
        bool HasArrayInParents { get; }

        /// <summary>
        /// Gets a value indicating whether this element has been declared as 'FILLER'. 
        /// </summary>
        /// <remarks>
        /// If an element is decorated as FILLER, it means that while the element takes up space in the 
        /// buffer, the program will not be referencing the element by name. In COBOL, items marked FILLER
        /// are not given names.
        /// </remarks>
        bool IsFiller { get; }

        /// <summary>
        /// Gets a value indicating whether this element resides beneath an array.
        /// </summary>
        bool IsInArray { get; }

        /// <summary>
        /// Returns <c>true</c> if this element or any parent element implements IRedefine.
        /// </summary>
        bool IsInRedefine { get; }

        /// <summary>
        /// Returns <c>true</c> if this element is a IRedefine.
        /// </summary>
        bool IsARedefine { get; set; }

        /// <summary>
        /// Gets the number of bytes occupied in the buffer by this element.
        /// </summary>
        int LengthInBuffer { get; }

        /// <summary>
        /// Gets the nesting level for this buffer element. i.e. how many parents between this element and the record.
        /// A direct child of the record is Level=0. 
        /// </summary>
        int Level { get; }

        /// <summary>
        /// Gets the name of this buffer element.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// If HasArrayInAncestors is <c>true</c>, returns the closest ancestor IArray object, bypassing intervening 
        /// non-array parents. 
        /// </summary>
        IArrayBase NextArrayParent { get; }

        /// <summary>
        /// Gets the <see cref="IElementCollection"/> implementation which is the immediate parent of this element.
        /// </summary>
        IElementCollection Parent { get; set; }

        /// <summary>
        /// Gets the byte index of this element within the Buffer.
        /// </summary>
        int PositionInBuffer { get; }

        /// <summary>
        /// Gets the byte index of this element within its Parent.
        /// </summary>
        int PositionInParent { get; }

        /// <summary>
        /// Gets or sets the IRecord object which is the root owner of this object.
        /// </summary>
        IRecord Record { get; set; }

        /// <summary>
        /// Field Justification setting
        /// </summary>
        FieldFormat FieldJustification { get; set; }
        #endregion

        #region operations
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
        IBufferElement Duplicate(string name, int bufferPositionOffset, IEnumerable<int> arrayIndexes);

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
        IBufferElement Duplicate(string name, int bufferPositionOffset, IElementCollection newParent, IEnumerable<int> arrayIndexes);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        void SetReferenceTo(IRecord record);
        #endregion
    }
}
