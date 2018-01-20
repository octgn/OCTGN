using System;
using System.Collections.Generic;

namespace Octgn.Site.Api.Models
{

    public class CreateSession
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
    }
}
