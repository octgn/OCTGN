// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GongSolutions.Wpf.DragDrop;
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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Octide.SetTab.ItemModel
{
    public class AlternateModel : IdeBaseItem, IDropTarget
    {

        public CardPropertySet _altDef;

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
            Type = "";

            DeleteImageCommand = new RelayCommand(DeleteImage);
            //     Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, CardSizeChanged);
        }

        public AlternateModel(CardPropertySet altData, IdeCollection<IdeBaseItem> source) : base(source) //for loading an existing collection
        {
            _altDef = altData;

            //    foreach (var prop in altData.Properties)
            //    {
            //        Items.Add(new CardPropertyModel(this, ViewModelLocator.PropertyTabViewModel.Items.First(x => (x as PropertyItemModel)._property.Equals(prop.Key)) as PropertyItemModel));
            //    }

            DeleteImageCommand = new RelayCommand(DeleteImage);
            //   Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, CardSizeChanged);
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

            DeleteImageCommand = new RelayCommand(DeleteImage);
            //  Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, CardSizeChanged);
        }

        public void DragEnter(IDropInfo dropInfo) { }
        public void DragLeave(IDropInfo dropInfo) { }

        public void CustomPropertyChanged(CustomPropertyChangedMessage message)
        {
            switch (message.Action)
            {
                case PropertyChangedMessageAction.Remove:
                    _altDef.Properties.Remove(message.Prop.Property);
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
            //    _altDef.Properties = CachedProperties.ToDictionary(x => x.LinkedProperty._property, y => y.CachedValue);
        }

        public override void Cleanup()
        {
            base.Cleanup();

            Messenger.Default.Unregister<CardSizeChangedMesssage>(this, CardSizeChanged);
        }

        public List<CardPropertyModel> GetProperties
        {
            get
            {
                return ViewModelLocator.PropertyTabViewModel.Items.Select(x => new CardPropertyModel(this, (PropertyItemModel)x)).ToList();
            }
        }

        public new bool CanRemove => !IsDefault;
        public new bool CanDragDrop => !IsDefault;

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
                RaisePropertyChanged("CardImage");
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

        public string ImagePath
        {
            get
            {
                var imageFiles = GetImages();
                if (imageFiles?.Length > 0)
                {
                    return imageFiles.First();
                }
                else
                {
                    return SizeProperty.BackAsset.SafePath;
                }
            }
        }

        public BitmapImage CardImage
        {
            get
            {
                var ret = new BitmapImage();
                ret.BeginInit();
                ret.CacheOption = BitmapCacheOption.OnLoad;
                ret.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                ret.UriSource = new Uri(ImagePath, UriKind.RelativeOrAbsolute);
                ret.EndInit();
                return ret;
            }
        }

        public CardModel Card => Source.Parent as CardModel;
        public SetModel Set => Card.Source.Parent as SetModel;

        public string ImageDirectory => Path.Combine(
                        Config.Instance.ImageDirectoryFull,
                        ViewModelLocator.GameLoader.Game.Id.ToString(),
                        "Sets",
                        Set.Id.ToString(),
                        "Cards"
                        );

        public string ProxyDirectory => Path.Combine(ImageDirectory, "Proxies");

        public string ImageFileName
        {
            get
            {
                var ret = Card.Id.ToString();
                if (Type != "")
                {
                    ret += "." + Type;
                }
                return ret;
            }
        }


        public string[] GetImages()
        {
            if (!Directory.Exists(ImageDirectory)) return null;

            return Directory.GetFiles(ImageDirectory, ImageFileName + ".*")
                            .Where(x => Path.GetFileNameWithoutExtension(x)
                            .Equals(ImageFileName, StringComparison.InvariantCultureIgnoreCase))
                            .OrderBy(x => x.Length).ToArray();
        }


        public void DeleteImage()
        {
            var images = GetImages();
            if (images?.Length > 0)
            {
                var garbage = Config.Instance.Paths.GraveyardPath;
                if (!Directory.Exists(garbage))
                    Directory.CreateDirectory(garbage);
                // Delete all the old picture files
                foreach (var image in images.Select(x => new FileInfo(x)))
                {
                    image.MoveTo(Path.Combine(garbage, image.Name));
                }
                RaisePropertyChanged("CardImage");
            }
        }

        public void SaveImage(string file)
        {
            DeleteImage();
            Directory.CreateDirectory(ImageDirectory);
            var newPath = Path.Combine(ImageDirectory, ImageFileName + Path.GetExtension(file));
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
            var properties = GetProperties.ToDictionary(x => x.LinkedProperty, y => y.Value ?? "");
            properties.Add(PropertyTabViewModel.SetProperty, Set.Name);
            properties.Add(PropertyTabViewModel.NameProperty, Name);
            properties.Add(PropertyTabViewModel.SizeNameProperty, SizeProperty.Name);
            properties.Add(PropertyTabViewModel.SizeHeightProperty, SizeProperty.Height.ToString());
            properties.Add(PropertyTabViewModel.SizeWidthProperty, SizeProperty.Width.ToString());

            var proxymapping = properties.ToDictionary(x => x.Key.Name, y => y.Value);

            TemplateModel activeTemplate = (TemplateModel)ViewModelLocator.ProxyTabViewModel.Templates.DefaultItem;
            foreach (TemplateModel template in ViewModelLocator.ProxyTabViewModel.Templates)
            {
                if (template._def.Matches.All(x => proxymapping.ContainsKey(x.Name) && proxymapping[x.Name] == x.Value))
                {
                    activeTemplate = template;
                    break;
                }
            }
            BaseImage = activeTemplate.Asset.SafePath;
            BitmapImage image = new BitmapImage(new Uri(BaseImage));
            BaseWidth = image.PixelWidth;
            BaseHeight = image.PixelHeight;

            ActiveOverlayLayers = new ObservableCollection<OverlayBlockDefinitionItemModel>(
                activeTemplate._def
                .GetOverLayBlocks(proxymapping).Where(x => x.SpecialBlock == null)
                .Select(x => (OverlayBlockDefinitionItemModel)ViewModelLocator.ProxyTabViewModel.OverlayBlocks.First(y => ((OverlayBlockDefinitionItemModel)y).Name == x.Block))
                );

            ActiveTextLayers = new ObservableCollection<ProxyTextBlockItemModel>(
                activeTemplate._def
                .GetTextBlocks(proxymapping)
                .Select(x => new ProxyTextBlockItemModel(x)
                {
                    Card = this,
                    LinkedProperty = properties.First(y => y.Key.Name == x.NestedProperties.First().Name).Key
                }
                ));
        }
        #endregion
    }
}
