using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSY.Framework.IO.Common
{
    #region Internal Sort used for COBOL sorting
    public class InternalArraySort
    {

        #region private constants...
        private const int INT_badFileResult = 12;
        private const int INT_goodFileResult = 0;
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
        private IArrayElementAccessor<IGroup> _outputFile;
        private IArrayElementAccessor<IGroup> _inputFile;
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


        //public InternalSort(FileDefinition inputFile, FileDefinition outputFile)
        public InternalArraySort(IArrayElementAccessor<IGroup> inputFile, IArrayElementAccessor<IGroup> outputFile)
        {
            String tempFilePath = Path.GetTempPath();
            _sortInFileName = string.Concat(tempFilePath, "SI", DateTime.Now.ToString("yyMMddHHmmssfff"), STR_TextFileExt);
            _sortOutFileName = string.Concat(tempFilePath, "SO", DateTime.Now.ToString("yyMMddHHmmssfff"), STR_TextFileExt);
            SetInputList(inputFile);
            SetOutputList(outputFile);
        }


        #endregion
        #region Public Methods
        /// <summary>
        /// Set the full input file path name for already defined input files
        /// </summary>
        /// <param name="inputFileName"></param>
        //public void SetInputFile(FileDefinition inputFile)
        public void SetInputList(IArrayElementAccessor<IGroup> inputFile)
        {

            _inputFile = inputFile;

        }

        /// <summary>
        /// Set the full output file path name
        /// </summary>
        /// <param name="inputFileName"></param>
        //public void SetOutputFile(FileDefinition outputFile)
        public void SetOutputList(IArrayElementAccessor<IGroup> outputFile)
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
                    foreach (IBufferElement f in ((IGroup)sOBJ).Elements)
                    { //.ChildCollection.Values) {
                        if (f is IField)
                        {
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


            return SubmitInternalSort();
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

                sList = GetInputStringList(_inputFile);


                //Sort the List
                Console.WriteLine("Start InternalSort LinqSort: " + DateTime.Now.ToLongTimeString());

                sOutList = InternalLinqSort.Sort(sList, _sortParmList);


                //Write new file from sorted List
                Console.WriteLine("Start InternalSort Write Output File: " + DateTime.Now.ToLongTimeString());

                WriteOutputFile(sOutList, _outputFile);


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
        private static List<string> GetInputStringList(IArrayElementAccessor<IGroup> inputFile)
        {

            List<string> inputList = new List<string>();
            //while (inputFile.ReadLine() != string.Empty)
            //{
            //    inputList.Add(inputFile.BufferAsString);
            //}

            return inputList;
        }

        /// <summary>
        /// CReate List of Lists of bytes from IFileLink input file
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        private static List<IList<byte>> GetInputByteList(IArrayElementAccessor<IGroup> inputFile)
        {
            List<IList<byte>> inputList = new List<IList<byte>>();
            //while (inputFile.ReadLine() != string.Empty)
            //{
            //    inputList.Add(inputFile.BufferAsBytes.ToList());
            //}

            return inputList;
        }

        /// <summary>
        /// Write IFileLink output file from List of Lists of bytes
        /// </summary>
        /// <param name="sortedData"></param>
        /// <param name="outputFile"></param>
        private static void WriteOutputFile(List<IList<byte>> sortedData, IArrayElementAccessor<IGroup> outputFile)
        {
            for (int ctr = 0; ctr < sortedData.Count; ctr++)
            {
                // outputFile.WriteLine(sortedData[ctr].ToArray());
            }
        }

        private static void WriteOutputFile(List<string> sortedData, IArrayElementAccessor<IGroup> outputFile)
        {
            for (int ctr = 0; ctr < sortedData.Count; ctr++)
            {
                //  outputFile.WriteLine(sortedData[ctr]);
            }
        }

        #endregion
    }
    #endregion

}
