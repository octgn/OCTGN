namespace Octgn.Windows
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms;

    using Octgn.Library.Exceptions;

    public partial class Options 
    {
        public Options()
        {
            InitializeComponent();
            TextBoxDataDirectory.Text = Prefs.DataDirectory;
            CheckBoxInstallOnBoot.IsChecked = Prefs.InstallOnBoot;
            CheckBoxLightChat.IsChecked = Prefs.UseLightChat;
            CheckBoxUseHardwareRendering.IsChecked = Prefs.UseHardwareRendering;
            CheckBoxUseWindowTransparency.IsChecked = Prefs.UseWindowTransparency;
            CheckBoxEnableChatImages.IsChecked = Prefs.EnableChatImages;
            //CheckBoxEnableChatGifs.IsChecked = Prefs.EnableChatGifs;
            CheckBoxEnableWhisperSound.IsChecked = Prefs.EnableWhisperSound;
            CheckBoxEnableNameSound.IsChecked = Prefs.EnableNameSound;
            MaxChatHistory.Value = Prefs.MaxChatHistory;
            this.MinMaxButtonVisibility = Visibility.Collapsed;
            this.MinimizeButtonVisibility = Visibility.Collapsed;
            this.CanResize = false;
            this.ResizeMode = ResizeMode.CanMinimize;
        }

        void SetError(string error = "")
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    if (string.IsNullOrWhiteSpace(error))
                    {
                        LabelError.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        LabelError.Visibility = Visibility.Visible;
                        LabelError.Text = error;
                    }
                }));
        }

        void ValidateFields(ref string dataDirectory, bool installOnBoot, 
            bool useLightChat, bool useHardwareRendering, 
            bool useTransparentWindows, int maxChatHistory,
            bool enableChatImages, bool enableWhisperSound,
            bool enableNameSound)
        {
            try
            {
                var dir = new DirectoryInfo(dataDirectory);
                if(!dir.Exists)Directory.CreateDirectory(dataDirectory);
                dataDirectory = dir.FullName;
            }
            catch (Exception)
            {
                throw new UserMessageException("The data directory value is invalid");
            }
            if (maxChatHistory < 50) throw new UserMessageException("Max chat history can't be less than 50");

        }

        void SaveSettings()
        {
            SetError();
            if (MaxChatHistory.Value == null) MaxChatHistory.Value = 100;
            var dataDirectory = TextBoxDataDirectory.Text;
            var installOnBoot = CheckBoxInstallOnBoot.IsChecked ?? false;
            var useLightChat = CheckBoxLightChat.IsChecked ?? false;
            var useHardwareRendering = CheckBoxUseHardwareRendering.IsChecked ?? false;
            var useTransparentWindows = CheckBoxUseWindowTransparency.IsChecked ?? false;
            var maxChatHistory = MaxChatHistory.Value ?? 100;
            var enableChatImages = CheckBoxEnableChatImages.IsChecked ?? false;
            //var enableChatGifs = CheckBoxEnableChatGifs.IsChecked ?? false;
            var enableWhisperSound = CheckBoxEnableWhisperSound.IsChecked ?? false;
            var enableNameSound = CheckBoxEnableNameSound.IsChecked ?? false;
            var task = new Task(
                () => 
                    this.SaveSettingsTask(
                    ref dataDirectory, 
                    installOnBoot, 
                    useLightChat, 
                    useHardwareRendering, 
                    useTransparentWindows,
                    maxChatHistory,
                    enableChatImages,
                    enableWhisperSound,
                    enableNameSound));
            task.ContinueWith((t) =>
                                  {
                                      Dispatcher
                                          .Invoke(new Action(
                                              () => this.SaveSettingsComplete(t)));
                                  });
            task.Start();
        }

        void SaveSettingsTask(
            ref string dataDirectory, 
            bool installOnBoot, 
            bool useLightChat,
            bool useHardwareRendering, 
            bool useTransparentWindows,
            int maxChatHistory,
            bool enableChatImages,
            bool enableWhisperSound,
            bool enableNameSound)
        {
            this.ValidateFields(
                ref dataDirectory, 
                installOnBoot, 
                useLightChat, 
                useHardwareRendering, 
                useTransparentWindows,
                maxChatHistory,
                enableChatImages,
                enableWhisperSound,
                enableNameSound);

            Prefs.DataDirectory = dataDirectory;
            Prefs.InstallOnBoot = installOnBoot;
            Prefs.UseLightChat = useLightChat;
            Prefs.UseHardwareRendering = useHardwareRendering;
            Prefs.UseWindowTransparency = useTransparentWindows;
            Prefs.MaxChatHistory = maxChatHistory;
            Prefs.EnableChatImages = enableChatImages;
            Prefs.EnableWhisperSound = enableWhisperSound;
            Prefs.EnableNameSound = enableNameSound;
            //Prefs.EnableChatGifs = enableChatGifs;
        }

        void SaveSettingsComplete(Task task)
        {
            if (task.IsFaulted)
            {
                if (task.Exception != null)
                {
                    var ex = task.Exception.InnerExceptions.OfType<UserMessageException>().FirstOrDefault();
                    if (ex != null)
                    {
                        this.SetError(ex.Message);
                        return;
                    }
                }
                this.SetError("There was an error. Please exit OCTGN and try again.");
                return;
            }
            Program.FireOptionsChanged();
            this.Close();
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            this.SaveSettings();
        }

        private void ButtonPickDataDirectoryClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.MyDocuments;
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            TextBoxDataDirectory.Text = dialog.SelectedPath;
        }
    }
}
