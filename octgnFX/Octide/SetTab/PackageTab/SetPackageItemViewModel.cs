using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Octgn.Core.DataExtensionMethods;
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
    public class SetPackageItemViewModel : IdeListBoxItemBase
    {
        public Pack _pack;
        public ObservableCollection<IBasePack> _items;

        public RelayCommand AddOptionsCommand { get; private set; }
        public RelayCommand AddPickCommand { get; private set; }
        public RelayCommand GeneratePackCommand { get; private set; }
        public PackageDropHandler PackageDropHandler { get; set; }
        public ObservableCollection<string> _boosterCards;

        public SetPackageItemViewModel() //new item
        {
            _pack = new Pack
            {
                Name = "Package",
                Id = Guid.NewGuid(),
                Includes = new List<Include>(), //todo add include support
                Definition = new PackDefinition()
            };
            Items = new ObservableCollection<IBasePack>();
            PackageDropHandler = new PackageDropHandler();
            GeneratePackCommand = new RelayCommand(GeneratePack);
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
        }

        public SetPackageItemViewModel(Pack p) //loading item
        {
            _pack = p;
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                _pack.Definition.Items = Items.Select(x => x.PackItem).ToList();
            };
            foreach (var item in p.Definition.Items)
            {
                if (item is OptionsList)
                    Items.Add(new PackOptionsItemModel(item) { ParentCollection = Items });
                else if (item is Pick)
                    Items.Add(new PackPickItemModel(item) { ParentCollection = Items });
            }
            PackageDropHandler = new PackageDropHandler();
            GeneratePackCommand = new RelayCommand(GeneratePack);
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
        }

        public SetPackageItemViewModel(SetPackageItemViewModel p) //copying item
        {
            _pack = new Pack
            {
                Name = p.Name,
                Id = Guid.NewGuid(),
                Set = p._pack.Set,
                Definition = new PackDefinition(),
                Includes = new List<Include>() //todo add include support
            };
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                _pack.Definition.Items = Items.Select(x => x.PackItem).ToList();
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
            ItemSource = p.ItemSource;
            Parent = p.Parent;
            PackageDropHandler = new PackageDropHandler();
            GeneratePackCommand = new RelayCommand(GeneratePack);
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
        }

        public void AddPick()
        {
            Items.Add(new PackPickItemModel() { ParentCollection = Items });
        }

        public void AddOptions()
        {
            Items.Add(new PackOptionsItemModel() { ParentCollection = Items });
        }

        public void GeneratePack()
        {
            BoosterCards = new ObservableCollection<string>(_pack.GenerateContent().LimitedCards.Select(x => x.GetPicture()));
        }

        public ObservableCollection<IBasePack> Items
        {
            get { return _items; }
            set { Set(ref _items, value); }
        }

        public ObservableCollection<string> BoosterCards
        {
            get
            {
                return _boosterCards;
            }
            set
            {
                if (_boosterCards == value) return;
                _boosterCards = value;
                RaisePropertyChanged("BoosterCards");
            }
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

        public Set Set
        {
            get
            {
                return (Parent as SetItemViewModel)._set;
            }
        }

        public override object Clone()
        {
            return new SetPackageItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as SetPackageItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new SetPackageItemViewModel() { ItemSource = ItemSource, Parent = Parent});
        }
    }

    public class PackageDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && dropInfo.TargetItem is PackPropertyItemModel)
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
            else if (dropInfo.DragInfo.SourceItem is PackPropertyItemModel && dropInfo.DragInfo.SourceCollection.TryGetList().Count <= 1 && !dropInfo.KeyStates.HasFlag(System.Windows.DragDropKeyStates.ControlKey))
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
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
    }

    public interface IBasePack
    {
        IPackItem PackItem { get; set; }
    }
}
