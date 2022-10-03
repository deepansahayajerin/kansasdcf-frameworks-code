using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Unity;
using System.Globalization;
using Unity;
using MDSY.Framework.Buffer.Interfaces;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
//CHADusing Unity.Attributes;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Default IDataBufferAccess implementation. Interfaces between an element which persists to an 
    /// IDataBuffer (such as IFieldValue), and the IDataBuffer object.
    /// </summary>
    [InjectionImplementer(typeof(IDataBufferAccess))]
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed class DataBufferAccess : IDataBufferAccess
    {
        public IDataBuffer Buffer { get; set; }

        public int ElementStartIndex { get; set; }

        public int ElementLength { get; set; }

        public void SetElementBytes(byte[] value)
        {
            throw new NotImplementedException();
        }

        public byte[] GetElementBytes()
        {
            throw new NotImplementedException();
        }
    }
}
