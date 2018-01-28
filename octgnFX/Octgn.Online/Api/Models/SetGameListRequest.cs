using Octgn.Online.Hosting;

namespace Octgn.Online.Api.Models
{
    public class SetGameListRequest
    {
        public string ApiKey { get; set; }
        public HostedGame[] Games { get; set; }
    }
}
