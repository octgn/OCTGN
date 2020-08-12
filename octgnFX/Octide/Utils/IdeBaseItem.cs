// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Linq;

namespace Octide
{
    public interface IDroppable
    {
        bool CanAccept(object item);
    }

    public abstract class IdeBaseItem : ViewModelBase, ICloneable
    {
        private bool _canEdit = true;
        private bool _canRemove = true;
        private bool _canCopy = true;
        private bool _canInsert = true;
        private bool _canDragDrop = true;
        private bool _canBeDefault = false;
        private bool _isVisible = true;
        private string _icon = null;

        public RelayCommand RemoveCommand { get; set; }
        public RelayCommand CopyCommand { get; set; }
        public RelayCommand InsertCommand { get; set; }
        public RelayCommand MakeDefaultCommand { get; set; }
        public IdeCollection<IdeBaseItem> Source { get; set; }

        public IdeBaseItem(IdeCollection<IdeBaseItem> src)
        {
            Source = src;
            Source.DefaultItemChanged += (sender, args) =>
            {
                if (args.OldItem == this)
                {
                    RaisePropertyChanged("IsDefault");
                }
            };
            RemoveCommand = new RelayCommand(RemoveItem);
            CopyCommand = new RelayCommand(CopyItem);
            InsertCommand = new RelayCommand(InsertItem);
            MakeDefaultCommand = new RelayCommand(MakeDefault);
        }

        public void SetSource(IdeCollection<IdeBaseItem> src) 
        {
            Source = src;
            Source.DefaultItemChanged += (sender, args) =>
            {
                if (args.OldItem == this)
                {
                    RaisePropertyChanged("IsDefault");
                }
            };
        }

        public void RemoveItem()
        {
            if (CanRemove == false) return;
            if (Source == null) return;
            var index = Source.IndexOf(this);
            Source.Remove(this);
            if (Source.Count > 0)
                Source.SelectedItem = (Source.Count > index) ? Source[index] : Source.Last();
            Cleanup();
        }

        public void CopyItem()
        {
            if (CanCopy == false) return;
            if (Source == null) return;
            var index = Source.IndexOf(this);
            var item = Clone() as IdeBaseItem;
            Source.Insert(index + 1, item);
            Source.SelectedItem = item;
        }

        public void InsertItem()
        {
            if (CanInsert == false) return;
            if (Source == null) return;
            var index = Source.IndexOf(this);
            var item = Create() as IdeBaseItem;
            Source.Insert(index, item);
            Source.SelectedItem = item;
        }

        public void MakeDefault()
        {
            if (CanBeDefault == false) return;
            if (Source == null) return;
            if (Source.DefaultItem == this) return;
            Source.DefaultItem = this;
            RaisePropertyChanged("IsDefault");

        }

        public virtual object Clone()
        {
            return this;
        }

        public virtual object Create()
        {
            return this;
        }

        public bool CanEdit
        {
            get
            {
                return _canEdit;
            }
            set
            {
                if (_canEdit == value) return;
                _canEdit = value;
            }
        }

        public bool CanCopy
        {
            get
            {
                return _canCopy;
            }
            set
            {
                if (_canCopy == value) return;
                _canCopy = value;
            }
        }

        public bool CanRemove
        {
            get
            {
                return _canRemove;
            }
            set
            {
                if (IsDefault) return;
                if (_canRemove == value) return;
                _canRemove = value;
            }
        }

        public bool CanInsert
        {
            get
            {
                return _canInsert;
            }
            set
            {
                if (_canInsert == value) return;
                _canInsert = value;
            }
        }

        public bool CanDragDrop
        {
            get
            {
                return _canDragDrop;
            }
            set
            {
                if (_canDragDrop == value) return;
                _canDragDrop = value;
            }
        }

        public bool CanBeDefault
        {
            get
            {
                return _canBeDefault;
            }
            set
            {
                if (_canBeDefault == value) return;
                _canBeDefault = value;
            }
        }

        public bool IsDefault
        {
            get
            {
                return Source?.DefaultItem == this;
            }
            set
            {
                if (Source == null) return;
                if (Source.DefaultItem == this) return;
                Source.DefaultItem = this;
                RaisePropertyChanged("IsDefault");
            }
        }

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
            }
        }

        public string Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                if (_icon == value) return;
                _icon = value;
            }
        }
    }
}
