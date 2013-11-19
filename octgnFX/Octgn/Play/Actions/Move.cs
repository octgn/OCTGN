using System;
using System.Diagnostics;

namespace Octgn.Play.Actions
{
    using System.Reflection;

    using log4net;

    internal class MoveCard : ActionBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        
        internal Card Card;
        internal bool FaceUp;
        internal Group From;
        internal int Idx;
        internal Group To;
        internal Player Who;
        //        private int fromIdx;
        internal int X, Y;

        internal bool IsScriptMove;

        public MoveCard(Player who, Card card, Group to, int idx, bool faceUp, bool isScriptMove)
        {
            Who = who;
            Card = card;
            To = to;
            From = card.Group;
            Idx = idx;
            FaceUp = faceUp;
            IsScriptMove = isScriptMove;
        }

        public MoveCard(Player who, Card card, int x, int y, int idx, bool faceUp, bool isScriptMove)
        {
            Who = who;
            Card = card;
            To = Program.GameEngine.Table;
            From = card.Group;
            X = x;
            Y = y;
            Idx = idx;
            FaceUp = faceUp;
            IsScriptMove = isScriptMove;
        }

        internal static event EventHandler Done;
        internal static event EventHandler Doing;

        public override void Do()
        {
            if (Doing != null) Doing(this, EventArgs.Empty);

            if (Card == null) return;

            if (To == null)
            {
                Log.DebugFormat("To == null {0}", Card.Id);
                return;
            }

            if (Card.Group == null)
            {
                Log.DebugFormat("Card.Group == null {0}", Card.Id);
                return;
            }

            base.Do();
#if(DEBUG)
            Debug.WriteLine("Moving " + Card.Name + " from " + From + " to " + To);
#endif
            bool shouldSee = Card.FaceUp, shouldLog = true;
            var oldGroup = Card.Group;
            var oldIndex = Card.GetIndex();
            var oldX = (int)Card.X;
            var oldY = (int)Card.Y;
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
                    Program.GameEngine.EventProxy.OnMoveCard(Who,Card,oldGroup,To,oldIndex,Idx,oldX,oldY,X,Y, IsScriptMove);
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
                Program.GameEngine.EventProxy.OnMoveCard(Who,Card,oldGroup,To,oldIndex,Idx,oldX,oldY,X,Y,IsScriptMove);
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