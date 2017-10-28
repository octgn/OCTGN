using System.Threading.Tasks;

namespace Octgn.Online.Hosting
{
    public interface IClientHostingRPC
    {
        Task<IHostedGame> HostGame(HostGameRequest request);

        Task SignalGameStarted(string gameId);
    }
}
