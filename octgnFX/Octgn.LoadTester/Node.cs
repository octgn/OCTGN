using Octgn.Communication;
using Octgn.DataNew.Entities;
using Octgn.Online;
using Octgn.Online.Hosting;
using Octgn.Site.Api;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Octgn.LoadTester
{
    public class Node
    {
        private readonly Library.Communication.Client _client;
        private readonly Version _version;
        private readonly DefaultHandshaker _handshaker;

        public Node() {
            _version = typeof(Node).Assembly.GetName().Version;
            _handshaker = new DefaultHandshaker();

            var connectionCreator = new TcpConnectionCreator(_handshaker);
            var config = new LibraryCommunicationClientConfig(connectionCreator);

            _client = new Library.Communication.Client(config, _version);
        }

        public async Task Run(CancellationToken cancellationToken) {
            await ConfigureClient();

            var game = GenerateHostedGame();

            while (!cancellationToken.IsCancellationRequested) {
                try {
                    var result = await _client.HostGame(game);
                    Console.Write(".");
                } catch (Exception ex) {
                    Console.Error.WriteLine(ex);
                }

                try {
                    await Task.Delay(5000, cancellationToken);
                } catch (OperationCanceledException) { }
            }
        }

        private async Task ConfigureClient() {
            var client = new ApiClient();

            var deviceId = Guid.NewGuid().ToString();

            var result = await client.CreateSession("username", "password", deviceId);

            if (result.Result.Type != LoginResultType.Ok) {
                throw new InvalidOperationException($"Login failed: {result.Result}");
            }

            _client.ConfigureSession(result.SessionKey, new User(result.UserId, "username"), deviceId);

            await _client.Connect(default);
        }

        protected virtual HostedGame GenerateHostedGame() {
            var game = LoadGame();
            var name = "zz__TestGame";
            var password = "asdfhoe";

            var hostedGame = new HostedGame {
                GameId = game.Id,
                GameVersion = game.Version.ToString(),
                Name = name,
                GameName = game.Name,
                GameIconUrl = game.IconUrl,
                Password = password,
                HasPassword = !string.IsNullOrWhiteSpace(password),
                OctgnVersion = _version.ToString(),
                Spectators = false
            };

            return hostedGame;
        }

        protected virtual Game LoadGame() {
            return new Game() {
                Id = Guid.Parse("2f3dbb9b-67c4-41c9-b648-047a2fa4fc56"),
                Version = Version.Parse("1.0.0.11"),
                Name = "Chess",
                IconUrl = "https://raw.github.com/kellyelton/octgn-game-chess/master/chess/Images/icon.jpg",
            };
        }
    }
}
