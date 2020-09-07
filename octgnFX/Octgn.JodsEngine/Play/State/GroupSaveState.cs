/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
using Octgn.DataNew.Entities;

namespace Octgn.Play.State
{
    public class GroupSaveState : SaveState<Play.Group, GroupSaveState>
    {
        public int Id { get; set; }
        public byte Controller { get; set; }
        public CardSaveState[] Cards { get; set; }
        public byte[] Viewers { get; set; }
        public GroupVisibility Visiblity { get; set; }

        public GroupSaveState()
        {
        }

        public override GroupSaveState Create(Play.Group group, Play.Player fromPlayer)
        {
            this.Id = group.Id;

            if (group.Controller != null)
                this.Controller = group.Controller.Id;
            this.Cards = group.Cards.Select(x => new CardSaveState().Create(x, fromPlayer)).ToArray();
            this.Viewers = group.Viewers.Where(x => x.Spectator == false).Select(x => x.Id).ToArray();
            this.Visiblity = group.Visibility;
            return this;
        }
    }
}