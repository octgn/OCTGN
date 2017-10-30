using System;

namespace Octgn.Site.Api.Models
{
    public class CreateSessionResult
    {
        public LoginResult Result { get; set; }
        public string SessionKey { get; set; }
        public string UserId { get; set; }

        public CreateSessionResult(LoginResult result, string key = null, string userId = null)
        {
            Result = result;
            SessionKey = key;
            if(result.Type == LoginResultType.Ok && String.IsNullOrWhiteSpace(SessionKey))
                throw new Exception("SessionKey is null but the result is OK");
            if(result.Type == LoginResultType.Ok && String.IsNullOrWhiteSpace(userId))
                throw new Exception("UserId is null but the result is OK");
        }

        public CreateSessionResult()
        {

        }
    }
}