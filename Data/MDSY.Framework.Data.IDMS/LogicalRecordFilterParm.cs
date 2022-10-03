using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Data.IDMS
{
    public class LogicalRecordFilterParm
    {
        public string ParmOperator
        {
            get;
            set;
        }
        public string RecordName
        {
            get;
            set;
        }
        public string FieldName
        {
            get;
            set;
        }
        public string FieldValue
        {
            get;
            set;
        }
        public string Operator
        {
            get;
            set;
        }
    }
}
