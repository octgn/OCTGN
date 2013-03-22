namespace Octgn.Bot
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    using IrcDotNet;

    public class ChannelBot
    {
        internal IrcChannel Channel { get; set; }
        internal bool Silence { get; set; }
        public ChannelBot(IrcChannel channel)
        {
            Channel = channel;

            Channel.UsersListReceived += ChannelOnUsersListReceived;
            Channel.UserJoined += ChannelOnUserJoined;
            Channel.UserLeft += ChannelOnUserLeft;
            Channel.MessageReceived += ChannelOnMessageReceived;
            Channel.Client.RawMessageSent += ClientOnRawMessageSent;
        }

        private void ClientOnRawMessageSent(object sender, IrcRawMessageEventArgs ircRawMessageEventArgs)
        {
            //ircRawMessageEventArgs.
        }

        internal void Message(string message, bool force = false)
        {
            const string template = "PRIVMSG {0} :{1}";
            if(!Silence)
                Channel.Client.SendRawMessage(String.Format(template, Channel.Name, message));
            else if(Silence && force)
                Channel.Client.SendRawMessage(String.Format(template,Channel.Name,message));
        }

        private void ChannelOnUsersListReceived(object sender, EventArgs eventArgs)
        {
            Message("Hello chums!");
        }

        private void ChannelOnMessageReceived(object sender, IrcMessageEventArgs ircMessageEventArgs)
        {
            if (ircMessageEventArgs.Source.Name == "botctgn") return;

            var commands = AppDomain.CurrentDomain.GetAssemblies()
                     .SelectMany(x => x.GetModules())
                     .SelectMany(x => x.GetTypes())
                     .Where(x => x.GetInterfaces().Any(y => y == typeof(ICommand)))
                     .ToList();

            var fullMess = ircMessageEventArgs.Text;
            var from = ircMessageEventArgs.Source.Name;

            if (string.IsNullOrWhiteSpace(fullMess)) return;

            if (!fullMess.StartsWith("."))
            {
                foreach (var com in commands)
                {
                    var ac = Activator.CreateInstance(com) as ICommand;
                    ac.CanProcessMessage(ircMessageEventArgs, fullMess);
                }
            }
            else
            {
                fullMess = fullMess.Substring(1);
                var firstSpace = fullMess.IndexOf(' ');
                if (firstSpace <= 0) firstSpace = fullMess.Length;
                var comstr = fullMess.Substring(0, firstSpace).Trim();

                var command = commands.FirstOrDefault(x => x.Name.ToLower() == comstr);
                if (command == null)
                {
                    Message("I don't understand '" + ircMessageEventArgs.Text + "'",true);
                    return;
                }

                var messstr = fullMess.Substring(comstr.Length).Trim();

                try
                {
                    var ac = Activator.CreateInstance(command) as ICommand;
                    ac.Channel = this;
                    ac.ProcessMessage(ircMessageEventArgs,from,messstr);
                }
                catch (ArgumentException e)
                {
                    this.Message("Paradox: " + e.Message,true);
                }
                catch(Exception e)
                {
                    Message("Something blew up...I'm scared.",true);
                    this.Message(e.ToString());
                }
            }
        }

        private void ChannelOnUserLeft(object sender, IrcChannelUserEventArgs ircChannelUserEventArgs)
        {
            Message(ircChannelUserEventArgs.ChannelUser.User.NickName + " was a real dick that year...");
        }

        private void ChannelOnUserJoined(object sender, IrcChannelUserEventArgs ircChannelUserEventArgs)
        {
            this.Message("Hello " + ircChannelUserEventArgs.ChannelUser.User.NickName);
        }
    }
}