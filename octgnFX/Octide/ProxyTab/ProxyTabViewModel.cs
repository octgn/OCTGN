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
using Octgn.Extentions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Octgn.ProxyGenerator.Definitions;
using System.Windows.Controls;
using Octgn.Core.DataExtensionMethods;
using System.Drawing.Text;
using Octide.ItemModel;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Octide.ViewModel
{
    public class ProxyTabViewModel : ViewModelBase
    {
        
        private ProxyDefinition _proxydef => ViewModelLocator.GameLoader.ProxyDef;

        private ProxyTemplateItemModel _selectedItem;

        public ObservableCollection<ProxyTemplateItemModel> Templates { get; private set; }
        public ObservableCollection<ProxyTextItemModel> TextBlocks { get; private set; }
        public ObservableCollection<ProxyOverlayItemModel> OverlayBlocks { get; private set; }

        public ObservableCollection<ProxyInputPropertyItemModel> StoredProxyProperties { get; set; }

        public RelayCommand AddCommand { get; private set; }
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
                UpdateProxyTemplate();
            };

            Templates = new ObservableCollection<ProxyTemplateItemModel>(_proxydef.TemplateSelector.GetTemplates().Select(x => new ProxyTemplateItemModel(x)));
            Templates.CollectionChanged += (a, b) =>
            {
                _proxydef.TemplateSelector.ClearTemplates();
                foreach (var x in Templates)
                {
                    _proxydef.TemplateSelector.AddTemplate(x._def);
                }
            };

            TextBlocks = new ObservableCollection<ProxyTextItemModel>(_proxydef.BlockManager.GetBlocks().Where(x => x.type == "text").Select(x => new ProxyTextItemModel(x)));
            TextBlocks.CollectionChanged += (a, b) =>
            {
                _proxydef.BlockManager.ClearBlocks();
                foreach (var x in TextBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
                foreach (var x in OverlayBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
            };

            OverlayBlocks = new ObservableCollection<ProxyOverlayItemModel>(_proxydef.BlockManager.GetBlocks().Where(x => x.type == "overlay").Select(x => new ProxyOverlayItemModel(x)));
            OverlayBlocks.CollectionChanged += (a, b) =>
            {
                _proxydef.BlockManager.ClearBlocks();
                foreach (var x in TextBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
                foreach (var x in OverlayBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
            };

            AddCommand = new RelayCommand(AddTemplate);
            AddOverlayCommand = new RelayCommand(AddOverlay);
            RaisePropertyChanged("StoredProxyProperties");
        }
        
        public void UpdateProxyTemplate()
        {
            if (SelectedItem == null) return;

            var properties = StoredProxyProperties.Where(x => x.Name != null).ToDictionary(x => x.Name, x => x.Value);

            //this stuff generates the real proxy image, maybe we'll need to keep it in for more accurate image
            var proxy = ProxyGenerator.GenerateProxy(_proxydef.BlockManager, _proxydef.RootPath, SelectedItem._def, properties, null);
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

            BaseImage = new BitmapImage(new Uri(Path.Combine(SelectedItem._def.rootPath, SelectedItem._def.src)));
            ActiveOverlayLayers = new ObservableCollection<ProxyOverlayItemModel>(
                SelectedItem._def.GetOverLayBlocks(properties).Select(
                    x => OverlayBlocks.First(y => y.Name == x.Block)));
            ActiveTextLayers = new ObservableCollection<ProxyTextLinkItemModel>(
                SelectedItem._def.GetTextBlocks(properties).Select(
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
        
        public ObservableCollection<ProxyOverlayItemModel> ActiveOverlayLayers { get; private set; }
        public ObservableCollection<ProxyTextLinkItemModel> ActiveTextLayers { get; private set; }

        public ProxyTemplateItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                UpdateProxyTemplate();
                RaisePropertyChanged("SelectedItem");
            }
        }
        
        private ViewModelBase _activeView;

        public ViewModelBase ActiveView
        {
            get { return _activeView; }
            set
            {
                _activeView = value;
                RaisePropertyChanged("ActiveView");
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
                if (value is ProxyOverlayItemModel)
                {
                    ViewModelLocator.ProxyOverlayViewModel.SelectedItem = (ProxyOverlayItemModel)value;
                    ActiveView = ViewModelLocator.ProxyOverlayViewModel;
                }
                else if (value is ProxyTextItemModel)
                {
                    ViewModelLocator.ProxyTextBlockViewModel.SelectedItem = (ProxyTextItemModel)value;
                    ActiveView = ViewModelLocator.ProxyTextBlockViewModel;
                }
                else
                {
                    ActiveView = null;
                }
                RaisePropertyChanged("Selection");
                RaisePropertyChanged("ActiveView");
            }
        }

        public void AddTemplate()
        {
            var ret = new ProxyTemplateItemModel() { Name = "Template" };
            Templates.Add(ret);
            SelectedItem = ret;
        }

        public void AddOverlay()
        {
            var ret = new ProxyOverlayItemModel();
            OverlayBlocks.Add(ret);
            RaisePropertyChanged("OverlayBlocks");
        }
    }

    public class ProxyTextBlockItemModel : ViewModelBase
    {
        public ProxyTextItemModel TextBlock { get; private set; }
        public string Format { get; private set; }
        public CardPropertyItemModel Property { get; set; }
        public ObservableCollection<ProxyInputPropertyItemModel> Properties;

        public LinkDefinition _linkDefinition;

        public ProxyTextBlockItemModel()
        {
        }

        public ProxyTextBlockItemModel(LinkDefinition ld)
        {
            _linkDefinition = ld;
            TextBlock = ViewModelLocator.ProxyTabViewModel.TextBlocks.First(x => x.Name == ld.Block);
        }

        public string GetText(Dictionary<string, string> properties)
        {
            List<string> propertyValueList = new List<string>();
            foreach (var propDef in _linkDefinition.NestedProperties)
            {
                if (properties.ContainsKey(propDef.Name))
                    propertyValueList.Add(string.Format(propDef.Format, properties.First(x => x.Key == propDef.Name).Value));
            }
            return string.Join(_linkDefinition.Separator, propertyValueList);
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

        public ProxyTextItemModel TextBlock
        {
            get
            {
                return ViewModelLocator.ProxyTabViewModel.TextBlocks.First(x => x.Name == _linkDefinition.Block);
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
                ViewModelLocator.ProxyTabViewModel.UpdateProxyTemplate();
                RaisePropertyChanged("Name");
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
                ViewModelLocator.ProxyTabViewModel.UpdateProxyTemplate();
                RaisePropertyChanged("Value");
            }
        }
    }
}