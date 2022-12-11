using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Octgn.Tabs.Login
{
    public sealed class NewsViewModel : ViewModelBase
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public NewsItemViewModel[] Items {
            get => _items;
            set => Set(ref _items, value);
        }
        private NewsItemViewModel[] _items;

        public NewsViewModel() {
        }

        public async Task Refresh() {
            try {
                Items = await GetNewsItems();
            } catch (Exception e) {
                Log.Warn(nameof(Refresh), e);
            }
        }

        private async Task<NewsItemViewModel[]> GetNewsItems() {
            var client = new WebClient();

            var raw_str = await client.DownloadStringTaskAsync("https://gist.githubusercontent.com/kellyelton/3ab99bcff9bddfcd62982e321c055fad/raw/");

            var news_items = JsonConvert.DeserializeObject<NewsItem[]>(raw_str);
                    
            var orderedItems = news_items.OrderByDescending(item => item.Timestamp);

            var latest10Updates = orderedItems.Take(10);

            var result = latest10Updates
                .Select(item => new NewsItemViewModel(item.Timestamp, item.Text))
                .ToArray()
            ;

            return result;
        }

/**
[
  {
    "Text": "We\u0027ve launched a new version of Octgn. If you have any issues, please let us know!",
    "Timestamp": "2021-08-26T16:18:02Z"
  },
  {
    "Text": "False alarm, our update will be tomorrow instead, Aug 26th at 10am CST.",
    "Timestamp": "2021-08-25T14:09:01Z"
  }
]
**/
        private class NewsItem
        {
            public string Text { get; set; }

            public DateTimeOffset Timestamp { get; set; }
        }
    }
}
