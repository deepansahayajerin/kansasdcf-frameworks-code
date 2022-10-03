using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Indicates what kind of deletion is to be performed when deleting a row:
    /// CascadeNone, CascadePermanent, CascadeSelective, CascadeAll
    /// </summary>
    public enum DeleteRowOption
    {
        CascadeNone,
        CascadePermanent,
        CascadeSelective,
        CascadeAll
    }
}

