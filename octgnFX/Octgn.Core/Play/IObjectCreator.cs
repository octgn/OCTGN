namespace Octgn.Core.Play
{
    using Octgn.Play;

    public interface IObjectCreator
    {
        T Create<T>();

        ITraceChatHandler CreateTraceChatHandler();

        IPlayCard CreateCard(IPlayPlayer owner, int id, ulong key, DataNew.Entities.Card model, bool mySecret);

        IPlayPlayer CreatePlayer(DataNew.Entities.Game game, string nick, byte id, ulong pkey);
    }
}