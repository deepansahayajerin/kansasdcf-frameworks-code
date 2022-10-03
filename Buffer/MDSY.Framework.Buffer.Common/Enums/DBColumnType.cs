using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{

    /// <summary>
    /// Specifies the data type contained by a field.
    /// </summary>
    public enum DBColumnType
    {
        Char = 0,
        Varchar,
        NChar,
        NVarChar,
        SmallInt,
        Int,
        BigInt,
        Decimal,
        Money,
        Float,
        Real,  
        Bit,
        Binary,
        DateTime,
        DateTime2,
        Date,
        Time,
        Db2Time,
        DB2TimeStamp
    }
}
