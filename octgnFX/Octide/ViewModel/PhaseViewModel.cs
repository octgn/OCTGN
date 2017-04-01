using System;
using System.Windows;
using System.Linq;
using System.ComponentModel;
using System.Windows.Media;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Octide.Messages;

namespace Octide.ViewModel
{
    public class PhaseViewModel : ViewModelBase
    {
        private PhaseItemModel _selectedItem;
        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();
                
        public RelayCommand DeleteCommand { get; private set; }

        public PhaseViewModel()
        {
            DeleteCommand = new RelayCommand(DeleteItem);
        }
        
        public void DeleteItem()
        {
            if (SelectedItem == null) return;
            ViewModelLocator.PreviewTabViewModel.Phases.Remove(SelectedItem);
        }
        
        public PhaseItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
                RaisePropertyChanged("DefaultVisibility");
            }
        }
    }

    public class PhaseItemModel : ViewModelBase, ICloneable
    {
        public GamePhase _phase;
        
        public PhaseItemModel()
        {
            _phase = new GamePhase();
            _phase.Icon = ViewModelLocator.SizeViewModel.Images.First().FullPath;
            _phase.Name = "Phase";
            RaisePropertyChanged("Icon");
            RaisePropertyChanged("IconImage");
        }

        public PhaseItemModel(GamePhase p)
        {
            _phase = p;
        }

        public PhaseItemModel(PhaseItemModel p)
        {
            _phase = new GamePhase();
            _phase.Icon = p.Icon.FullPath;
            _phase.Name = p.Name;
        }

        public object Clone()
        {
            return new PhaseItemModel(this);
        }
        
        
        public string Name
        {
            get
            {
                return _phase.Name;
            }
            set
            {
                if (_phase.Name == value) return;
                _phase.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public Asset Icon
        {
            get
            {
                if (_phase.Icon == null)
                    return new Asset();
                return Asset.Load(_phase.Icon);
            }
            set
            {
                if (value == null) return;
                if (!File.Exists(value.FullPath)) return;
                _phase.Icon = value.FullPath;
                RaisePropertyChanged("Icon");
                RaisePropertyChanged("IconImage");
            }
        }

        public string IconImage => _phase.Icon;

    }
}