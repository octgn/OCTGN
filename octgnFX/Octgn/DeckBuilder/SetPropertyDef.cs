using System.Collections.Generic;
using System.Linq;
using Octgn.Data;

namespace Octgn.DeckBuilder
{
    public class SetPropertyDef : PropertyDef
    {
        private readonly IList<Set> allSets;

        public SetPropertyDef(IList<Set> allSets)
            : base("Set", 0)
        {
            this.allSets = allSets.OrderBy(s => s.Name).ToList();
        }

        public IList<Set> Sets
        {
            get { return allSets; }
        }
    }
}