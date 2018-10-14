using Octgn.Installer.Bundle.UI.Pages;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Octgn.Installer.Bundle.UI
{
    public partial class MainWindow : Window
    {
        private static readonly DependencyProperty PageViewModelProperty
            = DependencyProperty.RegisterAttached(nameof(PageViewModel), typeof(PageViewModel), typeof(MainWindow), new PropertyMetadata(OnPageViewModelChanged));

        private static void OnPageViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var window = (MainWindow)d;

            var oldVm = (PageViewModel)e.OldValue;
            var newVm = (PageViewModel)e.NewValue;

            window.OnPageChanged(oldVm, newVm);
        }

        private void OnPageChanged(PageViewModel oldPage, PageViewModel newPage) {
            if(oldPage != null) {
                oldPage.Transition -= Page_Transition;
            }

            newPage.Transition += Page_Transition;

            Page = newPage.Page;
        }

        private void Page_Transition(object sender, PageTransitionEventArgs e) {
            PageViewModel = e.Page;
        }

        public PageViewModel PageViewModel {
            get => (PageViewModel)GetValue(PageViewModelProperty);
            set => SetValue(PageViewModelProperty, value);
        }

        private static readonly DependencyProperty PageProperty
            = DependencyProperty.RegisterAttached(nameof(Page), typeof(UserControl), typeof(MainWindow));

        public UserControl Page {
            get => (UserControl)GetValue(PageProperty);
            set => SetValue(PageProperty, value);
        }


        public MainWindow() {
            InitializeComponent();
            PageViewModel = new LoadingPageViewModel();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            this.DragMove();
        }

        private void Button1_Click(object sender, RoutedEventArgs e) {
            PageViewModel.Button1_Action();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            App.Current.Cancel();
        }
    }
}
