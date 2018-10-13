using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Octgn.Installer.Bundle.UI.Pages
{
    public partial class ProgressPage : UserControl
    {
        public ProgressPage() {
            InitializeComponent();
        }
    }

    public class ProgressPageViewModel : PageViewModel
    {
        public string Status {
            get => _status;
            set => SetAndNotify(ref _status, value);
        }
        private string _status;

        public int Progress {
            get => _progress;
            set => SetAndNotify(ref _progress, value);
        }
        private int _progress;

        public ProgressPageViewModel() {
            this.Page = new ProgressPage() {
                DataContext = this
            };

            Button1Text = "Cancel";
        }
    }
}
