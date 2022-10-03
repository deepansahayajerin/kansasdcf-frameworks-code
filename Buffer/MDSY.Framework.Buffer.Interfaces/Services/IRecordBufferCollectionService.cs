using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Contains a collection of IDataBuffers  
    /// </summary>
    /// <remarks><para>When IRecord.SetBufferReference() (or IRecord.SetAddressToAddressOf()) 
    /// is called, the instance record will be pointing at the given record's buffer.</para>
    /// <para>We need to keep a reference to the original IDataBuffer assigned to 
    /// the instance record so that we can re-assign the original buffer, if necessary.</para>
    /// </remarks>
    [InjectionInterface]
    public interface IRecordBufferCollectionService 
    {
        /// <summary>
        /// Adds the given <paramref name="dataBuffer"/> to the collection and returns 
        /// the <paramref name="dataBuffer"/>'s key in the collection. 
        /// The returned key is built from the given <paramref name="owningRecord"/>.Name.
        /// </summary>
        /// <remarks>If the item already exists in the collection, it is not 
        /// added, but we return the key of the extant object.</remarks>
        /// <param name="dataBuffer">The IDataBuffer to add to the collection.</param>
        /// <param name="owningRecord">The IRecord which owns the given <paramref name="dataBuffer"/>.</param>
        /// <returns>A key value for reaccessing the <paramref name="dataBuffer"/>.</returns>
        String Add(IDataBuffer dataBuffer, IRecord owningRecord);

        /// <summary>
        /// Get The IDataBuffer using the record key.
        /// </summary>
        /// <param name="RecKey">The record key.</param>
        /// <returns>IdataBuffer value.</returns>
        IDataBuffer Get(string RecKey);

        /// <summary>
        /// Get the record key for the given IDataBuffer.
        /// </summary>
        /// <param name="item">The IDataBuffer item.</param>
        /// <returns>The Record Key.</returns>
        string GetKeyFor(IDataBuffer item);

        /// <summary>
        /// Remove the IDataBuffer from Record buffer collection.
        /// </summary>
        /// <param name="key">The key to find the IDataBuffer.</param>
        void Remove(string key);

        /// <summary>
        /// Clear all record entries
        /// </summary>
        void Clear();
    }
}

