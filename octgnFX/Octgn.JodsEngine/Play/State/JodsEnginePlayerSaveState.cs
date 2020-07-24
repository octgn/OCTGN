/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Octgn.Core.Play.Save;

namespace Octgn.Play.State
{
    public class JodsEnginePlayerSaveState : SaveState<Play.Player, JodsEnginePlayerSaveState>, IPlayerSaveState
    {
        public byte Id { get; set; }
        public string Nickname { get; set; }
        public GroupSaveState[] Groups { get; set; }
        public Dictionary<string, string> GlobalVariables { get; set; }
        public CounterSaveState[] Counters { get; set; }
        public Color Color { get; set; }

        public JodsEnginePlayerSaveState()
        {

        }

        public override JodsEnginePlayerSaveState Create(Play.Player play, Play.Player fromPlayer)
        {
            this.Id = play.Id;
            this.Nickname = play.Name;
            this.GlobalVariables = play.GlobalVariables;
            this.Counters = play.Counters.Select(x => new CounterSaveState().Create(x, fromPlayer)).ToArray();
            this.Groups = play.Groups.Select(x => new GroupSaveState().Create(x, fromPlayer)).ToArray();
            this.Color = play.ActualColor;
            return this;
        }
    }
}