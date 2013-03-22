namespace Octgn.Bot.Commands
{
    using System.Net;

    using IrcDotNet;

    public class Ver : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage { get{return "Gets the current OCTGN version numbers";} }

        public string[] Arguments { get{return new string[0];}}

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            var ret =new WebClient().DownloadString(
                                "https://raw.github.com/kellyelton/OCTGN/master/octgnFX/Octgn/CurrentReleaseVersion.txt").Trim();
            Channel.Message("Current live version is " + ret,true);
            ret =
                new WebClient().DownloadString(
                    "https://raw.github.com/kellyelton/OCTGN/master/octgnFX/Octgn/CurrentTestVersion.txt").Trim();
            Channel.Message("Current test version is " + ret,true);
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }
}