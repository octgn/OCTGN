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

namespace Octide.ViewModel
{
    public class SetTabViewModel : ViewModelBase
    {

        public SetTabViewModel()
        {
            AddSetCommand = new RelayCommand(AddSet);
            RemoveSetCommand = new RelayCommand(RemoveSet, EnableSetButton);
            AddCardCommand = new RelayCommand(AddCard);
            RemoveCardCommand = new RelayCommand(RemoveCard, EnableCardButton);
            CopyCardCommand = new RelayCommand(CopyCard, EnableCardButton);
            UpCardCommand = new RelayCommand(MoveCardUp, EnableCardButton);
            DownCardCommand = new RelayCommand(MoveCardDown, EnableCardButton);
            AddAltCommand = new RelayCommand(AddAlt);
            RemoveAltCommand = new RelayCommand(RemoveAlt, EnableAltButton);
            CopyAltCommand = new RelayCommand(CopyAlt, EnableAltButton);
            UpAltCommand = new RelayCommand(MoveAltUp, EnableAltUpButton);
            DownAltCommand = new RelayCommand(MoveAltDown, EnableAltDownButton);
            SetPanelVisibility = Visibility.Hidden;
            CardPanelVisibility = Visibility.Hidden;
            AltPanelVisibility = Visibility.Hidden;

            SetItems = new ObservableCollection<SetItemModel>(ViewModelLocator.GameLoader.Sets.Select(x => new SetItemModel(x)));

            SetItems.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Sets = SetItems.Select(x => x._set).ToList();
            };

        }


        #region set tab

        private Visibility _setPanelVisibility;
        private SetItemModel _selectedSet;
        public ObservableCollection<SetItemModel> SetItems { get; private set; }

        public RelayCommand AddSetCommand { get; private set; }
        public RelayCommand RemoveSetCommand { get; private set; }


        public SetItemModel SelectedSet
        {
            get { return _selectedSet; }
            set
            {
                if (value == _selectedSet) return;
                _selectedSet = value;
                SetPanelVisibility = value == null ? Visibility.Hidden : Visibility.Visible;
                RaisePropertyChanged("SelectedSet");
                RemoveSetCommand.RaiseCanExecuteChanged();
            //    SelectedCard = SelectedSet.CardItems.FirstOrDefault() ?? null;
            }
        }

        public Visibility SetPanelVisibility
        {
            get { return _setPanelVisibility; }
            set
            {
                if (value == _setPanelVisibility) return;
                _setPanelVisibility = value;
                RaisePropertyChanged("SetPanelVisibility");
            }
        }

        public bool EnableSetButton()
        {
            return SelectedSet != null;
        }

        public void AddSet()
        {
            var ret = new SetItemModel() { Name = "Set" };
            SetItems.Add(ret);
            SelectedSet = ret;
            RaisePropertyChanged("SetItems");
        }

        public void RemoveSet()
        {
            SetItems.Remove(SelectedSet);
        }
        #endregion


        #region card tab

        private Visibility _cardPanelVisibility;
        private CardItemModel _selectedCard;


        public RelayCommand AddCardCommand { get; private set; }
        public RelayCommand RemoveCardCommand { get; private set; }
        public RelayCommand CopyCardCommand { get; private set; }
        public RelayCommand UpCardCommand { get; private set; }
        public RelayCommand DownCardCommand { get; private set; }

        public CardItemModel SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                if( !Set( ref _selectedCard, value ) ) return;

                CardPanelVisibility = value == null ? Visibility.Hidden : Visibility.Visible;

                RaisePropertyChanged( nameof( SelectedCard ) );
                RemoveCardCommand.RaiseCanExecuteChanged();
                UpCardCommand.RaiseCanExecuteChanged();
                DownCardCommand.RaiseCanExecuteChanged();
                CopyCardCommand.RaiseCanExecuteChanged();
                if (value != null)
                {
                    SelectedAlt = SelectedCard.Default;
                    RaisePropertyChanged("SelectedAlt");
                }
            }
        }

        public Visibility CardPanelVisibility
        {
            get { return _cardPanelVisibility; }
            set { Set( ref _cardPanelVisibility, value ); }
        }

        public bool EnableCardButton() => SelectedCard != null;

        public void AddCard()
        {
            var ret = new CardItemModel();
            SelectedSet.CardItems.Add(ret);
            SelectedCard = ret;
            RaisePropertyChanged("SelectedCard");
        }

        public void RemoveCard()
        {
            SelectedSet.CardItems.Remove(SelectedCard);
            RaisePropertyChanged("SelectedCard");
        }

        public void CopyCard()
        {
            if (SelectedCard == null) return;
            var ret = new CardItemModel(SelectedCard);
            SelectedSet.CardItems.Add(ret);
            SelectedCard = ret;
            RaisePropertyChanged("SelectedCard");
        }

        public void MoveCardUp()
        {
            MoveItem(-1);
        }

        public void MoveCardDown()
        {
            MoveItem(1);
        }

        public void MoveItem(int move)
        {
            var index = SelectedSet.CardItems.IndexOf(SelectedCard);
            int newIndex = index + move;
            if (newIndex < 0 || newIndex >= SelectedSet.CardItems.Count) return;
            SelectedSet.CardItems.Move(index, index + move);
        }
        #endregion


        #region alt tab

        private AltItemModel _selectedAlt;
        private Visibility _altPanelVisibility;


        public RelayCommand AddAltCommand { get; private set; }
        public RelayCommand RemoveAltCommand { get; private set; }
        public RelayCommand CopyAltCommand { get; private set; }
        public RelayCommand UpAltCommand { get; private set; }
        public RelayCommand DownAltCommand { get; private set; }

        public Visibility AltPanelVisibility
        {
            get { return _altPanelVisibility; }
            set { Set(ref _altPanelVisibility, value); }
        }

        public AltItemModel SelectedAlt
        {
            get { return _selectedAlt; }
            set
            {
                if (!Set(ref _selectedAlt, value)) return;
                AltPanelVisibility = value == null ? Visibility.Hidden : Visibility.Visible;
                RaisePropertyChanged(nameof(SelectedAlt));
                RemoveAltCommand.RaiseCanExecuteChanged();
                UpAltCommand.RaiseCanExecuteChanged();
                DownAltCommand.RaiseCanExecuteChanged();
                CopyAltCommand.RaiseCanExecuteChanged();
            }
        }

        public bool EnableAltButton()
        {
            if (_selectedAlt != null && _selectedAlt == SelectedCard.Default) return false;
            return _selectedAlt != null;
        }
        
        public void AddAlt()
        {
            var ret = new AltItemModel() { ParentCard = SelectedCard };
            SelectedCard.AltItems.Add(ret);
            SelectedAlt = ret;
            RaisePropertyChanged("SelectedAlt");
        }

        public void RemoveAlt()
        {
            SelectedCard.AltItems.Remove(SelectedAlt);
            RaisePropertyChanged("SelectedAlt");
        }

        public void CopyAlt()
        {
            if (SelectedAlt == null) return;
            var ret = new AltItemModel(SelectedAlt);
            SelectedCard.AltItems.Add(ret);
            SelectedAlt = ret;
            RaisePropertyChanged("SelectedAlt");
        }

        public void MoveAltUp()
        {
            MoveAlt(-1);
        }

        public bool EnableAltUpButton()
        {
            if (_selectedAlt == null || _selectedAlt == SelectedCard.Default) return false;
            return SelectedCard.AltItems.IndexOf(_selectedAlt) > 1;
        }

        public void MoveAltDown()
        {
            MoveAlt(1);
        }

        public bool EnableAltDownButton()
        {
            if (_selectedAlt == null || _selectedAlt == SelectedCard.Default) return false;
            return SelectedCard.AltItems.Last() != _selectedAlt;
        }

        public void MoveAlt(int move)
        {
            var index = SelectedCard.AltItems.IndexOf(SelectedAlt);
            int newIndex = index + move;
            if (newIndex < 0 || newIndex >= SelectedCard.AltItems.Count) return;
            SelectedCard.AltItems.Move(index, index + move);
            UpAltCommand.RaiseCanExecuteChanged();
            DownAltCommand.RaiseCanExecuteChanged();
        }
        
        #endregion


        #region properties
        
        public IEnumerable<SizeListItemModel> CardSizes => ViewModelLocator.SizeTabViewModel.Items;

        #endregion
    }

    public class SetItemModel : ViewModelBase
    {
        public Set _set;
        private ObservableCollection<CardItemModel> _cardItems;

        public ObservableCollection<CardItemModel> CardItems
        {
            get { return _cardItems; }
            set { Set( ref _cardItems, value ); }
        }

        public SetItemModel() //for creating a new set
        {
            _set = new Set();
            _set.Id = Guid.NewGuid();
            _set.GameId = ViewModelLocator.GameLoader.Game.Id;
            _set.Version = Version.Parse("1.0");
            _set.GameVersion = ViewModelLocator.GameLoader.Game.Version;
            string installPath = Path.Combine(ViewModelLocator.GameLoader.GamePath, "Sets", _set.Id.ToString());
            _set.InstallPath = installPath;
            _set.Filename = Path.Combine(installPath, "set.xml");
            _set.PackUri = Path.Combine(installPath, "Cards");
            _set.Hidden = false;
            _set.Cards = new List<Card>();

            CardItems = new ObservableCollection<CardItemModel>();

            CardItems.CollectionChanged += (a, b) =>
            {
                _set.Cards = CardItems.Select(x => x._card).ToList();
            };
            RaisePropertyChanged("CardItems");
        }

        public SetItemModel(Set s) // For loading existing set data
        {
            _set = s;
            CardItems = new ObservableCollection<CardItemModel>(_set.Cards.Select(x => new CardItemModel(x)));
            CardItems.CollectionChanged += (a, b) =>
            {
                _set.Cards = CardItems.Select(x => x._card).ToList();
            };
            RaisePropertyChanged("CardItems");

        }

        public Guid Id
        {
            get
            {
                return _set.Id;
            }
        }

        public string Name
        {
            get
            {
                return _set.Name;
            }
            set
            {
                if (value == _set.Name) return;
                _set.Name = value;
                RaisePropertyChanged("Name");
            }
        }
    }

    public class CardItemModel : ViewModelBase
    {
        public Card _card;
        private ObservableCollection<AltItemModel> _altItems;

        public AltItemModel Default => AltItems.First(x => x.Alt == "");

        public ObservableCollection<AltItemModel> AltItems
        {
            get { return _altItems; }
            set { Set(ref _altItems, value); }
        }

        public CardItemModel() //for adding new items
        {
            var guid = Guid.NewGuid();
            _card = new Card(
                guid,
                ViewModelLocator.SetTabViewModel.SelectedSet.Id,
                "Card",
                guid.ToString(),
                "",
                ViewModelLocator.SizeTabViewModel.DefaultSize._size,
                new Dictionary<string, CardPropertySet>());

            var alt = new AltItemModel() { ParentCard = this };
            alt._altCard.Type = "";
            _card.Properties.Add("", alt._altCard);

            AltItems = new ObservableCollection<AltItemModel>() { alt };
            AltItems.CollectionChanged += (a, b) =>
            {
                RefreshAltItems();
            };

        }

        public CardItemModel(Card c) //for loading an existing collection
        {
            _card = c;
            AltItems = new ObservableCollection<AltItemModel>(_card.Properties.Select(x => new AltItemModel(x.Value) { ParentCard = this }));
            AltItems.CollectionChanged += (a, b) =>
            {
                RefreshAltItems();
            };
        }

        public CardItemModel(CardItemModel c) //for copying the item
        {
            _card = new Card(c._card);
            _card.Id = Guid.NewGuid();
            _card.ImageUri = _card.Id.ToString();
            AltItems = new ObservableCollection<AltItemModel>(_card.Properties.Select(x => new AltItemModel(x.Value) { ParentCard = this }));
            AltItems.CollectionChanged += (a, b) =>
            {
                RefreshAltItems();
            };
        }
                
        public void UpdateDefaultName()
        {
            _card.Name = Default.Name;
            RaisePropertyChanged("Default");
            RaisePropertyChanged("Name");
        }

        public void RefreshAltItems()
        {
            _card.Properties = AltItems.Select(x => x._altCard).ToDictionary(x => x.Type, x => x);
        }
                
        public string Name
        {
            get
            {
                return _card.Name;
            }
            set
            {
                if (value == _card.Name) return;
                _card.Name = value;
                RaisePropertyChanged("Name");

            }
        }

        public Guid Id
        {
            get
            {
                return _card.Id;
            }
            set
            {
                if (value == _card.Id) return;
                _card.Id = value;
                RaisePropertyChanged("Id");
            }
        }

        public Guid SetId
        {
            get
            {
                return _card.SetId;
            }
            set
            {
                if (value == _card.SetId) return;
                _card.SetId = value;
                RaisePropertyChanged("SetId");
            }
        }
    }

    public class AltItemModel : ViewModelBase
    {
        public CardPropertySet _altCard;
        public SizeListItemModel _cardSize;
        public ObservableCollection<CardPropertyItemModel> _properties;
        public CardItemModel _parentCard;
        public Card _tempImageCard;

        public void RefreshTempCard()
        { 
            _tempImageCard = new Card(_parentCard._card);
            _tempImageCard.Alternate = _altCard.Type;
            RaisePropertyChanged("ProxyImage");
            RaisePropertyChanged("CardImage");
        }

        public PropertyDef _nameDef => _altCard.Properties.First(x => x.Key.Name == "Name").Key;

        public AltItemModel() //for adding new items
        {
            _altCard = new CardPropertySet();
            _altCard.Type = Guid.NewGuid().ToString();
            CardSize = ViewModelLocator.SizeTabViewModel.Items.First(x => x.Default);
            _altCard.Properties = new Dictionary<PropertyDef, object>();

            var nameProp = new PropertyDef()
            {
                Hidden = false,
                Name = "Name",
                Type = PropertyType.String,
                TextKind = PropertyTextKind.FreeText,
                IgnoreText = false,
                IsUndefined = false
            };

            _altCard.Properties.Add(nameProp, "CardName");
            AltTypeVisibility = Visibility.Collapsed;

            Properties = new ObservableCollection<CardPropertyItemModel>();
        }

        public AltItemModel(CardPropertySet props) //for loading an existing collection
        {
            _altCard = props;
            CardSize = ViewModelLocator.SizeTabViewModel.Items.FirstOrDefault(x => props.Size == x._size) ?? ViewModelLocator.SizeTabViewModel.Items.First(x => x.Default);
            Properties = new ObservableCollection<CardPropertyItemModel>(props.Properties.Where(x => x.Key.Name != "Name").Select(x => new CardPropertyItemModel(x.Key, x.Value) { _alt = this }));

            AltTypeVisibility = (Alt == "") ? Visibility.Collapsed : Visibility.Visible;
        }

        public AltItemModel(AltItemModel a) //for copying the item
        {
            _altCard = a._altCard.Clone() as CardPropertySet;
            _altCard.Type = Guid.NewGuid().ToString();
            ParentCard = a.ParentCard;
            CardSize = a.CardSize;
            Properties = new ObservableCollection<CardPropertyItemModel>(_altCard.Properties.Where(x => x.Key.Name != "Name").Select(x => new CardPropertyItemModel(x.Key, x.Value) { _alt = this }));
            AltTypeVisibility = Visibility.Visible;
        }

        public CardItemModel ParentCard
        {
            get
            {
                return _parentCard;
            }
            set
            {
                if (value == _parentCard) return;
                _parentCard = value;
                RaisePropertyChanged("ParentCard");
            }
        }
        

        public void UpdateProperty(string property)
        {
            RaisePropertyChanged(property);
        }

        public BitmapImage GetImage()
        {
            if (_tempImageCard == null) RefreshTempCard();
            var imagePath = "pack://application:,,,/Resources/Back.png";
            var files = Directory.GetFiles(_parentCard._card.GetSet().ImagePackUri, _tempImageCard.GetImageUri() + ".*")
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
                Directory.GetFiles(_parentCard._card.GetSet().ImagePackUri, _tempImageCard.GetImageUri() + ".*")
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
            var newPath = Path.Combine(_parentCard._card.GetSet().ImagePackUri, _tempImageCard.GetImageUri() + Path.GetExtension(file));
            File.Copy(file, newPath);
            RefreshTempCard();
        }

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
                var proxyDef = ViewModelLocator.GameLoader.Game.GetCardProxyDef();
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

        public ObservableCollection<CardPropertyItemModel> Properties
        {
            get { return _properties; }
            set { Set(ref _properties, value); }
        }

        public List<CardPropertyItemModel> GetProperties
        {
            get
            {
                return ViewModelLocator.PropertyTabViewModel.Items.Select(x => {
                    var prop = Properties.FirstOrDefault(y => y._def == x);
                    if (prop == null)
                    {
                        var newProp = new CardPropertyItemModel(x);
                        newProp._alt = this;
                        Properties.Add(newProp);
                        return newProp;
                    }
                    return prop;
                }).ToList();
            }
        }

        public Visibility _altTypeVisibility;

        public Visibility AltTypeVisibility
        {
            get { return _altTypeVisibility; }
            set { Set(ref _altTypeVisibility, value); }
        }

        public string Name
        {
            get
            {
                return _altCard.Properties[_nameDef].ToString();
            }
            set
            {
                if (_altCard.Properties[_nameDef].ToString() == value) return;
                _altCard.Properties[_nameDef] = value;
                _parentCard.UpdateDefaultName();
                RaisePropertyChanged("Name");
                RefreshTempCard();
            }
        }

        public SizeListItemModel CardSize
        {
            get
            {
                return _cardSize;
            }
            set
            {
                if (_cardSize == value) return;
                _cardSize = value;
                _altCard.Size = _cardSize._size;
                RaisePropertyChanged("CardSize");
            }
        }

        public string Alt
        {
            get
            {
                return _altCard.Type;
            }
            set
            {
                if (_altCard.Type == value) return;
                                                        //  if alt exists      alt is default no alt given
                if (_parentCard.AltItems.Select(x => x.Alt).Contains(value) || value == "" || value == null) return;
                _altCard.Type = value;
                _parentCard.RefreshAltItems();
                RefreshTempCard();
                RaisePropertyChanged("Alt");
            }
        }
    }

    public class CardPropertyItemModel : ViewModelBase
    {
        public AltItemModel _alt;
        public PropertyListItemModel _def { get; private set; }
        public object _value;

        public CardPropertyItemModel(PropertyDef prop, object value)
        {
            _def = ViewModelLocator.PropertyTabViewModel.Items.First(x => x.Name == prop.Name);
            _value = value;
        }

        public CardPropertyItemModel(PropertyListItemModel prop)
        {
            _def = prop;
        }

        public string Name => _def.Name;

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
                _alt._altCard.Properties[_def._property] = value;
                _alt.RefreshTempCard();
                RaisePropertyChanged("Value");
            }
        }
    }
}