using System;
using System.Diagnostics;

namespace Octgn.Play.Actions
{
    internal class MoveCard : ActionBase
    {
        internal Card Card;
        internal bool FaceUp;
        internal Group From;
        internal int Idx;
        internal Group To;
        internal Player Who;
        //        private int fromIdx;
        internal int X, Y;

        public MoveCard(Player who, Card card, Group to, int idx, bool faceUp)
        {
            Who = who;
            Card = card;
            To = to;
            From = card.Group;
            Idx = idx;
            FaceUp = faceUp;
        }

        public MoveCard(Player who, Card card, int x, int y, int idx, bool faceUp)
        {
            Who = who;
            Card = card;
            To = Program.GameEngine.Table;
            From = card.Group;
            X = x;
            Y = y;
            Idx = idx;
            FaceUp = faceUp;
        }

        internal static event EventHandler Done;
        internal static event EventHandler Doing;

        public override void Do()
        {
            if (Doing != null) Doing(this, EventArgs.Empty);

            if (Card == null || Card.Group == null || To == null)
            {
                return;
            }

            base.Do();
#if(DEBUG)
            Debug.WriteLine("Moving " + Card.Name + " from " + From + " to " + To);
#endif
            bool shouldSee = Card.FaceUp, shouldLog = true;
            // Move the card
            if (Card.Group != To)
            {
                Card.Group.Remove(Card);
                if (Card.DeleteWhenLeavesGroup)
                    Card.Group = null;
                    //TODO Card.Delete();
                else
                {
                    Card.SwitchTo(Who);
                    Card.SetFaceUp(FaceUp);//FaceUp may be false - it's one of the constructor parameters for this
                    Card.SetOverrideGroupVisibility(false);
                    Card.X = X;
                    Card.Y = Y;
                    To.AddAt(Card, Idx);
                }
            }
            else
            {
                shouldLog = false;
                Card.X = X;
                Card.Y = Y;
                if (To.Cards.IndexOf(Card) != Idx)
                {
                    if (To.Ordered)
                        Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(Who),
                                                 "{0} reorders {1}", Who, To);
                    Card.SetIndex(Idx);
                }
            }
            // Should the card be named in the log ?
            shouldSee |= Card.FaceUp;
            // Prepare the message
            if (shouldLog)
                Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(Who),
                                         "{0} moves '{1}' to {3}{2}",
                                         Who, shouldSee ? Card.Type : (object) "Card",
                                         To, To is Pile && Idx > 0 && Idx + 1 == To.Count ? "the bottom of " : "");

            if (Done != null) Done(this, EventArgs.Empty);
        }
    }
}