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

        public string HostGame(string username)
        {
            try
            {
                return "http://www.google.com";
            }
            catch (System.Exception e)
            {
                //TODO Log exception
                throw new HubException("There was an error hosting the game.");
            }
        }
    }
}