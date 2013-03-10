namespace Octgn.DataNew.Entities
{
    using System;
    using System.Xml;

    public class Pick : IPackItem
    {
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

        public PackContent GetCards(Pack pack)
        {
            var result = new PackContent();
            //TODO [DB MIGRATION] blah
            throw new NotImplementedException("Holy Moly");
            //var conditions = new string[2];
            //conditions[0] = "set_id = '" + pack.Set.Id + "'";
            //conditions[1] = string.Format("{0} = '{1}'", Key, Value);
            //if (Quantity < 0)
            //    result.UnlimitedCards.AddRange(pack.Set.Game.SelectCardModels(conditions));
            //else
            //    result.LimitedCards.AddRange(pack.Set.Game.SelectRandomCardModels(Quantity, conditions));
            return result;
        }
    }
}