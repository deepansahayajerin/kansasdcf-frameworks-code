using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Indicates the field condition:
    /// Identical, Changed, Truncated, Erased, InError
    /// </summary>
    public enum FieldCondition
    {
        Identical,
        Changed,
        Truncated,
        Erased,
        InError
    }
}

