using System.ComponentModel;

namespace Octgn.Data
{
    public class GameSettings : INotifyPropertyChanged
    {
        private readonly bool _initialized;
        private bool _useTwoSidedTable = true;

        public GameSettings()
        {
            _initialized = true;
        }

        public bool UseTwoSidedTable
        {
            get { return _useTwoSidedTable; }
            set
            {
                if (value != _useTwoSidedTable)
                {
                    _useTwoSidedTable = value;
                    if (_initialized)
                        OnPropertyChanged("UseTwoSidedTable");
                }
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