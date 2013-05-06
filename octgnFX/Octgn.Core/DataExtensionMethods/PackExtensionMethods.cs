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