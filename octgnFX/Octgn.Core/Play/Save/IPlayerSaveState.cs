using System.Windows.Media;

namespace Octgn.Core.Play.Save
{
    public interface IPlayerSaveState
    {
        byte Id { get; }
        string Nickname { get; }
        Color Color { get; }
    }
}
