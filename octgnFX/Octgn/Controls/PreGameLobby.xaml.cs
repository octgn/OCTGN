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
    using DriveSync;

    /// <summary>
    /// Interaction logic for PreGameLobby.xaml
    /// </summary>
    public partial class PreGameLobby :SliderPage
    {
        private bool _startingGame;
        public PreGameLobby()
        {
            InitializeComponent();
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {
            //if (_startingGame) return;
            //_startingGame = true;
            //Program.LobbyClient.HostedGameStarted();
            //e.Handled = true;
            //Start();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Back();
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (cbTwoSided.IsChecked != null) Program.GameSettings.UseTwoSidedTable = cbTwoSided.IsChecked.Value;
        }

        private void Back()
        {
            this.NavigateBack();
        }
    }
}
