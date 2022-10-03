using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MDSY.Framework.IO.Common;
using MDSY.Framework.Interfaces;
using MDSY.Framework.Buffer.Interfaces;
using Microsoft.Extensions.Configuration;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.IO.RemoteBatch
{
    public class FileLink_RemoteBatch : IFileLink
    {

        #region private fields
        private byte formFeed = 0x0C;
        private byte lineFeed = 0x0A;
        private byte carriageReturn = 0x0D;
        private string _dispStatus = FileDispositions.NEW;
        private const string LOG_PREFIX = "MDSY_IO";
        private byte[] byteBuffer;
        private bool _varAsLseq;
        private PhysicalFile_RemoteBatch _physicalFile;
        private byte[] lineReturnBytes;
        private string lineReturnString;
        private int _readPointer;
        #endregion

        #region public constructors

        /// <summary>
        /// Creates a new FileLink and instantiates the embedded FileDetails object, populating any attributes that are available
        /// </summary>
        /// <param name="fileHandler">The containing file handler object</param>
        /// <param name="ddName">The DDName that will be used to refer to the new file link</param>
        /// <param name="physicalFileName">The full Windows file system path the the linked file</param>
        /// <param name="type">This may be an unknown valeu</param>
        public FileLink_RemoteBatch(Common.IFileHandler fileHandler, string ddName)
        {
            FileHandler = fileHandler;
            DDName = ddName;
            _physicalFile = new PhysicalFile_RemoteBatch();
            FilePaths = new List<string>();
            CacheInsertData = 0;
            IsCachedUpdate = false;
            _varAsLseq = ConfigSettings.GetAppSettingsString("VarFormat") == "LSEQ";

            if (ConfigSettings.GetAppSettingsBool("LinuxFile"))
                lineReturnBytes = new byte[1] { lineFeed };
            else
                lineReturnBytes = Encoding.ASCII.GetBytes(Environment.NewLine);

            lineReturnString = Convert.ToString(lineReturnBytes);
        }

        #endregion

        #region public properties

        public byte[] BufferAsBytes
        {
            get { return byteBuffer; }
        }

        public string BufferAsString
        {
            get
            {
                if (Settings.InputFileEncodingBodyName == string.Empty)
                    return Encoding.ASCII.GetString(byteBuffer, 0, byteBuffer.Length);
                else
                    return Encoding.GetEncoding(Settings.InputFileEncodingBodyName).GetString(byteBuffer, 0, byteBuffer.Length);
            }
        }

        //  public Buffer DBSBuffer { get; set; }

        public string DDName { get; set; }

        public IBufferValue DynamicFileName { get; set; }

        public FileAccessMode FileAccess { get; set; }

        public Common.IFileHandler FileHandler { get; set; }

        public System.IO.FileMode FileMode { get; set; }

        public FileOrganization FileOrganization { get; set; }

        public string FilePath
        {
            get { return _physicalFile.FilePath; }
            set { _physicalFile.FilePath = value; }
        }

        public int ReadPointer
        {
            get { return _readPointer; }
            set { _readPointer = value; }
        }


        public List<string> FilePaths;

        public FileStatus FileStatus { get; set; }

        public FileType FileType { get; set; }

        public FileAccessType FileAccessType { get; set; }

        public bool IsFirstRead { get; set; }

        /// <summary>
        /// returns boolean to indicate whether the Initialize method has been called on this object.
        /// </summary>
        public bool IsInitialized { get; set; }

        public IRecord Record { get; set; }

        public int RecordLength
        {
            get
            {
                return _physicalFile.RecordLength;
            }
            set
            {
                _physicalFile.RecordLength = value;
            }
        }

        public IBufferValue StatusField { get; set; }

        public string VsamSegmentName { get; set; }

        public string VsamKeys { get; set; }

        public IBufferValue RecordKey { get; set; }

        public string CatalogFileName { get; set; }

        public bool IsSequentialRead { get; set; }
        public bool IsPrintFile { get; set; }

        public IBufferValue AssociatedBuffer { get; set; }

        public int OverrideReadCache
        {
            get
            {
                //if (vsamDalObject != null)
                //    return vsamDalObject.ReadCache;
                //else
                return 0;
            }
            set
            {

            }
            //{
            //    if (vsamDalObject != null)
            //        vsamDalObject.ReadCache = value;
            //}

        }

        public int CacheInsertData { get; set; }

        public bool IsCachedUpdate { get; set; }

        public bool IsOptional { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Open file
        /// </summary>
        /// <param name="fam"></param>
        public void OpenFile(FileAccessMode fam)  //READ/WRITE/WRITEAPPEND
        {
            try
            {
                if (DynamicFileName != null)
                {
                    _physicalFile.FilePath = DynamicFileName.BytesAsString;
                    _physicalFile.IsDynamicFile = true;
                    if (FileOrganization == FileOrganization.UnKnown)
                    {
                        FileOrganization = FileOrganization.LineSequential;
                    }
                }
                FileHandler.SendDebugMessage(String.Format("** Opening file: {0} ==> {1} Record length: {2} Disp: {3} -  Access: {4}",
                                                  DDName, _physicalFile.FilePath, _physicalFile.RecordLength, _dispStatus, FileAccess));

                //Check for Variable length File override
                if (FileOrganization == Common.FileOrganization.Variable && _varAsLseq)
                {
                    FileOrganization = Common.FileOrganization.LineSequential;
                }

                //Check for VSAM DB file
                if (FileType == Common.FileType.VSAM_SQLServer)
                {
                    GetDalObject(VsamSegmentName);
                    return;
                }

                //if (_physicalFile.RecordLength == 0)
                //    FileHandler.SendWarning("Record length set to zero.");

                if (_physicalFile != null && this.FilePath != "Console")
                {
                    if (_physicalFile.IsOpen)
                        _physicalFile.Close();
                    System.Threading.Thread.Sleep(100);
                }
                int openStatus = 0;
                if (this.FilePath != "Console")
                //open the filestream
                {
                    if (fam == FileAccessMode.Read)
                        openStatus = _physicalFile.OpenForRead(FileOrganization, IsOptional);
                    else
                        openStatus = _physicalFile.OpenForWrite(fam, FileOrganization, IsOptional);
                }


                FileStatus = SetFileStatus(openStatus);
                FileAccess = fam;

            }
            catch (Exception ex)
            {
                FileStatus = SetFileStatus(30);
                throw new Exception(string.Format("Untrapped error on attempt to open file {0} for {1} {2}  ERROR===>{3} {2} {4}", _physicalFile.FilePath, FileAccess.ToString(), lineReturnString, ex.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// Read line from open file
        /// </summary>
        /// <returns></returns>
        public string ReadLine()
        {
            try
            {
                if (FileType == Common.FileType.VSAM_SQLServer)
                {
                    return ReadVsamSqlServerRow();
                }

                if (FileAccess != FileAccessMode.Read && FileAccess != FileAccessMode.ReadWrite && _physicalFile.IsOpen)
                {
                    CloseFile();
                    System.Threading.Thread.Sleep(100);
                }
                if (!_physicalFile.IsOpen)
                {
                    OpenFile(FileAccessMode.Read);
                    IsFirstRead = true;
                    FileAccess = FileAccessMode.Read;
                }
                byteBuffer = null;
                byteBuffer = _physicalFile.ReadNextRecord(FileOrganization);

                if (byteBuffer == null)
                {
                    if (this.FilePaths.Count > 0)
                    {
                        this.FilePath = FilePaths[0];
                        FilePaths.Remove(this.FilePath);
                        CloseFile();

                        return "OK";
                    }

                    FileStatus = SetFileStatus(10);

                    return string.Empty;
                }
                else
                {
                    if (AssociatedBuffer != null)
                    {
                        AssociatedBuffer.AssignFrom(byteBuffer);
                    }
                    FileStatus = SetFileStatus(0);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                FileStatus = SetFileStatus(30);
                throw new Exception(string.Format("FileLink_RemoteBatch.Readline  Unable to read next line from file  Error==>{0}", ex.Message));
            }
        }

        public string ReadLine(params string[] options)
        {
            string rtn = ReadLine();
            if (options != null && options.Contains("NEXT") && FileStatus == FileStatus.Indicates_no_record_found)
            {
                FileStatus = FileStatus.End_of_file;
                StatusField.Assign("10");
            }
            return rtn;
        }

        /// <summary>
        /// Read line into bytearray
        /// </summary>
        /// <returns></returns>
        public byte[] ReadLineInto()
        {
            if (ReadLine() != string.Empty)
            {
                return BufferAsBytes;
            }
            else
            {
                return new byte[] { 0x00 };
            }
        }

        public byte[] ReadLineInto(params string[] options)
        {
            if (ReadLine(options) != string.Empty)
            {
                return BufferAsBytes;
            }
            else
            {
                return new byte[] { 0x00 };
            }
        }

        /// <summary>
        /// Read file based on Index key - USed for Converted VSAM files
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public FileStatus ReadByKey(IBufferValue keyValue)
        {
            //int returnCode = vsamDalObject.ReadByKey(new VsamKey(keyValue));
            //if (returnCode == 0)
            //{
            //    FileStatus = FileStatus.Successful_completion;
            //    byteBuffer = vsamDalObject.AsBytes;
            //    //if (AssociatedBuffer != null)
            //    //{
            //    //    AssociatedBuffer.AssignFrom(byteBuffer);
            //    //}
            //}
            //else
            //{
            //    FileStatus = FileStatus.Indicates_no_record_found;
            //    byteBuffer = null;
            //}

            //if (StatusField != null)
            //{
            //    StatusField.Assign(returnCode);
            //}

            return FileStatus;
        }

        /// <summary>
        /// Read file based on Index key - Used for Converted VSAM files
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public FileStatus ReadByKey(string keyValue)
        {
            //VsamKey tempVsamKey = new VsamKey();
            //tempVsamKey.StringKey = keyValue;

            //int returnCode = vsamDalObject.ReadByKey(tempVsamKey);
            //if (returnCode == 0)
            //{
            //    FileStatus = FileStatus.Successful_completion;
            //    byteBuffer = vsamDalObject.AsBytes;
            //    if (AssociatedBuffer != null)
            //    {
            //        AssociatedBuffer.AssignFrom(byteBuffer);
            //    }
            //}
            //else
            //{
            //    FileStatus = FileStatus.Indicates_no_record_found;
            //    byteBuffer = null;
            //}

            //if (StatusField != null)
            //{
            //    StatusField.Assign(returnCode);
            //}

            return FileStatus;
        }

        /// <summary>
        /// Start read of indexed file - Used for Converted VSAM files
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="startOption"></param>
        /// <returns></returns>
        public FileStatus StartRead(IBufferValue keyValue, ReadOption startOption)
        {
            //    int returnCode = vsamDalObject.StartRead(new VsamKey(keyValue));
            //    if (returnCode == 0)
            //    {
            //        FileStatus = FileStatus.Successful_completion;
            //        byteBuffer = vsamDalObject.VsamDalRecord.Buffer.ReadBytes();
            //        if (AssociatedBuffer != null)
            //        {
            //            AssociatedBuffer.AssignFrom(byteBuffer);
            //        }
            //    }
            //    else
            //    {
            //        FileStatus = FileStatus.End_of_file;
            //        byteBuffer = null;
            //    }

            //    if (StatusField != null)
            //    {
            //        StatusField.Assign(returnCode);
            //    }

            return FileStatus;
        }

        /// <summary>
        /// Start read of indexed file - Used for Converted VSAM files
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="startOption"></param>
        /// <returns></returns>
        public FileStatus StartRead(string keyValue, ReadOption startOption)
        {
            //VsamKey tempVsamKey = new VsamKey();
            //tempVsamKey.StringKey = keyValue;
            //int returnCode = vsamDalObject.StartRead(tempVsamKey);
            //if (returnCode == 0)
            //{
            //    FileStatus = FileStatus.Successful_completion;
            //    byteBuffer = vsamDalObject.VsamDalRecord.Buffer.ReadBytes();
            //    if (AssociatedBuffer != null)
            //    {
            //        AssociatedBuffer.AssignFrom(byteBuffer);
            //    }
            //}
            //else
            //{
            //    FileStatus = FileStatus.End_of_file;
            //    byteBuffer = null;
            //}

            //if (StatusField != null)
            //{
            //    StatusField.Assign(returnCode);
            //}

            return FileStatus;
        }

        /// <summary>
        /// Rewrite a line to file
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public FileStatus RewriteLine(string line)
        {
            if (FileType == Common.FileType.VSAM_SQLServer)
            {
                byteBuffer = line.ToByteList().ToArray();
                return RewriteVsamSqlServerRow();
            }
            return RewriteLine(line.ToByteList().ToArray());
        }
        public FileStatus RewriteLine(byte[] value)
        {
            try
            {
                if (FileAccess != FileAccessMode.ReadWrite)
                {
                    throw new Exception("File AccessMode ReadWrite required when trying to Rewrite in a file!");
                }
                this._physicalFile.ReWriteRecord(value);
                this.FileStatus = SetFileStatus(0);
                return this.FileStatus;
            }
            catch (Exception exc)
            {
                this.FileHandler.SendDebugMessage(String.Format("File: {0} error while rewriting.", this._physicalFile.FilePath));
                this.FileHandler.SendError(exc.Message);
                this.FileStatus = SetFileStatus(30);
                throw new Exception(string.Format("Untrapped error on attempt to rewrite file {0} for {1} {2}  ERROR===>{3} {2} {4}", _physicalFile.FilePath, FileAccess.ToString(), value.ToString(), exc.Message, exc.StackTrace));
            }
        }
        /// <summary>
        /// Write a line to file from string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FileStatus WriteLine(string value)
        {
            return WriteLine(value.ToByteList());
        }

        /// <summary>
        /// Write a line to file from byte list
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FileStatus WriteLine(List<byte> value)
        {
            if (RecordLength <= 0 && FileOrganization == FileOrganization.Fixed)
            {
                if (value.Count > 0)
                    RecordLength = value.Count;
                else
                    return Common.FileStatus.Record_zero_length;
            }

            List<byte> formattedValue = value;

            //Pad to Record size
            if (RecordLength >= 0 && FileOrganization != FileOrganization.LineSequentialCompressed && FileOrganization != FileOrganization.Variable)
            {
                if (formattedValue.EndsWith(Environment.NewLine))
                {
                    formattedValue = formattedValue.Take(formattedValue.Count - 2).ToList<byte>();
                    formattedValue = formattedValue.PadRight(RecordLength, (byte)' ');

                }
                else
                {
                    formattedValue = formattedValue.PadRight(RecordLength, (byte)' ');
                }
            }

            if (FileOrganization == FileOrganization.Variable)
            {
                short recLength = (short)formattedValue.Count;
                byte[] shortBytes = BitConverter.GetBytes(recLength);
                
                formattedValue.InsertRange(0,shortBytes);

            }
            else if (FileOrganization == FileOrganization.Fixed || FileOrganization == FileOrganization.FBA)
            {
                if (RecordLength > formattedValue.Count)
                {
                    formattedValue = formattedValue.PadRight(RecordLength, (byte)' ');
                }
                else if ((RecordLength != -1) && (RecordLength < formattedValue.Count))
                {
                    /// keep going until all is printed
                    while (formattedValue.Count > RecordLength && FileOrganization != FileOrganization.LineSequential)
                    {
                        this.WriteLine(formattedValue.Take<byte>(RecordLength).ToList<byte>());
                        formattedValue = formattedValue.Skip(RecordLength).ToList<byte>();
                    }
                }
                if (RecordLength > formattedValue.Count)
                {
                    formattedValue = formattedValue.PadRight(RecordLength, (byte)' ');
                }
            }

            return this.WriteLine(formattedValue.ToArray());
        }

        /// <summary>
        /// Write a line to file from byte array
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public FileStatus WriteLine(byte[] buffer, bool flushFlag = false)
        {
            try
            {
                if (FileType == Common.FileType.VSAM_SQLServer)
                {
                    byteBuffer = buffer;
                    return WriteVsamSqlServerRow();
                }
                if (this.FilePath == "Console")
                {
                    System.Text.Encoding encEncoder = System.Text.ASCIIEncoding.ASCII;
                    Console.WriteLine(encEncoder.GetString(buffer));
                    return SetFileStatus(0);
                }

                if (FileAccess == FileAccessMode.Read)
                {
                    if (_physicalFile.IsOpen)
                        CloseFile();
                    FileAccess = FileAccessMode.Write;
                }
                if (!_physicalFile.IsOpen)
                {
                    OpenFile(FileAccess);
                }

                if (buffer != null && buffer.Length > 0)
                {
                    if (IsPrintFile)
                    {
                        var message = new[] { lineFeed }.Concat(TrimByteArray(buffer).Concat(lineReturnBytes));
                        this._physicalFile.WriteRecord(message.ToArray());
                    }
                    else
                    {
                        this._physicalFile.WriteRecord(TrimByteArray(buffer));
                        if (FileOrganization == FileOrganization.LineSequential || FileOrganization == FileOrganization.LineSequentialCompressed)
                            _physicalFile.WriteRecord(lineReturnBytes);
                    }

                }

                string outString = "";

                if (this.FileHandler.IsDebugging)
                {
                    System.Text.Encoding encEncoder = System.Text.ASCIIEncoding.ASCII;
                    outString = encEncoder.GetString(buffer);

                    this.FileHandler.SendDebugMessage(String.Format("File: {0} wrote buffer: [{1}]", this._physicalFile.FilePath, outString));
                }
                this.FileStatus = SetFileStatus(0);
                return this.FileStatus;
            }
            catch (Exception exc)
            {
                this.FileHandler.SendDebugMessage(String.Format("File: {0} error while writing.", this._physicalFile.FilePath));
                //TODO handle errors for file status
                this.FileHandler.SendError(exc.Message);
                this.FileStatus = SetFileStatus(30);
                throw new Exception(string.Format("Untrapped error on attempt to write file {0} for {1} {2}  ERROR===>{3} {2} {4}", _physicalFile.FilePath, FileAccess.ToString(), lineReturnString, exc.Message, exc.StackTrace));
            }
        }

        /// <summary>
        /// Write Printer control lines - (ex. after advancing)
        /// </summary>
        /// <param name="line"></param>
        /// <param name="controlType"></param>
        /// <param name="beforeWriteCount"></param>
        /// <param name="afterWriteCount"></param>
        /// <returns></returns>
        public FileStatus WriteLinePrinter(string line, PrinterControl controlType, int beforeWriteCount, int afterWriteCount)
        {
            try
            {
                if (FileAccess == FileAccessMode.Read)
                {
                    if (_physicalFile.IsOpen)
                        CloseFile();
                    FileAccess = FileAccessMode.Write;
                }
                if (!_physicalFile.IsOpen)
                {
                    OpenFile(FileAccess);

                }
                byte controlCharacter;
                if (!IsPrintFile)
                    IsPrintFile = true;

                line = line.PadRight(this.RecordLength);
                StringBuilder beforeString = new StringBuilder();
                StringBuilder afterString = new StringBuilder();

                switch (controlType)
                {
                    case PrinterControl.LINEBREAK:
                        controlCharacter = lineFeed;
                        if (_physicalFile.IsStartOfFile)
                            afterWriteCount--;
                        break;
                    case PrinterControl.PAGEBREAK:
                        controlCharacter = formFeed;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(controlType.ToString(), controlType, null);
                }

                for (int i = 0; i < afterWriteCount; i++)
                {
                    afterString.Append(Encoding.ASCII.GetString(new[] { controlCharacter }));
                }
                for (int i = 0; i < beforeWriteCount; i++)
                {
                    beforeString.Append(Encoding.ASCII.GetString(new[] { controlCharacter }));
                }

                if (afterString.Length > 0)
                {
                    this._physicalFile.WriteRecord(afterString.ToString().ToByteList().ToArray());
                }

                this._physicalFile.WriteRecord(TrimByteArray(line.ToByteList().ToArray()).Concat(new[] { carriageReturn }).ToArray());

                if (beforeString.Length > 0)
                {
                    this._physicalFile.WriteRecord(beforeString.ToString().ToByteList().ToArray());
                }

                this.FileStatus = SetFileStatus(0);
                return this.FileStatus;
            }
            catch (Exception exc)
            {
                this.FileHandler.SendDebugMessage(String.Format("File: {0} error while writing.", this._physicalFile.FilePath));
                this.FileHandler.SendError(exc.Message);
                this.FileStatus = SetFileStatus(30);
                throw new Exception(string.Format("Untrapped error on attempt to write file {0} for {1} {2}  ERROR===>{3} {2} {4}", _physicalFile.FilePath, FileAccess.ToString(), lineReturnString, exc.Message, exc.StackTrace));
            }

        }

        public override string ToString()
        {
            return BufferAsString;
        }

        /// <summary>
        /// Flush the Write buffer 
        /// </summary>
        public void FlushWriteBuffer()
        {
            //vsamDalObject.FinishBulkCopyWrite();
            //vsamDalObject.SendCachedUpdatesToDatabase();
        }

        /// <summary>
        /// Close File
        /// </summary>
        public void CloseFile()
        {
            if (FileType == Common.FileType.VSAM_SQLServer)
            {
                FlushWriteBuffer();
                //vsamDalObject.CloseConnection();
            }
            else
            {
                if (FileAccess != FileAccessMode.Read && IsPrintFile)
                    _physicalFile.WriteRecord(new[] { carriageReturn, lineFeed });

                _physicalFile.Close();
            }
            SetFileStatus(0);
        }

        /// <summary>
        /// Delete a line from Indexed file
        /// </summary>
        public void Delete()
        {
            if (FileType == Common.FileType.VSAM_SQLServer)
            {
                //int returnCode = vsamDalObject.Delete();
                //if (returnCode == 0)
                //{
                //    FileStatus = SetFileStatus(0);
                //}
                //else
                //{
                //    FileStatus = SetFileStatus(30);
                //}
            }

        }

        public void GetDataSetInfo(string Nothing)
        {
            //required by the interface but not used here.
        }

        /// <summary>
        /// Returns the file status message.
        /// </summary>
        public string GetFileStatusFormatted()   //unreferenced?
        {
            return String.Format("{0:00}", Int32.Parse(this.FileStatus.ToString("D")));
        }
        #endregion

        #region Private Methods



        /// <summary>
        /// Determines if a given file is locked for use.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool isLocked(string fileName)   //unreferenced?
        {
            if (!File.Exists(fileName)) return false;
            try { FileStream f = File.Open(fileName, FileMode.Open, System.IO.FileAccess.ReadWrite); f.Close(); }
            catch { return true; }
            return false;
        }

        private byte[] TrimByteArray(byte[] bytearray)
        {
            if (this.RecordLength == 0)
            {
                return bytearray;
            }

            bool allNonSpace = true;
            int newByteSize = 0;
            for (int x = bytearray.Length - 1; x >= 0; x--)
            {
                if (bytearray[x] != 0x00 && bytearray[x] != 0x20 && x < this.RecordLength)
                {
                    allNonSpace = false;
                    newByteSize = x;
                    break;
                }
            }

            if (allNonSpace)
            {
                return new byte[0];
            }


            return bytearray.Take(newByteSize + 1).ToArray();

        }

        private FileStatus SetFileStatus(int statusCode)
        {
            if (StatusField != null)
            {
                StatusField.Assign(statusCode.ToString().PadLeft(2, '0'));
            }
            return (FileStatus)statusCode;
        }

        #region Catalog VSAM methods
        private void GetDalObject(string segmentName)
        {
            //Type dalType;
            //if (segmentName.Trim() != string.Empty)
            //{
            //    dalType = ProgramUtilities.GetDALType(string.Concat("DAL_", segmentName.Trim()));

            //    if (dalType == null)
            //        throw new Exception("No data access layer program exists for " + segmentName.Trim());

            //    vsamDalObject = (VsamDalBase)Activator.CreateInstance(dalType);

            //}
            //else
            //{
            //    DynamicVsamFile tempDalBase = new DynamicVsamFile(RecordLength);
            //    vsamDalObject = tempDalBase;
            //}

            //vsamDalObject.TableName = FilePath.Substring(FilePath.LastIndexOf(".") + 1);

            //vsamDalObject.OpenConnection();
            //FileStatus = FileStatus.Successful_completion;
            //if (StatusField != null)
            //{
            //    StatusField.Assign(0);
            //}


        }

        private string ReadVsamSqlServerRow()
        {
            string returnString = string.Empty;
            //vsamDalObject.IsSequentialRead = this.IsSequentialRead;
            //int returnCode = vsamDalObject.ReadNext(new VsamKey());
            //if (returnCode == 0)
            //{
            //    FileStatus = FileStatus.Successful_completion;
            //    byteBuffer = vsamDalObject.AsBytes;
            //    if (AssociatedBuffer != null)
            //    {
            //        AssociatedBuffer.AssignFrom(byteBuffer);
            //    }
            //    returnString = "OK";
            //}
            //else
            //{
            //    FileStatus = FileStatus.End_of_file;
            //    byteBuffer = null;
            //    returnString = string.Empty;
            //}
            //if (StatusField != null)
            //{
            //    StatusField.Assign(returnCode);
            //}

            return returnString;
        }

        private FileStatus RewriteVsamSqlServerRow()
        {
            //vsamDalObject.SetBytes(byteBuffer);
            //int returnCode = 0;

            //returnCode = vsamDalObject.ReWrite();

            //if (returnCode == 0)
            //{
            //    FileStatus = FileStatus.Successful_completion;
            //}
            //else
            //{
            //    FileStatus = FileStatus.Rewrite_error;
            //}
            //if (StatusField != null)
            //{
            //    StatusField.Assign(returnCode);
            //}
            return FileStatus;
        }

        private FileStatus WriteVsamSqlServerRow()
        {
            //vsamDalObject.SetBytes(byteBuffer);
            //int returnCode = 0;
            //if (CacheInsertData > 1)
            //{
            //    returnCode = vsamDalObject.WriteUsingBulkCopy(CacheInsertData);
            //}
            //else
            //{
            //    returnCode = vsamDalObject.Write();
            //}
            //if (returnCode == 0)
            //{
            //    FileStatus = FileStatus.Successful_completion;
            //}
            //else
            //{
            //    FileStatus = FileStatus.Statement_was_unsuccessfully_executed;
            //}
            //if (StatusField != null)
            //{
            //    StatusField.Assign(returnCode);
            //}

            return FileStatus;
        }

        #endregion

        #endregion



        #region Initialize routines

        /// <summary>
        /// Sets up the IFileLink-implementing object with its owning <paramref name="FileHandler"/>,
        /// its <paramref name="ddName"/> and any other implementation-specific named parameters.
        /// </summary>
        /// <param name="fileHandler">IFileHandler object which owns this file link.</param>
        /// <param name="ddName">DDName of the new file link.</param>
        /// <param name="properties">Any implementation-specific values to be passed to the IFileLink-implementing object.</param>
        public void Initialize(Common.IFileHandler fileHandler, string ddName, params Tuple<string, object>[] properties)
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

        public void Initialize(Common.IFileHandler fileHandler, string ddName, IDictionary<string, object> propertyBag)
        {
            if (!IsInitialized)
            {
                FileHandler = (Common.IFileHandler)fileHandler;
                DDName = ddName;

                if (propertyBag != null)
                {
                    InternalInitialize(propertyBag);
                }

                IsInitialized = true;
            }
        }

        protected void InternalInitialize(IDictionary<string, object> propertyBag)
        { }

        #endregion


    }
}
