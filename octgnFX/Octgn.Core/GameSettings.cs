using System.ComponentModel;

namespace Octgn.Data
{
    public class GameSettings : INotifyPropertyChanged
    {
        private readonly bool _initialized;
        private bool _useTwoSidedTable = true;
        private bool _hideBoard = false;
        private bool _allowSpectators = false;

        public GameSettings()
        {
            _initialized = true;
            _allowSpectators = true;
        }

        public bool UseTwoSidedTable
        {
            get { return _useTwoSidedTable; }
            set
            {
                if (value == _useTwoSidedTable) return;
                _useTwoSidedTable = value;
                if (_initialized)
                    OnPropertyChanged("UseTwoSidedTable");
            }
        }

        public bool HideBoard
        {
            get { return _hideBoard; }
            set
            {
                if (value == _hideBoard) return;
                _hideBoard = value;
                if (_initialized)
                {
                    OnPropertyChanged("HideBoard");
                    OnPropertyChanged("ShowBoard");
                }
            }
        }

        public bool AllowSpectators
        {
            get { return _allowSpectators; }
			set
			{
			    if (value == _allowSpectators)
			        return;
			    _allowSpectators = value;
                if (_initialized)
                {
                    OnPropertyChanged("AllowSpectators");
                }
			}
        }

        public bool ShowBoard
        {
            get
            {
                return HideBoard == false;
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}