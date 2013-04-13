namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;

    public static class CardExtensionMethods
    {
        public static Set GetSet(this ICard card)
        {
            return SetManager.Get().Sets.FirstOrDefault(x => x.Id == card.SetId);
        }
        public static MultiCard ToMultiCard(this ICard card, int quantity = 1)
        {
            return new MultiCard
                         {
                             Alternate = card.Alternate,
                             Dependent = card.Dependent,
                             Id = card.Id,
                             ImageUri = card.ImageUri,
                             IsMutable = card.IsMutable,
                             Name = card.Name,
                             Quantity = quantity,
                             SetId = card.SetId,
                             Properties = card.Properties
                         };
        }
        public static bool IsDependant(this ICard card)
        {
            if (string.IsNullOrWhiteSpace(card.Dependent) || card.Dependent.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                return false;
            return true;
        }
        public static string GetPicture(this ICard card)
        {
            var set = card.GetSet();
            var uri = set.GetPictureUri(card.ImageUri);
            if (uri == null)
            {
                uri = new System.Uri(Path.Combine(set.GetPackProxyUri(), card.ImageUri + ".png"));
                set.GetGame().GetCardProxyDef().SaveProxyImage(card.GetProxyMappings(), uri.LocalPath);
                return uri.LocalPath;
            }
            else
            {
                return uri.LocalPath;
            }

        }
        public static string AlternatePicture(this ICard card)
        {
            return card.GetSet().GetPictureUri(card.ImageUri + ".b").LocalPath;
            //string au = card.ImageUri.Replace(".jpg", ".b.jpg");
            //return card.GetSet().GetPackUri() + au;
        }
        public static bool HasProperty(this Card card, string name)
        {
            return card.Properties.Any(x => x.Key.Name == name);
        }

        public static Dictionary<string, string> GetProxyMappings(this ICard card)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            ret.Add("Name", card.Name);
            foreach (KeyValuePair<PropertyDef, object> kvi in card.Properties)
            {
                ret.Add(kvi.Key.Name, kvi.Value.ToString());
            }
            return (ret);
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