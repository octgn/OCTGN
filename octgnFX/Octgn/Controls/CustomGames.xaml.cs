using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Octgn.Utils;
using Skylabs.Lobby;

namespace Octgn.Controls
{
    using System.Collections.ObjectModel;

    using Microsoft.Scripting.Utils;

    /// <summary>
	/// Interaction logic for CustomGames.xaml
	/// </summary>
	public partial class CustomGames : UserControl
	{
    //    public static DependencyProperty HostedGameListProperty = DependencyProperty.Register(
    //"HostedGameList", typeof(List<HostedGameViewModel>), typeof(CustomGames));

    //    public List<HostedGameViewModel> HostedGameList
    //    {
    //        get { return GetValue(HostedGameListProperty) as List<HostedGameViewModel>; }
    //        set{SetValue(HostedGameListProperty, value);}
    //    }

        public ObservableCollection<HostedGameViewModel> HostedGameList { get; set; }

		private Timer timer;
		private bool isConnected;
		private bool waitingForGames;

		public CustomGames()
		{
			InitializeComponent();
            HostedGameList = new ObservableCollection<HostedGameViewModel>();
			Program.LobbyClient.OnLoginComplete += LobbyClient_OnLoginComplete;
			Program.LobbyClient.OnDisconnect += LobbyClient_OnDisconnect;
			Program.LobbyClient.OnDataReceived += LobbyClient_OnDataReceived;
			timer = new Timer(8000);
			timer.Start();
			timer.Elapsed += timer_Elapsed;
		}

		void LobbyClient_OnDisconnect(object sender, EventArgs e)
		{
			isConnected = false;
		}

		void LobbyClient_OnLoginComplete(object sender, LoginResults results)
		{
			isConnected = true;
		}

		void LobbyClient_OnDataReceived(object sender, DataRecType type, object data)
		{
			if (type == DataRecType.GameList || type == DataRecType.GamesNeedRefresh)
			{
				Trace.WriteLine("Games Received");
				RefreshGameList();
				waitingForGames = false;
			}
		}

		void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Trace.WriteLine("Timer ticks");
			if (!isConnected || waitingForGames) return;
			Trace.WriteLine("Begin refresh games.");
			waitingForGames = true;
			Program.LobbyClient.BeginGetGameList();
		}

		void RefreshGameList()
		{
			Trace.WriteLine("Refreshing list...");
			var list = Program.LobbyClient.GetHostedGames().Select(x=>new HostedGameViewModel(x)).ToList();
			Dispatcher.Invoke(new Action(() =>
			{
				var removeList = HostedGameList.Where(i => !list.Any(x => x.Port == i.Port)).ToList();
                removeList.ForEach(x=>HostedGameList.Remove(x));
				var addList = list.Where(i => !HostedGameList.Any(x => x.Port == i.Port)).ToList();
				HostedGameList.AddRange(addList);
			}));
		}
	}
	public class HostedGameViewModel
	{
		public Guid GameId { get; set; }
		public string GameName { get; set; }
		public Version GameVersion { get; set; }
		public string Name { get; set; }
		public NewUser User { get; set; }
		public int Port { get; set; }
		public Skylabs.Lobby.EHostedGame Status { get; set; }
		public DateTime StartTime { get; set; }
		public HostedGameViewModel(HostedGameData data)
		{
			var game = Program.GamesRepository.Games.FirstOrDefault(x => x.Id == data.GameGuid);
			GameId = data.GameGuid;
			GameVersion = data.GameVersion;
			Name = data.Name;
			User = data.UserHosting;
			Port = data.Port;
			Status = data.GameStatus;
			StartTime = data.TimeStarted;
		    GameName = "{Unknown Game}";
			if (game == null) return;
			GameName = game.Name;
		}
	}
}
