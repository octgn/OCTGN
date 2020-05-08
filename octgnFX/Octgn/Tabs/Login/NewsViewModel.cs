using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
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
            var feed = await Task.Run(() => {
                using (var reader = XmlReader.Create(AppConfig.NewsFeedPath)) {
                    return SyndicationFeed.Load(reader);
                }
            });

            var orderedItems = feed.Items.OrderByDescending(item => item.PublishDate);

            var latest10Updates = orderedItems.Take(10);

            var result = latest10Updates
                .Select(item => new NewsItemViewModel(item));

            return result;
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

        public NewsItemViewModel(SyndicationItem item) {
            Message = item.Title.Text;
            Time = item.PublishDate;
        }

        public NewsItemViewModel(XElement element) {
            Message = (string)element;
            var dateAttribute = element.Attribute("date");
            Time = DateTimeOffset.Parse(dateAttribute.Value);
        }
    }
}
