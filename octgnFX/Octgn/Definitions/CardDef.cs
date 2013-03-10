using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;
using Octgn.Data;

namespace Octgn.Definitions
{
    using Octgn.DataNew;
    using Octgn.DataNew.Entities;
    using Octgn.Library.Exceptions;

    public class CardDef
    {
        private string back, front;

        public string Back
        {
            get
            {
                return back != null
                           ? Program.Game.Definition.PackUri + back
                           : "pack://application:,,,/Resources/Back.jpg";
            }
        }

        public string Front
        {
            get
            {
                return front != null
                           ? Program.Game.Definition.PackUri + front
                           : "pack://application:,,,/Resources/Front.jpg";
            }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int CornerRadius { get; private set; }
        public Dictionary<string, DataNew.Entities.PropertyDef> Properties { get; private set; }

        internal static CardDef LoadFromXml(XElement xml, PackagePart part)
        {
            var backUri = xml.Attr<string>("back");
            if (backUri != null) backUri = part.GetRelationship(backUri).TargetUri.OriginalString;

            var frontUri = xml.Attr<string>("front");
            if (frontUri != null) frontUri = part.GetRelationship(frontUri).TargetUri.OriginalString;

            var cd = new CardDef
                       {
                           back = backUri,
                           front = frontUri,
                           Width = xml.Attr<int>("width"),
                           Height = xml.Attr<int>("height"),
                           CornerRadius = xml.Attr<int>("cornerRadius"),
                           //Properties = xml.Elements(Defs.XmlnsOctgn + "property")
                           //    //.Select(PropertyDef.LoadFromXml)
                           //    .ToDictionary(x => x.Name)
                       };
            var list = xml.Elements(Defs.XmlnsOctgn + "property");
            var proplist = new Dictionary<string,PropertyDef>();
            foreach (var l in list)
            {
                var name = l.Attr<string>("name");

                string kindStr = l.Attr("textKind", "Free");
                PropertyTextKind kind;
                switch (kindStr)
                {
                    case "Free":
                        kind = PropertyTextKind.FreeText;
                        break;
                    case "Enum":
                        kind = PropertyTextKind.Enumeration;
                        break;
                    case "Tokens":
                        kind = PropertyTextKind.Tokens;
                        break;
                    default:
                        throw new InvalidFileFormatException(
                            string.Format("Unknown value '{0}' for textKind attribute (Property = {1})",
                                          kindStr, name));
                }

                var prop = new PropertyDef
                {
                    Name = name,
                    Type = l.Attr<PropertyType>("type"),
                    IgnoreText = l.Attr<bool>("ignoreText"),
                    TextKind = kind,
                    Hidden = l.Attr<bool>("hidden")
                };
                proplist.Add(name,prop);
            }
            cd.Properties = proplist;
            return cd;
        }
    }
}