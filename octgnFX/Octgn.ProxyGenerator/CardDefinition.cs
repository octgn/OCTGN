using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator
{
    public class CardDefinition
    {
        public List<OverlayDefinition> Overlays = new List<OverlayDefinition>();
        public List<SectionDefinition> Sections = new List<SectionDefinition>();

        public string id;
        public string filename;

        public static CardDefinition LoadCardDefinition(XmlNode node)
        {
            CardDefinition ret = new CardDefinition();
            ret.id = node.Attributes["id"].Value;
            ret.filename = node.Attributes["filename"].Value;
            foreach (XmlNode subNode in node)
            {
                if (subNode.Name.Equals("sections"))
                {
                    ret.LoadSections(subNode);
                }
                if (subNode.Name.Equals("overlays"))
                {
                    ret.LoadOverlays(node);
                }
            }

            return (ret);
        }

        public void LoadOverlays(XmlNode node)
        {
            foreach (XmlNode overlayNode in node.ChildNodes)
            {
                Overlays.Add(OverlayDefinition.LoadOverlay(overlayNode));
            }
        }

        public void LoadSections(XmlNode node)
        {
            foreach (XmlNode sectionNode in node.ChildNodes)
            {
                Sections.Add(SectionDefinition.LoadSectionDefinition(sectionNode));
            }
        }
    }
}
