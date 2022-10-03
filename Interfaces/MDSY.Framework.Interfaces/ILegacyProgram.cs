using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Interfaces
{
    /// <summary>
    /// Defines a base definition of an object which represents a legacy code symbol, 
    /// usually an external program that can be called via ICallableProgram.
    /// </summary>
    /// <remarks>ILegacyProgram descendants are managed and accessed through ExternalProgramManager.</remarks>
    public interface ILegacyProgram
    {
        /// <summary>
        /// Gets the legacy source code type of this routine. e.g., "COBOL", "Natural", etc.
        /// </summary>
        string SourceType { get; }
        string AssemblyName { get; }
        string Name { get; }
    }
}

