using System.Threading.Tasks;

namespace Octgn.Sdk.Extensibility
{
    public interface ISinglePlayerGameMode : IGameMode
    {
        Task StartSinglePlayer();
    }
}
