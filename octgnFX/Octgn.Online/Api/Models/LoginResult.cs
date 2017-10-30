namespace Octgn.Site.Api
{
    public class LoginResult
    {
        public LoginResultType Type { get; set; }
        public string Username { get; set; }

        public static LoginResult UnknownError => new LoginResult { Type = LoginResultType.UnknownError };
        public static LoginResult EmailUnverified => new LoginResult { Type = LoginResultType.EmailUnverified };
        public static LoginResult UnknownUsername => new LoginResult { Type = LoginResultType.UnknownUsername };
        public static LoginResult PasswordWrong => new LoginResult { Type = LoginResultType.PasswordWrong };
        public static LoginResult NotSubscribed => new LoginResult { Type = LoginResultType.NotSubscribed };
        public static LoginResult NoEmailAssociated => new LoginResult { Type = LoginResultType.NoEmailAssociated };
        public static LoginResult Ok( string username ) {
            return new LoginResult { Type = LoginResultType.Ok, Username = username };
        }
    }

    public enum LoginResultType
    {
        UnknownError,
        Ok,
        EmailUnverified,
        UnknownUsername,
        PasswordWrong,
        NotSubscribed,
        NoEmailAssociated
    }
}