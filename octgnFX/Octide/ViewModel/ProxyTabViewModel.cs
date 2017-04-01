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

namespace Octide.ViewModel
{
    public class ProxyTabViewModel : ViewModelBase
    {

        private Visibility _panelVisibility;
        private ProxyDefinition _proxydef;
        private ProxyTemplateItemModel _selectedItem;
        public ObservableCollection<ProxyTemplateItemModel> Templates { get; private set; }
        public ObservableCollection<ProxyTextItemModel> TextBlocks { get; private set; }
        public ObservableCollection<ProxyOverlayItemModel> OverlayBlocks { get; private set; }

        public RelayCommand AddCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }


        public ProxyTabViewModel()
        {
            _proxydef = ViewModelLocator.GameLoader.ProxyDef;
            PanelVisibility = Visibility.Collapsed;
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
            RemoveCommand = new RelayCommand(RemoveTemplate, EnableButton);
        }

        public ProxyTemplateItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                if (value == null) PanelVisibility = Visibility.Collapsed;
                else PanelVisibility = Visibility.Visible;
                RaisePropertyChanged("SelectedItem");
                RemoveCommand.RaiseCanExecuteChanged();
            }
        }
        
        public Visibility PanelVisibility
        {
            get { return _panelVisibility; }
            set
            {
                if (value == _panelVisibility) return;
                _panelVisibility = value;
                RaisePropertyChanged("PanelVisibility");
            }
        }

        public bool EnableButton()
        {
            return _selectedItem != null;
        }

        public void AddTemplate()
        {
            var ret = new ProxyTemplateItemModel() { Name = "Template" };
            Templates.Add(ret);
            SelectedItem = ret;
        }

        public void RemoveTemplate()
        {
            Templates.Remove(SelectedItem);
        }
    }

    public class ProxyTemplateItemModel : ViewModelBase
    {
        public TemplateDefinition _def;

        public ProxyTemplateItemModel()
        {
            _def = new TemplateDefinition();
        }

        public ProxyTemplateItemModel(TemplateDefinition t)
        {
            _def = t;
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
    }

    public class ProxyOverlayItemModel : ViewModelBase
    {
        public BlockDefinition _def;
        public int _width;
        public int _height;

        public ProxyOverlayItemModel()
        {
            _def = new BlockDefinition();
            
        }

        public ProxyOverlayItemModel(BlockDefinition b)
        {
            _def = b;
            System.Drawing.Image img = System.Drawing.Image.FromFile(Path);
            Width = img.Width;
            Height = img.Height;

        }

        public string Id
        {
            get { return _def.id; }
            set
            {
                if (value == _def.id) return;
                _def.id = value;
                RaisePropertyChanged("Id");
            }
        }

        public int Left
        {
            get
            {
                return _def.location.x;
            }
        }

        public int Top
        {
            get
            {
                return _def.location.y;
            }
        }

        public string Path
        {
            get
            {
                return System.IO.Path.Combine(_def.Manager.RootPath, _def.src);
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }
            set
            {
                if (_width == value) return;
                _width = value;
                RaisePropertyChanged("Width");
            }
        }
        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                if (_height == value) return;
                _height = value;
                RaisePropertyChanged("Height");
            }
        }
    }

    public class ProxyTextItemModel : ViewModelBase
    {
        public BlockDefinition _def;
        public FontFamily _font;

        public ProxyTextItemModel()
        {
            _def = new BlockDefinition();
        }

        public ProxyTextItemModel(BlockDefinition b)
        {
            _def = b;
            
            var font = new Font();
            font.Size = b.text.size;
            font.Src = Path.Combine(b.Manager.RootPath, b.text.font);
            _font = font.GetFontFamily(new FontFamily("Arial"));

        }

        public string Id
        {
            get { return _def.id; }
            set
            {
                if (value == _def.id) return;
                _def.id = value;
                RaisePropertyChanged("Id");
            }
        }

        public FontFamily Font
        {
            get
            {
                return _font;
            }
        }

        public int Left { get { return _def.location.x; } }

        public int Top { get { return _def.location.y; } }

        public int Width { get { return _def.wordwrap.width; } }

        public int Height { get { return _def.wordwrap.height; } }

        public int FontSize { get { return _def.text.size; } }

        public Color FontColor { get { return _def.text.color; } }

        public VerticalAlignment VAlign
        {
            get
            {
                if (_def.wordwrap.valign == "center") return VerticalAlignment.Center;
                if (_def.wordwrap.valign == "far") return VerticalAlignment.Bottom;
                return VerticalAlignment.Top;
            }
        }
    }
}