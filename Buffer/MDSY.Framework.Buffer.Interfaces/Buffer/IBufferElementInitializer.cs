using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which can receive initialization during the creation of a buffer element. 
    /// </summary>
    public interface IBufferElementInitializer
    {
        /// <summary>
        /// Gets or sets the byte index of this element within its Parent.
        /// </summary>
        int PositionInParent { get; set; }

        /// <summary>
        /// The buffer object to and from which values are normally stored and retrieved. 
        /// </summary>
        IDataBuffer Buffer { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes occupied in the buffer by this element.
        /// </summary>
        int LengthInBuffer { get; set; }

        /// <summary>
        /// Gets a value indicating whether this element has been declared as 'FILLER'. 
        /// </summary>
        /// <remarks>
        /// If an element is decorated as FILLER, it means that while the element takes up space in the 
        /// buffer, the program will not be referencing the element by name. In COBOL, items marked FILLER
        /// are not given names.
        /// </remarks>
        bool IsFiller { get; set; }

        /// <summary>
        /// Gets or sets the IElementCollection parent of the new buffer element.
        /// </summary>
        IElementCollection Parent { get; set; }

        /// <summary>
        /// Gets or sets the record object which is the new buffer element's root owner.
        /// </summary>
        IRecord Record { get; set; }

        /// <summary>
        /// Gets or sets the name of the new buffer element.
        /// </summary>
        string Name { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this element resides beneath an array.
        /// </summary>
        bool IsInArray { get; set; }
    }
}
