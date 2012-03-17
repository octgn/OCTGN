using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.Lobby;
using agsXMPP;
using agsXMPP.protocol.client;

namespace Skylabs.LobbyServer
{
    public static class GameBot
    {
        private static XmppClientConnection Xmpp { get; set; }
        static GameBot() 
        { 
            Xmpp.RegisterAccount = false;
            Xmpp.AutoAgents = true;
            Xmpp.AutoPresence = true;
            Xmpp.AutoRoster = true;
            Xmpp.AutoResolveConnectServer = true;
            Xmpp.Username = "gamebot";
            Xmpp.Password = "123456";
#if(DEBUG)
            Xmpp.Server = "www.skylabsonline.com";
#else
            Xmpp.Server = "localhost";
#endif
            Xmpp.Priority = 1;
            Xmpp.OnLogin += XmppOnOnLogin;
            Xmpp.OnMessage += XmppOnOnMessage;
            Xmpp.OnIq += XmppOnOnIq;
            Xmpp.OnXmppConnectionStateChanged += XmppOnOnXmppConnectionStateChanged;
            Xmpp.Open();
        }

        private static void XmppOnOnIq(object sender , IQ iq)
        {
            var hg = iq as HostedGameData;
            if (hg == null) return;
            int port = Gaming.HostGame(hg.GameGuid , hg.GameVersion , hg.Name , "" , hg.UserHosting);
            if(port != -1)
            {
                hg.Port = port;
            }
            var f = iq.From;
            iq.From = Xmpp.MyJID;
            iq.To = f;
            Xmpp.Send(iq);
        }

        private static void XmppOnOnXmppConnectionStateChanged(object sender , XmppConnectionState state)
        {
            if(state == XmppConnectionState.Disconnected)
                Xmpp.Open();
        }

        private static void XmppOnOnMessage(object sender , Message msg)
        {
            
        }

        private static void XmppOnOnLogin(object sender) 
        {
 
        }
    }
}
