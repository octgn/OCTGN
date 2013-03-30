﻿namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public interface IMultiCard : ICard
    {
        int Quantity { get; set; }
    }

    public class MultiCard : Card,IMultiCard
    {
        public int Quantity { get; set; }
    }

    public class ObservableMultiCard : IMultiCard, INotifyPropertyChanged
    {
        private Guid id;

        private Guid setId;

        private string name;

        private string imageUri;

        private Guid alternate;

        private string dependent;

        private bool isMutable;

        private IDictionary<PropertyDef, object> properties;

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

        public Guid Alternate
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
            }
        }

        public string Dependent
        {
            get
            {
                return this.dependent;
            }
            set
            {
                if (this.dependent == value) return;
                this.dependent = value;
                OnPropertyChanged("Dependent");
            }
        }

        public bool IsMutable
        {
            get
            {
                return this.isMutable;
            }
            set
            {
                if (this.isMutable == value) return;
                this.isMutable = value;
                OnPropertyChanged("IsMutable");
            }
        }

        public IDictionary<PropertyDef, object> Properties
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

        public event PropertyChangedEventHandler PropertyChanged;
        internal void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}