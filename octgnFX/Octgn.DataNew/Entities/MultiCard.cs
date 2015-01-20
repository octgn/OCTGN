namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public interface IMultiCard : ICard
    {
        int Quantity { get; set; }
    }

    public class MultiCard : Card, IMultiCard
    {
        public int Quantity { get; set; }

        public MultiCard(Guid id, Guid setId, string name, string imageuri, string alternate, CardSize size, IDictionary<string, CardPropertySet> properties, int quantity)
            : base(id, setId, name, imageuri, alternate, size, properties)
        {
            Quantity = quantity;
        }

        public MultiCard(ICard card, int quantity)
            : base(card.Id, card.SetId, card.Name.Clone() as string, card.ImageUri.Clone() as string, card.Alternate.Clone() as string, card.Size.Clone() as CardSize, card.CloneProperties())
        {
            Quantity = quantity;
        }

        public MultiCard(IMultiCard card)
            : this(card, card.Quantity)
        {

        }
    }

    public class ObservableMultiCard : IMultiCard, INotifyPropertyChanged
    {
        private Guid id;

        private Guid setId;

        private string name;

        private string imageUri;

        private string alternate;

        private CardSize size;

        private IDictionary<string, CardPropertySet> properties;

        private int quantity;

        public Guid Id
        {
            get
            {
                return this.id;
            }
            set
            {
                if (this.id == value) return;
                this.id = value;
                OnPropertyChanged("Id");
            }
        }

        public Guid SetId
        {
            get
            {
                return this.setId;
            }
            set
            {
                if (this.setId == value) return;
                this.setId = value;
                OnPropertyChanged("SetId");
            }
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

        public string ImageUri
        {
            get
            {
                return this.imageUri;
            }
            set
            {
                if (this.imageUri == value) return;
                this.imageUri = value;
                OnPropertyChanged("ImageUri");
            }

        }

        public string Alternate
        {
            get
            {
                return this.alternate;
            }
            set
            {
                if (this.alternate == value) return;
                this.alternate = value;
                OnPropertyChanged("Alternate");
                this.OnPropertyChanged("Properties");
            }
        }

        public IDictionary<string, CardPropertySet> Properties
        {
            get
            {
                return this.properties;
            }
            set
            {
                if (this.properties == value) return;
                this.properties = value;
                OnPropertyChanged("Properties");
            }
        }

        public int Quantity
        {
            get
            {
                return this.quantity;
            }
            set
            {
                if (this.quantity == value) return;
                this.quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

        public CardSize Size
        {
            get
            {
                return this.size;
            }
            set
            {
                if (this.size == value) return;
                this.size = value;
                OnPropertyChanged("Size");
            }
        }

        public ObservableMultiCard(Guid id, Guid setId, string name, string imageuri, string alternate, CardSize size, IDictionary<string, CardPropertySet> properties, int quantity)
        {
            Quantity = quantity;
            Id = id;
            SetId = setId;
            Name = name.Clone() as string;
            ImageUri = imageuri.Clone() as string;
            Alternate = alternate.Clone() as string;
            Size = size.Clone() as CardSize;
            Properties = properties;
            this.properties = this.CloneProperties();
        }

        public ObservableMultiCard(ICard card, int quantity)
            : this(card.Id, card.SetId, card.Name.Clone() as string, card.ImageUri.Clone() as string, card.Alternate.Clone() as string, card.Size.Clone() as CardSize, card.CloneProperties(),quantity)
        {
            Quantity = quantity;
        }

        public ObservableMultiCard(IMultiCard card)
            : this(card, card.Quantity)
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}