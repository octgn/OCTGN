using System.Threading.Tasks;
using Octgn.Communication;
using Octgn.Communication.Modules.SubscriptionModule;

namespace Octgn.Online.Hosting
{
    public class ServerHostingModule : IServerModule
    {
        private Server _server;
        private object octgnChatDataProvider;

        public ServerHostingModule(Server server, IDataProvider octgnChatDataProvider) {
            _server = server;
            this.octgnChatDataProvider = octgnChatDataProvider;
        }

        public Task HandleRequest(object sender, HandleRequestEventArgs args) {
            throw new System.NotImplementedException();
        }

        public Task UserStatucChanged(object sender, UserStatusChangedEventArgs e) {
            throw new System.NotImplementedException();
        }
    }
}