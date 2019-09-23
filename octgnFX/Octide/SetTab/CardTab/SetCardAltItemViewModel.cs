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
        public CardPropertySet _altDef;
        public SizeItemViewModel _cardSize;
        public ObservableCollection<CardPropertyItemModel> Items { get; private set; }

        public ObservableCollection<IdeListBoxItemBase> CardSizes => ViewModelLocator.PreviewTabViewModel.CardSizes;

        public RelayCommand DeleteImageCommand { get; private set; }

        public SetCardAltItemViewModel() //for adding new items
        {
            DeleteImageCommand = new RelayCommand(DeleteImage);

            _altDef = new CardPropertySet
            {
                Properties = new Dictionary<PropertyDef, object>(),
            };
            CardSize = ViewModelLocator.PreviewTabViewModel.DefaultSize;
            Items = new ObservableCollection<CardPropertyItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                UpdateAltPropertySet();
            };
            Items.Add(new CardPropertyItemModel(ViewModelLocator.PropertyTabViewModel.NameProperty) { Parent = this }); //adds the card name property
            //  UpdateProxyTemplate();
        }

        public SetCardAltItemViewModel(CardPropertySet altData) //for loading an existing collection
        {
            DeleteImageCommand = new RelayCommand(DeleteImage);
            _altDef = altData;
            CardSize = (SizeItemViewModel)ViewModelLocator.PreviewTabViewModel.CardSizes.FirstOrDefault(x => altData.Size == (x as SizeItemViewModel)._size) ?? ViewModelLocator.PreviewTabViewModel.DefaultSize;
            Items = new ObservableCollection<CardPropertyItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                UpdateAltPropertySet();
            };
            foreach (var alt in altData.Properties)
            {
                var LinkedProperty = ViewModelLocator.PropertyTabViewModel.Items.First(x => (x as PropertyItemViewModel).Name == alt.Key.Name) as PropertyItemViewModel;
                Items.Add(new CardPropertyItemModel(LinkedProperty, alt, this));
            }
          //  UpdateProxyTemplate();
        }

        public SetCardAltItemViewModel(SetCardAltItemViewModel a) //for copying the item
        {
            DeleteImageCommand = new RelayCommand(DeleteImage);
            Parent = a.Parent;
            ItemSource = a.ItemSource;
            _altDef = a._altDef.Clone() as CardPropertySet;
            _altDef.Type = Utils.GetUniqueName(a.Name, a.ItemSource.Select(x => (x as SetCardAltItemViewModel).Name));
            CardSize = a.CardSize;
            Items = new ObservableCollection<CardPropertyItemModel>();
            Items.CollectionChanged += (x, b) =>
            {
                UpdateAltPropertySet();
            };
            foreach (var alt in _altDef.Properties)
            {
                var LinkedProperty = ViewModelLocator.PropertyTabViewModel.Items.First(x => (x as PropertyItemViewModel).Name == alt.Key.Name) as PropertyItemViewModel;
                Items.Add(new CardPropertyItemModel(LinkedProperty, alt, this));
            }
           // UpdateProxyTemplate();
        }

        public void UpdateAltPropertySet()
        {
            var NewAltPropSet = new Dictionary<PropertyDef, object>();
            foreach (PropertyItemViewModel customProp in ViewModelLocator.PropertyTabViewModel.Items)
            {
                PropertyDef clonedProp = customProp._property.Clone() as PropertyDef;
                var storedProp = Items.FirstOrDefault(x => x.Property == customProp);
                if (storedProp == null)
                {
                  //TODO  clonedProp.IsUndefined = true;
                    NewAltPropSet.Add(clonedProp, "");
                }
                else
                {
                    if (storedProp.IsDefined)
                    {
                   //TODO     clonedProp.IsUndefined = false;
                        NewAltPropSet.Add(clonedProp, storedProp.Value);
                    }
                    else
                    {
                    //TODO    clonedProp.IsUndefined = true;
                        NewAltPropSet.Add(clonedProp, "");
                    }
                }
            }
            _altDef.Properties = NewAltPropSet;
        }

        public void UpdateAltCardSize()
        {
            if (!CardSizes.Contains(CardSize) || CardSize.IsDefault)
            {
                CardSize = ViewModelLocator.PreviewTabViewModel.DefaultSize;
            }
        }

        public List<CardPropertyItemModel> GetProperties
        {
            get
            {
                var ret = new List<CardPropertyItemModel>();
                foreach (PropertyItemViewModel gameProp in ViewModelLocator.PropertyTabViewModel.Items)
                {
                    var storedProp = Items.FirstOrDefault(x => x.Property == gameProp);
                    if (storedProp == null)
                    {
                        storedProp = new CardPropertyItemModel(gameProp) { Parent = this };
                        Items.Add(storedProp);
                    }
                    ret.Add(storedProp);
                }
                return ret;
            }
        }

        public SizeItemViewModel CardSize
        {
            get
            {
                return _cardSize;
            }
            set
            {
                if (_cardSize == value) return;
                _cardSize = value ?? ViewModelLocator.PreviewTabViewModel.DefaultSize;
                _altDef.Size = _cardSize?._size;
                RaisePropertyChanged("CardSize");
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
                _altDef.Type = Utils.GetUniqueName(value, ((SetCardItemViewModel)Parent).Items.Select(x => (x as SetCardAltItemViewModel).Name));
                ((SetCardItemViewModel)Parent).UpdateCardAlts();
                RaisePropertyChanged("Name");
            }
        }

        public PropertyDef CardNameProperty => ViewModelLocator.PropertyTabViewModel.NameProperty._property;

        public object CardName  // this is the alt's actual card name
        {
            get
            {
                return _altDef.Properties[CardNameProperty];
            }
            set
            {
                if (_altDef.Properties[CardNameProperty] == value) return;
                _altDef.Properties[CardNameProperty] = value;
                if (IsDefault)
                    (Parent as SetCardItemViewModel).UpdateCardName();
                RaisePropertyChanged("CardName");
            }
        }

        public override object Clone()
        {
            return new SetCardAltItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = (Parent as SetCardItemViewModel).Items.IndexOf(this);
            ItemSource.Insert(index, Clone() as SetCardAltItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = (Parent as SetCardItemViewModel).Items.IndexOf(this);
            ItemSource.Insert(index, new SetCardAltItemViewModel() { ItemSource = ItemSource, Parent = Parent, Name = "Alt" });
        }

        public Card GetTempCard()
        {
            var ret = (Parent as SetCardItemViewModel)._card.Clone();
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

            ActiveOverlayLayers = new ObservableCollection<ProxyOverlayItemModel>(activeTemplate._def.GetOverLayBlocks(properties).Select(x => ViewModelLocator.ProxyTabViewModel.OverlayBlocks.First(y => y.Name == x.Block)));
            ActiveTextLayers = new ObservableCollection<ProxyTextBlockItemModel>(activeTemplate._def.GetTextBlocks(properties).Select(x => new ProxyTextBlockItemModel(x) { Property = GetProperties.First(y => y.Name == x.NestedProperties.First().Name) }));
            RaisePropertyChanged("");
        }
        #endregion
    }
}
