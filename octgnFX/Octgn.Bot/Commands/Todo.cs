namespace Octgn.Bot.Commands
{
    using System;
    using System.IO;
    using System.Linq;

    using IrcDotNet;

    public class Todo : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage { get{return "List off the todo items";} }

        public string[] Arguments {get{return new string[0];}}

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            if (!File.Exists("todo.txt"))
            {
                Channel.Message("Nothing to do.",true);
                return;
            }

            var lines = File.ReadAllLines("todo.txt");
            if (lines.Length == 0)
            {
                Channel.Message("Nothing to do.",true);
                return;
            }
            Channel.Message("==============TODO==============",true);
            var i = 0;
            foreach (var l in lines)
            {
                Channel.Message("[" + i + "] " + l,true);
                i++;
            }
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }

    public class TodoA : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage { get{return "Add a todo item";} }

        public string[] Arguments {get{return new string[1]{"message"};}}

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            if (!File.Exists("todo.txt")) File.Create("todo.txt").Close();
            if(string.IsNullOrWhiteSpace(message))throw new ArgumentException("You summoned me for nothing?");
            var fstr = File.ReadAllLines("todo.txt").ToList();
            fstr.Add(from + ": " + message);
            File.WriteAllLines("todo.txt",fstr);
            Channel.Message("I'll jot that down asshole.");
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }

    public class TodoR : ICommand
    {
        public ChannelBot Channel { get; set; }

        public string Usage { get{return "Removes a todo item";} }

        public string[] Arguments { get{return new string[1]{"ItemNumber"};} }

        public void ProcessMessage(IrcMessageEventArgs args, string @from, string message)
        {
            var num = -1;
            if (!int.TryParse(message, out num))
                throw new ArgumentException("Don't you know what a number is?");

            if(!File.Exists("todo.txt"))File.Create("todo.txt").Close();

            var filestr = File.ReadAllLines("todo.txt").ToList();
            var remstr = filestr[num];
            Channel.Message("[Removed " + num + "] " + remstr,true);
            filestr.RemoveAt(num);
            File.WriteAllLines("todo.txt",filestr);
        }

        public bool CanProcessMessage(IrcMessageEventArgs args, string message)
        {
            return false;
        }
    }
}