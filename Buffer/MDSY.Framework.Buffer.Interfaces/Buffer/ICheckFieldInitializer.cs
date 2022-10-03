using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which can receive initialization during the creation of an ICheckField. 
    /// </summary>
    public interface ICheckFieldInitializer : IArrayElementInitializer<ICheckField>
    {
        /// <summary>
        /// Gets or sets the name of the checkfield.
        /// </summary>
        new string Name { get; set; }

        /// <summary>
        /// Gets or sets the field object associated with the checkfield.
        /// </summary>
        IField Field { get; set; }
    }
}
