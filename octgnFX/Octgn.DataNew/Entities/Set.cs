namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;

    public class Set
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string Name { get; set; }
        public Version GameVersion { get; set; }
        public Version Version { get; set; }
        public string Filename { get; internal set; }
        public string PackageName { get; set; }
        public IEnumerable<Pack> Packs { get; set; }
        public IEnumerable<Marker> Markers { get; set; } 
    }
}