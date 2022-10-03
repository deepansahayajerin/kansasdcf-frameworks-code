using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using System.ComponentModel;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which represents a group of child elements (IFields and/or other nested IGroups)
    /// in an IRecord or IGroup.
    /// </summary>
    [InjectionInterface]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface IGroup : IBufferElement, IElementCollection, IBufferValue, IArrayElement,
        IComparable<IField>, IComparable<IGroup>, IComparable<IRecord>, IComparable<string>,
        IEquatable<IField>, IEquatable<IGroup>, IEquatable<IRecord>, IEquatable<string>
    {
        /// <summary>
        /// Causes the group object to point its buffer reference to the 
        /// buffer of the given <paramref name="element"/>.
        /// </summary>
        /// <typeparam name="T">Type of the element.</typeparam>
        /// <param name="element">The IGroup whose buffer address will be stored.</param>
        void SetAddressToAddressOf<T>(T element) where T : IBufferElement, IBufferValue;

        /// <summary>
        /// Causes the group object to point its buffer reference to the 
        /// buffer of the given <paramref name="recordBuffer"/>.
        /// </summary>
        /// <param name="recordBuffer">The IRecord whose buffer address will be stored.</param>
        void SetAddressToAddressOf(IRecord recordBuffer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        void GetAcceptData(string text);

        int BufferAddress { get; }
    }
}
