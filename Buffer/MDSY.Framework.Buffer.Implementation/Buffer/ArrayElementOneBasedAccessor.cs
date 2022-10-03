using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
using System.Text;
//CHADusing Unity.Attributes;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// One-based Implementation of the injection interface IArrayElementAccessor(of IArrayElementAccessor).
    /// </summary>
    [InjectionImplementer(typeof(IArrayElementAccessor<>))]
    [Serializable]
    internal sealed class ArrayElementOneBasedAccessor<TItem> : ArrayElementAccessorBase<TItem>
        where TItem : IArrayElement, IBufferElement
    {
        /// <summary>
        /// Implements IndexOffset amount for one-based arrays. 
        /// Returns one so that array indexes will be adjusted accordingly. 
        /// </summary>
        /// <returns>Returns the retrieved offset value.</returns>
        protected override int GetIndexOffset()
        {
            return 1;
        }
    }
}
