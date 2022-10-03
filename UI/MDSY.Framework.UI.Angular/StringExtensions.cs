using System;
using System.Collections.Generic;
using System.Text;

namespace MDSY.Framework.UI.Angular
{
    public static class StringExtensions
    {
        public static string TrimNotNull(this string value)
        {
            if (value == null) return null;
            return value.Trim();
        }
        public static bool isEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
        public static string noNull(this string stringValue, string stringDefault = "")
        {
            if (stringValue.isEmpty()) return stringDefault;
            return stringValue;
        }
        public static string Format(this string format, params object[] parm)
        {
            return string.Format(format, parm);
        }

        public static bool ToBoolean(this string stringValue, bool defaultValue = false)
        {
            if (stringValue.isEmpty()) return false;
            bool result;
            if (!bool.TryParse(stringValue, out result)) return defaultValue;
            return result;
        }
    }
}
