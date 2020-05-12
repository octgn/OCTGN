// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.ProxyGenerator.Definitions;
using Octide.ProxyTab.Handlers;
using Octide.ViewModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Octide.ProxyTab.ItemModel
{
    public class TemplateModel : IdeBaseItem
    {
        public TemplateDefinition _def;

        public IdeCollection<IdeBaseItem> Matches { get; private set; }

        public RelayCommand AddMatchCommand { get; private set; }
        public RelayCommand AddOverlayConditionalCommand { get; private set; }
        public RelayCommand AddOverlaySwitchCommand { get; private set; }
        public RelayCommand AddTextConditionalCommand { get; private set; }
        public RelayCommand AddTextSwitchCommand { get; private set; }
        public BlockContainer OverlayContainer { get; set; }
        public BlockContainer TextBlockContainer { get; set; }

        public AssetController Asset { get; set; }

        public TemplateMainDropHandler OverlayDropHandler { get; set; }
        public TemplateMainDropHandler TextDropHandler { get; set; }
        public TemplateMainDragHandler DragHandler { get; set; }

        public TemplateModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            CanBeDefault = true;
            CanEdit = false;
            _def = new TemplateDefinition
            {
                Matches = new List<Property>(),
                OverlayBlocks = new List<LinkDefinition.LinkWrapper>(),
                TextBlocks = new List<LinkDefinition.LinkWrapper>(),
                rootPath = ViewModelLocator.ProxyTabViewModel._proxydef.RootPath
            };

            Asset = new AssetController(AssetType.Image);
            _def.src= Asset.SelectedAsset.RelativePath;
            Asset.PropertyChanged += AssetUpdated;


            Matches = new IdeCollection<IdeBaseItem>(this);
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
            AddMatchCommand = new RelayCommand(AddMatch);
            AddOverlayConditionalCommand = new RelayCommand(AddOverlayConditional);
            AddOverlaySwitchCommand = new RelayCommand(AddOverlaySwitch);
            AddTextConditionalCommand = new RelayCommand(AddTextConditional);
            AddTextSwitchCommand = new RelayCommand(AddTextSwitch);
            OverlayDropHandler = new TemplateMainDropHandler() { IsOverlayHandler = true };
            TextDropHandler = new TemplateMainDropHandler() { IsOverlayHandler = false };
            DragHandler = new TemplateMainDragHandler();
        }

        public TemplateModel(TemplateDefinition t, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            CanBeDefault = true;
            CanEdit = false;
            _def = t;

            Asset = new AssetController(AssetType.Image, Path.Combine(t.rootPath, t.src));
            Asset.PropertyChanged += AssetUpdated;

            Matches = new IdeCollection<IdeBaseItem>(this);
            foreach (var match in t.Matches)
            {
                Matches.Add(new MatchModel(match, Matches));
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
            OverlayDropHandler = new TemplateMainDropHandler() { IsOverlayHandler = true };
            TextDropHandler = new TemplateMainDropHandler() { IsOverlayHandler = false };
            DragHandler = new TemplateMainDragHandler();
        }


        public TemplateModel(TemplateModel p, IdeCollection<IdeBaseItem> source) : base(source) //copy
        {
            CanBeDefault = true;
            CanEdit = false;
            _def = new TemplateDefinition()
            {
                OverlayBlocks = p._def.OverlayBlocks,
                TextBlocks = p._def.TextBlocks,
                Matches = p._def.Matches,
                rootPath = p._def.rootPath
            };

            Asset = new AssetController(AssetType.Image, Path.Combine(p._def.rootPath, p._def.src));
            _def.src = Asset.SelectedAsset.RelativePath;
            Asset.PropertyChanged += AssetUpdated;

            Matches = new IdeCollection<IdeBaseItem>(this);
            Matches.CollectionChanged += (a, b) =>
            {
                BuildMatchDef(b);
            };
            foreach (MatchModel match in p.Matches)
            {
                Matches.Add(new MatchModel(match, Matches));
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
            OverlayDropHandler = new TemplateMainDropHandler() { IsOverlayHandler = true };
            TextDropHandler = new TemplateMainDropHandler() { IsOverlayHandler = false };
            DragHandler = new TemplateMainDragHandler();

        }

        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedAsset")
            {
                _def.src = Asset.SelectedAsset.RelativePath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Icon");
            }
        }
        public override void Cleanup()
        {
            Asset.SelectedAsset = null;
            base.Cleanup();
        }

        public override object Clone()
        {
            return new TemplateModel(this, Source);
        }
        public override object Create()
        {
            return new TemplateModel(Source);
        }

        public void BuildMatchDef(NotifyCollectionChangedEventArgs args)
        {
            _def.Matches = Matches.Select(x => ((MatchModel)x)._match).ToList();
        }

        public void AddMatch()
        {
            var ret = new MatchModel(Matches);
            Matches.Add(ret);
        }

        public void AddOverlayConditional()
        {
            var ret = new ConditionalBlockModel(OverlayContainer.Items);
            OverlayContainer.Items.Add(ret);
        }
        public void AddOverlaySwitch()
        {
            var ret = new SwitchBlockModel(OverlayContainer.Items);
            OverlayContainer.Items.Add(ret);
        }
        public void AddTextConditional()
        {
            var ret = new ConditionalBlockModel(TextBlockContainer.Items);
            TextBlockContainer.Items.Add(ret);
        }
        public void AddTextSwitch()
        {
            var ret = new SwitchBlockModel(TextBlockContainer.Items);
            TextBlockContainer.Items.Add(ret);
        }

        public string Name
        {
            get
            {
                return _def.src;
            }
            set
            {
                if (_def.src == value) return;
                _def.src = value;
                RaisePropertyChanged("Name");
            }
        }
        public new string Icon => Asset.SelectedAsset?.FullPath;

    }
}