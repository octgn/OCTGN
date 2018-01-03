using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.Messages;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octide.ItemModel
{
    public class PackageItemModel : IdeListBoxItemBase, ICloneable
    {
        public Pack _pack;
        public ObservableCollection<IBasePack> _items;


        public PackageItemModel() //new item
        {
            _pack = new Pack();
            _pack.Name = "Package";
            _pack.Id = Guid.NewGuid();
            _pack.Includes = new List<Include>(); //todo add include support
            _pack.SetId = ViewModelLocator.SetTabViewModel.SelectedItem.Id;
            _pack.Definition = new PackDefinition();
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
            Items = new ObservableCollection<IBasePack>();
        }

        public PackageItemModel(Pack p) //loading item
        {
            _pack = p;
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                _pack.Definition.Items = Items.Select(x => x.packItem).ToList();
            };
            foreach (var item in p.Definition.Items)
            {
                if (item is OptionsList)
                    Items.Add(new PackOptionsItemModel(item) { ParentCollection = Items });
                else if (item is Pick)
                    Items.Add(new PackPickItemModel(item) { ParentCollection = Items });
            }
        }

        public PackageItemModel(PackageItemModel p) //copying item
        {
            _pack = new Pack();
            _pack.Name = p.Name;
            _pack.Id = Guid.NewGuid();
            _pack.Includes = new List<Include>(); //todo add include support
            _pack.SetId = p._pack.SetId;
            _pack.Definition = new PackDefinition();
            Parent = p.Parent;
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                _pack.Definition.Items = Items.Select(x => x.packItem).ToList();
            };
            foreach (var packItem in p.Items)
            {
                if (packItem is PackPickItemModel)
                {
                    Items.Add(new PackPickItemModel(packItem as PackPickItemModel) { ParentCollection = Items });
                }
                else if (packItem is PackOptionsItemModel)
                {
                    Items.Add(new PackOptionsItemModel(packItem as PackOptionsItemModel) { ParentCollection = Items });
                }
            }
        }

        public ObservableCollection<IBasePack> Items
        {
            get { return _items; }
            set { Set(ref _items, value); }
        }
        
        public string Name
        {
            get
            {
                return _pack.Name;
            }
            set
            {
                if (_pack.Name == value) return;
                _pack.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public object Clone()
        {
            return new PackageItemModel(this);
        }

        public void Copy()
        {
            if (CanCopy == false) return;
            var index = (Parent as SetItemModel).PackItems.IndexOf(this);
            (Parent as SetItemModel).PackItems.Insert(index, Clone() as PackageItemModel);
        }

        public void Remove()
        {
            if (CanRemove == false) return;
            (Parent as SetItemModel).PackItems.Remove(this);
        }

        public void Insert()
        {
            if (CanInsert == false) return;
            var index = (Parent as SetItemModel).PackItems.IndexOf(this);
            (Parent as SetItemModel).PackItems.Insert(index, new PackageItemModel() { Parent = Parent});
        }
    }


    public interface IBasePack
    {
        IPackItem packItem { get; set; }
    }

    public partial class PackOptionsItemModel : ViewModelBase, IBasePack, ICloneable
    {
        public ObservableCollection<IBasePack> ParentCollection { get; set; }
        public IPackItem packItem { get; set; }
        public ObservableCollection<PackOptionItemModel> _items;
        public RelayCommand AddOptionCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }

        public PackOptionsItemModel() // new item
        {
            packItem = new OptionsList();

            Items = new ObservableCollection<PackOptionItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                (packItem as OptionsList).Options = Items.Select(x => x.option).ToList();
            };
            AddOptionCommand = new RelayCommand(AddOption);
            RemoveCommand = new RelayCommand(Remove);
        }

        public PackOptionsItemModel(IPackItem p) // load item
        {
            packItem = p;
            Items = new ObservableCollection<PackOptionItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                (packItem as OptionsList).Options = Items.Select(x => x.option).ToList();
            };
            foreach (var item in (packItem as OptionsList).Options)
            {
                Items.Add(new PackOptionItemModel(item) { ParentCollection = Items });
            }
            AddOptionCommand = new RelayCommand(AddOption);
            RemoveCommand = new RelayCommand(Remove);

        }

        public PackOptionsItemModel(PackOptionsItemModel p) // copy item
        {
            packItem = new OptionsList();
            Items = new ObservableCollection<PackOptionItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                (packItem as OptionsList).Options = Items.Select(x => x.option).ToList();
            };
            foreach (var item in p.Items)
            {
                Items.Add(new PackOptionItemModel(item) { ParentCollection = Items });
            }
            AddOptionCommand = new RelayCommand(AddOption);
            RemoveCommand = new RelayCommand(Remove);
        }

        public void AddOption()
        {
            Items.Add(new PackOptionItemModel() { ParentCollection = Items });
        }

        public void Remove()
        {
            ParentCollection.Remove(this);
        }

        public object Clone()
        {
            return new PackOptionsItemModel(this) { ParentCollection = ParentCollection };
        }

        public ObservableCollection<PackOptionItemModel> Items
        {
            get
            {
                return _items;
            }
            set
            {
                if (_items == value) return;
                _items = value;
                RaisePropertyChanged("Items");
            }
        }
    }

    public class PackOptionItemModel : ViewModelBase, ICloneable
    {
        public ObservableCollection<PackOptionItemModel> ParentCollection { get; set; }
        public Option option { get; set; }
        public RelayCommand AddPickCommand { get; private set; }
        public RelayCommand AddOptionsCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }

        public ObservableCollection<IBasePack> _items;

        public PackOptionItemModel() // new item
        {
            option = new Option();
            option.Definition = new PackDefinition();
            Items = new ObservableCollection<IBasePack>();

            Items.CollectionChanged += (a, b) =>
            {
                option.Definition.Items = Items.Select(x => x.packItem).ToList();
            };
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            RemoveCommand = new RelayCommand(Remove);
        }

        public PackOptionItemModel(Option o) // load item
        {
            option = o;
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                option.Definition.Items = Items.Select(x => x.packItem).ToList();
            };
            foreach (var item in o.Definition.Items)
            {
                if (item is OptionsList)
                    Items.Add(new PackOptionsItemModel(item) { ParentCollection = Items });
                else if (item is Pick)
                    Items.Add(new PackPickItemModel(item) { ParentCollection = Items });
            }
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            RemoveCommand = new RelayCommand(Remove);
        }

        public PackOptionItemModel(PackOptionItemModel p) // copy item
        {
            option = new Option();
            option.Definition = new PackDefinition();
            option.Probability = p.Probability;
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                option.Definition.Items = Items.Select(x => x.packItem).ToList();
            };
            foreach (var packItem in p.Items)
            {
                if (packItem is PackPickItemModel)
                    Items.Add(new PackPickItemModel(packItem as PackPickItemModel) { ParentCollection = Items });
                if (packItem is PackOptionsItemModel)
                    Items.Add(new PackOptionsItemModel(packItem as PackOptionsItemModel) { ParentCollection = Items });
            }
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            RemoveCommand = new RelayCommand(Remove);
        }

        public object Clone()
        {
            return new PackOptionItemModel(this) { ParentCollection = ParentCollection };
        }

        public void Remove()
        {
            ParentCollection.Remove(this);
        }

        public void AddPick()
        {
            Items.Add(new PackPickItemModel() { ParentCollection = Items });
        }

        public void AddOptions()
        {
            Items.Add(new PackOptionsItemModel() { ParentCollection = Items });
        }

        public double Probability
        {
            get
            {
                return option.Probability;
            }
            set
            {
                if (option.Probability == value) return;
                option.Probability = value;
                RaisePropertyChanged("Probability");
            }
        }

        public ObservableCollection<IBasePack> Items
        {
            get
            {
                return _items;
            }
            set
            {
                if (_items == value) return;
                _items = value;
                RaisePropertyChanged("Items");
            }
        }
    }

    public partial class PackPickItemModel : ViewModelBase, IBasePack, ICloneable
    {
        public ObservableCollection<IBasePack> ParentCollection { get; set; }
        public IPackItem packItem { get; set; }
        public ObservableCollection<PackPropertyItemModel> _items;
        public bool _isUnlimited;
        public RelayCommand AddPropertyCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }


        public PackPickItemModel() // new item
        {
            packItem = new Pick();
            (packItem as Pick).Properties = new List<PickProperty>();
            Items = new ObservableCollection<PackPropertyItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                (packItem as Pick).Properties = Items.Select(x => x.PropertyDef).ToList();
            };
            Items.Add(new PackPropertyItemModel() { ParentCollection = Items });
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);
        }

        public PackPickItemModel(IPackItem p) // load item
        {
            packItem = p;
            IsUnlimited = (p as Pick).Quantity == -1 ? true : false;
            Items = new ObservableCollection<PackPropertyItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                (packItem as Pick).Properties = Items.Select(x => x.PropertyDef).ToList();
            };
            foreach (var item in (packItem as Pick).Properties)
            {
                Items.Add(new PackPropertyItemModel(item) { ParentCollection = Items });
            }
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);

        }

        public PackPickItemModel(PackPickItemModel p) // copy item
        {
            packItem = new Pick();
            Quantity = p.Quantity;
            Items = new ObservableCollection<PackPropertyItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                (packItem as Pick).Properties = Items.Select(x => x.PropertyDef).ToList();
            };
            foreach (var item in p.Items)
            {
                Items.Add(new PackPropertyItemModel(item) { ParentCollection = Items });
            }
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);
        }

        public void AddProperty()
        {
            Items.Add(new PackPropertyItemModel() { ParentCollection = Items });
        }

        public object Clone()
        {
            return new PackPickItemModel(this) { ParentCollection = ParentCollection };
        }

        public void Remove()
        {
            ParentCollection.Remove(this);
        }

        public bool IsUnlimited
        {
            get
            {
                return _isUnlimited;
            }
            set
            {
                if (_isUnlimited == value) return;
                _isUnlimited = value;
                (packItem as Pick).Quantity = (value == true) ? -1 : 0;
                RaisePropertyChanged("IsUnlimited");
                RaisePropertyChanged("Quantity");
            }

        }

        public int Quantity
        {
            get
            {
                return (packItem as Pick).Quantity;
            }
            set
            {
                if ((packItem as Pick).Quantity == value) return;
                if (value < 0)
                {
                    IsUnlimited = true;
                }
                else
                {
                    IsUnlimited = false;
                    (packItem as Pick).Quantity = value;
                }
                RaisePropertyChanged("Quantity");
            }
        }

        public ObservableCollection<PackPropertyItemModel> Items
        {
            get
            {
                return _items;
            }
            set
            {
                if (_items == value) return;
                _items = value;
                RaisePropertyChanged("Items");
            }
        }
    }


    public class PackPropertyItemModel : ViewModelBase, ICloneable
    {
        public ObservableCollection<PackPropertyItemModel> ParentCollection { get; set; }
        public PickProperty PropertyDef { get; set; }
        public PropertyItemModel _activeProperty;
        public RelayCommand RemoveCommand { get; private set; }

        public ObservableCollection<PropertyItemModel> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;

        public PackPropertyItemModel() // new item
        {
            PropertyDef = new PickProperty();
            ActiveProperty = CustomProperties.First();
            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public PackPropertyItemModel(PickProperty p) // loading item
        {
            PropertyDef = p;
            ActiveProperty = CustomProperties.FirstOrDefault(x => x.Name == PropertyDef.Name);
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

        public void CustomPropertyChanged(CustomPropertyChangedMessage m)
        {
            var prop = m.Prop;
            if (ActiveProperty == prop)
            {
                PropertyDef.Name = prop.Name;
                RaisePropertyChanged("ActiveProperty");
            }
        }

        public void Remove()
        {
            if (ParentCollection.Count > 1)
                ParentCollection.Remove(this);
        }

        public PropertyItemModel ActiveProperty
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
                    value = CustomProperties.First();
                }
                _activeProperty = value;
                PropertyDef.Name = value.Name;
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
