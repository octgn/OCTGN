using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator
{
    public class OverlayDefinition
    {
        public SectionStructs.Location location;
        public string filename;

        public static OverlayDefinition LoadOverlay(XmlNode node)
        {
            OverlayDefinition ret = new OverlayDefinition();

            ret.filename = node.Attributes["filename"].Value;
            if (node.HasChildNodes)
            {
                XmlNode prop = node.FirstChild;
                ret.location.x = int.Parse(prop.Attributes["x"].Value);
                ret.location.y = int.Parse(prop.Attributes["y"].Value);
            }

            return (ret);
        }
    }
}
