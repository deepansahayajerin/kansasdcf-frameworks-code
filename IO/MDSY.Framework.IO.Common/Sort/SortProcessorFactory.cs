using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Core;

namespace MDSY.Framework.IO.Common
{
    /// <summary>
    /// Provides instances of the ISortProcessor implementer. 
    /// </summary>
    public static class SortProcessorFactory
    {
        /// <summary>
        /// Returns an instance of the implementing ISortProcessor object.
        /// </summary>
        /// <returns>A new instance, if an implementing object is correctly mapped in the app.config file.</returns>
        /// <exception cref="Ateras.Core.Inversion.InversionContainerException">If no mapping exists for ISortProcessor.</exception>
        public static ISortProcessor GetSortProcessor()
        {
            if (!InversionContainer.ContainsMapping<ISortProcessor>())
                throw new InversionContainerException("No mapping found for type.", typeof(ISortProcessor), null);

            return InversionContainer.GetImplementingObject<ISortProcessor>();
        }


        public static int SubmitExternalProcessSort(IFileLink inputFileLink,
            IFileLink outputFileLink,
            long inputFileLength,
            IEnumerable<SortParm> sortParams)
        {
            int result = default(int);

            var sorter = GetSortProcessor();
            if (sorter != null)
            {
                sorter.SetInputFile(inputFileLink);
                sorter.SetOutputFile(outputFileLink);
                sorter.SortRecordLength = inputFileLink.RecordLength;
            }


            return result;


        }

    }
}
