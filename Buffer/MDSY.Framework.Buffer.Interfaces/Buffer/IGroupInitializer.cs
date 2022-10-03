using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which can receive initialization during the creation of a group array.
    /// </summary>
    public interface IGroupInitializer : IBufferElementInitializer,
        IArrayElementInitializer<IGroup>
    {
        IDictionary<string, IArrayElementAccessorBase> DefineTimeAccessors { get; set; }
    }
}
