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

        internal void Message(string message)
        {
            const string template = "PRIVMSG {0} :{1}";
            Channel.Client.SendRawMessage(String.Format(template,Channel.Name,message));
        }

        private void ChannelOnUsersListReceived(object sender, EventArgs eventArgs)
        {
            Message("Hello chums!");
        }

        private void ChannelOnMessageReceived(object sender, IrcMessageEventArgs ircMessageEventArgs)
        {
            if (ircMessageEventArgs.Source.Name == "botctgn") return;
            switch (ircMessageEventArgs.Text.Trim().ToLower())
            {
                case "shutup":
                case "shut up":
                    this.Message("Ok fine " + ircMessageEventArgs.Source.Name);
                    Silence = true;
                    break;
                case "sorry":
                    this.Message("Better be. I'm a real sensitive guy.");
                    Silence = false;
                    break;
            }
            if (Silence) return;
            if (String.IsNullOrWhiteSpace(ircMessageEventArgs.Text.Trim())) return;
            switch (ircMessageEventArgs.Text.Trim().ToLower().Split(' ')[0])
            {
                case ".uo":
                    {
                        var ret = new WebClient().DownloadString("http://www.octgn.net/api/stats/usersonlinenow.php");
                        this.Message("There are " + ret.Trim() + " users online right now.");
                        break;
                    }
                case ".ver":
                    {
                        var ret =
                            new WebClient().DownloadString(
                                "https://raw.github.com/kellyelton/OCTGN/master/octgnFX/Octgn/CurrentReleaseVersion.txt").Trim();
                        this.Message("Current live version is " + ret);
                        ret =
                            new WebClient().DownloadString(
                                "https://raw.github.com/kellyelton/OCTGN/master/octgnFX/Octgn/CurrentTestVersion.txt").Trim();
                        this.Message("Current test version is " + ret);
                        break;
                    }
                case ".h":
                    {
                        this.Message(".uo - Online users on OCTGN");
                        this.Message(".ver - Current live version");
                        this.Message(".tv - Current test version");
                        this.Message(".todo {message} - Add a todo message");
                        this.Message(".todor {mess#} - Remove todo message");
                        this.Message(".die - ...");
                        break;
                    }
                case ".todo":
                    {
                        if (!File.Exists("todo.txt")) File.Create("todo.txt").Close();
                        var str = ircMessageEventArgs.Text.Trim().Replace(".todo", "").Trim();
                        if (String.IsNullOrWhiteSpace(str))
                        {
                            var filestr = File.ReadAllLines("todo.txt");
                            var i = 0;
                            foreach (var s in filestr)
                            {
                                this.Message(i + " " + s);
                                i++;
                            }
                        }
                        else
                        {
                            var filestr = File.ReadAllLines("todo.txt").ToList();
                            filestr.Add(ircMessageEventArgs.Source.Name + ": " + str);
                            File.WriteAllLines("todo.txt",filestr);
                            this.Message("I'll jot that down asshole.");
                        }
                        break;
                    }
                case ".todor":
                    {
                        if(!File.Exists("todo.txt"))File.Create("todo.txt").Close();
                        var numstr = ircMessageEventArgs.Text.Trim().Replace(".todor", "").Trim();
                        if (string.IsNullOrWhiteSpace(numstr)) break;
                        int num = -1;
                        if (!int.TryParse(numstr, out num))
                        {
                            this.Message("Don't be dumb " + ircMessageEventArgs.Source.Name + ", " + numstr + " is not a number!");
                        }
                        else
                        {
                            var filestr = File.ReadAllLines("todo.txt").ToList();
                            var remstr = filestr[num];
                            this.Message("[Removed] " + num + " " + remstr);
                            filestr.RemoveAt(num);
                            File.WriteAllLines("todo.txt",filestr);
                        }
                        break;
                    }
                case ".die":
                    {
                        if (ircMessageEventArgs.Source.Name.ToLower() != "kellyelton")
                        {
                            this.Message("You wished " + ircMessageEventArgs.Source.Name);
                            break;
                        }
                        this.Message("I will never forget this " + ircMessageEventArgs.Source.Name);
                        var task = new Task(
                            () =>
                                {
                                    Thread.Sleep(5000);
                                    this.Channel.Leave("WHY DOES NOBODY LOVE ME FAAAAAWK!");
                                    Thread.Sleep(5000);
                                    Program.client.Quit("WAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                                    Thread.Sleep(5000);
                                    Program.client.Disconnect();
                                });
                        task.Start();
                        break;
                    }
                default:
                    {
                        if (ircMessageEventArgs.Text.Trim().EndsWith("?"))
                        {
                            this.Message("http://lmgtfy.com/?q=" + HttpUtility.UrlEncode(ircMessageEventArgs.Text));
                        }
                        else if (ircMessageEventArgs.Text.Contains(":p")
                                 && ircMessageEventArgs.Source.Name.Contains("Grave"))
                        {
                            if(!File.Exists("gtc.txt"))File.Create("gtc.txt").Close();
                            var numstr = File.ReadAllText("gtc.txt");
                            int num = 0;
                            int.TryParse(numstr, out num);
                            num++;
                            this.Message("gtc: " + num);
                            File.WriteAllText("gtc.txt",num.ToString());
                        }
                        else if (ircMessageEventArgs.Text.Trim().StartsWith(".")) Message("I don't understand '" + ircMessageEventArgs.Text + "'");
                        break;
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