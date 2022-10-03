using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.IO.Common
{
    [CodeVersion(1)]
    public static class TypeExtension
    {
        public static int? GetCodeVersion(this Type instance)
        {
            int? codeVersionNumber = null;

            if (instance != null)
            {
                List<object> attributes = new List<object>(instance.GetCustomAttributes(false));

                var codeVersion = attributes.Find(item => item.GetType() == typeof(CodeVersion));

                if (codeVersion != null)
                {
                    codeVersionNumber = ((CodeVersion)codeVersion).Version;
                }
            }

            return codeVersionNumber; 
        }
    }
}
