using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which manages an array of occurrences of type <typeparamref name="TItem"/>
    /// </summary>
    /// <typeparam name="TItem">The type of the IBufferElement-descendant to be managed.</typeparam>
    //[InjectionInterface]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface IArray<TItem> : IArrayBase
        where TItem : IBufferElement
    {
        /// <summary>
        /// Gets an occurrence of SourceElement as indicated by <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Specifies the SourceElement</param>
        /// <returns>An occurence of Source Element</returns>
        TItem this[int index] { get; }
    }

}
