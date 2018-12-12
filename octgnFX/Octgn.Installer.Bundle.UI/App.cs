using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Octgn.Installer.Bundle.UI.Pages;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Octgn.Installer.Bundle.UI
{
    public class App : BootstrapperApplication
    {
        public static App Current { get; set; }

        public App() {
            if (Current != null) throw new InvalidOperationException("Already created App");

            Current = this;

            ApplyComplete += this.OnApplyComplete;
            PlanPackageBegin += this.App_PlanPackageBegin;
            PlanComplete += this.OnPlanComplete;
            DetectComplete += App_DetectComplete;
            Error += App_Error;
        }

        public bool IsCancelling { get; private set; }

        public RunMode RunMode { get; set; }

        public ActionResult Result { get; private set; }

        public Dispatcher Dispatcher { get; private set; }

        public string Version { get; private set; }

        public void StartInstall() {
            Engine.Plan(LaunchAction.Install);
        }

        public void StartUninstall() {
            Engine.Plan(LaunchAction.Uninstall);
        }

        public void StartModify() {
            Engine.Plan(LaunchAction.Modify);
        }

        public void Cancel() {
            Result = ActionResult.UserExit;

            IsCancelling = true;

            Dispatcher.InvokeShutdown();
        }

        private MainWindow _mainWindow;

        protected override void Run() {
            Dispatcher = Dispatcher.CurrentDispatcher;

            Engine.Detect();

            Version = Engine.StringVariables["BundleVersion"];

            _mainWindow = new MainWindow();
            _mainWindow.Show();

            switch (Command.Action) {
                case LaunchAction.Uninstall:
                    RunMode = RunMode.Uninstall;
                    break;
                case LaunchAction.Install:
                    if (Command.Resume == ResumeType.Arp) {
                        RunMode = RunMode.UninstallOrModify;
                    } else {
                        RunMode = RunMode.Install;
                    }
                    break;
                case LaunchAction.Modify:
                    RunMode = RunMode.Modify;
                    break;
                default:
                    throw new NotImplementedException();
            }

            Dispatcher.Run();

            _mainWindow.Close();

            this.Engine.Quit(0);
        }

        private void App_PlanPackageBegin(object sender, PlanPackageBeginEventArgs e) {
            if (e.PackageId == "MainPackage") {
                if (RunMode == RunMode.Modify) {
                    e.State = RequestState.Repair;
                }
            }
        }

        private void App_DetectComplete(object sender, DetectCompleteEventArgs e) {
            Dispatcher.BeginInvoke(new Action(() => {
                if (!WaitForOctgnToClose()) {
                    Result = ActionResult.UserExit;

                    Dispatcher.InvokeShutdown();

                    return;
                }

                var status = e.Status;
                switch (RunMode) {
                    case RunMode.Install:
                        _mainWindow.PageViewModel = new TermsPageViewModel();
                        break;
                    case RunMode.Uninstall:
                        _mainWindow.PageViewModel = new ProgressPageViewModel();
                        StartUninstall();
                        break;
                    case RunMode.Modify:
                        _mainWindow.PageViewModel = new DirectorySelectionPageViewModel();
                        break;
                    case RunMode.UninstallOrModify:
                        _mainWindow.PageViewModel = new UninstallOrModifyPageViewModel();
                        break;
                    default:
                        throw new NotImplementedException($"RunMode {RunMode} not implemented"); ;
                }
            }));
        }

        private void OnPlanComplete(object sender, PlanCompleteEventArgs e) {
            if (e.Status >= 0) {
                Engine.Apply(System.IntPtr.Zero);
            } else {
                Result = ActionResult.Failure;

                var error = new Win32Exception(e.Status).Message;
                var message = $"Error installing: {error}. Code: 0x{e.Status:x8}";

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Dispatcher.InvokeShutdown();
            }
        }

        private void OnApplyComplete(object sender, ApplyCompleteEventArgs e) {
            if (e.Status == 0) {
                Result = ActionResult.Success;
                Dispatcher.InvokeShutdown();
            } else {
                Result = ActionResult.Failure;
            }
        }

        private void App_Error(object sender, Microsoft.Tools.WindowsInstallerXml.Bootstrapper.ErrorEventArgs e) {
            MessageBox.Show(e.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Result = ActionResult.Failure;

            Dispatcher.InvokeShutdown();
        }

        public bool IsIncompatibleOctgnInstalled() {
            //TODO: This should be able to check the registry or something, the previous installer should have left some artifact we can use. This may be unneccisary though.
            var oldPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            oldPath = Path.Combine(oldPath, "Octgn", "OCTGN");

            if (Directory.Exists(oldPath)) {
                return true;
            } return false;
        }

        public bool WaitForOctgnToClose() {
            while (true) {
                if (!IsOctgnRunning()) {
                    return true;
                }

                var result = MessageBox.Show("OCTGN is running. Please close OCTGN before you continue.", "OCTGN is running", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                switch (result) {
                    case MessageBoxResult.Cancel:
                    case MessageBoxResult.No:
                        return false;
                }
            }
        }

        public bool IsOctgnRunning() {
            foreach (var clsProcess in Process.GetProcesses()) {
                if (clsProcess.ProcessName.Contains("OCTGN")) {
                    return true;
                }
            }
            return false;
        }
    }

    public enum ActionResult
    {
        NotExecuted	 = 0,
        Success = 1,
        UserExit = 2,
        Failure = 3
    }

    public enum RunMode
    {
        Install,
        Uninstall,
        Modify,
        UninstallOrModify
    }
}
