using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Scripting.Utils;


namespace Octgn.Play.Actions
{
    using System.Reflection;

    using log4net;
    
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
                var oldFaceUp = card.FaceUp;
                var oldFilter = card.FilterColorString;
                var oldAlternate = card.Alternate();

                // Move the card
                if (card.Group == To)
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
                }
                else
                {
                    card.Group.Remove(card);
                    if (card.DeleteWhenLeavesGroup) //nonpersisting cards will be deleted
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
                    }
                }

                if ((oldGroup != To) || (oldX != X[iindex]) || (oldY != Y[iindex]) || (oldIndex != Idx[iindex]))
                {
                    cardstomove.Add(new CardMoveData()
                    {
                        Card = card,
                        Player = Who,
                        From = oldGroup,
                        To = To,
                        Index = Idx[iindex],
                        OldHighlight = oldHighlight,
                        OldIndex = oldIndex,
                        OldMarkers = oldMarkers,
                        OldFaceUp = oldFaceUp,
                        OldFilter = oldFilter,
                        OldAlternate = oldAlternate,
                        OldX = oldX,
                        OldY = oldY,
                        X = X[iindex],
                        Y = Y[iindex]
                    });
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
                var oldFaceUps = new bool[cardstomove.Count];
                var oldFilters = new string[cardstomove.Count];
                var oldAlternates = new string[cardstomove.Count];

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
                    oldFaceUps[i] = c.OldFaceUp;
                    oldFilters[i] = c.OldFilter;
                    oldAlternates[i] = c.OldAlternate;

                    Program.GameEngine.EventProxy.OnMoveCard_3_1_0_0(Who, c.Card, c.From, c.To, c.OldIndex, c.Index, c.OldX, c.OldY, c.X, c.Y, IsScriptMove);

                    if (IsScriptMove)
                    {
                        Program.GameEngine.EventProxy.OnScriptedMoveCard_3_1_0_1(Who, c.Card, c.From, c.To, c.OldIndex, c.Index, c.OldX, c.OldY, c.X, c.Y, c.OldFaceUp, c.OldHighlight, c.OldMarkers);
                    }
                    else
                    {
                        Program.GameEngine.EventProxy.OnMoveCard_3_1_0_1(Who, c.Card, c.From, c.To, c.OldIndex, c.Index, c.OldX, c.OldY, c.X, c.Y, c.OldFaceUp, c.OldHighlight, c.OldMarkers);
                    }
                }

                if (cardstomove.Count > 0)
                {
                    Program.GameEngine.EventProxy.OnMoveCards_3_1_0_0(Who, cards, oldGroups, tos, oldIndexes, indexes, oldX, oldY, x, y, oldHighlights, oldMarkers, IsScriptMove);
                    if (IsScriptMove)
                    {
                        Program.GameEngine.EventProxy.OnScriptedMoveCards_3_1_0_1(Who, cards, oldGroups, tos, oldIndexes, indexes, oldX, oldY, x, y, oldHighlights, oldMarkers, oldFaceUps);
                        Program.GameEngine.EventProxy.OnScriptedCardsMoved_3_1_0_2(Who, cards, oldGroups, tos, oldIndexes, oldX, oldY, oldHighlights, oldMarkers, oldFaceUps, oldFilters, oldAlternates);
                    }
                    else
                    {
                        Program.GameEngine.EventProxy.OnMoveCards_3_1_0_1(Who, cards, oldGroups, tos, oldIndexes, indexes, oldX, oldY, x, y, oldHighlights, oldMarkers, oldFaceUps);
                        Program.GameEngine.EventProxy.OnCardsMoved_3_1_0_2(Who, cards, oldGroups, tos, oldIndexes, oldX, oldY, oldHighlights, oldMarkers, oldFaceUps, oldFilters, oldAlternates);
                    }
                }
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
            public bool OldFaceUp { get; set; }
            public string OldFilter { get; set; }
            public string OldAlternate { get; set; }


        }
    }
}