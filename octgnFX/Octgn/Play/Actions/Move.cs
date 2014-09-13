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
            var oldHighlight = Card.HighlightColorString;
            var oldMarkers = Card.MarkersString;
            // Move the card
            if (Card.Group != To)
            {
                Card.Group.Remove(Card);
                if (Card.DeleteWhenLeavesGroup)
                    Card.Group = null;
                    //TODO Card.Delete();
                else
                {
                    Card.CardMoved = true;
                    Card.SwitchTo(Who);
                    Card.SetFaceUp(FaceUp);//FaceUp may be false - it's one of the constructor parameters for this
                    Card.SetOverrideGroupVisibility(false);
                    Card.X = X;
                    Card.Y = Y;
                    To.AddAt(Card, Idx);
                    if ((oldGroup != To) || (oldX != X) || (oldY != Y))
                    {
                        if (IsScriptMove) Program.GameEngine.EventProxy.OnScriptedMoveCard_3_1_0_1(Who, Card, oldGroup, To, oldIndex, Idx, oldX, oldY, X, Y, IsScriptMove, oldHighlight, oldMarkers);
                        else Program.GameEngine.EventProxy.OnMoveCard_3_1_0_1(Who, Card, oldGroup, To, oldIndex, Idx, oldX, oldY, X, Y, IsScriptMove, oldHighlight, oldMarkers);
                        
                    } Program.GameEngine.EventProxy.OnMoveCard_3_1_0_0(Who, Card, oldGroup, To, oldIndex, Idx, oldX, oldY, X, Y, IsScriptMove);
                }
            }
            else
            {
                shouldLog = false;
                if ((Card.X != X) || (Card.Y != Y)) Card.CardMoved = true;
                Card.X = X;
                Card.Y = Y;
                if (To.Cards.IndexOf(Card) != Idx)
                {
                    if (To.Ordered)
                        Program.GameMess.PlayerEvent(Who,"reorders {0}",To);
                    Card.SetIndex(Idx);
                }
                if ((oldGroup != To) || (oldX != X) || (oldY != Y))
                {
                    if (IsScriptMove) Program.GameEngine.EventProxy.OnScriptedMoveCard_3_1_0_1(Who, Card, oldGroup, To, oldIndex, Idx, oldX, oldY, X, Y, IsScriptMove, oldHighlight, oldMarkers);
                    else Program.GameEngine.EventProxy.OnMoveCard_3_1_0_1(Who, Card, oldGroup, To, oldIndex, Idx, oldX, oldY, X, Y, IsScriptMove, oldHighlight, oldMarkers);
                    
                } Program.GameEngine.EventProxy.OnMoveCard_3_1_0_0(Who, Card, oldGroup, To, oldIndex, Idx, oldX, oldY, X, Y, IsScriptMove);
            }
            // Should the card be named in the log ?
            shouldSee |= Card.FaceUp;
            // Prepare the message
            if (shouldLog)
                Program.GameMess.PlayerEvent(Who,"moves '{0}' to {2}{1}",
                                         shouldSee ? Card.Type : (object) "Card",
                                         To, To is Pile && Idx > 0 && Idx + 1 == To.Count ? "the bottom of " : "");

            if (Done != null) Done(this, EventArgs.Empty);
        }
    }
}