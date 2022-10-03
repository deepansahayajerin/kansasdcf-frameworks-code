using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSY.Framework.Data.SQL
{
    public class FieldTimeStamp
    {
        public FieldTimeStamp(IField indField)
        {
            TimeStampField = indField;
        }

        public FieldTimeStamp(IArrayElementAccessor<IField> indArrayField)
        {
            TimeStampFieldArray = indArrayField;
        }

        public IField TimeStampField { get; private set; }

        public IArrayElementAccessor<IField> TimeStampFieldArray { get; private set; }

    }
}
