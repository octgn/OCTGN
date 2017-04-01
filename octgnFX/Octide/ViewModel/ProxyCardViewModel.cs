using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.IO;

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
using System.Windows.Media.Imaging;
using System.Drawing;

namespace Octide.ViewModel
{
    public class ProxyCardViewModel : ViewModelBase
    {
        public AltItemModel _card;
        private ProxyDefinition _proxydef;
        private ObservableCollection<ProxyOverlayItemModel> _activeLayers;
        private ObservableCollection<ProxyTextBlockItemModel> _activeTextLayers;

        public int _baseWidth;
        public int _baseHeight;

        public string baseImage { get; private set; }

        public ProxyCardViewModel()
        {
            _proxydef = ViewModelLocator.GameLoader.ProxyDef;
        }

        public AltItemModel Card
        {
            get
            {
                return _card;
            }
            set
            {
               // if (_card == value) return;
                _card = value;
                if (value != null)
                {
                    var properties = _card._tempImageCard.GetProxyMappings();
                    ProxyTemplateItemModel activeTemplate = ViewModelLocator.ProxyTabViewModel.Templates.First(x => x._def.defaultTemplate == true);

                    foreach (var template in ViewModelLocator.ProxyTabViewModel.Templates)
                    {
                        bool validTemplate = true;
                        foreach (var match in template._def.Matches)
                        {
                            if (!properties.ContainsKey(match.Name) || properties[match.Name] != match.Value)
                            {
                                validTemplate = false;
                                break;
                            }
                        }
                        if (validTemplate == true)
                        {
                            activeTemplate = template;
                            break;
                        }
                    }
                    baseImage = Path.Combine(activeTemplate._def.rootPath, activeTemplate._def.src);
                    BitmapImage image = new BitmapImage(new Uri(baseImage));
                    BaseWidth = image.PixelWidth;
                    BaseHeight = image.PixelHeight;
                    
                    ActiveLayers = new ObservableCollection<ProxyOverlayItemModel>(activeTemplate._def.GetOverLayBlocks(properties).Select(x => ViewModelLocator.ProxyTabViewModel.OverlayBlocks.First(y => y.Id == x.Block)));
                   // ActiveTextLayers = new ObservableCollection<ProxyTextItemModel>(activeTemplate._def.GetTextBlocks(properties).Select(x => ViewModelLocator.ProxyTabViewModel.TextBlocks.First(y => y.Id == x.Block)));
                    ActiveTextLayers = new ObservableCollection<ProxyTextBlockItemModel>(activeTemplate._def.GetTextBlocks(properties).Select(x => new ProxyTextBlockItemModel(x)));

                }


                RaisePropertyChanged("Card");
                RaisePropertyChanged("");
            }
        }

        public int BaseWidth
        {
            get
            {
                return _baseWidth;
            }
            set
            {
                if (_baseWidth == value) return;
                _baseWidth = value;
                RaisePropertyChanged("BaseWidth");
            }
        }

        public int BaseHeight
        {
            get
            {
                return _baseHeight;
            }
            set
            {
                if (_baseHeight == value) return;
                _baseHeight = value;
                RaisePropertyChanged("BaseHeight");
            }
        }

        public ObservableCollection<ProxyOverlayItemModel> ActiveLayers
        {
            get
            {
                return _activeLayers;
            }
            set
            {
                if (_activeLayers == value) return;
                _activeLayers = value;
                RaisePropertyChanged("ActiveLayers");
            }
        }
        
        public ObservableCollection<ProxyTextBlockItemModel> ActiveTextLayers
        {
            get
            {
                return _activeTextLayers;
            }
            set
            {
                if (_activeTextLayers == value) return;
                _activeTextLayers = value;
                RaisePropertyChanged("ActiveTextLayers");
            }
        }

    }

    public class ProxyTextBlockItemModel : ViewModelBase
    {
        public ProxyTextItemModel TextBlock { get; private set; }
        private string _property;

        public ProxyTextBlockItemModel()
        {
        }

        public ProxyTextBlockItemModel(LinkDefinition ld)
        {
            TextBlock = ViewModelLocator.ProxyTabViewModel.TextBlocks.First(x => x.Id == ld.Block);
            _property = ld.NestedProperties.First().Name;
        }

        public object Text
        {
            get
            {
                if (_property == "Name")
                {
                    return ViewModelLocator.ProxyDesignViewModel.Card.Name;
                }
                else
                {
                    return ViewModelLocator.ProxyDesignViewModel.Card.Properties.First(x => x.Name == _property).Value;
                }
            }
            set
            {
                if (_property == "Name")
                {
                    var prop = ViewModelLocator.ProxyDesignViewModel.Card.Name;
                    if (prop == value.ToString()) return;
                    prop = value.ToString();
                }
                else
                {
                    var prop = ViewModelLocator.ProxyDesignViewModel.Card.Properties.First(x => x.Name == _property);
                    if (prop.Value == value) return;
                    prop.Value = value;
                }
                RaisePropertyChanged("Text");
            }

        }
    }
}