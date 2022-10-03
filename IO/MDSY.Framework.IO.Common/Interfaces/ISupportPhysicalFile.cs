using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace MDSY.Framework.IO.Common
{
    /// <summary>
    /// Defines an object which provides access to a physical file (rather than virtual datasources, or DB persistence). 
    /// </summary>
    public interface ISupportPhysicalFile
    {
        /// <summary>
        /// Gets or sets the filename of the physical file.
        /// </summary>
        string PhysicalFilename { get; set; }

        /// <summary>
        /// Returns the size, in bytes, of the physical file.
        /// </summary>
        long GetFileSize();

    }
}

