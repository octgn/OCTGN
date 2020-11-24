using Octgn.Core;
using Octgn.DataNew.Entities;
using System;

namespace Octgn.GameWizard.Models
{
    public class NewGame : ViewModelBase
    {
        public string Id {
            get => _id;
            set => SetAndNotify(ref _id, value);
        }

        private string _id = Guid.NewGuid().ToString().ToUpper();

        public string Name {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        private string _name = string.Empty;

        public string Version {
            get => _version;
            set => SetAndNotify(ref _version, value);
        }

        private string _version = "1.0.0.0";

        public string Description {
            get => _description;
            set => SetAndNotify(ref _description, value);
        }

        private string _description = string.Empty;

        public string Authors {
            get => _authors;
            set => SetAndNotify(ref _authors, value);
        }

        private string _authors = string.Empty;

        public string Url {
            get => _url;
            set => SetAndNotify(ref _url, value);
        }

        private string _url = string.Empty;

        public string ImageUrl {
            get => _imageUrl;
            set => SetAndNotify(ref _imageUrl, value);
        }

        private string _imageUrl = "https://raw.githubusercontent.com/octgn/OCTGN/master/octgnFX/Graphics/logo%20plain.png";

        public bool IsDualSided {
            get => _isDualSided;
            set => SetAndNotify(ref _isDualSided, value);
        }

        private bool _isDualSided = true;

        public string Directory {
            get => _directory;
            set => SetAndNotify(ref _directory, value);
        }

        private string _directory;
    }
}
