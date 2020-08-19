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
    using System.Collections.Specialized;

    public class PreviewTabViewModel : ViewModelBase
    {
        public ObservableCollection<SampleCardItemModel> Cards { get; set; }


        public IdeCollection<IdeBaseItem> Piles { get; set; }
    //    public ObservableCollection<GroupItemModel> VisiblePiles => new ObservableCollection<GroupItemModel>(Piles.Where(x => x.Collapsed == false));
    //    public ObservableCollection<GroupItemModel> CollapsedPiles => new ObservableCollection<GroupItemModel>(Piles.Where(x => x.Collapsed == true));
        public IdeCollection<IdeBaseItem> GlobalPiles { get; set; }
    //    public ObservableCollection<GroupItemModel> VisibleGlobalPiles => new ObservableCollection<GroupItemModel>(GlobalPiles.Where(x => x.Collapsed == false));
    //    public ObservableCollection<GroupItemModel> CollapsedGlobalPiles => new ObservableCollection<GroupItemModel>(GlobalPiles.Where(x => x.Collapsed == true));
        public IdeCollection<IdeBaseItem> Counters { get; set; }
        public IdeCollection<IdeBaseItem> GlobalCounters { get; set; }
        public IdeCollection<IdeBaseItem> CardSizes { get; set; }
        public IdeCollection<IdeBaseItem> Boards { get; set; }
        public BoardItemModel ActiveBoard { get; set; }
        public IdeCollection<IdeBaseItem> Phases { get; set; }

        public TableItemModel Table { get; set; }

        public RelayCommand AddPileCommand { get; private set; }
        public RelayCommand AddGlobalPileCommand { get; private set; }
        public RelayCommand AddCounterCommand { get; private set; }
        public RelayCommand AddGlobalCounterCommand { get; private set; }
        public RelayCommand AddSizeCommand { get; private set; }
        public RelayCommand AddBoardCommand { get; private set; }
        public RelayCommand AddPhaseCommand { get; private set; }
        
        public CardsizeDropHandler CardsizeDropHandler{ get; set; } = new CardsizeDropHandler();
        public TableDropHandler TableDropHandler{ get; set; } = new TableDropHandler();


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

        public Game _game => ViewModelLocator.GameLoader.Game;

        public PreviewTabViewModel()
        {
            #region table
            if (_game.Table == null)
            {
                _game.Table = new Group()
                {
                    Name = "Table",
                    Width = 640,
                    Height = 480
                };
            }
            Table = new TableItemModel(_game.Table, new IdeCollection<IdeBaseItem>(this))
            {
                CanRemove = false,
                CanCopy = false,
                CanInsert = false
            };
            #endregion
            #region piles
            if (_game.Player == null)
            {
                _game.Player = new Player();
            }
            Piles = new IdeCollection<IdeBaseItem>(this);
            foreach (var pile in _game.Player.Groups)
            {
                Piles.Add(new PileItemModel(pile, Piles));
            }
            Piles.CollectionChanged += (sender, args) =>
            {
                _game.Player.Groups = Piles.Select(x => ((PileItemModel)x)._group);
                RaisePropertyChanged("CollapsedPiles");
                RaisePropertyChanged("VisiblePiles");
                Messenger.Default.Send(new GroupChangedMessage(args));
            };
            AddPileCommand = new RelayCommand(AddPile);
            #endregion
            #region counters
            Counters = new IdeCollection<IdeBaseItem>(this);
            foreach (var counter in _game.Player.Counters)
            {
                Counters.Add(new CounterItemModel(counter, Counters));
            }
            Counters.CollectionChanged += (sender, args) =>
            {
                _game.Player.Counters = Counters.Select(x => ((CounterItemModel)x)._counter);
            };
            AddCounterCommand = new RelayCommand(AddCounter);
            #endregion
            #region globalpiles
            if (_game.GlobalPlayer == null)
            {
                _game.GlobalPlayer = new GlobalPlayer();
            }
            GlobalPiles = new IdeCollection<IdeBaseItem>(this);
            foreach (var pile in _game.GlobalPlayer.Groups)
            {
                GlobalPiles.Add(new PileItemModel(pile, GlobalPiles));
            }
            GlobalPiles.CollectionChanged += (sender, args) =>
            {
                _game.GlobalPlayer.Groups = GlobalPiles.Select(x => ((PileItemModel)x)._group);
                RaisePropertyChanged("CollapsedGlobalPiles");
                RaisePropertyChanged("VisibleGlobalPiles");
                Messenger.Default.Send(new GroupChangedMessage(args));
            };
            AddGlobalPileCommand = new RelayCommand(AddGlobalPile);
            #endregion
            #region globalcounters
            GlobalCounters = new IdeCollection<IdeBaseItem>(this);
            foreach (var counter in _game.GlobalPlayer.Counters)
            {
                GlobalCounters.Add(new CounterItemModel(counter, GlobalCounters));
            }
            GlobalCounters.CollectionChanged += (sender, args) =>
            {
                _game.GlobalPlayer.Counters = GlobalCounters.Select(x => ((CounterItemModel)x)._counter);
            };
            AddGlobalCounterCommand = new RelayCommand(AddGlobalCounter);
            #endregion
            #region sizes
            CardSizes = new IdeCollection<IdeBaseItem>(this);
            foreach (var sizeDef in _game.CardSizes)
            {
                var size = new SizeItemModel(sizeDef.Value, CardSizes);
                CardSizes.Add(size);
                if (sizeDef.Key == "")
                {
                    CardSizes.DefaultItem = size;
                }
            }
            CardSizes.CollectionChanged += (sender, args) =>
            {
                UpdateCardSizesDef();
                Messenger.Default.Send(new CardSizeChangedMesssage(args));
            };
            CardSizes.DefaultItemChanged += (sender, args) =>
            {
                UpdateCardSizesDef();
            };
            AddSizeCommand = new RelayCommand(AddSize);
            #endregion
            #region phases
            Phases = new IdeCollection<IdeBaseItem>(this);
            foreach (var phase in _game.Phases)
            {
                Phases.Add(new PhaseItemModel(phase, Phases));
            }
            Phases.CollectionChanged += (sender, args) =>
            {
                _game.Phases = Phases.Select(x => ((PhaseItemModel)x)._phase).ToList();
            };
            AddPhaseCommand = new RelayCommand(AddPhase);
            #endregion
            #region boards
            Boards = new IdeCollection<IdeBaseItem>(this);
            foreach (var boardDef in _game.GameBoards)
            {
                var board = new BoardItemModel(boardDef.Value, Boards);
                Boards.Add(board);
                if (boardDef.Key == "")
                {
                    Boards.DefaultItem = board;
                }
            }
            Boards.CollectionChanged += (sender, args) =>
            {
                UpdateBoardsDef();
            };
            Boards.DefaultItemChanged += (sender, args) =>
            {
                UpdateBoardsDef();
            };
            AddBoardCommand = new RelayCommand(AddBoard);
            #endregion
            #region samplecards
            Cards = new ObservableCollection<SampleCardItemModel>();
            if (CardSizes.DefaultItem != null)
            {
                var card = new SampleCardItemModel
                {
                    Size = (SizeItemModel)CardSizes.DefaultItem
                };
                Cards.Add(card);
            }
            #endregion
            #region activeboard
            if (Boards.DefaultItem != null)
            {
                ActiveBoard = (BoardItemModel)Boards.DefaultItem;
            }
            #endregion
        }

        public void UpdateCardSizesDef()
        {
            ViewModelLocator.GameLoader.Game.CardSizes = CardSizes.ToDictionary(
                                                            x => x.IsDefault ? "" : ((SizeItemModel)x).Name,
                                                            y => ((SizeItemModel)y)._size
                                                            );
        }
        public void UpdateBoardsDef()
        {
            ViewModelLocator.GameLoader.Game.GameBoards = Boards.ToDictionary(
                                                    x => x.IsDefault ? "" : ((BoardItemModel)x).Name,
                                                    y => ((BoardItemModel)y)._board
                                                    );
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
            var ret = new PileItemModel(Piles);
            Piles.Add(ret);
            Selection = ret;
            RaisePropertyChanged("Piles");
        }

        public void AddGlobalPile()
        {
            var ret = new PileItemModel(GlobalPiles);
            GlobalPiles.Add(ret);
            Selection = ret;
            RaisePropertyChanged("GlobalPiles");
        }

        public void AddCounter()
        {
            var ret = new CounterItemModel(Counters);
            Counters.Add(ret);
            Selection = ret;
            RaisePropertyChanged("Counters");
        }

        public void AddGlobalCounter()
        {
            var ret = new CounterItemModel(GlobalCounters);
            GlobalCounters.Add(ret);
            Selection = ret;
            RaisePropertyChanged("GlobalCounters");
        }

        public void AddSize()
        {
            var ret = new SizeItemModel(CardSizes);
            CardSizes.Add(ret);
            Selection = ret;
            RaisePropertyChanged("CardSizes");
        }

        public void AddPhase()
        {
            var ret = new PhaseItemModel(Phases);
            Phases.Add(ret);
            Selection = ret;
            RaisePropertyChanged("Phases");
        }

        public void AddBoard()
        {
            var ret = new BoardItemModel(Boards);
            Boards.Add(ret);
            Selection = ret;
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
                if (value is BoardItemModel)
                {
                    ActiveBoard = (BoardItemModel)value;
                }
                RaisePropertyChanged("Selection");
                RaisePropertyChanged("ClickHand");
                RaisePropertyChanged("ClickTable");
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
                var card = new SampleCardItemModel
                {
                    Size = item
                };
                ViewModelLocator.PreviewTabViewModel.Cards.Add(card);
            }
        }
    }

}