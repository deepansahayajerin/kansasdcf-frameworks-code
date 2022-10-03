//using System;
//using System.Collections.Generic;
//using System.Linq;
//CHADusing Unity.Attributes;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IBufferAddress.
    /// </summary>
    [InjectionImplementer(typeof(IBufferAddress))]
    internal sealed class BufferAddress : IBufferAddress
    {
        /// <summary>
        /// Gets or sets the name of the element whose Buffer and PositionInBuffer 
        /// will be used as the target of the buffer address redirection.
        /// </summary>
        /// <remarks>If ElementName is empty, the address is the record itself, 
        /// i.e. PositionInBuffer == 0, unless OptionalBufferStartIndex is set.</remarks>
        public string ElementName { get; set; }

        /// <summary>
        /// Gets or sets the key value of the Record (in BufferServices.Records)
        /// whose buffer will be the target of the buffer address redirection.
        /// </summary>
        public int RecordKey { get; set; }

        /// <summary>
        /// Gets or sets an optional PositionInBuffer in the case where ElementName
        /// is empty.
        /// </summary>
        public int OptionalBufferStartIndex { get; set; }
    }
}
