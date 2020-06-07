using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Octgn.Core
{
    public class Model : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetAndNotify<T>(ref T field, T value, [CallerMemberName]string propertyName = null) {
            var wasSet = Set<T>(ref field, value, propertyName);

            if (wasSet)
                Notify(propertyName);

            return wasSet;
        }

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (ReferenceEquals(field, value)) return false;

            field = value;

            return true;
        }

        protected virtual void Notify([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
