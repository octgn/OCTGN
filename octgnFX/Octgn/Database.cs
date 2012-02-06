using System;
using System.Collections.Generic;
using System.Linq;
using Octgn.Data;

namespace Octgn
{
	public static class Database
	{
		private static Data.Game openedGame;

        public static Data.Game OpenedGame
        { get { return openedGame; } }

		public static void Open(Definitions.GameDef game, bool readOnly)
		{
			openedGame = Program.GamesRepository.Games.First(g => g.Id == game.Id);
			openedGame.OpenDatabase(readOnly);
		}

		public static void Close()
		{
			if (openedGame != null)
				openedGame.CloseDatabase();
		}

		public static CardModel GetCardByName(string name)
		{
			return openedGame.GetCardByName(name);
		}

		public static CardModel GetCardById(Guid id)
		{
			return openedGame.GetCardById(id);			
		}

		public static IEnumerable<MarkerModel> GetAllMarkers()
		{
			return openedGame.GetAllMarkers();			
		}

    public static IEnumerable<CardModel> GetCards(params string[] conditions)
    {
      return openedGame.SelectCardModels(conditions);
    }

    public static IEnumerable<Set> GetAllSets()
    {
      return openedGame.Sets;
    }

    internal static Pack GetPackById(Guid packId)
    {
      return openedGame.GetPackById(packId);
    }
  }
}