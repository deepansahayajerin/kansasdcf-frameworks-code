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
    public interface INatService
    {
        [OperationContract]
        bool Test();

        [OperationContract]
        [ServiceKnownType(typeof(NatServiceItemKey))]
        [ServiceKnownType(typeof(NatServiceItemControl))]
        void SetValues(List<IAterasServiceItem> controlList);

        [OperationContract]
        [ServiceKnownType(typeof(NatServiceItemKey))]
        [ServiceKnownType(typeof(NatServiceItemControl))]
        List<IAterasServiceItem> GetValues();

        [OperationContract]
        void Run();

        [OperationContract]
        string SessionId();

        [OperationContract]
        string IsValidUser(string userID, string termID, string password);

        [OperationContract]
        void Cleanup(string loginUserID, string termID);

        [OperationContract]
        void Initialize(string loginUserID, string termID);
    }
}
