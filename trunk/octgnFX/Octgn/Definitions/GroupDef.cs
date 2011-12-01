using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;
using Octgn.Data;

namespace Octgn.Definitions
{
  public class GroupDef
  {
    public byte Id { get; private set; }
    public string Name { get; private set; }

    public string icon;
    public string background;
    public string board;

    public string Shortcut { get; private set; }
    public string BackgroundStyle { get; private set; }
    public System.Windows.Rect BoardPosition { get; private set; }
    public GroupVisibility Visibility { get; private set; }
    public bool Ordered { get; private set; }
    public bool Collapsed { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public BaseActionDef[] cardActions;
    public BaseActionDef[] groupActions;

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
      string _background = xml.Attr<string>("background");
      if (_background != null)
        _background = part.GetRelationship(_background).TargetUri.OriginalString;

      string _board = xml.Attr<string>("board");
      if (_board != null)
        _board = part.GetRelationship(_board).TargetUri.OriginalString;

      string _icon = xml.Attr<string>("icon");
      if (_icon != null)
        _icon = part.GetRelationship(_icon).TargetUri.OriginalString;

      return new GroupDef
      {
        Id = (byte)index,
        Name = xml.Attr<string>("name"),
        icon = _icon,
        Shortcut = xml.Attr<string>("shortcut"),
        background = _background,
        BackgroundStyle = xml.Attr<string>("backgroundStyle"),
        board = _board,
        BoardPosition = xml.Attr<System.Windows.Rect>("boardPosition"),
        Visibility = xml.Attr<GroupVisibility>("visibility"),
        Width = xml.Attr<int>("width"),
        Height = xml.Attr<int>("height"),
        Ordered = xml.Attr<bool>("ordered", true),
        Collapsed = xml.Attr<bool>("collapsed"),
        cardActions = xml.Elements()
                         .Where(x => x.Name == Defs.xmlnsOctgn + "cardaction" || x.Name == Defs.xmlnsOctgn + "cardactions")
                         .Select(x => BaseActionDef.LoadFromXml(x))
                         .ToArray(),
        groupActions = xml.Elements()
                          .Where(x => x.Name == Defs.xmlnsOctgn + "groupaction" || x.Name == Defs.xmlnsOctgn + "groupactions")
                          .Select(x => BaseActionDef.LoadFromXml(x))
                          .ToArray()
      };
    }
  }
}