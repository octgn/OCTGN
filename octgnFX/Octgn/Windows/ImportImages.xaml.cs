using GalaSoft.MvvmLight.Messaging;
using Octgn.UiMessages;

namespace Octgn.Windows
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Windows;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Forms;
    using System.Text.RegularExpressions;

    using log4net;

    using Octgn.Annotations;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew.Entities;
    using System.Linq;
    using Octgn.DeckBuilder;
    using Octgn.Library;
    using Octgn.Controls;
    using System.Windows.Data;
    using Octgn.Core;

    /// <summary>
    /// Interaction logic for ImportImages.xaml
    /// </summary>
    public partial class ImportImages : INotifyPropertyChanged
    {

        public ImportImages() : this(null)
        {
        }

        public ImportImages(Game game)
        {
            Game = game;
            if (!string.IsNullOrWhiteSpace(Prefs.ImportImagesLastPath))
                FolderDir = Prefs.ImportImagesLastPath;
            Items = new ObservableCollection<ImportImagesItem>();
            Sets = (new SetPropertyDef(Game.Sets())).Sets;
            Properties = game.AllProperties();
            InitializeComponent();
            setsCombo.SelectedIndex = 0;
            propertyCombo.SelectedIndex = 0;
        }

        public Game Game { get; private set; }
        public IEnumerable<Set> Sets { get; private set; }
        public IEnumerable<PropertyDef> Properties { get; private set; }
        public ObservableCollection<ImportImagesItem> Items { get; private set; }
        public static ObservableCollection<string> ImagePaths;

        private ImportImagesItem _selectedItem;

        public ImportImagesItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }
        
        private string _folderDir;

        public string FolderDir
        {
            get
            { return _folderDir; }
            set
            {
                if (value == _folderDir) return;
                _folderDir = value;
                OnPropertyChanged("FolderDir");
            }
        }
        
        public string SanitizeString(string name)
        {
            string ret = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToLower().TrimStart('0');
            return ret;
        }


        private void ButtonImportFolder(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (FolderDir != null && Directory.Exists(FolderDir)) dialog.SelectedPath = FolderDir;
            dialog.ShowDialog();
            FolderDir = dialog.SelectedPath;
            Prefs.ImportImagesLastPath = (string)dialog.SelectedPath;
        }

        private void LoadClicked(object sender, RoutedEventArgs e)
        {
            if (setsCombo.SelectedItem == null) return;
            if (propertyCombo.SelectedItem == null) return;
            if (!Directory.Exists(FolderDir)) return;
            Items.Clear();
            var selectedSet = setsCombo.SelectedItem as Set;

            var selectedProperty = propertyCombo.SelectedItem as PropertyDef;
            ImagePaths = new ObservableCollection<string>(Directory.EnumerateFiles(FolderDir).Where(file => Regex.IsMatch(file, @"^.+\.(jpg|jpeg|png)$")));

            foreach (var c in selectedSet.Cards)
            {
                foreach (var alternate in c.Properties)
                {
                    Card card = new Card(c);
                    card.Alternate = alternate.Key;
                    var cardPropertyValue = card.Properties[card.Alternate].Properties[selectedProperty].ToString();
                    var sanitizedValue = SanitizeString(cardPropertyValue);

                    // first check for exact matches, then if none show up we check for partial matches
                    var matchedImages = ImagePaths.Where(x => SanitizeString(Path.GetFileNameWithoutExtension(x)) == sanitizedValue).ToList();
                    if (matchedImages.Count() < 1) matchedImages = ImagePaths.Where(x => SanitizeString(Path.GetFileNameWithoutExtension(x)).Contains(sanitizedValue)).ToList();

                    var ret = new ImportImagesItem() { Path = null, Card = card, Filter = sanitizedValue, PossiblePaths = null };

                    if (matchedImages.Count() < 1)
                    {
                    }
                    else if (matchedImages.Count() > 1)
                    {
                        ret.PossiblePaths = new ObservableCollection<string>(matchedImages);
                    }
                    else
                    {
                        var singleImage = matchedImages.First();
                        ret.Path = singleImage;
                        ImagePaths.Remove(singleImage);
                        ret.PossiblePaths = new ObservableCollection<string>(matchedImages);
                    }
                    Items.Add(ret);
                }
            }
            OnPropertyChanged("Items");
        }

        private void ImportClicked(object sender, RoutedEventArgs e)
        {
            if (Items.Where(x => x.Path == null).Count() > 0)
            {
                var res = TopMostMessageBox.Show("Not all cards have an image matched.  Continue?", "Missing Images Warning",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation);
                if (res == MessageBoxResult.No)
                    return;
            }
            
            foreach (var item in Items)
            {

                var file = item.Path;
                if (!File.Exists(file)) continue;

                var card = item.Card;
                var set = card.GetSet();
                var garbage = Config.Instance.Paths.GraveyardPath;
                if (!Directory.Exists(garbage)) Directory.CreateDirectory(garbage);
                var imageUri = card.GetImageUri();

                var files =
                    Directory.GetFiles(set.ImagePackUri, imageUri + ".*")
                        .Where(x => Path.GetFileNameWithoutExtension(x).Equals(imageUri, StringComparison.InvariantCultureIgnoreCase))
                        .OrderBy(x => x.Length)
                        .ToArray();

                // Delete all the old picture files
                foreach (var f in files.Select(x => new FileInfo(x)))
                {
                    f.MoveTo(System.IO.Path.Combine(garbage, f.Name));
                }

                var newPath = System.IO.Path.Combine(set.ImagePackUri, imageUri + Path.GetExtension(file));
                File.Copy(file, newPath);
            }
            this.Close();
        }
        
        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class PathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Path.GetFileNameWithoutExtension(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }

    public partial class ImportImagesItem : INotifyPropertyChanged
    {
        private Card _card;
        
        private string _path;

        private ObservableCollection<string> _possiblePaths;

        private string _filter;

        public ImportImagesItem()
        {
        }

        public Card Card
        {
            get
            {
                return _card;
            }
            set
            {
                if (_card == value) return;
                _card = value;
                OnPropertyChanged("Card");
            }
        }

        public string Name
        {
            get
            {
                return _card.Properties[_card.Alternate].Properties.Where(x => x.Key.Name == "Name").First().Value.ToString();
            }
        }
        
        public ObservableCollection<string> PossiblePaths
        {
            get
            {
                if (_possiblePaths == null || _possiblePaths.Count() < 1) return ImportImages.ImagePaths;
                return _possiblePaths;
            }
            set
            {
                if (_possiblePaths == value) return;
                _possiblePaths = value;
                OnPropertyChanged("PossiblePaths");
            }
        }

        public string Error
        {
            get
            {
                if (_path != null) return null;
                if (_possiblePaths == null) return "No Images Found";
                else return "Multiple Images Found";
            }
        }

        public string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                if (_filter == value) return;
                _filter = value;
                OnPropertyChanged("Filter");
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                if (_path == value) return;
                _path = value;
                OnPropertyChanged("Path");
                OnPropertyChanged("Error");
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
