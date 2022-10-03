using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Control.CICS
{
    public class DataSpool
    {
        public List<string> DataList =  new List<string> { };
        public string UserID { get; set; }
        public string SpoolNode { get; set; }
        public string SpoolClass { get; set; }

        public DataSpool(string userID, string spoolNode, string spoolClass)
        {
            UserID = userID;
            SpoolNode = spoolNode;
            SpoolClass = spoolClass;
        }


        public void AddDataToSpool(string spoolData)
        {
            DataList.Add(spoolData);
        }

    }
}
