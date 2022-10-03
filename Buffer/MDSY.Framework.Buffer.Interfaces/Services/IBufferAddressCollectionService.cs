using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Provides utilities for managing buffer arrays collection.
    /// </summary>
    [InjectionInterface]
    public interface IBufferAddressCollectionService 
    {
        /// <summary>
        /// Add IBufferAddress to Buffer Address collection
        /// </summary>
        /// <param name="item">Item to be added to the collection.</param>
        /// <returns></returns>
        int Add(IBufferAddress item);

        /// <summary>
        /// Get the IBufferAddress from Buffer Address collection
        /// </summary>
        /// <param name="RecKey">The key used to retrieve the IBufferAddress.</param>
        /// <returns>The IBufferAddress.</returns>
        IBufferAddress Get(int RecKey);
    }
}
