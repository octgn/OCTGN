namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;

    using log4net;

    public static class CardExtensionMethods
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public static Set GetSet(this ICard card)
        {
            return SetManager.Get().GetById(card.SetId);
            //return SetManager.Get().Sets.FirstOrDefault(x => x.Id == card.SetId);
        }
        public static MultiCard ToMultiCard(this ICard card, int quantity = 1)
        {
            return new MultiCard
                         {
                             Alternate = card.Alternate.Clone() as String,
                             Id = card.Id,
                             ImageUri = card.ImageUri.Clone() as String,
                             Name = card.Name.Clone() as String,
                             Quantity = quantity,
                             SetId = card.SetId,
                             Properties = card.Properties.ToDictionary(x=>x.Key,y=>y.Value)
                         };
        }
        public static string GetPicture(this ICard card)
        {
            var set = card.GetSet();

            Uri uri = null;

            var files = Directory.GetFiles(set.GetPackUri(), card.GetImageUri() + ".*").OrderBy(x=>x.Length).ToArray();
            if (files.Length == 0) //Generate or grab proxy
            {
                files = Directory.GetFiles(set.GetPackProxyUri(), card.GetImageUri() + ".png");
                if (files.Length != 0)
                {
                    uri = new Uri(files.First());
                }
            }
            else
                uri = new Uri(files.First());

            if (uri == null)
            {
                uri = new System.Uri(Path.Combine(set.GetPackProxyUri(), card.GetImageUri() + ".png"));
                set.GetGame().GetCardProxyDef().SaveProxyImage(card.GetProxyMappings(), uri.LocalPath);
                return uri.LocalPath;
            }
            else
            {
                return uri.LocalPath;
            }
        }

        public static string GetImageUri(this ICard card)
        {
            var ret = card.ImageUri;
            if (!String.IsNullOrWhiteSpace(card.Alternate)) ret = ret + "." + card.Alternate;
            return ret;
        }

        public static bool HasProperty(this Card card, string name)
        {
            return card.Properties[card.Alternate].Properties.Any(x => x.Key.Name.Equals(name,StringComparison.InvariantCultureIgnoreCase) && x.Key.IsUndefined == false);
        }

        public static Dictionary<string, string> GetProxyMappings(this ICard card)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (KeyValuePair<PropertyDef, object> kvi in card.PropertySet())
            {
                ret.Add(kvi.Key.Name, kvi.Value.ToString());
            }
            return (ret);
        }

        public static IDictionary<PropertyDef, object> PropertySet(this ICard card)
        {
             var ret = card.Properties[card.Alternate].Properties.Where(x=>x.Key.IsUndefined == false).ToDictionary(x=>x.Key,x=>x.Value);
            return ret;
        }

        public static void SetPropertySet(this Card card, string propertyType = "")
        {
            if (String.IsNullOrWhiteSpace(propertyType)) propertyType = "";
            if(card.Properties.Any(x=>x.Key.Equals(propertyType,StringComparison.InvariantCultureIgnoreCase)))
                card.Alternate = propertyType;
        }

        public static string PropertyName(this Card card)
        {
            return
                card.PropertySet()
                    .First(x => x.Key.Name.Equals("name", StringComparison.InvariantCultureIgnoreCase))
                    .Value as string;
        }

        public static MultiCard Clone(this MultiCard card)
        {
            var ret = new MultiCard
                          {
                              Name = card.Name.Clone() as string,
                              Id = card.Id,
                              Alternate = card.Alternate.Clone() as string,
                              ImageUri = card.ImageUri.Clone() as string,
                              Quantity = card.Quantity,
                              Properties = new Dictionary<string,CardPropertySet>(),
                              SetId = card.SetId
                          };
            foreach (var p in card.Properties)
            {
                ret.Properties.Add(p.Key, p.Value);
            }
            return ret;
        }

        public static Card Clone(this Card card)
        {
            var ret = new Card
                          {
                              Name = card.Name.Clone() as string,
                              Alternate = card.Alternate.Clone() as string,
                              Id = card.Id,
                              ImageUri = card.ImageUri.Clone() as string,
                              Properties =
                                  card.Properties.ToDictionary(
                                      x => x.Key.Clone() as string, x => x.Value.Clone() as CardPropertySet),
                              SetId = card.SetId
                          };
            return ret;
        }
    }
}