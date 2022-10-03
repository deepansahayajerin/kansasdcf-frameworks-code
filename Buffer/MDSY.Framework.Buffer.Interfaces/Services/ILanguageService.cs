using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;

namespace MDSY.Framework.Buffer.Interfaces
{

    /// <summary>
    /// Provides language emulation services.
    /// </summary>
    [InjectionInterface]
    public interface ILanguageService
    {
        /// <summary>
        /// Gets the language emulation object.
        /// </summary>
        ILegacyLanguage Language { get; }
    }
}
