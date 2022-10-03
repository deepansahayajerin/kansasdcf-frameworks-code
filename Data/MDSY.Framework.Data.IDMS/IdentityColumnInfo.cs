using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Data.IDMS
{
    // This Implmentation assumes that table IDs will be set as Identity Columns and automatically incremented on Inserts.
    public class IdentityColumnInfo: IIdentityColumnInfo
    {
        public string GetName(string tableName, string columnName)
        {
            return string.Empty;
        }

        public string GetValue(string tableName)
        {
            return string.Empty;
        }

        public string GetInsertedValue(string tableName)
        {
            return string.Empty;
        }


    }
}
