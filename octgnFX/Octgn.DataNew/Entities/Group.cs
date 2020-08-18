namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;

    public class Group
    {
        public byte Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Shortcut { get; set; }
        public string ShuffleShortcut { get; set; }
        public bool MoveTo { get; set; }
        public string Background { get; set; }
        public BackgroundStyle BackgroundStyle { get; set; }
        public GroupVisibility Visibility { get; set; }
        public GroupViewState ViewState { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Ordered { get; set; }
        public IEnumerable<IGroupAction> CardActions { get; set; }
        public IEnumerable<IGroupAction> GroupActions { get; set; } 

        public Group()
        {
            CardActions = new List<IGroupAction>();
            GroupActions = new List<IGroupAction>();
        }
    }
    public interface IGroupAction
    {
        string Name { get; set; }
        bool IsGroup { get; set; }
        string ShowExecute { get; set; }
        string HeaderExecute { get; set; }
    }
    public class GroupActionSubmenu : IGroupAction
    {
        public string Name { get; set; }
        public bool IsGroup { get; set; }
        public string ShowExecute { get; set; }
        public string HeaderExecute { get; set; }
        public IEnumerable<IGroupAction> Children { get; set; } 

        public GroupActionSubmenu()
        {
            Children = new List<IGroupAction>();
        }
    }
    public class GroupAction : IGroupAction
    {
        public string Name { get; set; }
        public bool IsGroup { get; set; }
        public string ShowExecute { get; set; }
        public string HeaderExecute { get; set; }
        public bool DefaultAction { get; set; }
        public string Shortcut { get; set; }
        public string Execute { get; set; }
        public bool IsBatchExecutable { get; set; }
    }
    public class GroupActionSeparator : IGroupAction
    {
        public string Name { get; set; }
        public bool IsGroup { get; set; }
        public string ShowExecute { get; set; }
        public string HeaderExecute { get; set; }
    }
}