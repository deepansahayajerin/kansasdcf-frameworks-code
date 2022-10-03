using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which participates in a REDEFINE operation, redefining a section of buffer space.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface IRedefinition
    {
        /// <summary>
        /// Gets or sets the (optional) buffer element that this redefinition object redefines.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If RedefinedElement is null, this object is participating in a REDEFINE at a level lower 
        /// than the root level, thus it is redefining only a sub-section of the Redefinition root's RedefinedElement.
        /// </para>
        /// <para>To get the root-level RedefinedElement, access RootLevelRedefinition.</para>
        /// </remarks>
        IBufferElement RedefinedElement { get; set; }

        /// <summary>
        /// Gets the parent element which is the root of the REDEFINE.
        /// </summary>
        IRedefinition RootLevelRedefinition { get; }

        /// <summary>
        /// Returns <c>true</c> if any parent above this element implements IRedefinition.
        /// </summary>
        bool HasRedefineInParents { get; }
    }
}
