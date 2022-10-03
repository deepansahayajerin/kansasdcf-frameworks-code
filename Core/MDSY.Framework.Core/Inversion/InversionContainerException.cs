using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MDSY.Framework.Core
{
    [Serializable]
    public class InversionContainerException : Exception
    {
        public InversionContainerException(string message, Type interfaceType, Type implementationType, Exception innerException)
            : base(message, innerException)
        {
            InterfaceType = interfaceType;
            ImplementationType = implementationType;
        }

        public InversionContainerException(string message, Type interfaceType, Type implementationType)
            : this(message, interfaceType, implementationType, null)
        { }

        public InversionContainerException()
        {

        }

        public InversionContainerException(string message)
            : base(message)
        {

        }

        public InversionContainerException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public Type InterfaceType { get; set; }
        public Type ImplementationType { get; set; }
    }
}
