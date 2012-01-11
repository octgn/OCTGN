using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Octgn.Data;
using System.IO.Packaging;
using System.IO;


namespace Octgn.Definitions
{
  public class ScriptDef
  {
    public string Python { get; private set; }

    public static List<ScriptDef> LoadAllFromXml(XElement xml, PackagePart part)
    {
      if (xml == null) return new List<ScriptDef>(0);

      return xml.Elements("script")
               .Select(x => ScriptDef.FromXml(x, part))
               .ToList();
    }

    public static ScriptDef FromXml(XElement xml, PackagePart part)
    {      
      string srcUri = part.GetRelationship(xml.Attr<string>("src")).TargetUri.OriginalString;
      var scriptPart = part.Package.GetPart(new Uri(srcUri, UriKind.Relative));

      using (var stream = scriptPart.GetStream(FileMode.Open, FileAccess.Read))
      using (var textReader = new StreamReader(stream))
        return new ScriptDef { Python = textReader.ReadToEnd() };
    }
  }
}
