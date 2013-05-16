using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Octgn.Data;

namespace Octgn.Definitions
{
    using Octgn.DataNew;

    public class DeckDef
    {
        public Dictionary<string, DeckSectionDef> Sections { get; set; }

        internal static DeckDef LoadFromXml(XElement xml)
        {
            if (xml == null) return null;

            return new DeckDef
                       {
                           Sections = xml.Elements(Defs.XmlnsOctgn + "section")
                               .Select(DeckSectionDef.LoadFromXml)
                               .ToDictionary(x => x.Name)
                       };
        }
    }

    public class DeckSectionDef
    {
        public string Name { get; set; }
        public string Group { get; set; }

        internal static DeckSectionDef LoadFromXml(XElement xml)
        {
            return new DeckSectionDef
                       {
                           Name = xml.Attr<string>("name"),
                           Group = xml.Attr<string>("group")
                       };
        }
    }
}