// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octgn.Library;
using Octide.ItemModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Octide.ViewModel
{
    public class GameEventTabViewModel : ViewModelBase
    {
        public Dictionary<Version, GameEventDescriptionItemModel[]> AllEvents { get; private set; }
        public IdeCollection<IdeBaseItem> Events { get; private set; }
        public RelayCommand AddEventCommand { get; private set; }

        private Version CurrentVersion => ViewModelLocator.GameLoader.Game.ScriptVersion;
        public GameEventDescriptionItemModel[] RegisteredEvents => AllEvents[CurrentVersion];

        public GameEventTabViewModel()
        {
            AllEvents = new Dictionary<Version, GameEventDescriptionItemModel[]>();
            using (var gameEventsXml = Assembly.GetAssembly(typeof(Paths)).GetManifestResourceStream("Octgn.Library.Scripting.GameEvents.xml"))
            {
                XDocument doc = XDocument.Load(gameEventsXml);
                foreach (var versionelement in doc.Root.Elements().Where(x => x.Name.LocalName == "eventversion"))
                {
                    var apiVersion = Version.Parse(versionelement.Attribute("version").Value);
                    var eventList = versionelement.Elements()
                        .Where(x => x.Name.LocalName == "event")
                        .Select(x => new GameEventDescriptionItemModel()
                        {
                            Name = x.Attribute("name").Value,
                            Description = x.Attribute("hint")?.Value,
                            ApiVersion = CurrentVersion,
                            Arguments = x.Elements().Where(y => y.Name.LocalName == "param")
                            .Select(y => new GameEventArgumentItemModel()
                            {
                                Name = y.Attribute("name").Value,
                                Description = y.Attribute("hint")?.Value,
                                Type = y.Attribute("type").Value
                            }).ToList()
                        })
                        .ToArray();
                    AllEvents.Add(apiVersion, eventList);
                }
            };

            Events = new IdeCollection<IdeBaseItem>();
            foreach (var gameEventType in ViewModelLocator.GameLoader.Game.Events)
            {
                foreach (var gameEvent in gameEventType.Value)
                {
                    Events.Add(new GameEventItemModel(gameEvent, Events));
                }
            }

            Events.CollectionChanged += (a, b) =>
            {
                var items =
                ViewModelLocator.GameLoader.Game.Events = Events.GroupBy(x => ((GameEventItemModel)x)._gameEvent.Name)
                                                                .ToDictionary(
                                                                    x => x.Key,
                                                                    y => y.Select(z => ((GameEventItemModel)z)._gameEvent).ToArray());
            };
            AddEventCommand = new RelayCommand(AddGameEvent);

        }

        public void AddGameEvent()
        {
            var ret = new GameEventItemModel(Events);
            Events.Add(ret);
        }
    }

}