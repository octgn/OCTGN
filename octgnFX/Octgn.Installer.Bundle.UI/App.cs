using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Octgn.Installer.Bundle.UI.Pages;
using System;
using System.ComponentModel;
using System.Diagnostics;
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
            DetectPackageComplete += this.OnDetectPackageComplete;
            PlanComplete += this.OnPlanComplete;
            DetectComplete += App_DetectComplete;
            Error += App_Error;
        }

        public bool IsInstall { get; private set; }

        public void StartInstall() {
            Engine.Plan(LaunchAction.Install);
        }

        public void StartUninstall() {
            Engine.Plan(LaunchAction.Uninstall);
        }

        public bool IsCancelling { get; private set; }

        public void Cancel() {
            Result = ActionResult.UserExit;

            IsCancelling = true;

            Dispatcher.InvokeShutdown();
        }

        private MainWindow _mainWindow;

        public Dispatcher Dispatcher { get; private set; }

        public ActionResult Result { get; set; }

        protected override void Run() {
            Dispatcher = Dispatcher.CurrentDispatcher;

            Engine.Detect();

            _mainWindow = new MainWindow();
            _mainWindow.Show();

            Dispatcher.Run();

            _mainWindow.Close();

            this.Engine.Quit(0);
        }

        private void OnDetectPackageComplete(object sender, DetectPackageCompleteEventArgs e) {
            if (e.PackageId == "MainPackage") {
                if (e.State == PackageState.Absent) {
                    IsInstall = true;
                } else if (e.State == PackageState.Present) {
                    IsInstall = false;
                }
            }
        }

        private void App_DetectComplete(object sender, DetectCompleteEventArgs e) {
            Dispatcher.BeginInvoke(new Action(() => {
                if (IsInstall) {
                    _mainWindow.PageViewModel = new TermsPageViewModel();
                } else {
                    _mainWindow.PageViewModel = new ProgressPageViewModel();
                    StartUninstall();
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

        private void App_Error(object sender, ErrorEventArgs e) {
            MessageBox.Show(e.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Result = ActionResult.Failure;

            Dispatcher.InvokeShutdown();
        }
    }

    public enum ActionResult
    {
        NotExecuted	 = 0,
        Success = 1,
        UserExit = 2,
        Failure = 3
    }
}
