using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;

namespace Octgn.Scripting
{
  [SecuritySafeCritical]
  public class OCounter : MarshalByRefObject
  {
    private Play.Counter[] counters;

    public OCounter(Play.Player player)
    { counters = player.Counters; }

    [System.Runtime.CompilerServices.IndexerName("values")]
    public int this[string name]
    {
      get { return Find(name).Value; }
      set
      {
        var counter = Find(name);
        //Engine.Current.Invoke<object>(() => counter.Value = value);
      }
    }

    private Play.Counter Find(string name)
    {
      return counters.First(c => string.Equals(name, c.Name, StringComparison.InvariantCultureIgnoreCase));
    }
  }
}
