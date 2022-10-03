
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Unity;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which represents a legacy language; one in which applications have been
    /// written which are to be converted so as to run in C#. 
    /// </summary>
    [InjectionInterface]
    public interface ILegacyLanguage
    {
        /// <summary>
        /// Gets the name of the currently active legacy language.
        /// </summary>
        string ActiveLanguageName { get; }


    }




}
