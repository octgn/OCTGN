using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using Octgn.Data;
using Path = System.IO.Path;

namespace Octgn.Controls
{
	/// <summary>
	/// Interaction logic for SetList.xaml
	/// </summary>
	public partial class SetList : UserControl
	{
		public Data.Game SelectedGame;
		private Data.Game game;

		public SetList()
		{
			InitializeComponent();
		}

		public void Set(Data.Game game)
		{
			SelectedGame = game;
			RefreshList();
		}

		public void RefreshList()
		{
			lbSetList.Items.Clear();
			foreach (Set s in SelectedGame.Sets)
			{
				lbSetList.Items.Add(s);
			}
		}

		public void DeletedSelected()
		{
			if (SelectedGame == null)
				return;
			var wnd = new Windows.ChangeSetsProgressDialog("Removing Sets...") { Owner = Program.MainWindowNew};
			System.Collections.IList items = lbSetList.SelectedItems;
			ThreadPool.QueueUserWorkItem(_ =>
			{
				int current = 0, max = items.Count;
				wnd.UpdateProgress(current, max, null, false);
				wnd.ShowMessage("Set Removal can take some time. Please be patient.");
				foreach (Set s in items)
				{
					++current;
					try
					{
						wnd.ShowMessage(string.Format("Removing '{0}' ...", s.Name));
						SelectedGame.DeleteSet(s);
						wnd.UpdateProgress(current, max,
										   string.Format("'{0}' removed.", s.Name),
										   false);
					}
					catch (Exception ex)
					{
						wnd.UpdateProgress(current, max,
										   string.Format(
											   "'{0}' an error occured during removal:",
											   s.Name), true);
						wnd.UpdateProgress(current, max, ex.Message, true);
					}
				}
			});
			wnd.ShowDialog();
			RefreshList();
		}

		public void InstallSets()
		{
			if (SelectedGame == null)
				return;
			var ofd = new OpenFileDialog
			{
				Filter = "Cards set definition files (*.o8s)|*.o8s",
				Multiselect = true
			};
			if (ofd.ShowDialog() != true) return;


			//Move the definition file to a new location, so that the old one can be deleted
			string path = System.IO.Path.Combine(Prefs.DataDirectory, "Games", SelectedGame.Id.ToString(), "Sets");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			var wnd = new Windows.ChangeSetsProgressDialog("Installing Sets...") { Owner = Program.MainWindowNew };
			ThreadPool.QueueUserWorkItem(_ =>
			{
				int current = 0, max = ofd.FileNames.Length;
				wnd.UpdateProgress(current, max, null, false);
				foreach (string setName in ofd.FileNames)
				{
					++current;
					string shortName = System.IO.Path.GetFileName(setName);
					try
					{
						if (shortName != null)
						{
							string copyto = Path.Combine(path, shortName);
							if (setName.ToLower() != copyto.ToLower())
								File.Copy(setName, copyto, true);
							SelectedGame.InstallSet(copyto);
						}
						wnd.UpdateProgress(current, max,
										   string.Format("'{0}' installed.", shortName),
										   false);
					}
					catch (Exception ex)
					{
						wnd.UpdateProgress(current, max,
										   string.Format(
											   "'{0}' an error occured during installation:",
											   shortName), true);
						wnd.UpdateProgress(current, max, ex.Message, true);
					}
				}
			});
			wnd.ShowDialog();
			RefreshList();
		}

		public void PatchSelected()
		{
			if (SelectedGame == null)
				return;
			new Windows.PatchDialog { Owner = Program.MainWindowNew }.ShowDialog();
			RefreshList();
		}

		public void AddAutoUpdatedSets()
		{
			if (SelectedGame == null)
				return;
			new Windows.UrlSetList { game = SelectedGame }.ShowDialog();
			RefreshList();
		}
	}
}
