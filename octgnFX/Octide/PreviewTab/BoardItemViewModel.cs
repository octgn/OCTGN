using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System;
using System.Linq;

namespace Octide.ItemModel
{
    public class BoardItemViewModel : IdeListBoxItemBase
    {
        public GameBoard _board;

        public BoardItemViewModel()
        {
            _board = new GameBoard
            {
                Source = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
          //      Name = Utils.GetUniqueName("New Board", source.Select(x => (x as BoardItemModel).Name)),
                Height = 15,
                Width = 15,
            };
            RaisePropertyChanged("Name");
            RaisePropertyChanged("Asset");
        }

        public BoardItemViewModel(GameBoard g)
        {
            _board = g;
        }

        public BoardItemViewModel(BoardItemViewModel b)
        {
            _board = new GameBoard
            {
                Name = Utils.GetUniqueName(b.Name, b.ItemSource.Select(x => (x as BoardItemViewModel).Name)),
                Height = b.Height,
                Width = b.Width,
                XPos = b.XPos,
                YPos = b.YPos,
                Source = b.Asset.FullPath
            };
            Parent = b.Parent;
            ItemSource = b.ItemSource;
        }

        public override object Clone()
        {
            return new BoardItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as BoardItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new BoardItemViewModel()
            {
                ItemSource = ItemSource,
                Parent = Parent,
                Name = Utils.GetUniqueName("New Board", ItemSource.Select(x => (x as BoardItemViewModel).Name))
            });
        }

        public string Name
        {
            get
            {
                return _board.Name;
            }
            set
            {
                if (_board.Name == value) return;
                if (string.IsNullOrEmpty(value)) return;
                _board.Name = Utils.GetUniqueName(value, ItemSource.Select(x => (x as BoardItemViewModel).Name));
                RaisePropertyChanged("Name");
            }
        }

        public new bool IsDefault
        {
            get
            {
                return ViewModelLocator.PreviewTabViewModel.DefaultBoard == this;
            }
            set
            {
                var oldDefault = ViewModelLocator.PreviewTabViewModel.DefaultBoard;
                if (oldDefault != null)
                {
                    ViewModelLocator.PreviewTabViewModel.DefaultBoard = null;
                    oldDefault.RaisePropertyChanged("IsDefault");
                }
                if (value == true)
                {
                    ViewModelLocator.PreviewTabViewModel.DefaultBoard = this;
                }
                RaisePropertyChanged("IsDefault");
                ViewModelLocator.PreviewTabViewModel.UpdateBoards();
            }
        }

        public Asset Asset
        {
            get
            {
                return Asset.Load(_board.Source);
            }
            set
            {
                _board.Source = value?.FullPath;
                RaisePropertyChanged("Asset");
            }
        }

        public int Height
        {
            get
            {
                return Convert.ToInt32(_board.Height);
            }
            set
            {
                if (value == _board.Height) return;
                if (value < 5) return;
                _board.Height = value;
                RaisePropertyChanged("Height");
            }
        }

        public int Width
        {
            get
            {
                return Convert.ToInt32(_board.Width);
            }
            set
            {
                if (value == _board.Width) return;
                if (value < 5) return;
                _board.Width = value;
                RaisePropertyChanged("Width");
            }
        }

        public int XPos
        {
            get
            {
                return Convert.ToInt32(_board.XPos);
            }
            set
            {
                if (value == _board.XPos) return;
                _board.XPos = value;
                RaisePropertyChanged("XPos");
            }
        }

        public int YPos
        {
            get
            {
                return Convert.ToInt32(_board.YPos);
            }
            set
            {
                if (value == _board.YPos) return;
                _board.YPos = value;
                RaisePropertyChanged("YPos");
            }
        }

        internal void RefreshValues()
        {
            RaisePropertyChanged("");
            RaisePropertyChanged("XPos");
            RaisePropertyChanged("YPos");
        }
    }
}
