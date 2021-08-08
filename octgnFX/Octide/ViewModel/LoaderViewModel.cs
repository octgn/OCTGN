// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;

namespace Octide.ViewModel
{
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using Microsoft.Win32;
    using Octide.Messages;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class LoaderViewModel : ViewModelBase
    {
        public RelayCommand NewGameCommand { get; private set; }
        public RelayCommand ImportGameCommand { get; private set; }
        public RelayCommand LoadGameCommand { get; private set; }
        public LoaderViewModel()
        {
            NewGameCommand = new RelayCommand(NewGame);
            ImportGameCommand = new RelayCommand(ImportGame);
            LoadGameCommand = new RelayCommand(LoadGame);
        }
        public void NewGame()
        {
            ViewModelLocator.GameLoader.CreateGame();
            Task.Factory.StartNew(LoadMainWindow);

        }
        public void LoadGame()
        {
            var fo = new OpenFileDialog
            {
                Title = "Load a Game (select the definition.xml file within the root folder.)",
                Filter = "Definition File|definition.xml"
            };
            if ((bool)fo.ShowDialog() == false)
            {
                return;
            }

            var fileInfo = new FileInfo(fo.FileName);

            ViewModelLocator.GameLoader.LoadGame(fileInfo);
            Task.Factory.StartNew(LoadMainWindow);


        }
        public void ImportGame()
        {
            var fo = new OpenFileDialog
            {
                Title = "Import a Game Package",
                Filter = "OCTGN Game Package (*.nupkg)|*.nupkg"
            };
            if (fo.ShowDialog() == false) return;

            var fileInfo = new FileInfo(fo.FileName);

            ViewModelLocator.GameLoader.ImportGame(fileInfo);
            Task.Factory.StartNew(LoadMainWindow);


        }

        public string Title
        {
            get
            {
                return "OCTGN Game Development Studio";
            }
        }

        public Version Version
        {
            get
            {
                return typeof(LoaderViewModel).Assembly.GetName().Version;
            }
        }

        private void LoadMainWindow()
        {
            Messenger.Default.Send(new WindowActionMessage<MainViewModel>(WindowActionType.Create));
            Messenger.Default.Send(new WindowActionMessage<MainViewModel>(WindowActionType.Show));
            Messenger.Default.Send(new WindowActionMessage<MainViewModel>(WindowActionType.SetMain));
            Messenger.Default.Send(new WindowActionMessage<LoaderViewModel>(WindowActionType.Close));
        }
    }
}