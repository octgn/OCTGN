namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

    public class PackDefinition : List<IPackItem>
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
}