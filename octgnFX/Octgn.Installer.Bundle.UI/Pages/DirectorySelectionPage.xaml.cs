using Microsoft.WindowsAPICodePack.Dialogs;
using Octgn.Installer.Shared;
using System;
using System.Windows.Controls;

namespace Octgn.Installer.Bundle.UI.Pages
{
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

        public bool EnableSetInstallDirectory {
            get => _enableSetInstallDirectory;
            set => SetAndNotify(ref _enableSetInstallDirectory, value);
        }
        private bool _enableSetInstallDirectory;

        public DirectorySelectionPageViewModel() {
            switch (App.Current.RunMode) {
                case RunMode.Install:
                    Button1Text = "Install";
                    EnableSetInstallDirectory = true;
                    break;
                case RunMode.Modify:
                    Button1Text = "Modify";
                    EnableSetInstallDirectory = false;
                    break;
                case RunMode.UninstallOrModify:
                case RunMode.Uninstall:
                    throw new NotImplementedException($"RunMode {App.Current.RunMode} is unsupported with this page.");
            }

            Button1Text = "Install";

            var installedOctgn = InstalledOctgn.Get();

            if (installedOctgn.IsInstalled) {
                InstallDirectory = installedOctgn.InstalledDirectory.FullName;
            } else {
                InstallDirectory = Paths.Get.DefaultInstallPath;
            }

            if (installedOctgn.DataDirectory != null) {
                DataDirectory = installedOctgn.DataDirectory.FullName;
            } else {
                DataDirectory = Paths.Get.DefaultDataDirectory;
            }

            Page = new DirectorySelectionPage() {
                DataContext = this
            };
        }

        public override void Button1_Action() {
            base.Button1_Action();

            App.Current.Engine.StringVariables["INSTALLDIR"] = InstallDirectory;
            App.Current.Engine.StringVariables["DATADIRECTORY"] = DataDirectory;

            DoTransition(new ProgressPageViewModel());

            switch (App.Current.RunMode) {
                case RunMode.Install:
                    App.Current.StartInstall();
                    break;
                case RunMode.Modify:
                    App.Current.StartModify();
                    break;
                case RunMode.Uninstall:
                case RunMode.UninstallOrModify:
                default:
                    throw new NotImplementedException($"RunMode {App.Current.RunMode} is unsupported with this page.");
            }
        }
    }
}
