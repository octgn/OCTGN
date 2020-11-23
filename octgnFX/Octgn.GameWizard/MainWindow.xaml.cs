using Octgn.GameWizard.Controls;
using Octgn.GameWizard.Models;
using Octgn.GameWizard.Pages;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Octgn.GameWizard
{
    public partial class MainWindow : Window
    {
        public WizardPage Page {
            get { return (WizardPage)GetValue(PageProperty); }
            set { SetValue(PageProperty, value); }
        }

        public static readonly DependencyProperty PageProperty =
            DependencyProperty.Register(nameof(Page), typeof(WizardPage), typeof(MainWindow), new PropertyMetadata(null));

        public ObservableCollection<WizardPage> Pages { get; }

        public bool ForwardEnabled {
            get { return (bool)GetValue(ForwardEnabledProperty); }
            set { SetValue(ForwardEnabledProperty, value); }
        }

        public static readonly DependencyProperty ForwardEnabledProperty =
            DependencyProperty.Register(nameof(ForwardEnabled), typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public bool BackEnabled {
            get { return (bool)GetValue(BackEnabledProperty); }
            set { SetValue(BackEnabledProperty, value); }
        }

        public static readonly DependencyProperty BackEnabledProperty =
            DependencyProperty.Register(nameof(BackEnabled), typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public int CurrentPageIndex {
            get {
                if (Page == null) throw new InvalidOperationException("Page is null");

                var currentPageIndex = Pages.IndexOf(Page);

                if (currentPageIndex < 0) throw new InvalidOperationException($"Page {Page} is not in {nameof(Pages)}");

                return currentPageIndex;
            }
        }

        public MainWindow()
        {
            Pages = new ObservableCollection<WizardPage>();

            InitializeComponent();

            Pages.Add(new WelcomePage());
            Pages.Add(new BasicInfoPage());
            Pages.Add(new TableSidesPage());

            var newgame = new NewGame();
            foreach (var page in Pages) {
                page.Game = newgame;
                page.DataContext = newgame;
            }

            Page = Pages[0];

            UpdateForwardBack();

            Pages.CollectionChanged += Pages_CollectionChanged;
        }

        private void Pages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateForwardBack();
        }

        private void UpdateForwardBack()
        {
            var previousPageIndex = CurrentPageIndex - 1;
            var nextPageIndex = CurrentPageIndex + 1;

            BackEnabled = previousPageIndex >= 0;
            ForwardEnabled = nextPageIndex < Pages.Count;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var nextPageIndex = CurrentPageIndex - 1;

            if (nextPageIndex < 0) throw new InvalidOperationException($"Can't go back, we're on the first page already.");

            Page.OnLeavingPage();

            Page = Pages[nextPageIndex];

            Page.OnEnteringPage();

            UpdateForwardBack();
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            var nextPageIndex = CurrentPageIndex + 1;

            if (nextPageIndex >= Pages.Count) throw new InvalidOperationException($"Can't go forward, we're on the last page already.");

            Page.OnLeavingPage();

            Page = Pages[nextPageIndex];

            Page.OnEnteringPage();

            UpdateForwardBack();
        }
    }
}
