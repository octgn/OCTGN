namespace Octgn.Bot.Commands
{
    using System.Net;

    using IrcDotNet;

    public class Uo : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage { get{return "Online users on octgn.";} }
        public string[] Arguments
        {
            get
            {
                return new string[0];
            }
        }
        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            var ret = new WebClient().DownloadString("http://www.octgn.net/api/stats/usersonlinenow.php");
            Channel.Message("There are " + ret.Trim() + " users online right now.",true);
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }
}