using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octgn.DataNew.Entities;
using Octgn.Core.DataExtensionMethods;
using Octide.Messages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using Octgn.Library;
using GongSolutions.Wpf.DragDrop;

namespace Octide.ViewModel
{
    public class CardItemModel : ViewModelBase, ICloneable
    {
        private ObservableCollection<AltItemModel> _altItems;

        public Card CardDef { get; private set; }

        public SetItemModel ParentSet { get; set; }
        public RelayCommand RemoveCommand { get; private set; }

        public AltItemModel Default => AltItems.First(x => x.Alt == "");
        
        public CardItemModel() //for adding new items
        {
            var guid = Guid.NewGuid();  //every card has a new GUID
            CardDef = new Card(
                guid,
                ViewModelLocator.SetTabViewModel.SelectedSet.Id,
                "Card",
                guid.ToString(),
                "",
                ViewModelLocator.PreviewTabViewModel.DefaultSize.SizeDef,
                new Dictionary<string, CardPropertySet>());

            AltItems = new ObservableCollection<AltItemModel>();
            AltItems.CollectionChanged += (a, b) =>
            {
                UpdateCardAlts();
            };

            var alt = new AltItemModel();
            alt.ParentCard = this;
            alt.AltDef.Type = "";
            AltItems.Add(alt);

            RemoveCommand = new RelayCommand(Remove);

        }

        public CardItemModel(Card c) //for loading an existing collection
        {
            CardDef = c;
            AltItems = new ObservableCollection<AltItemModel>(CardDef.Properties.Select(x => new AltItemModel(x.Value) { ParentCard = this }));
            AltItems.CollectionChanged += (a, b) =>
            {
                UpdateCardAlts();
            };
            RemoveCommand = new RelayCommand(Remove);
        }

        public CardItemModel(CardItemModel c) //for copying the item
        {
            CardDef = new Card(c.CardDef);
            CardDef.Id = Guid.NewGuid();
            CardDef.ImageUri = CardDef.Id.ToString();
            AltItems = new ObservableCollection<AltItemModel>(CardDef.Properties.Select(x => new AltItemModel(x.Value) { ParentCard = this }));
            AltItems.CollectionChanged += (a, b) =>
            {
                UpdateCardAlts();
            };
            RemoveCommand = new RelayCommand(Remove);
        }

        public ObservableCollection<AltItemModel> AltItems
        {
            get { return _altItems; }
            set { Set(ref _altItems, value); }
        }

        public void UpdateDefaultName()
        {
            CardDef.Name = Default.Name;
            RaisePropertyChanged("Default");
            RaisePropertyChanged("Name");
        }

        public void UpdateCardAlts()
        {
            CardDef.Properties = AltItems.Select(x => x.AltDef).ToDictionary(x => x.Type, x => x);
        }
                
        public object Clone()
        {
            return new CardItemModel(this) { ParentSet = ParentSet };
        }

        public void Remove()
        {
            ParentSet.CardItems.Remove(this);
        }
        
        public string Name //update to new format
        {
            get
            {
                return CardDef.Name;
            }
            set
            {
                if (value == CardDef.Name) return;
                CardDef.Name = value;
                AltItems.First(x => x.Alt == "").Properties.First(x => x.Property.Name == "Name").Value = value;
                RaisePropertyChanged("Name");
            }
        }
    }

    public class AltItemModel : ViewModelBase
    {
        public CardPropertySet AltDef { get; set; }
        public SizeItemModel _cardSize;
        public ObservableCollection<CardPropertyItemModel> _properties;
        public CardItemModel ParentCard { get; set; }
        public Card _tempImageCard;
        public RelayCommand RemoveAltCommand { get; private set; }

        public AltItemModel() //for adding new items
        {
            AltDef = new CardPropertySet();
            AltDef.Type = Guid.NewGuid().ToString(); // guarantees that the type will be unique as it's a dictionary key
            CardSize = ViewModelLocator.PreviewTabViewModel.DefaultSize;

            Properties = new ObservableCollection<CardPropertyItemModel>();
            
            var nameProp = new PropertyDef()
            {
                Hidden = false,
                Name = "Name",
                Type = PropertyType.String,
                TextKind = PropertyTextKind.FreeText,
                IgnoreText = false,
                IsUndefined = false
            };
            Properties.Add(new CardPropertyItemModel(nameProp, "CardName"));
            CustomPropertyChanged(null);

            RemoveAltCommand = new RelayCommand(RemoveAlt);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, action => CardSizeChanged(action));
        }

        public AltItemModel(CardPropertySet props) //for loading an existing collection
        {
            AltDef = props;
            _cardSize = ViewModelLocator.PreviewTabViewModel.CardSizes.FirstOrDefault(x => props.Size == x.SizeDef) ?? ViewModelLocator.PreviewTabViewModel.DefaultSize;
            Properties = new ObservableCollection<CardPropertyItemModel>(props.Properties.Select(x => new CardPropertyItemModel(x.Key, x.Value) { ParentAlt = this }));
            
            RemoveAltCommand = new RelayCommand(RemoveAlt);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, action => CardSizeChanged(action));
        }

        public AltItemModel(AltItemModel a) //for copying the item
        {
            AltDef = a.AltDef.Clone() as CardPropertySet;
            AltDef.Type = Guid.NewGuid().ToString();
            ParentCard = a.ParentCard;
            CardSize = a.CardSize;
            Properties = new ObservableCollection<CardPropertyItemModel>(AltDef.Properties.Select(x => new CardPropertyItemModel(x.Key, x.Value) { ParentAlt = this }));
            RemoveAltCommand = new RelayCommand(RemoveAlt);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, action => CardSizeChanged(action));
        }

        public void CustomPropertyChanged(CustomPropertyChangedMessage m)
        {
            AltDef.Properties = new Dictionary<PropertyDef, object>();
            foreach (var customprop in ViewModelLocator.PropertyTabViewModel.Items)
            {
                var prop = Properties.FirstOrDefault(x => x.Property.Name == customprop.Name);
                if (prop == null)
                {
                    prop = new CardPropertyItemModel(customprop) { ParentAlt = this };
                    Properties.Add(prop);
                }
                else
                {
                    if (prop.Property != customprop)
                        prop.Property = customprop;
                }
                if (prop.Value != null)
                    AltDef.Properties.Add(prop.Property.PropertyDef, prop.Value);
            }
            RaisePropertyChanged("GetProperties");
        }

        public void CardSizeChanged(CardSizeChangedMesssage m)
        {
            var size = m.Size;
            if (CardSize == size)
            {
                AltDef.Size = size.SizeDef;
                RaisePropertyChanged("CardSize");
            }
            else
            {
                CardSize = ViewModelLocator.PreviewTabViewModel.DefaultSize;
            }
        }

        public void RemoveAlt()
        {
            ParentCard.AltItems.Remove(this);
        }
                
        public ObservableCollection<CardPropertyItemModel> Properties
        {
            get { return _properties; }
            set { Set(ref _properties, value); }
        }

        public List<CardPropertyItemModel> GetProperties
        {
            get
            {
                return ViewModelLocator.PropertyTabViewModel.Items.Select(x => Properties.First(y => y.Property == x)).ToList();
            }
        }
        
        public string Name
        {
            get
            {
                return AltDef.Properties.First(x => x.Key.Name == "Name").Value.ToString();
            }
            set
            {
                var NameProp = AltDef.Properties.First(x => x.Key.Name == "Name");
                if (NameProp.Value.ToString() == value) return;
                AltDef.Properties[NameProp.Key] = value;
                RaisePropertyChanged("Name");
            }
        }

        public SizeItemModel CardSize
        {
            get
            {
                return _cardSize;
            }
            set
            {
                if (_cardSize == value) return;
                _cardSize = value;
                AltDef.Size = _cardSize.SizeDef;
                if (Alt == "") ParentCard.CardDef.Size = value.SizeDef;
                RaisePropertyChanged("CardSize");
            }
        }

        public string Alt
        {
            get
            {
                return AltDef.Type;
            }
            set
            {
                if (AltDef.Type == value) return;
                                                       //  if alt exists    alt is default    no alt given
                if (ParentCard.AltItems.Select(x => x.Alt).Contains(value) || value == "" || value == null) return;
                AltDef.Type = value;
                // ParentCard.UpdateCardAlts();
                // RefreshTempCard();
                RaisePropertyChanged("Alt");
                RaisePropertyChanged("AltName");
            }
        }

        public string AltName
        {
            get
            {
                return Alt == "" ? "Default" : Alt;
            }
        }
        
        public Visibility IsDefault
        {
            get
            {
                return Alt == "" ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        #region image section
        public BitmapImage CardImage
        {
            get
            {
                return GetImage();
            }
        }

        public BitmapImage ProxyImage
        {
            get
            {
                var proxyDef = ViewModelLocator.GameLoader.ProxyDef;
                var proxy = proxyDef.GenerateProxyImage(_tempImageCard.GetProxyMappings());

                Stream imageStream = new MemoryStream();

                proxy.Save(imageStream, ImageFormat.Png);

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                image.StreamSource = imageStream;
                image.EndInit();
                imageStream.Close();
                return image;
            }
        }

        public BitmapImage GetImage()
        {
            if (_tempImageCard == null) RefreshTempCard();
            var imagePath = "pack://application:,,,/Resources/Back.png";
            var files = Directory.GetFiles(ParentCard.CardDef.GetSet().ImagePackUri, _tempImageCard.GetImageUri() + ".*")
                            .Where(x => Path.GetFileNameWithoutExtension(x)
                            .Equals(_tempImageCard.GetImageUri(), StringComparison.InvariantCultureIgnoreCase))
                            .OrderBy(x => x.Length)
                            .ToArray();
            if (files.Length > 0) imagePath = _tempImageCard.GetPicture();
            Stream imageStream = null;
            if (imagePath.StartsWith("pack"))
            {
                var sri = Application.GetResourceStream(new Uri(imagePath));
                imageStream = sri.Stream;
            }
            else
            {
                imageStream = File.OpenRead(imagePath);
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

            var files =
                Directory.GetFiles(ParentCard.CardDef.GetSet().ImagePackUri, _tempImageCard.GetImageUri() + ".*")
                    .Where(x => Path.GetFileNameWithoutExtension(x)
                    .Equals(_tempImageCard.GetImageUri(), StringComparison.InvariantCultureIgnoreCase))
                    .OrderBy(x => x.Length)
                    .ToArray();
            if (files.Length == 0) return;

            // Delete all the old picture files
            foreach (var f in files.Select(x => new FileInfo(x)))
            {
                f.MoveTo(Path.Combine(garbage, f.Name));
            }
            RefreshTempCard();
        }

        public void SaveImage(string file)
        {
            DeleteImage();
            var newPath = Path.Combine(ParentCard.CardDef.GetSet().ImagePackUri, _tempImageCard.GetImageUri() + Path.GetExtension(file));
            File.Copy(file, newPath);
            RefreshTempCard();
        }

        public void RefreshTempCard()
        {
            _tempImageCard = new Card(ParentCard.CardDef);
            _tempImageCard.Alternate = AltDef.Type;
            ViewModelLocator.ProxyDesignViewModel.Card = this;
            RaisePropertyChanged("ProxyImage");
            RaisePropertyChanged("CardImage");
        }
    #endregion image section
    }

    public class CardPropertyItemModel : ViewModelBase
    {
        public AltItemModel ParentAlt { get; set; }
        public CustomPropertyItemModel Property { get; set; }
        public object _value;

        public CardPropertyItemModel(PropertyDef prop, object value)
        {
            Property = ViewModelLocator.PropertyTabViewModel.Items.First(x => x.Name == prop.Name);
            _value = value;
        }

        public CardPropertyItemModel(CustomPropertyItemModel prop)
        {
            Property = prop;
        }

        public string Name => Property.Name;

        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == value) return;
                _value = value;
                ParentAlt.AltDef.Properties[Property.PropertyDef] = value;
                if (Property.Name == "Name" && ParentAlt.Alt == "")
                    ParentAlt.ParentCard.Name = value.ToString();
                // ParentAlt.RefreshTempCard();
                RaisePropertyChanged("Value");
            }
        }
    }
}