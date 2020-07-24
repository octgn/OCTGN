/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Newtonsoft.Json;
using Octgn.Core.DataExtensionMethods;
using Octgn.Core.Play.Save;
using Octgn.DataNew.Entities;

namespace Octgn.Play.State
{

    public class CardSaveState : SaveState<Play.Card, CardSaveState>
    {
        public int Id { get; set; }
        public Guid Type { get; set; }
        public int Index { get; set; }
        public bool FaceUp { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public MarkerSaveState[] Markers { get; set; }
        public bool DeleteWhenLeavesGroup { get; set; }
        public bool OverrideGroupVisibility { get; set; }
        public CardOrientation Orientation { get; set; }
        public bool IsTarget { get; set; }
        public byte TargetedBy { get; set; }
        public bool TargetsOtherCards { get; set; }
        public Color? HighlightColor { get; set; }
        public byte[] PeekingPlayers { get; set; }
        public string Alternate { get; set; }
        public byte Controller { get; set; }
        public byte Owner { get; set; }
        public string Size { get; set; }
        public Dictionary<string, Dictionary<string, object>> PropertyOverrides { get; set; }

        public CardSaveState()
        {

        }

        public override CardSaveState Create(Play.Card card, Play.Player fromPlayer)
        {
            this.Id = card.Id;
            this.Type = card.Type.Model.Id;
            this.Index = card.GetIndex();
            this.FaceUp = ((card.FaceUp && card.Group.Viewers.Contains(fromPlayer)) || (card.Group.Viewers.Contains(fromPlayer)) || card.IsVisibleToAll());
            X = card.X;
            Y = card.Y;

            this.Markers =
                card.Markers.Select(x => new MarkerSaveState().Create(x, fromPlayer)).ToArray();
            this.DeleteWhenLeavesGroup = card.DeleteWhenLeavesGroup;
            this.OverrideGroupVisibility = card.OverrideGroupVisibility;
            this.Orientation = card.Orientation;
            //this.IsTarget = card.tar
            //this.TargetedBy = card.TargetedBy.Id;
            if (card.TargetedBy != null)
                this.TargetedBy = card.TargetedBy.Id;
            this.TargetsOtherCards = card.TargetsOtherCards;
            this.HighlightColor = card.HighlightColor;
            this.PeekingPlayers = card.PeekingPlayers.Select(x => x.Id).ToArray();
            this.Alternate = card.Alternate();
            this.Controller = card.Controller.Id;
            this.Owner = card.Owner.Id;
            this.Size = card.Size.Name;
            this.PropertyOverrides = card.PropertyOverrides;

            return this;
        }
    }
}