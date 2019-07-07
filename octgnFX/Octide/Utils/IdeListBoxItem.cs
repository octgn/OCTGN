using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Octide
{
    public abstract class IdeListBoxItemBase : ViewModelBase, ICloneable
    {
        private bool _canEdit = true;
        private bool _canRemove = true;
        private bool _canCopy = true;
        private bool _canInsert = true;
        private bool _canDragDrop = true;
        private bool _isDefault = false;
        private bool _isVisible = true;
        private string _icon = null;
        public object Parent;

        public RelayCommand RemoveCommand { get; set; }
        public RelayCommand CopyCommand { get; set; }
        public RelayCommand InsertCommand { get; set; }

        public ObservableCollection<IdeListBoxItemBase> ItemSource { get; set; }


        public IdeListBoxItemBase()
        {
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
        }

        public virtual void Remove()
        {
            if (CanRemove == false) return;
            ItemSource.Remove(this);
        }

        public virtual void Copy()
        {

        }

        public virtual void Insert()
        {

        }

        public virtual object Clone()
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

        public bool IsDefault
        {
            get
            {
                return _isDefault;
            }
            set
            {
                if (_isDefault == value) return;
                _isDefault = value;
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
