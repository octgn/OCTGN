namespace Octgn.GameManagement
{
    using System.Linq;
    using System.Windows.Forms;

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
                var game = Program.GamesRepository.AllGames.FirstOrDefault(x => ttl.Contains(x.Id.ToString().ToLower()));
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
    }
}
