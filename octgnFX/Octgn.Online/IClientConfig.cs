using Octgn.Communication;

namespace Octgn.Library.Communication
{
    public interface IClientConfig
    {
        string GameBotUsername { get; }
        string ChatHost { get; }
        IConnectionCreator ConnectionCreator { get; }
    }
}