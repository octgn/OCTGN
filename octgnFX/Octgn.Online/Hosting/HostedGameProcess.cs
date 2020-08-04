using System;
using System.Diagnostics;
using System.IO;
using Octgn.Online.Hosting;
using System.Collections.Generic;
using Octgn.Communication;

namespace Octgn.Library
{
    public class HostedGameProcess
    {
        private static ILogger Log = LoggerFactory.Create(nameof(HostedGameProcess));
        public HostedGame HostedGame { get; }

        public HostedGameProcess(
            HostedGame game,
            bool isDebug,
            bool isLocal,
            int broadcastPort = 21234
            ) {

            HostedGame = game ?? throw new ArgumentNullException(nameof(game));

            if (game.HostUser == null) throw new InvalidOperationException($"{nameof(game)}.{nameof(game.HostUser)} can't be null.");

            var arguments = CreateArguments(game, broadcastPort, isLocal);

            if (!Version.TryParse(game.OctgnVersion, out var sasVersion) && !isLocal) throw new InvalidOperationException($"{nameof(game.OctgnVersion)} '{game.OctgnVersion}' is invalid.");

            var path = GetSASPath(sasVersion, isLocal, isDebug);

            _process = new Process();
            _process.StartInfo.Arguments = string.Join(" ", arguments);
            _process.StartInfo.FileName = path;

            if (isLocal) { //Launch as a subprocess and share our console. Don't do this on the server otherwise it causes issues restarting things individually
                _process.StartInfo.UseShellExecute = false;
                _process.StartInfo.CreateNoWindow = true;
            }
        }

        private readonly Process _process;

        public void Stop() {
            _process.Close();
        }

        public void Start() {
            Log.Info($"Starting {_process.StartInfo.FileName} {_process.StartInfo.Arguments}");
            _process.Start();
            HostedGame.ProcessId = _process.Id;
        }

        public static string GetSASPath(Version sasVersion, bool isLocal, bool isDebug) {
            var path = "";
            // Get file path
            if (isDebug) {
                if (System.Reflection.Assembly.GetEntryAssembly().GetName().Name == "Octgn.Desktop") {
                    path = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\Octgn.Online.StandAloneServer\bin\Debug\Octgn.Online.StandAloneServer.exe");
                } else {
                    path = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Octgn.Online.StandAloneServer\bin\Debug\Octgn.Online.StandAloneServer.exe");
                }
                path = Path.GetFullPath(path);
            } else if (isLocal) {
                path = Directory.GetCurrentDirectory() + "\\Octgn.Online.StandAloneServer.exe";
            } else {

                path = Path.Combine("c:\\Server\\sas", sasVersion + "_Override", "Octgn.Online.StandAloneServer.exe");
                if (!File.Exists(path)) {
                    path = Path.Combine("c:\\Server\\sas", sasVersion.ToString(), "Octgn.Online.StandAloneServer.exe");
                }
            }

            return path;
        }

        public static string[] CreateArguments(HostedGame game, int broadcastPort, bool isLocal) {
            var atemp = new List<string>();
            atemp.Add("-id=" + game.Id.ToString());
            atemp.Add("-name=\"" + game.Name + "\"");
            atemp.Add("-hostuserid=\"" + game.HostUser.Id + "\"");
            atemp.Add("-hostusername=\"" + game.HostUser.DisplayName + "\"");
            atemp.Add("-gamename=\"" + game.GameName + "\"");
            atemp.Add("-gameid=" + game.GameId);
            atemp.Add("-gameversion=" + game.GameVersion);
            atemp.Add("-bind=" + game.HostAddress);
            atemp.Add("-password=" + game.Password);
            atemp.Add("-broadcastport=" + broadcastPort);
            if (!string.IsNullOrWhiteSpace(game.GameIconUrl))
                atemp.Add("-gameiconurl=" + game.GameIconUrl);
            if (!string.IsNullOrWhiteSpace(game.HostUserIconUrl))
                atemp.Add("-usericonurl=" + game.HostUserIconUrl);
            if (game.Spectators)
                atemp.Add("-spectators");
            if (isLocal)
                atemp.Add("-local");

            return atemp.ToArray();
        }
    }
}
