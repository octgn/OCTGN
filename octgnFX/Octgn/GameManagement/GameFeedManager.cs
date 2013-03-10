namespace Octgn.GameManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    using NuGet;

    using Octgn.Core.DataManagers;
    using Octgn.ViewModels;

    public class GameFeedManager
    {
        public GameFeedManager()
        {
            //new NuGet.PackageManager()
        }

        public void CheckForUpdates()
        {
            if (!Program.UseGamePackageManagement) return;
            var repo = NuGet.PackageRepositoryFactory.Default.CreateRepository(Program.GameFeed);
            var packages = repo.GetPackages().Where(x=>x.IsLatestVersion).ToList();
            foreach (var pack in packages)
            {
                var ttl = pack.Tags.ToLower();
                var game = GameManager.Get().GetById(new Guid(pack.Id));
                if (game == null) continue;
                if (pack.Version.Version > game.Version)
                {
                    var res = MessageBox.Show(
                        string.Format("There is a new version of {0} - {1}, would you like to install it?", game.Name,pack.Version.Version),
                        "OCTGN",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (res == DialogResult.OK)
                    {
                        
                    }
                }
            }
        }

        /// <summary>
        /// Gets a list of games from a feed.
        /// </summary>
        /// <returns>List of games</returns>
        public List<GameFeedViewModel> GetGameList()
        {
            var repo = NuGet.PackageRepositoryFactory.Default.CreateRepository(Program.GameFeed);
            var list = repo.GetPackages().Where(x => x.IsLatestVersion).OrderByDescending(x => x.Version.Version).ToList();
            var ret = new List<GameFeedViewModel>();
            foreach (var i in list)
            {
                try
                {
                    var model = new GameFeedViewModel();
                    model.Version = i.Version.Version;
                    model.Authors = i.Authors;
                    model.Description = i.Description;
                    model.DownloadCount = i.DownloadCount;
                    model.IconUrl = i.IconUrl;
                    model.Id = i.Id;
                    model.Owners = i.Owners;
                    model.ProjectUrl = i.ProjectUrl;
                    model.Published = i.Published;
                    model.ReleaseNotes = i.ReleaseNotes;
                    model.Summary = i.Summary;
                    model.Tags = i.Tags;
                    model.Title = i.Title;
                    ret.Add(model);
                }
                catch 
                {
                }
            }
            return ret;
        }
    }
}
