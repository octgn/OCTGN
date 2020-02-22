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
        public PropertyItemViewModel _property;
        public ObservableCollection<PropertyItemViewModel> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;
        public ObservableCollection<IBaseConditionalCase> Items { get; set; }
        public RelayCommand AddCaseCommand { get; set; }
        public RelayCommand AddDefaultCommand { get; set; }

        public DefaultCaseModel DefaultCase
        {
            get
            {
                return Items.FirstOrDefault(x => x is DefaultCaseModel) as DefaultCaseModel;
            }
        }

        public SwitchBlockModel() //new
        {
            var _conditonalDefinition = new ConditionalDefinition()
            {
            };
            Property = CustomProperties.First();
            _wrapper = new LinkDefinition.LinkWrapper() { Conditional = _conditonalDefinition };
            Items = new ObservableCollection<IBaseConditionalCase>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildSwitchDefinitions(b);
            };
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            AddCaseCommand = new RelayCommand(AddCase);
            AddDefaultCommand = new RelayCommand(AddDefault);
        }

        public SwitchBlockModel(LinkDefinition.LinkWrapper lw) //load
        {
            _wrapper = lw;
            _property = CustomProperties.FirstOrDefault(x => x._property.Name == _wrapper.Conditional.switchProperty);
            Items = new ObservableCollection<IBaseConditionalCase>();
            if (lw.Conditional.switchNodeList != null)
            {
                foreach (var switchcase in lw.Conditional.switchNodeList)
                {
                    Items.Add(new SwitchCaseModel(switchcase) { ItemSource = Items, Parent = this });
                }
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildSwitchDefinitions(b);
            };
            if (lw.Conditional.elseNode != null)
                Items.Add(new DefaultCaseModel(lw.Conditional.elseNode) { ItemSource = Items, Parent = this });

            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            AddCaseCommand = new RelayCommand(AddCase);
            AddDefaultCommand = new RelayCommand(AddDefault);
        }

        public SwitchBlockModel(SwitchBlockModel switchItem) //copy
        {
            _wrapper = new LinkDefinition.LinkWrapper()
            {
                Conditional = new ConditionalDefinition()
                {
                }
            };
            Property = switchItem.Property;
            Items = new ObservableCollection<IBaseConditionalCase>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildSwitchDefinitions(b);
            };
            foreach (IBaseConditionalCase caseItem in switchItem.Items)
            {
                if (caseItem is SwitchCaseModel switchCase)
                    Items.Add(new SwitchCaseModel(switchCase));
                if (caseItem is DefaultCaseModel defaultCase)
                    Items.Add(new DefaultCaseModel(defaultCase));
            }
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            AddCaseCommand = new RelayCommand(AddCase);
            AddDefaultCommand = new RelayCommand(AddDefault);
        }
        
        public void BuildSwitchDefinitions(NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IBaseConditionalCase x in args.NewItems)
                {
                    x.ItemSource = Items;
                    x.Parent = this;
                }
            }
            _wrapper.Conditional.switchNodeList = Items.Where(x => x is SwitchCaseModel).Select(x => x._case).ToList();
            _wrapper.Conditional.elseNode = Items.FirstOrDefault(x => x is DefaultCaseModel)?._case;
        }

        public void AddCase()
        {
            if (DefaultCase == null)

                Items.Add(new SwitchCaseModel());
            else
            {
                Items.Insert(Items.IndexOf(DefaultCase), new SwitchCaseModel());
            }
        }
        public void AddDefault()
        {
            if (DefaultCase != null) return;
            Items.Add(new DefaultCaseModel());
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
            return new SwitchBlockModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as SwitchBlockModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new SwitchBlockModel());
        }
        public void CustomPropertyChanged(CustomPropertyChangedMessage args)
        {
            if (args.Prop == _property)
            {
                _wrapper.Conditional.switchProperty = args.Prop.Name;
                RaisePropertyChanged("Property");
            }
        }

        public PropertyItemViewModel Property
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
                    value = CustomProperties.First();
                }
                _property = value;
                _wrapper.Conditional.switchProperty = value.Name;
                RaisePropertyChanged("Property");
            }
        }
    }

    public class SwitchCaseModel : IBaseConditionalCase
    {
        public SwitchCaseModel() //new
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

        public SwitchCaseModel(CaseDefinition caseItem) //load
        {
            _case = caseItem;
            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
        }

        public SwitchCaseModel(SwitchCaseModel switchcase) //copy
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

        public override void Remove()
        {
            if (!CanRemove) return;
            ItemSource.Remove(this);
        }
        public override object Clone()
        {
            return new SwitchCaseModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as SwitchCaseModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new SwitchCaseModel() { Parent = Parent });
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
        public DefaultCaseModel() //new
        {
            _case = new CaseDefinition();
            BlockContainer = new BlockContainer();
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            CanDragDrop = false;
        }

        public DefaultCaseModel(CaseDefinition caseItem) //load
        {
            _case = caseItem;
            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef(b);
            };
            CanDragDrop = false;
        }

        public DefaultCaseModel(DefaultCaseModel switchcase) //copy
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

        public override void Remove()
        {
            if (CanRemove == false) return;
            ((SwitchBlockModel)Parent).RemoveDefault();
        }
        public override object Clone()
        {
            return new DefaultCaseModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as DefaultCaseModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new DefaultCaseModel() { Parent = Parent });
        }
    }
}
