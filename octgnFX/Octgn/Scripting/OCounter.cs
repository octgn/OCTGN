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
        private readonly Counter[] counters;

        public OCounter(Player player)
        {
            counters = player.Counters;
        }

        [IndexerName("values")]
        public int this[string name]
        {
            get { return Find(name).Value; }
            set
            {
                //Counter counter = Find(name);
                //Engine.Current.Invoke<object>(() => counter.Value = value);
            }
        }

        private Counter Find(string name)
        {
            return counters.First(c => string.Equals(name, c.Name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}