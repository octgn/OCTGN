using System;
using System.Collections.Generic;

using Octgn.Definitions;

namespace Octgn
{
    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;

    public static class Database
    {
        public static DataNew.Entities.Game OpenedGame { get; private set; }

        public static void Open(GameDef game, bool readOnly)
        {
            OpenedGame = GameManager.Get().GetById(game.Id);
            //OpenedGame = Program.GamesRepository.Games.First(g => g.Id == game.Id);
        }

        public static void Close()
        {
        }

        public static Card GetCardByName(string name)
        {
            return OpenedGame.GetCardByName(name);
        }

        public static Card GetCardById(Guid id)
        {
            return OpenedGame.GetCardById(id);
        }

        public static IEnumerable<Marker> GetAllMarkers()
        {
            return OpenedGame.GetAllMarkers();
        }

        //public static IEnumerable<CardModel> GetCards(params string[] conditions)
        //{
        //    return OpenedGame.SelectCardModels(conditions);
        //}

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