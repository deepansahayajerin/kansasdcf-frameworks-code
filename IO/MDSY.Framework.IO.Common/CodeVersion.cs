using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework
{
    /// <summary>
    /// Holds the version information of each .cs file.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    [CodeVersion(1)]
    public class CodeVersion : System.Attribute
    {
        public int Version;

        public CodeVersion(int version)
        {
            Version = version;
        }
    }
}
