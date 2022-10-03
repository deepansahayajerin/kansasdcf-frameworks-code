using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.IO.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDSY.Framework.IO.Common
{
    public class SortParameter
    {
        private readonly IField field;
        public int Offset { get; set; }
        public int Length { get; set; }
        public SortDirection SortDirection { get; set; }
        public SortKeyFormat Format { get; set; }

        public SortParameter()
        {
            SortDirection = SortDirection.Ascending;
            Format = SortKeyFormat.Character;
        }

        public SortParameter(IField sortField, SortDirection sortDir)
        {
            field = sortField;
            Length = field.LengthInBuffer;
            Offset = sortField.PositionInParent;
            SortDirection = sortDir;
            switch (field.FieldType)
            {
                case Buffer.Common.FieldType.PackedDecimal:
                    Format = SortKeyFormat.PackedDecimal; break;
                case Buffer.Common.FieldType.SignedDecimal:
                case Buffer.Common.FieldType.SignedNumeric:
                    Format = SortKeyFormat.ZonedDecimal; break;
                default: 
                    Format = SortKeyFormat.Character; break;
            }
        }
    }

    public enum SortKeyFormat
    {
        Character,
        Binary,
        PackedDecimal,
        ZonedDecimal
    }
}
