using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;

namespace Octgn.Scripting.Controls
{
    public partial class PackDlg
    {
        public IEnumerable<Set> Sets { get; set; }

        public PackDlg()
        {
            Sets = Program.GameEngine.Definition.Sets().Where(x => x.Packs.Count() > 0).OrderBy(x => x.Name).ToArray();
            InitializeComponent();
            Owner = WindowManager.PlayWindow;
            setsCombo.SelectionChanged += setsCombo_SelectionChanged;
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

       private void StartClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (packsCombo.SelectedItem != null)
                DialogResult = true;
        }

        public Pack GetPack()
        {
            ShowDialog();
            if (packsCombo.SelectedItem == null) return null;
            return (Pack)packsCombo.SelectedItem;
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        private void setsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            packsCombo.SelectedIndex = 0;
        }

    }
}