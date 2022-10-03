using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer;
using System.Text;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IArrayElementAccessor(of IArrayElementAccessor).
    /// </summary>
    [InjectionImplementer(typeof(IArrayElementAccessor<>))]
    [Serializable]
    internal sealed class ArrayElementAccessor<TItem> : ArrayElementAccessorBase<TItem>,
        IArrayElementAccessor<TItem>
        where TItem : IArrayElement, IBufferElement
    {
        /// <summary>
        /// Retrieves the amount by which the client code-given array index will 
        /// be adjusted in order to match the zero-based arrays of .NET.
        /// </summary>
        /// <returns>Returns the retrieved offset value.</returns>
        protected override int GetIndexOffset()
        {
            return 0;
        }
    }

}
