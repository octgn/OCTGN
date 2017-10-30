namespace Octgn.Site.Api
{
    public class SharedDeckUploadResult
    {
        public bool Error { get; set; }
        public string Message { get; set; }

        public string DeckPath { get; set; }

        public SharedDeckUploadResult()
        {
            
        }

        public SharedDeckUploadResult(bool error, string deckpath, string format, params object[] args)
        {
            this.Error = error;
            this.Message = string.Format(format ?? "", args);
            this.DeckPath = deckpath;
        }

        internal static SharedDeckUploadResult UnknownError(int id)
        {
            return new SharedDeckUploadResult(
                true,
                null,
                "There was a problem uploading your deck. Please try again later. 0x" + id);
        }

        internal static SharedDeckUploadResult CredentialsError()
        {
            return new SharedDeckUploadResult(
                true,
                null,
                "The credentials you provided were incorrect.");
        }

        internal static SharedDeckUploadResult ParameterError()
        {
            return new SharedDeckUploadResult(
                true,
                null,
                "The parameters you provided were incorrect.");
        }

        internal static SharedDeckUploadResult DeckFormatError()
        {
            return new SharedDeckUploadResult(
                true,
                null,
                "The deck you tried to upload was invalid.");
        }

        internal static SharedDeckUploadResult Success(string path)
        {
            return new SharedDeckUploadResult(
                false,
                path,
                "Your deck has been uploaded successfully.");
        }

        internal static SharedDeckUploadResult NameError()
        {
            return new SharedDeckUploadResult(
                true,
                null,
                "The name you entered is invalid. You can only use a-z 0-9 -_.()");
        }

        internal static SharedDeckUploadResult SharedDeckCountError()
        {
            return new SharedDeckUploadResult(
                true,
                null,
                "You already have too many decks shared.");
        }
    }
}