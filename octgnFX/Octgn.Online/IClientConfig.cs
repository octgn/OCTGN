using Octgn.Communication;

namespace Octgn.Library.Communication
{
    public interface IClientConfig : IClientConnectionProvider
    {
        string GameBotUsername { get; }
        string ChatHost { get; }
    }
}