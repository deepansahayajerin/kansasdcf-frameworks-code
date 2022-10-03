using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Interfaces
{
    public interface ICustomSecurity
    {
        /// <summary>
        /// Returns the User ID.
        /// </summary>
        /// <returns></returns>
        bool IsUserAuthenticated();

        string GetUserID();
    }
}
