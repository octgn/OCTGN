/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Octgn.Play.State
{
    public class MarkerSaveState : SaveState<Play.Marker, MarkerSaveState>
    {
        public string Id { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }

        public MarkerSaveState()
        {
        }

        public override MarkerSaveState Create(Play.Marker marker, Play.Player fromPlayer)
        {
            this.Id = marker.Model.Id;
            this.Count = marker.Count;
            this.Name = marker.Model.Name;
            return this;
        }
    }
}