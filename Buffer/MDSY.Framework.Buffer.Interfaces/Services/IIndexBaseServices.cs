using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Unity;

namespace MDSY.Framework.Buffer.Interfaces
{

    /// <summary>
    /// Defines a service which provides methods for dealing with zero-based or
    /// one-based indexes.
    /// </summary>
    [InjectionInterface]
    public interface IIndexBaseServices
    {
        /// <summary>
        /// Manipulates the given <paramref name="convertedCodeIndex"/> so that 
        /// it becomes zero-based as needed by the core Framework C# code. 
        /// </summary>
        /// <param name="convertedCodeIndex">The index value appearing in converted code.</param>
        /// <returns>The zero-based index value.</returns>
        int ConvertedCodeIndexToCSharpIndex(int convertedCodeIndex);

        /// <summary>
        /// Manipulates the given zero-based <paramref name="cSharpIndex"/> so that 
        /// it is output in the appropriate format as needed by the converted code. 
        /// e.g. For COBOL, the value is returned as one-based.
        /// </summary>
        /// <param name="cSharpIndex">The index value as used in C# code.</param>
        /// <returns>The index as needed by the converted code.</returns>
        int CSharpIndexToConvertedCodeIndex(int cSharpIndex);

        /// <summary>
        /// Returns <c>true</c> if the currently executing converted code has
        /// zero-based indexes. For COBOL, this would return false.
        /// </summary>
        bool IsZeroBased { get; }
    }
}
