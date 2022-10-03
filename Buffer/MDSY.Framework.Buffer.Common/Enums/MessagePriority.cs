using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Specifies priority of the logging messages.
    /// </summary>
    public enum MessagePriority
    {
        /// <summary>
        /// Indicates highest message priority. Numeric value is 1.
        /// </summary>
        Highest = 1,

        /// <summary>
        /// Indicates higher message priority. Numeric value is 2.
        /// </summary>
        Higher = 2,

        /// <summary>
        /// Indicates high message priority. Numeric value is 3.
        /// </summary>
        High = 3,

        /// <summary>
        /// Indicates medium message priority. Numeric value is 4.
        /// </summary>
        Medium = 4,

        /// <summary>
        /// Indicates low message priority. Numeric value is 5.
        /// </summary>
        Low = 5,

        /// <summary>
        /// Indicates lower message priority. Numeric value is 6.
        /// </summary>
        Lower = 6,

        /// <summary>
        /// Indicates lowest message priority. Numeric value is 7.
        /// </summary>
        Lowest = 7
    }
}
