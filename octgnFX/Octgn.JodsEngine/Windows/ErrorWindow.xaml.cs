using log4net;
using Octgn.Launchers;
using Octgn.Library.Exceptions;
using System;
using System.Reflection;
using System.Windows;

namespace Octgn.Windows
{
    public partial class ErrorWindow
    {
        private readonly static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Obsolete("Use only for designer")]
        public ErrorWindow() {
            InitializeComponent();
        }

        public ErrorWindow(UserMessageException ex) {
            InitializeComponent();

            if (ex.InnerException == null) {
                detailsBox.Text = ex.ToString();
            } else {
                detailsBox.Text = ex.InnerException.ToString();
            }

            detailsBox.Text = ex.ToString();

            Error_Message.Inlines.Clear();
            Error_Message.Inlines.Add(ex.Message);

            AddConsiquence();
        }

        public ErrorWindow(string message, Exception ex) {
            InitializeComponent();

            detailsBox.Text = ex.ToString();

            Error_Message.Inlines.Clear();
            Error_Message.Inlines.Add(message);

            AddConsiquence();
        }

        private void AddConsiquence() {
            Error_Consiquence.Inlines.Clear();
            if (Program.Launcher is DeckEditorLauncher) {
                Error_Consiquence.Inlines.Add("Unfortunately the Deck Editor will have to be shut down.");
            } else {
                if(Program.GameEngine != null && Program.GameEngine.Definition != null) {
                    if (Program.GameEngine.IsReplay) {
                        Error_Consiquence.Inlines.Add($"Unfortunately the {Program.GameEngine.Definition.Name} replay will have to be shut down.");
                    } else {
                        Error_Consiquence.Inlines.Add($"Unfortunately {Program.GameEngine.Definition.Name} will have to be shut down.");
                    }
                } else {
                    Error_Consiquence.Inlines.Add($"Unfortunately the current game will have to be shut down.");
                }
            }
        }

        private void CopyDetails(object sender, RoutedEventArgs e) {
            e.Handled = true;

            try {
                Clipboard.SetText(detailsBox.Text);
            } catch (Exception ex) {
                Log.Warn($"Clipboard error. This is not a cause for concern usually: {ex}");
            }
        }

        private void CloseClicked(object sender, RoutedEventArgs e) {
            try {
                DialogResult = true;
            } catch {
            }

            try {
                this.Close();
            } catch {
            }
        }
    }
}