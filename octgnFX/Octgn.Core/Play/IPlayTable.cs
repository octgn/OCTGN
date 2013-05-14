namespace Octgn.Play
{
    public interface IPlayTable : IPlayGroup
    {
        void BringToFront(IPlayCard card);

        void SendToBack(IPlayCard card);
    }
}