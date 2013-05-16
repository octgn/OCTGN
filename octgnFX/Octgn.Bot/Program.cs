using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Bot
{
    using System.Threading;
    using System.Threading.Tasks;

    using IrcDotNet;

    internal class Program
    {
        private static bool closingTime;

        internal static IrcClient client;

        private static IrcClient octgnClient;
        private static IrcClient octgnDevClient;
        private static List<ChannelBot> channels = new List<ChannelBot>(); 
        static void Main(string[] args)
        {
            client = new IrcClient();
            var reg = new IrcUserRegistrationInfo();
            reg.NickName = "botctgn";
            reg.UserName = "botctgn";
            reg.RealName = "botctgn";
            client.Connect("irc.freenode.net",6667,false,reg);
            client.Disconnected += ClientOnDisconnected;
            client.Connected += ClientOnConnected;
            client.ErrorMessageReceived += ClientOnErrorMessageReceived;
            client.MotdReceived += ClientOnMotdReceived;
            client.RawMessageReceived += ClientOnRawMessageReceived;
            client.ChannelListReceived += ClientOnChannelListReceived;
            client.ProtocolError += ClientOnProtocolError;
            while (!closingTime)
            {
                Thread.Sleep(10);
            }
        }

        private static void Connect()
        {
            
        }

        private static void ClientOnProtocolError(object sender, IrcProtocolErrorEventArgs ircProtocolErrorEventArgs)
        {
            if (ircProtocolErrorEventArgs.Code == 433)
            {
                
            }
        }

        private static void ClientOnChannelListReceived(object sender, IrcChannelListReceivedEventArgs ircChannelListReceivedEventArgs)
        {
            foreach (var c in ircChannelListReceivedEventArgs.Channels)
            {
                var tc = c;
                client.Channels.Join(c.Name);
                var task = new Task(
                    () =>
                        {
                            while (client.Channels.FirstOrDefault(x => x.Name == tc.Name) == null)
                            {
                                Thread.Sleep(10);
                            }
                            channels.Add(new ChannelBot(client.Channels.First(x => x.Name == tc.Name)));
                        });
                task.Start();
            }
        }

        private static void ClientOnRawMessageReceived(object sender, IrcRawMessageEventArgs ircRawMessageEventArgs)
        {
            Console.WriteLine(ircRawMessageEventArgs.Message + " " + ircRawMessageEventArgs.RawContent);
        }

        private static void ClientOnMotdReceived(object sender, EventArgs eventArgs)
        {
            Console.WriteLine("motd");
            client.ListChannels("#octgn-dev");
            //channels.Add(new ChannelBot(client.Channels.First(x => x.Name == "#octgn")));
            //channels.Add(new ChannelBot(client.Channels.First(x => x.Name == "#octgn-dev")));
        }

        private static void ClientOnErrorMessageReceived(object sender, IrcErrorMessageEventArgs ircErrorMessageEventArgs)
        {
            Console.WriteLine("Error: " + ircErrorMessageEventArgs.Message);
        }

        private static void ClientOnConnected(object sender, EventArgs eventArgs)
        {
            Console.WriteLine("Connected");
            //client.ListChannels("octgn","octgn-dev");
            //client.Channels.Join("octgn", "octgn-dev");
        }

        private static void ClientOnDisconnected(object sender, EventArgs eventArgs)
        {
            Console.WriteLine("Disconnected");
            closingTime = true;
        }
    }
}
