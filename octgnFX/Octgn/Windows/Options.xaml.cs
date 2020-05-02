/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Octgn.Core;
using Octgn.Library;
using Octgn.Library.Exceptions;

namespace Octgn.Windows
{
    public partial class Options
    {
        public Options() {
            InitializeComponent();

            TextBoxDataDirectory.Text = Config.Instance.DataDirectory;
            TextBoxImageDirectory.Text = Config.Instance.ImageDirectory;
            TextBoxWindowSkin.Text = Prefs.WindowSkin;
            CheckBoxTileWindowSkin.IsChecked = Prefs.TileWindowSkin;
            CheckBoxLightChat.IsChecked = Prefs.UseLightChat;
            CheckBoxUseHardwareRendering.IsChecked = Prefs.UseHardwareRendering;
            CheckBoxUseWindowTransparency.IsChecked = Prefs.UseWindowTransparency;
            foreach (ComboBoxItem item in TextBoxWindowBorderDecorator.Items) {
                if (string.Equals(Prefs.WindowBorderDecorator, (string)item.Tag, StringComparison.Ordinal)) {
                    item.IsSelected = true;
                }
            }
            CheckBoxIgnoreSSLCertificates.IsChecked = Prefs.IgnoreSSLCertificates;
            CheckBoxEnableChatImages.IsChecked = Prefs.EnableChatImages;
            CheckBoxEnableWhisperSound.IsChecked = Prefs.EnableWhisperSound;
            CheckBoxEnableNameSound.IsChecked = Prefs.EnableNameSound;
            CheckBoxUseWindowsForChat.IsChecked = Prefs.UseWindowsForChat;
            MaxChatHistory.Value = Prefs.MaxChatHistory;
            ChatFontSize.Value = Prefs.ChatFontSize;
            CheckBoxUseInstantSearch.IsChecked = Prefs.InstantSearch;
            CheckBoxEnableGameSounds.IsChecked = Prefs.EnableGameSound;
            ComboBoxZoomOptions.SelectedIndex = (int)Prefs.ZoomOption;
            ComboBoxJoinSound.SelectedIndex = (int)Prefs.SoundOption;
            CheckBoxEnableGameFonts.IsChecked = Prefs.UseGameFonts;
            CheckBoxShowLanMode.IsChecked = Prefs.EnableLanGames;
            ComboBoxCardMoveNotification.SelectedIndex = (int)Prefs.CardMoveNotification;
            CheckBoxUseTestReleases.IsChecked = File.Exists(Path.Combine(Config.Instance.Paths.ConfigDirectory, "TEST"));
            HandDensitySlider.Value = Prefs.HandDensity;

            this.MinMaxButtonVisibility = Visibility.Collapsed;
            this.MinimizeButtonVisibility = Visibility.Collapsed;

            this.CanResize = false;
            this.ResizeMode = ResizeMode.CanMinimize;

        }

        void SetError(string error = "") {
            Dispatcher.Invoke(new Action(() => {
                if (string.IsNullOrWhiteSpace(error)) {
                    LabelError.Visibility = Visibility.Collapsed;
                } else {
                    LabelError.Visibility = Visibility.Visible;
                    LabelError.Text = error;
                }
            }));
        }

        async Task SaveSettings() {
            SetError();

            var dataDirectory = TextBoxDataDirectory.Text;
            var imageDirectory = TextBoxImageDirectory.Text;
            var windowSkin = TextBoxWindowSkin.Text;
            var tileWindowSkin = CheckBoxTileWindowSkin.IsChecked ?? false;
            var useLightChat = CheckBoxLightChat.IsChecked ?? false;
            var useHardwareRendering = CheckBoxUseHardwareRendering.IsChecked ?? false;
            var useTransparentWindows = CheckBoxUseWindowTransparency.IsChecked ?? false;
            var windowBorderDecorator = (string)(TextBoxWindowBorderDecorator.SelectedItem as ComboBoxItem)?.Tag;
            var ignoreSSLCertificates = CheckBoxIgnoreSSLCertificates.IsChecked ?? false;
            var maxChatHistory = MaxChatHistory.Value ?? 100;
            var enableChatImages = CheckBoxEnableChatImages.IsChecked ?? false;
            var enableWhisperSound = CheckBoxEnableWhisperSound.IsChecked ?? false;
            var enableNameSound = CheckBoxEnableNameSound.IsChecked ?? false;
            var useWindowsForChat = CheckBoxUseWindowsForChat.IsChecked ?? false;
            var chatFontSize = ChatFontSize.Value ?? 12;
            var useInstantSearch = CheckBoxUseInstantSearch.IsChecked ?? false;
            var enableGameSounds = CheckBoxEnableGameSounds.IsChecked ?? false;
            var showLanMode = CheckBoxShowLanMode.IsChecked ?? false;
            var useGameFonts = CheckBoxEnableGameFonts.IsChecked ?? false;
            var handDensity = HandDensitySlider.Value;
            var useTestReleases = CheckBoxUseTestReleases.IsChecked ?? false;
            var zoomOption = (Prefs.ZoomType)ComboBoxZoomOptions.SelectedIndex;
            var soundOption = (Prefs.SoundType)ComboBoxJoinSound.SelectedIndex;
            var animOption = (Prefs.CardAnimType)ComboBoxCardMoveNotification.SelectedIndex;

            try {
                // ---- Validate settings
                await Task.Run(() => {
                    try {
                        Config.Instance.ValidatePath(dataDirectory);
                    } catch (Exception ex) {
                        throw new UserMessageException("The data directory value is invalid", ex);
                    }

                    try {
                        Config.Instance.ValidatePath(imageDirectory);
                    } catch (Exception ex) {
                        throw new UserMessageException("The image directory value is invalid", ex);
                    }

                    if (maxChatHistory < 50) throw new UserMessageException("Max chat history can't be less than 50");

                    if (!string.IsNullOrWhiteSpace(windowSkin)) {
                        if (!File.Exists(windowSkin)) throw new UserMessageException("Window skin file doesn't exist");
                    }
                }).ConfigureAwait(true);

                // ---- Save settings
                var changedDataDirectory = Config.Instance.DataDirectory != dataDirectory;

                await Task.Run(() => {
                    Config.Instance.DataDirectory = dataDirectory;
                    Config.Instance.ImageDirectory = imageDirectory;
                    Prefs.UseLightChat = useLightChat;
                    Prefs.UseHardwareRendering = useHardwareRendering;
                    Prefs.UseWindowTransparency = useTransparentWindows;
                    Prefs.IgnoreSSLCertificates = ignoreSSLCertificates;
                    Prefs.MaxChatHistory = maxChatHistory;
                    Prefs.EnableChatImages = enableChatImages;
                    Prefs.EnableWhisperSound = enableWhisperSound;
                    Prefs.EnableNameSound = enableNameSound;
                    Prefs.WindowSkin = windowSkin;
                    Prefs.WindowBorderDecorator = windowBorderDecorator;
                    Prefs.TileWindowSkin = tileWindowSkin;
                    Prefs.UseWindowsForChat = useWindowsForChat;
                    Prefs.ChatFontSize = chatFontSize;
                    Prefs.InstantSearch = useInstantSearch;
                    Prefs.EnableGameSound = enableGameSounds;
                    Prefs.SoundOption = soundOption;
                    Prefs.ZoomOption = zoomOption;
                    Prefs.CardMoveNotification = animOption;
                    Prefs.EnableLanGames = showLanMode;
                    Prefs.UseGameFonts = useGameFonts;
                    Prefs.HandDensity = handDensity;
                    if (useTestReleases && !File.Exists(Path.Combine(Config.Instance.Paths.ConfigDirectory, "TEST")))
                        File.Create(Path.Combine(Config.Instance.Paths.ConfigDirectory, "TEST"));
                    else if (!useTestReleases && File.Exists(Path.Combine(Config.Instance.Paths.ConfigDirectory, "TEST")))
                        File.Delete(Path.Combine(Config.Instance.Paths.ConfigDirectory, "TEST"));
                }).ConfigureAwait(true);

                Program.FireOptionsChanged();

                this.Close();
            } catch (UserMessageException ex) {
                SetError(ex.Message);
            } catch (Exception ex) {
                SetError("There was an unexpected error. Please exit OCTGN and try again.");

                Log.Error("Error saving options", ex);
            }
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private async void ButtonSaveClick(object sender, RoutedEventArgs e) {
            await SaveSettings().ConfigureAwait(true);
        }

        private void ButtonPickDataDirectoryClick(object sender, RoutedEventArgs e) {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = Config.Instance.DataDirectoryFull;

            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;

            TextBoxDataDirectory.Text = dialog.SelectedPath;
        }

        private void ButtonPickImageDirectoryClick(object sender, RoutedEventArgs e) {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = Config.Instance.ImageDirectoryFull;

            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;

            TextBoxImageDirectory.Text = dialog.SelectedPath;
        }

        private void ButtonPickWindowSkinClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.Filter = "All Images|*.BMP;*.JPG;*.JPEG;*.PNG|BMP Files: (*.BMP)|*.BMP|JPEG Files: (*.JPG;*.JPEG)|*.JPG;*.JPEG|PNG Files: (*.PNG)|*.PNG";
            dialog.CheckFileExists = true;

            var res = dialog.ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK) {
                TextBoxWindowSkin.Text = dialog.FileName;
            }
        }
    }
}
