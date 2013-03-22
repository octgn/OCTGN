namespace Octgn.Bot.Commands
{
    using System;
    using System.IO;
    using System.Linq;

    using IrcDotNet;

    public class For : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage {
            get
            {
                return "Leave someone a message";
            }
        }

        public string[] Arguments {
            get
            {
                return new[]{"username","message"};
            }
        }

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            var wd = Directory.GetCurrentDirectory();
            var path = Path.Combine(wd, "Messages");
            Directory.CreateDirectory(path);

            var firstSpace = message.IndexOf(' ');
            if(firstSpace <= 0) throw new ArgumentException("What the hell are you trying to do?");

            var to = message.Substring(0, firstSpace );
            var mess = message.Substring(firstSpace, message.Length - to.Length);

            var fpath = Path.Combine(path, to + ".txt");

            if(!File.Exists(fpath))File.Create(fpath).Close();
            var cur = File.ReadAllLines(fpath).ToList();

            cur.Add(DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToShortDateString() + " " + from + ": " + mess);
            File.WriteAllLines(fpath,cur);
            Channel.Message("So it shall be.");
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }
    public class Mess : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage { get{return "Read all your messages";} }

        public string[] Arguments{get{return new string[0];}}

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            var wd = Directory.GetCurrentDirectory();
            var path = Path.Combine(wd, "Messages");
            Directory.CreateDirectory(path);

            var fpath = Path.Combine(path, from + ".txt");
            if (!File.Exists(fpath))
            {
                Channel.Message("There are no messages for you, what a surprise.",true);
                return;
            }
            var cur = File.ReadAllLines(fpath);
            if (cur.Length == 0)
            {
                Channel.Message("There are no messages for you, what a surprise.",true);
                return;
            }
            else
            {
                foreach (var s in cur)
                {
                    Channel.Message(s,true);
                    Channel.Message("Well that's about it. Now don't you feel special?");
                }
                File.WriteAllText(fpath,"");
            }
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }
}