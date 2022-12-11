using GalaSoft.MvvmLight;
using System;

namespace Octgn.Tabs.Login
{
    public sealed class NewsItemViewModel : ViewModelBase
    {
        public DateTimeOffset Time {
            get { return _time; }
            set { base.Set(ref _time, value); }
        }
        private DateTimeOffset _time;

        public string Message {
            get { return _message; }
            set { base.Set(ref _message, value); }
        }
        private string _message;

        public NewsItemViewModel(DateTimeOffset time, string message) {
            Message = message;
            Time = time;
        }
    }
}
