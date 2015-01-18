using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Scripting.Utils;


namespace Octgn.Play.Actions
{
    using System.Reflection;

    using log4net;

    //    internal class MoveCard : ActionBase
    //    {
    //        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


    //        internal Card Card;
    //        internal bool FaceUp;
    //        internal Group From;
    //        internal int Idx;
    //        internal Group To;
    //        internal Player Who;
    //        //        private int fromIdx;
    //        internal int X, Y;

    //        internal bool IsScriptMove;

    //        public MoveCard(Player who, Card card, Group to, int idx, bool faceUp, bool isScriptMove)
    //        {
    //            Who = who;
    //            Card = card;
    //            To = to;
    //            From = card.Group;
    //            Idx = idx;
    //            FaceUp = faceUp;
    //            IsScriptMove = isScriptMove;
    //        }

    //        public MoveCard(Player who, Card card, int x, int y, int idx, bool faceUp, bool isScriptMove)
    //        {
    //            Who = who;
    //            Card = card;
    //            To = Program.GameEngine.Table;
    //            From = card.Group;
    //            X = x;
    //            Y = y;
    //            Idx = idx;
    //            FaceUp = faceUp;
    //            IsScriptMove = isScriptMove;
    //        }

    //        internal static event EventHandler Done;
    //        internal static event EventHandler Doing;

    //        public override void Do()
    //        {
    //            if (Doing != null) Doing(this, EventArgs.Empty);

    //            if (Card == null) return;

    //            if (To == null)
    //            {
    //                Log.DebugFormat("To == null {0}", Card.Id);
    //                return;
    //            }

    //            if (Card.Group == null)
    //            {
    //                Log.DebugFormat("Card.Group == null {0}", Card.Id);
    //                return;
    //            }

    //            base.Do();
    //#if(DEBUG)
    //            Debug.WriteLine("Moving " + Card.Name + " from " + From + " to " + To);
    //#endif
    //            bool shouldSee = Card.FaceUp, shouldLog = true;
    //            var oldGroup = Card.Group;
    //            var oldIndex = Card.GetIndex();
    //            var oldX = (int)Card.X;
    //            var oldY = (int)Card.Y;
    //            var oldHighlight = Card.HighlightColorString;
    //            var oldMarkers = Card.MarkersString;
    //            // Move the card
    //            if (Card.Group != To)
    //            {
    //                Card.Group.Remove(Card);
    //                if (Card.DeleteWhenLeavesGroup)
    //                    Card.Group = null;
    //                //TODO Card.Delete();
    //                else
    //                {
    //                    Card.CardMoved = true;
    //                    Card.SwitchTo(Who);
    //                    Card.SetFaceUp(FaceUp);//FaceUp may be false - it's one of the constructor parameters for this
    //                    Card.SetOverrideGroupVisibility(false);
    //                    Card.X = X;
    //                    Card.Y = Y;
    //                    To.AddAt(Card, Idx);
    //                    if ((oldGroup != To) || (oldX != X) || (oldY != Y))
    //                    {
    //                        if (IsScriptMove) Program.GameEngine.EventProxy.OnScriptedMoveCard_3_1_0_1(Who, Card, oldGroup, To, oldIndex, Idx, oldX, oldY, X, Y, IsScriptMove, oldHighlight, oldMarkers);
    //                        else Program.GameEngine.EventProxy.OnMoveCard_3_1_0_1(Who, Card, oldGroup, To, oldIndex, Idx, oldX, oldY, X, Y, IsScriptMove, oldHighlight, oldMarkers);

    //                    } Program.GameEngine.EventProxy.OnMoveCard_3_1_0_0(Who, Card, oldGroup, To, oldIndex, Idx, oldX, oldY, X, Y, IsScriptMove);
    //                }
    //            }
    //            else
    //            {
    //                shouldLog = false;
    //                if ((Card.X != X) || (Card.Y != Y)) Card.CardMoved = true;
    //                Card.X = X;
    //                Card.Y = Y;
    //                if (To.Cards.IndexOf(Card) != Idx)
    //                {
    //                    if (To.Ordered)
    //                        Program.GameMess.PlayerEvent(Who, "reorders {0}", To);
    //                    Card.SetIndex(Idx);
    //                }
    //                if ((oldGroup != To) || (oldX != X) || (oldY != Y))
    //                {
    //                    if (IsScriptMove) Program.GameEngine.EventProxy.OnScriptedMoveCard_3_1_0_1(Who, Card, oldGroup, To, oldIndex, Idx, oldX, oldY, X, Y, IsScriptMove, oldHighlight, oldMarkers);
    //                    else Program.GameEngine.EventProxy.OnMoveCard_3_1_0_1(Who, Card, oldGroup, To, oldIndex, Idx, oldX, oldY, X, Y, IsScriptMove, oldHighlight, oldMarkers);

    //                } Program.GameEngine.EventProxy.OnMoveCard_3_1_0_0(Who, Card, oldGroup, To, oldIndex, Idx, oldX, oldY, X, Y, IsScriptMove);
    //            }
    //            // Should the card be named in the log ?
    //            shouldSee |= Card.FaceUp;
    //            // Prepare the message
    //            if (shouldLog)
    //                Program.GameMess.PlayerEvent(Who, "moves '{0}' to {2}{1}",
    //                                         shouldSee ? Card.Type : (object)"Card",
    //                                         To, To is Pile && Idx > 0 && Idx + 1 == To.Count ? "the bottom of " : "");

    //            if (Done != null) Done(this, EventArgs.Empty);
    //        }
    //    }

    internal class MoveCards : ActionBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal Card[] Cards;
        internal bool[] FaceUp;
        internal Group From;
        internal int[] Idx;
        internal Group To;
        internal Player Who;
        //        private int fromIdx;
        internal int[] X, Y;
        internal bool Raw;

        internal bool IsScriptMove;
        internal static event EventHandler Done;
        internal static event EventHandler Doing;

        public MoveCards(Player who, Card[] cards, Group to, int[] idx, bool[] faceUp, bool isScriptMove, bool raw = false)
        {
            Who = who;
            Cards = cards;
            To = to;
            From = cards[0].Group;
            Idx = idx;
            FaceUp = faceUp;
            IsScriptMove = isScriptMove;
            Raw = raw;
            X = new int[cards.Length];
            Y = new int[cards.Length];
        }

        public MoveCards(Player who, Card[] card, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove, bool raw = false)
        {
            Who = who;
            Cards = card;
            To = Program.GameEngine.Table;
            From = card[0].Group;
            X = x;
            Y = y;
            Idx = idx;
            FaceUp = faceUp;
            IsScriptMove = isScriptMove;
            Raw = raw;
        }

        public override void Do()
        {
            if (Doing != null) Doing(this, EventArgs.Empty);

            if (Cards == null) return;
            if (Cards.Length == 0)
                return;

            if (To == null)
            {
                Log.DebugFormat("To == null");
                return;
            }

            if (Cards.Any(x => x.Group == null))
            {
                Log.DebugFormat("Card.Group == null");
                return;
            }

            base.Do();
#if(DEBUG)
            Debug.WriteLine("Moving " + Cards.Length + " cards from " + From + " to " + To);
#endif
            var iindex = 0;
            var cardstomove = new List<CardMoveData>();
            foreach (var card in Cards)
            {
                bool shouldSee = card.FaceUp, shouldLog = true;
                var oldGroup = card.Group;
                var oldIndex = card.GetIndex();
                var oldX = (int)card.X;
                var oldY = (int)card.Y;
                var oldHighlight = card.HighlightColorString;
                var oldMarkers = card.MarkersString;
                // Move the card
                if (card.Group != To)
                {
                    card.Group.Remove(card);
                    if (card.DeleteWhenLeavesGroup)
                        card.Group = null;
                    //TODO Card.Delete();
                    else
                    {
                        card.CardMoved = true;
                        card.SwitchTo(Who);
                        card.SetFaceUp(FaceUp[iindex]);//FaceUp may be false - it's one of the constructor parameters for this
                        card.SetOverrideGroupVisibility(false);
                        card.X = X[iindex];
                        card.Y = Y[iindex];
                        To.AddAt(card, Idx[iindex]);
                        if ((oldGroup != To) || (oldX != X[iindex]) || (oldY != Y[iindex]))
                        {
                            if (IsScriptMove) Program.GameEngine.EventProxy.OnScriptedMoveCard_3_1_0_1(Who, card, oldGroup, To, oldIndex, Idx[iindex], oldX, oldY, X[iindex], Y[iindex], IsScriptMove, oldHighlight, oldMarkers);
                            else Program.GameEngine.EventProxy.OnMoveCard_3_1_0_1(Who, card, oldGroup, To, oldIndex, Idx[iindex], oldX, oldY, X[iindex], Y[iindex], IsScriptMove, oldHighlight, oldMarkers);
                            cardstomove.Add(new CardMoveData()
                            {
                                Card = card,
                                Player = Who,
                                From = oldGroup,
                                Index = Idx[iindex],
                                OldHighlight = oldHighlight,
                                OldIndex = oldIndex,
                                OldMarkers = oldMarkers,
                                OldX = oldX,
                                OldY = oldY,
                                To = To,
                                X = X[iindex],
                                Y = Y[iindex]
                            });
                        }
                        Program.GameEngine.EventProxy.OnMoveCard_3_1_0_0(Who, card, oldGroup, To, oldIndex, Idx[iindex], oldX, oldY, X[iindex], Y[iindex], IsScriptMove);
                    }
                }
                else
                {
                    shouldLog = false;
                    if ((card.X != X[iindex]) || (card.Y != Y[iindex])) card.CardMoved = true;
                    card.X = X[iindex];
                    card.Y = Y[iindex];
                    if (To.Cards.IndexOf(card) != Idx[iindex])
                    {
                        if (To.Ordered)
                            Program.GameMess.PlayerEvent(Who, "reorders {0}", To);
                        card.SetIndex(Idx[iindex]);
                    }
                    if ((oldGroup != To) || (oldX != X[iindex]) || (oldY != Y[iindex]))
                    {
                        if (IsScriptMove) Program.GameEngine.EventProxy.OnScriptedMoveCard_3_1_0_1(Who, card, oldGroup, To, oldIndex, Idx[iindex], oldX, oldY, X[iindex], Y[iindex], IsScriptMove, oldHighlight, oldMarkers);
                        else Program.GameEngine.EventProxy.OnMoveCard_3_1_0_1(Who, card, oldGroup, To, oldIndex, Idx[iindex], oldX, oldY, X[iindex], Y[iindex], IsScriptMove, oldHighlight, oldMarkers);
                        cardstomove.Add(new CardMoveData()
                        {
                            Card = card,
                            Player = Who,
                            From = oldGroup,
                            Index = Idx[iindex],
                            OldHighlight = oldHighlight,
                            OldIndex = oldIndex,
                            OldMarkers = oldMarkers,
                            OldX = oldX,
                            OldY = oldY,
                            To = To,
                            X = X[iindex],
                            Y = Y[iindex]
                        });
                    }
                    Program.GameEngine.EventProxy.OnMoveCard_3_1_0_0(Who, card, oldGroup, To, oldIndex, Idx[iindex], oldX, oldY, X[iindex], Y[iindex], IsScriptMove);
                }
                // Should the card be named in the log ?
                shouldSee |= card.FaceUp;
                // Prepare the message
                if (shouldLog)
                    Program.GameMess.PlayerEvent(Who, "moves '{0}' to {2}{1}",
                                             shouldSee ? card.Type : (object)"Card",
                                             To, To is Pile && Idx[iindex] > 0 && Idx[iindex] + 1 == To.Count ? "the bottom of " : "");
                iindex++;

            }

            {
                var cards = new Card[cardstomove.Count];
                var oldGroups = new Group[cardstomove.Count];
                var tos = new Group[cardstomove.Count];
                var oldIndexes = new int[cardstomove.Count];
                var indexes = new int[cardstomove.Count];
                var oldX = new int[cardstomove.Count];
                var oldY = new int[cardstomove.Count];
                var x = new int[cardstomove.Count];
                var y = new int[cardstomove.Count];
                var oldHighlights = new string[cardstomove.Count];
                var oldMarkers = new string[cardstomove.Count];

                for (var i = 0; i < cardstomove.Count; i++)
                {
                    var c = cardstomove[i];
                    cards[i] = c.Card;
                    oldGroups[i] = c.From;
                    tos[i] = c.To;
                    oldIndexes[i] = c.OldIndex;
                    indexes[i] = c.Index;
                    oldX[i] = c.OldX;
                    oldY[i] = c.OldY;
                    x[i] = c.X;
                    y[i] = c.Y;
                    oldHighlights[i] = c.OldHighlight;
                    oldMarkers[i] = c.OldMarkers;
                }

                Program.GameEngine.EventProxy.OnMoveCards_3_1_0_0(Who, cards, oldGroups, tos, oldIndexes, indexes, oldX, oldY, x, y, oldHighlights, oldMarkers, IsScriptMove);
				if(IsScriptMove)
					Program.GameEngine.EventProxy.OnScriptedMoveCards_3_1_0_1(Who, cards, oldGroups, tos, oldIndexes, indexes, oldX, oldY, x, y, oldHighlights, oldMarkers, IsScriptMove);
				else
					Program.GameEngine.EventProxy.OnMoveCards_3_1_0_1(Who, cards, oldGroups, tos, oldIndexes, indexes, oldX, oldY, x, y, oldHighlights, oldMarkers, IsScriptMove);
            }

            if (Done != null) Done(this, EventArgs.Empty);
        }

        private class CardMoveData
        {
            public Player Player { get; set; }
            public Card Card { get; set; }
            public Group To { get; set; }
            public Group From { get; set; }
            public int OldIndex { get; set; }
            public int Index { get; set; }
            public int OldX { get; set; }
            public int OldY { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string OldHighlight { get; set; }
            public string OldMarkers { get; set; }


        }
    }
}