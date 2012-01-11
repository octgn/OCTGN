using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.Lobby;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public static class Gaming
    {
        private static Dictionary<int, HostedGame> Games { get; set; }

        private static readonly object GamingLocker = new object();

        private static int _currentHostPort = 5000;

        static Gaming()
        {
            Games = new Dictionary<int, HostedGame>();
        }
        public static int HostGame(Guid g,Version v, string name,string pass,User u)
        {
            lock(GamingLocker)
            {
                Console.WriteLine("LOCK(HostGame)GamingLocker");
                while (Games.ContainsKey(_currentHostPort) || !Networking.IsPortAvailable(_currentHostPort))
                {
                    _currentHostPort++;
                    if (_currentHostPort >= 8000)
                        _currentHostPort = 5000;
                    
                }
                HostedGame hs = new HostedGame(_currentHostPort,g,v,name,pass,u);
                hs.HostedGameDone += HostedGameExited;
                if (hs.StartProcess())
                {
                    Games.Add(_currentHostPort, hs);
                    Console.WriteLine("UNLOCK(HostGame)GamingLocker");
                    return _currentHostPort;
                }
                else
                {
                    Console.WriteLine("UNLOCK(HostGame)GamingLocker");
                    hs.HostedGameDone -= HostedGameExited;
                    return -1;
                }
                Console.WriteLine("UNLOCK(HostGame)GamingLocker");
            }
        }
        public static void StartGame(int port)
        {
            lock(GamingLocker)
            {
                Console.WriteLine("LOCK(StartGame)GamingLocker");
                try
                {
                    Games[port].Status = Lobby.HostedGame.eHostedGame.GameInProgress;
                }
                catch (Exception)
                {

                }
                Console.WriteLine("UNLOCK(StartGame)GamingLocker");
            }
        }
        public static List<Skylabs.Lobby.HostedGame> GetLobbyList()
        {
            lock(GamingLocker)
            {
                Console.WriteLine("LOCK(GetLobbyList)GamingLocker");
                List<Lobby.HostedGame> sendgames = new List<Lobby.HostedGame>();
                foreach(KeyValuePair<int,HostedGame> g in Games)
                {
                    Lobby.HostedGame newhg =
                        new Lobby.HostedGame(g.Value.GameGuid, (Version)g.Value.GameVersion.Clone(),
                            g.Value.Port, (string)g.Value.Name.Clone(),
                            !String.IsNullOrWhiteSpace(g.Value.Password), (User)g.Value.Hoster.Clone());
                    newhg.GameStatus = g.Value.Status;
                    sendgames.Add(newhg);
                }
                Console.WriteLine("UNLOCK(GetLobbyList)GamingLocker");
                return sendgames;
            }
        }
        private static void HostedGameExited(object sender,EventArgs e)
        {
            lock (GamingLocker)
            {
                Console.WriteLine("LOCK(HostedGameExited)GamingLocker");
                HostedGame s = sender as HostedGame;
                if (s != null)
                {
                    s.Status = Lobby.HostedGame.eHostedGame.StoppedHosting;
                    SocketMessage sm = new SocketMessage("gameend");
                    sm.AddData("port", s.Port);
                    Server.AllUserMessage(sm);
                    Games.Remove(s.Port);
                }
                Console.WriteLine("UNLOCK(HostedGameExited)GamingLocker");
            }
        }
    }
}
