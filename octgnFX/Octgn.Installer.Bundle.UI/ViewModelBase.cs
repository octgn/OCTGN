using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Octgn.Installer.Bundle.UI
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<T>(ref T field, T value) {
            if (field == null && value == null) return false;

            if (object.Equals(field, value)) return false;

            field = value;

            return true;
        }

        protected void Notify([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetAndNotify<T>(ref T field, T value, [CallerMemberName]string propertyName = null) {
            if(Set(ref field, value)) {
                Notify(propertyName);
            }
        }
    }
}
