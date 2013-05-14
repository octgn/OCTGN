namespace Octgn.Core.Play
{
    using Octgn.Play;

    public class CounterStateMachine
    {

        public IPlayCounter Find(int id)
        {
            IPlayPlayer p = K.C.Get<PlayerStateMachine>().Find((byte)(id >> 16));
            if (p == null || (byte)id > p.Counters.Length || (byte)id == 0)
                return null;
            return p.Counters[(byte)id - 1];
        }
    }
}