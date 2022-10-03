using System;
using System.Text;

namespace MDSY.Framework.Interfaces
{
    /// <summary>
    /// Returns connection string.
    /// </summary>
    public interface IConnectionString
    {
        string GetConnectionString(string connectionStringKey);
    }
}
