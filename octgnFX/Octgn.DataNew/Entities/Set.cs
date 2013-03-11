namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Db4objects.Db4o.Config.Attributes;
    [TransparentPersisted]
    public class Set
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
        public SetCards Cards { get; set; }
    }
    public class SetCards
    {
        public IEnumerable<Card> Cards { get; set; }

        public SetCards()
        {
            Cards = new List<Card>();
        }
    }
}