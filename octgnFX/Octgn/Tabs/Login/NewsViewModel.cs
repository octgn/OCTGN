using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Octgn.Tabs.Login
{

    public class NewsViewModel : ViewModelBase
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public NewsItemViewModel[] Items {
            get => _items;
            set => Set(ref _items, value);
        }
        private NewsItemViewModel[] _items;

        private readonly Uri _newsUrl;

        public NewsViewModel() {
            var builder = new UriBuilder(AppConfig.StaticWebsitePath);
            builder.Path = "news.xml";

            _newsUrl = builder.Uri;
        }

        public async Task Refresh() {
            try {
                Items = (await GetNewsItems()).ToArray();

            } catch (Exception e) {
                Log.Warn(nameof(Refresh), e);
            }
        }

        private async Task<IEnumerable<NewsItemViewModel>> GetNewsItems() {
            using (var wc = new WebClient()) {
                var newsXml = await wc.DownloadStringTaskAsync(_newsUrl);
                if (string.IsNullOrWhiteSpace(newsXml)) {
                    throw new Exception("Null news feed.");
                }

                var doc = XDocument.Parse(newsXml);
                var elements = doc.Root.Elements("item");

                return elements
                    .Select(e => new NewsItemViewModel(e))
                    .OrderByDescending(ni => ni.Time)
                    .ToArray();
            }
        }
    }

    public class NewsItemViewModel : ViewModelBase
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

        public NewsItemViewModel() {

        }

        public NewsItemViewModel(XElement element) {
            Message = (string)element;
            var dateAttribute = element.Attribute("date");
            Time = DateTimeOffset.Parse(dateAttribute.Value);
        }
    }
}
