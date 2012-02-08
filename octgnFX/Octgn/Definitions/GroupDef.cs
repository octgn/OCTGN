using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Octgn.Data;

namespace Octgn.Definitions
{
    public class GroupDef
    {
        // TODO: Make these private
        public string background;
        public string board;
        public string icon;

        public BaseActionDef[] CardActions;
        public BaseActionDef[] GroupActions;
        public byte Id { get; private set; }
        public string Name { get; private set; }

        public string Shortcut { get; private set; }
        public string BackgroundStyle { get; private set; }
        public Rect BoardPosition { get; private set; }
        public GroupVisibility Visibility { get; private set; }
        public bool Ordered { get; private set; }
        public bool Collapsed { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public string Background
        {
            get
            {
                if (background == null) return null;
                return Program.Game.Definition.PackUri + background;
            }
        }

        public string Board
        {
            get
            {
                if (board == null) return null;
                return Program.Game.Definition.PackUri + board;
            }
        }

        public string Icon
        {
            get
            {
                if (icon == null) return null;
                return Program.Game.Definition.PackUri + icon;
            }
        }

        internal static GroupDef LoadFromXml(XElement xml, PackagePart part, int index)
        {
            var bg = xml.Attr<string>("background");
            if (bg != null)
                bg = part.GetRelationship(bg).TargetUri.OriginalString;

            var board = xml.Attr<string>("board");
            if (board != null)
                board = part.GetRelationship(board).TargetUri.OriginalString;

            var icon = xml.Attr<string>("icon");
            if (icon != null)
                icon = part.GetRelationship(icon).TargetUri.OriginalString;

            return new GroupDef
                       {
                           Id = (byte) index,
                           Name = xml.Attr<string>("name"),
                           icon = icon,
                           Shortcut = xml.Attr<string>("shortcut"),
                           background = bg,
                           BackgroundStyle = xml.Attr<string>("backgroundStyle"),
                           board = board,
                           BoardPosition = xml.Attr<Rect>("boardPosition"),
                           Visibility = xml.Attr<GroupVisibility>("visibility"),
                           Width = xml.Attr<int>("width"),
                           Height = xml.Attr<int>("height"),
                           Ordered = xml.Attr("ordered", true),
                           Collapsed = xml.Attr<bool>("collapsed"),
                           CardActions = xml.Elements()
                               .Where(
                                   x =>
                                   x.Name == Defs.xmlnsOctgn + "cardaction" || x.Name == Defs.xmlnsOctgn + "cardactions")
                               .Select(x => BaseActionDef.LoadFromXml(x))
                               .ToArray(),
                           GroupActions = xml.Elements()
                               .Where(
                                   x =>
                                   x.Name == Defs.xmlnsOctgn + "groupaction" ||
                                   x.Name == Defs.xmlnsOctgn + "groupactions")
                               .Select(x => BaseActionDef.LoadFromXml(x))
                               .ToArray()
                       };
        }
    }
}