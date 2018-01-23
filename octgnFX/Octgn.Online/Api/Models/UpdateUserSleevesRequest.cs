using System.Collections.Generic;
using System.Linq;

namespace Octgn.Site.Api.Models
{
    public class UpdateUserSleevesRequest
    {
        public IList<int> ToAdd { get; set; }

        public UpdateUserSleevesRequest()
        {
            ToAdd = new List<int>();
        }

        public UpdateUserSleevesRequest(IEnumerable<ApiSleeve> add)
        {
            ToAdd = add.Select(x => x.Id).ToArray();
        }

        public void Add(ApiSleeve s)
        {
            ToAdd.Add(s.Id);
        }
    }
}