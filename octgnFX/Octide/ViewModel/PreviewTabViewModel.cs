using System.Collections.ObjectModel;

namespace Octide.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
    using GongSolutions.Wpf.DragDrop;
    using System.Windows;
    using Octide.ItemModel;

    public class PreviewTabViewModel : ViewModelBase, IDropTarget
    {
        private Game _game;

        public TableViewModel Table => ViewModelLocator.TableViewModel;
        public ObservableCollection<CardViewModel> Cards { get; set; }


        public ObservableCollection<GroupItemModel> Piles { get; set; }
        public ObservableCollection<GroupItemModel> VisiblePiles => new ObservableCollection<GroupItemModel>(Piles.Where(x => x.Collapsed == false));
        public ObservableCollection<GroupItemModel> CollapsedPiles => new ObservableCollection<GroupItemModel>(Piles.Where(x => x.Collapsed == true));
        public GroupItemModel Hand { get; set; }
        public GroupItemModel TableGroup { get; set; }
        public ObservableCollection<CounterItemModel> Counters { get; set; }
        public ObservableCollection<SizeItemModel> CardSizes { get; set; }
        public SizeItemModel DefaultSize { get; set; }
        public ObservableCollection<BoardItemModel> Boards { get; set; }
        public BoardItemModel DefaultBoard { get; set; }
        public BoardItemModel ActiveBoard { get; set; }
        public ObservableCollection<PhaseItemModel> Phases { get; set; }

        public RelayCommand AddPileCommand { get; private set; }
        public RelayCommand AddCounterCommand { get; private set; }
        public RelayCommand AddSizeCommand { get; private set; }
        public RelayCommand AddBoardCommand { get; private set; }
        public RelayCommand AddPhaseCommand { get; private set; }
        

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
        
        public string Summary
        {
            get { return ViewModelLocator.GameLoader.Game.Player.IndicatorsFormat; }
            set
            {
                if (ViewModelLocator.GameLoader.Game.Player.IndicatorsFormat == value) return;
                ViewModelLocator.GameLoader.Game.Player.IndicatorsFormat = value;
                RaisePropertyChanged("Summary");
            }
        }

        public PreviewTabViewModel()
        {
            _game = ViewModelLocator.GameLoader.Game;
            Hand = _game.Player.Hand == null ? null : new GroupItemModel(_game.Player.Hand);
            TableGroup = new GroupItemModel(_game.Table);
            Piles = new ObservableCollection<GroupItemModel>(_game.Player.Groups.Select(x => new GroupItemModel(x)));
            Piles.CollectionChanged += (a, b) => {
                _game.Player.Groups = Piles.Select(x => x._group).ToList();
                RaisePropertyChanged("CollapsedPiles");
                RaisePropertyChanged("VisiblePiles");
            };
            AddPileCommand = new RelayCommand(AddPile);
            Counters = new ObservableCollection<CounterItemModel>(_game.Player.Counters.Select(x => new CounterItemModel(x)));
            Counters.CollectionChanged += (a, b) =>
            {
                _game.Player.Counters = Counters.Select(x => x._counter).ToList();
            };
            AddCounterCommand = new RelayCommand(AddCounter);
            
            CardSizes = new ObservableCollection<SizeItemModel>(_game.CardSizes.Values.Select(x => new SizeItemModel(x)));
            DefaultSize = CardSizes.FirstOrDefault(x => x.Default);
            CardSizes.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.SizeViewModel.UpdateSizes();
                Messenger.Default.Send(new CardSizeChangedMesssage());
            };
            AddSizeCommand = new RelayCommand(AddSize);

            Phases = new ObservableCollection<PhaseItemModel>(_game.Phases.Select(x => new PhaseItemModel(x)));
            Phases.CollectionChanged += (a, b) =>
            {
                _game.Phases = Phases.Select(x => x._phase).ToList();
            };
            AddPhaseCommand = new RelayCommand(AddPhase);


            Boards = new ObservableCollection<BoardItemModel>(_game.GameBoards.Values.Where(x => x.Name != "Default").Select(x => new BoardItemModel(x)));
            if (_game.GameBoards["Default"].Source != null)
            {
                DefaultBoard = new BoardItemModel(_game.GameBoards["Default"]);
                ActiveBoard = DefaultBoard;
            }
            Boards.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.BoardViewModel.UpdateBoards();
            };
            AddBoardCommand = new RelayCommand(AddBoard);

            Cards = new ObservableCollection<CardViewModel>();
            var card = new CardViewModel();
            card.Size = DefaultSize;
            Cards.Add(card);
            RaisePropertyChanged("Cards");
            
        }
                
        public bool ClickSize
        {
            get
            {
                return (Selection == DefaultSize);
            }
            set
            {
                Selection = DefaultSize;
                RaisePropertyChanged("Selection");
            }
        }

        public bool ClickBoard
        {
            get
            {
                return (Selection == DefaultBoard);
            }
            set
            {
                Selection = DefaultBoard;
                RaisePropertyChanged("Selection");
            }
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

        public bool ClickTable
        {
            get
            {
                return (Selection == Table);
            }
            set
            {
                Selection = Table;
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

        public void AddCounter()
        {
            var ret = new CounterItemModel();
            Counters.Add(ret);
            Selection = ret;
            RaisePropertyChanged("Counters");
            RaisePropertyChanged("SelectedCounter");
        }

        public void AddSize()
        {
            var ret = new SizeItemModel();
            CardSizes.Add(ret);
            Selection = ret;
            RaisePropertyChanged("CardSizes");
            RaisePropertyChanged("SelectedSize");
        }

        public void AddPhase()
        {
            var ret = new PhaseItemModel();
            Phases.Add(ret);
            Selection = ret;
            RaisePropertyChanged("Phases");
            RaisePropertyChanged("SelectedPhase");
        }

        public void AddBoard()
        {
            var ret = new BoardItemModel();
            if (DefaultBoard == null)
            {
                ret._board.Name = "Default";
                DefaultBoard = ret;
                ViewModelLocator.BoardViewModel.UpdateBoards();
                RaisePropertyChanged("DefaultBoard");
            }
            else Boards.Add(ret);
            Selection = ret;
            RaisePropertyChanged("Boards");
            RaisePropertyChanged("SelectedBoard");
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is SizeItemModel)
            {
                dropInfo.Effects = DragDropEffects.Move;

            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is SizeItemModel)
            {
                SizeItemModel item = dropInfo.Data as SizeItemModel;
                var card = new CardViewModel();
                card.Size = item;
                Cards.Add(card);
                RaisePropertyChanged("Cards");
            }
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
                else if (value is SizeItemModel)
                {
                    var vm = ViewModelLocator.SizeViewModel;
                    vm.SelectedItem = (SizeItemModel)value;
                    ActiveView = vm;
                }
                else if (value is PhaseItemModel)
                {
                    var vm = ViewModelLocator.PhaseViewModel;
                    vm.SelectedItem = (PhaseItemModel)value;
                    ActiveView = vm;
                }
                else if (value is BoardItemModel)
                {
                    var vm = ViewModelLocator.BoardViewModel;
                    vm.SelectedItem = (BoardItemModel)value;
                    ActiveView = vm;
                    ActiveBoard = (BoardItemModel)value;
                    RaisePropertyChanged("ActiveBoard");
                }
                else if (value is CardViewModel)
                {
                    var vm = ViewModelLocator.SizeViewModel;
                    vm.SelectedItem = (SizeItemModel)value;
                    ActiveView = vm;
                }
                else if (value is IBaseAction)
                {
                    var vm = ViewModelLocator.ActionViewModel;
                    vm.SelectedItem = (IBaseAction)value;
                    ActiveView = vm;
                }
                else if (value is TableViewModel)
                {
                    ActiveView = ViewModelLocator.TableViewModel;
                }
                else
                {
                    ActiveView = null;
                }
                RaisePropertyChanged("Selection");
                RaisePropertyChanged("ActiveView");
                RaisePropertyChanged("ClickHand");
                RaisePropertyChanged("ClickTable");
                RaisePropertyChanged("ClickSize");
                RaisePropertyChanged("ClickBoard");
            }
        }
    }
}