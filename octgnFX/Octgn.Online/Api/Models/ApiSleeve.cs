using System;
using System.Collections.Generic;
using System.Linq;

namespace Octgn.Site.Api.Models
{
    public class ApiSleeve
    {
        public int Id { get; set; }
        public ApiUser Owner { get; set; }
        public string Url { get; set; }
        public bool IsPrivate { get; set; }
    }

    public class ApiSleeveList
    {
        public ApiSleeve[] Sleeves { get; set; }
        public int TotalSleeveCount { get; set; }
        public Exception e { get; set; }

        public ApiSleeveList()
        {
            
        }

        public ApiSleeveList(IEnumerable<ApiSleeve> sleeves, int totalcount)
        {
            Sleeves = sleeves.ToArray();
            TotalSleeveCount = totalcount;
        }
    }
}