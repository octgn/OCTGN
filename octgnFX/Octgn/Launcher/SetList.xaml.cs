using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Octgn.Data;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for GameList.xaml
    /// </summary>
    public partial class SetList : Page
    {
        public Data.Game SelectedGame;

        public SetList(Data.Game selectedGame)
        {
            InitializeComponent();
            SelectedGame = selectedGame;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Refresh_List();
        }

        public void Refresh_List()
        {
            lbSetList.Items.Clear();
            foreach (Set s in SelectedGame.Sets)
            {
                lbSetList.Items.Add(s);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        public void Deleted_Selected()
        {
            foreach (Set s in lbSetList.SelectedItems)
            {
                SelectedGame.DeleteSet(s);
            }
            Refresh_List();
        }

        public void Install_Sets()
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
                                                         string copyto = Path.Combine(path, shortName);
                                                         if (setName.ToLower() != copyto.ToLower())
                                                             File.Copy(setName, copyto, true);

                                                         SelectedGame.InstallSet(copyto);
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
            Refresh_List();
        }

        public void Patch_Selected()
        {
            new PatchDialog {Owner = Program.ClientWindow}.ShowDialog();
            Refresh_List();
        }
    }
}