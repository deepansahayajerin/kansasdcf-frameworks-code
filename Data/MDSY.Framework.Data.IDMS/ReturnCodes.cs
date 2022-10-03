using System;

using System.Collections;
using System.Text;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.BaseClasses;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer;

namespace MDSY.Framework.Data.IDMS
{
    /// <summary>
    /// Constant values for return codes.
    /// </summary>
    public static class ReturnCodes
    {
        public static readonly int StatusGood = 0000;
        public static readonly int RowNotFound = 0326;
        public static readonly int EndOfList = 0307;
        public static readonly int ErrorLoBound = 1;
        public static readonly int ErrorHiBound = 9999;
        public static readonly int EndOfIndex = 1707;
        public static readonly int IndexRowNotFound = 1726;
        public static readonly int QueueIdNotFound = 4404;
        public static readonly int QueueRecordNotFound = 4405;
        public static readonly int ScratchAreaNotFound = 4303;
        public static readonly int ScratchRecordNotFound = 4305;
        public static readonly int ScratchRecordReplaced = 4317;
    }
}

