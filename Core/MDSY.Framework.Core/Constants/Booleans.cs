using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Core.Constants
{
    /// <summary>
    /// Contains definitions of constans that are related to Boolean type.
    /// </summary>
    public static class Booleans
    {
        /// <summary>
        /// 1 integer constant, which corresponds to True boolean value.
        /// </summary>
        public const int TrueNum = 1;

        /// <summary>
        /// 0 integer constant, which corresponds to False boolean value.
        /// </summary>
        public const int FalseNum = 0;

        /// <summary>
        /// "1" string constant, which corresponds to True boolean value.
        /// </summary>
        public const string TrueNumString = "1";

        /// <summary>
        /// "0" string constant, which corresponds to False boolean value.
        /// </summary>
        public const string FalseNumString = "0";

        /// <summary>
        /// Boolean-specific string values.
        /// </summary>
        public static class Strings
        {
            /// <summary>
            /// "T" string constant.
            /// </summary>
            public static string Char1True = "T";

            /// <summary>
            /// "TR" string constant.
            /// </summary>
            public static string Char2True = "TR";

            /// <summary>
            /// "TRU" string constant.
            /// </summary>
            public static string Char3True = "TRU";

            /// <summary>
            /// "TRUE" string constant.
            /// </summary>
            public static string Char4True = "TRUE";

            /// <summary>
            /// "TRUE " string constant.
            /// </summary>
            public static string Char5True = "TRUE ";

            /// <summary>
            /// "F" string constant.
            /// </summary>
            public static string Char1False = "F";

            /// <summary>
            /// "FAL" string constant.
            /// </summary>
            public static string Char2False = "FA";

            /// <summary>
            /// "FAL" string constant
            /// </summary>
            public static string Char3False = "FAL";

            /// <summary>
            /// "FALS" string constant.
            /// </summary>
            public static string Char4False = "FALS";

            /// <summary>
            /// "FALSE" string constant.
            /// </summary>
            public static string Char5False = "FALSE";
        }

        /// <summary>
        /// An array, which contains all string constants that represent True boolean value.
        /// </summary>
        public static string[] TrueStrings = new string[6] { Booleans.TrueNumString, Strings.Char1True, Strings.Char2True, Strings.Char3True, Strings.Char4True, Strings.Char5True };
        
        /// <summary>
        /// An array, which contains all string constants that represent False boolean value.
        /// </summary>
        public static string[] FalseStrings = new string[6] { Booleans.FalseNumString, Strings.Char1False, Strings.Char2False, Strings.Char3False, Strings.Char4False, Strings.Char5False };

    }
}
