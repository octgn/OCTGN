using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Octgn.Core
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (Set<T>(ref field, value)) {
                Notify(propertyName);

                return true;
            }

            return false;
        }

        public bool Set<T>(ref T field, T value) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;

                return true;
            }

            return false;
        }

        public void Notify([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
