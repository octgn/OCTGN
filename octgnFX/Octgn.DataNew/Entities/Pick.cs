namespace Octgn.DataNew.Entities
{
    using System;
    using System.Linq;
    using System.Xml;
    using Octgn.Library.ExtensionMethods;
    public class Pick : IPackItem
    {
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
                        card.Properties.Any(
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
                        card.Properties.Any(
                            x =>
                            x.Key.Name.ToLower() ==Key.ToLower()
                            && x.Value.ToString().ToLower() ==Value.ToLower())
                    select card).ToArray();

                for (var i = 0; i <Quantity; i++)
                    ret.LimitedCards.Add(list.RandomElement());
            }

            return ret;
        }
    }
}