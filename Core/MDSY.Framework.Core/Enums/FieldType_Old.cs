using System;
using System.Collections.Generic;

namespace MDSY.Framework.Core
{
    [Obsolete("Replaced by MDSY.Framework.Buffer.Common.FieldType", true)]
    public enum FieldType_Old
    {
        String,
        CompInteger,
        CompShort,
        PackedDecimal,
        CompLong,
        Float,
        Boolean,
        Short,
        Binary,
        UnsignedNumeric,
        SignedNumeric,
        UnsignedDecimal,
        SignedDecimal,
        AttributeControl,

        [Obsolete("'Numeric' will be replaced by 'UsignedNumeric'.", true)]
        Numeric,
        [Obsolete("'Decimal' will be replaced by 'SignedDecimal'.", true)]
        Decimal
    }
}

