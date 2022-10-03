using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Unity
{


    /// <summary>
    /// Indicates an interface that is defined with the intention of being implemented 
    /// and resolved through dependency injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class InjectionInterfaceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the InjectionInterfaceAttribute class.
        /// </summary>
        public InjectionInterfaceAttribute()
        {

        }
    }
}

