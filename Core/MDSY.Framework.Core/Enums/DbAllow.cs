using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// List of DB allowed error codes
    /// Good = 0, EndOfList = 307, RowNotFound = 326, DBNotAvailable = 966, DuplicatesViolation = 1205, EndOfIndex = 1707,
    /// IndexRowNotFound = 1726, ScratchNotFound = 4303, ScratchRowNotFound = 4305, ScratchRowReplaced = 4317, QueueNotFound = 4404,
    /// QueueRowNotFound = 4405, WaitRequired = 5149, DBAnyError = 9999
    /// </summary>
    public enum DbAllow
    {
        Good = 0,
        EndOfList = 307,
        RowNotFound = 326,
        DBDeadlock = 329,
        DBNotAvailable = 966,
        DuplicatesViolation = 1205,
        EndOfIndex = 1707,
        IndexRowNotFound = 1726,
        ScratchNotFound = 4303,
        ScratchRecordNotFound = 4305,
        ScratchRecordReplaced = 4317,
        QueueNotFound = 4404,
        QueueRowNotFound = 4405,
        WaitRequired = 5149,
        DBAnyError = 9999

    }
}

