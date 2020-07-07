using System;
using System.Windows;

namespace Octgn.Desktop.Interfaces.Easy
{
    public partial class MainWindow : Window
    {
        private readonly NavigationService _navigationService;

        [Obsolete("For designer only")]
        public MainWindow() {
            InitializeComponent();
        }

        public MainWindow(NavigationService navigationService) {
            InitializeComponent();

            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            _navigationService.Navigate += NavigationService_Navigate;
        }

        private void NavigationService_Navigate(object sender, NavigateEventArgs e) {
            Dispatcher.InvokeAsync(() => {
                this.WindowBorder.Child = e.Destination;
            });

            e.IsHandled = true;
        }
    }
}
