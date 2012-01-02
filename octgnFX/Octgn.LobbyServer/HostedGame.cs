using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Skylabs.Net;
using Skylabs.Lobby;

namespace Skylabs.LobbyServer
{
    public class HostedGame : IEquatable<HostedGame>
    {
        /// <summary>
        /// Games GUID. Based on the GameDefinitionFiles.
        /// </summary>
        public Guid GameGuid { get; private set; }
        /// <summary>
        /// Games Version. Based on the GameDefinitionFiles.
        /// </summary>
        public Version GameVersion { get; private set; }
        /// <summary>
        /// Port we're hosting on.
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// Name of the hosted game.
        /// </summary>
        public String Name { get; private set; }
        /// <summary>
        /// Password for the hosted game.
        /// </summary>
        public String Password { get; private set; }
        /// <summary>
        /// The process of the StandAloneServer that hosts the game.
        /// </summary>
        public Process StandAloneApp { get; set; }
        /// <summary>
        /// Is this game running?
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// Server.cs instance.
        /// </summary>
        public Server Server { get; set; }
        /// <summary>
        /// Hoster of this crazy game.
        /// </summary>
        public User Hoster { get; private set; }
        /// <summary>
        /// The status of the hosted game.
        /// </summary>
        public Skylabs.Lobby.HostedGame.eHostedGame Status { get; set; }
        /// <summary>
        /// Host a game.
        /// </summary>
        /// <param name="server">Server.cs instance</param>
        /// <param name="gameguid">GUID of the game</param>
        /// <param name="gameversion">Version of the game</param>
        /// <param name="name">Name of the room</param>
        /// <param name="password">Password for the game</param>
        /// <param name="hoster">User hosting the game</param>
        public HostedGame(Server server, Guid gameguid,Version gameversion,string name, string password, User hoster)
        {
            Server = server;
            GameGuid = gameguid;
            GameVersion = gameversion;
            Name = name;
            Password = password;
            Hoster = hoster;
            Status = Lobby.HostedGame.eHostedGame.StoppedHosting;
            lock (Program.Server)
            {
                Port = Program.Server.NextHostPort;
                if (Port != -1)
                {
                    StandAloneApp = new Process();
#if(DEBUG)
                    StandAloneApp.StartInfo.FileName = Directory.GetCurrentDirectory() + "/Octgn.StandAloneServer.exe";
                    StandAloneApp.StartInfo.Arguments = "-g=" + GameGuid.ToString() + " -v=" + GameVersion + " -p=" + Port.ToString();
#else
                    StandAloneApp.StartInfo.FileName = "/opt/mono-2.10/bin/mono";
                    StandAloneApp.StartInfo.Arguments = Directory.GetCurrentDirectory() + "/Octgn.StandAloneServer.exe -g=" + GameGuid.ToString() + " -v=" + GameVersion + " -p=" + Port.ToString();
                    
#endif
                    StandAloneApp.Exited += new EventHandler(StandAloneApp_Exited);
                    StandAloneApp.EnableRaisingEvents = true;
                    try
                    {
                        StandAloneApp.Start();
                        IsRunning = true;
                        Status = Lobby.HostedGame.eHostedGame.StartedHosting;
                        return;
                    }
                    catch (Exception e)
                    {
                        Port = -1;
                        //TODO Need some sort of proper error handling here.
                        Console.WriteLine("");
                        Console.WriteLine(StandAloneApp.StartInfo.FileName);
                        Console.WriteLine(StandAloneApp.StartInfo.Arguments);
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                }
                IsRunning = false;
            }
        }
        /// <summary>
        /// This happens when the Octgn.Server in StandAloneServer stops.
        /// This means eather all of the users disconnected, or it crashed.
        /// Eaither way, its unjoinable at this point.
        /// </summary>
        /// <param name="sender">Who knows</param>
        /// <param name="e">Jesus</param>
        void StandAloneApp_Exited(object sender, EventArgs e)
        {
            Status = Lobby.HostedGame.eHostedGame.StoppedHosting;
            SocketMessage sm = new SocketMessage("gameend");
            sm.AddData("port",Port);
            Server.AllUserMessage(sm);
            Server.Games.Remove(this);
        }
        /// <summary>
        /// Just an equality verifier. 
        /// All hosted games are uniquly identified by there port.
        /// </summary>
        /// <param name="other">Hosted game to check against.</param>
        /// <returns></returns>
        public bool Equals(HostedGame other)
        {
            return other.Port.Equals(Port);
        }
    }
}
