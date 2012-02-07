using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.ComponentModel;

namespace Octgn.Data
{
    public class GameSettings : INotifyPropertyChanged
    {
        private bool _useTwoSidedTable = true;
        private bool _initialized = false;

        public GameSettings()
        {
            _initialized = true;
        }

        public bool UseTwoSidedTable
        {
            get
            {
                return _useTwoSidedTable;
            }
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

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
