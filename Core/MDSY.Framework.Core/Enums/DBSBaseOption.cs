using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    #region public enum
    public enum DBSBaseOption : int
    {
        /// <summary>
        /// The number to add to '0' character to make the number negative
        /// </summary>
        Negative = 0x40,
        /// <summary>
        /// This is the first character code when the number is negative
        /// </summary>
        NegativeFirstItem = 0x70
    }
    #endregion
}

