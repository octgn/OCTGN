/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
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
using Octgn.Extentions;
using Octgn.Library;
using Octgn.Site.Api;
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
                var allSleeves = SleeveManager.Instance.GetAllSleeves();
                var mySleeves = SleeveManager.Instance.GetUserSleeves();

                SetList(allSleeves, mySleeves);
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

        public void SetList(IEnumerable<ApiSleeve> sleeves, IEnumerable<ApiSleeve> ownedSleeves)
        {
            var nl = (from s in
                          (from s in sleeves
                           select new SleeveViewModel(s, ownedSleeves.Any(x => x.Id == s.Id))
                          )
                      orderby s.Owned
                      select s
                     );
            Dispatcher.Invoke(new Action(() =>
            {
                Sleeves.Clear();
                foreach (var s in nl)
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

        private void GetSleeve(object sender, RoutedEventArgs e)
        {
            var vm = (sender as System.Windows.Controls.Button).DataContext as SleeveViewModel;
            if (vm == null)
                return;
            vm.GetAsync();
        }

        private void ButtonRefreshClick(object sender, RoutedEventArgs e)
        {
            this.RefreshListAsync();
        }
    }

    public class SleeveViewModel : ViewModelBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string OWNED_STRING = "Already Owned";
        private const string NOT_OWNED_STRING = "Get It";

        public int Id { get; set; }
        public string Url { get; set; }

        private bool _owned;

        private bool _isBusy;


        public bool ButtonEnabled
        {
            get { return IsBusy == false && Owned == false; }
        }


        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
                RaisePropertyChanged(() => ButtonEnabled);
            }
        }

        public bool Owned
        {
            get { return _owned; }
            set
            {
                if (_owned == value) return;
                _owned = value;
                RaisePropertyChanged(() => Owned);
                RaisePropertyChanged(() => ButtonText);
                RaisePropertyChanged(() => ButtonEnabled);
            }
        }

        public string ButtonText { get { return _owned ? OWNED_STRING : NOT_OWNED_STRING; } }


        public SleeveViewModel(ApiSleeve sleeve, bool owned)
        {
            this.Id = sleeve.Id;
            this.Url = sleeve.Url;
            Owned = owned;
        }

        public void GetAsync()
        {
            Task.Factory.StartNew(Get);
        }

        public void Get()
        {
            IsBusy = true;
            try
            {
                var result = SleeveManager.Instance.AddSleeveToAccount(this.Id);
                if (result)
                    this.Owned = true;
            }
            catch (Exception e)
            {
                Log.Error("Get", e);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
