using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace MDSY.Framework.Data.ADASQL
{
    public class ReaderQuery
    {
        public string ReaderName { get; set; }

        public string QueryText { get; set; }

        public int RESPONSE_CODE { get; set; }

        public int ISN_QUANTITY { get; set; }

        public int ISN { get; set; }

        public int QUANTITY { get; set; }

        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public object[] FieldParms { get; set; }

        public DbDataReader DataReader { get; set; }

        public ReaderQuery(string readerName, string queryText, string options, object[] parms)
        {
            ReaderName = readerName;
            QueryText = queryText;
            FieldParms = parms;
            if (options != null)
            {
                if (options.Trim().StartsWith("PREFIX"))
                {
                    Prefix = options.Replace("PREFIX", "").Replace("=","").Trim();
                }
                else if (options.Trim().StartsWith("SUFFIX"))
                {
                    Prefix = options.Replace("SUFFIX", "").Replace("=", "").Trim();
                }
            }
        }
     
    }
}
