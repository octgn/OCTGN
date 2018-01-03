using GalaSoft.MvvmLight;
using Octgn.DataNew.Entities;
using Octgn.Extentions;
using Octgn.ProxyGenerator.Definitions;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using Font = Octgn.DataNew.Entities.Font;
using FontFamily = System.Windows.Media.FontFamily;

namespace Octide.ItemModel
{
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
            _def.src = ViewModelLocator.ProxyTabViewModel.Images.First().RelativePath;

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

        public Asset Overlay
        {
            get
            {
                if (_def.src == null)
                    return new Asset();
                return Asset.Load(Path);
            }
            set
            {
                if (value == null) return;
                if (!File.Exists(value.FullPath)) return;
                _def.src = value.RelativePath;
                RaisePropertyChanged("Overlay");
                RaisePropertyChanged("OverlayImage");
            }
        }

        public string OverlayImage => Path;

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

        public TextWrapping WordWrap
        {
            get
            {
                return (_def.wordwrap.height != 0) ? TextWrapping.Wrap : TextWrapping.NoWrap;
            }
        }

        public HorizontalAlignment HAlign
        {
            get
            {
                if (_def.wordwrap.align == "center") return HorizontalAlignment.Center;
                if (_def.wordwrap.align == "far") return HorizontalAlignment.Right;
                return HorizontalAlignment.Left;
            }
        }

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
