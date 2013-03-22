namespace Octgn.Bot.Commands
{
    using IrcDotNet;

    public class Shutup : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage {
            get
            {
                return "Silence me";
            }
        }

        public string[] Arguments
        {
            get
            {
                return new string[0];
            }
        }

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            Channel.Message("Ok fine " + from);
            Channel.Silence = true;
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }
    public class Sorry : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage {
            get
            {
                return "Make up for being a dick, then maybe I'll talk again";
            }
        }
        public string[] Arguments
        {
            get
            {
                return new string[0];
            }
        }

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            Channel.Silence = false;
            Channel.Message("Better be. I'm a real sensitive guy.");
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }
}