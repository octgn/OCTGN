using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Octgn.Controls;
using Octgn.DataNew.Entities;
using Octgn.Core.DataExtensionMethods;

namespace Octgn.DeckBuilder
{
    /// <summary>
    /// Dialog for exporting card images to o8c or zip format
    /// </summary>
    public partial class ExportCardImagesDialog : DecorableWindow
    {
        private readonly Game _game;
        private readonly IDeck _deck;
        private List<CardImageItem> _cardImages;

        public ExportCardImagesDialog(Game game, IDeck deck)
        {
            InitializeComponent();
            _game = game;
            _deck = deck;
            LoadCardImages();
            Loaded += (s, args) =>
            {
                CardImagesList.Focus();
                if (CardImagesList.Items.Count > 0)
                {
                    CardImagesList.SelectedIndex = 0;
                }
            };
        }

        private void LoadCardImages()
        {
            _cardImages = new List<CardImageItem>();

            // Get all cards from deck sections
            var allCards = new List<IMultiCard>();

            if (_deck.Sections != null)
            {
                foreach (var section in _deck.Sections)
                {
                    if (section.Cards != null)
                    {
                        allCards.AddRange(section.Cards);
                    }
                }
            }

            // Remove duplicates
            var uniqueCards = allCards.GroupBy(c => c.Id).Select(g => g.First()).ToList();

            foreach (var card in uniqueCards)
            {
                var imagePath = GetCardImagePath(card);
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    if (!IsProxyImage(imagePath))
                    {
                        var set = card.GetSet();
                        _cardImages.Add(new CardImageItem
                        {
                            Card = card,
                            ImagePath = imagePath,
                            IsSelected = true,
                            SetName = set?.Name ?? "Unknown",
                            CardName = card.Name
                        });
                    }
                }
            }

            // Bind to UI
            CardImagesList.ItemsSource = _cardImages;
            UpdateSelectionCount();
        }

        private string GetCardImagePath(ICard card)
        {
            try
            {
                return card.GetPicture();
            }
            catch
            {
                return null;
            }
        }

        private bool IsProxyImage(string imagePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(imagePath);
            return fileName.IndexOf("proxy", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void CardImagesList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var item = CardImagesList.SelectedItem as CardImageItem;
                if (item != null)
                {
                    item.IsSelected = !item.IsSelected;
                    UpdateSelectionCount();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _cardImages)
            {
                item.IsSelected = true;
            }

            UpdateSelectionCount();
        }

        private void SelectNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _cardImages)
            {
                item.IsSelected = false;
            }

            UpdateSelectionCount();
        }

        private void ExportO8c_Click(object sender, RoutedEventArgs e)
        {
            var selectedCards = _cardImages.Where(c => c.IsSelected).ToList();
            if (!selectedCards.Any())
            {
                MessageBox.Show("Please select at least one card to export.", "No Cards Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                Filter = "Card Image Pack|*.o8c",
                DefaultExt = "o8c",
                FileName = $"{_game.Name} - Card Images"
            };

            if (sfd.ShowDialog() != true) return;

            try
            {
                ExportToO8c(selectedCards, sfd.FileName);
                MessageBox.Show($"Successfully exported {selectedCards.Count} card images to {sfd.FileName}",
                    "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting images: {ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportZip_Click(object sender, RoutedEventArgs e)
        {
            var selectedCards = _cardImages.Where(c => c.IsSelected).ToList();
            if (!selectedCards.Any())
            {
                MessageBox.Show("Please select at least one card to export.", "No Cards Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                Filter = "ZIP Archive|*.zip",
                DefaultExt = "zip",
                FileName = $"{_game.Name} - Card Images"
            };

            if (sfd.ShowDialog() != true) return;

            try
            {
                ExportToZip(selectedCards, sfd.FileName);
                MessageBox.Show($"Successfully exported {selectedCards.Count} card images to {sfd.FileName}",
                    "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting images: {ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToO8c(List<CardImageItem> cards, string filename)
        {
            // o8c format: {gameGuid}/Sets/{setGuid}/Cards/{imageFilename}
            if (File.Exists(filename)) File.Delete(filename);
            using (var archive = ZipFile.Open(filename, ZipArchiveMode.Create))
            {
                foreach (var card in cards)
                {
                    var set = card.Card.GetSet();
                    var imageFileName = Path.GetFileName(card.ImagePath);
                    var entryName = $"{_game.Id}/Sets/{set.Id}/Cards/{imageFileName}";

                    archive.CreateEntryFromFile(card.ImagePath, entryName, CompressionLevel.Optimal);
                }
            }
        }

        private void ExportToZip(List<CardImageItem> cards, string filename)
        {
            // Zip format uses human-readable names for sharing outside OCTGN
            if (File.Exists(filename)) File.Delete(filename);
            using (var archive = ZipFile.Open(filename, ZipArchiveMode.Create))
            {
                foreach (var card in cards)
                {
                    var gameDir = SafeFileName(_game.Name);
                    var set = card.Card.GetSet();
                    var setDir = SafeFileName(set?.Name ?? "Unknown");
                    var ext = Path.GetExtension(card.ImagePath);
                    var cardFileName = SafeFileName(card.CardName) + ext;
                    var entryName = $"{gameDir}/{setDir}/{cardFileName}";

                    archive.CreateEntryFromFile(card.ImagePath, entryName, CompressionLevel.Optimal);
                }
            }
        }

        private string SafeFileName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var safe = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
            return string.IsNullOrWhiteSpace(safe) ? "Unknown" : safe;
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateSelectionCount();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void UpdateSelectionCount()
        {
            var selected = _cardImages?.Count(c => c.IsSelected) ?? 0;
            SelectionCount.Text = $"{selected} of {_cardImages?.Count ?? 0} cards selected";
        }
    }

    public class CardImageItem : System.ComponentModel.INotifyPropertyChanged
    {
        public ICard Card { get; set; }
        public string ImagePath { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
        public string SetName { get; set; }
        public string CardName { get; set; }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
