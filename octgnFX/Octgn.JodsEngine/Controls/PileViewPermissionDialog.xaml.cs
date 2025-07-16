using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Octgn.Play;

namespace Octgn.Controls
{    public partial class PileViewPermissionDialog : UserControl
    {
        public bool IsGranted { get; private set; }
        public bool IsPermanent { get; private set; }
        public bool DialogResult { get; private set; }
        public Action<bool, bool, bool> OnResult { get; set; } // granted, permanent, dialogResult

        public PileViewPermissionDialog(Group pile, Player requester, string viewDescription = null)
        {
            InitializeComponent();
            
            // Set the message with player name and specific view details
            if (!string.IsNullOrEmpty(viewDescription))
            {
                RequestMessageText.Text = $"{requester.Name} is requesting to view {viewDescription} in your {pile.Name}";
            }
            else
            {
                RequestMessageText.Text = $"{requester.Name} is requesting to view your {pile.Name}";
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (AlwaysRadio.IsChecked == true)
            {
                IsGranted = true;
                IsPermanent = true;
            }
            else if (YesRadio.IsChecked == true)
            {
                IsGranted = true;
                IsPermanent = false;
            }
            else if (NoRadio.IsChecked == true)
            {
                IsGranted = false;
                IsPermanent = false;
            }
            else if (NeverRadio.IsChecked == true)
            {
                IsGranted = false;
                IsPermanent = true;
            }
            
            DialogResult = true;
            OnResult?.Invoke(IsGranted, IsPermanent, DialogResult);
            CloseDialog();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsGranted = false;
            IsPermanent = false;
            DialogResult = false;
            OnResult?.Invoke(IsGranted, IsPermanent, DialogResult);
            CloseDialog();
        }

        private void CloseDialog()
        {
            // Close the backstage dialog
            if (WindowManager.PlayWindow != null)
            {
                WindowManager.PlayWindow.HideBackstage();
            }
        }
    }
}