namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

    public class PackDefinition
    {
        public List<IPackItem> Items { get; set; } 
        public PackDefinition()
        {
            Items = new List<IPackItem>();
        }
        public PackDefinition(XmlReader reader)
        {
            Items = new List<IPackItem>();
            while (true)
            {
                if (reader.IsStartElement("pick"))
                    Items.Add(new Pick(reader));
                else if (reader.IsStartElement("options"))
                    Items.Add(new OptionsList(reader));
                else
                    return;
            }
        }

        public PackContent GenerateContent(Pack pack, Set set)
        {
            var result = new PackContent();
            foreach (PackContent defContent in Items.Select(def => def.GetCards(pack,set)))
            {
                result.Merge(defContent);
            }
            return result;
        }
    }
}