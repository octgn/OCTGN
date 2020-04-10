namespace Octgn.DataNew.Entities
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    using System.Collections.Generic;
    using log4net;

    using Octgn.Library.ExtensionMethods;
    using Octgn.Core.DataExtensionMethods;

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
            PickProperty baseProperty = new PickProperty();
            baseProperty.Name = reader.GetAttribute("key");
            baseProperty.Value = reader.GetAttribute("value");
            var newPropList = new List<PickProperty>();
            newPropList.Add(baseProperty);
            Properties = newPropList;
            reader.Read(); // <pick />
        }
        public int Quantity { get; set; }
        public List<PickProperty> Properties { get; set; }

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
            .Select(picked =>
            {
                var card = new Card(picked.Card);

                foreach (var p in picked.Include.Properties)
                {
                    var key = picked.Card.GetFullCardProperties().Where(x => x.Key.Name.ToLower() == p.Item1.ToLower()).FirstOrDefault().Key;
                    if (key != null) // if the include property name isn't a defined custom property, ignore it
                    {
                        if (key.Type is PropertyType.RichText)
                        {
                            var span = new RichSpan();
                            span.Items.Add(new RichText() { Text = p.Item2 });
                            card.PropertySets[""].Properties[key] = new RichTextPropertyValue() { Value = span };
                        }
                        else
                        {
                            card.PropertySets[""].Properties[key] = p.Item2;
                        }
                    }
                }

                return card;
            })
            ;
            cardList.AddRange(cards);

            foreach (var prop in Properties)
            {
                var Key = prop.Name;
                var Value = prop.Value;
                var list = (
                    from card in cardList
                    where
                        card.GetFullCardProperties().Any(
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
