using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Encapsulates the information necessary to implement buffer address routines.
    /// </summary>
    [InjectionInterface]
    public interface IBufferAddress
    {
        /// <summary>
        /// Gets or sets the name of the element whose Buffer and PositionInBuffer 
        /// will be used as the target of the buffer address redirection.
        /// </summary>
        /// <remarks>If ElementName is empty, the address is the record itself, 
        /// i.e. PositionInBuffer == 0, unless OptionalBufferStartIndex is set.</remarks>
        string ElementName { get; set; }

        /// <summary>
        /// Gets or sets the key value of the Record (in BufferServices.Records)
        /// whose buffer will be the target of the buffer address redirection.
        /// </summary>
        int RecordKey { get; set; }

        /// <summary>
        /// Gets or sets an optional PositionInBuffer in the case where ElementName
        /// is empty.
        /// </summary>
        int OptionalBufferStartIndex { get; set; }
    }
}
