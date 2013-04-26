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
            return SetManager.Get().Sets.FirstOrDefault(x => x.Id == card.SetId);
        }
        public static MultiCard ToMultiCard(this ICard card, int quantity = 1)
        {
            return new MultiCard
                         {
                             Alternate = card.Alternate,
                             Id = card.Id,
                             ImageUri = card.ImageUri,
                             Name = card.Name,
                             Quantity = quantity,
                             SetId = card.SetId,
                             Properties = card.Properties,
                             Alternates = card.Alternates
                         };
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
            try
            {
                return card.GetSet().GetPictureUri(card.ImageUri + ".b").LocalPath;
            }
            catch (Exception e)
            {
                Log.Warn("",e);
            }
            return null;
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
                              ImageUri = card.ImageUri,
                              Quantity = card.Quantity,
                              Properties = new Dictionary<PropertyDef, object>(),
                                Alternates = new Dictionary<string, CardAlternate>(),
                                SetId = card.SetId
                          };
            foreach (var p in card.Properties)
            {
                ret.Properties.Add(p.Key, p.Value);
            }
            foreach (var p in card.Alternates)
            {
                ret.Alternates.Add(p.Key,p.Value);
            }
            return ret;
        }
    }
}