using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    public class TemplateDefinition
    {
        public List<LinkDefinition.LinkWrapper> OverlayBlocks = new List<LinkDefinition.LinkWrapper>();
        public List<LinkDefinition.LinkWrapper> TextBlocks = new List<LinkDefinition.LinkWrapper>();
        public List<Property> Matches = new List<Property>();

        public string src;
        public bool defaultTemplate = false;
        
        public List<LinkDefinition> GetOverLayBlocks(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();

            foreach (LinkDefinition.LinkWrapper wrapper in OverlayBlocks)
            {
                if (wrapper.Link != null)
                {
                    ret.Add(wrapper.Link);
                }
                if (wrapper.Conditional != null)
                {
                    foreach (var item in wrapper.Conditional.ResolveConditional(values))
                        ret.Add(item.Link);
                }
                if (wrapper.CardArtCrop != null)
                {
                    LinkDefinition l = new LinkDefinition();
                    l.SpecialBlock = wrapper.CardArtCrop;
                    ret.Add(l);
                }
            }

            return (ret);
        }

        public List<LinkDefinition> GetTextBlocks(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();

            foreach (LinkDefinition.LinkWrapper wrapper in TextBlocks)
            {
                if (wrapper.Link != null)
                {
                    ret.Add(wrapper.Link);
                }
                if (wrapper.Conditional != null)
                {
                    foreach (var item in wrapper.Conditional.ResolveConditional(values))
                        ret.Add(item.Link);
                }
            }

            return (ret);
        }


    }
}
