using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// This determines the type of DB access to be used.
    /// StoredProcedures = 0, DynamicSql = 1, Other = 5
    /// </summary>
    public enum DbAccessType
    {
        StoredProcedures = 0,
        DynamicSql = 1,
        Other = 5
    }
}

