// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Octgn.ProxyGenerator.Definitions;
using Octide.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace Octide.ProxyTab.TemplateItemModel
{
    public class TemplateModel : IdeListBoxItemBase
    {
        public TemplateDefinition _def;

        public RelayCommand AddMatchCommand { get; private set; }
        public RelayCommand AddOverlayConditionalCommand { get; private set; }
        public RelayCommand AddOverlaySwitchCommand { get; private set; }
        public RelayCommand AddTextConditionalCommand { get; private set; }
        public RelayCommand AddTextSwitchCommand { get; private set; }
        public new ObservableCollection<TemplateModel> ItemSource { get; set; }
        public BlockContainer OverlayContainer { get; set; }
        public BlockContainer TextBlockContainer { get; set; }
        public ObservableCollection<MatchModel> Matches { get; set; }
        public TemplateMatchDropHandler MatchDropHandler { get; set; }
        public TemplateMainDropHandler OverlayDropHandler { get; set; }
        public TemplateMainDragHandler DragHandler { get; set; }
        public TemplateMainDropHandler TextDropHandler { get; set; }

        public TemplateModel() //new
        {
            _def = new TemplateDefinition
            {
                Matches = new List<Property>(),
                OverlayBlocks = new List<LinkDefinition.LinkWrapper>(),
                TextBlocks = new List<LinkDefinition.LinkWrapper>(),
                defaultTemplate = false,
                rootPath = ViewModelLocator.GameLoader.ProxyDef.RootPath
            };
            Matches = new ObservableCollection<MatchModel>();
            Matches.CollectionChanged += (a, b) =>
            {
                BuildMatchDef(b);
            };
            OverlayContainer = new BlockContainer();
            OverlayContainer.OnContainerChanged += (a, b) =>
            {
                _def.OverlayBlocks = OverlayContainer.BuildTemplateBlockDef(b);
            };
            TextBlockContainer = new BlockContainer();
            TextBlockContainer.OnContainerChanged += (a, b) =>
            {
                _def.TextBlocks = TextBlockContainer.BuildTemplateBlockDef(b);
            };
            Asset = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image);
            AddMatchCommand = new RelayCommand(AddMatch);
            AddOverlayConditionalCommand = new RelayCommand(AddOverlayConditional);
            AddOverlaySwitchCommand = new RelayCommand(AddOverlaySwitch);
            AddTextConditionalCommand = new RelayCommand(AddTextConditional);
            AddTextSwitchCommand = new RelayCommand(AddTextSwitch);
            MatchDropHandler = new TemplateMatchDropHandler();
            OverlayDropHandler = new TemplateMainDropHandler() { IsOverlayHandler = true, Container = OverlayContainer };
            TextDropHandler = new TemplateMainDropHandler() { IsOverlayHandler = false, Container = TextBlockContainer };
            DragHandler = new TemplateMainDragHandler();
        }

        public TemplateModel(TemplateDefinition t) //load
        {
            _def = t;
            Matches = new ObservableCollection<MatchModel>();
            foreach (var match in t.Matches)
            {
                Matches.Add(new MatchModel(match) { ItemSource = Matches, Parent = this });
            }
            Matches.CollectionChanged += (a, b) =>
            {
                BuildMatchDef(b);
            };
            OverlayContainer = new BlockContainer(t.OverlayBlocks);
            OverlayContainer.OnContainerChanged += (a, b) =>
            {
                _def.OverlayBlocks = OverlayContainer.BuildTemplateBlockDef(b);
            };
            TextBlockContainer = new BlockContainer(t.TextBlocks);
            TextBlockContainer.OnContainerChanged += (a, b) =>
            {
                _def.TextBlocks = TextBlockContainer.BuildTemplateBlockDef(b);
            };
            AddMatchCommand = new RelayCommand(AddMatch);
            AddOverlayConditionalCommand = new RelayCommand(AddOverlayConditional);
            AddOverlaySwitchCommand = new RelayCommand(AddOverlaySwitch);
            AddTextConditionalCommand = new RelayCommand(AddTextConditional);
            AddTextSwitchCommand = new RelayCommand(AddTextSwitch);
            MatchDropHandler = new TemplateMatchDropHandler();
            OverlayDropHandler = new TemplateMainDropHandler() { IsOverlayHandler = true, Container = OverlayContainer };
            TextDropHandler = new TemplateMainDropHandler() { IsOverlayHandler = false, Container = TextBlockContainer };
            DragHandler = new TemplateMainDragHandler();
        }


        public TemplateModel(TemplateModel p) //copy
        {
            _def = new TemplateDefinition()
            {
                OverlayBlocks = p._def.OverlayBlocks,
                TextBlocks = p._def.TextBlocks,
                Matches = p._def.Matches,
                rootPath = p._def.rootPath,
                src = p.Asset.FullPath,
                defaultTemplate = false
            };

            Matches = new ObservableCollection<MatchModel>();
            Matches.CollectionChanged += (a, b) =>
            {
                BuildMatchDef(b);
            };
            foreach (var match in p.Matches)
            {
                Matches.Add(new MatchModel(match));
            }

            OverlayContainer = new BlockContainer(p.OverlayContainer);
            OverlayContainer.OnContainerChanged += (a, b) =>
            {
                _def.OverlayBlocks = OverlayContainer.BuildTemplateBlockDef(b);
            };
            _def.OverlayBlocks = OverlayContainer.BuildTemplateBlockDef(null);
            TextBlockContainer = new BlockContainer(p.TextBlockContainer);
            TextBlockContainer.OnContainerChanged += (a, b) =>
            {
                _def.TextBlocks = TextBlockContainer.BuildTemplateBlockDef(b);
            };
            _def.TextBlocks = TextBlockContainer.BuildTemplateBlockDef(null);

            AddMatchCommand = new RelayCommand(AddMatch);
            AddOverlayConditionalCommand = new RelayCommand(AddOverlayConditional);
            AddOverlaySwitchCommand = new RelayCommand(AddOverlaySwitch);
            AddTextConditionalCommand = new RelayCommand(AddTextConditional);
            AddTextSwitchCommand = new RelayCommand(AddTextSwitch);
            MatchDropHandler = new TemplateMatchDropHandler();
            OverlayDropHandler = new TemplateMainDropHandler() { IsOverlayHandler = true, Container = OverlayContainer };
            TextDropHandler = new TemplateMainDropHandler() { IsOverlayHandler = false, Container = TextBlockContainer };
            DragHandler = new TemplateMainDragHandler();

        }
        public void BuildMatchDef(NotifyCollectionChangedEventArgs args)
        {
            if (args?.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MatchModel x in args.NewItems)
                {
                    x.ItemSource = Matches;
                    x.Parent = this;
                }
            }
            _def.Matches = Matches.Select(x => x._match).ToList();
        }


        public void AddMatch()
        {
            var ret = new MatchModel();
            Matches.Add(ret);
        }

        public void AddOverlayConditional()
        {
            var ret = new ConditionalBlockModel();
            OverlayContainer.Items.Add(ret);
        }
        public void AddOverlaySwitch()
        {
            var ret = new SwitchBlockModel();
            OverlayContainer.Items.Add(ret);
        }
        public void AddTextConditional()
        {
            var ret = new ConditionalBlockModel();
            TextBlockContainer.Items.Add(ret);
        }
        public void AddTextSwitch()
        {
            var ret = new SwitchBlockModel();
            TextBlockContainer.Items.Add(ret);
        }

        public string Name
        {
            get { return _def.src; }
            set
            {
                if (value == _def.src) return;
                _def.src = value;
                RaisePropertyChanged("Name");
            }
        }
        public bool Default
        {
            get
            {
                return _def.defaultTemplate;
            }
            set
            {
                if (value == _def.defaultTemplate) return;
                _def.defaultTemplate = value;
                RaisePropertyChanged("Default");
            }
        }
        public new string Icon => Asset?.FullPath;

        public Asset Asset
        {
            get
            {
                return Asset.Load(Path.Combine(_def.rootPath, _def.src));
            }
            set
            {
                _def.src = value?.RelativePath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Icon");
            }
        }

        public override object Clone()
        {
            return new TemplateModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as TemplateModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new TemplateModel());
        }
    }

}