using System.Collections.Generic;
using System.ServiceModel;

namespace MDSY.Framework.Service.Interfaces
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(ADSServiceItemKey))]
    [ServiceKnownType(typeof(ADSServiceItemControl))]
    [ServiceKnownType(typeof(CustomServiceData))]
    [ServiceKnownType(typeof(AterasServiceItemCustomEvent))]
    public interface IADSService
    {
        [OperationContract]
        bool Test();

        [OperationContract]
        void SetValues(List<IAterasServiceItem> controlList);

        [OperationContract]
        List<IAterasServiceItem> GetValues();

        [OperationContract]
        List<IAterasServiceItem> Run(List<IAterasServiceItem> controlList);

        [OperationContract]
        string SessionId();

        [OperationContract]
        void Initialize();

        [OperationContract]
        void Cleanup();

        [OperationContract]
        CustomServiceData GetCustomData();

        [OperationContract]
        void SetCustomData(CustomServiceData customData);
        
        [OperationContract]
        void SetUserID(string userID);
    }
}
