using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object for IArrayElementAccessor(of TItem).
    /// </summary>
    /// <typeparam name="TItem"></typeparam>

    public interface IEditableArrayElementAccessor<TItem>
            where TItem : IArrayElement
    {
        /// <summary>
        /// Adds the given <paramref name="element"/> to the internal elements list.
        /// </summary>
        /// <param name="element">Elemet to be aaded to the elements list</param>
        void AddElement(TItem element);

        /// <summary>
        /// Returns an IArrayElementACcessor(of TItem) version of the object.
        /// </summary>
        /// <returns></returns>
        IArrayElementAccessor<TItem> AsReadOnly();
    }
}
