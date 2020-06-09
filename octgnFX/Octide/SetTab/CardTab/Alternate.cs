// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GongSolutions.Wpf.DragDrop;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;
using Octgn.Library;
using Octide.ItemModel;
using Octide.Messages;
using Octide.ProxyTab.ItemModel;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Octide.SetTab.ItemModel
{
    public class AlternateModel : IdeBaseItem, IDropTarget
    {

        public CardPropertySet _altDef;
        public ObservableCollection<CardPropertyModel> Items { get; private set; }

        public IdeCollection<IdeBaseItem> CardSizes => ViewModelLocator.PreviewTabViewModel.CardSizes;

        public RelayCommand DeleteImageCommand { get; private set; }

        public AlternateModel(IdeCollection<IdeBaseItem> source) : base(source) //for adding new items
        {

            _altDef = new CardPropertySet
            {
                Properties = new Dictionary<PropertyDef, object>(),
                Size = ((SizeItemModel)CardSizes.DefaultItem)._size,
                Name = "Card",
            };
            Type = (Source.Count == 0) ? "" : "alt";
            Items = new ObservableCollection<CardPropertyModel>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildAltDef(b);
            };
            DeleteImageCommand = new RelayCommand(DeleteImage);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, action => CardSizeChanged(action));
        }

        public AlternateModel(CardPropertySet altData, IdeCollection<IdeBaseItem> source) : base(source) //for loading an existing collection
        {
            _altDef = altData;

            Items = new ObservableCollection<CardPropertyModel>();
            foreach (var prop in altData.Properties)
            {
                Items.Add(new CardPropertyModel
                {
                    Property = ViewModelLocator.PropertyTabViewModel.Items.First(y => (y as PropertyItemModel)._property.Equals(prop.Key)) as PropertyItemModel,
                    _cachedValue = prop.Value,
                    Parent = this,
                    _isDefined = true
                });
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildAltDef(b);
            };
            DeleteImageCommand = new RelayCommand(DeleteImage);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, action => CardSizeChanged(action));
        }

        public AlternateModel(AlternateModel a, IdeCollection<IdeBaseItem> source) : base(source) //for copying the item
        {
            _altDef = new CardPropertySet()
            {
                Size = a._altDef.Size,
                Properties = a._altDef.Properties.ToDictionary(x => x.Key, x => x.Value),
                Name = a.Name

            };
            Type = a.Type;
            Items = new IdeCollection<CardPropertyModel>(this);
            Items.CollectionChanged += (c, b) =>
            {
                BuildAltDef(b);
            };
            foreach (var prop in _altDef.Properties)
            {
                Items.Add(new CardPropertyModel
                {
                    Property = ViewModelLocator.PropertyTabViewModel.Items.First(y => (y as PropertyItemModel)._property.Equals(prop.Key)) as PropertyItemModel,
                    _cachedValue = prop.Value,
                    _isDefined = true,
                    Parent = this
                });
            }

            DeleteImageCommand = new RelayCommand(DeleteImage);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, action => CardSizeChanged(action));
        }

        public void CustomPropertyChanged(CustomPropertyChangedMessage message)
        {
            switch (message.Action)
            {
                case PropertyChangedMessageAction.Remove:
                    _altDef.Properties.Remove(message.Prop._property);
                    break;
            }
            if (Source.SelectedItem == this)
              RaisePropertyChanged("GetProperties");
        }
        public void CardSizeChanged(CardSizeChangedMesssage message)
        {
            if (!ViewModelLocator.PreviewTabViewModel.CardSizes.Contains(SizeProperty) || SizeProperty.IsDefault)
            {
                SizeProperty = (SizeItemModel)CardSizes.DefaultItem;
            }
        }
        public void BuildAltDef(NotifyCollectionChangedEventArgs args)
        {
            _altDef.Properties = Items.ToDictionary(x => x.Property._property, y => y._cachedValue);
        }

        public List<CardPropertyModel> GetProperties
        {
            get
            {
                return ViewModelLocator.PropertyTabViewModel.Items
                    .Select(x => Items.FirstOrDefault(y => y.Property == x) ?? new CardPropertyModel()
                    {
                        Property = (PropertyItemModel)x,
                        Parent = this,
                        _isDefined = false
                    })
                    .ToList();
            }
        }

        public IEnumerable<string> UniqueNames => Source.Select(x => ((AlternateModel)x).Type);

        public string Type
        {
            get
            {
                return _altDef.Type;
            }
            set
            {
                if (_altDef.Type == value) return;
                _altDef.Type = Utils.GetUniqueName(value, UniqueNames);
                ((CardModel)Source.Parent).BuildCardDef(null);
                RaisePropertyChanged("Type");
            }
        }

        public string Name
        {
            get
            {
                return _altDef.Name;
            }
            set
            {
                if (_altDef.Name == value) return;
                _altDef.Name = value;
                ((CardModel)Source.Parent).UpdateCardName();
                UpdateProxyTemplate();
                RaisePropertyChanged("Name");
            }
        }
        public SizeItemModel SizeProperty
        {
            get
            {
                return (SizeItemModel)CardSizes.FirstOrDefault(x => ((SizeItemModel)x)._size == _altDef.Size);
            }
            set
            {
                if (_altDef.Size == value._size) return;
                _altDef.Size = value._size ?? ((SizeItemModel)CardSizes.DefaultItem)._size;
                UpdateProxyTemplate();
                RaisePropertyChanged("SizeProperty");
            }
        }

        public override object Clone()
        {
            return new AlternateModel(this, Source);
        }
        public override object Create()
        {
            return new AlternateModel(Source);
        }

        #region image section

        public Card GetTempCard()
        {
            var ret = ((CardModel)Source.Parent)._card.Clone();
            ret.Alternate = _altDef.Type;
            return ret;
        }

        public BitmapImage CardImage
        {
            get
            {
                return GetImage();
            }
        }
        
        public BitmapImage GetImage()
        {
            var defaultCardBack = "pack://application:,,,/Resources/Back.png";
            var tempCard = GetTempCard();
            var files = Directory.GetFiles(tempCard.GetSet().ImagePackUri, tempCard.GetImageUri() + ".*")
                            .Where(x => Path.GetFileNameWithoutExtension(x)
                            .Equals(tempCard.GetImageUri(), StringComparison.InvariantCultureIgnoreCase))
                            .OrderBy(x => x.Length)
                            .ToArray();
            if (files.Length > 0) defaultCardBack = tempCard.GetPicture();
            Stream imageStream = null;
            if (defaultCardBack.StartsWith("pack"))
            {
                var sri = Application.GetResourceStream(new Uri(defaultCardBack));
                imageStream = sri.Stream;
            }
            else
            {
                imageStream = File.OpenRead(defaultCardBack);
            }
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.StreamSource = imageStream;
            image.EndInit();
            imageStream.Close();
            return image;
        }

        public void DeleteImage()
        {
            var garbage = Config.Instance.Paths.GraveyardPath;
            if (!Directory.Exists(garbage))
                Directory.CreateDirectory(garbage);
            var tempCard = GetTempCard();

            var files =
                Directory.GetFiles(tempCard.GetSet().ImagePackUri, tempCard.GetImageUri() + ".*")
                    .Where(x => Path.GetFileNameWithoutExtension(x)
                    .Equals(tempCard.GetImageUri(), StringComparison.InvariantCultureIgnoreCase))
                    .OrderBy(x => x.Length)
                    .ToArray();
            if (files.Length == 0) return;

            // Delete all the old picture files
            foreach (var f in files.Select(x => new FileInfo(x)))
            {
                f.MoveTo(Path.Combine(garbage, f.Name));
            }
            RaisePropertyChanged("CardImage");
        }

        public void SaveImage(string file)
        {
            var tempCard = GetTempCard();
            DeleteImage();
            var newPath = Path.Combine(tempCard.GetSet().ImagePackUri, tempCard.GetImageUri() + Path.GetExtension(file));
            File.Copy(file, newPath);
            RaisePropertyChanged("CardImage");
        }

        public void DragOver(IDropInfo dropInfo)
        {
            try
            {
                var dropData = dropInfo.Data as IDataObject;

                if (!dropData.GetDataPresent(DataFormats.FileDrop)) return;
                var dropFiles = (string[])dropData.GetData(DataFormats.FileDrop);
                var file = dropFiles.First();

                // Check to see if it's an image
                var testImage = Image.FromFile(file);

                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
            catch
            {
                dropInfo.Effects = DragDropEffects.None;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var dropData = dropInfo.Data as IDataObject;
            var dropFiles = (string[])dropData.GetData(DataFormats.FileDrop);
            var file = dropFiles.First();
            SaveImage(file);
        }

        #endregion

        #region proxy

        private ObservableCollection<OverlayBlockDefinitionItemModel> _activeOverlayLayers;
        private ObservableCollection<ProxyTextBlockItemModel> _activeTextLayers;
        public int _baseWidth;
        public int _baseHeight;

        public string BaseImage { get; private set; }
        
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

        public ObservableCollection<OverlayBlockDefinitionItemModel> ActiveOverlayLayers
        {
            get
            {
                if (_activeOverlayLayers == null)
                    UpdateProxyTemplate();
                return _activeOverlayLayers;
            }
            set
            {
                if (_activeOverlayLayers == value) return;
                _activeOverlayLayers = value;
                RaisePropertyChanged("ActiveOverlayLayers");
            }
        }

        public ObservableCollection<ProxyTextBlockItemModel> ActiveTextLayers
        {
            get
            {
                if (_activeTextLayers == null)
                    UpdateProxyTemplate();
                return _activeTextLayers;
            }
            set
            {
                if (_activeTextLayers == value) return;
                _activeTextLayers = value;
                RaisePropertyChanged("ActiveTextLayers");
            }
        }

        public void UpdateProxyTemplate()
        {
            var tempCard = GetTempCard();
            var properties = tempCard.GetProxyMappings();
            TemplateModel activeTemplate = (TemplateModel)ViewModelLocator.ProxyTabViewModel.Templates.DefaultItem;
            foreach (TemplateModel template in ViewModelLocator.ProxyTabViewModel.Templates)
            {
                bool isValidTemplate = true;
                foreach (var match in template._def.Matches)
                {
                    if (!properties.ContainsKey(match.Name) || properties[match.Name] != match.Value)
                    {
                        isValidTemplate = false;
                        break;
                    }
                }
                if (isValidTemplate == true)
                {
                    activeTemplate = template;
                    break;
                }
            }
            BaseImage = Path.Combine(activeTemplate._def.rootPath, activeTemplate._def.src);
            BitmapImage image = new BitmapImage(new Uri(BaseImage));
            BaseWidth = image.PixelWidth;
            BaseHeight = image.PixelHeight;

            ActiveOverlayLayers = new ObservableCollection<OverlayBlockDefinitionItemModel>(
                activeTemplate._def
                .GetOverLayBlocks(properties).Where(x => x.SpecialBlock == null)
                .Select(x => (OverlayBlockDefinitionItemModel)ViewModelLocator.ProxyTabViewModel.OverlayBlocks.First(y => ((OverlayBlockDefinitionItemModel)y).Name == x.Block))
                );

            var allCardProperties = new List<CardPropertyModel>(GetProperties);
            allCardProperties.Add(new CardPropertyModel() { Property = ViewModelLocator.PropertyTabViewModel.NameProperty, _cachedValue = Name });
            allCardProperties.Add(new CardPropertyModel() { Property = ViewModelLocator.PropertyTabViewModel.SizeProperty, _cachedValue = SizeProperty.Name });

            foreach (var x in allCardProperties)
            {
                var y = x._cachedValue;
                var z = x.Name;
            }

            ActiveTextLayers = new ObservableCollection<ProxyTextBlockItemModel>(
                activeTemplate._def
                .GetTextBlocks(properties)
                .Select(x => new ProxyTextBlockItemModel(x)
                {
                    Property = allCardProperties.First(y => y.Name == x.NestedProperties.First().Name)
                }
                ));
        }
        #endregion
    }
}
