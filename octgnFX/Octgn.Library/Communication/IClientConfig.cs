namespace Octgn.Library.Communication
{
    public interface IClientConfig
    {
        string GameBotUsername { get; }
        string ChatHost { get; }
        User GameBotUser { get; }
    }
}