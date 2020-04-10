// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octide
{
    public delegate void NotifyDefaultChangedEventHandler(object sender, NotifyDefaultChangedEventArgs e);
    public delegate void NotifySelectedItemChangedEventHandler(object sender, NotifySelectedItemChangedEventArgs e);

    public class IdeCollection<T> : ObservableCollection<T>
    {
        public object Parent { get; private set; }
        public IdeCollection(): base()
        {
            Parent = null;
        }
        public IdeCollection(object parent): base()
        {
            Parent = parent;
        }

        public virtual event NotifyDefaultChangedEventHandler DefaultItemChanged;
        public virtual event NotifySelectedItemChangedEventHandler SelectedItemChanged;

        private T defaultItem;
        public T DefaultItem
        {
            get
            {
                return defaultItem;
            }
            set
            {
                if (defaultItem != null && defaultItem.Equals(value)) return;
                var oldItem = defaultItem;
                var newItem = value;
                defaultItem = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DefaultItem"));
                OnDefaultItemChanged(new NotifyDefaultChangedEventArgs(oldItem, newItem));
            }
        }

        protected virtual void OnDefaultItemChanged(NotifyDefaultChangedEventArgs e)
        {
            DefaultItemChanged?.Invoke(this, e);
        }

        private T selectedItem;
        public T SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                if (selectedItem != null && selectedItem.Equals(value)) return;
                var oldItem = selectedItem;
                var newItem = value;
                selectedItem = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SelectedItem"));
                OnSelectedItemChanged(new NotifySelectedItemChangedEventArgs(oldItem, newItem));
            }
        }
        protected virtual void OnSelectedItemChanged(NotifySelectedItemChangedEventArgs e)
        {
            SelectedItemChanged?.Invoke(this, e);
        }
    }


    public class NotifyDefaultChangedEventArgs : EventArgs
    {
        private readonly object oldItem;
        private readonly object newItem;
        public NotifyDefaultChangedEventArgs(object oldItem, object newItem)
        {
            this.oldItem = oldItem;
            this.newItem = newItem;
        }

        public virtual object OldItem
        {
            get
            {
                return oldItem;
            }
        }

        public virtual object NewItem
        {
            get
            {
                return newItem;
            }
        }
    }
    public class NotifySelectedItemChangedEventArgs : EventArgs
    {
        private readonly object oldItem;
        private readonly object newItem;
        public NotifySelectedItemChangedEventArgs(object oldItem, object newItem)
        {
            this.oldItem = oldItem;
            this.newItem = newItem;
        }

        public virtual object OldItem
        {
            get
            {
                return oldItem;
            }
        }

        public virtual object NewItem
        {
            get
            {
                return newItem;
            }
        }
    }

}
