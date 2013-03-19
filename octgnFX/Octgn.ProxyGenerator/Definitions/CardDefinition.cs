using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    public class CardDefinition
    {
        public List<OverlayDefinition> Overlays = new List<OverlayDefinition>();
        public List<SectionDefinition> Sections = new List<SectionDefinition>();
        public MultiMatchDefinition MultiMatch = null;

        public string id;
        public string filename;
        public string rootPath;

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
                    ret.LoadOverlays(subNode);
                }
                if (subNode.Name.Equals("multimatch"))
                {
                    ret.LoadMultiMatch(subNode);
                }
            }

            return (ret);
        }

        public void LoadOverlays(XmlNode node)
        {
            foreach (XmlNode overlayNode in node.ChildNodes)
            {
                OverlayDefinition overlay = OverlayDefinition.LoadOverlay(overlayNode);
                overlay.rootpath = rootPath;
                Overlays.Add(overlay);
            }
        }

        public void LoadSections(XmlNode node)
        {
            foreach (XmlNode sectionNode in node.ChildNodes)
            {
                Sections.Add(SectionDefinition.LoadSectionDefinition(sectionNode));
            }
        }

        public void LoadMultiMatch(XmlNode node)
        {
            MultiMatch = MultiMatchDefinition.LoadMultiMatchDefinition(node);
        }
    }
}
