using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Octgn
{
    [ServiceContract]
    public interface IBaseInterface
    {
        [OperationContract]
        string GetMessage();

        [OperationContract]
        void SetMessage(string message);
    }

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
    }
}
