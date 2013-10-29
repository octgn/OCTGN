namespace Octgn.Extentions
{
    using System;
    using System.IO;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;
    using Octgn.Library.Exceptions;
    using Octgn.Site.Api;

    public static class DeckExtensionMethods
    {
        public static string Share(this IDeck deck, string name)
        {
            try
            {
                var tempFile = Path.GetTempFileName();
                var game = GameManager.Get().GetById(deck.GameId);
                deck.Save(game,tempFile);

                var client = new ApiClient();
                if (!Program.LobbyClient.IsConnected) throw new UserMessageException("You must be logged in to share a deck.");
                if (string.IsNullOrWhiteSpace(name)) throw new UserMessageException("The deck name can't be blank.");
                if (name.Length > 32) throw new UserMessageException("The deck name is too long.");
                var result = client.ShareDeck(Program.LobbyClient.Username, Program.LobbyClient.Password, name, tempFile);
                if (result.Error)
                {
                    throw new UserMessageException(result.Message);
                }
                return result.DeckPath;
            }
            catch (Exception e)
            {
                throw new UserMessageException("There was an error sharing your deck. 0xFFFF");
            }
        }
    }
}