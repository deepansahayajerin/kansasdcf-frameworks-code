using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Core;
using MDSY.Framework.Interfaces;

namespace MDSY.Framework.IO.Common
{
    /// <summary>
    /// Provides an optional base class for classes that implement IFileLink. 
    /// </summary>
    /// <remarks>
    /// <para>FileLinkBase introduces some default behaviors and several abstract methods which 
    /// need to be overridden by descendant classes.</para>
    /// <para>To make use of FileLinkBase, create a descendant class that also implements IFileLink 
    /// (see example).</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public sealed class MyFileLink : FileLinkBase, IFileLink
    /// {...}
    /// </code>
    /// </example>
    public abstract class FileLinkBase
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the FileLinkBase class.
        /// </summary>
        public FileLinkBase()
        {
            IsInitialized = false;
        }
        #endregion

        #region private methods
        //private IFileIO GetFileIoObject()
        //{
        //    return InversionContainer.GetImplementingObject<IFileIO>();
        //}
        #endregion

        #region static
        /// <summary>
        /// Optional Record object factory method. Unless you need to do something funky to create a new Record 
        /// object for your IFileLink, you can just call this.
        /// </summary>
        /// <returns></returns>
        //[Obsolete]
        //protected static IRecord NewRecordObject()
        //{
        //    return new Record_Old();
        //}

        //protected static IRecord NewRecordObject(Action<IStructureDefinition> recordDef)
        //{
        //    return MDSY.Framework.Buffer.BufferServices.Factory.NewRecord("Record", recordDef);
        //}

        //protected static IRecord NewRecordObject(int length)
        //{
        //    return NewRecordObject(rec => { rec.NewField("Text", FieldType.String, length); });
        //}

        #endregion

        #region abstracts
        /// <summary>
        /// Returns whether the file link object is pointing at a valid, extant file/datasource. 
        /// </summary>
        /// <remarks>
        /// Descendant classes should return <c>true</c> if the file/datasource link is valid 
        /// and the file/datasource is ready to be opened.
        /// </remarks>
        protected abstract bool GetIsLinked();

        /// <summary>
        /// Returns an MDSY.Framework.Buffer.Interfaces.IRecord object to contain data read from the file/datasource. 
        /// </summary>
        /// <remarks>
        //protected abstract IRecord GetRecord();

        /// <summary>
        /// Gets the length, in bytes, of the record in the file/datasource to be read or written to.
        /// </summary>
        protected abstract int GetRecordLength();

        /// <summary>
        /// Sets the length, in bytes, of the record in the file/datasource to be read or written to.
        /// </summary>
        protected abstract void SetRecordLength(int value);

        /// <summary>
        /// Processes any values passed into the object's Initialize() method via the property grab bag. 
        /// </summary>
        /// <remarks>
        /// Descendant classes should override HandlePropertyBag() to handle any values the object is expecting.
        /// </remarks>
        /// <param name="propertyBag">The value grab bag.</param>
        protected abstract void InternalInitialize(IDictionary<string, object> propertyBag);


        /// <summary>
        /// Closes the "file" that was opened by a call to OpenFile().
        /// </summary>
        public abstract void CloseFile();

        /// <summary>
        /// Deletes from the file/datastore the line of data indicated 
        /// by the given key value.
        /// </summary>
        public abstract void Delete(string key);

        /// <summary>
        /// Deletes the next line (or record, etc.) of the
        /// associated text file (or DB table, etc.)
        /// </summary>
        public abstract void Delete();

        /// <summary>
        /// Start read based on Key position
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="startOption"></param>
        /// <returns></returns>
        public abstract FileStatus StartRead(string keyValue, ReadOption startOption);

        /// <summary>
        /// Overwrites the most recently read line with the given line of text. Analogous to the COBOL REWRITE statement.
        /// </summary>
        /// <param name="line">The string value to write.</param>
        public abstract FileStatus RewriteLine(string line);

        /// <summary>
        /// Specifies the file/datasource which will be accessed by this file link object.
        /// </summary>
        /// <param name="filename">Physical filename if implementing object supports files; otherwise some datasource name as needed.</param>
        /// <param name="args"></param>
        public abstract void SelectFile(string filename, params string[] args);

        /// <summary>
        /// Opens the linked file/datasource for reading.
        /// </summary>
        public abstract void OpenInput(params string[] args);

        /// <summary>
        /// Opens the linked file/datasource for writing.
        /// </summary>
        /// <param name="args"></param>
        public abstract void OpenOutput(params string[] args);

        /// <summary>
        /// Opens the linked file/datasource for read/write operations.
        /// </summary>
        public abstract void OpenIO(params string[] args);

        /// <summary>
        /// Returns a string value as indicated by the given <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The type of the key's value.</typeparam>
        /// <param name="key">Contains a value (of type <typeparamref name="T"/>) which is meaningful
        /// to the IFileIO-implementing object.</param>
        public abstract string ReadLine(string key);

        /// <summary>
        /// Returns the next line (or record, etc.) of the 
        /// associated text file (or DB table, etc.)
        /// </summary>
        /// <returns>The next line of text.</returns>
        public abstract string ReadLine();

        /// <summary>
        /// Appends the given line of text to the file/datasource. Analogous to the COBOL WRITE statement.
        /// </summary>
        /// <param name="line">The string value to append.</param>
        public abstract FileStatus WriteLine(string line);

        /// <summary>
        /// Appends the given byte array to the file/datasource. Analogous to the COBOL WRITE statement.
        /// </summary>
        /// <param name="buffer">The bytes value to append.</param>
        public abstract FileStatus WriteLine(byte[] buffer);

        public abstract FileStatus WriteLinePrinter(string line, PrinterControl controlType, int beforeWriteCount, int afterWriteCount);
        #endregion

        #region protected properties
        /// <summary>
        /// Returns <c>true</c> if Initialize() has been called for this object.
        /// </summary>
        protected bool IsInitialized { get; set; }
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the file's DDName value.
        /// </summary>
        public string DDName { get; set; }

        /// <summary>
        /// Gets or sets the IFileHandler object which owns this link. 
        /// </summary>
        public IFileHandler FileHandler { get; set; }

        /// <summary>
        /// Gets or sets the status of the file/datasource.
        /// </summary>
        public FileStatus FileStatus { get; set; }

        /// <summary>
        /// Gets or sets the File Organization (Fixed, Variable, LineSequential, FBA)
        /// </summary>
        public FileOrganization FileOrganization { get; set; }

        /// <summary>
        /// Gets or sets the FileType (Flat, GDG, VSAM)
        /// </summary>
        public FileType FileType { get; set; }

        /// <summary>
        /// Gets or sets the FileDisposition (OLD,SHR,NEW,MOD)
        /// </summary>
        public FileDispositions FileDisposition { get; set; }

        /// <summary>
        /// Gets whether the link to the file/datasource is currently active.
        /// </summary>
        public bool IsLinked
        {
            get { return GetIsLinked(); }
        }

        /// <summary>
        /// Gets the Record object associated with the data of the linked file/datasource.
        /// </summary>
        //public IRecord Record
        //{
        //    get { return GetRecord(); }
        //}

        /// <summary>
        /// Gets or sets the size of file records (in bytes). 
        /// </summary>
        public int RecordLength
        {
            get { return GetRecordLength(); }
            set { SetRecordLength(value); }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Sets up the IFileLink-implementing object with its owning <paramref name="FileHandler"/>,
        /// its <paramref name="ddName"/> and any other implementation-specific named parameters.
        /// </summary>
        /// <param name="fileHandler">IFileHandler object which owns this file link.</param>
        /// <param name="ddName">DDName of the new file link.</param>
        /// <param name="properties">Any implementation-specific values to be passed to the IFileLink-implementing object.</param>
        public void Initialize(IFileHandler fileHandler, string ddName, params Tuple<string, object>[] properties)
        {
            if (!IsInitialized)
            {
                var props = new Dictionary<string, object>();
                foreach (Tuple<string, object> item in properties)
                {
                    props.Add(item.Item1, item.Item2);
                }

                Initialize(fileHandler, ddName, props);
            }
        }

        /// <summary>
        /// Sets up the IFileLink-implementing object with its owning <paramref name="FileHandler"/>,
        /// its <paramref name="ddName"/> and any other implementation-specific named parameters
        /// (<paramref name="propertyBag"/>).
        /// </summary>
        /// <param name="fileHandler">IFileHandler object which owns this file link.</param>
        /// <param name="ddName">DDName of the new file link.</param>
        /// <param name="propertyBag">Any implementation-specific values to be passed to the IFileLink-implementing object.</param>
        public void Initialize(IFileHandler fileHandler, string ddName, IDictionary<string, object> propertyBag)
        {
            if (!IsInitialized)
            {
                FileHandler = fileHandler;
                DDName = ddName;

                if (propertyBag != null)
                {
                    InternalInitialize(propertyBag);
                }

                IsInitialized = true;
            }
        }
        #endregion

    }
}
