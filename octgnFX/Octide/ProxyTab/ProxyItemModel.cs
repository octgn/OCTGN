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
using Font = Octgn.DataNew.Entities.Font;
using FontFamily = System.Windows.Media.FontFamily;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Octide.ItemModel
{
    public class ProxyTemplateItemModel : IdeListBoxItemBase
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

        public ProxyTemplateItemModel(ProxyTemplateItemModel p)
        {
            _def = new TemplateDefinition()
            {
                OverlayBlocks = p._def.OverlayBlocks,
                TextBlocks = p._def.TextBlocks,
                Matches = p._def.Matches,
                rootPath = p._def.rootPath
            };
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

        public override object Clone()
        {
            return new ProxyTemplateItemModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ViewModelLocator.ProxyTabViewModel.Templates.IndexOf(this);
            ViewModelLocator.ProxyTabViewModel.Templates.Insert(index, Clone() as ProxyTemplateItemModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ViewModelLocator.ProxyTabViewModel.Templates.IndexOf(this);
            ViewModelLocator.ProxyTabViewModel.Templates.Insert(index, new ProxyTemplateItemModel());
        }
    }

    public class ProxyOverlayItemModel : IdeListBoxItemBase
    {
        public BlockDefinition _def;

        public ProxyOverlayItemModel() // new overlay
        {
            _def = new BlockDefinition
            {
                src = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.RelativePath,
                type = "overlay",
                id = Utils.GetUniqueName("overlay", ViewModelLocator.ProxyTabViewModel.OverlayBlocks.Select(x => x.Name))
            };
        }

        public ProxyOverlayItemModel(BlockDefinition b) // load overlay
        {
            _def = b;
        }

        public ProxyOverlayItemModel(ProxyOverlayItemModel o)
        {
            _def = new BlockDefinition
            {
                src = o.Asset.RelativePath,
                id = Utils.GetUniqueName(o.Name, ViewModelLocator.ProxyTabViewModel.OverlayBlocks.Select(x => x.Name))
            };
        }
        
        public string Name
        {
            get { return _def.id; }
            set
            {
                if (value == _def.id) return; if (string.IsNullOrEmpty(value)) return;
                _def.id = Utils.GetUniqueName(value, ViewModelLocator.ProxyTabViewModel.OverlayBlocks.Select(x => x.Name));

                RaisePropertyChanged("Name");
            }
        }

        public Asset Asset
        {
            get
            {
                if (_def.src == null)
                    return new Asset();
                return Asset.Load(Path.Combine(_def.Manager.RootPath, _def.src));
            }
            set
            {
                _def.src = value?.RelativePath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Width");
                RaisePropertyChanged("Height");
            }
        }

        public int Left
        {
            get
            {
                return _def.location.x;
            }
            set
            {
                if (_def.location.x == value) return;
                _def.location.x = value;
                RaisePropertyChanged("Left");
            }
        }

        public int Top
        {
            get
            {
                return _def.location.y;
            }
            set
            {
                if (_def.location.y == value) return;
                _def.location.y = value;
                RaisePropertyChanged("Top");
            }
        }

        public int Width
        {
            get
            {
                using (var img = System.Drawing.Image.FromFile(Asset?.FullPath))
                {
                    return img.Width;
                }
            }
        }
        public int Height
        {
            get
            {
                using (var img = System.Drawing.Image.FromFile(Asset?.FullPath))
                {
                    return img.Height;
                }
            }
        }

        public override object Clone()
        {
            return new ProxyOverlayItemModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ViewModelLocator.ProxyTabViewModel.OverlayBlocks.IndexOf(this);
            ViewModelLocator.ProxyTabViewModel.OverlayBlocks.Insert(index, Clone() as ProxyOverlayItemModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ViewModelLocator.ProxyTabViewModel.OverlayBlocks.IndexOf(this);
            ViewModelLocator.ProxyTabViewModel.OverlayBlocks.Insert(index, new ProxyOverlayItemModel());
        }
    }

    public class ProxyTextItemModel : IdeListBoxItemBase
    {
        public BlockDefinition _def;

        public ProxyTextItemModel() // new overlay
        {
            _def = new BlockDefinition
            {
                type = "text",
                id = Utils.GetUniqueName("text", ViewModelLocator.ProxyTabViewModel.TextBlocks.Select(x => x.Name))
            };
        }

        public ProxyTextItemModel(BlockDefinition b)
        {
            _def = b;

            var font = new Font
            {
                Size = b.text.size,
                Src = System.IO.Path.Combine(b.Manager.RootPath, b.text.font ?? "")
            };
        }


        public ProxyTextItemModel(ProxyTextItemModel t)
        {
            _def = new BlockDefinition
            {
                id = Utils.GetUniqueName(t.Name, ViewModelLocator.ProxyTabViewModel.TextBlocks.Select(x => x.Name))
            };
        }

        public string Name
        {
            get { return _def.id; }
            set
            {
                if (value == _def.id) return;
                if (string.IsNullOrEmpty(value)) return;
                _def.id = Utils.GetUniqueName(value, ViewModelLocator.ProxyTabViewModel.TextBlocks.Select(x => x.Name));
                RaisePropertyChanged("Name");
            }
        }

        #region Text
        public Asset Asset
        {
            get
            {
                if (_def.text.font == null)
                    return new Asset();
                return Asset.Load(Path.Combine(_def.Manager.RootPath, _def.text.font));
            }
            set
            {
                _def.text.font = value?.RelativePath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Font");
            }
        }

        public int FontSize
        {
            get
            {
                return _def.text.size;
            }
            set
            {
                if (_def.text.size == value) return;
                _def.text.size = value;
                RaisePropertyChanged("FontSize");
            }
        }

        public Color FontColor
        {
            get
            {
                var color = _def.text.color;
                return Color.FromArgb(color.A, color.R, color.G, color.B);
            }
            set
            {
                var color = System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);
                if (_def.text.color == color) return;
                _def.text.color = color;
                RaisePropertyChanged("FontColor");
                RaisePropertyChanged("FontBrush");
            }
        }

        #endregion

        #region location
        public int Left
        {
            get
            {
                return _def.location.x;
            }
            set
            {
                if (_def.location.x == value) return;
                _def.location.x = value;
                RaisePropertyChanged("Left");
            }
        }

        public int Top
        {

            get
            {
                return _def.location.y;
            }
            set
            {
                if (_def.location.y == value) return;
                _def.location.y = value;
                RaisePropertyChanged("Top");
            }
        }

        public int Rotate
        {
            get
            {
                return _def.location.rotate;
            }
            set
            {
                if (_def.location.rotate == value) return;
                _def.location.rotate = value;
                RaisePropertyChanged("Rotate");
            }
        }

        public bool AltRotate
        {
            get
            {
                return _def.location.altrotate;
            }
            set
            {
                if (_def.location.altrotate == value) return;
                _def.location.altrotate = value;
                RaisePropertyChanged("AltRotate");
            }
        }

        public bool Flip
        {
            get
            {
                return _def.location.flip;
            }
            set
            {
                if (_def.location.flip == value) return;
                _def.location.flip = value;
                RaisePropertyChanged("Flip");
            }
        }
        #endregion

        #region wordwrap
        public int Width
        {

            get
            {
                return _def.wordwrap.width;
            }
            set
            {
                if (_def.wordwrap.width == value) return;
                _def.wordwrap.width = value;
                RaisePropertyChanged("Width");
            }
        }

        public int Height
        {

            get
            {
                return _def.wordwrap.height;
            }
            set
            {
                if (_def.wordwrap.height == value) return;
                _def.wordwrap.height = value;
                RaisePropertyChanged("Height");
            }
        }

        public bool ShrinkToFit
        {
            get
            {
                return _def.wordwrap.shrinkToFit;
            }
            set
            {
                if (_def.wordwrap.shrinkToFit == value) return;
                _def.wordwrap.shrinkToFit = value;
                RaisePropertyChanged("ShrinkToFit");
            }
        }

        public string HorizontalAlignment
        {
            get
            {
                return _def.wordwrap.align;
            }
            set
            {
                if (_def.wordwrap.align == value) return;
                _def.wordwrap.align = value;
                RaisePropertyChanged("HorizontalAlignment");
            }
        }

        public string VerticalAlignment
        {
            get
            {
                return _def.wordwrap.valign;
            }
            set
            {
                if (_def.wordwrap.valign == value) return;
                _def.wordwrap.valign = value;
                RaisePropertyChanged("VerticalAlignment");
            }
        }

        #endregion

        #region border
        public int BorderThickness
        {
            get
            {
                return _def.border.size;
            }
            set
            {
                if (_def.border.size == value) return;
                _def.border.size = value;
                RaisePropertyChanged("BorderThickness");
            }
        }
        public Color BorderColor
        {
            get
            {
                var color = _def.border.color;
                return Color.FromArgb(color.A, color.R, color.G, color.B);
            }
            set
            {
                var color = System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);
                if (_def.border.color == color) return;
                _def.border.color = color;
                RaisePropertyChanged("BorderColor");
                RaisePropertyChanged("BorderBrush");
            }
        }
        #endregion

        public TextAlignment TextAlignment
        {
            get
            {
                if (_def.wordwrap.align == "center") return TextAlignment.Center;
                if (_def.wordwrap.align == "far") return TextAlignment.Right;
                return TextAlignment.Left;
            }
        }

        public TextWrapping WordWrap
        {
            get
            {
                return (_def.wordwrap.height != 0) ? TextWrapping.Wrap : TextWrapping.NoWrap;
            }
            set
            {
                RaisePropertyChanged("WordWrap");
            }
        }

        public FontFamily Font
        {
            get
            {
                if (Asset == null)
                {
                    return new FontFamily("Arial");
                }
                return new FontFamily(Asset.FullPath);
            }
        }

        public Brush FontBrush => new SolidColorBrush(FontColor);
        public Brush BorderBrush => new SolidColorBrush(BorderColor);

        public override object Clone()
        {
            return new ProxyTextItemModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ViewModelLocator.ProxyTabViewModel.TextBlocks.IndexOf(this);
            ViewModelLocator.ProxyTabViewModel.TextBlocks.Insert(index, Clone() as ProxyTextItemModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ViewModelLocator.ProxyTabViewModel.TextBlocks.IndexOf(this);
            ViewModelLocator.ProxyTabViewModel.TextBlocks.Insert(index, new ProxyTextItemModel());
        }
    }
}
