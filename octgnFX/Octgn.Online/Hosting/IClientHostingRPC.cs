using System.Threading.Tasks;

namespace Octgn.Online.Hosting
{
    public interface IClientHostingRPC
    {
        Task<HostedGame> HostGame(HostedGame game);
    }
}
