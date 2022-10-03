using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Core;
using System.IO;
using System.Diagnostics;
using MDSY.Framework.Interfaces;
using MDSY.Framework.Buffer.Interfaces;

namespace MDSY.Framework.IO.Common
{
    #region Internal Sort used for COBOL sorting
    public class InternalSort
    {
        #region private constants...
        private const int INT_badFileResult = 12;
        private const int INT_goodFileResult = 0;
        private const string STR_NSortexe = "NSort.exe";
        private const string STR_TextFileExt = ".txt";
        #endregion

        #region private Fields
        private string _sortInFileName;
        private string _sortOutFileName;
        private string _sortControlData;
        private int _sortRecordLength;
        private long _inputFileLength;
        private List<SortParameter> _sortParmList = new List<SortParameter>();
        //private bool _atStartOfData = false;
        //private bool _atEndOfData = false;

        //private FileDefinition _inputFile;
        //private FileDefinition _outputFile;
        private IFileLink _outputFile;
        private IFileLink _inputFile;
        // private char _doubleQuote = '"';
        #endregion

        #region public properties

        /// <summary>
        /// Returns the number of Sort In Records
        /// </summary>
        public int SortCount { get; set; }

        /// <summary>
        /// Returns the length of the Sort Record
        /// </summary>
        public int SortRecordLength
        {
            get { return _sortRecordLength; }
        }

        #endregion


        #region Constructors

        public InternalSort()
        {
            String tempFilePath = Path.GetTempPath();
            _sortInFileName = string.Concat(tempFilePath, "SI", DateTime.Now.ToFileTimeUtc().ToString(), STR_TextFileExt);
            _sortOutFileName = string.Concat(tempFilePath, "SO", DateTime.Now.ToFileTimeUtc().ToString(), STR_TextFileExt);

            Console.WriteLine("Temp SORTIN  file = '" + _sortInFileName + "'");
            Console.WriteLine("Temp SORTOUT file = '" + _sortOutFileName + "'");
        }

        //public InternalSort(FileDefinition inputFile, FileDefinition outputFile)
        public InternalSort(IFileLink inputFile, IFileLink outputFile)
        {
            String tempFilePath = Path.GetTempPath();
            _sortInFileName = string.Concat(tempFilePath, "SI", DateTime.Now.ToFileTimeUtc().ToString(), STR_TextFileExt);
            _sortOutFileName = string.Concat(tempFilePath, "SO", DateTime.Now.ToFileTimeUtc().ToString(), STR_TextFileExt);

            Console.WriteLine("Temp SORTIN  file = '" + _sortInFileName + "'");
            Console.WriteLine("Temp SORTOUT file = '" + _sortOutFileName + "'");

            SetInputFile(inputFile);
            SetOutputFile(outputFile);
        }


        #endregion
        #region Public Methods
        /// <summary>
        /// Set the full input file path name for already defined input files
        /// </summary>
        /// <param name="inputFileName"></param>
        //public void SetInputFile(FileDefinition inputFile)
        public void SetInputFile(IFileLink inputFile)
        {

            _inputFile = inputFile;

            if (string.IsNullOrEmpty(_inputFile.FilePath))
            {
                _inputFile.FilePath = _sortInFileName;
            }
            if (_inputFile.RecordLength == 0 && _inputFile.AssociatedBuffer != null)
            {
                _inputFile.RecordLength = _inputFile.AssociatedBuffer.Buffer.Length;
            }
            if (_inputFile.FileType == FileType.UNKNOWN)
            {
                _inputFile.FileType = FileType.FLAT;
            }
            if (_inputFile.FileOrganization == FileOrganization.UnKnown)
            {
                _inputFile.FileOrganization = FileOrganization.Fixed;
            }

        }

        /// <summary>
        /// Set the full output file path name
        /// </summary>
        /// <param name="inputFileName"></param>
        //public void SetOutputFile(FileDefinition outputFile)
        public void SetOutputFile(IFileLink outputFile)
        {
            _outputFile = outputFile;

        }

        /// <summary>
        /// Sort the SortIn data based on sort parameters
        /// </summary>
        /// <param name="sortParms"> Sort Parameters </param>
        public int SortData()
        {
            return SubmitSort();
        }

        /// <summary>
        /// Sort the SortIn data based on sort parm text
        /// </summary>
        /// <param name="sortParms"> Sort Parameters </param>
        public int SortData(string SortParms)
        {
            _sortParmList.Clear();
            BuildSortParms(SortParms);
            return SubmitSort();
        }

        /// <summary>
        /// Sort the SortIn data based on sort parameters
        /// </summary>
        /// <param name="sortParms"> Sort Parameters </param>
        public int SortData(params SortParameter[] sortParms)
        {
            _sortParmList.Clear();
            Array.ForEach(sortParms, _sortParmList.Add);
            return SortData();
        }

        /// <summary>
        /// Sort the SortIn data based on sort parameters
        /// </summary>
        /// <param name="sortObjParams"></param>
        public int SortData(params Object[] sortObjParams)
        {
            _sortParmList.Clear();
            // int sortOffSet = 0;
            foreach (object sOBJ in sortObjParams)
            {
                if (sOBJ is IField)
                {
                    IField sortField = (IField)sOBJ;
                    SortParameter newSortParm = new SortParameter(sortField, SortDirection.Ascending); //{ ParmOffset = sortField.GroupBufferOffset };
                    _sortParmList.Add(newSortParm);
                }
                else if (sOBJ is SortDirection)
                {
                    _sortParmList[_sortParmList.Count - 1].SortDirection = (SortDirection)sOBJ;
                }
                else if (sOBJ is IGroup) 
                {
                  foreach (IBufferElement f in ((IGroup)sOBJ).Elements) { //.ChildCollection.Values) {
                    if (f is IField) {
                      IField sortField = (IField)f;
                      SortParameter newSortParm = new SortParameter(sortField, SortDirection.Ascending); //{ ParmOffset = sortField.GroupBufferOffset };
                      _sortParmList.Add(newSortParm);
                    }
                  }
                }
                else
                {
                    string strParm = (string)sOBJ;
                    if (String.Compare(strParm, "DESC") == 0 || String.Compare(strParm, "DESCENDING") == 0)
                        _sortParmList[_sortParmList.Count - 1].SortDirection = SortDirection.Descending;
                }
            }
            //Array.ForEach(sortObjParams, sObj =>
            //{
            //    if (sObj.GetType() == new Field().GetType())
            //    {
            //        Field sortField = (Field)sObj;
            //        SortParm newSortParm = new SortParm(sortField, SortDirection.Ascending) { ParmOffset = sortField.GroupBufferOffset };
            //        _sortParmList.Add(newSortParm);
            //        // sortOffSet += sortField.TotalLength;
            //    }
            //    else if (sObj.GetType() == SortDirection.Descending.GetType())
            //        _sortParmList[_sortParmList.Count - 1].ParmSortDirection = (SortDirection)sObj;
            //    else
            //    {
            //        string strParm = (string)sObj;
            //        if (String.Compare(strParm, "DESC") == 0 || String.Compare(strParm, "DESCENDING") == 0)
            //            _sortParmList[_sortParmList.Count - 1].ParmSortDirection = SortDirection.Descending;
            //    }
            //});
            return SortData();
        }

        #endregion

        #region private Methods

        /// <summary>
        /// Build Process Sort Control for sorting data
        /// </summary>
        private void BuildProcessSortControl()
        {
            StringBuilder sbControl = new StringBuilder();
            const int offset = 0;
            foreach (SortParameter sParm in _sortParmList)
            {
                string batchDataType;
                switch (sParm.Format)
                {
                    case SortKeyFormat.Character: batchDataType = "char"; break;
                    case SortKeyFormat.ZonedDecimal:
                        batchDataType = "decimal"; break;
                    case SortKeyFormat.PackedDecimal: batchDataType = "packed"; break;
                    default: batchDataType = "char"; break;
                }

                int newOffSet = sParm.Offset == 0 ? offset : sParm.Offset;

                sbControl.AppendFormat(" -key=offset:{0},size:{1},{2},{3}",
                    newOffSet,
                    sParm.Length,
                    sParm.SortDirection,
                    batchDataType);
            }

            _sortControlData = sbControl.ToString();
        }

        /// <summary>
        /// Parse Mainframe style sort string and create sort parm objects
        /// </summary>
        /// <param name="sortString"></param>
        private void BuildSortParms(string sortString)
        {
            sortString = sortString.Replace("SORT FIELDS=(", "");
            string[] stringArray = sortString.Split(new char[] { ',' });

            int parmPos = 0;
            int intTest;
            int currentListIdx = 0;
            foreach (string strParm in stringArray)
            {
                if (int.TryParse(strParm, out intTest))
                {
                    if (parmPos == 1)
                    {
                        _sortParmList[currentListIdx].Length = intTest;
                        parmPos = 2;
                    }
                    else
                    {
                        _sortParmList.Add(new SortParameter());
                        currentListIdx = _sortParmList.Count - 1;
                        _sortParmList[currentListIdx].Offset = intTest - 1;
                        parmPos = 1;
                    }
                }
                else
                {
                    if (String.Compare(strParm, "A", false) == 0 || String.Compare(strParm, "D", false) == 0)
                    {
                        _sortParmList[currentListIdx].SortDirection =
                            String.Compare(strParm, "A", false) == 0 ?
                                SortDirection.Ascending :
                                SortDirection.Descending;
                    }
                    {
                        switch (strParm.ToUpper())
                        {
                            case "CH": _sortParmList[currentListIdx].Format = SortKeyFormat.Character; break;
                            case "ZD": _sortParmList[currentListIdx].Format = SortKeyFormat.ZonedDecimal; break;
                            case "PD": _sortParmList[currentListIdx].Format = SortKeyFormat.PackedDecimal; break;
                            //Implement other cases as needed
                            default: _sortParmList[currentListIdx].Format = SortKeyFormat.Character; break;


                        }
                    }

                }
            }

        }

        /// <summary>
        /// Submit Sort Job either asa Batch Sort or Process Sort
        /// </summary>
        /// <returns>int ReturnCode</returns>
        private int SubmitSort()
        {
            int result = 16; // and yes, I'll entertain suggestions as to what this magic number is...

            //it is possible that an internal sort does not actually write to the temp file.
            if (_inputFile.FileStatus == FileStatus.File_doesnt_exist)
            {
                using (StreamWriter outfile = new StreamWriter(_inputFile.FilePath))
                {
                }
            }
            else
            {
                _inputFile.CloseFile();
            }               

            if (_inputFile is IFileLink)
            {
                string filename = (_inputFile as IFileLink).FilePath;
                if (File.Exists(filename))
                {
                    FileInfo fileInfo = new FileInfo(filename);
                    if (_inputFile.FilePath != _sortInFileName)
                        fileInfo.CopyTo(_sortInFileName);
                    _inputFileLength = fileInfo.Length;
                    _sortRecordLength = _inputFile.RecordLength;

                    BuildProcessSortControl();
                    if (!_sortControlData.IsEmpty())
                    {
                        result = SubmitInternalSort();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Submit Internal Sort
        /// </summary>
        /// <returns></returns>
        private int SubmitInternalSort()
        {

            try
            {
                List<IList<byte>> bList = new List<IList<byte>>(), bOutList = new List<IList<byte>>();
                List<string> sList = new List<string>(), sOutList = new List<string>();

                //Read The file records into List
                Console.WriteLine("Start InternalSort Read Input File: " + DateTime.Now.ToLongTimeString());
                if (_inputFile.FileOrganization == FileOrganization.LineSequential)
                    sList = GetInputStringList(_inputFile);
                else
                    bList = GetInputByteList(_inputFile);

                //Sort the List
                Console.WriteLine("Start InternalSort LinqSort: " + DateTime.Now.ToLongTimeString());
                if (_inputFile.FileOrganization == FileOrganization.LineSequential)
                    sOutList = InternalLinqSort.Sort(sList, _sortParmList);
                else
                    bOutList = InternalLinqSort.Sort(bList, _sortParmList);

                //Write new file from sorted List
                Console.WriteLine("Start InternalSort Write Output File: " + DateTime.Now.ToLongTimeString());
                if (_inputFile.FileOrganization == FileOrganization.LineSequential)
                    WriteOutputFile(sOutList, _outputFile);
                else
                    WriteOutputFile(bOutList, _outputFile);

                Console.WriteLine("Finish InternalSort: " + DateTime.Now.ToLongTimeString());
                return bOutList.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("InternalSort error: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Read input file into List of strings
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        private static List<string> GetInputStringList(IFileLink inputFile)
        {

            List<string> inputList = new List<string>();
            while (inputFile.ReadLine() != string.Empty)
            {
                inputList.Add(inputFile.BufferAsString);
            }

            return inputList;
        }

        /// <summary>
        /// CReate List of Lists of bytes from IFileLink input file
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        private static List<IList<byte>> GetInputByteList(IFileLink inputFile)
        {
            List<IList<byte>> inputList = new List<IList<byte>>();
            while (inputFile.ReadLine() != string.Empty)
            {
                inputList.Add(inputFile.BufferAsBytes.ToList());
            }

            return inputList;
        }

        /// <summary>
        /// Write IFileLink output file from List of Lists of bytes
        /// </summary>
        /// <param name="sortedData"></param>
        /// <param name="outputFile"></param>
        private static void WriteOutputFile(List<IList<byte>> sortedData, IFileLink outputFile)
        {
            for (int ctr = 0; ctr < sortedData.Count; ctr++)
            {
                outputFile.WriteLine(sortedData[ctr].ToArray());
            }
        }

        private static void WriteOutputFile(List<string> sortedData, IFileLink outputFile)
        {
            for (int ctr = 0; ctr < sortedData.Count; ctr++)
            {
                outputFile.WriteLine(sortedData[ctr]);
            }
        }

        #endregion
    } 
    #endregion

    #region Internal Link Sort

    internal static class InternalLinqSort
    {
        /// <summary>
        /// Sort the Bytes List
        /// </summary>
        /// <param name="byteList"></param>
        /// <param name="sortParms"></param>
        /// <returns></returns>
        public static List<IList<byte>> Sort(List<IList<byte>> byteList, List<SortParameter> sortParms)
        {
            StringBuilder sParmString = new StringBuilder();
            int ctr = 1;
            foreach (SortParameter sparm in sortParms)
            {
                sParmString.AppendLine(string.Concat("Sort Field #", ctr.ToString(), ": Offset:", sparm.Offset.ToString(), " Length:", sparm.Length.ToString(),
                    " Format:", sparm.Format.ToString(),
                    " Order:", sparm.SortDirection.ToString()));
                ctr++;
            }

            List<IList<byte>> newSortedList = SortByteListRecords(byteList, sortParms);

            Console.WriteLine(sParmString.ToString());

            return newSortedList;
        }

        /// <summary>
        /// Sort teh String List
        /// </summary>
        /// <param name="sList"></param>
        /// <param name="sortParms"></param>
        /// <returns></returns>
        public static List<string> Sort(List<string> sList, List<SortParameter> sortParms)
        {
            StringBuilder sParmString = new StringBuilder();
            int ctr = 1;
            foreach (SortParameter sparm in sortParms)
            {
                sParmString.AppendLine(string.Concat("Sort Field #", ctr.ToString(), ": Offset:", sparm.Offset.ToString(), " Length:", sparm.Length.ToString(),
                    " Format:", sparm.Format.ToString(),
                    " Order:", sparm.SortDirection.ToString()));
                ctr++;
            }

            List<string> newSortedList = SortStringListRecords(sList, sortParms);

            Console.WriteLine(sParmString.ToString());

            return newSortedList;
        }

        /// <summary>
        /// Create Lambda ordering and sort the Byte List
        /// </summary>
        /// <param name="records"></param>
        /// <param name="sortParms"></param>
        /// <returns></returns>
        private static List<IList<byte>> SortByteListRecords(List<IList<byte>> records, List<SortParameter> sortParms)
        {
            ByteListComparer byteComparer = new ByteListComparer();
            ByteListComp3Comparer byteComp3Comparer = new ByteListComp3Comparer();
            ByteListZDComparer byteZDComparer = new ByteListZDComparer();
            IComparer<IList<byte>> comparer = byteComparer;

            Func<IList<byte>, IList<byte>> sortLambda = GetSortLambda(sortParms[0]);
            switch (sortParms[0].Format)
            {
                case SortKeyFormat.Character:
                case SortKeyFormat.Binary: comparer = byteComparer; break;
                case SortKeyFormat.PackedDecimal: comparer = byteComp3Comparer; break;
                case SortKeyFormat.ZonedDecimal: comparer = byteZDComparer; break;
            }

            var result = (sortParms[0].SortDirection == SortDirection.Descending ? records.OrderByDescending(sortLambda, comparer) : records.OrderBy(sortLambda, comparer));


            foreach (SortParameter crit in sortParms.Skip(1))
            {
                Func<IList<byte>, IList<byte>> thenByLambda = GetSortLambda(crit);
                switch (crit.Format)
                {
                    case SortKeyFormat.Character:
                    case SortKeyFormat.Binary: comparer = byteComparer; break;
                    case SortKeyFormat.PackedDecimal: comparer = byteComp3Comparer; break;
                    case SortKeyFormat.ZonedDecimal: comparer = byteZDComparer; break;
                }
                result = crit.SortDirection == SortDirection.Descending ? result.ThenByDescending(thenByLambda, comparer) : result.ThenBy(thenByLambda, comparer);
            }

            return result.ToList();
        }

        private static List<string> SortStringListRecords(List<string> records, List<SortParameter> sortParms)
        {
            StringListComparer stringComparer = new StringListComparer();
            StringListNumberComparer stringNumberComparer = new StringListNumberComparer();
            IComparer<string> comparer;

            Func<string, string> sortLambda = GetStringSortLambda(sortParms[0]);
            switch (sortParms[0].Format)
            {
                case SortKeyFormat.ZonedDecimal: comparer = stringNumberComparer; break;
                default: comparer = stringComparer; break;
            }

            var result = (sortParms[0].SortDirection == SortDirection.Descending ? records.OrderByDescending(sortLambda, comparer) : records.OrderBy(sortLambda, comparer));

            foreach (SortParameter crit in sortParms.Skip(1))
            {
                Func<string, string> thenByLambda = GetStringSortLambda(crit);
                switch (crit.Format)
                {
                    case SortKeyFormat.ZonedDecimal: comparer = stringNumberComparer; break;
                    default: comparer = stringComparer; break;
                }
                result = crit.SortDirection == SortDirection.Descending ? result.ThenByDescending(thenByLambda, comparer) : result.ThenBy(thenByLambda, comparer);
            }

            return result.ToList();
        }

        private static Func<IList<byte>, IList<byte>> GetSortLambda(SortParameter sortCrit)
        {
            return rec => rec.Skip(sortCrit.Offset).Take(sortCrit.Length).ToList();
        }

        private static Func<string, string> GetStringSortLambda(SortParameter sortCrit)
        {
            return rec => rec.Substring(sortCrit.Offset, sortCrit.Length);
        }

        public class ByteListComparer : IComparer<IList<byte>>
        {
            public int Compare(IList<byte> x, IList<byte> y)
            {
                int result;
                for (int index = 0; index < Math.Min(x.Count, y.Count); index++)
                {
                    result = x[index].CompareTo(y[index]);
                    if (result != 0) return result;
                }
                return x.Count.CompareTo(y.Count);
            }
        }

        public class ByteListComp3Comparer : IComparer<IList<byte>>
        {
            public int Compare(IList<byte> x, IList<byte> y)
            {
                return GetComp3Value(x).CompareTo(GetComp3Value(y));
            }

            private decimal GetComp3Value(IList<byte> compBytes)
            {
                string packSign = string.Empty;
                StringBuilder hexValue = new StringBuilder(compBytes.Count * 2);
                foreach (byte b in compBytes)
                {
                    hexValue.AppendFormat("{0:x2}", b);
                }
                if (hexValue[hexValue.Length - 1] == 'D' || hexValue[hexValue.Length - 1] == 'd')
                {
                    packSign = "-";
                }
                else if (hexValue[hexValue.Length - 1] == 'C' || hexValue[hexValue.Length - 1] == 'c' || hexValue[hexValue.Length - 1] == 'F' || hexValue[hexValue.Length - 1] == 'f')
                {
                    packSign = "+";
                }
                else
                {
                    hexValue.Clear();
                    hexValue.Append("+0");
                }
                hexValue.Remove(hexValue.Length - 1, 1);
                decimal outDec = 0;
                decimal.TryParse(string.Concat(packSign, hexValue.ToString()), out outDec);
                return outDec;

            }
        }

        public class ByteListZDComparer : IComparer<IList<byte>>
        {
            public int Compare(IList<byte> x, IList<byte> y)
            {
                return GetZDValue(x).CompareTo(GetZDValue(y));
            }

            private decimal GetZDValue(IList<byte> compBytes)
            {
                char zdSign = '+'; string lastDigit;
                byte lastByte = compBytes.Last();
                if ((lastByte >= 0xD0 && lastByte <= 0xD9) || (lastByte >= 0x70 && lastByte <= 0x79))
                {
                    zdSign = '-';
                }

                lastDigit = lastByte.ToString("{0:x2}").Substring(1);

                StringBuilder numString = new StringBuilder();
                numString.Append(zdSign);

                for (int ctr = 0; ctr < (compBytes.Count - 1); ctr++)
                {
                    numString.Append(Convert.ToChar(compBytes[ctr]));
                }
                numString.Append(lastDigit);

                decimal outDec = 0;
                decimal.TryParse(numString.ToString(), out outDec);
                return outDec;


            }
        }

        public class StringListComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return x.CompareTo(y);
            }
        }
        public class StringListNumberComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                decimal dx = 0, dy = 0;
                decimal.TryParse(x, out dx);
                decimal.TryParse(y, out dy);
                return dx.CompareTo(dy);
            }
        }
    } 
    #endregion
}
