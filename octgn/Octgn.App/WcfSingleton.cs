using System;
using System.Collections.Generic;
using System.Linq;
using Octgn;
using System.Web;
using System.ServiceModel;

namespace Octgn.App
{
    public class WcfSingleton
    {

        private static WcfSingleton instance = null;
        public static WcfSingleton GetInstance()
        {
            if (instance == null)
            {
                instance = new WcfSingleton();
            }
            return (instance);
        }

        ChannelFactory<IBaseInterface> channelFactory;
        IBaseInterface channel;
        private CallBackObject callback = new CallBackObject();


        public WcfSingleton()
        {
            channelFactory =
        new ChannelFactory<IBaseInterface>(new NetNamedPipeBinding(),
          new EndpointAddress(
            "net.pipe://localhost/PipeBase"));
        }

        public IBaseInterface GetChannel()
        {
            if (channel == null)
            {
                channel = channelFactory.CreateChannel();
            }
            
            return (channel);
        }

        class CallBackObject : IBaseInterfaceCallBack
        {
            public void CallBack(string message)
            {
                //blerh
            }
        }
    }
}