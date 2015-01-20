using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Exceptionless.Json;
using log4net;
using NuGet;

namespace Octgn.Tabs.ChallengeBoards
{
    public partial class ChallengeBoards : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        internal Timer RefreshTimer;

        public ObservableCollection<BoardViewModel> Boards { get; set; }
		

        public ChallengeBoards()
        {
            Boards = new ObservableCollection<BoardViewModel>();
            InitializeComponent();
            RefreshTimer = new Timer(60000 * 5);
            RefreshTimer.Elapsed += RefreshTimerOnElapsed;

            Task.Factory.StartNew(() => RefreshTimerOnElapsed(null, null));
            RefreshTimer.Start();
        }

        private void RefreshTimerOnElapsed(object sender, ElapsedEventArgs evilDevilMan)
        {
            try
            {
                var query = "http://www.challengeboards.net/boards/Search?search=a";
                var client = (HttpWebRequest)HttpWebRequest.Create(query);
                //User-Agent: OCTGN
                //Host: www.challengeboards.net
                //Accept: application/json, text/javascript, */*; q=0.01
                //X-Requested-With: XMLHttpRequest
                client.UserAgent = "OCTGN";
				client.Accept = "application/json, text/javascript, */*; q=0.01";
                client.Headers.Set("X-Requested-With", "XMLHttpRequest");
                client.Method = "GET";

                var resp = client.GetResponse();
                var str = resp.GetResponseStream().ReadToEnd();

                var obj = JsonConvert.DeserializeObject <SearchBoardsResponse>(str);

                Dispatcher.Invoke(new Action(() => Boards.Clear()));
                foreach (var i in obj.Boards)
                {
                    Dispatcher.Invoke(new Action(() => Boards.Add(i)));
                }

                //Requires the following pull request to be merged
                //https://github.com/jrmitch120/ChallengeBoard/pull/10

            }
            catch (Exception e) 
            {
                Log.Warn("RefreshTimerOnElapsed Error", e);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var sen = sender as FrameworkElement;
            if (sen == null) return;
            var dc = sen.DataContext as BoardViewModel;
            if (dc == null) return;

            var url = String.Format("http://www.challengeboards.net/boards/standings/{0}", dc.BoardId);
			Program.LaunchUrl(url);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PullRequestClick(object sender, MouseButtonEventArgs e)
        {
            Program.LaunchUrl("https://github.com/jrmitch120/ChallengeBoard/pull/10");
        }
    }

  //{
  //"Boards": [
  //  {
  //    "BoardId": 2133,
  //    "Name": "League cavern of souls - khans of Tarkir",
  //    "Owner": {
  //      "Name": "jasmintondreau",
  //      "CompetitorId": 5095
  //    },
  //    "Created": "7\/8\/2014 11:06 AM",
  //    "End": "7\/4\/2015 12:00 AM",
  //    "PercentComplete": 18
  //  },
  //  {
  //    "BoardId": 2135,
  //    "Name": "Android: Netrunner BH",
  //    "Owner": {
  //      "Name": "ligaanrbh",
  //      "CompetitorId": 5097
  //    },
  //    "Created": "7\/8\/2014 8:58 PM",
  //    "End": "6\/16\/2015 12:00 AM",
  //    "PercentComplete": 37
  //  },

    public class SearchBoardsResponse
    {
        public List<BoardViewModel> Boards { get; set; }
    }

    public class BoardViewModel
    {
        public int BoardId { get; set; }
        public string Name { get; set; }
        public CompetitorModel Owner { get; set; }
        public DateTime Created { get; set; }
        public DateTime End { get; set; }
        public int PercentComplete { get; set; }
    }

    public class CompetitorModel
    {
        public int CompetitorId { get; set; }
        public string Name { get; set; }
    }
}
