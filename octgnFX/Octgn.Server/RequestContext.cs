using Octgn.Online.Library.Models;
using System;

namespace Octgn.Server
{
    public class RequestContext
    {
        public Player Sender { get; set; }
        public Game Game { get; set; }

        public bool IsLocalGame { get; private set; }
        public string ApiKey { get; private set; }

        public RequestContext() {

        }
    }
}
