using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Octgn.Data;

namespace Octgn.Definitions
{
    using Octgn.DataNew;

    public class VariableDef
    {
        public string Name { get; private set; }
        public int DefaultValue { get; private set; }
        public bool Reset { get; private set; }
        public bool Global { get; private set; }

        public static List<VariableDef> LoadAllFromXml(XElement xml)
        {
            if (xml == null) return new List<VariableDef>(0);

            return xml.Elements("variable")
                .Select(x => new VariableDef
                                 {
                                     Name = x.Attr<string>("name"),
                                     DefaultValue = x.Attr<int>("default"),
                                     Reset = x.Attr("reset", true),
                                     Global = x.Attr("global", true)
                                 })
                .ToList();
        }
    }
}