using System.Windows.Controls;

namespace Octgn.DeckBuilder
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Shapes;

    using Octgn.Annotations;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew.Entities;

    /// <summary>
    /// Interaction logic for DeckEditorPreviewControl.xaml
    /// </summary>
    public partial class DeckEditorPreviewControl : INotifyPropertyChanged
    {
        private CardViewModel card;

        public static readonly DependencyProperty GameProperty =
            DependencyProperty.Register("Game", typeof(Game), typeof(DeckEditorPreviewControl), new PropertyMetadata(default(Game)));

        public Game Game
        {
            get
            {
                return (Game)GetValue(GameProperty);
            }
            set
            {
                var g = new Game() { Name = "No Game Selected", CardBack = "pack://application:,,,/Resources/Back.jpg" };
                if (value != null)
                {
                    g = value;
                }
                SetValue(GameProperty, g);
                OnPropertyChanged("Card");
                OnPropertyChanged("IsCardSelected");
            }
        }

        public CardViewModel Card
        {
            get
            {
                return this.card;
            }
            set
            {
                if (this.card == value) return;
                this.card = value;
                OnPropertyChanged("Card");
                OnPropertyChanged("NoCardSelected");
            }
        }

        public DeckEditorPreviewControl()
        {
            Game = new Game() { Name = "No Game Selected", CardBack = "pack://application:,,,/Resources/Back.jpg" };
            Card = new CardViewModel();
            //Card = new CardViewModel(new Card() { ImageUri = "pack://application:,,,/Resources/Back.jpg" });

            InitializeComponent();
        }

        public void SetGame(Game game)
        {
            Game = game;
            Card.SetCard(null);
        }

        private void BackArrowMouseUp(object sender, MouseButtonEventArgs e)
        {
            Card.Index--;
        }

        private void ForwardArrowMouseUp(object sender, MouseButtonEventArgs e)
        {
            Card.Index++;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged

        public class CardViewModel : INotifyPropertyChanged
        {
            private Card card;

            private int index;

            public Card Card
            {
                get
                {
                    return this.card;
                }
                set
                {
                    if (this.card == value) return;
                    this.card = value;
                    this.OnPropertyChanged("Card");
                    this.OnPropertyChanged("CardUri");
                    this.OnPropertyChanged("HasAlternates");
                    this.OnPropertyChanged("AlternateCount");
                    this.OnPropertyChanged("NoCardSelected");
                }
            }

            public string CardUri
            {
                get
                {
                    if (Card == null) return null;
                    var ret = Card.GetPicture();
                    return ret;
                }
            }

            public bool HasAlternates
            {
                get
                {
                    if (Card == null) return false;
                    return Card.Properties.Count != 1;
                }
            }

            public bool NoCardSelected
            {
                get
                {
                    return Card == null;
                }
            }

            public int AlternateCount
            {
                get
                {
                    if (Card == null) return 0;
                    return Card.Properties.Count;
                }
            }

            public int Index
            {
                get
                {
                    return index;
                }
                set
                {
                    if (value == index) return;
                    if (value < 0)
                        index = AlternateCount - 1;
                    else if (value >= AlternateCount) index = 0;
                    else index = value;

                    Card.SetPropertySet(Card.Properties.ToArray()[index].Key);

                    for (var i = 0; i < Alternates.Count; i++)
                    {
                        if (i != index) Alternates[i].Selected = false;
                        else
                        {
                            Alternates[i].Selected = true;
                        }
                    }

                    this.OnPropertyChanged("Index");
                    this.OnPropertyChanged("CardUri");
                }
            }

            public ObservableCollection<Alternate> Alternates { get; set; }

            public CardViewModel()
            {
                Alternates = new ObservableCollection<Alternate>();
            }

            public void SetCard(Card c)
            {
                Card = c;
                Alternates.Clear();
                if (Card == null) return;
                var i = 0;
                foreach (var a in c.Properties)
                {
                    Alternates.Add(new Alternate(this,a.Key, i));
                    i++;
                }
                Index = 0;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
        public class Alternate : INotifyPropertyChanged
        {
            private CardViewModel VM;

            public string Name { get; set; }

            public int Index { get; set; }

            public bool Selected
            {
                get
                {
                    return VM.Index == Index;
                }
                set
                {
                    this.OnPropertyChanged("Selected");
                    if (value.Equals(VM.Index == Index))
                    {
                        return;
                    }
                    VM.Index = this.Index;
                }
            }

            public Alternate(CardViewModel vm, string altName, int altIndex)
            {
                VM = vm;
                Name = altName;
                Index = altIndex;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
