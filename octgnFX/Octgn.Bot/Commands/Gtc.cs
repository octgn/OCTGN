namespace Octgn.Bot.Commands
{
    using System;
    using System.IO;

    using IrcDotNet;

    public class Gtc : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage { get{return "Gets gtc";} }

        public string[] Arguments{get{return new string[0];}}

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            if (!File.Exists("gtc.txt")) File.Create("gtc.txt").Close();
            var numstr = File.ReadAllText("gtc.txt");
            int num = 0;
            int.TryParse(numstr, out num);
            Channel.Message("GTC=" + num,true);
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            if(args.Source.Name.ToLower().Contains("grave"))
                if (message.ToLower().Contains(":p"))
                {
                    int count = 0, n = 0;

                    while ((n = message.ToLower().IndexOf(":p", n, StringComparison.InvariantCulture)) != -1)
                    {
                        n += ":p".Length;
                        ++count;
                    }

                    if (!File.Exists("gtc.txt")) File.Create("gtc.txt").Close();
                    var numstr = File.ReadAllText("gtc.txt");
                    int num = 0;
                    int.TryParse(numstr, out num);
                    num+= count;
                    File.WriteAllText("gtc.txt", num.ToString());
                    return true;
                }
            return false;
        }
    }
}