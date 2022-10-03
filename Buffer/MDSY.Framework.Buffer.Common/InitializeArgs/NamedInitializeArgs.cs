using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// InitializeArgs for classes which implement INamed.
    /// </summary>
    [Obsolete("Use NamedBufferElementInitArgs instead", true)]
    public class NamedInitializeArgs : InitializeArgs
    {

        #region constructors
        /// <summary>
        /// Initializes a new instance of the NamedInitializeArgs class.
        /// </summary>
        public NamedInitializeArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the NamedInitializeArgs class.
        /// </summary>
        /// <param name="name">Initialization argument name.</param>
        public NamedInitializeArgs(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");
            Name = name;
        }

        #endregion

        /// <summary>
        /// Gets or sets the Name value.
        /// </summary>
        public string Name { get; set; }
    }
}

