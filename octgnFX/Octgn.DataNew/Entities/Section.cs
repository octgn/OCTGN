namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public interface ISection
    {
        string Name { get; }
        ICollection<IMultiCard> Cards { get; } 
    }

    public class Section : ISection
    {
        public string Name { get; set; }
        public ICollection<IMultiCard> Cards { get; set; }
    }

    public class ObservableSection : ISection,INotifyPropertyChanged
    {
        private string name;

        private ObservableCollection<IMultiCard> cards;

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

        public ICollection<IMultiCard> Cards
        {
            get
            {
                return this.cards;
            }
            set
            {
                if (this.cards == value) return;
                this.cards = new ObservableCollection<IMultiCard>(value);
                OnPropertyChanged("Cards");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}