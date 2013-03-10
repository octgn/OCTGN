namespace Octgn.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Game
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Filename { get; set; }
        public Version Version { get; set; }
        public int CardWidth { get; set; }
        public int CardHeight { get; set; }
        public string CardBack { get; set; }
        public string FileHash { get; set; }
        public IEnumerable<string> DeckSections { get; set; }
        public IEnumerable<string> SharedDeckSections { get; set; }
        public IEnumerable<PropertyDef> CustomProperties { get; set; }
        public IQueryable<Set> Sets { get; set; }
    }
}