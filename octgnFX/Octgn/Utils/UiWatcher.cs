// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Octgn.Library.Annotations;

namespace Octgn.Utils
{
    public class UiWatcher : INotifyPropertyChanged
    {
        private bool _running;
        private Task _task;

        private Dispatcher _dispatcher;
        private bool _isUiBusy;

        public bool IsUiBusy
        {
            get { return _isUiBusy; }
            set
            {
                if (value.Equals(_isUiBusy)) return;
                _isUiBusy = value;
                OnPropertyChanged("IsUiBusy");
                OnPropertyChanged("IsUiNotBusy");
            }
        }

        public bool IsUiNotBusy { get { return !IsUiBusy; } } 

        public UiWatcher(Dispatcher d)
        {
            _dispatcher = d;
        }

        public void Start()
        {
            lock (this)
            {
                if (_running) return;
                _running = true;
                _task = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
            }
        }

        public void Stop()
        {
            lock (this)
            {
                _running = false;
                _task.Wait();
                _task = null;
            }
        }

        private void Run()
        {
            var set = new AutoResetEvent(true);
            while (_running)
            {
                _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    set.Set();
                }));
                var result = set.WaitOne(2000) == false;
                var isSame = result == IsUiBusy; 
                IsUiBusy = result;
                if (IsUiBusy)
                {
                    System.Diagnostics.Trace.WriteLine("UI BUSY");
                }
                //if(isSame)
                    Thread.Sleep(500);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}