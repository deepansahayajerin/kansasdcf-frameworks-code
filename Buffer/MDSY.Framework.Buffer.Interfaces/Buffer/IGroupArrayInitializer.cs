using MDSY.Framework.Buffer.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which can receive initialization during the creation of a field array.
    /// </summary>
    public interface IGroupArrayInitializer : IArrayBaseInitializer
    {
        IGroupArray AsReadOnly();

    }
}
