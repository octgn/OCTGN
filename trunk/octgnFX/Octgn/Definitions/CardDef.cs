using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using Octgn.Data;

namespace Octgn.Definitions
{
  public class CardDef
  {
    // TODO: make those private
    public string back, front;

    public string Back
    {
      get
      {
        return back != null ?
            Program.Game.Definition.PackUri + back :
            "pack://application:,,,/Resources/Back.jpg";
      }
    }

    public string Front
    {
      get
      {
        return front != null ?
            Program.Game.Definition.PackUri + front :
            "pack://application:,,,/Resources/Front.jpg";
      }
    }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public int CornerRadius { get; private set; }
    public Dictionary<string, Data.PropertyDef> Properties { get; private set; }

    internal static CardDef LoadFromXml(XElement xml, PackagePart part)
    {
      string backUri = xml.Attr<string>("back");
      if (backUri != null) backUri = part.GetRelationship(backUri).TargetUri.OriginalString;

      string frontUri = xml.Attr<string>("front");
      if (frontUri != null) frontUri = part.GetRelationship(frontUri).TargetUri.OriginalString;

      return new CardDef
      {
        back = backUri,
        front = frontUri,
        Width = xml.Attr<int>("width"),
        Height = xml.Attr<int>("height"),
        CornerRadius = xml.Attr<int>("cornerRadius"),
        Properties = xml.Elements(Defs.xmlnsOctgn + "property")
                        .Select(x => Data.PropertyDef.LoadFromXml(x))
                        .ToDictionary(x => x.Name)
      };
    }
  }
}