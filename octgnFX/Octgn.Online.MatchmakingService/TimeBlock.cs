using System;

namespace Octgn.Online.MatchmakingService
{
    public class TimeBlock
    {
        public TimeSpan When { get; set; }
        public DateTime LastRun { get; set; }

        public bool IsTime
        {
            get
            {
                var ret = new TimeSpan(DateTime.Now.Ticks - LastRun.Ticks).TotalMilliseconds >= When.TotalMilliseconds;
                if (ret)
                    LastRun = DateTime.Now;
                return ret;
            }
        }

        public TimeBlock(TimeSpan when)
        {
            When = when;
            LastRun = DateTime.MinValue;
        }

        public void SetRun()
        {
            LastRun = DateTime.Now;
        }
    }
}