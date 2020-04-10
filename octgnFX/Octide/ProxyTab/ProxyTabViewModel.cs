// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using FontFamily = System.Windows.Media.FontFamily;
using System.IO;
using System.Drawing;
using Font = Octgn.DataNew.Entities.Font;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octide.Messages;
using Octgn.DataNew.Entities;
using Octgn.ProxyGenerator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Octgn.ProxyGenerator.Definitions;
using System.Windows.Controls;
using Octgn.Core.DataExtensionMethods;
using System.Drawing.Text;
using Octide.ItemModel;
using Octide.ProxyTab.TemplateItemModel;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Collections.Specialized;

namespace Octide.ViewModel
{
    public class ProxyTabViewModel : ViewModelBase
    {
        
        private ProxyDefinition _proxydef => ViewModelLocator.GameLoader.ProxyDef;
        public IdeCollection<IdeBaseItem> Templates { get; private set; }
        public IdeCollection<IdeBaseItem> TextBlocks { get; private set; }
        public IdeCollection<IdeBaseItem> OverlayBlocks { get; private set; }

        public ObservableCollection<ProxyInputPropertyItemModel> StoredProxyProperties { get; set; }

        public RelayCommand AddTextBlockCommand { get; private set; }
        public RelayCommand AddTemplateCommand { get; private set; }
        public RelayCommand AddOverlayCommand { get; private set; }

        public ProxyTabViewModel()
        {
            StoredProxyProperties = new ObservableCollection<ProxyInputPropertyItemModel>
            {
                new ProxyInputPropertyItemModel("Name", "Card"),
                new ProxyInputPropertyItemModel("CardSizeName", "Size"),
                new ProxyInputPropertyItemModel("Type", "Creature"),
                new ProxyInputPropertyItemModel("Color", "Red"),
                new ProxyInputPropertyItemModel("Rules", "LOL FREE CARD")
            };
            
            StoredProxyProperties.CollectionChanged += (a, b) =>
            {
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            };

            Templates = new IdeCollection<IdeBaseItem>(this);
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
                ((TemplateModel)args.OldItem)._def.defaultTemplate = false;
                ((TemplateModel)args.NewItem)._def.defaultTemplate = true;
            };

            TextBlocks = new IdeCollection<IdeBaseItem>(this);
            foreach (var textblock in _proxydef.BlockManager.GetBlocks().Where(x => x.type == "text"))
            {
                TextBlocks.Add(new ProxyTextDefinitionItemModel(textblock, TextBlocks));
            }
            TextBlocks.CollectionChanged += (sender, args) =>
            {
                _proxydef.BlockManager.ClearBlocks();
                foreach (ProxyTextDefinitionItemModel x in TextBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
                foreach (ProxyOverlayDefinitionItemModel x in OverlayBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
            };

            OverlayBlocks = new IdeCollection<IdeBaseItem>(this);
            foreach (var overlayblock in _proxydef.BlockManager.GetBlocks().Where(x => x.type == "overlay"))
            {
                OverlayBlocks.Add(new ProxyOverlayDefinitionItemModel(overlayblock, OverlayBlocks));
            }
            OverlayBlocks.CollectionChanged += (sender, args) =>
            {
                _proxydef.BlockManager.ClearBlocks();
                foreach (ProxyTextDefinitionItemModel x in TextBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
                foreach (ProxyOverlayDefinitionItemModel x in OverlayBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
            };

            AddTextBlockCommand = new RelayCommand(AddTextBlock);
            AddTemplateCommand = new RelayCommand(AddTemplate);
            AddOverlayCommand = new RelayCommand(AddOverlay);
            RaisePropertyChanged("StoredProxyProperties");
            Messenger.Default.Register<ProxyTemplateChangedMessage>(this, action => UpdateProxyTemplate(action));
        }
        
        public void UpdateProxyTemplate(ProxyTemplateChangedMessage message)
        {
            if (SelectedTemplate == null) return;

            var properties = StoredProxyProperties.Where(x => x.Name != null).ToDictionary(x => x.Name, x => x.Value);

            //this stuff generates the real proxy image, maybe we'll need to keep it in for more accurate image
            var proxy = ProxyGenerator.GenerateProxy(_proxydef.BlockManager, _proxydef.RootPath, SelectedTemplate._def, properties, null);
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

            BaseImage = new BitmapImage(new Uri(Path.Combine(SelectedTemplate._def.rootPath, SelectedTemplate._def.src)));
            ActiveOverlayLayers = new ObservableCollection<ProxyOverlayDefinitionItemModel>(
                SelectedTemplate._def.GetOverLayBlocks(properties).Where(x => x.SpecialBlock == null).Select(
                    x => (ProxyOverlayDefinitionItemModel)OverlayBlocks.First(y => ((ProxyOverlayDefinitionItemModel)y).Name == x.Block)));
            ActiveTextLayers = new ObservableCollection<ProxyTextLinkItemModel>(
                SelectedTemplate._def.GetTextBlocks(properties).Select(
                    x => new ProxyTextLinkItemModel(x) ));

            RaisePropertyChanged("BaseImage");
            RaisePropertyChanged("BaseWidth");
            RaisePropertyChanged("BaseHeight");
            RaisePropertyChanged("");
        }

        public BitmapImage ProxyImage { get; private set; }

        public BitmapImage BaseImage { get; private set; }

        public double BaseWidth => BaseImage?.PixelWidth ?? 0;

        public double BaseHeight => BaseImage?.PixelHeight ?? 0;
        
        public ObservableCollection<ProxyOverlayDefinitionItemModel> ActiveOverlayLayers { get; private set; }
        public ObservableCollection<ProxyTextLinkItemModel> ActiveTextLayers { get; private set; }

        public TemplateModel _selectedTemplate;

        public TemplateModel SelectedTemplate
        {
            get
            {
                return _selectedTemplate;
            }
            set
            {
                if (value == _selectedTemplate) return;
                _selectedTemplate = value;
                Selection = null;
                RaisePropertyChanged("SelectedTemplate");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }
        
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
            SelectedTemplate = ret;
        }

        public void AddOverlay()
        {
            var ret = new ProxyOverlayDefinitionItemModel(OverlayBlocks);
            OverlayBlocks.Add(ret);
            Selection = ret;
        }
        public void AddTextBlock()
        {
            var ret = new ProxyTextDefinitionItemModel(TextBlocks);
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

        public ProxyTextDefinitionItemModel TextBlock
        {
            get
            {
                return (ProxyTextDefinitionItemModel)ViewModelLocator.ProxyTabViewModel.TextBlocks.First(x => ((ProxyTextDefinitionItemModel)x).Name == _linkDefinition.Block);
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