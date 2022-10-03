using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Data.IDMS
{
    public class SequenceIdentityColumnInfo: IIdentityColumnInfo
    {
        //Get the Identity Column name
        public string GetName(string tableName, string columnName)
        {
            return columnName;
        }

        // Get next Identity Value from Sequence object
        public string GetValue(string tableName)
        {
            return string.Concat("NEXT VALUE FOR E", tableName.Substring(1));
        }

        //Get Last Sequence Number used
        public string GetInsertedValue(string tableName)
        {
            return string.Concat("SELECT PREVIOUS VALUE FOR E", tableName.Substring(1), " FROM SYSIBM.SYSDUMMY1" );
        }


    }
}
