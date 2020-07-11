using Octgn.Networking;
using System.Threading.Tasks;

namespace Octgn.Play.Save
{
    public class ReplayClient : IClient
    {
        public IServerCalls Rpc { get; set; }
        public Handler Handler { get; set; }
        public int Muted { get; set; }

        public ReplayClient() {
            Rpc = new ReplayRpc();
            Handler = new Handler();
        }

        public Task Connect() {
            return Task.CompletedTask;
        }

        public void Shutdown() {
        }

        public void AddMessage(byte[] message) {
            Handler.ReceiveMessage(message);
        }
        public class ReplayRpc : BaseBinaryStub
        {
            protected override void Send(byte[] data) {
                // Do nothing.
            }
        }
    }
}
