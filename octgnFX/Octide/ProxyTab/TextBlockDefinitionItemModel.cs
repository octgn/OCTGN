// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Command;

using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Octide.ItemModel;
using Microsoft.Win32;
using GongSolutions.Wpf.DragDrop;
using System.Windows.Data;
using Octgn.ProxyGenerator.Definitions;
using Octide.ViewModel;
using Octide.SetTab.CardItemModel;

namespace Octide.ItemModel
{
    public class ProxyTextDefinitionItemModel : IdeListBoxItemBase
    {
        public BlockDefinition _def;
        public new ObservableCollection<ProxyTextDefinitionItemModel> ItemSource { get; set; }

        public ProxyTextDefinitionItemModel() // new text definition
        {
            _def = new BlockDefinition
            {
                type = "text",
                id = Utils.GetUniqueName("text", ViewModelLocator.ProxyTabViewModel.TextBlocks.Select(x => x.Name))
            };
        }

        public ProxyTextDefinitionItemModel(BlockDefinition b) // copy from 
        {
            _def = b;

            var font = new Font
            {
                Size = b.text.size,
                Src = System.IO.Path.Combine(b.Manager.RootPath, b.text.font ?? "")
            };
        }


        public ProxyTextDefinitionItemModel(ProxyTextDefinitionItemModel t)
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
            return new ProxyTextDefinitionItemModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ViewModelLocator.ProxyTabViewModel.TextBlocks.IndexOf(this);
            ViewModelLocator.ProxyTabViewModel.TextBlocks.Insert(index, Clone() as ProxyTextDefinitionItemModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ViewModelLocator.ProxyTabViewModel.TextBlocks.IndexOf(this);
            ViewModelLocator.ProxyTabViewModel.TextBlocks.Insert(index, new ProxyTextDefinitionItemModel());
        }
    }

    public class ProxyTextBlockItemModel : ViewModelBase
    {
        public ProxyTextDefinitionItemModel LinkedTextBlock { get; private set; }
        public string Format { get; private set; }
        public PropertyModel Property { get; set; }

        public ObservableCollection<ProxyInputPropertyItemModel> EditableProperties;

        public LinkDefinition _linkDefinition;

        public ProxyTextBlockItemModel()
        {
        }

        public ProxyTextBlockItemModel(LinkDefinition ld)
        {
            _linkDefinition = ld;
            LinkedTextBlock = ViewModelLocator.ProxyTabViewModel.TextBlocks.First(x => x.Name == ld.Block);
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