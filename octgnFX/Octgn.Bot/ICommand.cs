namespace Octgn.Bot
{
    using IrcDotNet;

    public interface ICommand
    {
        ChannelBot Channel { get; set; }

        string Usage { get; }

        string[] Arguments { get; }

        void ProcessMessage(IrcMessageEventArgs args, string from, string message);

        bool CanProcessMessage(IrcMessageEventArgs args, string message);
    }
}