using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Octgn.Data;

namespace Octgn.Play.Dialogs
{
    using Octgn.Core.DataExtensionMethods;
    using Octgn.Controls;

    public partial class LimitedDialog
    {
        public LimitedDialog()
        {
            Singleton = this;
            Packs = new ObservableCollection<SelectedPack>();
            Sets = Program.GameEngine.Definition.Sets().Where(x=>x.Packs.Count() > 0).OrderBy(x=>x.Name).ToArray();
            InitializeComponent();
            setsCombo.SelectionChanged += setsCombo_SelectionChanged;
        }

        public static LimitedDialog Singleton { get; private set; }

        public ObservableCollection<SelectedPack> Packs { get; set; }
        public IEnumerable<DataNew.Entities.Set> Sets { get; set; } 

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Singleton = null;
        }

        private void AddSetClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (packsCombo.SelectedItem == null) return;
            // I am creating lightweight "clones" of the pack, because the 
            // WPF ListBox doesn't like having multiple copies of the same 
            // instance and messes up selection
            var pack = (DataNew.Entities.Pack) packsCombo.SelectedItem;
            Packs.Add(new SelectedPack {Id = pack.Id, FullName = pack.GetFullName()});
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (Player.All.Any(p => p.Groups.Any(x => x.Count > 0)))
            {
                if (MessageBoxResult.Yes ==
                    TopMostMessageBox.Show(
                        "Some players have cards currently loaded.\n\nReset the game before starting limited game?",
                        "Warning", MessageBoxButton.YesNo))
                    Program.Client.Rpc.ResetReq();
            }
            if (addCards.Visibility == Visibility.Visible)
            {
                if (addCards.SelectedIndex == 1)
                    Program.Client.Rpc.AddPacksReq(Packs.Select(p => p.Id).ToArray(), false);
                else if (addCards.SelectedIndex == 0)
                    Program.Client.Rpc.AddPacksReq(Packs.Select(p => p.Id).ToArray(), true);
            }
            else Program.Client.Rpc.StartLimitedReq(Packs.Select(p => p.Id).ToArray());
            Close();
            // Solves an issue where Dialog isn't the active window anymore if the confirmation dialog above was shown
            //fix MAINWINDOW bug
            WindowManager.PlayWindow.Activate();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        private void RemoveClicked(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) Packs.Remove((SelectedPack) btn.DataContext);
        }

        private void setsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            packsCombo.SelectedIndex = 0;
        }
        public void showAddCardsCombo(bool vis)
        {
            if (vis) addCards.Visibility = Visibility.Visible;
            else addCards.Visibility = Visibility.Hidden;
        }

        #region Nested type: SelectedPack

        public class SelectedPack
        {
            public Guid Id { get; set; }
            public string FullName { get; set; }
        }

        #endregion
    }
}