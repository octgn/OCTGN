using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net;
using Octgn.Library;

namespace Skylabs.Lobby
{
    using System.Collections.Generic;

    public class HostedGame : IEquatable<HostedGame>
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        ///   Host a game.
        /// </summary>
        /// <param name="port"> Port we are hosting on. </param>
        /// <param name="gameguid"> GUID of the game </param>
        /// <param name="gameversion"> Version of the game </param>
        /// <param name="name"> Name of the room </param>
        /// <param name="password"> Password for the game </param>
        /// <param name="hoster"> User hosting the game </param>
        public HostedGame(int port, Guid gameguid, Version gameversion, string gameName, string name, string password
            , User hoster, bool localGame = false, bool isOnServer = false
            , Guid id = new Guid(),
            int broadcastPort = 21234)
        {
            GameLog = "";
            GameGuid = gameguid;
            GameVersion = gameversion;
            Name = name;
            Password = password;
            Hoster = hoster;
            Status = EHostedGame.StoppedHosting;
            Port = port;
            TimeStarted = new DateTime(0);
            LocalGame = localGame;
            GameName = gameName;

            var atemp = new List<string>();
            this.Id = id;
            atemp.Add("-id=" + Id.ToString());
            atemp.Add("-name=\"" + name + "\"");
            atemp.Add("-hostusername=\"" + hoster.UserName + "\"");
            atemp.Add("-gamename=\"" + gameName + "\"");
            atemp.Add("-gameid=" + gameguid);
            atemp.Add("-gameversion=" + gameversion);
            atemp.Add("-bind=" + "0.0.0.0:" + port.ToString());
            atemp.Add("-password=" + password);
            atemp.Add("-broadcastport=" + broadcastPort);
            if (localGame)
                atemp.Add("-local");

            // ReSharper disable HeuristicUnreachableCode
            var path = "";
            // Get file path
            if (X.Instance.Debug || X.Instance.TestServer)
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Octgn.Online.StandAloneServer\bin\Debug\Octgn.Online.StandAloneServer.exe");
                path = Path.GetFullPath(path);
            }
            else
            {
                if (isOnServer)
                {
                    var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "sas"));
                    var newLocation = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "OCTGN", "SAS", Guid.NewGuid().ToString()));
                    Directory.CreateDirectory(newLocation.FullName);
                    foreach (var dirPath in Directory.GetDirectories(di.FullName, "*", SearchOption.AllDirectories))
                    {
                        var cpy = dirPath.Replace(di.FullName, newLocation.FullName);
                        if (!Directory.Exists(cpy))
                            Directory.CreateDirectory(cpy);
                    }

                    //Copy all the files
                    foreach (string newPath in Directory.GetFiles(di.FullName, "*.*", SearchOption.AllDirectories))
                        File.Copy(newPath, newPath.Replace(di.FullName, newLocation.FullName), true);
                    path = Path.Combine(newLocation.FullName, "Octgn.Online.StandAloneServer.exe");
                }
                else
                {
                    path = Directory.GetCurrentDirectory() + "\\Octgn.Online.StandAloneServer.exe";
                }
            }
            // ReSharper restore HeuristicUnreachableCode


            StandAloneApp = new Process();
            StandAloneApp.StartInfo.Arguments = String.Join(" ", atemp);
            StandAloneApp.StartInfo.FileName = path;

            if (X.Instance.Debug || X.Instance.TestServer)
            {
                StandAloneApp.StartInfo.UseShellExecute = true;
                StandAloneApp.StartInfo.CreateNoWindow = true;
            }
            else
            {
                StandAloneApp.StartInfo.CreateNoWindow = true;
                StandAloneApp.StartInfo.UseShellExecute = false;
            }

            StandAloneApp.Exited += StandAloneAppExited;
            StandAloneApp.EnableRaisingEvents = true;
        }

        public Guid Id { get; private set; }

        /// <summary>
        ///   Games GUID. Based on the GameDefinitionFiles.
        /// </summary>
        public Guid GameGuid { get; private set; }

        /// <summary>
        /// Is this game a Local Game(not hosted by a LobbyServer)?
        /// </summary>
        public bool LocalGame { get; private set; }

        /// <summary>
        ///   Games Version. Based on the GameDefinitionFiles.
        /// </summary>
        public Version GameVersion { get; private set; }

        /// <summary>
        ///   Port we're hosting on.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        ///   Name of the hosted game.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Name of the actual game
        /// </summary>
        public string GameName { get; private set; }

        /// <summary>
        ///   Password for the hosted game.
        /// </summary>
        public String Password { get; private set; }

        /// <summary>
        ///   The process of the StandAloneServer that hosts the game.
        /// </summary>
        public Process StandAloneApp { get; set; }

        /// <summary>
        ///   Hoster of this crazy game.
        /// </summary>
        public User Hoster { get; private set; }

        /// <summary>
        ///   The status of the hosted game.
        /// </summary>
        public EHostedGame Status { get; set; }

        public DateTime TimeStarted { get; private set; }

        #region IEquatable<HostedGame> Members

        /// <summary>
        ///   Just an equality verifier. All hosted games are uniquly identified by there port.
        /// </summary>
        /// <param name="other"> Hosted game to check against. </param>
        /// <returns> </returns>
        public bool Equals(HostedGame other)
        {
            return other.Port.Equals(Port);
        }

        #endregion

        public event EventHandler HostedGameDone;

        public string GameLog { get; private set; }

        public void Stop()
        {
            try
            {
                StandAloneApp.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (Debugger.IsAttached) Debugger.Break();
            }
        }

        public bool StartProcess(bool throwException = false)
        {
            Status = EHostedGame.StoppedHosting;
            try
            {
                StandAloneApp.Start();
                Status = EHostedGame.StartedHosting;
                TimeStarted = new DateTime(DateTime.Now.ToUniversalTime().Ticks);
                return true;
            }
            catch (Exception e)
            {
                if (throwException)
                    throw;
                Console.WriteLine("");
                Console.WriteLine(StandAloneApp.StartInfo.FileName);
                Console.WriteLine(StandAloneApp.StartInfo.Arguments);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            return false;
        }

        /// <summary>
        ///   This happens when the Octgn.Server in StandAloneServer stops. This means eather all of the users disconnected, or it crashed. Eaither way, its unjoinable at this point.
        /// </summary>
        /// <param name="sender"> Who knows </param>
        /// <param name="e"> Jesus </param>
        private void StandAloneAppExited(object sender, EventArgs e)
        {
            StandAloneApp.Exited -= StandAloneAppExited;
            if (HostedGameDone != null)
                HostedGameDone.Invoke(this, e);
            Console.WriteLine("Game Log[{0}]{1}{2}End Game Log[{0}]", Port, Environment.NewLine, GameLog);
        }
    }
}