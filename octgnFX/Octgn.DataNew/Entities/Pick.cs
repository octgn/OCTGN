namespace Octgn.DataNew.Entities
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    using System.Collections.Generic;
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
            Tuple<string, string> baseProperty = Tuple.Create(reader.GetAttribute("key"), reader.GetAttribute("value"));
            var newPropList = new List<Tuple<string, string>>();
            newPropList.Add(baseProperty);
            Properties = newPropList;
            reader.Read(); // <pick />
        }
        public int Quantity { get; set; }
        public List<Tuple<string, string>> Properties { get; set; }

        public PackContent GetCards(Pack pack, Set set)
        {
            var ret = new PackContent();
            var cardList = set.Cards.ToList();

            // add the include cards to the set for this booster

            var cards =
            (
                from qset in pack.Includes.Select(x=>x.SetId).Distinct().SelectMany(x=>DbContext.Get().SetsById(x))
                from card in qset.Cards
                join inc in pack.Includes on qset.Id equals inc.SetId
                where card.Id == inc.Id
                select new { Card = card, Include = inc }
            )
            .Select(dong =>
            {
                var card = new Card(dong.Card);

                foreach (var p in dong.Include.Properties)
                {
                    var key = dong.Card.Properties[""].Properties.Where(x => x.Key.Name.ToLower() == p.Item1.ToLower()).FirstOrDefault().Key;
                    if (key != null) // if the include property name isn't a defined custom property, ignore it
                    {
                        var span = new RichSpan();
                        span.Items.Add(new RichText() { Text = p.Item2 });
                        card.Properties[""].Properties[key] = new PropertyDefValue() { Value = span };
                    }
                }

                return card;
            })
            ;
            cardList.AddRange(cards);

            foreach (var prop in Properties)
            {
                var Key = prop.Item1;
                var Value = prop.Item2;
                var list = (
                    from card in cardList
                    where
                        card.Properties.Where(x => x.Key == "").SelectMany(x => x.Value.Properties).Any(
                            x =>
                            x.Key.Name.ToLower() == Key.ToLower()
                            && x.Value.ToString().ToLower() == Value.ToLower())
                    select card).ToList();
                cardList = list;
            }

            if (Quantity < 0)
            {
                ret.UnlimitedCards.AddRange(cardList);
            }
            else
            {
                for (var i = 0; i < Quantity; i++)
                {
                    var pick = cardList.RandomElement();
                    if (pick != null)
                    {
                        ret.LimitedCards.Add(pick);
                        cardList.Remove(pick);
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
