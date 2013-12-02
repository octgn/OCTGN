namespace Octide.ViewModel
{
    using System;
    using System.Linq;
    using System.Windows.Media;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;

    using Octgn.DataNew.Entities;

    public class GameTabViewModel : ViewModelBase
    {
        public string Name
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.Name;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.Name) return;
                ViewModelLocator.GameLoader.Game.Name = value;
				RaisePropertyChanged("Name");
				ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public string Description
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.Description;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.Description) return;
                ViewModelLocator.GameLoader.Game.Description = value;
                RaisePropertyChanged("Description");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public string GameUrl
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.GameUrl;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.GameUrl) return;
                ViewModelLocator.GameLoader.Game.GameUrl= value;
                RaisePropertyChanged("GameUrl");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public string IconUrl
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.IconUrl;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.IconUrl) return;
                ViewModelLocator.GameLoader.Game.IconUrl = value;
                RaisePropertyChanged("IconUrl");
				ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public int MarkerSize
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.MarkerSize;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.MarkerSize) return;
                ViewModelLocator.GameLoader.Game.MarkerSize = value;
                RaisePropertyChanged("MarkerSize");
				ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public string Version
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.Version.ToString();
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.Version.ToString()) return;
                Version v;
                if (System.Version.TryParse(value, out v))
                {
                    ViewModelLocator.GameLoader.Game.Version = v;
                }
				RaisePropertyChanged("Version");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public string Authors
        {
            get
            {
                return String.Join(", ",ViewModelLocator.GameLoader.Game.Authors);
            }
            set
            {
                var list = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x=>!String.IsNullOrWhiteSpace(x)).ToList();
                ViewModelLocator.GameLoader.Game.Authors = list;
                RaisePropertyChanged("Authors");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public string Tags
        {
            get
            {
                return String.Join(", ", ViewModelLocator.GameLoader.Game.Tags);
            }
            set
            {
                var list = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
                ViewModelLocator.GameLoader.Game.Tags = list;
                RaisePropertyChanged("Tags");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public bool UseTwoSidedTable
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.UseTwoSidedTable;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.UseTwoSidedTable) return;
                ViewModelLocator.GameLoader.Game.UseTwoSidedTable = value;
                RaisePropertyChanged("UseTwoSidedTable");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public Color NoteBackgroundColor
        {
            get
            {
                return (Color)ColorConverter.ConvertFromString(ViewModelLocator.GameLoader.Game.NoteBackgroundColor);
            }
            set
            {
                ViewModelLocator.GameLoader.Game.NoteBackgroundColor = new ColorConverter().ConvertToString(value);
                RaisePropertyChanged("NoteBackgroundColor");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public Color NoteForegroundColor
        {
            get
            {
                return (Color)ColorConverter.ConvertFromString(ViewModelLocator.GameLoader.Game.NoteForegroundColor);
            }
            set
            {
                ViewModelLocator.GameLoader.Game.NoteForegroundColor = new ColorConverter().ConvertToString(value);
                RaisePropertyChanged("NoteForegroundColor");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public GameTabViewModel()
        {
			Messenger.Default.Register<PropertyChangedMessage<Game>>(this,
			    x =>
			    {
					this.RaisePropertyChanged(string.Empty);
			    });
        }
    }
}