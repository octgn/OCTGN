using System;

namespace Octgn.Desktop.Interfaces.Easy
{
    public partial class LoadingScreen : Screen
    {
        [Obsolete("For designer only")]
        public LoadingScreen() {
            Title = "Loading...";

            InitializeComponent();
        }
    }
}
