/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using log4net;
using Microsoft.Win32;
using Octgn.Annotations;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew;
using Octgn.DataNew.Entities;
using Octgn.Library;

namespace Octgn.DeckBuilder
{
    public partial class SleeveManagerView : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool isLoading;

        private SleeveViewModel _selectedSleeve;

        public SleeveViewModel SelectedSleeve
        {
            get { return _selectedSleeve; }
            set
            {
                if (_selectedSleeve == value) return;
                _selectedSleeve = value;
                OnPropertyChanged("SelectedSleeve");
            }
        }

        public ObservableCollection<SleeveViewModel> Sleeves { get; set; }

        public event Action<Sleeve> OnClose;

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }
            set
            {
                if (value.Equals(this.isLoading))
                {
                    return;
                }
                this.isLoading = value;
                this.OnPropertyChanged("IsLoading");
            }
        }

        public SleeveManagerView()
        {
            Sleeves = new ObservableCollection<SleeveViewModel>();
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Loaded -= OnLoaded;
            RefreshListAsync();
        }

        public void RefreshListAsync()
        {
            Task.Factory.StartNew(RefreshList);
        }

        public void RefreshList()
        {
            IsLoading = true;
            try
            {
                var sleeves = Sleeve.GetSleeves().ToArray();

                SetList(sleeves);
            }
            catch (Exception e)
            {
                Log.Error("RefreshList", e);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void SetList(IEnumerable<Sleeve> sleeves)
        {
            var vms = sleeves
                .OrderBy(sleeve => sleeve.Source)
                .ThenBy(sleeve => sleeve.Name)
                .Select(sleeve => new SleeveViewModel(sleeve))
                .ToArray();

            Dispatcher.Invoke(new Action(() =>
            {
                Sleeves.Clear();

                foreach (var sleeve in vms)
                {
                    sleeve.LoadImage();

                    Sleeves.Add(sleeve);
                }
            }));
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void SelectSleeve(object sender, RoutedEventArgs e)
        {
            var vm = (sender as System.Windows.Controls.Button).DataContext as SleeveViewModel;

            if (vm == null)
                return;

            FireOnClose(vm.Sleeve);
            this.Hide();
        }

        private void ButtonRefreshClick(object sender, RoutedEventArgs e)
        {
            this.RefreshListAsync();
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            FireOnClose(null);
            this.Hide();
        }

        private async void DeleteSleeve(object sender, RoutedEventArgs e) {
            var vm = (sender as System.Windows.Controls.Button).DataContext as SleeveViewModel;

            if (vm == null)
                return;

            var result = MessageBox.Show("Are you sure you want to delete this sleeve?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes) {
                await vm.DeleteAsync();

                RefreshListAsync();
            }
        }

        private void ButtonAddClick(object sender, RoutedEventArgs e) {
            var fo = new OpenFileDialog {
                Filter = "All Images|*.BMP;*.JPG;*.JPEG;*.PNG|BMP Files: (*.BMP)|*.BMP|JPEG Files: (*.JPG;*.JPEG)|*.JPG;*.JPEG|PNG Files: (*.PNG)|*.PNG"
            };
            if ((bool)fo.ShowDialog()) {
                if (File.Exists(fo.FileName)) {
                    var newPath = Path.Combine(Config.Instance.Paths.SleevePath, Path.GetFileName(fo.FileName));

                    if (File.Exists(newPath)) {
                        var result = MessageBox.Show("Sleeve already exists. Do you want to overwrite it?", "File Exists", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result != MessageBoxResult.Yes) return;
                    }

                    var dir = Path.GetDirectoryName(newPath);

                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.Copy(fo.FileName, newPath);

                    RefreshListAsync();
                }
            }
        }

        protected virtual void FireOnClose(Sleeve sleeve) => OnClose?.Invoke(sleeve);

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SleeveViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string Name { get; set; }

        private bool _isBusy;

        public BitmapImage Image { get; set; }

        public bool CanDelete => Sleeve.Source == SleeveSource.User && !IsBusy;

        public bool CanSelect => !IsBusy;


        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
                RaisePropertyChanged(() => CanDelete);
                RaisePropertyChanged(() => CanSelect);
            }
        }

        public Sleeve Sleeve { get; set; }

        public SleeveViewModel(Sleeve sleeve)
        {
            this.Name = sleeve.Name;

            Sleeve = sleeve;
        }

        public void LoadImage() {
            Image = Sleeve.GetImage();
        }

        public Task DeleteAsync() {
            return Task.Run(new Action(Delete));
        }

        public void Delete() {
            IsBusy = true;
            try
            {
                if (File.Exists(Sleeve.FilePath)) {
                    File.Delete(Sleeve.FilePath);
                }
            }
            catch (Exception e)
            {
                Log.Error("Delete", e);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
