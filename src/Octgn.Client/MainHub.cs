using Microsoft.AspNet.SignalR;

namespace Octgn.Client
{
    public class MainHub : Hub
    {
        public static IHubContext Instance
        {
            get { return GlobalHost.ConnectionManager.GetHubContext<MainHub>(); }
        }

        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }

        public void PingClient()
        {
            System.Console.Beep();
        }
    }
}