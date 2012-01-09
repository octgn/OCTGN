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
                    return _currentHostPort;
                }
                else
                {
                    hs.HostedGameDone -= HostedGameExited;
                    return -1;
                }
            }
        }
        public static void StartGame(int port)
        {
            lock(GamingLocker)
            {
                Games[port].Status = Lobby.HostedGame.eHostedGame.GameInProgress;
            }
        }
        public static List<Skylabs.Lobby.HostedGame> GetLobbyList()
        {
            lock(GamingLocker)
            {
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
                return sendgames;
            }
        }
        private static void HostedGameExited(object sender,EventArgs e)
        {
            lock (GamingLocker)
            {
                HostedGame s = sender as HostedGame;
                if (s != null)
                {
                    s.Status = Lobby.HostedGame.eHostedGame.StoppedHosting;
                    SocketMessage sm = new SocketMessage("gameend");
                    sm.AddData("port", s.Port);
                    Server.AllUserMessage(sm);
                    Games.Remove(s.Port);
                }
            }
        }
    }
}
