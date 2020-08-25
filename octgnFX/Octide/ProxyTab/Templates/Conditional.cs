// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.ProxyGenerator.Definitions;
using Octide.ItemModel;
using Octide.Messages;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace Octide.ProxyTab.ItemModel
{
    public class ConditionalBlockModel : IBaseBlock, IDroppable
    {
        public IdeCollection<IdeBaseItem> Items { get; private set; }
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

        public ConditionalBlockModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            var _conditonalDefinition = new ConditionalDefinition();
            _wrapper = new LinkDefinition.LinkWrapper() { Conditional = _conditonalDefinition };

            Items = new IdeCollection<IdeBaseItem>(this, typeof(IBaseConditionalCase));
            Items.CollectionChanged += (a, b) =>
            {
                BuildConditionalDefinitions(b);
            };
            Items.Add(new IfCaseModel(Items)) ;
            AddElseIfCommand = new RelayCommand(AddElseIf);
            AddElseCommand = new RelayCommand(AddElse);
        }

        public ConditionalBlockModel(LinkDefinition.LinkWrapper lw, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _wrapper = lw;
            Items = new IdeCollection<IdeBaseItem>(this, typeof(IBaseConditionalCase));
            if (lw.Conditional.ifNode != null)
                Items.Add(new IfCaseModel(lw.Conditional.ifNode, Items));
            if (lw.Conditional.elseifNodeList != null)
            {
                foreach (var elseif in lw.Conditional.elseifNodeList)
                {
                    Items.Add(new ElseIfCaseModel(elseif, Items));
                }
            }
            if (lw.Conditional.elseNode != null)
                Items.Add(new ElseCaseModel(lw.Conditional.elseNode, Items) );
            Items.CollectionChanged += (a, b) =>
            {
                BuildConditionalDefinitions(b);
            };
            AddElseIfCommand = new RelayCommand(AddElseIf);
            AddElseCommand = new RelayCommand(AddElse);
        }

        public ConditionalBlockModel(ConditionalBlockModel conditional, IdeCollection<IdeBaseItem> source) : base(source) //copy
        {
            _wrapper = new LinkDefinition.LinkWrapper() { Conditional = new ConditionalDefinition() };

            Items = new IdeCollection<IdeBaseItem>(this, typeof(IBaseConditionalCase));
            Items.CollectionChanged += (a, b) =>
            {
                BuildConditionalDefinitions(b);
            };
            foreach (IBaseConditionalCase caseItem in conditional.Items)
            {
                if (caseItem is IfCaseModel ifCase)
                    Items.Add(new IfCaseModel(ifCase, Items) );
                if (caseItem is ElseIfCaseModel elseIfCase)
                    Items.Add(new ElseIfCaseModel(elseIfCase, Items));
                if (caseItem is ElseCaseModel elseCase)
                    Items.Add(new ElseCaseModel(elseCase, Items));
            }
            AddElseIfCommand = new RelayCommand(AddElseIf);
            AddElseCommand = new RelayCommand(AddElse);

        }
        public void BuildConditionalDefinitions(NotifyCollectionChangedEventArgs args)
        {
            _wrapper.Conditional.ifNode = (Items.FirstOrDefault(x => x is IfCaseModel) as IfCaseModel)?._case;
            _wrapper.Conditional.elseifNodeList = Items.Where(x => x is ElseIfCaseModel).Select(x => ((ElseIfCaseModel)x)._case).ToList();
            _wrapper.Conditional.elseNode = (Items.FirstOrDefault(x => x is ElseCaseModel) as ElseCaseModel)?._case;
        }

        public void AddElseIf()
        {
            if (ElseCase == null)

                Items.Add(new ElseIfCaseModel(Items) );
            else
            {
                Items.Insert(Items.IndexOf(ElseCase), new ElseIfCaseModel(Items) );
            }
        }
        public void AddElse()
        {
            if (ElseCase != null) return;
            Items.Add(new ElseCaseModel(Items));
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
            return new ConditionalBlockModel(this, Source);
        }
        public override object Create()
        {
            return new ConditionalBlockModel(Source);
        }
        public bool CanAccept(object item)
        {
            if (item is ElseIfCaseModel)
            {
                return true;
            }
            if (item is ElseCaseModel)
            {
                return (ElseCase == null) ? true : false;
            }
            return false;
        }
    }

    public class IfCaseModel : IBaseConditionalCase
    {
        public IfCaseModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            _case = new CaseDefinition()
            {
                value = ""
            };
            Property = (PropertyItemModel)CustomProperties.First();
            BlockContainer = new BlockContainer();
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            CanDragDrop = false;
            CanRemove = false;
        }

        public IfCaseModel(CaseDefinition caseItem, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _case = caseItem; 
            _property = (PropertyItemModel)CustomProperties.FirstOrDefault(x => ((PropertyItemModel)x)._property.Name == _case.property);

            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            CanDragDrop = false;
            CanRemove = false;
        }

        public IfCaseModel(IfCaseModel conditionalCase, IdeCollection<IdeBaseItem> source) : base(source) //copy
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

        public override object Clone()
        {
            return new IfCaseModel(this, Source);
        }
        public override object Create()
        {
            return new IfCaseModel(Source);
        }
    }


    public class ElseIfCaseModel : IBaseConditionalCase
    {
        public ElseIfCaseModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            _case = new CaseDefinition
            {
                value = ""
            };
            Property = (PropertyItemModel)CustomProperties.First();
            BlockContainer = new BlockContainer();
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public ElseIfCaseModel(CaseDefinition caseItem, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _case = caseItem;
            _property = (PropertyItemModel)CustomProperties.FirstOrDefault(x => ((PropertyItemModel)x)._property.Name == _case.property);
            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public ElseIfCaseModel(ElseIfCaseModel conditionalCase, IdeCollection<IdeBaseItem> source) : base(source) //copy
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

        public override object Clone()
        {
            return new ElseIfCaseModel(this, Source);
        }
        public override object Create()
        {
            return new ElseIfCaseModel(Source);
        }
    }

    public class ElseCaseModel : IBaseConditionalCase
    {
        public ElseCaseModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            _case = new CaseDefinition();
            BlockContainer = new BlockContainer();
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            CanDragDrop = false;
        }

        public ElseCaseModel(CaseDefinition caseItem, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _case = caseItem;
            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            CanDragDrop = false;
        }

        public ElseCaseModel(ElseCaseModel caseItem, IdeCollection<IdeBaseItem> source) : base(source) //copy
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
        public override object Clone()
        {
            return new ElseCaseModel(this, Source);
        }
        public override object Create()
        {
            return new ElseCaseModel(Source);
        }
    }
}
