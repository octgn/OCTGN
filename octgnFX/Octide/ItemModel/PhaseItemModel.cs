using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octide.ItemModel
{
    public class PhaseItemModel : IdeListBoxItemBase, ICloneable
    {
        public GamePhase _phase;

        public PhaseItemModel()
        {
            _phase = new GamePhase();
            _phase.Icon = ViewModelLocator.SizeViewModel.Images.First().FullPath;
            _phase.Name = "Phase";
            RaisePropertyChanged("Icon");
            RaisePropertyChanged("IconImage");
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
        }

        public PhaseItemModel(GamePhase p)
        {
            _phase = p;
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
        }

        public PhaseItemModel(PhaseItemModel p)
        {
            _phase = new GamePhase();
            _phase.Icon = p.Icon.FullPath;
            _phase.Name = p.Name;
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
        }

        public object Clone()
        {
            return new PhaseItemModel(this);
        }

        public void Copy()
        {
            if (CanCopy == false) return;
            var index = ViewModelLocator.PreviewTabViewModel.Phases.IndexOf(this);
            ViewModelLocator.PreviewTabViewModel.Phases.Insert(index, Clone() as PhaseItemModel);
        }

        public void Remove()
        {
            if (CanRemove == false) return;
            ViewModelLocator.PreviewTabViewModel.Phases.Remove(this);
        }

        public void Insert()
        {
            if (CanInsert == false) return;
            var index = ViewModelLocator.PreviewTabViewModel.Phases.IndexOf(this);
            ViewModelLocator.PreviewTabViewModel.Phases.Insert(index, new PhaseItemModel());
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
