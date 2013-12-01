namespace Octide.ViewModel
{
    using System;
    using System.Linq;
    using System.Windows.Media;

    using GalaSoft.MvvmLight;

    using Octgn.DataNew.Entities;
    using Octgn.DataNew.FileDB;

    public class GameTabViewModel : ViewModelBase
    {
        internal Game Game { get; set; }

        public string Name
        {
            get
            {
                return this.Game.Name;
            }
            set
            {
                if (value == this.Game.Name) return;
                this.Game.Name = value;
				RaisePropertyChanged("Name");
            }
        }

        public string Description
        {
            get
            {
                return this.Game.Description;
            }
            set
            {
                if (value == this.Game.Description) return;
                this.Game.Description = value;
                RaisePropertyChanged("Description");
            }
        }

        public string GameUrl
        {
            get
            {
                return this.Game.GameUrl;
            }
            set
            {
                if (value == this.Game.GameUrl) return;
                this.Game.GameUrl= value;
                RaisePropertyChanged("GameUrl");
            }
        }

        public string IconUrl
        {
            get
            {
                return this.Game.IconUrl;
            }
            set
            {
                if (value == this.Game.IconUrl) return;
                this.Game.IconUrl = value;
                RaisePropertyChanged("IconUrl");
            }
        }

        public int MarkerSize
        {
            get
            {
                return this.Game.MarkerSize;
            }
            set
            {
                if (value == this.Game.MarkerSize) return;
                this.Game.MarkerSize = value;
                RaisePropertyChanged("MarkerSize");
            }
        }

        public string Version
        {
            get
            {
                return this.Game.Version.ToString();
            }
            set
            {
                if (value == this.Game.Version.ToString()) return;
                Version v;
                if (System.Version.TryParse(value, out v))
                {
                    this.Game.Version = v;
                }
				RaisePropertyChanged("Version");
            }
        }

        public string Authors
        {
            get
            {
                return String.Join(", ",this.Game.Authors);
            }
            set
            {
                var list = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x=>!String.IsNullOrWhiteSpace(x)).ToList();
                this.Game.Authors = list;
                RaisePropertyChanged("Authors");
            }
        }

        public string Tags
        {
            get
            {
                return String.Join(", ", this.Game.Tags);
            }
            set
            {
                var list = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
                this.Game.Tags = list;
                RaisePropertyChanged("Tags");
            }
        }

        public bool UseTwoSidedTable
        {
            get
            {
                return this.Game.UseTwoSidedTable;
            }
            set
            {
                if (value == this.Game.UseTwoSidedTable) return;
                this.Game.UseTwoSidedTable = value;
                RaisePropertyChanged("UseTwoSidedTable");
            }
        }

        public Color NoteBackgroundColor
        {
            get
            {
                return (Color)ColorConverter.ConvertFromString(this.Game.NoteBackgroundColor);
            }
            set
            {
                this.Game.NoteBackgroundColor = new ColorConverter().ConvertToString(value);
                RaisePropertyChanged("NoteBackgroundColor");
            }
        }

        public Color NoteForegroundColor
        {
            get
            {
                return (Color)ColorConverter.ConvertFromString(this.Game.NoteForegroundColor);
            }
            set
            {
                this.Game.NoteForegroundColor = new ColorConverter().ConvertToString(value);
                RaisePropertyChanged("NoteForegroundColor");
            }
        }

        public GameTabViewModel()
        {
            var s = new Octgn.DataNew.GameSerializer();
            var conf = new FileDbConfiguration();
            conf.DefineCollection<Game>("Game");
            s.Def = new CollectionDefinition<Game>(conf, "GameDatabase");
            try
            {
                Game = (Game)s.Deserialize(@"c:\programming\test\o8g\definition.xml");
                WindowLocator.MainViewModel.Title = "OCTIDE - " + Game.Name;
                Name = Game.Name;
            }
            catch (System.Exception e)
            {
                
            }
        }
    }
}