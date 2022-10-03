using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Core
{
    public sealed class StringificationByField : StringificationBase, IStringificationDef
    {
        /// <summary>
        /// Initializes a new instance of the DelimitedByField class.
        /// </summary>
        /// <param name="delimiter">A reference to the IField object, which contains delimiter value.</param>
        /// <param name="texts">An array of the strings.</param>
        public StringificationByField(IField delimiter, params string[] texts)
            : base(delimiter.GetValue<string>(), texts)
        {

        }

    }

}

