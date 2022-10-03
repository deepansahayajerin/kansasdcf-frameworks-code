using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Indicates the database status codes returned
    /// Good = 0, EndOfList = 307, RowNotFound = 326, DBNotAvailable = 966, DuplicatesViolation = 1205, EndofIndex = 1707, IndexRowNotFound = 1726
    /// </summary>
    public enum DbStatus
    {
        Good = 0,
        EndOfList = 307,
        RowNotFound = 326,
        DBNotAvailable = 966,
        DuplicatesViolation = 1205,
        EndofIndex = 1707,
        IndexRowNotFound = 1726
    }
}

