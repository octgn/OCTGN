/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using log4net;
using Octgn.Annotations;
using Octgn.Core;
using Octgn.Site.Api.Models;

namespace Octgn.Tabs.Store
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

        public Task RefreshListAsync()
        {
            return Task.Factory.StartNew(RefreshList);
        }

        public void RefreshList()
        {
            IsLoading = true;
            try
            {
                var c = new Octgn.Site.Api.ApiClient();
                var sleeves = c.GetAllSleeves(0, 100);
				SetList(sleeves.Sleeves.Select(x=>new SleeveViewModel(x)));
            }
            catch (Exception e)
            {
                Log.Error("RefreshList",e);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void SetList(IEnumerable<SleeveViewModel> sleeves)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Sleeves.Clear();
                foreach (var s in sleeves)
                {
                    Sleeves.Add(s);
                }
            }));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class SleeveViewModel : ViewModelBase
    {
        public int Id { get; set; }
        public bool Owned { get; set; }
        public string Url { get; set; }

        public SleeveViewModel(ApiSleeve sleeve)
        {
            this.Id = sleeve.Id;
            this.Url = sleeve.Url;
            Owned = Prefs.Username.Equals(sleeve.Owner.UserName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
