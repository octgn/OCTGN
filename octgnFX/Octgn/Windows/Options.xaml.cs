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

        void ValidateFields(ref string dataDirectory, bool installOnBoot, bool useLightChat)
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

        }

        void SaveSettings()
        {
            SetError();
            var dataDirectory = TextBoxDataDirectory.Text;
            var installOnBoot = CheckBoxInstallOnBoot.IsChecked ?? false;
            var useLightChat = CheckBoxLightChat.IsChecked ?? false;
            var task = new Task(()=>this.SaveSettingsTask(ref dataDirectory,installOnBoot,useLightChat));
            task.ContinueWith((t) => { Dispatcher.Invoke(new Action(() => this.SaveSettingsComplete(t))); });
            task.Start();
        }

        void SaveSettingsTask(ref string dataDirectory, bool installOnBoot, bool useLightChat)
        {
            this.ValidateFields(ref dataDirectory,installOnBoot,useLightChat);
            Prefs.DataDirectory = dataDirectory;
            Prefs.InstallOnBoot = installOnBoot;
            Prefs.UseLightChat = useLightChat;
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
