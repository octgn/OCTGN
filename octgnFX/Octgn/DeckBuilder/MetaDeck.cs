using System.Drawing;
using System.IO;
using System.Linq;
using log4net;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Octgn.DeckBuilder
{
    using System.ComponentModel;

    using Octgn.Annotations;
    using Octgn.Core.DataManagers;

    public class MetaDeck : IDeck, INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool isVisible;

        public string Path { get; set; }
        public string Name { get; set; }
        public Guid GameId { get; private set; }
        public bool IsShared { get; set; }
        public string Notes { get; set; }
        public string CardBack { get; set; }
        public IEnumerable<ISection> Sections { get; private set; }
        public IEnumerable<ISection> NonEmptySections { get; set; }
        public bool IsGameInstalled { get; set; }
        public bool IsCorrupt { get; set; }

        public bool IsVisible
        {
            get
            {
                return this.isVisible;
            }
            set
            {
                if (value.Equals(this.isVisible))
                {
                    return;
                }
                this.isVisible = value;
                this.OnPropertyChanged("IsVisible");
            }
        }

        public MetaDeck()
        {
            IsVisible = true;
            IDeck d = null;
            this.Path = "c:\\test.o8g";
            this.Name = new FileInfo(Path).Name.Replace(new FileInfo(Path).Extension, "");
            CardBack = "../Resources/Back.jpg";
        }

        public MetaDeck(string path)
        {
            IDeck d = null;
            this.Path = path;
            this.Name = new FileInfo(Path).Name.Replace(new FileInfo(Path).Extension,"");
            CardBack = "pack://application:,,,/Resources/Back.jpg";
            try
            {
                d = this.Load(path,false);
            }
            catch (Exception e)
            {
                Log.Warn("New MetaDeck Error",e);
                IsCorrupt = true;
            }
            if (d == null) return;
            this.GameId = d.GameId;
            this.IsShared = d.IsShared;
            this.Notes = d.Notes ?? "";
            this.Sections = d.Sections.Select(x=>new Section()
                                                 {
                                                     Name = x.Name,
                                                     Shared = x.Shared,
                                                     Cards = x.Cards.Select(y=>new MetaMultiCard(y)).ToArray()
                                                 }).ToArray();
            this.NonEmptySections = this.Sections.Where(x => x.Quantity > 0).ToArray();
            this.CardBack = GameManager.Get().GetById(this.GameId).CardBack;
        }

        public void UpdateFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                IsVisible = true;
                return;
            }
            filter = filter.ToLowerInvariant();
            if (Name.ToLowerInvariant().Contains(filter))
            {
                IsVisible = true;
                return;
            }
            if (this.IsCorrupt)
            {
                IsVisible = false;
                return;
            }
            if (Notes.ToLowerInvariant().Contains(filter))
            {
                IsVisible = true;
                return;
            }
            if (Sections.SelectMany(x => x.Cards).Any(x => x.Name.ToLowerInvariant().Contains(filter)))
            {
                IsVisible = true;
                return;
            }
            IsVisible = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class MetaMultiCard : IMultiCard, INotifyPropertyChanged
    {
        private bool isVisible;

        public bool IsVisible
        {
            get
            {
                return this.isVisible;
            }
            set
            {
                if (value.Equals(this.isVisible))
                {
                    return;
                }
                this.isVisible = value;
                this.OnPropertyChanged("IsVisible");
            }
        }

        public MetaMultiCard(IMultiCard card)
        {
            Id = card.Id;
            SetId = card.SetId;
            Name = card.Name;
            ImageUri = card.ImageUri;
            Alternate = card.Alternate;
            Properties = card.Properties;
            Quantity = card.Quantity;
            IsVisible = true;
        }

        public Guid Id { get; private set; }
        public Guid SetId { get; private set; }
        public string Name { get; private set; }
        public string ImageUri { get; private set; }
        public string Alternate { get; private set; }
        public IDictionary<string, CardPropertySet> Properties { get; private set; }
        public int Quantity { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
