using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Common;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which provides support for system directives. 
    /// </summary>
    [InjectionInterface]
    public interface IDirectiveServices
    {
        /// <summary>
        /// Specifies which numeric field value move type is in effect.
        /// </summary>
        FieldValueMoveType FieldValueMoves { get; set; }
    }
}
