/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Octgn.Play.State
{
    public class CounterSaveState : SaveState<Play.Counter, CounterSaveState>
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public byte TypeId { get; set; }
        public int Id { get; set; }

        public CounterSaveState()
        {
        }

        public override CounterSaveState Create(Play.Counter counter, Play.Player fromPlayer)
        {
            this.Name = counter.Name;
            this.Value = counter.Value;
            this.TypeId = counter.Definition.Id;
            this.Id = counter.Id;
            return this;
        }
    }
}