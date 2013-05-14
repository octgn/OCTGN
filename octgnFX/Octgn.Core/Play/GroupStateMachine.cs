namespace Octgn.Core.Play
{
    using Octgn.Play;

    public class GroupStateMachine
    {
        internal IPlayGroup Find(int id)
        {
            if (id == 0x01000000) return K.C.Get<IGameEngine>().Table;
            IPlayPlayer player =K.C.Get<PlayerStateMachine>().Find((byte) (id >> 16));
            return player.IndexedGroups[(byte) id];
        }
    }
}