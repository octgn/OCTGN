using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace Octgn.Data
{
  public enum PropertyType : byte { String, Integer, Char };
  public enum PropertyTextKind : byte { FreeText, Enumeration, Tokens };

  public class PropertyDef
  {
    public static readonly PropertyDef NameProperty = new PropertyDef("Name", PropertyType.String);

    public string Name { get; private set; }
    public PropertyType Type { get; private set; }
    public bool IgnoreText { get; private set; }
    public bool Hidden { get; private set; }
    public PropertyTextKind TextKind { get; private set; }

    private PropertyDef()
    { }

    public PropertyDef(string name, Data.PropertyType type)
    { this.Name = name; this.Type = type; }

    // TODO: make this internal
    public static PropertyDef LoadFromXml(XElement xml)
    {
      string name = xml.Attr<string>("name");

      string kindStr = xml.Attr<string>("textKind", "Free");
      PropertyTextKind kind;
      switch (kindStr)
	    {
        case "Free": kind = PropertyTextKind.FreeText; break;
        case "Enum": kind = PropertyTextKind.Enumeration; break;
        case "Tokens": kind = PropertyTextKind.Tokens; break;
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
      return other.Type == this.Type && other.Name == this.Name;
    }

    public override int GetHashCode()
    { return Name.GetHashCode(); }

    #endregion
  }
}
