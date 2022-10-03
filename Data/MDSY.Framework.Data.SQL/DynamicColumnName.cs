using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Data.SQL
{
    public class DynamicColumnName
    {
        public string OldName { get; set; }
        public string NewName { get; set; }
        public bool IsDuplicateColumnName { get; set; }
    }
}
