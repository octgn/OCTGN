using System;

namespace Octgn.Online.Api.Models
{
    public class ReportUserRequest
    {
        public string User { get; set; }

        public Guid SessionId { get; set; }

        public string SleeveImage { get; set; }

        public string ChatLog { get; set; }

        public string Reason { get; set; }

        public string Details { get; set; }
    }
}
