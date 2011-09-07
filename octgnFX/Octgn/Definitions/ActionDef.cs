using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using Octgn.Data;
using Octgn.Script;

namespace Octgn.Definitions
{
  public abstract class BaseActionDef
  {
    public string Name { get; protected set; }

    internal static BaseActionDef LoadFromXml(XElement xml)
    {
      if (xml.Name.LocalName.EndsWith("actions"))
        return ActionGroupDef.LoadFromXml(xml);
      else
        return ActionDef.LoadFromXml(xml);
    }
  }

  public sealed class ActionGroupDef : BaseActionDef
  {
    public BaseActionDef[] Children { get; private set; }

    internal static new ActionGroupDef LoadFromXml(XElement xml)
    {
      return new ActionGroupDef
      {
        Name = xml.Attr<string>("menu"),
        Children = xml.Elements()
                      .Select(x => BaseActionDef.LoadFromXml(x))
                      .ToArray()
      };
    }
  }

  public sealed class ActionDef : BaseActionDef
  {
    public bool DefaultAction { get; private set; }
    public string Shortcut { get; private set; }
    public string Execute { get; private set; }
    public string BatchExecute { get; private set; }

    internal static new ActionDef LoadFromXml(XElement xml)
    {
      return new ActionDef
      {
        Name = xml.Attr<string>("menu"),
        DefaultAction = xml.Attr<bool>("default"),
        Shortcut = xml.Attr<string>("shortcut"),
        Execute = xml.Attr<string>("execute"),
        BatchExecute = xml.Attr<string>("batchExecute")
      };
    }
  }
}
