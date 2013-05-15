namespace Octgn.Play
{
    public interface IPlayPile : IPlayGroup
    {
        bool Collapsed { get; set; }
        bool AnimateInsertion { get; set; }
        IPlayCard TopCard { get; }
        bool Shuffle();
        void DoShuffle();
    }
}