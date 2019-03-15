using Octgn.Networking;
using System;
using System.Threading.Tasks;

namespace Octgn.Play.Save
{
    public class Replay
    {
        public string Name { get; set; }

        public Guid GameId { get; set; }

        public Replay() {
        }
    }
    public class ReplayEventContext
    {
        public GameEngine Engine { get; set; }

        public ReplayClient Client { get; set; }

        public double ReplaySpeed { get; set; }
    }
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
    }

    public class ReplayRpc : BaseBinaryStub
    {
        protected override void Send(byte[] data) {
            // nothing 2 do
        }
    }
}
