namespace Octgn.Bot.Commands
{
    using System.Threading;
    using System.Threading.Tasks;

    using IrcDotNet;

    public class Die : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage { get
        {
            return "Cleanly kills this bot, without leaving a trace.";
        } }

        public string[] Arguments { get{return new string[0];} }

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            if (from.ToLower() != "kellyelton")
            {
                Channel.Message("You wished " + from);
                return;
            }
            Channel.Message("I will never forget this " + from);
            var task = new Task(
                () =>
                {
                    Thread.Sleep(5000);
                    this.Channel.Channel.Leave("WHY DOES NOBODY LOVE ME FAAAAAWK!");
                    Thread.Sleep(5000);
                    Program.client.Quit("WAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                    Thread.Sleep(5000);
                    Program.client.Disconnect();
                });
            task.Start();
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }
}