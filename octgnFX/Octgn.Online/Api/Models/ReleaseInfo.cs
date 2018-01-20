using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Site.Api.Models
{
    public class ReleaseInfo
    {
        public string LiveVersion { get; set; }
        public string LiveVersionDownloadLocation { get; set; }
        public string TestVersion { get; set; }
        public string TestVersionDownloadLocation { get; set; }
    }
}
