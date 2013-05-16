using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;
using Octgn.Data;

namespace Octgn.Definitions
{
    using Octgn.DataNew;

    public class IconDef
    {
        public string Icon { get; private set; }
        public string Pattern { get; private set; }

        public static List<IconDef> LoadAllFromXml(XElement xml, PackagePart part)
        {
            if (xml == null) return new List<IconDef>(0);

            return xml.Elements("replace")
                .Select(x => FromXml(x, part))
                .ToList();
        }

        private static IconDef FromXml(XElement xml, PackagePart part)
        {
            var icon = xml.Attr<string>("icon");
            if (icon != null)
                icon = part.GetRelationship(icon).TargetUri.OriginalString;
            return new IconDef { Icon = icon, Pattern = xml.Attr<string>("pattern")};
        }
    }
}
