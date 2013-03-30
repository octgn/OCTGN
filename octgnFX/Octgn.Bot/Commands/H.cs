namespace Octgn.Bot.Commands
{
    using System;
    using System.Linq;
    using System.Reflection;

    using IrcDotNet;

    public class H : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage { get
        {
            return "Get help cause ur a dum dum";
        } }

        public string[] Arguments { get
        {
            return new string[0];
        } }

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            var commands = AppDomain.CurrentDomain.GetAssemblies()
                     .SelectMany(x => x.GetModules())
                     .SelectMany(x => x.GetTypes())
                     .Where(x => x.GetInterfaces().Any(y => y == typeof(ICommand)))
                     .ToList();
            Channel.Message("====================Help====================",true);
            foreach (var c in commands)
            {
                var com = (ICommand)Activator.CreateInstance(c);
                var argstring = string.Join(" ", com.Arguments.Select(x => "{" + x + "}"));
                Channel.Message("." + c.Name.ToLower() +  " " +  argstring + " - " + com.Usage,true);
            }
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }
}