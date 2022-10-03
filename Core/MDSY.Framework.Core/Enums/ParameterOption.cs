using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    public enum ParameterOption
    {
        /// <summary>
        /// No set
        /// </summary>
        Null,
        /// <summary>
        /// The fields must match on both sides of the CALL NAT, this is by default
        /// </summary>
        ByReference,
        /// <summary>
        /// The fields might not match both the total lenght of the sent in parameter must match the total length of the parameter
        /// </summary>
        ByValue,
        /// <summary>
        /// The fields might not match both the total lenght of the sent in parameter must match the total length of the parameter
        /// and it will return back.
        /// </summary>
        ByValueResult
    }
}

