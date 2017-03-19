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
        

        public bool ClickHand
        {
            get
            {
                return (Selection == Hand);
            }
            set
            {
                Selection = Hand;
                RaisePropertyChanged("Selection");
            }
        }

        public void AddPile()
        {
            var ret = new GroupItemModel();
            Piles.Add(ret);
            Selection = ret;
            RaisePropertyChanged("Groups");
            RaisePropertyChanged("SelectedGroup");
        }

        public object _selection;

        public object Selection
        {
            get
            {
                return _selection;
            }
            set
            {
                if (value == _selection) return;
                _selection = value;
                if (value is GroupItemModel)
                {

                    var vm = ViewModelLocator.GroupViewModel;
                    vm.SelectedItem = (GroupItemModel)value;
                    ActiveView = vm;
                }
                else if (value is CounterItemModel)
                {
                    var vm = ViewModelLocator.CounterViewModel;
                    vm.SelectedItem = (CounterItemModel)value;
                    ActiveView = vm;
                }
                else
                {
                    ActiveView = null;
                }
                RaisePropertyChanged("Selection");
                RaisePropertyChanged("ActiveView");
                RaisePropertyChanged("ClickHand");
            }
        }
        
        public void AddCounter()
        {
            var ret = new CounterItemModel();
            Counters.Add(ret);
            Selection = ret;
            RaisePropertyChanged("Counters");
            RaisePropertyChanged("SelectedCounter");
        }

    }
}