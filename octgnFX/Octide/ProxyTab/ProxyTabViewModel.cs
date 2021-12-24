// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew;
using Octgn.ProxyGenerator;
using Octgn.ProxyGenerator.Definitions;
using Octide.ItemModel;
using Octide.Messages;
using Octide.ProxyTab.ItemModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Octide.ViewModel
{
    public class ProxyTabViewModel : ViewModelBase
    {
        //TODO: Add a toggle to ignore the proxygen, blanking the src link and disabling the tab contents

        public ProxyDefinition _proxydef;

        public AssetController Asset { get; set; }
        public IdeCollection<IdeBaseItem> Templates { get; private set; }
        public IdeCollection<IdeBaseItem> TextBlocks { get; private set; }
        public IdeCollection<IdeBaseItem> OverlayBlocks { get; private set; }

        public ObservableCollection<ProxyInputPropertyItemModel> StoredProxyProperties { get; set; }

        public RelayCommand AddTextBlockCommand { get; private set; }
        public RelayCommand AddTemplateCommand { get; private set; }
        public RelayCommand AddOverlayCommand { get; private set; }

        public ProxyTabViewModel()
        {
            StoredProxyProperties = new ObservableCollection<ProxyInputPropertyItemModel>()
            {
                new ProxyInputPropertyItemModel("Type", "Creature"),
                new ProxyInputPropertyItemModel("Color", "Blue")
            };

            StoredProxyProperties.CollectionChanged += (a, b) =>
            {
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            };
            var game = ViewModelLocator.GameLoader.Game;

            var proxySerializer = new ProxyGeneratorSerializer(game.Id) { Game = game };

            if (game.ProxyGenSource == null)
            {
                _proxydef = new ProxyDefinition();
                var proxyAsset = ViewModelLocator.AssetsTabViewModel.LoadExternalAsset(new FileInfo("dummy/proxydef.xml"), new string[] { "Proxy"});

                proxyAsset.IsReserved = true;
                Asset = new AssetController(AssetType.Xml);
                Asset.SelectedAsset = proxyAsset;
            }
            else
            {
                Asset = new AssetController(AssetType.Xml);
                Asset.Register(game.ProxyGenSource);
                //TODO: Catch if the target xml is not a valid proxy definition
                _proxydef = (ProxyDefinition)proxySerializer.Deserialize(game.ProxyGenSource);
            }
            Asset.PropertyChanged += AssetUpdated;
            if (Asset.SelectedAsset != null)
            {
                Asset.SelectedAsset.IsReserved = true;
            }

            Templates = new IdeCollection<IdeBaseItem>(this, typeof(TemplateModel));
            foreach (TemplateDefinition templateDef in _proxydef.TemplateSelector.GetTemplates())
            {
                var template = new TemplateModel(templateDef, Templates);
                Templates.Add(template);
                if (templateDef.defaultTemplate)
                {
                    Templates.DefaultItem = template;
                }
            }

            Templates.CollectionChanged += (sender, args) =>
            {
                _proxydef.TemplateSelector.ClearTemplates();
                foreach (TemplateModel x in Templates)
                {
                    _proxydef.TemplateSelector.AddTemplate(x._def);
                }
            };
            Templates.DefaultItemChanged += (sender, args) =>
            {
                if (args.OldItem != null)
                    ((TemplateModel)args.OldItem)._def.defaultTemplate = false;
                ((TemplateModel)args.NewItem)._def.defaultTemplate = true;
            };
            Templates.SelectedItemChanged += (sender, args) =>
            {
                Selection = null;
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            };

            if (Templates.Count == 0)
            {
                // Games require a default template.  This will add a default template in if there isn't any templates defined.  This will happen when a new blank game is created through the IDE.
                var defTemplate = new TemplateModel(Templates);
                Templates.Add(defTemplate);
                Templates.DefaultItem = defTemplate;

            }

            TextBlocks = new IdeCollection<IdeBaseItem>(this, typeof(TextBlockDefinitionItemModel));
            foreach (var textblock in _proxydef.BlockManager.GetBlocks().Where(x => x.type == "text"))
            {
                TextBlocks.Add(new TextBlockDefinitionItemModel(textblock, TextBlocks));
            }
            TextBlocks.CollectionChanged += (sender, args) =>
            {
                _proxydef.BlockManager.ClearBlocks();
                foreach (TextBlockDefinitionItemModel x in TextBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
                foreach (OverlayBlockDefinitionItemModel x in OverlayBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
            };

            OverlayBlocks = new IdeCollection<IdeBaseItem>(this, typeof(OverlayBlockDefinitionItemModel));
            foreach (var overlayblock in _proxydef.BlockManager.GetBlocks().Where(x => x.type == "overlay"))
            {
                OverlayBlocks.Add(new OverlayBlockDefinitionItemModel(overlayblock, OverlayBlocks));
            }
            OverlayBlocks.CollectionChanged += (sender, args) =>
            {
                _proxydef.BlockManager.ClearBlocks();
                foreach (TextBlockDefinitionItemModel x in TextBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
                foreach (OverlayBlockDefinitionItemModel x in OverlayBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
            };

            AddTextBlockCommand = new RelayCommand(AddTextBlock);
            AddTemplateCommand = new RelayCommand(AddTemplate);
            AddOverlayCommand = new RelayCommand(AddOverlay);
            RaisePropertyChanged("StoredProxyProperties");
            Messenger.Default.Register<ProxyTemplateChangedMessage>(this, UpdateProxyTemplate);
        }

        public override void Cleanup()
        {
            Asset.Cleanup();
            base.Cleanup();
        }

        public void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                ViewModelLocator.GameLoader.Game.ProxyGenSource = Asset.FullPath;
            }
        }

        public void UpdateProxyTemplate(ProxyTemplateChangedMessage message)
        {
            if (Templates.SelectedItem == null) return;
            var selectedTemplate = (TemplateModel)Templates.SelectedItem;

            var properties = StoredProxyProperties.Where(x => x.Name != null).ToDictionary(x => x.Name, x => x.Value);

            //this stuff generates the real proxy image, maybe we'll need to keep it in for more accurate image
            var proxy = ProxyGenerator.GenerateProxy(_proxydef.BlockManager, selectedTemplate._def, properties, null);
            using (var ms = new MemoryStream())
            {
                proxy.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                ProxyImage = new BitmapImage();
                ProxyImage.BeginInit();
                ProxyImage.CacheOption = BitmapCacheOption.OnLoad;
                ProxyImage.StreamSource = ms;
                ProxyImage.EndInit();
            }
            proxy.Dispose();

            BaseImage = new BitmapImage(new Uri(selectedTemplate._def.src));

            ActiveOverlayLayers = new ObservableCollection<IdeBaseItem>();
            foreach (var linkDefinition in selectedTemplate._def.GetOverLayBlocks(properties))
            {
                if (linkDefinition.SpecialBlock == null)
                {
                    ActiveOverlayLayers.Add((OverlayBlockDefinitionItemModel)OverlayBlocks.First(x => ((OverlayBlockDefinitionItemModel)x).Name == linkDefinition.Block));
                }
                else
                {
                    ActiveOverlayLayers.Add(new ArtCropDefinitionItemModel(linkDefinition.SpecialBlock, new IdeCollection<IdeBaseItem>())
                    {
                    });
                }
            }
            //  ActiveOverlayLayers = new ObservableCollection<OverlayBlockDefinitionItemModel>(
            //      selectedTemplate._def.GetOverLayBlocks(properties).Where(x => x.SpecialBlock == null).Select(
            //          x =>  (OverlayBlockDefinitionItemModel)OverlayBlocks.First(y => ((OverlayBlockDefinitionItemModel)y).Name == x.Block)));
            ActiveTextLayers = new ObservableCollection<ProxyTextLinkItemModel>(
                selectedTemplate._def.GetTextBlocks(properties).Select(
                    x => new ProxyTextLinkItemModel(x)));

            RaisePropertyChanged("BaseImage");
            RaisePropertyChanged("BaseWidth");
            RaisePropertyChanged("BaseHeight");
            RaisePropertyChanged("");
        }

        public BitmapImage ProxyImage { get; private set; }

        public BitmapImage BaseImage { get; private set; }

        public double BaseWidth => BaseImage?.PixelWidth ?? 0;

        public double BaseHeight => BaseImage?.PixelHeight ?? 0;

        public ObservableCollection<IdeBaseItem> ActiveOverlayLayers { get; private set; }
        public ObservableCollection<ProxyTextLinkItemModel> ActiveTextLayers { get; private set; }


        public object _selection;

        public object Selection
        {
            get
            {
                return _selection;
            }
            set
            {
                if (value == _selection) return;
                _selection = value;
                RaisePropertyChanged("Selection");
            }
        }

        public void AddTemplate()
        {
            var ret = new TemplateModel(Templates);
            Templates.Add(ret);
        }

        public void AddOverlay()
        {
            var ret = new OverlayBlockDefinitionItemModel(OverlayBlocks);
            OverlayBlocks.Add(ret);
            Selection = ret;
        }
        public void AddTextBlock()
        {
            var ret = new TextBlockDefinitionItemModel(TextBlocks);
            TextBlocks.Add(ret);
            Selection = ret;
        }
    }

    public class ProxyTextLinkItemModel : ViewModelBase
    {
        public LinkDefinition _linkDefinition;

        public ProxyTextLinkItemModel()
        { }

        public ProxyTextLinkItemModel(LinkDefinition link)
        {
            _linkDefinition = link;
        }

        public TextBlockDefinitionItemModel TextBlock
        {
            get
            {
                return (TextBlockDefinitionItemModel)ViewModelLocator.ProxyTabViewModel.TextBlocks.First(x => ((TextBlockDefinitionItemModel)x).Name == _linkDefinition.Block);
            }
        }

        public string Value
        {
            get
            {
                var ret = _linkDefinition.NestedProperties.Select(x => ViewModelLocator.ProxyTabViewModel.StoredProxyProperties.FirstOrDefault(y => y.Name == x.Name)?.Value);
                return string.Join(_linkDefinition.Separator, ret);
            }
        }


    }

    public class ProxyInputPropertyItemModel : ViewModelBase
    {
        public string _name;
        public string _value;

        public ProxyInputPropertyItemModel()
        {
        }

        public ProxyInputPropertyItemModel(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == _name) return;
                if (ViewModelLocator.ProxyTabViewModel.StoredProxyProperties.FirstOrDefault(x => x.Name == value) != null) return;
                _name = value;
                RaisePropertyChanged("Name");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value == _value) return;
                _value = value;
                RaisePropertyChanged("Value");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }
    }
}