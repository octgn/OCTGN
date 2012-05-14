using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Octgn
{

    public interface IBaseInterfaceCallBack
    {
        [OperationContract]
        void CallBack(string message);
    }

    //[ServiceContract(CallbackContract = typeof(IBaseInterfaceCallBack))]
    [ServiceContract]
    public interface IBaseInterface
    {
        [OperationContract]
        string GetMessage();

        [OperationContract]
        void SetMessage(string message);

        [OperationContract]
        void CreateRandomMessage();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        UseSynchronizationContext=false,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class BaseClass : IBaseInterface
    {
        private string msg = "testing through wcf";

        public string GetMessage()
        {
            return (msg);
        }

        public void SetMessage(string message)
        {
            msg = message;
        }

        public void CreateRandomMessage()
        {
            Random r = new Random();
            msg = string.Format("testing through wcf: {0}", r.Next(int.MinValue, int.MaxValue));
        }
    }
}
