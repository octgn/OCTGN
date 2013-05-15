namespace Octgn.Core.Play
{
    using Octgn.Play;

    public interface IObjectCreator
    {
        ITraceChatHandler CreateShuffleTraceChatHandler();

        IPlayCard CreateCard(IPlayPlayer owner, int id, ulong key, DataNew.Entities.Card model, bool mySecret);

        IPlayPlayer CreatePlayer(DataNew.Entities.Game game, string nick, byte id, ulong pkey);
    }
}