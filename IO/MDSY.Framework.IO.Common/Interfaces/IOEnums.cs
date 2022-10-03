
namespace MDSY.Framework.IO.Common
{
    public enum FileAccessMode
    {
        Read = 64,
        Write = 128,
        ReadWrite = 264,
        WriteExtend = 1024,
    }
    
    public enum FileStatus
    {
        /// <summary>
        /// Successful completion.
        /// </summary>
        Successful_completion = 0,
        /// <summary>
        /// Indexed files only. Possible causes:
        ///  For a READ statement, the key value for the current key is equal to the value of that same key in the next record in the current key of reference.
        ///  For a WRITE or REWRITE statement, the record just written created a duplicate key value for at least one alternate record key for which duplicates are allowed.
        /// </summary>
        Indexed_files_only = 2,
        /// <summary>
        /// The length of the record being processed does not conform to the fixed file attributes for that file.
        /// </summary>
        Wrong_record_length = 4,
        /// <summary>
        /// The referenced optional file is not present at the time the OPEN statement is executed.
        /// </summary>
        Optional_file_not_present = 5,
        /// <summary>
        /// Attempted to write to a file that has been opened for input.
        /// </summary>
        Cannot_write_on_input_file = 6,
        /// <summary>
        /// Sequential files only. For an OPEN or CLOSE statement with the REEL/UNIT phrase the referenced file is a non-reel/unit medium.
        /// </summary>
        Sequential_files_only = 7,
        /// <summary>
        /// Attempted to read from a file opened for output.
        /// </summary>
        Cannot_read_from_output_file = 8,
        /// <summary>
        /// No room in directory or directory does not exist.
        /// </summary>
        Directory_doesnt_exist = 9,
        /// <summary>
        /// No next logical record exists. You have reached the end of the file.
        /// </summary>
        End_of_file = 10,
        /// <summary>
        /// Attempted to open a file that is already open.
        /// </summary>
        File_already_open = 12,
        /// <summary>
        /// File not found.
        /// </summary>
        File_not_found = 13,
        /// <summary>
        /// Relative files only. The number of significant digits in the relative record number is larger than the size of the relative key data item described for that file.
        /// </summary>
        Relative_files_only = 14,
        /// <summary>
        /// Too many indexed files opened.
        /// </summary>
        Too_many_indexed_files_opened = 15,
        /// <summary>
        /// Too many device files open .
        /// </summary>
        Too_many_device_files_opened = 16,
        /// <summary>
        /// 17 	Record error: probably zero length .
        /// </summary>
        Record_zero_length = 17,
        /// <summary>
        /// Read part record error: EOF before EOR or file open in wrong mode .
        /// </summary>
        Read_part_record_error = 18,
        /// <summary>
        /// Rewrite error: open mode or access mode wrong .
        /// </summary>
        Rewrite_error = 19,
        /// <summary>
        /// Device or resource busy .
        /// </summary>
        Device_busy = 20,
        /// <summary>
        /// Sequentially accessed files only. Indicates a sequence error. The ascending key requirements of 
        /// successive record key values has been violated, or, the prime record key value has been changed 
        /// by a COBOL program between successful execution of a READ statement and execution of the next 
        /// REWRITE statement for that file.
        /// </summary>
        Sequentially_accessed_files_only = 21,
        /// <summary>
        /// Indexed and relative files only. Indicates a duplicate key condition. Attempt has been made to 
        /// store a record that would create a duplicate key in the indexed or relative file OR a duplicate 
        /// alternate record key that does not allow duplicates.
        /// </summary>
        Indexed_and_relative_files_only = 22,
        /// <summary>
        /// Indicates no record found. An attempt has been made to access a record, identified by a key, 
        /// and that record does not exist in the file. Alternatively a START or READ operation has been 
        /// tried on an optional input file that is not present.
        /// </summary>
        Indicates_no_record_found = 23,
        /// <summary>
        /// Relative and indexed files only. Indicates a boundary violation. Possible causes:
        /// Attempting to write beyond the externally defined boundaries of a file
        /// Attempting a sequential WRITE operation has been tried on a relative file, but the number of 
        /// significant digits in the relative record number is larger than the size of the relative key 
        /// data item described for the file.
        /// </summary>
        Relative_and_indexed_files_only = 24,
        /// <summary>
        /// The I/O statement was unsuccessfully executed as the result of a boundary violation for a sequential
        /// file or as the result of an I/O error, such as a data check parity error, or a transmission error.
        /// </summary>
        Statement_was_unsuccessfully_executed = 30,
        /// <summary>
        /// Too many Indexed files opened. This can also happen when a sequential file is open for input and an 
        /// attempt is made to open the same file for output.
        /// </summary>
        Wrong_file_access_type = 32,
        /// <summary>
        /// The I/O statement failed because of a boundary violation. This condition indicates that an attempt 
        /// has been made to write beyond the externally defined boundaries of a sequential file.
        /// </summary>
        Boundary_violation = 34,
        /// <summary>
        /// An OPEN operation with the I-O, INPUT, or EXTEND phrases has been tried on a non-OPTIONAL file that 
        /// is not present. Trying to open a file that does not exist.
        ///	May need to map the COBOL file name to the physical file name. (Micro Focus, refer to the 
        ///	ASSIGN(EXTERNAL) directive)
        /// </summary>
        File_doesnt_exist = 35,
        /// <summary>
        /// An OPEN operation has been tried on a file which does not support the open mode specified in the OPEN statement.
        /// </summary>
        File_doesnt_support_open_mode = 37,
        /// <summary>
        /// An OPEN operation has been tried on a file previously closed with a lock.
        /// </summary>
        Cannot_open_locked_file = 38,
        /// <summary>
        /// A conflict has been detected between the actual file attributes and the attributes specified for the file in the program.
        ///	This is usually caused by a conflict with record-length, key-length, key-position or file organization.
        ///	Other possible causes are:
        ///   1. Alternate indexes are incorrectly defined (Key length or position, duplicates or sparse parameters).
        ///   2. The Recording Mode is Variable or Fixed or not defined the same as when the file was created..
        /// </summary>
        File_attributes_conflit = 39,
        /// <summary>
        /// An OPEN operation has been tried on file already opened.
        /// </summary>
        File_already_opened = 41,
        /// <summary>
        /// A CLOSE operation has been tried on file already closed.
        /// </summary>
        File_already_closed = 42,
        /// <summary>
        /// Files in sequential access mode. The last I/O statement executed for the file, before the 
        /// execution of a DELETE or REWRITE statement, was not a READ statement.
        /// </summary>
        Wrong_sequential_access_mode = 43,
        /// <summary>
        /// A boundary violation exists. Possible causes:
        ///		Attempting to WRITE or REWRITE a record that is larger than the largest, or smaller than
        ///		the smallest record allowed by the RECORD IS VARYING clause of the associated file
        ///		Attempting to REWRITE a record to a file and the record is not the same size as the record
        ///		being replaced.
        /// </summary>
        Boundary_violation_on_output_operation = 44,
        /// <summary>
        /// An attempt has been made to REWRITE a record to a file, and the record is not the same size 
        /// as the record being replaced.
        /// For line sequential files this refers to the physical size of the record, that is after space 
        /// removal, tab compression and null insertion. In this case, the physical size of the new record
        /// is allowed to be smaller than that of the record being replaced.
        /// </summary>
        Record_wrong_size = 45,
        /// <summary>
        /// A sequential READ operation has been tried on a file open in the INPUT or I-O mode but no valid 
        /// next record has been established.
        /// </summary>
        Invalid_record_read = 46,
        /// <summary>
        /// A READ or START operation has been tried on a file not opened INPUT or I-O.
        /// </summary>
        File_not_opened_as_input = 47,
        /// <summary>
        /// A WRITE operation has been tried on a file not opened in the OUTPUT, I-O, or EXTEND mode, or on a 
        /// file open I-O in the sequential access mode.
        /// </summary>
        File_not_opened_as_output = 48,
        /// <summary>
        /// 49 	A DELETE or REWRITE operation has been tried on a file that is not opened I-O.
        /// </summary>
        File_not_opened_as_input_output = 49,
        /// <summary>
        /// 90 	Extended file status.
        /// </summary>
        File_extended_file_status = 90
    }
    
    public enum FileOrganization
    {
        UnKnown = 0,
        Fixed = 1,
        Variable = 2,
        LineSequential = 3,
        FBA = 4,
        Indexed = 5,
        Sort = 6,
        LineSequentialCompressed = 7
    };

    public enum FileAccessType  //this really should be FileAccessMode but that name is already taken
    {
        Sequential = 0,
        Dynamic = 1,
        Random = 2,
    };
    
    public enum FileType
    {
        UNKNOWN = 0,
        FLAT = 1,
        GDG = 2,
        VSAM_SQLServer = 3,
        VSAM_Oracle = 4
    };
    
    public enum PrinterControl
    {
        LINEBREAK = 0,
        PAGEBREAK = 1

    };
    
    public enum SortDirection
    {
        Ascending = 0,
        Descending = 1

    };
}
