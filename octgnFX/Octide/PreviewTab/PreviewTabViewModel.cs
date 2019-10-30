// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

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

    public class PreviewTabViewModel : ViewModelBase
    {
        public ObservableCollection<CardViewModel> Cards { get; set; }


        public ObservableCollection<IdeListBoxItemBase> Piles { get; set; }
        public ObservableCollection<IdeListBoxItemBase> VisiblePiles => new ObservableCollection<IdeListBoxItemBase>(Piles.Where(x => (x as GroupItemViewModel).Collapsed == false));
        public ObservableCollection<IdeListBoxItemBase> CollapsedPiles => new ObservableCollection<IdeListBoxItemBase>(Piles.Where(x => (x as GroupItemViewModel).Collapsed == true));
        public ObservableCollection<IdeListBoxItemBase> GlobalPiles { get; set; }
        public ObservableCollection<IdeListBoxItemBase> VisibleGlobalPiles => new ObservableCollection<IdeListBoxItemBase>(GlobalPiles.Where(x => (x as GroupItemViewModel).Collapsed == false));
        public ObservableCollection<IdeListBoxItemBase> CollapsedGlobalPiles => new ObservableCollection<IdeListBoxItemBase>(GlobalPiles.Where(x => (x as GroupItemViewModel).Collapsed == true));
        public IdeListBoxItemBase Hand { get; set; }
        public GroupItemViewModel TableGroup { get; set; }
        public ObservableCollection<IdeListBoxItemBase> Counters { get; set; }
        public ObservableCollection<IdeListBoxItemBase> GlobalCounters { get; set; }
        public ObservableCollection<IdeListBoxItemBase> CardSizes { get; set; }
        public ObservableCollection<IdeListBoxItemBase> Boards { get; set; }
        public BoardItemViewModel DefaultBoard { get; set; }
        public BoardItemViewModel ActiveBoard { get; set; }
        public ObservableCollection<IdeListBoxItemBase> Phases { get; set; }

        public TableItemViewModel Table { get; set; }

        public RelayCommand AddPileCommand { get; private set; }
        public RelayCommand AddGlobalPileCommand { get; private set; }
        public RelayCommand AddCounterCommand { get; private set; }
        public RelayCommand AddGlobalCounterCommand { get; private set; }
        public RelayCommand AddSizeCommand { get; private set; }
        public RelayCommand AddBoardCommand { get; private set; }
        public RelayCommand AddPhaseCommand { get; private set; }
        
        public CardsizeDropHandler CardsizeDropHandler{ get; set; } = new CardsizeDropHandler();
        public TableDropHandler TableDropHandler{ get; set; } = new TableDropHandler();

        public SizeItemViewModel DefaultSize => CardSizes.FirstOrDefault(x => x.IsDefault) as SizeItemViewModel;

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

        public string GlobalSummary
        {
            get { return ViewModelLocator.GameLoader.Game.GlobalPlayer.IndicatorsFormat; }
            set
            {
                if (ViewModelLocator.GameLoader.Game.GlobalPlayer.IndicatorsFormat == value) return;
                ViewModelLocator.GameLoader.Game.GlobalPlayer.IndicatorsFormat = value;
                RaisePropertyChanged("GlobalSummary");
            }
        }

        public PreviewTabViewModel()
        {
            var _game = ViewModelLocator.GameLoader.Game;
            if (_game.GlobalPlayer == null)
            {
                _game.GlobalPlayer = new GlobalPlayer()
                {
                    Counters = new List<Counter>(),
                    Groups = new List<Group>()
                };
            }
            Hand = _game.Player.Hand == null ? null : new GroupItemViewModel(_game.Player.Hand);
            TableGroup = new GroupItemViewModel(_game.Table);
            Piles = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var pile in _game.Player.Groups)
            {
                Piles.Add(new GroupItemViewModel(pile) { ItemSource = Piles });
            }
            Piles.CollectionChanged += (a, b) =>
            {
                _game.Player.Groups = Piles.Select(x => (x as GroupItemViewModel)._group).ToList();
                RaisePropertyChanged("CollapsedPiles");
                RaisePropertyChanged("VisiblePiles");
                Messenger.Default.Send(new GroupChangedMessage(b));
            };
            AddPileCommand = new RelayCommand(AddPile);

            Counters = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var counter in _game.Player.Counters)
            {
                Counters.Add(new CounterItemViewModel(counter) { ItemSource = Counters });
            }
            Counters.CollectionChanged += (a, b) =>
            {
                _game.Player.Counters = Counters.Select(x => (x as CounterItemViewModel)._counter).ToList();
            };
            AddCounterCommand = new RelayCommand(AddCounter);

            GlobalPiles = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var pile in _game.GlobalPlayer.Groups)
            {
                GlobalPiles.Add(new GroupItemViewModel(pile) { ItemSource = GlobalPiles });
            }
            GlobalPiles.CollectionChanged += (a, b) =>
            {
                _game.GlobalPlayer.Groups = Piles.Select(x => (x as GroupItemViewModel)._group).ToList();
                RaisePropertyChanged("CollapsedGlobalPiles");
                RaisePropertyChanged("VisibleGlobalPiles");
                Messenger.Default.Send(new GroupChangedMessage(b));
            };
            AddGlobalPileCommand = new RelayCommand(AddGlobalPile);

            GlobalCounters = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var counter in _game.GlobalPlayer.Counters)
            {
                GlobalCounters.Add(new CounterItemViewModel(counter) { ItemSource = GlobalCounters });
            }

            GlobalCounters.CollectionChanged += (a, b) =>
            {
                _game.GlobalPlayer.Counters = GlobalCounters.Select(x => (x as CounterItemViewModel)._counter).ToList();
            };
            AddGlobalCounterCommand = new RelayCommand(AddGlobalCounter);

            CardSizes = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var sizeDef in _game.CardSizes)
            {
                var size = new SizeItemViewModel(sizeDef.Value) { ItemSource = CardSizes };
                if (sizeDef.Key == "")
                {
                    size.IsDefault = true;
                    size.CanRemove = false;
                }
                CardSizes.Add(size);
            }
            CardSizes.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Game.CardSizes = CardSizes.ToDictionary(
                                                            x => x.IsDefault ? "" : (x as SizeItemViewModel).Name,
                                                            y => (y as SizeItemViewModel)._size
                                                            );
                Messenger.Default.Send(new CardSizeChangedMesssage(b));
            };
            AddSizeCommand = new RelayCommand(AddSize);

            Phases = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var phase in _game.Phases)
            {
                Phases.Add(new PhaseItemViewModel(phase) { ItemSource = Phases });
            }
            Phases.CollectionChanged += (a, b) =>
            {
                _game.Phases = Phases.Select(x => (x as PhaseItemViewModel)._phase).ToList();
            };
            AddPhaseCommand = new RelayCommand(AddPhase);

            Boards = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var boardDef in _game.GameBoards)
            {
                var board = new BoardItemViewModel(boardDef.Value) { ItemSource = Boards };
                if (boardDef.Key == "")
                {
                    DefaultBoard = board;
                }
                Boards.Add(board);
            }
            ActiveBoard = DefaultBoard;
            Boards.CollectionChanged += (a, b) =>
            {
                UpdateBoards();
            };
            AddBoardCommand = new RelayCommand(AddBoard);

            Table = new TableItemViewModel();

            Cards = new ObservableCollection<CardViewModel>();
            var card = new CardViewModel
            {
                Size = DefaultSize
            };
            Cards.Add(card);

            RaisePropertyChanged("Cards");
        }

        public bool ClickHand
        {
            get
            {
                return (Selection == Hand && Selection != null);
            }
            set
            {
                if (Hand == null)
                {
                    Hand = new GroupItemViewModel
                    {
                        Name = "Hand"
                    };
                    ViewModelLocator.GameLoader.Game.Player.Hand = (Hand as GroupItemViewModel)._group;
                    RaisePropertyChanged("Hand");
                }
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

        public void UpdateBoards()
        {
            ViewModelLocator.GameLoader.Game.GameBoards = Boards.ToDictionary(
                                                        x => (x as BoardItemViewModel).IsDefault ? "" : (x as BoardItemViewModel).Name,
                                                        y => (y as BoardItemViewModel)._board
                                                        );
        }

        public void AddPile()
        {
            var ret = new GroupItemViewModel() { ItemSource = Piles };
            Piles.Add(ret);
            Selection = ret;
            RaisePropertyChanged("Piles");
        }

        public void AddGlobalPile()
        {
            var ret = new GroupItemViewModel() { ItemSource = GlobalPiles };
            GlobalPiles.Add(ret);
            Selection = ret;
            RaisePropertyChanged("GlobalPiles");
        }

        public void AddCounter()
        {
            var ret = new CounterItemViewModel() { ItemSource = Counters };
            Counters.Add(ret);
            Selection = ret;
            RaisePropertyChanged("Counters");
        }

        public void AddGlobalCounter()
        {
            var ret = new CounterItemViewModel() { ItemSource = GlobalCounters };
            GlobalCounters.Add(ret);
            Selection = ret;
            RaisePropertyChanged("GlobalCounters");
        }

        public void AddSize()
        {
            var ret = new SizeItemViewModel() { ItemSource = CardSizes, Name = "Size"};
            CardSizes.Add(ret);
            Selection = ret;
            RaisePropertyChanged("CardSizes");
        }

        public void AddPhase()
        {
            var ret = new PhaseItemViewModel() { ItemSource = Phases };
            Phases.Add(ret);
            Selection = ret;
            RaisePropertyChanged("Phases");
        }

        public void AddBoard()
        {
            var ret = new BoardItemViewModel()
            {
                ItemSource = Boards,
                Name = Utils.GetUniqueName("New Board", Boards.Select(x => (x as BoardItemViewModel).Name))
            };
            Boards.Add(ret);
            Selection = ret;
        }

        public void RemoveHand()
        {
            Hand = null;
            ViewModelLocator.GameLoader.Game.Player.Hand = null;
            Selection = null;
            RaisePropertyChanged("Hand");
            RaisePropertyChanged("ClickHand");
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
                if (value is BoardItemViewModel)
                {
                    ActiveBoard = (BoardItemViewModel)value;
                }
                RaisePropertyChanged("Selection");
                RaisePropertyChanged("ClickHand");
                RaisePropertyChanged("ClickTable");
                RaisePropertyChanged("ClickSize");
                RaisePropertyChanged("ClickBoard");
                RaisePropertyChanged("ActiveBoard");
            }
        }
    }
    
    public class CardsizeDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Move;
        }

        public virtual void Drop(IDropInfo dropInfo)
        {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
        }
    }

    public class TableDropHandler : IDropTarget
    {
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is SizeItemViewModel)
            {
                dropInfo.Effects = DragDropEffects.Move;

            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is SizeItemViewModel)
            {
                SizeItemViewModel item = dropInfo.Data as SizeItemViewModel;
                var card = new CardViewModel
                {
                    Size = item
                };
                ViewModelLocator.PreviewTabViewModel.Cards.Add(card);
            }
        }
    }

}