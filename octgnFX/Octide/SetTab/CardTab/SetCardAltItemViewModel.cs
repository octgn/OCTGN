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
using Octide.Messages;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Octide.ItemModel
{
    public class SetCardAltItemViewModel : IdeListBoxItemBase, IDropTarget
    {
        public new SetCardItemViewModel Parent { get; set; }

        public CardPropertySet _altDef;
        public SizeItemViewModel _cardSize;

        public CardNamePropertyItemModel NameProperty { get; set; }
        public CardSizePropertyItemModel SizeProperty { get; set; }
        public ObservableCollection<CardPropertyItemModel> Items { get; private set; }

        public ObservableCollection<IdeListBoxItemBase> CardSizes => ViewModelLocator.PreviewTabViewModel.CardSizes;

        public RelayCommand DeleteImageCommand { get; private set; }

        public SetCardAltItemViewModel() //for adding new items
        {
            DeleteImageCommand = new RelayCommand(DeleteImage);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));

            _altDef = new CardPropertySet
            {
                Name = "Card",
                Properties = new Dictionary<PropertyDef, object>(),
            };
            NameProperty = new CardNamePropertyItemModel() { Parent = this };
            SizeProperty = new CardSizePropertyItemModel() { Parent = this, Value = ViewModelLocator.PreviewTabViewModel.DefaultSize };
            Items = new ObservableCollection<CardPropertyItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                _altDef.Properties = Items.ToDictionary(x => x.Property._property, y => y.Value);
            };
        }

        public SetCardAltItemViewModel(CardPropertySet altData) //for loading an existing collection
        {
            DeleteImageCommand = new RelayCommand(DeleteImage);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            _altDef = altData;
            NameProperty = new CardNamePropertyItemModel() { Parent = this };
            SizeProperty = new CardSizePropertyItemModel()
            {
                Parent = this,
                Value = ViewModelLocator.PreviewTabViewModel.CardSizes.Cast<SizeItemViewModel>().FirstOrDefault(x => altData.Size == x._size) ?? ViewModelLocator.PreviewTabViewModel.DefaultSize
            };

            Items = new ObservableCollection<CardPropertyItemModel>();
            foreach (var prop in altData.Properties)
            {
                Items.Add(new CardPropertyItemModel
                {
                    Property = ViewModelLocator.PropertyTabViewModel.Items.First(y => (y as PropertyItemViewModel)._property.Equals(prop.Key)) as PropertyItemViewModel,
                    _value = prop.Value,
                    Parent = this,
                    _isUndefined = prop.Value == null
                });
            }
            Items.CollectionChanged += (a, b) =>
            {
                _altDef.Properties = Items.ToDictionary(x => x.Property._property, y => y.Value);
            };
        }

        public SetCardAltItemViewModel(SetCardAltItemViewModel a) //for copying the item
        {
            DeleteImageCommand = new RelayCommand(DeleteImage);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Parent = a.Parent;
            ItemSource = a.ItemSource;
            _altDef = a._altDef.Clone() as CardPropertySet;
            _altDef.Type = Utils.GetUniqueName(a.Name, a.ItemSource.Select(x => (x as SetCardAltItemViewModel).Name));
            NameProperty = new CardNamePropertyItemModel() { Parent = this };
            SizeProperty = new CardSizePropertyItemModel() { Parent = this, Value = a.SizeProperty.Value };
            Items = new ObservableCollection<CardPropertyItemModel>();

            foreach (var prop in _altDef.Properties)
            {
                Items.Add(new CardPropertyItemModel
                {
                    Property = ViewModelLocator.PropertyTabViewModel.Items.First(y => (y as PropertyItemViewModel)._property.Equals(prop.Key)) as PropertyItemViewModel,
                    _value = prop.Value,
                    Parent = this
                });
            }

            Items.CollectionChanged += (c, b) =>
            {
                _altDef.Properties = Items.ToDictionary(x => x.Property._property, y => y.Value);
            };

        }

        public void CustomPropertyChanged(CustomPropertyChangedMessage message)
        {
            switch (message.Action)
            {
                case PropertyChangedMessageAction.Remove:
                    _altDef.Properties.Remove(message.Prop._property);
                    break;
            }
            if (Parent.SelectedItem == this)
              RaisePropertyChanged("GetProperties");
        }

        public void UpdateAltCardSize()
        {
            if (!CardSizes.Contains(SizeProperty.Value) || SizeProperty.Value.IsDefault)
            {
                SizeProperty.Value = ViewModelLocator.PreviewTabViewModel.DefaultSize;
            }
        }

        public List<CardPropertyItemModel> GetProperties
        {
            get
            {
                return ViewModelLocator.PropertyTabViewModel.Items.Cast<PropertyItemViewModel>()
                    .Select(x => Items.FirstOrDefault(y => y.Property == x) ?? new CardPropertyItemModel()
                    {
                        Property = x,
                        Parent = this,
                        _isUndefined = true
                    })
                    .ToList();
            }
        }

        public string Name  // this is the altID, NOT the alt's actual cardname
        {
            get
            {
                return _altDef.Type;
            }
            set
            {
                if (_altDef.Type == value) return;
                _altDef.Type = Utils.GetUniqueName(value, Parent.Items.Select(x => (x as SetCardAltItemViewModel).Name));
                Parent.UpdateCardAlts();
                RaisePropertyChanged("Name");
            }
        }

        public override object Clone()
        {
            return new SetCardAltItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = Parent.Items.IndexOf(this);
            ItemSource.Insert(index, Clone() as SetCardAltItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = Parent.Items.IndexOf(this);
            ItemSource.Insert(index, new SetCardAltItemViewModel() { ItemSource = ItemSource, Parent = Parent, Name = "Alt" });
        }

        public Card GetTempCard()
        {
            var ret = Parent._card.Clone();
            ret.Alternate = _altDef.Type;
            return ret;
        }

        #region image section

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

        private ObservableCollection<ProxyOverlayItemModel> _activeOverlayLayers;
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

        public ObservableCollection<ProxyOverlayItemModel> ActiveOverlayLayers
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
            ProxyTemplateItemModel activeTemplate = ViewModelLocator.ProxyTabViewModel.Templates.First(x => x._def.defaultTemplate == true);

            foreach (var template in ViewModelLocator.ProxyTabViewModel.Templates)
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

            ActiveOverlayLayers = new ObservableCollection<ProxyOverlayItemModel>(
                activeTemplate._def
                .GetOverLayBlocks(properties)
                .Select(x => ViewModelLocator.ProxyTabViewModel.OverlayBlocks.First(y => y.Name == x.Block))
                );

            var allCardProperties = new List<CardPropertyItemModel>(GetProperties);
            allCardProperties.Add(NameProperty);
            allCardProperties.Add(SizeProperty);

            foreach (var x in allCardProperties)
            {
                var y = x.Value;
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
            RaisePropertyChanged("");
        }
        #endregion
    }
}
