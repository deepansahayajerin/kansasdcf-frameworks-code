using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which can receive finalization during the creation of an array(of TItem). 
    /// </summary>
    public interface IArrayFinalizer<TItem>
            where TItem : IBufferElement
    {

    }
}
