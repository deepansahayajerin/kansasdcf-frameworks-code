using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Diagnostics;

namespace MDSY.Framework.IO.Common
{
    public interface ISortProcessor
    {
        #region attributes
        /// <summary>
        /// Returns the number of Sort In Records
        /// </summary>
        [Obsolete("This doesn't appear to ever be set or used.", false)]
        int SortCount { get; }

        /// <summary>
        /// Returns the length of the Sort Record
        /// </summary>
        int SortRecordLength { get; set; }
        #endregion

        #region operations

        [Obsolete]
        int ExecuteSort();

        /// <summary>
        /// Sort the SortIn data based on sort parm text
        /// </summary>
        /// <param name="sortParms"> Sort Parameters </param>
        int ExecuteSort(string SortParms);

        /// <summary>
        /// Sort the SortIn data based on sort parameters
        /// </summary>
        /// <param name="sortParms"> Sort Parameters </param>
        int ExecuteSort(params SortParm[] sortParms);

        int ExecuteSort(IEnumerable<SortParm> sortParms);

        /// <summary>
        /// Sort the SortIn data based on sort parameters
        /// </summary>
        /// <param name="sortObjParams"></param>
        [Obsolete("Use ExecuteSort with SortParm[] or with IEnumerable<SortParm> instead", true)]
        int ExecuteSort(params Object[] sortObjParams);

        /// <summary>
        /// Set the full input file path name for already defined input files
        /// </summary>
        /// <param name="inputFileName"></param>
        void SetInputFile(IFileLink inputFile);

        /// <summary>
        /// Set the full output file 
        /// </summary>
        /// <param name="inputFileName"></param>
        void SetOutputFile(IFileLink outputFile);
        #endregion
    }
}
