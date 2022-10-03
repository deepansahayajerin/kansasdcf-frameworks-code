using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MDSY.Framework.Buffer.Unity
{
    /// <summary>
    /// Encapsulates a row of xml data from a TypeRegXml file.
    /// </summary>
    [Serializable]
    internal sealed class RegistrationAssemblyInfo
    {
        /// <summary>
        /// Initializes a new instance of the RegistrationAssemblyInfo class via serialization.
        /// </summary>
        public RegistrationAssemblyInfo(SerializationInfo info, StreamingContext context)
        {
            Filename = info.GetString("Filename");
            Enabled = info.GetBoolean("Enabled");
            Throw = info.GetBoolean("Throw");
        }

        /// <summary>
        /// Initializes a new instance of the RegistrationAssemblyInfo class.
        /// </summary>
        public RegistrationAssemblyInfo(string filename, bool enabled, bool throwOnLoadError)
        {
            Filename = filename;
            Enabled = enabled;
            Throw = throwOnLoadError;
        }

        /// <summary>
        /// Gets or sets the name of the assembly from which type registrations should be loaded. 
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets whether the assembly indicated by Filename should be loaded. 
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets whether an exception should be thrown in the event of a load error.
        /// </summary>
        public bool Throw { get; set; }
    }
}

