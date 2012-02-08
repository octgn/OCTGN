using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Octgn.Data;

namespace Octgn.Launcher
{
    public partial class PatchDialog
    {
        public static readonly DependencyProperty PatchFileNameProperty =
            DependencyProperty.Register("PatchFileName", typeof (string), typeof (PatchDialog));

        public static readonly DependencyProperty TargetFolderNameProperty =
            DependencyProperty.Register("TargetFolderName", typeof (string), typeof (PatchDialog));

        public static readonly DependencyProperty PatchInstalledGamesProperty =
            DependencyProperty.Register("PatchInstalledGames", typeof (bool), typeof (PatchDialog),
                                        new UIPropertyMetadata(true));

        public static readonly DependencyProperty PatchFolderProperty =
            DependencyProperty.Register("PatchFolder", typeof (bool), typeof (PatchDialog),
                                        new UIPropertyMetadata(false));

        public PatchDialog()
        {
            InitializeComponent();
        }

        public string PatchFileName
        {
            get { return (string) GetValue(PatchFileNameProperty); }
            set { SetValue(PatchFileNameProperty, value); }
        }

        public string TargetFolderName
        {
            get { return (string) GetValue(TargetFolderNameProperty); }
            set { SetValue(TargetFolderNameProperty, value); }
        }

        public bool PatchInstalledSets
        {
            get { return (bool) GetValue(PatchInstalledGamesProperty); }
            set { SetValue(PatchInstalledGamesProperty, value); }
        }

        public bool PatchFolder
        {
            get { return (bool) GetValue(PatchFolderProperty); }
            set { SetValue(PatchFolderProperty, value); }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PickPatchFile(object sender, MouseButtonEventArgs e)
        {
            var ofd = new OpenFileDialog {Filter = "Sets patch (*.o8p)|*.o8p"};
            if (ofd.ShowDialog() != true) return;
            PatchFileName = ofd.FileName;
        }

        private void PickTargetFolder(object sender, MouseButtonEventArgs e)
        {
            var ofd = new OpenFileDialog
                          {
                              FileName = "Filename will be ignored",
                              CheckPathExists = true,
                              CheckFileExists = false,
                              ValidateNames = false
                          };
            if (ofd.ShowDialog() != true) return;
            TargetFolderName = Path.GetDirectoryName(ofd.FileName);
            PatchFolder = true;
        }

        private void DropPatchFile(object sender, DragEventArgs e)
        {
            var droppedFiles = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (droppedFiles == null) return;
            string file = droppedFiles.FirstOrDefault(f => Path.GetExtension(f) == ".o8p");
            if (file != null)
                PatchFileName = file;
        }

        private void DropTargetFolder(object sender, DragEventArgs e)
        {
            var droppedFolders = e.Data.GetData(DataFormats.FileDrop) as string[];
            string folder = droppedFolders.FirstOrDefault(f => Directory.Exists(f));
            if (folder == null) return;
            TargetFolderName = folder;
            PatchFolder = true;
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            Close();
            if (PatchFileName == null || (!PatchInstalledSets && (!PatchFolder || TargetFolderName == null))) return;
            var patch = new Patch(PatchFileName);
            var dlg = new PatchProgressDialog {Owner = Owner};
            patch.Progress += dlg.UpdateProgress;

            // Capture variables to prevent a cross-thread call to dependency properties.
            bool patchInstalledSets = PatchInstalledSets;
            string targetFolder = PatchFolder ? TargetFolderName : null;
            ThreadPool.QueueUserWorkItem(
                _ => patch.Apply(Program.GamesRepository, patchInstalledSets, targetFolder));

            dlg.ShowDialog();
        }
    }
}