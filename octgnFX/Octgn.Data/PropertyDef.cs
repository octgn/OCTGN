using System.Xml.Linq;

namespace Octgn.Data
{
    using Octgn.Library.Exceptions;

    public enum PropertyType : byte
    {
        String,
        Integer,
        GUID,
        Char
    };

    public enum PropertyTextKind : byte
    {
        FreeText,
        Enumeration,
        Tokens
    };

    public class PropertyDef
    {
        public static readonly PropertyDef NameProperty = new PropertyDef("Name", PropertyType.String);

        private PropertyDef()
        {
        }

        public PropertyDef(string name, PropertyType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; private set; }
        public PropertyType Type { get; private set; }
        public bool IgnoreText { get; private set; }
        public bool Hidden { get; private set; }
        public PropertyTextKind TextKind { get; private set; }

        public static PropertyDef LoadFromXml(XElement xml)
        {
            var name = xml.Attr<string>("name");

            string kindStr = xml.Attr("textKind", "Free");
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

            return new PropertyDef
                       {
                           Name = name,
                           Type = xml.Attr<PropertyType>("type"),
                           IgnoreText = xml.Attr<bool>("ignoreText"),
                           TextKind = kind,
                           Hidden = xml.Attr<bool>("hidden")
                       };
        }

        #region Equality

        public override bool Equals(object obj)
        {
            var other = obj as PropertyDef;
            if (other == null) return false;
            return other.Type == Type && other.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        #endregion
    }
}