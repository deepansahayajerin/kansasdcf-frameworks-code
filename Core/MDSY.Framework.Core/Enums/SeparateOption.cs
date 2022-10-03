using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    [Flags]
    public enum SeparateOption
    {
        Null = 0,
        /// <summary>
        /// This option causes leading blanks which may occur between the
        /// delimiter character and the next non-blank character to be
        /// removed from the target operand. 
        /// </summary>
        LeftJustified = 1,
        /// <summary>
        /// If you specify IGNORE, Natural will ignore it if there are not
        /// enough target operands to receive the source value. 
        /// </summary>
        Ignore = 2,
        /// <summary>
        /// Normally, the delimiter characters themselves are not moved into
        /// the target operands. 
        /// When you specify RETAINED, however, each delimiter (that is,
        /// either default delimiters and blanks, or the delimiter specified
        /// with operand6) will also be placed into a target operand. 
        /// Example:
        /// The following SEPARATE statement would place "150" into #B,
        /// "+" into #C, and "30" into #D: 
        /// ...
        /// MOVE ’150+30’ TO #A
        /// SEPARATE #A INTO #B #C #D WITH RETAINED DELIMITER ’+’
        /// </summary>
        WithRetainedDelimiters = 4,
        /// <summary>
        /// WITH INPUT DELIMITERS indicates that the blank and the default input delimiter character (as
        /// specified with the session parameter ID) is to be used as delimiter character. 
        /// </summary>
        WithInputDelimiters = 8,
        /// <summary>
        /// 
        /// </summary>
        WithAnyDelimiters = 16
    }
}

