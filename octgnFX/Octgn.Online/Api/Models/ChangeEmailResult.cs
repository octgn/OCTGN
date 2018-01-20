namespace Octgn.Site.Api.Models
{
    public enum ChangeEmailResult
    {
        UnknownError,
        Ok,
        EmailAlreadyTaken,
        UnknownUsername,
        PasswordWrong,
    }
}
