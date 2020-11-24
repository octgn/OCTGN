using Octgn.DataNew;
using Octgn.DataNew.Entities;
using Octgn.GameWizard.Controls;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Octgn.GameWizard.Pages
{
    public partial class CreatePage : WizardPage
    {
        public CreatePage() {
            InitializeComponent();

            ForwardEnabled = false;
            BackEnabled = false;
        }

        public override async void OnEnteringPage() {
            try {
                ForwardEnabled = false;
                BackEnabled = false;
                Progress.IsIndeterminate = true;
                Progress.Foreground = Brushes.CadetBlue;

                await BuildGamePlugin().ConfigureAwait(true);

                //TODO: Go forward automatically
                ForwardEnabled = true;

                StatusText.Text = "Complete";
                Progress.IsIndeterminate = false;
                Progress.Foreground = Brushes.CadetBlue;
                Progress.Maximum = 1;
                Progress.Value = 1;
            } catch (Exception e) {
                //TODO: Log exception
                BackEnabled = true;

                StatusText.Text = "Could not Complete";

                Progress.IsIndeterminate = false;
                Progress.Foreground = Brushes.Red;
                Progress.Maximum = 1;
                Progress.Value = 1;

                var message
                = e.Message
                + Environment.NewLine
                + e.StackTrace.Take(500);

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private async Task BuildGamePlugin() {
            var plugin_directory = await CopyTemplate().ConfigureAwait(true);

            Game.Directory = plugin_directory;

            await ConfigureTemplate(plugin_directory).ConfigureAwait(true);
        }

        private async Task<string> CopyTemplate() {
            //TODO: Allow this to be changed
            var game_name_folder = Game.Name.Replace(".", "").Replace("~", "").Replace("/", "").Replace("\\", "");

            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                game_name_folder);

            StatusText.Text = "Copying Template...";

            await Task.Run(() => {
                if (Directory.Exists(directory)) {
                    var message
                    = directory
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Directory already exists.";

                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    throw new Exception($"Directory already exists: {directory}");
                }

                try {
                    Directory.CreateDirectory(directory);
                } catch (IOException e) {
                    //TODO: Log exception

                    var message
                    = directory
                    + Environment.NewLine
                    + Environment.NewLine
                    + e.Message
                    + Environment.NewLine
                    + e.HResult;

                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    throw;
                }
            }).ConfigureAwait(true);

            await Task.Run(() => {
                //TODO: Change this for production
                var template_directory = Path.GetFullPath("../../../Octgn.GameStudio/Resources/Templates/Basic-Game/Game Plugin");

                CopyContents(template_directory, directory);
            }).ConfigureAwait(true);

            return directory;
        }

        private async Task ConfigureTemplate(string plugin_directory) {
            StatusText.Text = "Configuring Template...";

            var def_path = Path.Combine(plugin_directory, "definition.xml");

            // Correct anything making it unreadable by the serializer
            await Task.Run(() => {
                var def_contents = File.ReadAllText(def_path);

                var empty_guid = Guid.Empty.ToString().ToUpper();
                def_contents = def_contents.Replace("SAMPLEID-GUID-0000-0000-00000000GAME", empty_guid);

                File.WriteAllText(def_path, def_contents);
            });

            var serializer = new GameSerializer();

            var game = await Task.Run(() => (Game)serializer.Deserialize(def_path));

            game.Id = Guid.Parse(Game.Id);
            game.Name = Game.Name;
            game.Description = Game.Description;
            game.Authors = Game.Authors.Split(',').ToList();
            game.GameUrl = Game.Url;
            game.IconUrl = Game.ImageUrl;
            game.UseTwoSidedTable = Game.IsDualSided;
            game.Version = new Version(Game.Version);

            await Task.Run(() => {
                var game_bytes = serializer.Serialize(game);

                File.WriteAllBytes(def_path, game_bytes);
            });

            // Correcting set.xml
            var set_path = Path.Combine(plugin_directory, "Sets", "Base-Set", "set.xml");
            await Task.Run(() => {
                var set_contents = File.ReadAllText(set_path);

                set_contents = set_contents.Replace("SAMPLEID-GUID-0000-0000-00000000GAME", game.Id.ToString().ToUpper());
                set_contents = set_contents.Replace("SAMPLEID-GUID-0000-0000-000000000SET", Guid.NewGuid().ToString().ToUpper());
                set_contents = set_contents.Replace("SAMPLEID-GUID-0000-0000-0000000CARD1", Guid.NewGuid().ToString().ToUpper());

                File.WriteAllText(set_path, set_contents);
            });
        }

        private void CopyContents(string source, string destination) {
            foreach (var subdir in Directory.EnumerateDirectories(source)) {
                var dirname = Path.GetFileName(subdir); // Since this is a dir, it will return the directory name

                var newdir = Path.Combine(destination, dirname);

                Directory.CreateDirectory(newdir);

                CopyContents(subdir, newdir);
            }

            foreach (var file in Directory.EnumerateFiles(source)) {
                var file_name = Path.GetFileName(file);

                var destination_file_path = Path.Combine(destination, file_name);

                while (true) {
                    try {
                        File.Copy(file, destination_file_path);

                        break;
                    } catch (IOException e) {
                        //TODO: Log exception

                        var message
                            = file
                            + Environment.NewLine
                            + destination_file_path
                            + Environment.NewLine
                            + Environment.NewLine
                            + e.Message
                            + Environment.NewLine
                            + e.HResult
                            + Environment.NewLine
                            + "Try Again?"
                        ;

                        var result = MessageBox.Show(message, "Error - Try Again?", MessageBoxButton.OKCancel, MessageBoxImage.Error);

                        if (result != MessageBoxResult.OK) {
                            throw;
                        }
                    }
                }
            }
        }
    }
}
