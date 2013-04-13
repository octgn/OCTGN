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
        IEnumerable<IMultiCard> Cards { get; } 
    }

    public class Section : ISection
    {
        public string Name { get; set; }
        public int Quantity {
            get
            {
                if (Cards == null) return 0;
                return Cards.Count();
            }
        }
        public IEnumerable<IMultiCard> Cards { get; set; }
    }

    public class ObservableSection : ISection,INotifyPropertyChanged
    {
        private string name;

        private ObservableCollection<ObservableMultiCard> cards;

        private void CardsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
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
                return cards.Count;
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
                    .Select(x=>new ObservableMultiCard
                                   {
                                       Id = x.Id,
                                       Name = x.Name,
                                       Properties = x.Properties.ToDictionary(z => z.Key, y => y.Value),
                                       ImageUri = x.ImageUri,
                                       IsMutable = x.IsMutable,
                                       Alternate = x.Alternate,
                                       SetId = x.SetId,
                                       Dependent = x.Dependent,
                                       Quantity = x.Quantity
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