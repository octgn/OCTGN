using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.Messages;

namespace Octide.ViewModel
{
    public class CardViewModel : ViewModelBase
    {
        private double _x;
        private double _y;

        public double X
        {
            get { return _x; }
            set
            {
                if (_x == value) return;
                _x = value;
                RaisePropertyChanged("X");
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                if (_y == value) return;
                _y = value;
                RaisePropertyChanged("Y");
            }
        }

        public int CardWidth
        {
            get
            {
                return ViewModelLocator.GameLoader.ValidGame ? ViewModelLocator.GameLoader.Game.CardWidth : 50;
            }
        }

        public int CardHeight
        {
            get
            {
                return ViewModelLocator.GameLoader.ValidGame ? ViewModelLocator.GameLoader.Game.CardHeight : 50;
            }
        }

        public string CardBack
        {
            get
            {
                return ViewModelLocator.GameLoader.ValidGame ? ViewModelLocator.GameLoader.Game.CardBack: "";
            }
        }

        public CardViewModel()
        {
            Messenger.Default.Register<CardDetailsChangedMessage>(this, x => this.RefreshValues());
            Messenger.Default.Register<PropertyChangedMessage<Game>>(this, x => this.RefreshValues());
        }

        internal void RefreshValues()
        {
            RaisePropertyChanged("");
        }
    }
}