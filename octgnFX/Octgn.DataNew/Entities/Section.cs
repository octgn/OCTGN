namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;

    public interface ISection
    {
        string Name { get; }
        int Quantity { get; }
        bool Shared { get; }
        IEnumerable<IMultiCard> Cards { get; } 
    }

    public class Section : ISection
    {
        public string Name { get; set; }
        public int Quantity {
            get
            {
                if (Cards == null) return 0;
                return Cards.Sum(x=>x.Quantity);
            }
        }

        public bool Shared { get; set; }
        public IEnumerable<IMultiCard> Cards { get; set; }
    }

    public class ObservableSection : ISection,INotifyPropertyChanged
    {
        private string name;

        private ObservableCollection<ObservableMultiCard> cards;

        private bool shared;

        private void CardsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add || args.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (ObservableMultiCard i in args.NewItems)
                {
                    i.PropertyChanged += this.CardOnPropertyChanged;
                }
            }
            this.OnPropertyChanged("Cards");
            this.OnPropertyChanged("Quantity");
        }

        private void CardOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            this.OnPropertyChanged("Cards");
                this.OnPropertyChanged("Quantity");
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (this.name == value) return;
                this.name = value;
                OnPropertyChanged("Name");
            }
        }
        
        public int Quantity {
            get
            {
                return Cards.Sum(x => x.Quantity);
            }
        }

        public bool Shared
        {
            get
            {
                return this.shared;
            }
            set
            {
                if (this.shared == value) return;
                this.shared = value;
                OnPropertyChanged("Shared");
            }
        }

        public IEnumerable<IMultiCard> Cards
        {
            get
            {
                return this.cards;
            }
            set
            {
                if (this.cards == value) return;
                this.cards = new ObservableCollection<ObservableMultiCard>
                    (value
                    .Select(x=>
                        {
                            var ret = new ObservableMultiCard
                                          {
                                              Id = x.Id,
                                              Name = x.Name,
                                              Properties =
                                                  x.Properties.ToDictionary(z => z.Key, y => y.Value),
                                              ImageUri = x.ImageUri,
                                              Alternate = x.Alternate,
                                              SetId = x.SetId,
                                              Quantity = x.Quantity
                                          };
                            ret.PropertyChanged += this.CardOnPropertyChanged;
                            return ret;
                        }));
                cards.CollectionChanged += CardsOnCollectionChanged;
                OnPropertyChanged("Cards");
                this.OnPropertyChanged("Quantity");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}