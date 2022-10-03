using MDSY.Framework.Interfaces;
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Data.IDMS
{
    public class ConnectionStringInfo: IConnectionString
    {
        public string GetConnectionString(string connectionStringKey)
        {
            return ConfigSettings.GetConnectionStrings(connectionStringKey, "connectionString");
        }
    }
}
