using System;

namespace Octgn.Site.Api.Models
{
    public class AuthorizedResponse<T>
    {
        public T Response { get; set; }

        public bool Authorized { get; set; }

        public AuthorizedResponse(T response, bool authorized)
        {
            Response = response;
            Authorized = authorized;
        } 
    }
}