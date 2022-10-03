using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer;


namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Provide object for passing strings
    /// </summary>
    public class StringParm
    {

        public StringParm(string stringValue)
        {
            Value = stringValue;
        }
        public string Value { get; set; }

    }


}
