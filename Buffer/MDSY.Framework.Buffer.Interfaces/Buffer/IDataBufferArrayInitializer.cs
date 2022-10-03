using System;
using System.Collections.Generic;
using System.Linq;

using System.ComponentModel;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which can receive initialization during the creation of a buffer array. 
    /// </summary>
    public interface IDataBufferArrayInitializer
    {
        void InitializeBytes(Byte[] bytes);
        IDataBuffer AsReadOnly();
    }
}
