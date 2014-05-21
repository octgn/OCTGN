/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Octgn.Online.MatchmakingService
{
    public class AverageTime
    {
		private object _lock = new object();

        public TimeSpan Time
        {
            get
            {
                lock (_lock)
                {
                    var list = _previousOnes.ToList();
                    list.Add(DateTime.Now.Ticks - _startTime.Ticks);
                    var span = new TimeSpan((long)list.Average(x => x));
                    return span;
                }
            }
        }

        private DateTime _startTime;
        private readonly int _count;
        private readonly Queue<long> _previousOnes = new Queue<long>();

        public AverageTime(int count)
        {
            _count = count;
            _startTime = DateTime.Now;
        }

        public void Cycle()
        {
            lock (_lock)
            {
                _previousOnes.Enqueue(DateTime.Now.Ticks - _startTime.Ticks);
                while (_previousOnes.Count > _count)
                {
                    _previousOnes.Dequeue();
                }
            }
        }
    }
}