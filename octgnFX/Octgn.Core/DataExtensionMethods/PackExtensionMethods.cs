namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;

    public static class PackExtensionMethods
    {
        internal static Random Random = new Random();
        public static PackContent CrackOpen(this Pack pack)
        {
            var set = pack.Set();
            // add the include cards to the set for this booster
            foreach (var i in pack.Includes)
            {
                var refSet = SetManager.Get().GetById(i.SetId);
                var refCard = refSet.Cards.FirstOrDefault(x => x.Id == i.Id);
                var iCard = refCard.Clone();
                
                foreach (var p in i.Properties)
                {
                    var key = refCard.Properties[""].Properties.Where(x => x.Key.Name.ToLower() == p.Item1.ToLower()).FirstOrDefault().Key;
                    iCard.Properties[""].Properties[key] = p.Item2;
                }

                set.AddCard(iCard);
            }

            return pack.Definition.GenerateContent(pack,pack.Set());
        }
        public static string GetFullName(this Pack pack)
        {
            var set = SetManager.Get().GetById(pack.SetId);
            //var set = SetManager.Get().Sets.FirstOrDefault(x => x.Packs.Any(y => y.Id == pack.Id));
            if (set == null) return null;
            return set.Name + ", " + pack.Name;
        }
        public static Set Set(this Pack pack)
        {
            return SetManager.Get().GetById(pack.SetId);
        }
    }
}