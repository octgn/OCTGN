﻿using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Octgn.Controls;
using Octgn.Definitions;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for GameList.xaml
    /// </summary>
    public partial class GameList
    {
        #region LoadEvent enum

        public enum LoadEvent
        {
            None,
            InstallGame
        };

        #endregion

        public GameList()
        {
            InitializeComponent();
            Program.GamesRepository.GameInstalled += GamesRepositoryGameInstalled;
        }

        public GameList(LoadEvent le)
        {
            InitializeComponent();
            Program.GamesRepository.GameInstalled += GamesRepositoryGameInstalled;
            if (le == LoadEvent.InstallGame)
                InstallGame();
        }

        public event EventHandler OnGameClick;

        private void ReloadGameList()
        {
            stackPanel1.Children.Clear();
            foreach (var gs in Program.GamesRepository.AllGames.Select(g => new GameListItem {Game = g}))
            {
                //gs.MouseDoubleClick += new MouseButtonEventHandler(gs_MouseDoubleClick);
                gs.MouseUp += GsMouseUp;
                stackPanel1.Children.Add(gs);
            }
        }

        private void GsMouseUp(object sender, MouseButtonEventArgs e)
        {
            var gs = (GameListItem) sender;
            if (OnGameClick != null)
            {
                OnGameClick.Invoke(gs.Game, new EventArgs());
            }
        }

        private void GamesRepositoryGameInstalled(object sender, EventArgs e)
        {
            ReloadGameList();
        }

        /*
        private void gs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var gs = (GameListItem) sender;
            if (OnGameClick != null)
            {
                OnGameClick.Invoke(gs.Game, new EventArgs());
            }
        }
        */

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            ReloadGameList();
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            Program.GamesRepository.GameInstalled -= GamesRepositoryGameInstalled;
        }

        public void InstallGame()
        {
            var ofd = new OpenFileDialog {Filter = "Game definition files (*.o8g)|*.o8g"};
            if (ofd.ShowDialog() != true) return;

            //Fix def filename
            String newFilename = Uri.UnescapeDataString(ofd.FileName);
            if (!newFilename.ToLower().Equals(ofd.FileName.ToLower()))
            {
                try
                {
                    File.Move(ofd.FileName, newFilename);
                }
                catch (Exception)
                {
                    MessageBox.Show(
                        "This file is currently in use. Please close whatever application is using it and try again.");
                    return;
                }
            }

            try
            {
                //Move the definition file to a new location, so that the old one can be deleted
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn",
                                           "Defs");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var fi = new FileInfo(newFilename);
                string copyto = Path.Combine(path, fi.Name);
                try
                {
                    if (newFilename.ToLower() != copyto.ToLower())
                        File.Copy(newFilename, copyto, true);
                }
                catch (Exception)
                {
                    MessageBox.Show(
                        "File in use. You shouldn't install games or sets when in the deck editor or when playing a game.");
                    return;
                }
                newFilename = copyto;
                // Open the archive
                GameDef game = GameDef.FromO8G(newFilename);
                if (!game.CheckVersion()) return;

                // Check if the game already exists
                if (Program.GamesRepository.Games.Any(g => g.Id == game.Id))
                    if (
                        MessageBox.Show("This game already exists.\r\nDo you want to overwrite it?", "Confirmation",
                                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes)
                        return;

                var gameData = new Data.Game
                                   {
                                       Id = game.Id,
                                       Name = game.Name,
                                       Filename = newFilename,
                                       Version = game.Version,
                                       CardWidth = game.CardDefinition.Width,
                                       CardHeight = game.CardDefinition.Height,
                                       CardBack = game.CardDefinition.back,
                                       DeckSections = game.DeckDefinition.Sections.Keys,
                                       SharedDeckSections =
                                           game.SharedDeckDefinition == null
                                               ? null
                                               : game.SharedDeckDefinition.Sections.Keys,
                                       Repository = Program.GamesRepository
                                   };
                Program.GamesRepository.InstallGame(gameData, game.CardDefinition.Properties.Values);
            }
            catch (FileFormatException)
            {
                //Removed ex.Message. The user doesn't need to see the exception
                MessageBox.Show("Your game definition file is corrupt. Please redownload it.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}