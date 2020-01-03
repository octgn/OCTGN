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
                BuildCaseDefinitions();
            };
            Items.Add(new IfCaseModel() { Parent = this, ItemSource = Items });
            AddElseIfCommand = new RelayCommand(AddElseIf);
            AddElseCommand = new RelayCommand(AddElse);
        }

        public ConditionalBlockModel(LinkDefinition.LinkWrapper lw) //load
        {
            _wrapper = lw;
            Items = new ObservableCollection<IBaseConditionalCase>();
            if (lw.Conditional.ifNode != null)
                Items.Add(new IfCaseModel(lw.Conditional.ifNode) { Parent = this });
            if (lw.Conditional.elseifNodeList != null)
            {
                foreach (var elseif in lw.Conditional.elseifNodeList)
                {
                    Items.Add(new ElseIfCaseModel(elseif) { Parent = this, ItemSource = Items });
                }
            }
            if (lw.Conditional.elseNode != null)
                Items.Add(new ElseCaseModel(lw.Conditional.elseNode) { Parent = this, ItemSource = Items });
            Items.CollectionChanged += (a, b) =>
            {
                BuildCaseDefinitions();
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
                BuildCaseDefinitions();
            };
            foreach (IBaseConditionalCase caseItem in conditional.Items)
            {
                if (caseItem is IfCaseModel)
                    Items.Add(new IfCaseModel(caseItem as IfCaseModel) { Parent = this, ItemSource = Items });
                if (caseItem is ElseIfCaseModel)
                    Items.Add(new ElseIfCaseModel(caseItem as ElseIfCaseModel) { Parent = this, ItemSource = Items });
                if (caseItem is ElseCaseModel)
                    Items.Add(new ElseCaseModel(caseItem as ElseCaseModel) { Parent = this, ItemSource = Items });
            }
            AddElseIfCommand = new RelayCommand(AddElseIf);
            AddElseCommand = new RelayCommand(AddElse);

            Parent = conditional.Parent;
            ItemSource = conditional.ItemSource;
        }
        public void BuildCaseDefinitions()
        {
            _wrapper.Conditional.ifNode = Items.FirstOrDefault(x => x is IfCaseModel)?._case;
            _wrapper.Conditional.elseifNodeList = Items.Where(x => x is ElseIfCaseModel).Select(x => x._case).ToList();
            _wrapper.Conditional.elseNode = Items.FirstOrDefault(x => x is ElseCaseModel)?._case;
        }

        public void AddElseIf()
        {
            if (ElseCase == null)

                Items.Add(new ElseIfCaseModel() { Parent = this, ItemSource = Items });
            else
            {
                Items.Insert(Items.IndexOf(ElseCase), new ElseIfCaseModel() { Parent = this, ItemSource = Items });
            }
        }
        public void AddElse()
        {
            if (ElseCase != null) return;
            Items.Add(new ElseCaseModel() { Parent = this, ItemSource = Items });
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
            ItemSource.Insert(index, new ConditionalBlockModel() { ItemSource = ItemSource, Parent = Parent });
        }
    }

    public class IfCaseModel : IBaseConditionalCase
    {
        public new ConditionalBlockModel Parent { get; set; }

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
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
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
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
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
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
            };
            _case.linkList = BlockContainer.BuildTemplateBlockDef();
            Parent = conditionalCase.Parent;
            ItemSource = conditionalCase.ItemSource;
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
            ItemSource.Insert(index, new IfCaseModel() { ItemSource = ItemSource, Parent = Parent });
        }
    }


    public class ElseIfCaseModel : IBaseConditionalCase
    {
        public new ObservableCollection<IBaseConditionalCase> ItemSource { get; set; }
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
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
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
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
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
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
            };
            _case.linkList = BlockContainer.BuildTemplateBlockDef();
            Property = conditionalCase.Property;
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Parent = conditionalCase.Parent;
            ItemSource = conditionalCase.ItemSource;
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
            ItemSource.Insert(index, new ElseIfCaseModel() { Parent = Parent });
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
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
            };
            CanDragDrop = false;
        }

        public ElseCaseModel(CaseDefinition caseItem) //load
        {
            _case = caseItem;
            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
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
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
            };
            _case.linkList = BlockContainer.BuildTemplateBlockDef();
            CanDragDrop = false;
            Parent = caseItem.Parent;
            ItemSource = caseItem.ItemSource;
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
            ItemSource.Insert(index, new ElseCaseModel() { Parent = Parent });
        }
    }
}
