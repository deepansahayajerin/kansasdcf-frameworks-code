using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Indicates what automatic status codes is returned after a DB operation:
    /// Good = 0, EndOfList = 307, RowNotFound = 326, EndofIndex = 1707, IndexRowNotFound = 1726, ScratchNotFound = 4303, ScratchRowNotFound = 4305,
    /// ScratchRowReplaced = 4317, QueueNotFound = 4404, QueueRowNotFound = 4405, WaitRequired = 5149
    /// </summary>
    public enum DbAutoStatus
    {
        Good = 0,
        EndOfList = 307,
        RowNotFound = 326,
        EndofIndex = 1707,
        IndexRowNotFound = 1726,
        ScratchNotFound = 4303,
        ScratchRowNotFound = 4305,
        ScratchRowReplaced = 4317,
        QueueNotFound = 4404,
        QueueRowNotFound = 4405,
        WaitRequired = 5149
    }
}

