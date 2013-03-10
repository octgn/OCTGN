using System.Linq;
using System.Xml.Linq;
using Octgn.Data;

namespace Octgn.Definitions
{
    using Octgn.DataNew;

    public abstract class BaseActionDef
    {
        public string Name { get; protected set; }

        internal static BaseActionDef LoadFromXml(XElement xml)
        {
            if (xml.Name.LocalName.EndsWith("actions"))
                return ActionGroupDef.LoadFromXml(xml);
            return ActionDef.LoadFromXml(xml);
        }
    }

    public sealed class ActionGroupDef : BaseActionDef
    {
        public BaseActionDef[] Children { get; private set; }

        internal new static ActionGroupDef LoadFromXml(XElement xml)
        {
            return new ActionGroupDef
                       {
                           Name = xml.Attr<string>("menu"),
                           Children = xml.Elements()
                               .Select(BaseActionDef.LoadFromXml)
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

        internal new static ActionDef LoadFromXml(XElement xml)
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