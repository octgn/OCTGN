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

        Task Turn(HostedGamePlayer player);

        Task Chat(HostedGamePlayer from, string message);

        Task Notify(string message);

        Task Notify(HostedGamePlayer player, string message);

        Task RandomResponse(int number);

        Task SetCounterValue(HostedGameCounter counter, int value);

        Task LoadDeck(HostedGamePlayer player, HostedGameDeck deck);

        Task CardAdded(HostedGamePlayer player, HostedGameCard card);

        Task CardChanged(HostedGamePlayer player, HostedGameCard card);
    }
}
