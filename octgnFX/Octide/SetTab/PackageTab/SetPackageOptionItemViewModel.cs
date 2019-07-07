using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class PackOptionItemModel : ViewModelBase, ICloneable
    {
        public ObservableCollection<PackOptionItemModel> ParentCollection { get; set; }
        public Option Option { get; set; }
        public RelayCommand AddPickCommand { get; private set; }
        public RelayCommand AddOptionsCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }

        public ObservableCollection<IBasePack> _items;

        public PackOptionItemModel() // new item
        {
            Option = new Option
            {
                Definition = new PackDefinition()
            };
            Items = new ObservableCollection<IBasePack>();

            Items.CollectionChanged += (a, b) =>
            {
                Option.Definition.Items = Items.Select(x => x.PackItem).ToList();
            };
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            RemoveCommand = new RelayCommand(Remove);
        }

        public PackOptionItemModel(Option o) // load item
        {
            Option = o;
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                Option.Definition.Items = Items.Select(x => x.PackItem).ToList();
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
            Option = new Option
            {
                Definition = new PackDefinition(),
                Probability = p.Probability
            };
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                Option.Definition.Items = Items.Select(x => x.PackItem).ToList();
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
                return Option.Probability;
            }
            set
            {
                if (Option.Probability == value) return;
                Option.Probability = value;
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
}
