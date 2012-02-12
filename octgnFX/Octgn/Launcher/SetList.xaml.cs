﻿using System;
using System.IO;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using Octgn.Data;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for GameList.xaml
    /// </summary>
    public partial class SetList
    {
        public Data.Game SelectedGame;

        public SetList(Data.Game selectedGame)
        {
            InitializeComponent();
            SelectedGame = selectedGame;
        }

        public void PageLoaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        public void RefreshList()
        {
            lbSetList.Items.Clear();
            foreach (Set s in SelectedGame.Sets)
            {
                lbSetList.Items.Add(s);
            }
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
        }

        public void DeletedSelected()
        {
            foreach (Set s in lbSetList.SelectedItems)
            {
                SelectedGame.DeleteSet(s);
            }
            RefreshList();
        }

        public void InstallSets()
        {
            var ofd = new OpenFileDialog
                          {
                              Filter = "Cards set definition files (*.o8s)|*.o8s",
                              Multiselect = true
                          };
            if (ofd.ShowDialog() != true) return;


            //Move the definition file to a new location, so that the old one can be deleted
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn", "Sets");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var wnd = new InstallSetsProgressDialog {Owner = Program.ClientWindow};
            ThreadPool.QueueUserWorkItem(_ =>
                                             {
                                                 int current = 0, max = ofd.FileNames.Length;
                                                 wnd.UpdateProgress(current, max, null, false);
                                                 foreach (string setName in ofd.FileNames)
                                                 {
                                                     ++current;
                                                     string shortName = Path.GetFileName(setName);
                                                     try
                                                     {
                                                         if (shortName != null)
                                                         {
                                                             string copyto = Path.Combine(path, shortName);
                                                             if (setName.ToLower() != copyto.ToLower())
                                                                 File.Copy(setName, copyto, true);
                                                             SelectedGame.InstallSet(copyto);
                                                         }
                                                         wnd.UpdateProgress(current, max,
                                                                            string.Format("'{0}' installed.", shortName),
                                                                            false);
                                                     }
                                                     catch (Exception ex)
                                                     {
                                                         wnd.UpdateProgress(current, max,
                                                                            string.Format(
                                                                                "'{0}' an error occured during installation:",
                                                                                shortName), true);
                                                         wnd.UpdateProgress(current, max, ex.Message, true);
                                                     }
                                                 }
                                             });
            wnd.ShowDialog();
            RefreshList();
        }

        public void PatchSelected()
        {
            new PatchDialog {Owner = Program.ClientWindow}.ShowDialog();
            RefreshList();
        }
    }
}