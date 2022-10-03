using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Indicates if changes to a map field's attributes should persist from 
    /// screen to screen or only for the current one.
    /// </summary>
    public enum FieldAttributeScope
    {
        Permanent,
        Temporary
    }
}

