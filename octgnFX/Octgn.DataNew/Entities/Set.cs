namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;
    public class Set:IEqualityComparer<Set>,IEquatable<Set>,IComparable<Set>,IComparer<Set>
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string Name { get; set; }
        public Version GameVersion { get; set; }
        public Version Version { get; set; }
        public string Filename { get; set; }
        public string PackageName { get; set; }
        public IEnumerable<Pack> Packs { get; set; }
        public IEnumerable<Marker> Markers { get; set; }
        public IEnumerable<Card> Cards { get; set; }
        public string InstallPath { get; set; }
        public string DeckPath { get; set; }
        public string PackUri { get; set; }
        public string ImageInstallPath { get; set; }
        public string ImagePackUri { get; set; }
        public string ProxyPackUri { get; set; }

        public bool Equals(Set x, Set y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Set obj)
        {
            return Id.GetHashCode();
        }

        public bool Equals(Set other)
        {
            return this.Id.Equals(other.Id);
        }

        public int CompareTo(Set other)
        {
            return this.Id.CompareTo(other.Id);
        }

        public int Compare(Set x, Set y)
        {
            return x.Id.CompareTo(y.Id);
        }
    }
}