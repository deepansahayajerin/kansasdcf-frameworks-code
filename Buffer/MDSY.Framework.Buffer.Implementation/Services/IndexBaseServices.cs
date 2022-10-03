using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
using Unity;
using MDSY.Framework.Buffer.Common;
using Microsoft.Extensions.Configuration;
using MDSY.Framework.Configuration.Common;


namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IIndexBaseServices.
    /// </summary>
    [InjectionImplementer(typeof(IIndexBaseServices))]
    internal sealed class IndexBaseServices : IIndexBaseServices
    {
        #region private methods
        private static bool GetIsZeroBased()
        {
            // We default to 1-based arrays. We'll only use the 0-based implementation 
            // if the config setting says to. 
            return ConfigSettings.GetAppSettingsBool(Constants.AppSettings.IsZeroBasedArrays); 
        }
        #endregion

        #region IIndexBaseServices

        /// <summary>
        /// Returns 1-based index unless config settings is 0-based.
        /// </summary>
        /// <param name="cSharpIndex">The zero based C# index</param>
        /// <returns>The 1-based index unless the config settings is 0-based.</returns>
        public int CSharpIndexToConvertedCodeIndex(int cSharpIndex)
        {
            return IsZeroBased ? cSharpIndex : cSharpIndex + 1;
        }

        /// <summary>
        /// Returns 1-based index if the config settings is 0-based.
        /// </summary>
        /// <param name="convertedCodeIndex"></param>
        /// <returns>The 1-based index if the config settings is 0-based.</returns>
        public int ConvertedCodeIndexToCSharpIndex(int convertedCodeIndex)
        {
            return IsZeroBased ? convertedCodeIndex : convertedCodeIndex - 1;
        }

        /// <summary>
        /// Returns true if if the config setting is 0-based
        /// </summary>
        public bool IsZeroBased
        {
            get { return GetIsZeroBased(); }
        }
        #endregion
    }
}
