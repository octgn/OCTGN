using System;

namespace Octgn.Desktop.Interfaces.Easy
{
    public partial class LoginScreen : Screen
    {
        private readonly Screen _returnScreen;

        [Obsolete("For designer only")]
        public LoginScreen() {
            InitializeComponent();
        }

        public LoginScreen(Screen returnScreen) {
            InitializeComponent();
            _returnScreen = returnScreen ?? throw new ArgumentNullException(nameof(returnScreen));
        }

        private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e) {
            NavigationService.NavigateTo(_returnScreen);
        }

        private void Login_Click(object sender, System.Windows.RoutedEventArgs e) {
            // TODO: Login
        }
    }
}
