﻿namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;

    public interface IDeck
    {
        Guid GameId { get; }
        bool IsShared { get; }
        IEnumerable<ISection> Sections { get; }
    }

    public class Deck : IDeck
    {
        public Guid GameId { get; set; }
        public bool IsShared { get; set; }
        public IEnumerable<ISection> Sections { get; set; }
    }

    public class ObservableDeck : IDeck, INotifyPropertyChanged
    {
        private Guid gameId;

        private bool isShared;

        private IEnumerable<ObservableSection> sections;

        public Guid GameId
        {
            get
            {
                return this.gameId;
            }
            set
            {
                if (this.gameId == value) return;
                this.gameId = value;
                OnPropertyChanged("GameId");
            }
        }

        public bool IsShared
        {
            get
            {
                return this.isShared;
            }
            set
            {
                if (this.isShared == value) return;
                this.isShared = value;
                OnPropertyChanged("IsShared");
            }
        }

        public IEnumerable<ISection> Sections
        {
            get
            {
                return this.sections;
            }
            set
            {
                if (this.sections == value) return;
                if (value is ObservableCollection<ObservableSection>) this.sections = value as ObservableCollection<ObservableSection>;
                else
                {
                    this.sections = new ObservableCollection<ObservableSection>(value.Select(x => new ObservableSection
                                                                                                      {
                                                                                                          Cards = new ObservableCollection<IMultiCard>(x.Cards.ToArray()),
                                                                                                          Name = x.Name
                                                                                                      }));
                }
                OnPropertyChanged("Sections");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}