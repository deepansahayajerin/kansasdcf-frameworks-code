using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which can be in a definition state.
    /// </summary>
    public interface IDefineable
    {
        /// <summary>
        /// Informs the object that its period of definition has ended. 
        /// </summary>
        void EndDefinition();

        /// <summary>
        /// Gets whether this object is currently in its period of definition.
        /// </summary>
        bool IsDefining { get; set; }

        void RestartDefinition();
    }
}
