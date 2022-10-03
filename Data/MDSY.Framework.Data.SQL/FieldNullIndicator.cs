using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSY.Framework.Data.SQL
{
    public class FieldNullIndicator
    {
        public FieldNullIndicator(IField indField)
        {
            NullFieldInd = indField;
        }

        public FieldNullIndicator(IArrayElementAccessor<IField> indArrayField)
        {
            NullArrayFieldInd = indArrayField;
        }

        public IField NullFieldInd { get; private set; }

        public IArrayElementAccessor<IField> NullArrayFieldInd { get; private set; }

    }
}
