using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Octgn.Data;

namespace Octgn.Definitions
{
    using Octgn.DataNew;

    public class GlobalVariableDef
    {
        public string Name { get; private set; }
        public string Value { get; private set; }
        public string DefaultValue { get; private set; }

        public static List<GlobalVariableDef> LoadAllFromXml(XElement xml)
        {
            if (xml == null) return new List<GlobalVariableDef>(0);

            return xml.Elements("globalvariable")
                .Select(x => new GlobalVariableDef
                                 {
                                     Name = x.Attr<string>("name"),
                                     Value = x.Attr<string>("value"),
                                     DefaultValue = x.Attr<string>("value")
                                 })
                .ToList();
        }
    }
}