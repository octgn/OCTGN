using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using Octgn.Play;

namespace Octgn.Scripting
{
    [SecuritySafeCritical]
    public class OCounter : MarshalByRefObject
    {
        private readonly IPlayCounter[] _counters;

        public OCounter(IPlayPlayer player)
        {
            _counters = player.Counters;
        }

        [IndexerName("values")]
        public int this[string name]
        {
            get { return Find(name).Value; }
        }

        private IPlayCounter Find(string name)
        {
            return _counters.First(c => string.Equals(name, c.Name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}