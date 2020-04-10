// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;

namespace Octide.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using Microsoft.Win32;
    using Octgn.DataNew;
    using Octgn.DataNew.Entities;
    using Octgn.DataNew.FileDB;
    using Octgn.Library;
    using Octgn.ProxyGenerator;
    using Octide.Messages;
    using Octide.Views;

	public class LoaderViewModel : ViewModelBase
	{
		public RelayCommand NewGameCommand { get; private set; }
		public RelayCommand ImportGameCommand { get; private set; }
		public RelayCommand LoadGameCommand { get; private set; }
		public Game SelectedFile { get; set; }
		public LoaderViewModel()
		{
			NewGameCommand = new RelayCommand(NewGame);
			ImportGameCommand = new RelayCommand(ImportGame);
			LoadGameCommand = new RelayCommand(LoadGame);

			var config = new FileDbConfiguration()
				.SetDirectory(Path.Combine(Config.Instance.Paths.DataDirectory, "IdeDevDatabase"))
				.SetExternalDb()
				.DefineCollection<Game>("Game")
				.OverrideRoot(x => x.Directory(""))
				.SetPart(x => x.Property(y => y.Id))
				.SetPart(x => x.File("definition.xml"))
				.SetSerializer<GameSerializer>()
				.Conf()
				.DefineCollection<Set>("Sets")
				.OverrideRoot(x => x.Directory(""))
				.SetPart(x => x.Property(y => y.GameId))
				.SetPart(x => x.Directory("Sets"))
				.SetPart(x => x.Property(y => y.Id))
				.SetPart(x => x.File("set.xml"))
				.SetSerializer<SetSerializer>()
				.Conf()
				.DefineCollection<GameScript>("Scripts")
				.OverrideRoot(x => x.Directory(""))
				.SetSteril()
				.Conf()
				.DefineCollection<ProxyDefinition>("Proxies")
				.OverrideRoot(x => x.Directory(""))
				.SetSteril()
				.Conf()
				.SetCacheProvider<FullCacheProvider>();

			DbContext.SetContext(config);

		}
		public void NewGame()
		{
			ViewModelLocator.GameLoader.New();
			RaisePropertyChanged("IdeDevDatabaseGames");

		}
		public void ImportGame()
		{
			var fo = new OpenFileDialog();
			fo.Filter = "Definition File (definition.xml)|definition.xml";
			if ((bool)fo.ShowDialog() == false)
			{
				return;
			}

			ViewModelLocator.GameLoader.ImportGame(fo.FileName);
			Task.Factory.StartNew(LoadMainWindow);


		}
		public void LoadGame()
		{
			ViewModelLocator.GameLoader.LoadGame(SelectedFile);
			Task.Factory.StartNew(LoadMainWindow);


		}
		public ObservableCollection<Game> IdeDevDatabaseGames
		{
			get
			{
				return new ObservableCollection<Game>(DbContext.Get().Games);
			}
		}

		public string Title
		{
			get
			{
				return "OCTIDE";
			}
		}

		public Version Version
		{
			get
			{
				return typeof(LoaderViewModel).Assembly.GetName().Version;
			}
		}

		private void LoadMainWindow()
		{
			Messenger.Default.Send(new WindowActionMessage<MainViewModel>(WindowActionType.Create));
			Messenger.Default.Send(new WindowActionMessage<MainViewModel>(WindowActionType.Show));
			Messenger.Default.Send(new WindowActionMessage<MainViewModel>(WindowActionType.SetMain));
			Messenger.Default.Send(new WindowActionMessage<LoaderViewModel>(WindowActionType.Close));
		}
	}
}