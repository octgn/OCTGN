using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.Messages;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octide.ItemModel
{
    public class BoardItemModel : ViewModelBase, ICloneable
    {
        public GameBoard _board;

        public BoardItemModel()
        {
            _board = new GameBoard();
            _board.Source = ViewModelLocator.BoardViewModel.Images.First().FullPath;
            _board.Name = Guid.NewGuid().ToString();
            RaisePropertyChanged("Name");
            RaisePropertyChanged("Source");
            RaisePropertyChanged("Image");
        }

        public BoardItemModel(GameBoard g)
        {
            _board = g;
        }

        public BoardItemModel(BoardItemModel g)
        {
            _board = new GameBoard();
            _board.Name = g.Name;
            _board.Height = g.Height;
            _board.Width = g.Width;
            _board.XPos = g.XPos;
            _board.YPos = g.YPos;
            _board.Source = g.Front.FullPath;
        }

        public object Clone()
        {
            return new BoardItemModel(this);
        }

        public bool Default
        {
            get
            {
                return _board.Name == "Default";
            }
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
                if (ViewModelLocator.PreviewTabViewModel.Boards.Count(x => x.Name == value) > 0 || value == "Default") return;
                _board.Name = value;
                //has to update the card data when the size name changes
                RaisePropertyChanged("Name");
                ViewModelLocator.BoardViewModel.UpdateBoards();
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public Asset Front
        {
            get
            {
                if (_board.Source == null)
                    return new Asset();
                return Asset.Load(_board.Source);
            }
            set
            {
                if (value == null) return;
                if (!File.Exists(value.FullPath)) return;
                _board.Source = value.FullPath;
                RaisePropertyChanged("Front");
                RaisePropertyChanged("FrontImage");
            }
        }

        public string FrontImage => _board.Source;

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
