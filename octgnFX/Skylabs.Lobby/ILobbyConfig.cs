namespace Skylabs.Lobby
{
    public interface ILobbyConfig
    {
        string GameBotUsername { get; }
        string ChatHost { get; }
        User GameBotUser { get; }
    }
}