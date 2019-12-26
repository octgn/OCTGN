// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

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
        public new SetItemViewModel Parent;

        public Pack _pack;
        public ObservableCollection<IBasePack> Items { get; private set; }
        public ObservableCollection<IdeListBoxItemBase> Includes { get; private set; }

        public RelayCommand AddOptionsCommand { get; private set; }
        public RelayCommand AddPickCommand { get; private set; }
        public RelayCommand GeneratePackCommand { get; private set; }
        public RelayCommand AddIncludeCommand { get; private set; }
        public PackageDropHandler PackageDropHandler { get; set; }
        public IncludeDropHandler IncludeDropHandler { get; set; }

        public ObservableCollection<string> _boosterCards;

        public SetPackageItemViewModel() //new item
        {
            _pack = new Pack
            {
                Name = "Package",
                Id = Guid.NewGuid(),
                Definition = new PackDefinition()
            };
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                _pack.Definition.Items = Items.Select(x => x.PackItem).ToList();
            };
            Includes = new ObservableCollection<IdeListBoxItemBase>();
            Includes.CollectionChanged += (a, b) =>
            {
                _pack.Includes = Includes.Select(x => (x as PackIncludeItemModel)._include).ToList();
            };
            PackageDropHandler = new PackageDropHandler();
            IncludeDropHandler = new IncludeDropHandler();
            GeneratePackCommand = new RelayCommand(GeneratePack);
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            AddIncludeCommand = new RelayCommand(AddInclude);
        }

        public SetPackageItemViewModel(Pack p) //loading item
        {
            _pack = p;
            Items = new ObservableCollection<IBasePack>();
            foreach (var item in p.Definition.Items)
            {
                if (item is OptionsList)
                    Items.Add(new PackOptionsItemModel(item) { ParentCollection = Items });
                else if (item is Pick)
                    Items.Add(new PackPickItemModel(item) { ParentCollection = Items });
            }
            Items.CollectionChanged += (a, b) =>
            {
                _pack.Definition.Items = Items.Select(x => x.PackItem).ToList();
            };

            Includes = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var include in p.Includes)
            {
                Includes.Add(new PackIncludeItemModel(include) { Parent = this, ItemSource = Includes });
            }       
            Includes.CollectionChanged += (a, b) =>
            {
                _pack.Includes = Includes.Select(x => (x as PackIncludeItemModel)._include).ToList();
            };

            PackageDropHandler = new PackageDropHandler();
            IncludeDropHandler = new IncludeDropHandler();
            GeneratePackCommand = new RelayCommand(GeneratePack);
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            AddIncludeCommand = new RelayCommand(AddInclude);
        }

        public SetPackageItemViewModel(SetPackageItemViewModel p) //copying item
        {
            _pack = new Pack
            {
                Name = p.Name,
                Id = Guid.NewGuid(),
                Set = p._pack.Set,
                Definition = new PackDefinition(),
                Includes = new List<Include>()
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
            Includes = new ObservableCollection<IdeListBoxItemBase>();
            Includes.CollectionChanged += (a, b) =>
            {
                _pack.Includes = Includes.Select(x => (x as PackIncludeItemModel)._include).ToList();
            };
            foreach (PackIncludeItemModel include in p.Includes)
            {
                Includes.Add(new PackIncludeItemModel(include) { Parent = this, ItemSource = Includes });
            }
            ItemSource = p.ItemSource;
            Parent = p.Parent;
            PackageDropHandler = new PackageDropHandler();
            IncludeDropHandler = new IncludeDropHandler();
            GeneratePackCommand = new RelayCommand(GeneratePack);
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            AddIncludeCommand = new RelayCommand(AddInclude);
        }

        public void AddInclude()
        {
            Includes.Add(new PackIncludeItemModel(this) { ItemSource = Includes});
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
                return _pack.Set;
            }
            set
            {
                if (_pack.Set == value) return;
                _pack.Set = value;
                RaisePropertyChanged("Set");
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
            ItemSource.Insert(index, new SetPackageItemViewModel() { ItemSource = ItemSource, Parent = Parent, Set = Parent._set });
        }
    }

    public class PackageDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.VisualSource != dropInfo.VisualTarget)
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
            else if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && dropInfo.TargetItem is PackPropertyItemModel)
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

    public class IncludeDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.VisualSource != dropInfo.VisualTarget)
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
            else if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && dropInfo.TargetItem is PackPropertyItemModel)
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
