/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;
using Octgn.Core.DataManagers;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.IO;
using Octgn.Library;
using Octgn.Play.Save;
using Octgn.Windows;

namespace Octgn.Tabs.GameHistory
{
    public partial class GameHistoryTab : INotifyPropertyChanged, IDisposable
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ObservableCollection<GameHistoryViewModel> GameHistories { get; }

        public GameHistoryTab() {
            InitializeComponent();
            GameHistories = new ObservableCollection<GameHistoryViewModel>();

            _refreshHistoryListTimer = new DispatcherTimer(InitialRefreshDelay.TimeSpan, DispatcherPriority.Normal, RefreshHistoryListTimer_Tick, Dispatcher);
        }

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var history = (GameHistoryViewModel)ListViewHistoryList.SelectedItem;

            if(history != null) {
                Dispatcher.VerifyAccess();

                if (WindowManager.PlayWindow != null) throw new InvalidOperationException($"Can't run more than one game at a time.");

                Dispatcher.InvokeAsync(async () => {
                    await Dispatcher.Yield(DispatcherPriority.Background);
                    var win = new GameHistoryWindow(history);
                    win.Show();
                    win.Activate();
                });
            }
        }

        private void ButtonRefreshClick(object sender, RoutedEventArgs e) {
            if (!IsRefreshingHistoryList) {
                RefreshHistoryListTimer_Tick(null, null);
            }
        }

        private void ButtonDeleteClick(object sender, RoutedEventArgs e) {
            if (!IsRefreshingHistoryList) {
                var selected = (GameHistoryViewModel)ListViewHistoryList.SelectedItem;

                TryDelete(selected.LogFile);
                TryDelete(selected.ReplayFile);
                TryDelete(selected.Path);

                RefreshHistoryListTimer_Tick(null, null);
            }
        }

        private void TryDelete(string path) {
            while (true) {
                try {
                    if (!File.Exists(path)) return;

                    File.Delete(path);

                    return;
                } catch (Exception ex) {
                    var result = MessageBox.Show($"Error deleting {path}: {ex.Message}.\r\n\r\nTry again?", "Try Again?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result != MessageBoxResult.Yes) return;
                }
            }
        }

        public bool IsHistorySelected {
            get => _isHistorySelected;
            set {
                if (value == _isHistorySelected)
                    return;

                _isHistorySelected = value;
                OnPropertyChanged();
            }
        }
        private bool _isHistorySelected;

        private void ListViewHistoryList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            IsHistorySelected = ListViewHistoryList.SelectedItem != null;
        }

        #region History List Refreshing

        public bool IsRefreshingHistoryList {
            get => _isRefreshingHistoryList;
            set => NotifyAndUpdate(ref _isRefreshingHistoryList, value);
        }

        private bool _isRefreshingHistoryList;
        private readonly DispatcherTimer _refreshHistoryListTimer;

        public static Duration InitialRefreshDelay { get; } = new Duration(TimeSpan.FromSeconds(2));
        public static Duration NormalRefreshDelay { get; } = new Duration(TimeSpan.FromSeconds(15));

        public Duration CurrentRefreshDelay {
            get => _currentRefreshDelay;
            set {
                if (!NotifyAndUpdate(ref _currentRefreshDelay, value)) return;
                OnPropertyChanged(nameof(IsInitialRefresh));
                _refreshHistoryListTimer.Interval = value.TimeSpan;
            }
        }

        public bool IsInitialRefresh => CurrentRefreshDelay.TimeSpan == InitialRefreshDelay.TimeSpan;

        private Duration _currentRefreshDelay = new Duration(TimeSpan.FromDays(10));


        private void RefreshHistoryListTimer_Tick(object sender, EventArgs e) {
            try {
                IsRefreshingHistoryList = true;

                _refreshHistoryListTimer.IsEnabled = false;

                if (CurrentRefreshDelay == InitialRefreshDelay) {
                    CurrentRefreshDelay = NormalRefreshDelay;
                }

                var histories = LoadHistories().ToArray();

                // /////// Update the visual list

                // Remove the games that don't exist anymore
                var removeList = GameHistories.Where(i => histories.All(x => x.Id != i.Id)).ToList();
                removeList.ForEach(x => GameHistories.Remove(x));

                // Add games that don't already exist in the list
                var addList = histories.Where(i => this.GameHistories.All(x => x.Id != i.Id)).ToList();
                GameHistories.AddRange(addList);

                // Update all the existing items with new data
                var dbgames = GameManager.Get().Games.ToArray();
                foreach (var g in GameHistories) {
                    var li = histories.FirstOrDefault(x => x.Id == g.Id);
                    g.Update(li);
                }
            } catch (Exception ex) {
                Log.Warn(nameof(RefreshHistoryListTimer_Tick), ex);
            } finally {
                IsRefreshingHistoryList = false;
                _refreshHistoryListTimer.IsEnabled = true;
            }
        }

        private IEnumerable<GameHistoryViewModel> LoadHistories() {
            var dir = new DirectoryInfo(Config.Instance.Paths.GameHistoryPath);

            if (!dir.Exists) {
                dir.Create();
            }

            var dbgames = GameManager.Get().Games
                .ToDictionary(x => x.Id, x => x);

            var historyFiles = dir.GetFiles("*.o8h");
            var replayFiles = dir
                .GetFiles("*.o8r")
                .ToDictionary(x => Path.GetFileNameWithoutExtension(x.Name), x => x)
            ;
            var logFiles = dir
                .GetFiles("*.o8l")
                .ToDictionary(x => Path.GetFileNameWithoutExtension(x.Name), x => x)
            ;

            foreach (var historyFile in historyFiles) {
                var historyFileName = Path.GetFileNameWithoutExtension(historyFile.Name);

                var historyFileContents = File.ReadAllBytes(historyFile.FullName);

                var history = History.Deserialize(historyFileContents);

                string gameName = "UNKNOWN";

                if (dbgames.ContainsKey(history.GameId)) {
                    gameName = dbgames[history.GameId].Name;
                }

                var vm = new GameHistoryViewModel(history, gameName, historyFile.FullName);

                if (replayFiles.TryGetValue(historyFileName, out var replayFile)) {
                    vm.ReplayFile = replayFile.FullName;
                }

                if (logFiles.TryGetValue(historyFileName, out var logFile)) {
                    vm.LogFile = logFile.FullName;
                }

                yield return vm;
            }
        }

        public void VisibleChanged(bool visible) {
            // Switching the interval on this timer allows the list to refresh quickly initially when the tab is ever viewed, then it'll wait the normal delay
            if(visible && (
                CurrentRefreshDelay != InitialRefreshDelay
                && CurrentRefreshDelay != NormalRefreshDelay)) {

                CurrentRefreshDelay = InitialRefreshDelay;

                _refreshHistoryListTimer.IsEnabled = visible;
            }

        }


        #endregion History List Refreshing

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool NotifyAndUpdate<T>(ref T privateField, T value, [CallerMemberName]string propertyName = null) {
            if (object.Equals(privateField, value)) return false;
            privateField = value;

            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose() {
            _refreshHistoryListTimer.IsEnabled = false;
        }
    }
}
