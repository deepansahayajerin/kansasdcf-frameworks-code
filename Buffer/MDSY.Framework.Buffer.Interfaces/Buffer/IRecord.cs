using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using System.ComponentModel;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which describes a data structure (made up of IFields, IGroups and other 
    /// IBufferElements) which overlays a buffer array of data.
    /// </summary>
    [InjectionInterface]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface IRecord : IElementCollection, IAssignable, ICloneable,
        IComparable<IField>, IComparable<IGroup>, IComparable<IRecord>, IComparable<string>,
        IEquatable<IField>, IEquatable<IGroup>, IEquatable<IRecord>, IEquatable<string>
    {
        #region attributes
        /// <summary>
        /// Gets or sets the record object's data buffer object. This is injected by Unity at build-up time using a 
        /// temporary, List(of byte)-based buffer. Later it is replaced with a byte array-buffer.
        /// </summary>
        IDataBuffer Buffer { get; set; }
        /// <summary>
        /// Gets and sets initialilzed buffer
        /// </summary>
        byte[] ResetBuffer { get; set; }

        /// <summary>
        /// Gets the length of this record (in bytes) as defined by the elements making up the record's structure.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets the name of this record object.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int RecordKey { get; set; }
        #endregion

        #region operations
        /// <summary>
        /// Adds the given <paramref name="element"/> to a collection of all the buffer elements owned by 
        /// this record. Element names must be unique. 
        /// </summary>
        /// <remarks>
        /// <para><note>This is a separate collection from any child elements owned by this record as an 
        /// IElementCollection. The structure elements are a collection of all elements under this record 
        /// at any level. i.e. Children, grandchildren, g'grandchildren, etc.</note></para>
        /// </remarks>
        /// <param name="element">The IBufferElement element to be added to the collection.</param>
        void AddStructureElement(IBufferElement element);

        /// <summary>
        /// Returns <c>true</c> if this record has among its structural elements the given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The IBufferElement element for which to search.</param>
        /// <returns><c>true</c> if the element exists in the StructuralElements collection.</returns>
        bool ContainsStructuralElement(IBufferElement element);

        /// <summary>
        /// Returns <c>true</c> if this record has among its structural elements an element with the given <paramref name="elementName"/>.
        /// </summary>
        /// <param name="elementName">The element for which to search.</param>
        /// <returns></returns>
        bool ContainsStructuralElement(string elementName);

        /// <summary>
        /// Returns a readonly collection of all the IBufferElements owned by this record at any level.
        /// </summary>
        /// <returns>An unsorted, flattened collection of IBufferElements.</returns>
        IEnumerable<IBufferElement> GetStructureElements();

        /// <summary>
        /// Restores the buffer pointer mapping of this record to its original 
        /// IDataBuffer object.
        /// </summary>
        /// <remarks>If SetAddressToAddressOf() has never been called, this method
        /// will have no effect.</remarks>
        void RestoreInitialDataBuffer();

        /// <summary>
        /// Resets Record buffer with Initial Values
        /// </summary>
        void ResetInitialValue();

        /// <summary>
        /// Gets A substring of the record buffer 
        /// </summary>
        /// <param name="startPos">The start position for the substring</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns></returns>
        string GetSubstring(int startPos, int length);

        /// <summary>
        /// Gets the Record Buffer Address Key for Buffer pointer operations
        /// </summary>
        /// <returns></returns>
        int GetBufferAddressKey();

        /// <summary>
        /// Functions as COBOL statement <c>SET ADDRESS OF RECORD_A TO ADDRESS OF RECORD_B</c>.
        /// Causes the record object to point its buffer reference to the 
        /// buffer of the given <paramref name="record"/>.
        /// </summary>
        /// <param name="record">The record object whose DataBuffer this record 
        /// will now point to.</param>
        void SetAddressToAddressOf(IRecord record);

        /// <summary>
        /// Returns the structure element with the given name, if it exists within the record at any level.
        /// </summary>
        /// <param name="name">Specifies the element's name.</param>
        /// <returns>The appropriate IBufferElement, if found; <c>null</c> otherwise.</returns>
        IBufferElement StructureElementByName(string name);

        /// <summary>
        /// Returns an IArrayElementAccessor of the given <typeparamref name="TItem"/> type
        /// if one is found with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Specifies the element's name.</param>
        IArrayElementAccessor<TItem> GetArrayElementAccessor<TItem>(string name) where TItem : IArrayElement;


        #endregion

    }

}
