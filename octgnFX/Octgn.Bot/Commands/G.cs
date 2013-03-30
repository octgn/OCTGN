namespace Octgn.Bot.Commands
{
    using System;
    using System.Web;

    using IrcDotNet;

    public class G : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage {get{return "Google it for me, cause I'm lazy";}}

        public string[] Arguments { get{return new[]{"Query string"};} }

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            if(string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("I'm not a dummy.");

            Channel.Message("http://lmgtfy.com/?q=" + HttpUtility.UrlEncode(message));
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }
}