using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace MDSY.Framework.Service.Interfaces
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(CICSServiceItemKey))]
    [ServiceKnownType(typeof(CICSServiceItemControl))]
    [ServiceKnownType(typeof(CustomServiceData))]
    [ServiceKnownType(typeof(AterasServiceItemCustomEvent))]
    public interface ICICSService
    {
        [OperationContract]
        bool Test();

        [OperationContract]
        [ServiceKnownType(typeof(CICSServiceItemKey))]
        [ServiceKnownType(typeof(CICSServiceItemControl))]
        void SetValues(List<IAterasServiceItem> controlList);

        [OperationContract]
        [ServiceKnownType(typeof(CICSServiceItemKey))]
        [ServiceKnownType(typeof(CICSServiceItemControl))]
        List<IAterasServiceItem> GetValues();

        [OperationContract]
        List<IAterasServiceItem> Run(List<IAterasServiceItem> controlList);

        [OperationContract]
        void Run();

        [OperationContract]
        string SessionId();

        [OperationContract]
        void Initialize(string LoginUserID, string TermID);

        [OperationContract]
        CustomServiceData GetCustomData();

        [OperationContract]
        void SetCustomData(CustomServiceData customData);

        [OperationContract]
        void Cleanup();
    }
}
