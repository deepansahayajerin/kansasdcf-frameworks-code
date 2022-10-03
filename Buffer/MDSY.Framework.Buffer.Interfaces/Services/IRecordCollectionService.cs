using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;

namespace MDSY.Framework.Buffer.Interfaces
{

    /// <summary>
    /// Provides access to records currently in the system.
    /// </summary>
    [InjectionInterface]
    public interface IRecordCollectionService 
    {
        /// <summary>
        /// Returns the IRecord object with the given <paramref name="name"/> 
        /// if it is found within the collection.
        /// </summary>
        /// <param name="name">The name for which to search.</param>
        /// <returns>The appropriate IRecord object if found, otherwise null.</returns>
        IRecord Get(string name);

        /// <summary>
        /// Returns the Irecord object with the given key
        /// </summary>
        /// <param name="RecKey">The key to get the IRecord.</param>
        /// <returns></returns>
        IRecord Get(int RecKey);

        /// <summary>
        /// Add IRecord to Record Collection.
        /// </summary>
        /// <param name="item">The IRecord item to add to the record collection.</param>
        /// <returns></returns>
        int Add(IRecord item);

        /// <summary>
        /// Remove IRecord from Record collection.
        /// </summary>
        /// <param name="item">The IRecord item to remove from the record collection.</param>
        void Remove(IRecord item);

        /// <summary>
        /// Get the key for the given IRecord in the Record Collection.
        /// </summary>
        /// <param name="item">The IRecord item in the Record Collection.</param>
        /// <returns></returns>
        int GetKeyFor(IRecord item);

        /// <summary>
        /// Clear all record entries
        /// </summary>
        void Clear();
    }
}
