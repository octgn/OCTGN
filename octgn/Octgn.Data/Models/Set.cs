using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Data.Models
{
    public class Set : IDisposable
    {
        public Guid Guid { get; set; }
        public Guid GameGuid { get; set; }

        //todo the database stuff. something with only one connection in the docs?
        public Database DB { get; set; }

        public Game Game
        {
            get
            {
                IList<Game> gameList = DB.DBConnection.Query<Game>(delegate(Game game)
                {
                    return game.Guid == GameGuid;
                });
                return gameList[0];
            }
        }

        public List<Card> GetCards()
        {
            List<Card> cardList = (List<Card>)DB.DBConnection.Query<Card>(delegate(Card card)
            {
                return card.SetGuid == Guid && card.GameGuid == GameGuid;
            });
            return cardList;
        }

        public Card GetCard(Guid cardGuid)
        {
            IList<Card> cardList = DB.DBConnection.Query<Card>(delegate(Card card)
            {
                return card.Guid == cardGuid && card.SetGuid == Guid && card.GameGuid == GameGuid;
            });
            return cardList[0];
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
