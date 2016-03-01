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
			var allSets = pack.Includes
				.Distinct(x=>x.SetId)
				.SelectMany(x=>DbContext.Get().SetsById(x.SetId))
			;
			var clist = pack.Includes.Select(x=>x.Id).ToArray();
			var allCards = allSets
				.SelectMany(x=>x.Cards)
				.Where(x=>clist.Contains(x.Id))
				.Select(refCard=> 
						{
							var card = new Card(refCard);

							foreach (var p in card.Properties)
							{
								var key = refCard.Properties[""].Properties.Where(x => x.Key.Name.ToLower() == p.Item1.ToLower()).FirstOrDefault().Key;
								if (key != null) // if the include property name isn't a defined custom property, ignore it
									iCard.Properties[""].Properties[key] = p.Item2;
							}

							return card;
						})
			;
			cardList.AddRange(allCards);

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
