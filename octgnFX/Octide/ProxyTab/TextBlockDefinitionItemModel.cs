// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.IO;

using GalaSoft.MvvmLight;

using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Octgn.ProxyGenerator.Definitions;
using Octide.ViewModel;
using Octide.SetTab.ItemModel;
using System.ComponentModel;

namespace Octide.ItemModel
{
    public class TextBlockDefinitionItemModel : IdeBaseItem
    {
        public BlockDefinition _def;

        public AssetController Asset { get; set; }
        public TextBlockDefinitionItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new text definition
        {
            _def = new BlockDefinition
            {
                type = "text",
            };
            Asset = new AssetController(AssetType.Font);
            Asset.CanRemove = true;
            _def.text.font = Asset.SelectedAsset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
            Name = "text";
        }

        public TextBlockDefinitionItemModel(BlockDefinition b, IdeCollection<IdeBaseItem> source) : base(source) // load 
        {
            _def = b;

            var font = new Font
            {
                Size = b.text.size,
            };
            string path = b.text.font ?? null;
            Asset = new AssetController(AssetType.Font);
            Asset.CanRemove = true;
            Asset.Register(path);
            Asset.PropertyChanged += AssetUpdated;
        }


        public TextBlockDefinitionItemModel(TextBlockDefinitionItemModel t, IdeCollection<IdeBaseItem> source) : base(source) // copy
        {
            _def = new BlockDefinition();
            Asset = new AssetController(AssetType.Font);
            Asset.CanRemove = true;
            Asset.Register(t._def.src);
            Asset.PropertyChanged += AssetUpdated;
            _def.text.font = Asset.SelectedAsset.FullPath;
            Name = t.Name;
        }
        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _def.text.font = Asset.SelectedAsset?.FullPath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Font");
            }
        }

        public override object Clone()
        {
            return new TextBlockDefinitionItemModel(this, Source);
        }
        public override object Create()
        {
            return new TextBlockDefinitionItemModel(Source);
        }

        public IEnumerable<string> UniqueNames => Source.Select(x => ((TextBlockDefinitionItemModel)x).Name);

        public string Name
        {
            get { return _def.id; }
            set
            {
                if (value == _def.id) return;
                _def.id = Utils.GetUniqueName(value, UniqueNames);
                RaisePropertyChanged("Name");
            }
        }

        #region Text

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

        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                switch (_def.wordwrap.align)
                {
                    case "center":
                        return HorizontalAlignment.Center;
                    case "far":
                        return HorizontalAlignment.Right;
                    default:
                        return HorizontalAlignment.Left;
                }
            }
            set
            {
                string ret;
                switch (value)
                {
                    case HorizontalAlignment.Center:
                        ret = "center";
                        break;
                    case HorizontalAlignment.Right:
                        ret = "far";
                        break;
                    default:
                        ret = "near";
                        break;
                }
                if (_def.wordwrap.align == ret) return;
                _def.wordwrap.align = ret;
                RaisePropertyChanged("HorizontalAlignment");
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get
            {
                switch (_def.wordwrap.valign)
                {
                    case "center":
                        return VerticalAlignment.Center;
                    case "far":
                        return VerticalAlignment.Bottom;
                    default:
                        return VerticalAlignment.Top;
                }
            }
            set
            {
                string ret;
                switch (value)
                {
                    case VerticalAlignment.Center:
                        ret = "center";
                        break;
                    case VerticalAlignment.Bottom:
                        ret = "far";
                        break;
                    default:
                        ret = "near";
                        break;
                }
                if (_def.wordwrap.valign == ret) return;
                _def.wordwrap.valign = ret;
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
                if (Asset?.SafePath == null)
                {
                    return new FontFamily("Arial");
                }
                return new FontFamily(Asset.SafePath);
            }
        }

        public Brush FontBrush => new SolidColorBrush(FontColor);
        public Brush BorderBrush => new SolidColorBrush(BorderColor);

    }

    public class ProxyTextBlockItemModel : ViewModelBase
    {
        public TextBlockDefinitionItemModel LinkedTextBlock { get; private set; }
        public string Format { get; private set; }
        public CardPropertyModel Property { get; set; }

        public ObservableCollection<ProxyInputPropertyItemModel> EditableProperties;

        public LinkDefinition _linkDefinition;

        public ProxyTextBlockItemModel()
        {
        }

        public ProxyTextBlockItemModel(LinkDefinition ld)
        {
            _linkDefinition = ld;
            LinkedTextBlock = (TextBlockDefinitionItemModel)ViewModelLocator.ProxyTabViewModel.TextBlocks.First(x => ((TextBlockDefinitionItemModel)x).Name == ld.Block);
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
}