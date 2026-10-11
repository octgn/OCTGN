namespace Octgn.Extentions
{
    using System;
    using System.IO;

    using log4net;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;
    using Octgn.Library.Exceptions;
    using Octgn.Site.Api;
    using Octgn.Core;

    public static class DeckExtensionMethods
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DeckExtensionMethods));

        private const string GenericError = "There was an error sharing your deck. 0xFFFF";

        public static string Share(this IDeck deck, string name)
        {
            string tempFile = null;
            try
            {
                if (!Program.LobbyClient.IsConnected) throw new UserMessageException("You must be logged in to share a deck.");
                if (string.IsNullOrWhiteSpace(name)) throw new UserMessageException("The deck name can't be blank.");
                if (name.Length > 32) throw new UserMessageException("The deck name is too long.");

                tempFile = Path.GetTempFileName();
                var game = GameManager.Get().GetById(deck.GameId);
                deck.Save(game,tempFile);

                var client = new ApiClient();
                var result = client.ShareDeck(Prefs.Username, Program.SessionKey, name, tempFile);
                if (result.Error)
                {
                    if (string.IsNullOrWhiteSpace(result.Message)) throw new UserMessageException(GenericError);
                    throw new UserMessageException(result.Message);
                }
                return result.DeckPath;
            }
            catch (UserMessageException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Type only: the message, URL or response body may contain credentials.
                Log.Warn("Sharing a deck failed unexpectedly: " + ex.GetType().FullName);
                throw new UserMessageException(GenericError);
            }
            finally
            {
                DeleteTempFile(tempFile);
            }
        }

        // Only ever called with the path returned by Path.GetTempFileName above.
        private static void DeleteTempFile(string tempFile)
        {
            if (tempFile == null) return;

            try
            {
                File.Delete(tempFile);
            }
            catch (IOException ex)
            {
                Log.Warn("Removing the temporary deck export failed: " + ex.GetType().FullName);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn("Removing the temporary deck export failed: " + ex.GetType().FullName);
            }
        }
    }
}
