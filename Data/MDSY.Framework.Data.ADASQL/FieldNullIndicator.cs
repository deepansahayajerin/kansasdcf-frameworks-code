using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSY.Framework.Data.ADASQL
{
    public class FieldNullIndicator
    {
        public FieldNullIndicator(IField indField)
        {
            NullFieldInd = indField;
        }

        public IField NullFieldInd { get; private set; }

    }
}
