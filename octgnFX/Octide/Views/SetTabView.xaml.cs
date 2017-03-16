using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Octide.ViewModel;
using System.Windows.Input;
using System.Text;
using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using Octgn;

namespace Octide.Views
{
    /// <summary>
    /// Interaction logic for SetTabView.xaml
    /// </summary>
    public partial class SetTabView : UserControl
    {
        public SetTabView()
        {
            InitializeComponent();
        }


        #region image drag-drop functions

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);       

        

        private void OnImageDrop(object sender, DragEventArgs args)
        {
            try
            {
                var vm = DataContext as SetTabViewModel;
                if (!IsDragDropping) return;
                IsDragDropping = false;
                var dropFiles = (string[])args.Data.GetData(DataFormats.FileDrop);

                var file = dropFiles[0];
                
                if (vm.SelectedAlt == null) return;

                vm.SelectedAlt.SaveImage(file);
                ReplaceIcon.Visibility = Visibility.Collapsed;
            }

            catch (Exception e)
            {
                Log.Warn("Could not replace image", e);
                Program.DoCrazyException(
                    e,
                    "Could not replace the image, something went terribly wrong...You might want to try restarting OCTGN and/or your computer.");
            }
        }

        private void OnImageDragEnter(object sender, DragEventArgs args)
        {
            try
            {
                if (!args.Data.GetDataPresent(DataFormats.FileDrop)) return;
                var dropFiles = (string[])args.Data.GetData(DataFormats.FileDrop);
                if (dropFiles.Length != 1) return;
                var file = dropFiles.First();
                var attr = File.GetAttributes(file);

                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory) return;

                using (var imageStream = File.OpenRead(file))
                using (var image = System.Drawing.Image.FromStream(imageStream))
                {
                    // Check to see if it's an image
                    Log.Debug(image.Height);
                }
                if (!File.Exists(file)) return;

                var vm = DataContext as SetTabViewModel;
                IsDragDropping = true;
                ReplaceIcon.Visibility = Visibility.Visible;
                args.Handled = true;
            }
            catch (Exception e)
            {
                Log.Warn("Drag error", e);
            }
        }

        private void OnImageDragLeave(object sender, DragEventArgs e)
        {
            var vm = DataContext as SetTabViewModel;
            ReplaceIcon.Visibility = Visibility.Collapsed;
            IsDragDropping = false;
        }

        private void OnImageGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.Copy)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.Hand);
            }
            else
                e.UseDefaultCursors = true;

            e.Handled = true;
        }

        private void DeleteClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var vm = DataContext as SetTabViewModel;
                
                if (vm.SelectedAlt == null) return;
                vm.SelectedAlt.DeleteImage();
            }
            catch (Exception ex)
            {
                Log.Warn("Could not delete card image", ex);
                Program.DoCrazyException(ex, "Could not delete the card image, something went terribly wrong...You might want to try restarting OCTGN and/or your computer.");
            }
        }

        public bool isDragDropping;

        public bool IsDragDropping
        {
            get
            {
                return isDragDropping;
            }
            set
            {
                if (value == isDragDropping) return;
                isDragDropping = value;
                OnPropertyChanged("IsDragDropping");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}