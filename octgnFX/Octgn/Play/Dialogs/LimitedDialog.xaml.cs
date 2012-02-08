﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Octgn.Data;

namespace Octgn.Play.Dialogs
{
    public partial class LimitedDialog
    {
        public LimitedDialog()
        {
            Singleton = this;
            Packs = new ObservableCollection<SelectedPack>();
            Sets = Database.GetAllSets();
            InitializeComponent();
        }

        public static LimitedDialog Singleton { get; private set; }

        public ObservableCollection<SelectedPack> Packs { get; set; }
        public IEnumerable<Set> Sets { get; set; }

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
            var pack = (Pack) packsCombo.SelectedItem;
            Packs.Add(new SelectedPack {Id = pack.Id, FullName = pack.FullName});
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (Player.All.Any(p => p.Groups.Any(x => x.Count > 0)))
            {
                if (MessageBoxResult.Yes ==
                    MessageBox.Show(
                        "Some players have cards currently loaded.\n\nReset the game before starting limited game?",
                        "Warning", MessageBoxButton.YesNo))
                    Program.Client.Rpc.ResetReq();
            }

            Program.Client.Rpc.StartLimitedReq(Packs.Select(p => p.Id).ToArray());
            Close();
            // Solves an issue where OCTGN isn't the active window anymore if the confirmation dialog above was shown
            //fix MAINWINDOW bug
            Program.PlayWindow.Activate();
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

        #region Nested type: SelectedPack

        public class SelectedPack
        {
            public Guid Id { get; set; }
            public string FullName { get; set; }
        }

        #endregion
    }
}