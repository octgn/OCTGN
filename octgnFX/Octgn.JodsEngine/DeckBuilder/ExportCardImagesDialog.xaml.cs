using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Octgn.DataNew.Entities;
using Octgn.DataNew;

namespace Octgn.DeckBuilder
{
    /// <summary>
    /// Dialog for exporting card images to o8c or zip format
    /// </summary>
    public partial class ExportCardImagesDialog : Window
    {
        private readonly Game _game;
        private readonly Deck _deck;
        private List<CardImageItem> _cardImages;

        public ExportCardImagesDialog(Game game, Deck deck)
        {
            InitializeComponent();
            _game = game;
            _deck = deck;
            LoadCardImages();
        }

        private void LoadCardImages()
        {
            _cardImages = new List<CardImageItem>();
            
            // Get all cards from deck sections
            var allCards = new List<Card>();
            
            if (_deck.DeckSections != null)
            {
                foreach (var section in _deck.DeckSections)
                {
                    if (section.Cards != null)
                    {
                        allCards.AddRange(section.Cards.Select(c => c.Card));
                    }
                }
            }
            
            if (_deck.DeckSharedSections != null)
            {
                foreach (var section in _deck.DeckSharedSections)
                {
                    if (section.Cards != null)
                    {
                        allCards.AddRange(section.Cards.Select(c => c.Card));
                    }
                }
            }

            // Remove duplicates and filter out proxy images
            var uniqueCards = allCards.GroupBy(c => c.Id).Select(g => g.First()).ToList();
            
            foreach (var card in uniqueCards)
            {
                var imagePath = GetCardImagePath(card);
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    // Check if it's a proxy image (skip if it is)
                    if (!IsProxyImage(imagePath))
                    {
                        _cardImages.Add(new CardImageItem
                        {
                            Card = card,
                            ImagePath = imagePath,
                            IsSelected = true,
                            SetName = card.Set?.Name ?? "Unknown",
                            CardName = card.Name
                        });
                    }
                }
            }

            // Bind to UI
            CardImagesList.ItemsSource = _cardImages;
            UpdateSelectionCount();
        }

        private string GetCardImagePath(Card card)
        {
            try
            {
                var imageUri = card.GetImageUri();
                if (imageUri == null) return null;

                // Convert URI to local file path
                var localPath = imageUri.LocalPath;
                return localPath;
            }
            catch
            {
                return null;
            }
        }

        private bool IsProxyImage(string imagePath)
        {
            // Check if the image is a proxy image
            // Proxy images typically have specific naming or location patterns
            var fileName = Path.GetFileNameWithoutExtension(imagePath);
            return fileName.Contains("proxy", StringComparison.OrdinalIgnoreCase);
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _cardImages)
            {
                item.IsSelected = true;
            }
            CardImagesList.Items.Refresh();
            UpdateSelectionCount();
        }

        private void SelectNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _cardImages)
            {
                item.IsSelected = false;
            }
            CardImagesList.Items.Refresh();
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
            using (var archive = ZipFile.Open(filename, ZipArchiveMode.Create))
            {
                foreach (var card in cards)
                {
                    var setDir = SafeFileName(card.SetName);
                    var cardFileName = SafeFileName(card.CardName) + ".png";
                    var entryName = $"{setDir}/{cardFileName}";
                    
                    archive.CreateEntryFromFile(card.ImagePath, entryName, CompressionLevel.Optimal);
                }
            }
        }

        private void ExportToZip(List<CardImageItem> cards, string filename)
        {
            using (var archive = ZipFile.Open(filename, ZipArchiveMode.Create))
            {
                foreach (var card in cards)
                {
                    // Use game/set/card names instead of GUIDs
                    var gameDir = SafeFileName(_game.Name);
                    var setDir = SafeFileName(card.SetName);
                    var cardFileName = SafeFileName(card.CardName) + ".png";
                    var entryName = $"{gameDir}/{setDir}/{cardFileName}";
                    
                    archive.CreateEntryFromFile(card.ImagePath, entryName, CompressionLevel.Optimal);
                }
            }
        }

        private string SafeFileName(string name)
        {
            // Remove invalid characters from filename
            var invalidChars = Path.GetInvalidFileNameChars();
            var safe = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
            return string.IsNullOrWhiteSpace(safe) ? "Unknown" : safe;
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

    public class CardImageItem
    {
        public Card Card { get; set; }
        public string ImagePath { get; set; }
        public bool IsSelected { get; set; }
        public string SetName { get; set; }
        public string CardName { get; set; }
    }
}
