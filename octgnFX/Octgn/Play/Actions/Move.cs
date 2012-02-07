using System;
using System.Diagnostics;

namespace Octgn.Play.Actions
{
    class MoveCard : ActionBase
    {
        internal static event EventHandler Done;
        internal static event EventHandler Doing;

        internal Player who;
        internal Card card;
        internal Group to, from;
        //        private int fromIdx;
        internal int x, y;
        internal int idx;
        internal bool faceUp;

        public MoveCard(Player who, Card card, Group to, int idx, bool faceUp)
        {
            this.who = who; this.card = card;
            this.to = to; this.from = card.Group;
            this.idx = idx;
            this.faceUp = faceUp;
        }

        public MoveCard(Player who, Card card, int x, int y, int idx, bool faceUp)
        {
            this.who = who; this.card = card;
            this.to = Program.Game.Table; this.from = card.Group;
            this.x = x; this.y = y;
            this.idx = idx;
            this.faceUp = faceUp;
        }

        public override void Do()
        {
            if (Doing != null) Doing(this, EventArgs.Empty);

            base.Do();
            bool shouldSee = card.FaceUp, shouldLog = true;
            // Move the card
            if (card.Group != to)
            {
                card.Group.Remove(card);
                if (card.DeleteWhenLeavesGroup)
                    card.Group = null;
                else
                {
                    card.SetFaceUp(faceUp);
                    card.SetOverrideGroupVisibility(false);
                    card.X = x; card.Y = y;
                    to.AddAt(card, idx);
                }
            }
            else
            {
                shouldLog = false;
                card.X = x; card.Y = y;
                if (to.Cards.IndexOf(card) != idx)
                {
                    if (to.Ordered)
                        Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who),
                                "{0} reorders {1}", who, to);
                    card.SetIndex(idx);
                }
            }
            // Should the card be named in the log ?
            shouldSee |= card.FaceUp;
            // Prepare the message
            if (shouldLog)
                Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who),
                        "{0} moves '{1}' to {3}{2}",
                                who, shouldSee ? card.Type : (object)"Card",
                                to, to is Pile && idx > 0 && idx + 1 == to.Count ? "the bottom of " : "");

            if (Done != null) Done(this, EventArgs.Empty);
        }

    }
}