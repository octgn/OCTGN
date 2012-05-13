using System;
using System.Collections.Generic;
using System.Linq;
using Octgn.Data;
using Octgn.Definitions;

namespace Octgn
{
    public static class Database
    {
        public static Data.Game OpenedGame { get; private set; }

        public static void Open(GameDef game, bool readOnly)
        {
            OpenedGame = Program.GamesRepository.Games.First(g => g.Id == game.Id);
        }

        public static void Close()
        {
        }

        public static CardModel GetCardByName(string name)
        {
            return OpenedGame.GetCardByName(name);
        }

        public static CardModel GetCardById(Guid id)
        {
            return OpenedGame.GetCardById(id);
        }

        public static IEnumerable<MarkerModel> GetAllMarkers()
        {
            return OpenedGame.GetAllMarkers();
        }

        public static IEnumerable<CardModel> GetCards(params string[] conditions)
        {
            return OpenedGame.SelectCardModels(conditions);
        }

        public static IEnumerable<Set> GetAllSets()
        {
            return OpenedGame.Sets;
        }

        internal static Pack GetPackById(Guid packId)
        {
            return OpenedGame.GetPackById(packId);
        }
    }
}