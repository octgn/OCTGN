// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octide.Messages;
using System.Windows;

namespace Octide.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public RelayCommand LoadCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand ExportCommand { get; private set; }

        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
            }
            ExportCommand = new RelayCommand(ExportPackage);
            SaveCommand = new RelayCommand(SaveGame);
            LoadCommand = new RelayCommand(LoadLoaderWindow);
        }

        private void SaveGame()
        {
            ViewModelLocator.GameLoader.SaveGame();
        }

        private void ExportPackage()
        {
            if (AskToSave())
            {
                ViewModelLocator.GameLoader.ExportGame();
            }

        }

        private void LoadLoaderWindow()
        {
            //TODO: Make this work -- app doesn't like showing previously-closed windows.
            if (CleanupCurrentGame())
            {
                Messenger.Default.Send(new WindowActionMessage<LoaderViewModel>(WindowActionType.Create));
                Messenger.Default.Send(new WindowActionMessage<LoaderViewModel>(WindowActionType.Show));
                Messenger.Default.Send(new WindowActionMessage<LoaderViewModel>(WindowActionType.SetMain));
                Messenger.Default.Send(new WindowActionMessage<MainViewModel>(WindowActionType.Close));
                ViewModelLocator.RebindViewModelLocator();
            }
        }

        public bool AskToSave()
        {
            bool ret = true;
            if (ViewModelLocator.GameLoader.NeedsSave)
            {
                switch (MessageBox.Show("You have unsaved changes. Would you like to save first?", "Save Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Cancel:
                        ret = false;
                        break;
                    case MessageBoxResult.Yes:
                        ViewModelLocator.GameLoader.SaveGame();
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
            return ret;
        }
        public bool CleanupCurrentGame()
        {
            if (!AskToSave())
            {
                return false;
            }
            return true;
        }
    }
}