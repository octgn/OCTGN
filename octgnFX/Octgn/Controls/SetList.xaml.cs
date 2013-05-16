using System;
using System.IO;
using System.Threading;
using System.Windows.Controls;

using Microsoft.Win32;

using Path = System.IO.Path;

namespace Octgn.Controls
{
    using System.Linq;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;

    /// <summary>
	/// Interaction logic for SetList.xaml
	/// </summary>
	public partial class SetList : UserControl
	{
        public DataNew.Entities.Game SelectedGame;
        private DataNew.Entities.Game game;

		public SetList()
		{
			InitializeComponent();
		}

        public void Set(DataNew.Entities.Game game)
		{
			SelectedGame = game;
			RefreshList();
		}

		public void RefreshList()
		{
			lbSetList.Items.Clear();
            foreach (var s in SelectedGame.Sets().Select(x=>new SetListSetItem(x)).OrderBy(x=>x.Name))
			{
				lbSetList.Items.Add(s);
			}
		}

		public void DeletedSelected()
		{
            //if (SelectedGame == null)
            //    return;
            //var wnd = new Windows.ChangeSetsProgressDialog("Removing Sets...") { Owner = WindowManager.Main };
            //System.Collections.IList items = lbSetList.SelectedItems;
            //ThreadPool.QueueUserWorkItem(_ =>
            //{
            //    int current = 0, max = items.Count;
            //    wnd.UpdateProgress(current, max, null, false);
            //    wnd.ShowMessage("Set Removal can take some time. Please be patient.");
            //    foreach (DataNew.Entities.Set s in items)
            //    {
            //        ++current;
            //        try
            //        {
            //            wnd.ShowMessage(string.Format("Removing '{0}' ...", s.Name));
                        
            //            SelectedGame.DeleteSet(s);
            //            wnd.UpdateProgress(current, max,
            //                               string.Format("'{0}' removed.", s.Name),
            //                               false);
            //        }
            //        catch (Exception ex)
            //        {
            //            wnd.UpdateProgress(current, max,
            //                               string.Format(
            //                                   "'{0}' an error occured during removal:",
            //                                   s.Name), true);
            //            wnd.UpdateProgress(current, max, ex.Message, true);
            //        }
            //    }
            //});
            //wnd.ShowDialog();
            //RefreshList();
		}

		public void PatchSelected()
		{
			if (SelectedGame == null)
				return;
            new Windows.PatchDialog { Owner = WindowManager.Main }.ShowDialog();
			RefreshList();
		}

        //public void AddAutoUpdatedSets()
        //{
        //    if (SelectedGame == null)
        //        return;
        //    new Windows.UrlSetList { game = SelectedGame }.ShowDialog();
        //    RefreshList();
        //}
        internal class SetListSetItem : Set 
        {
            public SetListSetItem(Set set)
            {
                this.Id = set.Id;
                this.Markers = set.Markers;
                this.Name = set.Name;
                this.PackageName = set.PackageName;
                this.Packs = set.Packs;
                this.Version = set.Version;
                this.GameId = set.GameId;
                this.GameVersion = set.GameVersion;
                this.Filename = set.Filename;
            }
            public override string ToString()
            {
                return this.Name;
            }
        }
	}
}
