namespace Octgn.Play
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Media;

    public interface IPlayPlayer : INotifyPropertyChanged
    {
        ulong PublicKey { get; }
        IPlayCounter[] Counters { get; }

        IPlayGroup[] IndexedGroups { get; }

        IEnumerable<IPlayGroup> Groups { get; }

        Dictionary<string, int> Variables { get; }

        Dictionary<string, string> GlobalVariables { get; }

        IPlayGroup Hand { get; }

        byte Id // Identifier
        { get; set; }
        string Name // Nickname
        { get; set; }

        bool IsGlobalPlayer { get; }

        bool InvertedTable // True if the lPlayer plays on the opposite side of the table (for two-sided table only)
        { get; set; }

        Color Color { get; set; }

        Brush Brush { get; set; }

        Brush TransparentBrush { get; set; }

        void SetPlayerColor(int idx);

        void Delete();

        string ToString();
    }
}