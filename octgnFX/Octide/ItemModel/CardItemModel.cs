using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;
using Octgn.Library;
using Octide.Messages;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Octide.ItemModel
{
    public class CardItemModel : IdeListBoxItemBase, ICloneable
    {
        public Card _cardDef { get; private set; }
        private ObservableCollection<AltItemModel> _items;
        
        public AltItemModel Default => Items.First(x => x.Alt == "");

        public CardItemModel() //for adding new items
        {
            var guid = Guid.NewGuid();  //every card has a new GUID
            _cardDef = new Card(
                guid,
                ViewModelLocator.SetTabViewModel.SelectedItem.Id,
                "Card",
                guid.ToString(),
                "",
                ViewModelLocator.PreviewTabViewModel.DefaultSize.SizeDef,
                new Dictionary<string, CardPropertySet>());

            Items = new ObservableCollection<AltItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                UpdateCardAlts();
            };

            var alt = new AltItemModel();
            alt.Parent = this;
            alt.AltDef.Type = "";
            Items.Add(alt);

            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);

        }

        public CardItemModel(Card c) //for loading an existing collection
        {
            _cardDef = c;
            Items = new ObservableCollection<AltItemModel>(_cardDef.Properties.Select(x => new AltItemModel(x.Value) { Parent = this }));
            Items.CollectionChanged += (a, b) =>
            {
                UpdateCardAlts();
            };
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
        }

        public CardItemModel(CardItemModel c) //for copying the item
        {
            _cardDef = new Card(c._cardDef);
            _cardDef.Id = Guid.NewGuid();
            _cardDef.ImageUri = _cardDef.Id.ToString();
            Parent = c.Parent;
            Items = new ObservableCollection<AltItemModel>(_cardDef.Properties.Select(x => new AltItemModel(x.Value) { Parent = this }));
            Items.CollectionChanged += (a, b) =>
            {
                UpdateCardAlts();
            };
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
        }

        public ObservableCollection<AltItemModel> Items
        {
            get { return _items; }
            set { Set(ref _items, value); }
        }

        public void UpdateDefaultName()
        {
            _cardDef.Name = Default.Name;
            RaisePropertyChanged("Default");
            RaisePropertyChanged("Name");
        }

        public void UpdateCardAlts()
        {
            _cardDef.Properties = Items.Select(x => x.AltDef).ToDictionary(x => x.Type, x => x);
        }

        public object Clone()
        {
            return new CardItemModel(this);
        }
        
        public void Copy()
        {
            if (CanCopy == false) return;
            var index = (Parent as SetItemModel).CardItems.IndexOf(this);
            (Parent as SetItemModel).CardItems.Insert(index, Clone() as CardItemModel);
        }

        public void Remove()
        {
            if (CanRemove == false) return;
            (Parent as SetItemModel).CardItems.Remove(this);
        }

        public void Insert()
        {
            if (CanInsert == false) return;
            var index = (Parent as SetItemModel).CardItems.IndexOf(this);
            (Parent as SetItemModel).CardItems.Insert(index, new CardItemModel() { Parent = Parent });
        }

        public string Name //update to new format
        {
            get
            {
                return _cardDef.Name;
            }
            set
            {
                if (value == _cardDef.Name) return;
                _cardDef.Name = value;
                Items.First(x => x.Alt == "").Properties.First(x => x.Property.Name == "Name").Value = value;
                RaisePropertyChanged("Name");
            }
        }
    }

    public class AltItemModel : IdeListBoxItemBase, ICloneable
    {
        public CardPropertySet AltDef { get; set; }
        public SizeItemModel _cardSize;
        public ObservableCollection<CardPropertyItemModel> _properties;
        public Card _tempImageCard;

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

            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, action => CardSizeChanged(action));
        }

        public AltItemModel(CardPropertySet props) //for loading an existing collection
        {
            AltDef = props;
            _cardSize = ViewModelLocator.PreviewTabViewModel.CardSizes.FirstOrDefault(x => props.Size == x.SizeDef) ?? ViewModelLocator.PreviewTabViewModel.DefaultSize;
            Properties = new ObservableCollection<CardPropertyItemModel>(props.Properties.Select(x => new CardPropertyItemModel(x.Key, x.Value) { ParentAlt = this }));

            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, action => CardSizeChanged(action));
        }

        public AltItemModel(AltItemModel a) //for copying the item
        {
            AltDef = a.AltDef.Clone() as CardPropertySet;
            AltDef.Type = Guid.NewGuid().ToString();
            Parent = a.Parent;
            CardSize = a.CardSize;
            Properties = new ObservableCollection<CardPropertyItemModel>(AltDef.Properties.Select(x => new CardPropertyItemModel(x.Key, x.Value) { ParentAlt = this }));

            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
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
            (Parent as CardItemModel).Items.Remove(this);
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
                if (Alt == "") (Parent as CardItemModel)._cardDef.Size = value.SizeDef;
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
                if ((Parent as CardItemModel).Items.Select(x => x.Alt).Contains(value) || value == "" || value == null) return;
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
            var files = Directory.GetFiles((Parent as CardItemModel)._cardDef.GetSet().ImagePackUri, _tempImageCard.GetImageUri() + ".*")
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
                Directory.GetFiles((Parent as CardItemModel)._cardDef.GetSet().ImagePackUri, _tempImageCard.GetImageUri() + ".*")
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
            var newPath = Path.Combine((Parent as CardItemModel)._cardDef.GetSet().ImagePackUri, _tempImageCard.GetImageUri() + Path.GetExtension(file));
            File.Copy(file, newPath);
            RefreshTempCard();
        }

        public void RefreshTempCard()
        {
            _tempImageCard = new Card((Parent as CardItemModel)._cardDef);
            _tempImageCard.Alternate = AltDef.Type;
            ViewModelLocator.ProxyCardViewModel.Card = this;
            RaisePropertyChanged("ProxyImage");
            RaisePropertyChanged("CardImage");
        }

        public object Clone()
        {
            return new AltItemModel(this);
        }

        public void Copy()
        {
            if (CanCopy == false) return;
            var index = (Parent as CardItemModel).Items.IndexOf(this);
            (Parent as CardItemModel).Items.Insert(index, Clone() as AltItemModel);
        }

        public void Remove()
        {
            if (CanRemove == false) return;
            (Parent as CardItemModel).Items.Remove(this);
        }

        public void Insert()
        {
            if (CanInsert == false) return;
            var index = (Parent as CardItemModel).Items.IndexOf(this);
            (Parent as CardItemModel).Items.Insert(index, new AltItemModel() { Parent = Parent });
        }

        #endregion image section
    }

    public class CardPropertyItemModel : ViewModelBase
    {
        public AltItemModel ParentAlt { get; set; }
        public PropertyItemModel Property { get; set; }
        public object _value;

        public CardPropertyItemModel(PropertyDef prop, object value)
        {
            Property = ViewModelLocator.PropertyTabViewModel.Items.First(x => x.Name == prop.Name);
            _value = value;
        }

        public CardPropertyItemModel(PropertyItemModel prop)
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
                    (ParentAlt.Parent as CardItemModel).Name = value.ToString();
                // ParentAlt.RefreshTempCard();
                RaisePropertyChanged("Value");
            }
        }
    }
}
