using MDSY.Framework.Core;
using System;
using System.Collections.Generic;

namespace MDSY.Framework.Service.Interfaces
{
    [Serializable]
    public class CustomServiceData
    {
        public CustomServiceData()
        {
            CustomDataCollection = new Dictionary<string, object>();
        }
        public IDictionary<string, object> CustomDataCollection { get; set; }

        public IDictionary<string, object> GetCustomCollection()
        {
            GetCustomContextObject().ReadDataCollection(CustomDataCollection);
            return CustomDataCollection;
        }
        public void SetCustomCollection()
        {
            GetCustomContextObject().FillDataCollection(CustomDataCollection);
        }
        private static ICustomData GetCustomContextObject()
        {
            return InversionContainer.GetImplementingObject<ICustomData>();
        }
    }

}
