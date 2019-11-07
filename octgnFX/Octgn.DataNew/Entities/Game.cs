﻿namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Game : IEqualityComparer<Game>, IEquatable<Game>, IComparable<Game>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string GameUrl { get; set; }
        public string SetsUrl { get; set; }
        public string IconUrl { get; set; }
        public string Filename { get; set; }
        public int MarkerSize { get; set; }
        public Dictionary<string, GameMarker> Markers { get; set; }
        public Version Version { get; set; }
        public Version OctgnVersion { get; set; }
        //public int CardWidth { get; set; }
        //public int CardHeight { get; set; }
        //public string CardBack { get; set; }
        //public string CardFront { get; set; }
        public CardSize CardSize { get; set; }
        //public int CardCornerRadius { get; set; }
        public string FileHash { get; set; }
        public Group Table { get; set; }
        public Player Player { get; set; }
        public GlobalPlayer GlobalPlayer { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Tags { get; set; } 
        public Dictionary<string,DeckSection> DeckSections { get; set; }
        public Dictionary<string,DeckSection> SharedDeckSections { get; set; }
        public List<PropertyDef> CustomProperties { get; set; }
        public List<GlobalVariable> GlobalVariables { get; set; }
        public Font ChatFont { get; set; }
        public Font ContextFont { get; set; }
        public Font NoteFont { get; set; }
        public Font DeckEditorFont { get; set; }
        public List<GamePhase> Phases { get; set; }
        public List<Document> Documents { get; set; }
        public List<Symbol> Symbols { get; set; }
        public Dictionary<string, CardSize> CardSizes { get; set; }
        public Dictionary<string, GameBoard> GameBoards { get; set; } 
        public List<string> Scripts { get; set; } 
        public Dictionary<string,GameSound> Sounds { get; set; }
        public Dictionary<string,GameEvent[]> Events { get; set; }
        public string InstallPath { get; set; }
        public string ProxyGenSource { get; set; }
        public bool UseTwoSidedTable { get; set; }
        public bool ChangeTwoSidedTable { get; set; }
        public string NoteBackgroundColor { get; set; }
        public string NoteForegroundColor { get; set; }
        public Version ScriptVersion { get; set; }
        public List<GameMode> Modes { get; set; } 

        public bool Equals(Game x, Game y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Game obj)
        {
            return Id.GetHashCode();
        }

        public bool Equals(Game other)
        {
            return Id == other.Id;
        }

        public int CompareTo(Game other)
        {
            return Id.CompareTo(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (obj is Game) return (obj as Game).Id == Id;
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}