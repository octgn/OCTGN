namespace Octgn.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Media.Imaging;

    public class DataGameViewModel : INotifyPropertyChanged
    {
        private bool isSelected;
        private BitmapImage cardBack;

        public event PropertyChangedEventHandler PropertyChanged;

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Version Version { get; private set; }
        public Uri CardBackUri { get; private set; }
        public BitmapImage CardBack { get
        {
            if (cardBack == null)
            {
                cardBack = new BitmapImage();
                cardBack.BeginInit();
                cardBack.CacheOption = BitmapCacheOption.OnLoad;
                cardBack.UriSource = CardBackUri;
                cardBack.EndInit();
            }
            return cardBack;
        }
        }
        public bool IsSelected
        {
            get{return this.isSelected;}
            private set
            {
                this.isSelected = value;
                this.OnPropertyChanged("IsSelected");
            }
        }

        public DataGameViewModel(Data.Game game)
        {
            Id = game.Id;
            Name = game.Name;
            Version = game.Version;
            CardBackUri = game.GetCardBackUri();
            IsSelected = false;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
