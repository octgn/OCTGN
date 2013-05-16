using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace Octgn.Data
{
    public class Pack
    {
        public Pack(Set set, string xml)
        {
            Set = set;
            using (var stringReader = new StringReader(xml))
            using (XmlReader reader = XmlReader.Create(stringReader))
            {
                reader.Read();
                string guid = reader.GetAttribute("id");
                if (guid != null) Id = new Guid(guid);
                Name = reader.GetAttribute("name");
                reader.ReadStartElement("pack");
                Definition = new PackDefinition(reader);
                reader.ReadEndElement(); // </pack>
            }
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Set Set { get; set; }
        public PackDefinition Definition { get; private set; }

        public string FullName
        {
            get { return Set.Name + ", " + Name; }
        }

        public PackContent CrackOpen()
        {
            return Definition.GenerateContent(this);
        }

        #region PackDefinition

        #region Nested type: BasePackItem

        public abstract class BasePackItem
        {
            public abstract PackContent GetCards(Pack pack);
        }

        #endregion

        #region Nested type: OptionsList

        public class OptionsList : BasePackItem
        {
            public OptionsList(XmlReader reader)
            {
                Options = new List<Option>();

                reader.ReadStartElement("options");
                while (reader.IsStartElement("option"))
                {
                    var option = new Option
                                     {
                                         Probability =
                                             double.Parse(reader.GetAttribute("probability") ?? "1",
                                                          CultureInfo.InvariantCulture)
                                     };
                    reader.Read(); // <option>
                    option.Definition = new PackDefinition(reader);
                    reader.ReadEndElement(); // </option>
                    Options.Add(option);
                }
                reader.ReadEndElement(); // </random>
            }

            public List<Option> Options { get; private set; }

            public override PackContent GetCards(Pack pack)
            {
                var rnd = new Random();
                double value = rnd.NextDouble();
                double threshold = 0;
                foreach (Option option in Options)
                {
                    threshold += option.Probability;
                    if (value <= threshold) return option.Definition.GenerateContent(pack);
                }
                return new PackContent(); // Empty pack
            }

            #region Nested type: Option

            public class Option
            {
                public double Probability { get; set; }
                public PackDefinition Definition { get; set; }
            }

            #endregion
        }

        #endregion

        #region Nested type: PackDefinition

        public class PackDefinition : List<BasePackItem>
        {
            public PackDefinition(XmlReader reader)
            {
                while (true)
                {
                    if (reader.IsStartElement("pick"))
                        Add(new Pick(reader));
                    else if (reader.IsStartElement("options"))
                        Add(new OptionsList(reader));
                    else
                        return;
                }
            }

            public PackContent GenerateContent(Pack pack)
            {
                var result = new PackContent();
                foreach (PackContent defContent in this.Select(def => def.GetCards(pack)))
                {
                    result.Merge(defContent);
                }
                return result;
            }
        }

        #endregion

        #region Nested type: Pick

        public class Pick : BasePackItem
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

            public override PackContent GetCards(Pack pack)
            {
                var result = new PackContent();
                var conditions = new string[2];
                conditions[0] = "set_id = '" + pack.Set.Id + "'";
                conditions[1] = string.Format("{0} = '{1}'", Key, Value);
                if (Quantity < 0)
                    result.UnlimitedCards.AddRange(pack.Set.Game.SelectCardModels(conditions));
                else
                    result.LimitedCards.AddRange(pack.Set.Game.SelectRandomCardModels(Quantity, conditions));
                return result;
            }
        }

        #endregion

        #endregion

        #region Nested type: PackContent

        public class PackContent
        {
            public PackContent()
            {
                LimitedCards = new List<CardModel>();
                UnlimitedCards = new List<CardModel>();
            }

            public List<CardModel> LimitedCards { get; private set; }
            public List<CardModel> UnlimitedCards { get; private set; }

            public void Merge(PackContent content)
            {
                LimitedCards.AddRange(content.LimitedCards);
                UnlimitedCards.AddRange(content.UnlimitedCards);
            }
        }

        #endregion
    }
}