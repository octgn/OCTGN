using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Octgn.Data;
using System.Collections.ObjectModel;

namespace Octgn.DeckBuilder
{
    public class SetPropertyDef : PropertyDef
    {
        private IList<Set> allSets;

        public IList<Set> Sets
        { get { return allSets; } }

        public SetPropertyDef(IList<Set> allSets)
            : base("Set", 0)
        {
            this.allSets = allSets.OrderBy(s => s.Name).ToList();
        }
    }
}
