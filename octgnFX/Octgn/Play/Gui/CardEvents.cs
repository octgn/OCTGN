using System.Windows;
using System.Collections.Generic;
using Octgn.Data;

namespace Octgn.Play.Gui
{
    public class CardEventArgs : RoutedEventArgs
    {
        public CardEventArgs(RoutedEvent routedEvent, object src)
            : base(routedEvent, src)
        { }

        public CardEventArgs(Card card, RoutedEvent routedEvent, object src)
            : base(routedEvent, src)
        { Card = card; }

        public CardEventArgs(CardModel model, RoutedEvent routedEvent, object src)
            : base(routedEvent, src)
        { CardModel = model; }

        public readonly Card Card;
        public readonly CardModel CardModel;
        public Vector MouseOffset { get; set; }
    }

    public class CardsEventArgs : RoutedEventArgs
    {
        public CardsEventArgs(Card card, IEnumerable<Card> cards, RoutedEvent routedEvent, object src)
            : base(routedEvent, src)
        {
            ClickedCard = card;
            Cards = cards;
        }

        public readonly Card ClickedCard;
        public readonly IEnumerable<Card> Cards;
        public IInputElement Handler { get; set; }
        public Vector MouseOffset { get; set; }
        public bool? FaceUp { get; set; }
        public bool CanDrop { get; set; }
        public Size CardSize { get; set; }
        internal List<CardDragAdorner> Adorners { get; set; }
    }

    public delegate void CardEventHandler(object sender, CardEventArgs e);
    public delegate void CardsEventHandler(object sender, CardsEventArgs e);
}