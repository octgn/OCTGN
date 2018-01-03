using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.Messages;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Octide.ItemModel
{
    public class PropertyItemModel : IdeListBoxItemBase, ICloneable
    {
        public PropertyDef PropertyDef { get; set; }
        private Guid _id;

        public PropertyItemModel()
        {
            PropertyDef = new PropertyDef();
            _id = Guid.NewGuid();
            Name = "Property";
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
        }

        public PropertyItemModel(PropertyDef p)
        {
            PropertyDef = p;
            _id = Guid.NewGuid();
            if (p.Name == "Name")
                Visibility = Visibility.Collapsed;
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
        }

        public PropertyItemModel(PropertyItemModel p)
        {
            PropertyDef = p.PropertyDef.Clone() as PropertyDef;
            _id = Guid.NewGuid();
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
        }

        public object Clone()
        {
            return new PropertyItemModel(this);
        }
        
        public void Copy()
        {
            if (CanCopy == false) return;
            var index = ViewModelLocator.PropertyTabViewModel.Items.IndexOf(this);
            ViewModelLocator.PropertyTabViewModel.Items.Insert(index, Clone() as PropertyItemModel);
        }

        public void Remove()
        {
            if (CanRemove == false) return;
            ViewModelLocator.PropertyTabViewModel.Items.Remove(this);
        }

        public void Insert()
        {
            if (CanInsert == false) return;
            var index = ViewModelLocator.PropertyTabViewModel.Items.IndexOf(this);
            ViewModelLocator.PropertyTabViewModel.Items.Insert(index, new PropertyItemModel());
        }

        public Guid Id
        {
            get
            {
                return _id;
            }
        }

        public string Name
        {
            get
            {
                return PropertyDef.Name;
            }
            set
            {
                if (value == PropertyDef.Name) return;
                PropertyDef.Name = value;
                RaisePropertyChanged("Name");
                Messenger.Default.Send(new CustomPropertyChangedMessage() { Prop = this });
            }
        }

        public PropertyType Type
        {
            get
            {
                return PropertyDef.Type;
            }
            set
            {
                if (value == PropertyDef.Type) return;
                PropertyDef.Type = value;
                RaisePropertyChanged("Type");
            }
        }

        public PropertyTextKind TextKind
        {
            get
            {
                return PropertyDef.TextKind;
            }
            set
            {
                if (value == PropertyDef.TextKind) return;
                PropertyDef.TextKind = value;
                RaisePropertyChanged("TextKind");
            }
        }

        public bool Hidden
        {

            get
            {
                return PropertyDef.Hidden;
            }
            set
            {
                if (value == PropertyDef.Hidden) return;
                PropertyDef.Hidden = value;
                RaisePropertyChanged("Hidden");
            }
        }
        public bool IgnoreText
        {

            get
            {
                return PropertyDef.IgnoreText;
            }
            set
            {
                if (value == PropertyDef.IgnoreText) return;
                PropertyDef.IgnoreText = value;
                RaisePropertyChanged("IgnoreText");
            }
        }
    }
}
