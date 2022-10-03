using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Interfaces;
using System.Collections.Generic;

namespace MDSY.Framework.IO.Common
{
    /// <summary>
    /// Defines an object which represents a link to a data file. 
    /// </summary>
    public interface IFileLink
    {
        //TODO Ifile link is probably too closely coupled to NeoBatch.  With some refactoring this should be a nice base class/interface but will require changing NeoBatch?
        #region Attributes
        
        byte[] BufferAsBytes { get; }

        string BufferAsString { get; }

        /// <summary>
        /// Gets or sets the file's DDName value.
        /// </summary>
        string DDName { get; set; }
       
        FileAccessMode FileAccess { get; set; }
                
        /// <summary>
        /// Gets or sets the IFileHandler object which owns this link. 
        /// </summary>
        IFileHandler FileHandler { get; set; }

        System.IO.FileMode FileMode{get;set;}
        
        /// <summary>
        /// Gets or sets the File Organization (Fixed, Variable, LineSequential, FBA)
        /// </summary>
        FileOrganization FileOrganization { get; set; }
        
        /// <summary>
        /// Gets or sets the status of the file/datasource.
        /// </summary>
        FileStatus FileStatus { get; set; }
        
        /// <summary>
        /// Gets or sets the FileType (Flat, GDG, VSAM)
        /// </summary>
        FileType FileType { get; set; }
        /// <summary>
        /// Gets or sets the type of the file access type.
        /// </summary>
        /// <value>
        /// The type of the file access.
        /// </value>
        FileAccessType FileAccessType { get; set; }

        bool IsFirstRead{get;set;}
        bool IsSequentialRead { get; set; }

        /// <summary>
        /// Used for COBOL Print File specification (ex. Print After Advancing)
        /// </summary>
        bool IsPrintFile { get; set; }

        /// <summary>
        /// Specifies an optional File
        /// </summary>
        bool IsOptional { get; set; }
          
        /// <summary>
        /// Gets the Record object associated with the data of the linked file/datasource.
        /// </summary>
        //DBSRecord Record { get; }

        /// <summary>
        /// Gets or sets the size of file records (in bytes). 
        /// </summary>
        int RecordLength { get ; set; }

        /// <summary>
        /// Specifies a status field used to report file status
        /// </summary>
        IBufferValue StatusField { get; set; }

        IBufferValue RecordKey { get; set; }

        string FilePath { get; set; }

        int ReadPointer { get; set; }

        IBufferValue AssociatedBuffer { get; set; }

        int OverrideReadCache { get; set; }

        int CacheInsertData { get; set; }

        bool IsCachedUpdate { get; set; }
        #endregion

        #region Operations
        
        /// <summary>
        /// Closes the "file" that was opened by a call to OpenFile().
        /// </summary>
        void CloseFile();
                
        /// <summary>
        /// Deletes the next line (or record, etc.) of the
        /// associated text file (or DB table, etc.)
        /// </summary>
        void Delete();
        
        void GetDataSetInfo(string ddName);
        
        /// <summary>
        /// Sets up the IFileLink-implementing object with its owning <paramref name="FileHandler"/>,
        /// its <paramref name="ddName"/> and any other implementation-specific named parameters
        /// (<paramref name="propertyBag"/>).
        /// </summary>
        /// <param name="fileHandler">IFileHandler object which owns this file link.</param>
        /// <param name="ddName">DDName of the new file link.</param>
        /// <param name="propertyBag">Any implementation-specific values to be passed to the IFileLink-implementing object.</param>
        void Initialize(IFileHandler fileHandler, string ddName, IDictionary<string, object> propertyBag);
        
        void OpenFile(FileAccessMode access);
        
        /// <summary>
        /// Returns the next line (or record, etc.) of the 
        /// associated text file (or DB table, etc.)
        /// </summary>
        /// <returns>The next line of text.</returns>
        string ReadLine();
        string ReadLine(params string[] options);

        /// <summary>
        /// Read The next line of data and returns byte array
        /// </summary>
        /// <returns></returns>
        byte[] ReadLineInto();
        byte[] ReadLineInto(params string[] options);

        /// <summary>
        /// Starts Read based on key value
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="startOption"></param>
        /// <returns></returns>
        FileStatus StartRead(IBufferValue keyValue, ReadOption startOption);

        /// <summary>
        /// Starts Read based on key value
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="startOption"></param>
        /// <returns></returns>
        FileStatus StartRead(string keyValue, ReadOption startOption);

        /// <summary>
        /// Reads record based on key value
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="startOption"></param>
        /// <returns></returns>
        FileStatus ReadByKey(IBufferValue keyValue);

        /// <summary>
        /// Reads record based on key value
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="startOption"></param>
        /// <returns></returns>
        FileStatus ReadByKey(string keyValue);
        
        /// <summary>
        /// Appends the given line of text to the file/datasource. Analogous to the COBOL WRITE statement.
        /// </summary>
        /// <param name="line">The string value to append.</param>
        FileStatus WriteLine(string line);

        /// <summary>
        /// Replaces last line read read with current string
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        FileStatus RewriteLine(string line);

        /// <summary> 
        /// Replaces last line read with current buffer
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        FileStatus RewriteLine(byte[] buffer);

        /// <summary>
        /// Appends the given byte array to the file/datasource. Analogous to the COBOL WRITE statement.
        /// </summary>
        /// <param name="buffer">The bytes value to append.</param>
        FileStatus WriteLine(byte[] buffer, bool flushFlag = false);

        /// <summary>
        /// Appends the given line of text to the file with printer control characters. Analogous to the COBOL WRITE AFTER/BEFORE
        /// </summary>
        /// <param name="line">The string value to append.</param>
        FileStatus WriteLinePrinter(string line, PrinterControl printerControl, int beforeWriteCount, int afterWriteCount);

        /// <summary>
        /// Flush the Write buffer when doing cached writes
        /// </summary>
        void FlushWriteBuffer();
        
        #endregion

      }
}