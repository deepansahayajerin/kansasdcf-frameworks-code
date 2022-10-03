using System;
using System.Collections.Generic;
using System.Text;

namespace MDSY.Framework.UI.Angular
{
    public class ErrorException : Exception
    {
        public ErrorException(string message) : base(message) { }
    }
}
