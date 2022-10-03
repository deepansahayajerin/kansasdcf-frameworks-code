using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Unity
{
    /// <summary>
    /// Indicates a class that provides implementation for a dependency injection 
    /// type definition (typically an interface). 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class InjectionImplementerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the InjectionImplementerAttribute class.
        /// </summary>
        public InjectionImplementerAttribute(Type interfaceType)
            : this(interfaceType, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the InjectionImplementerAttribute class.
        /// </summary>
        public InjectionImplementerAttribute(Type interfaceType, string registrationName)
        {
            InterfaceType = interfaceType;
            RegistrationName = registrationName;
        }

        /// <summary>
        /// The type of interface implemented by the decorated object.
        /// </summary>
        public Type InterfaceType { get; set; }

        /// <summary>
        /// The name used when registering this implementation object with the inversion container.
        /// </summary>
        public string RegistrationName { get; set; }
    }
}

