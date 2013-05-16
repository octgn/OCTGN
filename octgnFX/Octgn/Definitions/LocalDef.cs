using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;
using Octgn.Data;

namespace Octgn.Definitions
{
    using Octgn.DataNew;

    public abstract class LocalDef
    {
        public CounterDef[] Counters { get; private set; }
        public GroupDef[] Groups { get; private set; }

        protected void LoadBaseXml(XElement xml, PackagePart part)
        {
            int i = 1;
            Counters = xml.Elements(Defs.XmlnsOctgn + "counter")
                .Select(x => CounterDef.LoadFromXml(x, part, i++))
                .ToArray();
            i = 1;
            Groups = xml.Elements(Defs.XmlnsOctgn + "group")
                .Select(x => GroupDef.LoadFromXml(x, part, i++))
                .ToArray();
        }
    }

    public class PlayerDef : LocalDef
    {
        public string IndicatorsFormat { get; private set; }
        public GroupDef Hand { get; private set; }
        public List<GlobalVariableDef> GlobalVariables { get; private set; }

        internal static PlayerDef LoadFromXml(XElement xml, PackagePart part)
        {
            var res = new PlayerDef
                          {
                              IndicatorsFormat = xml.Attr<string>("summary"),
                              Hand = GroupDef.LoadFromXml(xml.Child("hand"), part, 0),
                              GlobalVariables = GlobalVariableDef.LoadAllFromXml(xml)
                          };
            res.LoadBaseXml(xml, part);
            return res;
        }
    }

    public class SharedDef : LocalDef
    {
        internal static SharedDef LoadFromXml(XElement xml, PackagePart part)
        {
            if (xml == null) return null;

            var res = new SharedDef();
            res.LoadBaseXml(xml, part);
            return res;
        }
    }
}