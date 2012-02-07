using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Skylabs.Lobby;

namespace Skylabs.LobbyServer
{
    public class HostedGame : IEquatable<HostedGame>
    {
        /// <summary>
        ///   Host a game.
        /// </summary>
        /// <param name="port"> Port we are hosting on. </param>
        /// <param name="gameguid"> GUID of the game </param>
        /// <param name="gameversion"> Version of the game </param>
        /// <param name="name"> Name of the room </param>
        /// <param name="password"> Password for the game </param>
        /// <param name="hoster"> User hosting the game </param>
        public HostedGame(int port, Guid gameguid, Version gameversion, string name, string password, User hoster)
        {
            GameGuid = gameguid;
            GameVersion = gameversion;
            Name = name;
            Password = password;
            Hoster = hoster;
            Status = Lobby.HostedGame.eHostedGame.StoppedHosting;
            Port = port;
            TimeStarted = new DateTime(0);
            StandAloneApp = new Process();
#if(DEBUG || TestServer)
            StandAloneApp.StartInfo.FileName = Directory.GetCurrentDirectory() + "/Octgn.StandAloneServer.exe";
            StandAloneApp.StartInfo.Arguments = "-g=" + GameGuid + " -v=" + GameVersion + " -p=" +
                                                Port.ToString(CultureInfo.InvariantCulture);
#else
            StandAloneApp.StartInfo.FileName = "/opt/mono-2.10/bin/mono";
            StandAloneApp.StartInfo.Arguments = Directory.GetCurrentDirectory() + "/Octgn.StandAloneServer.exe -g=" + GameGuid + " -v=" + GameVersion + " -p=" + Port.ToString(CultureInfo.InvariantCulture);
                    
#endif
            StandAloneApp.Exited += StandAloneApp_Exited;
            StandAloneApp.EnableRaisingEvents = true;
        }

        /// <summary>
        ///   Games GUID. Based on the GameDefinitionFiles.
        /// </summary>
        public Guid GameGuid { get; private set; }

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
        public Lobby.HostedGame.eHostedGame Status { get; set; }

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

        public void Stop()
        {
            try
            {
                StandAloneApp.Close();
            }
            catch
            {
            }
        }

        public bool StartProcess()
        {
            Status = Lobby.HostedGame.eHostedGame.StoppedHosting;
            try
            {
                StandAloneApp.Start();
                Status = Lobby.HostedGame.eHostedGame.StartedHosting;
                TimeStarted = new DateTime(DateTime.Now.ToUniversalTime().Ticks);
                return true;
            }
            catch (Exception e)
            {
                //TODO Need some sort of proper error handling here.
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
        private void StandAloneApp_Exited(object sender, EventArgs e)
        {
            if (HostedGameDone != null)
                HostedGameDone(this, e);
            Console.WriteLine("Ending Game[" + Port + "]");
        }
    }
}