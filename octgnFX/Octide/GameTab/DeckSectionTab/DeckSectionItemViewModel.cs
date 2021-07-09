// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class DeckSectionItemModel : IdeBaseItem
    {

        public DeckSection _deckSection;
        public IdeCollection<IdeBaseItem> Groups => ViewModelLocator.PreviewTabViewModel.Piles;
        public IdeCollection<IdeBaseItem> SharedGroups => ViewModelLocator.PreviewTabViewModel.GlobalPiles;

        public DeckSectionItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _deckSection = new DeckSection();
              SelectedGroup = (PileItemModel) (IsShared ? SharedGroups.First() : Groups.First()); 
            Name = "New Section";

        //    Messenger.Default.Register<GroupChangedMessage>(this, action => UpdateDeckSectionDef(action));
        }

        public DeckSectionItemModel(DeckSection d, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            _deckSection = d;
        }

        public DeckSectionItemModel(DeckSectionItemModel d, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            _deckSection = new DeckSection()
            {
                Shared = d._deckSection.Shared,
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
                _deckSection.Name = Utils.GetUniqueName(value, Source.Select(x => ((DeckSectionItemModel)x).Name));
                RaisePropertyChanged("Name");
            }
        }

        public bool IsShared
        {
            get
            {
                return _deckSection.Shared;
            }
            set
            {
                if (_deckSection.Shared == value) return;
                _deckSection.Shared = value;
                RaisePropertyChanged("IsShared");
            }
        }


        public PileItemModel SelectedGroup
        {
            get
            {
                if (IsShared)
                    return (PileItemModel)SharedGroups.FirstOrDefault(x => ((PileItemModel)x)._group == _deckSection.Group);
                else
                    return (PileItemModel)Groups.FirstOrDefault(x => ((PileItemModel)x)._group == _deckSection.Group);
            }
            set
            {

                if (SelectedGroup == value) return;
                _deckSection.Group = value._group;
                RaisePropertyChanged("SelectedGroup");
            }
        }
        public override object Clone()
        {
            return new DeckSectionItemModel(this, Source);
        }
        public override object Create()
        {
            return new DeckSectionItemModel(Source);
        }
    }
}
