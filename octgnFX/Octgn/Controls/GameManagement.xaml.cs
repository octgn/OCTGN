﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Octgn.Definitions;
using Octgn.Launcher;

namespace Octgn.Controls
{
	/// <summary>
	/// Interaction logic for GameManagement.xaml
	/// </summary>
	public partial class GameManagement : UserControl
	{
		static public bool GamesChanged = false;
		public Octgn.Data.Game SelectedGame = null;
		private bool isTaskRunning;
		public GameManagement()
		{
			InitializeComponent();
			Program.GamesRepository.GameInstalled += GamesRepositoryGameInstalled;
			this.Loaded += GameManagement_Loaded;
			this.ButtonInstallGame.Click += ButtonInstallGame_Click;
			this.ButtonUninstallGame.Click += ButtonUninstallGame_Click;
			this.ButtonInstallSet.Click += ButtonInstallSet_Click;
			this.ButtonUninstallSet.Click += ButtonUninstallSet_Click;
			this.ButtonAddAutoUpdateSet.Click += ButtonAddAutoUpdateSet_Click;
			this.ButtonPatchSet.Click += ButtonPatchSet_Click;
		}

		void ButtonPatchSet_Click(object sender, RoutedEventArgs e)
		{
			SetList.PatchSelected();
		}

		void ButtonAddAutoUpdateSet_Click(object sender, RoutedEventArgs e)
		{
			SetList.AddAutoUpdatedSets();
		}

		void ButtonUninstallSet_Click(object sender, RoutedEventArgs e)
		{
			SetList.DeletedSelected();
		}

		void ButtonInstallSet_Click(object sender, RoutedEventArgs e)
		{
			SetList.InstallSets();
		}

		void ButtonUninstallGame_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedGame == null) return;
			var msg = MessageBox.Show(string.Format("Are you sure you want to delete {0}?", SelectedGame.Name), "Confirmation", MessageBoxButton.YesNo);
			if (msg != MessageBoxResult.Yes) return;
			var uninstallTask = new Task(() => { 
				Program.GamesRepository.UninstallGame(SelectedGame);
				ReloadGameList();
			});
			StartTask(uninstallTask, 0);
		}

		void ButtonInstallGame_Click(object sender, RoutedEventArgs e)
		{
			var ofd = new OpenFileDialog { Filter = "Game definition files (*.o8g)|*.o8g", Multiselect = true };
			if (ofd.ShowDialog() != true) return;
			var max = ofd.FileNames.Length;
			var installTask = new Task(() =>
						 {
							 
							 for(var i= 0;i<max;i++)
							 {
								 var file = ofd.FileNames[i];
								 var newFileName = Uri.UnescapeDataString(file);
								 GameDef.FromO8G(newFileName).Install();
								 UpdateTask(i,max);
							 }
							 ReloadGameList();
						 });
			StartTask(installTask, max);
		}

		private void StartTask(Task task, int max)
		{
			if (isTaskRunning) return;
			Dispatcher.Invoke(new Action(() =>{
				this.IsEnabled = false;
				ProgressBar.Visibility = Visibility.Visible;
				ProgressBar.Maximum = max;
				ProgressBar.IsIndeterminate = true;
				task.ContinueWith(EndTask);
				task.Start();
			}));
		}

		private void UpdateTask(int value, int max)
		{
			Dispatcher.Invoke(new Action(() =>
			{
				ProgressBar.IsIndeterminate = false;
				ProgressBar.Maximum = max;
				ProgressBar.Value = value;
			}));
		}

		private void EndTask(Task task)
		{
			Dispatcher.Invoke(new Action(() =>
			{
				ProgressBar.Visibility = Visibility.Collapsed;
				isTaskRunning = false;
				this.IsEnabled = true;
			}));
		}

		private void GameManagement_Loaded(object sender, RoutedEventArgs e)
		{
			ReloadGameList();
		}

		private void GamesRepositoryGameInstalled(object sender, EventArgs e)
		{
			ReloadGameList();
			GamesChanged = true;
		}

		private void ReloadGameList()
		{
			Dispatcher.Invoke(new Action(() => { 
				stackPanel1.Children.Clear();
				foreach (GameListItem gs in Program.GamesRepository
					.AllGames
					.OrderBy(x=>x.Name)
					.Select(g => new GameListItem { Game = g }))
				{
					gs.MouseUp += GsMouseUp;
					stackPanel1.Children.Add(gs);
				}
				SelectedGame = null;
				HandleGameSelectedChanged();
			}));
		}

		private void GsMouseUp(object sender, MouseButtonEventArgs e)
		{
			var gs = (GameListItem)sender;
			SelectedGame = SelectedGame == gs.Game ? null : gs.Game;
			HandleGameSelectedChanged();
		}

		private void HandleGameSelectedChanged()
		{
			ButtonUninstallGame.IsEnabled = SelectedGame != null;
			StackPanelSetButtons.IsEnabled = SelectedGame != null;
			foreach (var gi in stackPanel1.Children.OfType<GameListItem>())
			{
				gi.IsSelected = false;
			}
			if (SelectedGame == null) return;
			SetList.Set(SelectedGame);
			var gameItem = stackPanel1.Children.OfType<GameListItem>().FirstOrDefault(x => x.Game.Id == SelectedGame.Id);
			if (gameItem != null)
				gameItem.IsSelected = true;
		}
	}
}
