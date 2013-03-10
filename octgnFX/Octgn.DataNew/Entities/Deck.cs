namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;

    public class Deck
    {
        public Guid GameId { get; set; }
        public bool IsShared { get; set; }
        public IEnumerable<Section> Sections { get; set; } 
    }
}