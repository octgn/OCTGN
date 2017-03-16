using System.Collections.ObjectModel;

namespace Octide.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;

    using Octgn.DataNew.Entities;

    using Octide.Messages;
    using System.Windows.Controls;
    using Views;
    using GalaSoft.MvvmLight.Command;

    public class PreviewTabViewModel : ViewModelBase
    {
        private Game _game;
        public ObservableCollection<GroupItemModel> Piles { get; set; }
        public ObservableCollection<GroupItemModel> CollapsedPiles => new ObservableCollection<GroupItemModel>(Piles.Where(x => x.Collapsed == true));
        public GroupItemModel Hand { get; set; }
        public ObservableCollection<CounterItemModel> Counters { get; set; }

        public RelayCommand AddPileCommand { get; private set; }
        public RelayCommand AddCounterCommand { get; private set; }

        private ViewModelBase _activeView;

        public ViewModelBase ActiveView
        {
            get { return _activeView; }
            set
            {
                _activeView = value;
                RaisePropertyChanged("ActiveView");
            }
        }
        
        public PreviewTabViewModel()
        {
            _game = ViewModelLocator.GameLoader.Game;
            Hand = _game.Player.Hand == null ? null : new GroupItemModel(_game.Player.Hand);
            Piles = new ObservableCollection<GroupItemModel>(_game.Player.Groups.Select(x => new GroupItemModel(x)));
            Piles.CollectionChanged += (a, b) => {
                _game.Player.Groups = Piles.Select(x => x._group).ToList();
            };
            AddPileCommand = new RelayCommand(AddPile);
            Counters = new ObservableCollection<CounterItemModel>(_game.Player.Counters.Select(x => new CounterItemModel(x)));
            Counters.CollectionChanged += (a, b) =>
            {
                _game.Player.Counters = Counters.Select(x => x._counter).ToList();
            };
            AddCounterCommand = new RelayCommand(AddCounter);
        }

        public void AddPile()
        {
            var ret = new GroupItemModel();
            Piles.Add(ret);
            SelectedGroup = ret;
            RaisePropertyChanged("Groups");
            RaisePropertyChanged("SelectedGroup");
        }
        
        public GroupItemModel _selectedGroup;

        public GroupItemModel SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (value == _selectedGroup) return;
                _selectedGroup = value;
                if (value == null)
                {
                    ActiveView = null;
                }
                else
                {
                    var vm = ViewModelLocator.GroupViewModel;
                    vm.SelectedItem = value;
                    ActiveView = vm;
                }
                RaisePropertyChanged("SelectedGroup");
                RaisePropertyChanged("ActiveView");
            }
        }

        public void AddCounter()
        {
            var ret = new CounterItemModel();
            Counters.Add(ret);
            SelectedCounter = ret;
            RaisePropertyChanged("Counters");
            RaisePropertyChanged("SelectedCounter");
        }

        public CounterItemModel _selectedCounter;

        public CounterItemModel SelectedCounter
        {
            get { return _selectedCounter; }
            set
            {
                if (value == _selectedCounter) return;
                _selectedCounter = value;
                if (value == null)
                {
                    ActiveView = null;
                }
                else
                {
                    var vm = ViewModelLocator.CounterViewModel;
                    vm.SelectedItem = value;
                    ActiveView = vm;
                }
                RaisePropertyChanged("SelectedCounter");
                RaisePropertyChanged("ActiveView");
            }
        }

    }
}