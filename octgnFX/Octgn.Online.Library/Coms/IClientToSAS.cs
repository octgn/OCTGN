namespace Octgn.Online.Library.Coms
{
    using System;
    using System.Threading.Tasks;

    using Octgn.Online.Library.Models;

    public interface IClientToSAS
    {
        Task Hello(string nick, Guid key, Version octgnOnlineLibraryVersion, Version gameVersion, string password);

        Task NextTurn();

        Task SetTurn(int playerId);

        Task Chat(string message);

        Task Notify(string message);

        Task Notify(int playerId, string message);

        Task Random(int min, int max);

        Task SetCounterValue( int counterId, int value);

        Task LoadDeck(string rawDeckString, HostedGameDeck deck);

        //Task Pick
    }
}