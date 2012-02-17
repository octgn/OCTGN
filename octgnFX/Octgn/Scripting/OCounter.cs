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
        private readonly Counter[] _counters;

        public OCounter(Player player)
        {
            _counters = player.Counters;
        }

        [IndexerName("values")]
        public int this[string name]
        {
            get { return Find(name).Value; }
        }

        private Counter Find(string name)
        {
            return _counters.First(c => string.Equals(name, c.Name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}