using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
using Unity;
//CHADusing Unity.Attributes;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface ILanguageServices.
    /// </summary>
    [InjectionImplementer(typeof(ILanguageService))]
    internal sealed class LanguageService : ILanguageService
    {

        /// <summary>
        /// Gets the name of the currently active legacy language. 
        /// </summary>
        public ILegacyLanguage Language
        {
            get { return UnitySingleton.Container.Resolve<ILegacyLanguage>(); }
        }
    }
}
