using System;
using System.Collections.Generic;

namespace MDSY.Framework.Data.SQL
{
    /// <summary>
    /// Indicates the database status codes returned
    /// </summary>
    public enum ErrorStatus
    {
        None = 0,
        NoRowsReturned = 1,
        MultipleRowsReturned = 2,
        DBNotAvailable = 3,
        DuplicatesViolation = 4,
        DataConstraintViolation = 5,
        DBTimeout = 6
    }
}

