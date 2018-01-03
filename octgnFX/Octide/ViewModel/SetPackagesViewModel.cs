using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octgn.DataNew.Entities;
using Octgn.Core.DataExtensionMethods;
using Octide.Messages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using Octgn.Library;
using GongSolutions.Wpf.DragDrop;
using Octide.ItemModel;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace Octide.ViewModel
{
    public class SetPackagesViewModel : ViewModelBase
    {                
        public SetPackagesViewModel()
        {
            PackageDropHandler = new PackageDropHandler();
            AddPackCommand = new RelayCommand(AddPack);
            GeneratePackCommand = new RelayCommand(GeneratePack);
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
        }
        
        public PackageDropHandler PackageDropHandler { get; set; }
        private Visibility _packPanelVisibility;
        private PackageItemModel _selectedItem;
        private SetItemModel _parentSet;
        public ObservableCollection<string> _boosterCards;


        public RelayCommand AddPackCommand { get; private set; }
        public RelayCommand AddOptionsCommand { get; private set; }
        public RelayCommand AddPickCommand { get; private set; }
        public RelayCommand GeneratePackCommand { get; private set; }
        
        public SetItemModel ParentSet
        {
            get
            {
                return _parentSet;
            }
            set
            {
                if (_parentSet == value) return;
                _parentSet = value;
                RaisePropertyChanged("Items");
                SelectedItem = Items?.FirstOrDefault();
            }
        }

        public ObservableCollection<PackageItemModel> Items => ParentSet?.PackItems;

        public PackageItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;

                PackPanelVisibility = value == null ? Visibility.Hidden : Visibility.Visible;

                RaisePropertyChanged("SelectedItem");
            }
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

        public Visibility PackPanelVisibility
        {
            get { return _packPanelVisibility; }
            set { Set(ref _packPanelVisibility, value); }
        }
        
        public void AddPack()
        {
            var ret = new PackageItemModel() { Parent = ParentSet};
            Items.Add(ret);
            SelectedItem = ret;
            RaisePropertyChanged("SelectedPack");
        }
                
        public void AddPick()
        {
            SelectedItem.Items.Add(new PackPickItemModel() { ParentCollection = SelectedItem.Items });
        }

        public void AddOptions()
        {
            SelectedItem.Items.Add(new PackOptionsItemModel() { ParentCollection = SelectedItem.Items });
        }
        
        public void GeneratePack()
        {
            BoosterCards = new ObservableCollection<string>(SelectedItem._pack.CrackOpen().LimitedCards.Select(x => x.GetPicture()));
        }

        public static IBasePack LoadPackItem(IPackItem p)
        {
            IBasePack pack = null;
            if (p is OptionsList)
                pack = new PackOptionsItemModel(p);
            else if (p is Pick)
                pack = new PackPickItemModel(p);
            return pack;
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
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
        }
    }

}