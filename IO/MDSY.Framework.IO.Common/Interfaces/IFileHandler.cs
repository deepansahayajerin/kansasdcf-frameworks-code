using MDSY.Framework.Buffer.Interfaces;
using System.Collections.Generic;

namespace MDSY.Framework.IO.Common
{
    /// <summary>
    /// Defines an object which manages a collection of <see cref="IFileLink"/>s. 
    /// </summary>
    /// <remarks>
    /// <note>IFileHandler and IFileLink are abstractions to allow multiple datasource I/O implementations 
    /// which emulate COBOL file I/O. </note>
    /// </remarks>
    public interface IFileHandler
    {

    #region Attributes

        /// <summary>
        /// Gets an internal IDictionary(string, IFileLink) for caching of IFileLink objects.
        /// </summary>
        IDictionary<string, IFileLink> Files { get; set; }

        /// <summary>
        /// Gets or sets a boolean value to indicate wheter the implementing object is under debugging.  Primarily used in messaging methods.
        /// </summary>
        bool IsDebugging { get; set; }

    #endregion

    #region Operations

        /// <summary>
        /// Adds the given <paramref name="fileLink"/> to the internal IFileLink collection using 
        /// <paramref name="fileLink"/>.DDName as the key value.
        /// </summary>
        /// <param name="fileLink"></param>
        void Add(IFileLink fileLink);
            

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logicalFileName"></param>
        /// <returns></returns>
        IFileLink GetFile(string logicalFileName);

        /// <summary>
        /// Sets up Dynamic File selection
        /// </summary>
        /// <param name="logicalFileName"></param>
        /// <returns></returns>
        IFileLink GetFileUsing(IBufferValue dynamicFileName);
        
        /// <summary>
        /// Returns an IFileLink object associated with the file/datasource specified by the given <paramref name="name"/>.
        /// </summary>
        /// <remarks>Implementing objects should create new IFileLink objects on first access to a file, then cache the IFileLink internally for future access.</remarks>
        /// <param name="name"></param>
        /// <returns>The IFileLink associated with the given name.</returns>
        IFileLink LoadFile(string name);

        IFileLink LoadFile(string DDName, string ddValue, FileType FileType);

        ///// <summary>
        ///// Removes the given <paramref name="fileLink"/> from the internal IFileLink collection.
        ///// </summary>
        ///// <param name="fileLink"></param>
        void Remove(IFileLink fileLink);

        ///// <summary>
        ///// Removes the IFileLink with the given <paramref name="name"/> from the internal IFileLink collection.
        ///// </summary>
        ///// <param name="name"></param>
        void Remove(string name);

        /// <summary>
        /// Writes console error message about a file. 
        /// </summary>
        /// <param name="message"></param>
        void SendError(string message);

        /// <summary>
        /// Writes console debug message about a file. 
        /// </summary>
        /// <param name="message"></param>
        void SendDebugMessage(string message); /// <summary>
       
        /// <summary>
        /// Writes console informational message about a file. 
        /// </summary>
        /// <param name="message"></param>
        void SendMessage(string message);

        ///  Writes console warning message about a file. 
        /// </summary>
        /// <param name="message"></param>
        void SendWarning(string message);
        
        /// <summary>
        /// Gets the IFileLink object associated with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IFileLink this[string name] { get; }

        /// <summary>
        /// Gets Catalog information for givin string name
        /// </summary>
        /// <returns></returns>
        ICatalogFileInfo GetCatalogFileInfo(string fileName);

        /// <summary>
        /// Create a catalog file entry
        /// </summary>
        /// <param name="catalogFileName"></param>
        /// <param name="catalogRecordLength"></param>
        /// <param name="catalogFileType"></param>
        /// <param name="catalogFileOrganization"></param>
        /// <param name="storageUnit"></param>
        /// <returns></returns>
        bool CreateCatalogFile(string catalogFileName, int catalogRecordLength, FileOrganization catalogFileOrganization, FileType catalogFileType,  string storageUnit);

        /// <summary>
        /// Delete Catalog File Entry
        /// </summary>
        /// <param name="catalogFileName"></param>
        /// <returns></returns>
        bool DeleteCatalogFile(string catalogFileName);


        /// <summary>
        /// Update VSAM Catalog File Entry
        /// </summary>
        /// <param name="catalogFileName">File Name</param>
        /// <returns></returns>
        bool UpdateVsamCatalogFile(string catalogFileName);

        /// <summary>
        /// Create a VSAM-SQLServer catalog file entry
        /// </summary>
        /// <param name="catalogFileName">Table name</param>
        /// <param name="catalogStoragePath">Table's SQL server and catalog details</param>
        /// <param name="catalogRecordLength">Record size</param>
        /// <param name="catalogFileType">VSAM file type</param>
        /// <param name="catalogFileOrganization">File organization wether indexed, sequential, flat etc.</param>
        /// <param name="catalogVsamKeys">VSAM file keys</param>
        /// <param name="catalogSegmentName">Copybook name</param>
        /// <param name="storageUnit">Catalog file storage area</param>
        /// <returns></returns>
        bool CreateVsamCatalogFile(string catalogFileName, string catalogStoragePath, int catalogFileType, FileOrganization catalogFileOrganization, int catalogRecordLength, string catalogVsamKeys, string catalogSegmentName, string storageUnit);

    #endregion

    }
}       