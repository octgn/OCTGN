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
                BuildCaseDefinitions();
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
                    Items.Add(new SwitchCaseModel(switchcase) { Parent = this, ItemSource = Items });
                }
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildCaseDefinitions();
            };
            if (lw.Conditional.elseNode != null)
                Items.Add(new DefaultCaseModel(lw.Conditional.elseNode) { Parent = this, ItemSource = Items });

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
                BuildCaseDefinitions();
            };
            foreach (IBaseConditionalCase caseItem in switchItem.Items)
            {
                if (caseItem is SwitchCaseModel)
                    Items.Add(new SwitchCaseModel(caseItem as SwitchCaseModel) { Parent = this, ItemSource = Items });
                if (caseItem is DefaultCaseModel)
                    Items.Add(new DefaultCaseModel(caseItem as DefaultCaseModel) { Parent = this, ItemSource = Items });
            }
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            AddCaseCommand = new RelayCommand(AddCase);
            AddDefaultCommand = new RelayCommand(AddDefault);

            Parent = switchItem.Parent;
            ItemSource = switchItem.ItemSource;
        }
        
        public void BuildCaseDefinitions()
        {
            _wrapper.Conditional.switchNodeList = Items.Where(x => x is SwitchCaseModel).Select(x => x._case).ToList();
            _wrapper.Conditional.elseNode = Items.FirstOrDefault(x => x is DefaultCaseModel)?._case;
        }

        public void AddCase()
        {
            if (DefaultCase == null)

                Items.Add(new SwitchCaseModel() { Parent = this, ItemSource = Items });
            else
            {
                Items.Insert(Items.IndexOf(DefaultCase), new SwitchCaseModel() { Parent = this, ItemSource = Items });
            }
        }
        public void AddDefault()
        {
            if (DefaultCase != null) return;
            Items.Add(new DefaultCaseModel() { Parent = this, ItemSource = Items });
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
            ItemSource.Insert(index, new SwitchBlockModel() { ItemSource = ItemSource, Parent = Parent });
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
        public new ObservableCollection<IBaseConditionalCase> ItemSource { get; set; }
        public SwitchCaseModel() //new
        {
            _case = new CaseDefinition
            {
                value = ""
            };
            BlockContainer = new BlockContainer();
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
            };
        }

        public SwitchCaseModel(CaseDefinition caseItem) //load
        {
            _case = caseItem;
            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
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
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
            };
            _case.linkList = BlockContainer.BuildTemplateBlockDef();
            Parent = switchcase.Parent;
            ItemSource = switchcase.ItemSource;
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
        public new ObservableCollection<IBaseConditionalCase> ItemSource { get; set; }
        public DefaultCaseModel() //new
        {
            _case = new CaseDefinition();
            BlockContainer = new BlockContainer();
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
            };
            CanDragDrop = false;
        }

        public DefaultCaseModel(CaseDefinition caseItem) //load
        {
            _case = caseItem;
            BlockContainer = new BlockContainer(caseItem.linkList);
            BlockContainer.OnContainerChanged += (a, b) =>
            {
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
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
                _case.linkList = BlockContainer.BuildTemplateBlockDef();
            };
            _case.linkList = BlockContainer.BuildTemplateBlockDef();
            CanDragDrop = false;
            Parent = switchcase.Parent;
            ItemSource = switchcase.ItemSource;
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
