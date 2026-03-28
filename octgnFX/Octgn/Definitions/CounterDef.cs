using System.IO.Packaging;
using System.Xml.Linq;
using Octgn.Core.DataExtensionMethods;
using Octgn.Data;

namespace Octgn.Definitions
{
    using System.IO;

    using Octgn.DataNew;

    public class CounterDef
    {
        private string _icon;
        public string Name { get; private set; }
        public int Start { get; private set; }
        public byte Id { get; private set; }
        public bool Reset { get; private set; }

        public string Icon
        {
            get
            {
                if (_icon == null) return null;
                return Path.Combine(Program.GameEngine.Definition.GetInstallPath() , _icon);
            }
            private set { _icon = value; }
        }

        public bool HasIcon
        {
            get { return _icon != null; }
        }

        internal static CounterDef LoadFromXml(XElement xml, PackagePart part, int index)
        {
            var icon = xml.Attr<string>("icon");
            if (icon != null)
                icon = part.GetRelationship(icon).TargetUri.OriginalString;

            return new CounterDef
                       {
                           Id = (byte) index,
                           Name = xml.Attr<string>("name"),
                           Icon = icon,
                           Start = xml.Attr<int>("default"),
                           Reset = xml.Attr("reset", true)
                       };
        }
    }
}