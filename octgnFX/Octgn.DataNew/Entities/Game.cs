namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Game : IEqualityComparer<Game>, IEquatable<Game>,IComparable<Game>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Filename { get; set; }
        public Version Version { get; set; }
        public int CardWidth { get; set; }
        public int CardHeight { get; set; }
        public string CardBack { get; set; }
        public string FileHash { get; set; }
        public List<string> DeckSections { get; set; }
        public List<string> SharedDeckSections { get; set; }
        public List<PropertyDef> CustomProperties { get; set; }
        public List<Font> Fonts { get; set; }
        public List<GameScript> Scripts { get; set; } 

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