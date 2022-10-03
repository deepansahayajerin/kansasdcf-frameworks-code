using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Indicates the condition upon which fields should be updated.
    /// </summary>
    public enum FieldSelectionScope
    {
        All,
        AllBut,
        AllButCurrent,
        AllCorrectFields,
        AllInErrorFields,
        Current,
        Field
    }
}

