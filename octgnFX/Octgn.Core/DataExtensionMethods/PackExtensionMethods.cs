namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using Octgn.Core.DataManagers;
    using Octgn.Library.ExtensionMethods;
    using Octgn.DataNew.Entities;
    using log4net;
    using System.Reflection;

    public static class PackExtensionMethods
    {
        private static RNGCryptoServiceProvider _provider;

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetFullName(this Pack pack)
        {
            if (pack.Set == null) return null;
            return pack.Set.Name + ", " + pack.Name;
        }

        public static PackContent GenerateContent(this Pack pack)
        {
            var packContents = new PackContent();
            var cardSelectionPool = pack.Set.Cards.ToList();
            var game = pack.Set.GetGame();

            // add the include cards to the set for this booster
            var includeCards =
            (
                from qset in pack.Includes.Select(x => x.SetId)
                    .Distinct()
                    .Select(x => SetManager.Get().GetById(x))
                from card in qset.Cards
                join inc in pack.Includes on qset.Id equals inc.SetId
                where card.Id == inc.Id
                select new { Card = card, Include = inc }
            )
            .Select(picked =>
            {
                var card = new Card(picked.Card);

                foreach (var include in picked.Include.Properties)
                {
                    card.PropertySets[""].Properties[include.Property] = include.Value;
                };
                return card;
            });

            cardSelectionPool.AddRange(includeCards);

            packContents.Merge(ProcessPackItems(pack, pack.Items, cardSelectionPool));
            return packContents;
        }

        private static PackContent ProcessPackItems(Pack pack, List<object> items, List<Card> cardPool)
        {
            PackContent content = new PackContent();
            foreach (var item in items)
            {
                if (item is Pick pick)
                {
                    var filteredPool = new List<Card>(cardPool);
                    foreach (var pickProperty in pick.Properties)
                    {
                        if (pickProperty is NamePickProperty)
                        {
                            var list = (
                                from card in filteredPool
                                where
                                    card.GetName().Equals(pickProperty.Value.ToString(), StringComparison.InvariantCultureIgnoreCase)
                                select card).ToList();
                            filteredPool = list;
                        }
                        else
                        {
                            var Prop = pickProperty.Property;
                            var Value = pickProperty.Value;
                            var list = (
                                from card in filteredPool
                                where
                                    card.MatchesPropertyValue(Prop, Value)
                                select card).ToList();
                            filteredPool = list;
                        }
                    }

                    if (pick.Quantity < 0)
                    {
                        content.UnlimitedCards.AddRange(filteredPool);
                    }
                    else
                    {
                        for (var i = 0; i < pick.Quantity; i++)
                        {
                            var randomCard = filteredPool.RandomElement();
                            if (randomCard != null)
                            {
                                content.LimitedCards.Add(randomCard);
                                filteredPool.Remove(randomCard);
                            }
                            else
                            {
                             //TODO: Proper error logging here
                                Log.Warn(String.Format("Set {0} ({1}) does not contain enough cards to create this booster pack correctly.", pack.Set.Name, pack.Name));
                            }
                        }
                    }
                }
                else if (item is OptionsList optionsList)
                {
                    double threshold = 0;
                    if (_provider == null)
                    {
                        _provider = new RNGCryptoServiceProvider();
                    }
                    var result = new byte[8];
                    _provider.GetBytes(result);
                    var value = ((double)BitConverter.ToUInt64(result, 0) / ulong.MaxValue);

                    Option selectedOption = null;
                    foreach (Option option in optionsList.Options)
                    {
                        threshold += option.Probability;
                        if (value <= threshold)
                        {
                            selectedOption = option;
                            break;
                        }
                    }
                    if (selectedOption != null)
                    {
                        content.Merge(ProcessPackItems(pack, selectedOption.Items, cardPool));
                    }
                }
            }
            return content;
        }
    }
}