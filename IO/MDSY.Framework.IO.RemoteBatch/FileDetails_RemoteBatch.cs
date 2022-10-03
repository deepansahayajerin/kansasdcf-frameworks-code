using System;
using System.IO;
using MDSY.Framework.IO.Common;
using System.Text;
namespace MDSY.Framework.IO.RemoteBatch
{
    public class PhysicalFile_RemoteBatch
    {
        private bool _isOpened = false;
        private FileStream fs = null;
        private StreamReader sr = null;
        private long fsLastOffset = 0;
        private long srLastOffset = 0;
        private long srCurrentOffset = 0;

        public bool EOF { get; set; }
        public string FilePath { get; set; }
        public int RecordLength { get; set; }
        public bool IsDynamicFile { get; set; }

        public FileOrganization FileOrganization { get; set; }
        public bool IsStartOfFile 
        {
            get { return fs.Position == 0; }
        }
        public bool IsOpen { get { return _isOpened; } }

        public bool Close()
        {
            if (fs != null && (fs.CanWrite || fs.CanRead))
            {
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
            if (sr != null)
            {
                sr.Close();
                sr.Dispose();
            }

            _isOpened = false;
            return true;
        }

        public int OpenForRead(FileOrganization fo, bool isOptional)
        {
            //files opened for read are defined to be pre-existing so only open parameters can be Open/Read
            try
            {
                if (!File.Exists(FilePath))
                {
                    if (isOptional)
                    {
                        return 5;
                    }
                    else
                    {
                        return 35;
                    }
                }

                if (fo == FileOrganization.LineSequential || fo == FileOrganization.LineSequentialCompressed)
                {
                    if (Settings.InputFileCodePage == string.Empty)
                        sr = new StreamReader(File.OpenRead(FilePath));
                    else
                    {
                        //https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding?view=netcore-3.1

                        string settingsCodePageString = Settings.InputFileCodePage.ToUpper();
                        int settingsCodePageInt = 20127;

                        if (settingsCodePageString.Equals("ASCII"))
                            settingsCodePageInt = 20127;
                        else if (settingsCodePageString.Equals("UNICODE"))
                            settingsCodePageInt = 1200;
                        else if (settingsCodePageString.Equals("UNICODE-BE"))
                            settingsCodePageInt = 1201;
                        else if (settingsCodePageString.Equals("UTF32"))
                            settingsCodePageInt = 12000;
                        else if (settingsCodePageString.Equals("UTF32-BE"))
                            settingsCodePageInt = 12001;
                        else if (settingsCodePageString.Equals("UTF7"))
                            settingsCodePageInt = 65000;
                        else if (settingsCodePageString.Equals("UTF8"))
                            settingsCodePageInt = 65001;
                        else if (settingsCodePageString.Equals("ISO"))
                            settingsCodePageInt = 28591;


                        sr = new StreamReader(File.OpenRead(FilePath), Encoding.GetEncoding(settingsCodePageInt), true);
                    }
                }
                else
                {
                    fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                }
                _isOpened = true;
                FileOrganization = fo;
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("PhysicalFile.OpenForRead unable to open file {0} for {1} {2}  ERROR===>{3} {2} {4}", FilePath, fo, Environment.NewLine, ex.Message, ex.StackTrace));
            }

        }

        public int OpenForWrite(FileAccessMode accessMode, FileOrganization fo, bool isOptional)
        {
            //files opened for write are defined to be pre-existing so only open parameters can be Open or Truncate with Write access
            try
            {
                int returnCode = 0;
                if (File.Exists(FilePath))
                {
                    if (accessMode == FileAccessMode.Write)
                    {
                        fs = new FileStream(FilePath, FileMode.Truncate, FileAccess.Write);
                    }
                    else if (accessMode == FileAccessMode.WriteExtend)
                    {
                        fs = new FileStream(FilePath, FileMode.Append, FileAccess.Write);
                    }
                    else
                    //Open for Read Write
                    {
                        fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                        if (fo == FileOrganization.LineSequential || fo == FileOrganization.LineSequentialCompressed)
                        {
                            sr = new StreamReader(fs);
                        }
                    }
                }
                else
                {
                    if  ((accessMode == FileAccessMode.ReadWrite) || (accessMode == FileAccessMode.WriteExtend))
                    {
                        if (isOptional)
                        {
                            returnCode = 5;
                        }
                        else
                        {
                            return 35;
                        }
                    }

                    fs = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write);
                }
                _isOpened = true;
                FileOrganization = fo;
                return returnCode;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("PhysicalFile.OpenForWrite unable to open file {0} as {1} for {2}. Truncate = {4}. Error ==> {3}", FilePath, FileMode.Create, FileAccess.Write, ex.Message, accessMode.ToString()));
            }
        }

        public byte[] ReadNextRecord(FileOrganization fo)
        {
            byte[] bytes;

            try
            {
                switch (fo)
                {
                    case (FileOrganization.FBA):                //for carriage control read 1 byte to strip off leading carriage control then read n bytes 
                        ReadBytes(1);   //Discard leading carriage control
                        bytes = ReadBytes(RecordLength);
                        break;

                    case (FileOrganization.Fixed):              //for fixed length just read num bytes                

                        bytes = ReadBytes(RecordLength);
                        break;
                    case (FileOrganization.LineSequentialCompressed):
                    case (FileOrganization.LineSequential):     //for line sequential ReadLine method on stream reader object returns line up to CR/LF
                        srLastOffset = srCurrentOffset;
                        string s = sr.ReadLine();
                        if (s == null)
                            bytes = null;
                        else
                        {
                            srCurrentOffset = srCurrentOffset + s.Length + 2;
                            if (s.Length < RecordLength)
                            {
                                s = s.PadRight(RecordLength);
                            }
                            if (Settings.InputFileEncodingBodyName == string.Empty)
                                bytes = Encoding.ASCII.GetBytes(s);
                            else
                                bytes = Encoding.GetEncoding(Settings.InputFileEncodingBodyName).GetBytes(s);
                        }
                        break;

                    case (FileOrganization.Variable):           //for variable read the next 2 bytes to get length, then read length bytes

                        bytes = ReadBytes(2);
                        if (bytes != null)
                        {
                            string var = System.Text.ASCIIEncoding.ASCII.GetString(bytes);
                            bytes = ReadBytes(Int16.Parse(var));
                        }
                        break;

                    default:
                        throw new Exception(string.Format("Unknown record format {0} passed to PhysicalFile.ReadNextRecord", fo));
                }
                return bytes;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("In PhysicalFile_RemoteBatch.ReadNextRecord encountered unknown error reading file {0} with file organization {1}  Error==>{2}", FilePath, fo, ex.Message));
            }
        }

        public bool WriteRecord(byte[] data)
        {
            //TODO Smarter with error trapping
            if (data != null)
            {
                fs.Write(data, 0, data.Length);
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool ReWriteRecord(byte[] data)
        {
            if (data != null)
            {
                if (this.FileOrganization == FileOrganization.LineSequential || this.FileOrganization == FileOrganization.LineSequentialCompressed)
                {
                    fs.Seek(srLastOffset, SeekOrigin.Begin);
                }
                else
                    fs.Seek(fsLastOffset, SeekOrigin.Begin);
                return WriteRecord(data);
            }
            else
            {
                return false;
            }

        }
        public byte[] ReadBytes(int HowMany)
        {
            //read HowMany bytes from the local FileStream object
            byte[] bytes = new byte[HowMany];
            int numBytesToRead = HowMany;
            int numBytesRead = 0;
            fsLastOffset = fs.Position;
            try
            {
                while (numBytesToRead > 0)
                {
                    int n = fs.Read(bytes, numBytesRead, numBytesToRead);
                    if (n == 0)
                        break;
                    numBytesRead += n;
                    numBytesToRead -= n;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Physical.ReadBytes unable to read {0} bytes from file {1}!   Error ==> {2}", HowMany, FilePath, ex.Message));
            }

            if (numBytesRead != bytes.Length)
            {
                EOF = true;
                bytes = null;
            }

            return bytes;

        }

        private class ByteBuffer
        {
            static int _bufferSize = 10000000;   //10 meg
            byte[] _buffer = new byte[_bufferSize];


            public ByteBuffer(FileStream fs)
            {
                int count = fs.Read(_buffer, 0, _bufferSize);


            }

        }

    }
}