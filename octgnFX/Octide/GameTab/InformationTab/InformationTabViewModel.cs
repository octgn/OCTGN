// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using System;
using System.Linq;

namespace Octide.ViewModel
{
    public class GameInformationTabViewModel : ViewModelBase
    {
        public GameInformationTabViewModel()
        {
            Messenger.Default.Register<PropertyChangedMessage<Game>>(this,
                x =>
                {
                    RaisePropertyChanged(string.Empty);
                });
        }

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
                ViewModelLocator.GameLoader.Game.GameUrl = value;
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

        public string Version
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.Version.ToString();
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.Version.ToString()) return;
                if (System.Version.TryParse(value, out Version v))
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
                return String.Join(", ", ViewModelLocator.GameLoader.Game.Authors);
            }
            set
            {
                var list = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
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
    }
}