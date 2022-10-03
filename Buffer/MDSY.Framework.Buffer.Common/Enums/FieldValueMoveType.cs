using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Specifies how numeric data is moved to a string field. Supports the ADSO
    /// <c>COBOL MOVES</c> directive.
    /// </summary>
    public enum FieldValueMoveType
    {
        /// <summary>Indicates no specific move type.</summary>
        Undefined = 0,
        /// <summary>Indicates that COBOL move rules are in effect.</summary>
        CobolMoves,
        /// <summary>Indicates that ADSO move rules are in effect.</summary>
        AdsoMoves
    }
}
