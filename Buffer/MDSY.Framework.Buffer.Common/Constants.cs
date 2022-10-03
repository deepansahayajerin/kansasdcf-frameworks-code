using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{



    /// <summary>
    /// Constant values common to multiple projects. 
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Names of config settings values. 
        /// </summary>
        public static class AppSettings
        {
            /// <summary>
            /// "IsZeroBasedArrays" string constant.
            /// </summary>
            public static readonly string IsZeroBasedArrays = "IsZeroBasedArrays";
        }

        /// <summary>
        /// Possible string representations of boolean values. 
        /// </summary>
        public static class BooleanStrings
        {
            /// <summary>
            /// A collection of strings that represent boolean true value.
            /// </summary>
            public static string[] TrueStrings = new string[5] { "true", "True", "TRUE", "T", "t" };

            /// <summary>
            /// A collection of strings that represent boolean false value.
            /// </summary>
            public static string[] FalseStrings = new string[5] { "false", "False", "FALSE", "F", "f" };
        }

        /// <summary>
        /// Constant default values. 
        /// </summary>
        public static class Defaults
        {
            /// <summary>
            /// Zero default value.
            /// </summary>
            public static readonly int LengthInBuffer = 0;

            /// <summary>
            /// Zero default value.
            /// </summary>
            public static readonly int PositionInParent = 0;

            /// <summary>
            /// Zero default value.
            /// </summary>
            public static readonly int DecimalDigits = 0;

            /// <summary>
            /// Zero default value.
            /// </summary>
            public static readonly int ArrayElementIndex = 0;

            /// <summary>
            /// Zero default value.
            /// </summary>
            public static readonly int ArrayElementLength = 0;

            /// <summary>
            /// Default value 2.
            /// </summary>
            public const int CompShortByteCount = 2;

            /// <summary>
            /// Default value 4.
            /// </summary>
            public const int CompIntByteCount = 4;

            /// <summary>
            /// Default value 8.
            /// </summary>
            public const int CompLongByteCount = 8;

            /// <summary>
            /// Default value 'false'.
            /// </summary>
            public static readonly bool IsInArray = false;

            /// <summary>
            /// Default value 'false'.
            /// </summary>
            public static readonly bool IsRedefine = false;

            /// <summary>
            /// Default value 'false'.
            /// </summary>
            public static readonly bool IsFiller = false;

        }

        /// <summary>
        /// Defines mapping registration names.
        /// </summary>
        public static class TypeMappingRegistrationNames
        {
            #region mapping of IValue types

            /// <summary>
            /// "Redefine" string constant.
            /// </summary>
            public static readonly string Redefine = "Redefine";

            /// <summary>
            /// "ForArray" string constant.
            /// </summary>
            public static readonly string ForArray = "ForArray";

            /// <summary>
            /// "ZeroBasedIdx" string constant.
            /// </summary>
            public static readonly string ZeroBasedIdx = "ZeroBasedIdx";

            /// <summary>
            /// "OneBasedIdx" string constant.
            /// </summary>
            public static readonly string OneBasedIdx = "OneBasedIdx";

            /// <summary>
            /// "InitialDataBuffer" string constant.
            /// </summary>
            public static readonly string InitialDataBuffer = "InitialDataBuffer";

            /// <summary>
            /// "PipelineDataBuffer" string constant.
            /// </summary>
            public static readonly string PipelineDataBuffer = "PipelineDataBuffer";


            #endregion
        }

    }
}
