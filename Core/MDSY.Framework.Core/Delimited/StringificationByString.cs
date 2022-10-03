using MDSY.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Core
{
    public sealed class StringificationByString : StringificationBase, IStringificationDef
    {
        /// <summary>
        /// Initializes a new instance of the DelimitedByString class.
        /// </summary>
        /// <param name="delimiter">Delimiter string value.</param>
        /// <param name="texts">An array of strings.</param>
        public StringificationByString(string delimiter, params string[] texts)
            : base(delimiter, texts)
        {

        }
    }

}

