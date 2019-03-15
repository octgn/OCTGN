using Octgn.Networking;
using System;
using System.IO;
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
            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);

            var length = reader.ReadInt32();
            var muted = reader.ReadInt32();

            byte method = reader.ReadByte();
            switch (method) {
                case 3: {
                    string nick = reader.ReadString();
                    string userId = reader.ReadString();
                    ulong pkey = reader.ReadUInt64();
                    string client = reader.ReadString();
                    Version clientVer = new Version(reader.ReadString());
                    Version octgnVer = new Version(reader.ReadString());
                    Guid lGameId = new Guid(reader.ReadBytes(16));
                    Version gameVer = new Version(reader.ReadString());
                    string password = reader.ReadString();
                    bool playerIsSpectator = reader.ReadBoolean();

                    new ReplayServerRpc().Welcome(byte.MaxValue - 5, Program.GameEngine.SessionId, "boob", false);
                    new ReplayServerRpc().PlayerSettings(byte.MaxValue - 5, false, true);

                    break;
                }
            }
        }
    }

    public class ReplayServerRpc : Octgn.Server.BaseBinaryStub
    {
        public ReplayServerRpc() : base(null) {

        }

        protected override void Send(byte[] data) {
            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);

            reader.ReadInt32();
            reader.ReadInt32();

            byte method = reader.ReadByte();

            var client = (ReplayClient)Program.Client;

            client.AddMessage(data);
        }
    }
}
