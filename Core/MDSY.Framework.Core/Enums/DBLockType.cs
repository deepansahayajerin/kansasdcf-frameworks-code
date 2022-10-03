using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// This determines the type of DB access to be used.
    /// StoredProcedures = 0, DynamicSql = 1, Other = 5
    /// </summary>
    public enum DBLockType
    {
        None = 0,
        RowShared = 1,
        RowExclusive = 2,
        Table = 3
    }
}

