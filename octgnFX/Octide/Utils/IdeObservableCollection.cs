// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace Octide
{
    public delegate void NotifyDefaultChangedEventHandler(object sender, NotifyDefaultChangedEventArgs e);
    public delegate void NotifySelectedItemChangedEventHandler(object sender, NotifySelectedItemChangedEventArgs e);

    public class IdeCollection<T> : ObservableCollection<T>, IDropTarget
    {
        public object Parent { get; private set; }

        public Type[] RestrictedTypes { get; private set; }
        public IdeCollection() : base()
        {
            Parent = null;
            RestrictedTypes = new Type[0];
        }
        public IdeCollection(object parent) : base()
        {
            Parent = parent;
        }
        public IdeCollection(object parent, params Type[] types) : base()
        {
            Parent = parent;
            RestrictedTypes = types;
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
                OnSelectedItemChanged(new NotifySelectedItemChangedEventArgs(oldItem, newItem));
                OnPropertyChanged(new PropertyChangedEventArgs("SelectedItem"));
            }
        }
        protected virtual void OnSelectedItemChanged(NotifySelectedItemChangedEventArgs e)
        {
            SelectedItemChanged?.Invoke(this, e);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (RestrictedTypes.Length > 0 && !RestrictedTypes.Contains(dropInfo.Data.GetType()))
            {
                dropInfo.Effects = DragDropEffects.None;
            }
            else
            {
                DragDrop.DefaultDropHandler.DragOver(dropInfo);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            DragDrop.DefaultDropHandler.Drop(dropInfo);
        }

        public void DragEnter(IDropInfo dropInfo) { }
        public void DragLeave(IDropInfo dropInfo) { }
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
