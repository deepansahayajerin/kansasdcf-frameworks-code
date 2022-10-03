using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Service.Interfaces
{
    interface ICustomController
    {

        void UpdateCustomServiceItems(List<IAterasServiceItem> threadData);


        void RetrieveCustomServiceItems(List<IAterasServiceItem> threadData);

    }
}
