namespace Octgn.Play
{
    using Octgn.DataNew.Entities;

    public interface IActionShortcut
    {
        IGroupAction ActionDef { get; set; }
        object Key { get; set; }
    }
}