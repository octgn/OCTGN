using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Skylabs.Lobby;

namespace Skylabs.LobbyServer
{
    public enum UserEvent { Online, Offline };

    public class Server
    {
        public IPAddress LocalIP { get; private set; }

        public int Port { get; private set; }

        public TcpListener ListenSocket { get; private set; }

        public List<Client> Clients { get; set; }

        private int NextID;

        public Version Version
        {
            get
            {
                Assembly asm = Assembly.GetCallingAssembly();
                AssemblyProductAttribute at = (AssemblyProductAttribute)asm.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
                return asm.GetName().Version;
            }
        }

        public Server(IPAddress ip, int port)
        {
            NextID = 0;
            LocalIP = ip;
            Port = port;
            ListenSocket = new TcpListener(LocalIP, Port);
            Clients = new List<Client>();
        }

        public void Start()
        {
            ListenSocket.Start();
            Accept_Clients();
        }

        public void Stop()
        {
            ListenSocket.Stop();
            foreach(Client c in Clients)
            {
                c.Stop();
            }
        }

        public Client GetOnlineClientByEmail(string email)
        {
            foreach(Client c in Clients)
            {
                if(c.LoggedIn)
                    if(c.Me.Email.ToLower().Equals(email.ToLower()))
                        return c;
            }
            return null;
        }

        public Client GetOnlineClientByUID(int uid)
        {
            foreach(Client c in Clients)
            {
                if(c.LoggedIn)
                    if(c.Me.UID == uid)
                        return c;
            }
            return null;
        }

        public void On_User_Event(UserEvent e, Client client)
        {
            User me = (User)client.Me.Clone();
            if(e == UserEvent.Offline)
                Clients.Remove(client);
            foreach(Client c in Clients)
                c.OnUserEvent(e, me);
        }

        private void Accept_Clients()
        {
            ListenSocket.BeginAcceptTcpClient(AcceptReceiveDataCallback, ListenSocket);
        }

        private void AcceptReceiveDataCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;
            try
            {
                Clients.Add(new Client(listener.EndAcceptTcpClient(ar), NextID, this));
                NextID++;
                Accept_Clients();
            }
            catch(ObjectDisposedException de)
            {
            }
        }
    }
}