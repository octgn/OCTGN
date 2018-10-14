using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows.Controls;

namespace Octgn.Installer.Bundle.UI.Pages
{
    /// <summary>
    /// Interaction logic for DirectorySelectionPage.xaml
    /// </summary>
    public partial class DirectorySelectionPage : UserControl
    {
        public DirectorySelectionPage() {
            InitializeComponent();
        }

        private void InstallDirectoryBrowse_Click(object sender, System.Windows.RoutedEventArgs e) {
            var vm = (DirectorySelectionPageViewModel)DataContext;

            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Select Install Directory";
            dialog.EnsurePathExists = true;
            dialog.DefaultDirectory = vm.InstallDirectory;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                vm.InstallDirectory = dialog.FileName;
            }
        }

        private void DataDirectoryBrowse_Click(object sender, System.Windows.RoutedEventArgs e) {
            var vm = (DirectorySelectionPageViewModel)DataContext;

            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Select Data Directory";
            dialog.EnsurePathExists = true;
            dialog.DefaultDirectory = vm.DataDirectory;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                vm.DataDirectory = dialog.FileName;
            }
        }
    }

    public class DirectorySelectionPageViewModel : PageViewModel
    {
        public string InstallDirectory {
            get => _installDirectory;
            set => SetAndNotify(ref _installDirectory, value);
        }
        private string _installDirectory;

        public string DataDirectory {
            get => _dataDirectory;
            set => SetAndNotify(ref _dataDirectory, value);
        }
        private string _dataDirectory;

        public DirectorySelectionPageViewModel() {
            Button1Text = "Install";

            string dataDirectory = null;
            using (var subKey = Registry.CurrentUser.OpenSubKey(@"Software\OCTGN")) {
                if (subKey != null) {
                    dataDirectory = (string)subKey.GetValue(@"DataDirectory");
                }
            }

            if (dataDirectory != null) {
                DataDirectory = dataDirectory;
            } else {
                DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OCTGN");
            }

            string installDirectory = null;
            using (var subKey = Registry.CurrentUser.OpenSubKey(@"Software\OCTGN")) {
                if (subKey != null) {
                    installDirectory = (string)subKey.GetValue(@"InstallDirectory");
                }
            }

            if (installDirectory != null) {
                InstallDirectory = installDirectory;
            } else {
                InstallDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "OCTGN");
            }

            Page = new DirectorySelectionPage() {
                DataContext = this
            };
        }

        public override void Button1_Action() {
            base.Button1_Action();

            DoTransition(new ProgressPageViewModel());

            App.Current.StartInstall();
        }
    }
}
