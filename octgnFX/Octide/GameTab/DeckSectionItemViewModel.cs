// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class DeckSectionItemViewModel : IdeListBoxItemBase
    {
        public DeckSection _deckSection;

        public GroupItemViewModel _group;

        public DeckSectionItemViewModel() // new item
        {
            _deckSection = new DeckSection();

            SelectedGroup = (GroupItemViewModel)Groups.FirstOrDefault();
        }

        public DeckSectionItemViewModel(DeckSection d) // load item
        {
            _deckSection = d;
        }

        public DeckSectionItemViewModel(DeckSectionItemViewModel d) // copy item
        {
            Parent = d.Parent;
            ItemSource = d.ItemSource;
            _deckSection = new DeckSection()
            {
                Shared = d.Global,
                Group = d._deckSection.Group
            };
            Name = d.Name;
        }

        public string Name
        {
            get
            {
                return _deckSection.Name;
            }
            set
            {
                if (_deckSection.Name == value) return;
                if (string.IsNullOrEmpty(value)) return;
                _deckSection.Name =  Utils.GetUniqueName(value, ItemSource.Select(x => (x as DeckSectionItemViewModel).Name)); ;
                RaisePropertyChanged("Name");
            }
        }

        public bool Global
        {
            get
            {
                return _deckSection.Shared;
            }
            set
            {
                if (_deckSection.Shared == value) return;
                _deckSection.Shared = value;
                RaisePropertyChanged("Global");
            }
        }

        public ObservableCollection<IdeListBoxItemBase> Groups => _deckSection.Shared ? ViewModelLocator.PreviewTabViewModel.GlobalPiles : ViewModelLocator.PreviewTabViewModel.Piles;

        public GroupItemViewModel SelectedGroup
        {
            get
            {
                if (_deckSection.Group == null)
                    return null;
                return (GroupItemViewModel) Groups.FirstOrDefault(x => (x as GroupItemViewModel).Name == _deckSection.Group);
            }
            set
            {

                if (SelectedGroup == value) return;
                _deckSection.Group = value.Name;
                RaisePropertyChanged("SelectedGroup");
            }
        }
        public override object Clone()
        {
            return new DeckSectionItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as DeckSectionItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new DeckSectionItemViewModel()
            {
                Parent = Parent,
                ItemSource = ItemSource,
                Name = Utils.GetUniqueName("Section", ItemSource.Select(x => (x as DeckSectionItemViewModel).Name))
            });
        }
    }
}
