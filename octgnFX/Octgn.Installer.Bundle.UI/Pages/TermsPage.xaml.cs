using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Octgn.Installer.Bundle.UI.Pages
{
    public partial class TermsPage : UserControl
    {
        public TermsPage() {
            InitializeComponent();
        }
    }

    public class TermsPageViewModel : PageViewModel
    {
        public TermsPageViewModel() {
            Button1Text = "I Accept";

            var stringPath = "pack://application:,,,/Octgn.Installer.Bundle.UI;Component/Resources/EULA.rtf";
            var eulaInfo = System.Windows.Application.GetResourceStream(new Uri(stringPath));

            Page = new TermsPage();
            Page.DataContext = this;

            (Page as TermsPage).TextBox.Selection.Load(eulaInfo.Stream, DataFormats.Rtf);
        }

        public override void Button1_Action() {
            if (App.Current.IsIncompatibleOctgnInstalled()) {
                DoTransition(new PreviousVersionPageViewModel());
            } else {
                DoTransition(new DirectorySelectionPageViewModel());
            }
        }
    }
}