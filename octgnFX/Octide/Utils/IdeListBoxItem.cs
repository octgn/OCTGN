using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Octide
{
    public class IdeListBoxItemBase : ViewModelBase
    {
        private bool _canEdit = true;
        private bool _canRemove = true;
        private bool _canCopy = true;
        private bool _canInsert = true;
        private bool _canDragDrop = true;
        private Visibility _visibility = Visibility.Visible;

        public object Parent;

        public RelayCommand RemoveCommand { get; set; }
        public RelayCommand CopyCommand { get; set; }
        public RelayCommand InsertCommand { get; set; }

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

        public Visibility Visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                if (_visibility == value) return;
                _visibility = value;
            }
        }
        
    }
}
