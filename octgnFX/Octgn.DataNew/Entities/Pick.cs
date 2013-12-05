namespace Octgn.DataNew.Entities
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    using log4net;

    using Octgn.Library.ExtensionMethods;

    public class Pick : IPackItem
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Pick()
        {
            
        }
        public Pick(XmlReader reader)
        {
            string qtyAttribute = reader.GetAttribute("qty");
            if (qtyAttribute != null) Quantity = qtyAttribute == "unlimited" ? -1 : int.Parse(qtyAttribute);
            Key = reader.GetAttribute("key");
            Value = reader.GetAttribute("value");
            reader.Read(); // <pick />
        }
        public int Quantity { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public PackContent GetCards(Pack pack, Set set)
        {
            var ret = new PackContent();

            if (Quantity < 0)
            {
                ret.UnlimitedCards.AddRange(
                    from card in set.Cards
                    where
                        card.Properties.SelectMany(x=>x.Value.Properties).Any(
                            x =>
                            x.Key.Name.ToLower() ==Key.ToLower()
                            && x.Value.ToString().ToLower() ==Value.ToLower())
                    select card);
            }
            else
            {
                var list = (
                    from card in set.Cards
                    where
                        card.Properties.Where(x=> x.Key == "").SelectMany(x=>x.Value.Properties).Any(
                            x =>
                            x.Key.Name.ToLower() ==Key.ToLower()
                            && x.Value.ToString().ToLower() ==Value.ToLower())
                    select card).ToList();

                for (var i = 0; i < Quantity; i++)
                {
                    var pick = list.RandomElement();
                    if (pick != null)
                    {
                        ret.LimitedCards.Add(pick);
                        list.Remove(pick);
                    }
                    else
                    {
                        Log.Warn(String.Format("Set {0} ({1}) does not contain enough cards to create this booster pack correctly.", set.Id, set.Name));
                    }
                }
            }

            return ret;
        }
    }
}