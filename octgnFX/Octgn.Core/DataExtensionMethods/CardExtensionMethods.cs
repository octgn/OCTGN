namespace Octgn.Core.DataExtensionMethods
{
    using System.Collections.Generic;
    using System.Linq;

    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;

    public static class CardExtensionMethods
    {
        public static Set GetSet(this Card card)
        {
            return GameManager.Get().Games.SelectMany(x=>x.Sets).FirstOrDefault(x => x.Cards.Any(z => z.Id == card.Id));
        }
        public static MultiCard ToMultiCard(this Card card, int quantity = 1)
        {
            return new MultiCard
                         {
                             Alternate = card.Alternate,
                             Dependent = card.Dependent,
                             Id = card.Id,
                             ImageUri = card.ImageUri,
                             IsMutable = card.IsMutable,
                             Name = card.Name,
                             Quantity = quantity
                         };
        }
        public static string GetPicture(this Card card)
        {
            return card.GetSet().GetPackUri() + card.ImageUri;
        }
        public static string AlternatePicture(this Card card)
        {
            string au = card.ImageUri.Replace(".jpg", ".b.jpg");
            return card.GetSet().GetPackUri() + au;
        }
        public static bool HasProperty(this Card card, string name)
        {
            return card.Properties.Any(x => x.Key.Name == name);
        }
        public static MultiCard Clone(this MultiCard card)
        {
            var ret = new MultiCard
                          {
                              Name = card.Name,
                              Id = card.Id,
                              Alternate = card.Alternate,
                              Dependent = card.Dependent,
                              ImageUri = card.ImageUri,
                              IsMutable = card.IsMutable,
                              Quantity = card.Quantity,
                              Properties = new Dictionary<PropertyDef, object>()
                          };
            foreach (var p in card.Properties)
            {
                ret.Properties.Add(p.Key, p.Value);
            }
            return ret;
        }
    }
}