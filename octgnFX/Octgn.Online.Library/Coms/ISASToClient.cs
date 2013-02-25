namespace Octgn.Online.Library.Coms
{
    using System.Threading.Tasks;

    using Octgn.Online.Library.Models;

    public interface ISASToClient
    {
        Task HelloResponse(Enums.EnumHelloResponse response);

        Task GameState(HostedGameState state);

        Task GameStatusChanged(Enums.EnumHostedGameStatus status);

        Task PlayerStateChanged(HostedGamePlayer player);
    }
}
