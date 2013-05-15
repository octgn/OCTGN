namespace Octgn.Play
{
    using Octgn.Core.Play;
    using Octgn.DataNew.Entities;

    public class ObjectCreator:IObjectCreator
    {
        public ITraceChatHandler CreateShuffleTraceChatHandler()
        {
            return new ShuffleTraceChatHandler();
        }

        public IPlayCard CreateCard(IPlayPlayer owner, int id, ulong key, DataNew.Entities.Card model, bool mySecret)
        {
            return new Card(owner,id,key,model,mySecret);
        }

        public IPlayPlayer CreatePlayer(Game game, string nick, byte id, ulong pkey)
        {
            return new Player(game,nick,id,pkey);
        }
    }
}