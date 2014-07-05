namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;

    public class Group
    {
        public byte Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Shortcut { get; set; }
        public string Background { get; set; }
        public string BackgroundStyle { get; set; }
        public string Board { get; set; }
        public DataRectangle BoardPosition { get; set; }
        public GroupVisibility Visibility { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Ordered { get; set; }
        public bool Collapsed { get; set; }
        public IEnumerable<IGroupAction> CardActions { get; set; }
        public IEnumerable<IGroupAction> GroupActions { get; set; } 
    }
    public interface IGroupAction
    {
        string Name { get; set; }
        bool IsGroup { get; set; }
        string ShowIf { get; set; }
    }
    public class GroupActionGroup : IGroupAction
    {
        public string Name { get; set; }
        public bool IsGroup { get; set; }
        public string ShowIf { get; set; }
        public IEnumerable<IGroupAction> Children { get; set; } 
    }
    public class GroupAction : IGroupAction
    {
        public string Name { get; set; }
        public bool IsGroup { get; set; }
        public string ShowIf { get; set; }
        public bool DefaultAction { get; set; }
        public string Shortcut { get; set; }
        public string Execute { get; set; }
        public string BatchExecute { get; set; }
    }
    public class GroupActionSeparator : IGroupAction
    {
        public string Name { get; set; }
        public bool IsGroup { get; set; }
        public string ShowIf { get; set; }
    }
}