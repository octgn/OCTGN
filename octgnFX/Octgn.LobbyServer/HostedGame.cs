using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public class HostedGame : IEquatable<HostedGame>
    {
        public Guid GameGuid { get; private set; }
        public Version GameVersion { get; private set; }
        public int Port { get; private set; }
        public String Name { get; private set; }
        public String Password { get; private set; }
        public Process StandAloneApp { get; set; }
        public bool IsRunning { get; private set; }
        public Server Server { get; set; }

        public HostedGame(Server server, Guid gameguid,Version gameversion,string name, string password)
        {
            Server = server;
            GameGuid = gameguid;
            GameVersion = gameversion;
            Name = name;
            Password = password;
            lock (Program.Server)
            {
                Port = Networking.NextPortInRange(5000, 9000);
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
                        Trace.TraceError(StandAloneApp.StartInfo.FileName);
                        Trace.Flush();
                    }
                }
                IsRunning = false;
            }
        }

        void StandAloneApp_Exited(object sender, EventArgs e)
        {
            SocketMessage sm = new SocketMessage("GameEnd");
            sm.AddData("port",Port);
            Server.AllUserMessage(sm);
            Server.Games.Remove(this);
        }

        public bool Equals(HostedGame other)
        {
            return other.Port == Port;
        }
    }
}
