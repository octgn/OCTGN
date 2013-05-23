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

    public class FontDef
    {
        public string FileName { get; private set; }
        public int Size { get; private set; }
        public string Target { get; private set; }
        public string Id { get; private set; }

        public static List<FontDef> LoadAllFromXml(XElement xml, PackagePart part)
        {
            if (xml == null) return new List<FontDef>(0);

            return xml.Elements("font")
                .Select(x => FromXml(x, part))
                .ToList();
        }

        public static FontDef FromXml(XElement xml, PackagePart part)
        {
            string srcUri = part.GetRelationship(xml.Attr<string>("src")).TargetUri.OriginalString;
            return new FontDef { FileName = srcUri, Size = xml.Attr<int>("size"), Target = xml.Attr<string>("target"), Id = xml.Attr<string>("src") };
        }
    }
}
