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
using System.Xml;

namespace Octide.ProxyTab.TemplateItemModel
{
    public class SwitchBlockModel : IBaseBlock
    {
        public PropertyItemModel _property;
        public IdeCollection<IdeBaseItem> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;
        public IdeCollection<IdeBaseItem> Items { get; private set; }
        public RelayCommand AddCaseCommand { get; set; }
        public RelayCommand AddDefaultCommand { get; set; }

        public DefaultCaseModel DefaultCase
        {
            get
            {
                return Items.FirstOrDefault(x => x is DefaultCaseModel) as DefaultCaseModel;
            }
        }

        public SwitchBlockModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            var _conditonalDefinition = new ConditionalDefinition();
            _wrapper = new LinkDefinition.LinkWrapper() { Conditional = _conditonalDefinition };

            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                BuildSwitchDefinitions(b);
            };
            Property = (PropertyItemModel)CustomProperties.First();
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            AddCaseCommand = new RelayCommand(AddCase);
            AddDefaultCommand = new RelayCommand(AddDefault);
        }

        public SwitchBlockModel(LinkDefinition.LinkWrapper lw, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _wrapper = lw;
            _property = (PropertyItemModel)CustomProperties.FirstOrDefault(x => ((PropertyItemModel)x)._property.Name == _wrapper.Conditional.switchProperty);
            Items = new IdeCollection<IdeBaseItem>(this);
            if (lw.Conditional.switchNodeList != null)
            {
                foreach (var switchcase in lw.Conditional.switchNodeList)
                {
                    Items.Add(new SwitchCaseModel(switchcase, Items));
                }
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildSwitchDefinitions(b);
            };
            if (lw.Conditional.elseNode != null)
                Items.Add(new DefaultCaseModel(lw.Conditional.elseNode, Items));

            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            AddCaseCommand = new RelayCommand(AddCase);
            AddDefaultCommand = new RelayCommand(AddDefault);
        }

        public SwitchBlockModel(SwitchBlockModel switchItem, IdeCollection<IdeBaseItem> source) : base(source) //copy
        {
            _wrapper = new LinkDefinition.LinkWrapper()
            {
                Conditional = new ConditionalDefinition()
                {
                }
            };
            Property = switchItem.Property;
            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                BuildSwitchDefinitions(b);
            };
            foreach (IBaseConditionalCase caseItem in switchItem.Items)
            {
                if (caseItem is SwitchCaseModel switchCase)
                    Items.Add(new SwitchCaseModel(switchCase, Items));
                if (caseItem is DefaultCaseModel defaultCase)
                    Items.Add(new DefaultCaseModel(defaultCase, Items));
            }
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            AddCaseCommand = new RelayCommand(AddCase);
            AddDefaultCommand = new RelayCommand(AddDefault);
        }
        
        public void BuildSwitchDefinitions(NotifyCollectionChangedEventArgs args)
        {
            _wrapper.Conditional.switchNodeList = Items.Where(x => x is SwitchCaseModel).Select(x => ((SwitchCaseModel)x)._case).ToList();
            _wrapper.Conditional.elseNode = (Items.FirstOrDefault(x => x is DefaultCaseModel) as DefaultCaseModel)?._case;
        }

        public void AddCase()
        {
            if (DefaultCase == null)

                Items.Add(new SwitchCaseModel(Items));
            else
            {
                Items.Insert(Items.IndexOf(DefaultCase), new SwitchCaseModel(Items));
            }
        }
        public void AddDefault()
        {
            if (DefaultCase != null) return;
            Items.Add(new DefaultCaseModel(Items));
            RaisePropertyChanged("CanAddDefault");
        }

        public void RemoveDefault()
        {
            if (DefaultCase == null) return;
            Items.Remove(DefaultCase);
            RaisePropertyChanged("CanAddDefault");
        }

        public Visibility CanAddDefault
        {
            get
            {
                return (DefaultCase == null) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public override object Clone()
        {
            return new SwitchBlockModel(this, Source);
        }
        public override object Create()
        {
            return new SwitchBlockModel(Source);
        }

        public void CustomPropertyChanged(CustomPropertyChangedMessage args)
        {
            if (args.Prop == _property)
            {
                _wrapper.Conditional.switchProperty = args.Prop.Name;
                RaisePropertyChanged("Property");
            }
        }

        public PropertyItemModel Property
        {
            get
            {
                return _property;
            }
            set
            {
                if (_property == value) return;
                if (value == null)
                {
                    value = (PropertyItemModel)CustomProperties.First();
                }
                _property = value;
                _wrapper.Conditional.switchProperty = value.Name;
                RaisePropertyChanged("Property");
            }
        }
    }

    public class SwitchCaseModel : IBaseConditionalCase
    {
        public SwitchCaseModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            _case = new CaseDefinition
            {
                value = ""
            };
            BlockContainer = new BlockContainer();
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
        }

        public SwitchCaseModel(CaseDefinition caseItem, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _case = caseItem;
            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
        }

        public SwitchCaseModel(SwitchCaseModel switchcase, IdeCollection<IdeBaseItem> source) : base(source) //copy
        {
            _case = new CaseDefinition()
            {
                linkList = new List<LinkDefinition.LinkWrapper>(),
                contains = switchcase._case.contains,
                value = switchcase._case.value,
                switchBreak = switchcase._case.switchBreak
            };
            BlockContainer = new BlockContainer(switchcase.BlockContainer);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            _case.linkList = BlockContainer.BuildTemplateBlockDef(null);
        }

        public override object Clone()
        {
            return new SwitchCaseModel(this, Source);
        }
        public override object Create()
        {
            return new SwitchCaseModel(Source);
        }

        public bool Break
        {
            get
            {
                return _case.switchBreak;
            }
            set
            {
                if (_case.switchBreak == value) return;
                _case.switchBreak = value;
                RaisePropertyChanged("Break");
            }
        }
    }

    public class DefaultCaseModel : IBaseConditionalCase
    {
        public DefaultCaseModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            _case = new CaseDefinition();
            BlockContainer = new BlockContainer();
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            CanDragDrop = false;
        }

        public DefaultCaseModel(CaseDefinition caseItem, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _case = caseItem;
            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            CanDragDrop = false;
        }

        public DefaultCaseModel(DefaultCaseModel switchcase, IdeCollection<IdeBaseItem> source) : base(source) //copy
        {
            _case = new CaseDefinition()
            {
                linkList = new List<LinkDefinition.LinkWrapper>(),
                contains = switchcase._case.contains,
                value = switchcase._case.value,
                switchBreak = switchcase._case.switchBreak
            };
            BlockContainer = new BlockContainer(switchcase.BlockContainer);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            _case.linkList = BlockContainer.BuildTemplateBlockDef(null);
            CanDragDrop = false;
        }

        public override object Clone()
        {
            return new DefaultCaseModel(this, Source);
        }
        public override object Create()
        {
            return new DefaultCaseModel(Source);
        }
    }
}
