using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    public class LinkDefinition
    {
        public string Block { get; set; }
        public List<Property> NestedProperties = new List<Property>();
        public string Separator = " ";
        public bool IsTextLink = false;

        public BlockDefinition SpecialBlock = null;


        public class LinkWrapper
        {
            public LinkDefinition Link = null;
            public ConditionalDefinition Conditional = null;
            public BlockDefinition CardArtCrop = null;
        }
    }
}
