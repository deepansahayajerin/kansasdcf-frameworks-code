using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Data.IDMS
{
    public interface IIdentityColumnInfo
    {
        string GetName(string tableName, string columnName);

        string GetValue(string tableName);

        string GetInsertedValue(string tableName);

    }
}
