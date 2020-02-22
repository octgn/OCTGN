// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.ProxyGenerator;
using Octgn.ProxyGenerator.Definitions;
using Octide;
using Octide.ItemModel;
using Octide.Messages;
using Octide.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace Octide.ProxyTab.TemplateItemModel
{
    public class ConditionalBlockModel : IBaseBlock
    {
        public ObservableCollection<IBaseConditionalCase> Items { get; set; }
        public RelayCommand AddElseIfCommand { get; set; }
        public RelayCommand AddElseCommand { get; set; }

        public IfCaseModel IfCase
        {
            get
            {
                return Items.FirstOrDefault(x => x is IfCaseModel) as IfCaseModel;
            }
        }
        public ElseCaseModel ElseCase
        {
            get
            {
                return Items.FirstOrDefault(x => x is ElseCaseModel) as ElseCaseModel;
            }
        }

        public ConditionalBlockModel() //new
        {
            var _conditonalDefinition = new ConditionalDefinition();
            _wrapper = new LinkDefinition.LinkWrapper() { Conditional = _conditonalDefinition };

            Items = new ObservableCollection<IBaseConditionalCase>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildConditionalDefinitions(b);
            };
            Items.Add(new IfCaseModel());
            AddElseIfCommand = new RelayCommand(AddElseIf);
            AddElseCommand = new RelayCommand(AddElse);
        }

        public ConditionalBlockModel(LinkDefinition.LinkWrapper lw) //load
        {
            _wrapper = lw;
            Items = new ObservableCollection<IBaseConditionalCase>();
            if (lw.Conditional.ifNode != null)
                Items.Add(new IfCaseModel(lw.Conditional.ifNode) { ItemSource = Items, Parent = this });
            if (lw.Conditional.elseifNodeList != null)
            {
                foreach (var elseif in lw.Conditional.elseifNodeList)
                {
                    Items.Add(new ElseIfCaseModel(elseif) { ItemSource = Items, Parent = this });
                }
            }
            if (lw.Conditional.elseNode != null)
                Items.Add(new ElseCaseModel(lw.Conditional.elseNode) { ItemSource = Items, Parent = this });
            Items.CollectionChanged += (a, b) =>
            {
                BuildConditionalDefinitions(b);
            };
            AddElseIfCommand = new RelayCommand(AddElseIf);
            AddElseCommand = new RelayCommand(AddElse);
        }

        public ConditionalBlockModel(ConditionalBlockModel conditional) //copy
        {
            _wrapper = new LinkDefinition.LinkWrapper() { Conditional = new ConditionalDefinition() };

            Items = new ObservableCollection<IBaseConditionalCase>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildConditionalDefinitions(b);
            };
            foreach (IBaseConditionalCase caseItem in conditional.Items)
            {
                if (caseItem is IfCaseModel ifCase)
                    Items.Add(new IfCaseModel(ifCase) );
                if (caseItem is ElseIfCaseModel elseIfCase)
                    Items.Add(new ElseIfCaseModel(elseIfCase));
                if (caseItem is ElseCaseModel elseCase)
                    Items.Add(new ElseCaseModel(elseCase));
            }
            AddElseIfCommand = new RelayCommand(AddElseIf);
            AddElseCommand = new RelayCommand(AddElse);

        }
        public void BuildConditionalDefinitions(NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IBaseConditionalCase x in args.NewItems)
                {
                    x.ItemSource = Items;
                    x.Parent = this;
                }
            }
            _wrapper.Conditional.ifNode = Items.FirstOrDefault(x => x is IfCaseModel)?._case;
            _wrapper.Conditional.elseifNodeList = Items.Where(x => x is ElseIfCaseModel).Select(x => x._case).ToList();
            _wrapper.Conditional.elseNode = Items.FirstOrDefault(x => x is ElseCaseModel)?._case;
        }

        public void AddElseIf()
        {
            if (ElseCase == null)

                Items.Add(new ElseIfCaseModel() );
            else
            {
                Items.Insert(Items.IndexOf(ElseCase), new ElseIfCaseModel() );
            }
        }
        public void AddElse()
        {
            if (ElseCase != null) return;
            Items.Add(new ElseCaseModel());
            RaisePropertyChanged("CanAddElse");
        }

        public void RemoveElse()
        {
            if (ElseCase == null) return;
            Items.Remove(ElseCase);
            RaisePropertyChanged("CanAddElse");
        }

        public Visibility CanAddElse
        {
            get
            {
                return (ElseCase == null) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public override object Clone()
        {
            return new ConditionalBlockModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as ConditionalBlockModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new ConditionalBlockModel() );
        }
    }

    public class IfCaseModel : IBaseConditionalCase
    {
        public IfCaseModel() //new
        {
            _case = new CaseDefinition()
            {
                value = ""
            };
            Property = CustomProperties.First();
            BlockContainer = new BlockContainer();
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            CanDragDrop = false;
            CanRemove = false;
        }

        public IfCaseModel(CaseDefinition caseItem) //load
        {
            _case = caseItem; 
            _property = CustomProperties.FirstOrDefault(x => x._property.Name == _case.property);

            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            CanDragDrop = false;
            CanRemove = false;
        }

        public IfCaseModel(IfCaseModel conditionalCase) //copy
        {
            _case = new CaseDefinition()
            {
                linkList = new List<LinkDefinition.LinkWrapper>(),
                contains = conditionalCase._case.contains,
                value = conditionalCase._case.value
            };
            Property = conditionalCase.Property;
            BlockContainer = new BlockContainer(conditionalCase.BlockContainer);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            _case.linkList = BlockContainer.BuildTemplateBlockDef(null);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            CanDragDrop = false;
            CanRemove = false;
        }

        public override void Remove()
        {
            if (!CanRemove) return;
            ItemSource.Remove(this);
        }

        public override object Clone()
        {
            return new IfCaseModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as IfCaseModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new IfCaseModel() );
        }
    }


    public class ElseIfCaseModel : IBaseConditionalCase
    {
        public ElseIfCaseModel() //new
        {
            _case = new CaseDefinition
            {
                value = ""
            };
            Property = CustomProperties.First();
            BlockContainer = new BlockContainer();
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public ElseIfCaseModel(CaseDefinition caseItem) //load
        {
            _case = caseItem;
            _property = CustomProperties.FirstOrDefault(x => x._property.Name == _case.property);
            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public ElseIfCaseModel(ElseIfCaseModel conditionalCase) //copy
        {
            _case = new CaseDefinition()
            {
                linkList = new List<LinkDefinition.LinkWrapper>(),
                contains = conditionalCase._case.contains,
                value = conditionalCase._case.property
            };
            BlockContainer = new BlockContainer(conditionalCase.BlockContainer);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            _case.linkList = BlockContainer.BuildTemplateBlockDef(null);
            Property = conditionalCase.Property;
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public override void Remove()
        {
            if (!CanRemove) return;
            ItemSource.Remove(this);
        }

        public override object Clone()
        {
            return new ElseIfCaseModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as ElseIfCaseModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new ElseIfCaseModel());
        }

    }

    public class ElseCaseModel : IBaseConditionalCase
    {
        public ElseCaseModel() //new
        {
            _case = new CaseDefinition();
            BlockContainer = new BlockContainer();
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            CanDragDrop = false;
        }

        public ElseCaseModel(CaseDefinition caseItem) //load
        {
            _case = caseItem;
            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            CanDragDrop = false;
        }

        public ElseCaseModel(ElseCaseModel caseItem) //copy
        {
            _case = new CaseDefinition()
            {
                linkList = new List<LinkDefinition.LinkWrapper>(),
                contains = caseItem._case.contains,
                property = caseItem._case.property,
                value = caseItem._case.property
            };
            BlockContainer = new BlockContainer(caseItem.BlockContainer);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            _case.linkList = BlockContainer.BuildTemplateBlockDef(null);
            CanDragDrop = false;
        }
        public override void Remove()
        {
            if (CanRemove == false) return;
            ((ConditionalBlockModel)Parent).RemoveElse();
        }

        public override object Clone()
        {
            return new ElseCaseModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as ElseCaseModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new ElseCaseModel());
        }
    }
}
