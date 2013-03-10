namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Linq;

    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;

    public static class PackExtensionMethods
    {
         public static PackContent CrackOpen(this Pack pack)
         {
             //TODO Finish this!
             throw new NotImplementedException("AHH FUCK");
         }
        public static string GetFullName(this Pack pack)
        {
            var set =
                GameManager.Get().Games.SelectMany(x => x.Sets).FirstOrDefault(x => x.Packs.Any(y => y.Id == pack.Id));
            if (set == null) return null;
            return set.Name + ", " + pack.Name;
        }
    }
}