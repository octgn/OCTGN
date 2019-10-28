using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.Messages;
using Octide.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.ItemModel
{

    public class PackPropertyItemModel : ViewModelBase, ICloneable
    {
        public ObservableCollection<PackPropertyItemModel> ParentCollection { get; set; }
        public PickProperty PropertyDef { get; set; }
        public PropertyItemViewModel _activeProperty;
        public RelayCommand RemoveCommand { get; private set; }

        public ObservableCollection<IdeListBoxItemBase> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;

        public PackPropertyItemModel() // new item
        {
            PropertyDef = new PickProperty();
            ActiveProperty = CustomProperties.First() as PropertyItemViewModel;
            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public PackPropertyItemModel(PickProperty p) // loading item
        {
            PropertyDef = p;
            ActiveProperty = CustomProperties.FirstOrDefault(x => (x as PropertyItemViewModel)._property == PropertyDef.Property) as PropertyItemViewModel;
            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public PackPropertyItemModel(PackPropertyItemModel p) // copy item
        {
            PropertyDef = new PickProperty();
            ActiveProperty = p.ActiveProperty;
            PropertyDef.Value = p.PropertyDef.Value;
            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public object Clone()
        {
            return new PackPropertyItemModel(this) { ParentCollection = ParentCollection };
        }

        public void CustomPropertyChanged(CustomPropertyChangedMessage args)
        {
            if (ActiveProperty == args.Prop)
            {
                PropertyDef.Property = args.Prop._property;
                RaisePropertyChanged("ActiveProperty");
            }
        }

        public void Remove()
        {
            if (ParentCollection.Count > 1)
                ParentCollection.Remove(this);
        }

        public PropertyItemViewModel ActiveProperty
        {
            get
            {
                return _activeProperty;
            }
            set
            {
                if (_activeProperty == value) return;
                if (value == null)
                {
                    value = CustomProperties.First() as PropertyItemViewModel;
                }
                _activeProperty = value;
                PropertyDef.Property = value._property;
                RaisePropertyChanged("ActiveProperty");
            }
        }

        public string Value
        {
            get
            {
                return PropertyDef.Value;
            }
            set
            {
                if (PropertyDef.Value == value) return;
                PropertyDef.Value = value;
                RaisePropertyChanged("Value");
            }
        }
    }
}
