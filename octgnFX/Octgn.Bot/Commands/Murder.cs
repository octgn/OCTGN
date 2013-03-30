namespace Octgn.Bot.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using IrcDotNet;

    public class Murder: ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage { get{return "Kick someone";} }

        public string[] Arguments { get{return new string[] { "Username" };} }

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            if(string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Yeah...so...I'll just murder no one then...");
            if(message.ToLower() == "kellyelton")
                throw new ArgumentException("I can't murder my creator!");
            if(message.ToLower() == Channel.Channel.Client.LocalUser.NickName.ToLower())
                throw new ArgumentException("That would be suicide, not murder!");

            new Task(() =>
            {
                Channel.Channel.Kick(message, from + " Told me to do it! I didn't mean it, I swear!");
                Thread.Sleep(2000);
                Channel.Message("Well that felt good.");
            }).Start();
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }
}