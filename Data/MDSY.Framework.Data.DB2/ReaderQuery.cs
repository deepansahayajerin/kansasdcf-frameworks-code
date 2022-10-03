using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace MDSY.Framework.Data.DB2
{
    public class ReaderQuery
    {
        public string ReaderName { get; set; }

        public string QueryText { get; set; }

        public object[] FieldParms { get; set; }

        public DbDataReader DataReader { get; set; }

        public ReaderQuery(string readerName, string queryText, object[] parms)
        {
            ReaderName = readerName;
            QueryText = queryText;
            FieldParms = parms;
        }
     
    }
}
