using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Octgn.Controls
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;

    using Octgn.Extentions;
    using Octgn.ViewModels;

    /// <summary>
    /// Interaction logic for GameSelector2.xaml
    /// </summary>
    public partial class GameSelector2 : UserControl
    {
        public ObservableCollection<Data.Game> Games { get; private set; }
        
        public GameSelector2()
        {
            InitializeComponent();
            if (this.IsInDesignMode()) return;

            Games = Program.GamesRepository.Games;
            Program.GamesRepository.GameInstalled += (sender, args) => Games = Program.GamesRepository.Games;
            Games.CollectionChanged += GamesOnCollectionChanged;
            this.Loaded += OnLoaded;
            this.IsVisibleChanged += OnIsVisibleChanged;
            this.LayoutUpdated += OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            //this.RefreshGameList();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            //this.RefreshGameList();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.RefreshGameList();
        }

        private void RefreshGameList()
        {
            StackPanelGames.Items.Clear();
            foreach (var g in Games)
            {
                var model = new DataGameViewModel(g);
                var cont = new GameSelectorGame();
                cont.Model = model;
                cont.Height = StackPanelGames.ActualHeight;
                StackPanelGames.Items.Add(cont);
            }
        }

        private void GamesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshGameList();
        }
    }
}
