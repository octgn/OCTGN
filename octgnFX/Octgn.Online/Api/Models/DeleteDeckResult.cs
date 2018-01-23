using System;

namespace Octgn.Site.Api.Models
{
    public class DeleteDeckResult
    {
        public bool Error { get; set; }
        public string Message { get; set; }

        public DeleteDeckResult()
        {

        }

        public DeleteDeckResult(bool error, string format, params object[] args)
        {
            Error = error;
            Message = String.Format(format, args);
        }

        public static DeleteDeckResult Success()
        {
            return new DeleteDeckResult(false, "Your deck has been deleted");
        }

        public static DeleteDeckResult DoesNotExistError(string deckName)
        {
            return new DeleteDeckResult(false,"Could not delete deck: The deck {0} doesn't exist.",deckName);
        }

        public static DeleteDeckResult CredentialsError()
        {
            return new DeleteDeckResult(
                true,
                "The credentials you provided were incorrect.");
        }

        public static DeleteDeckResult UnknownError(int id)
        {
            return new DeleteDeckResult(
                true,
                "There was a problem deleting your deck. Please try again later. 0x{0}", id);
        }

        public static DeleteDeckResult ParameterError()
        {
            return new DeleteDeckResult(
                true,
                "The parameters you provided were incorrect.");
        }
    }
}