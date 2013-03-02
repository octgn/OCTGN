namespace Octgn.Online.Library.Models
{
    using System;
    using System.Collections.Generic;

    public class HostedGameDeck
    {
        public Guid GameId { get; set; }

        public bool IsShared { get; set; }

        public int CardCount { get; set; }

        public List<HostedGameDeckSection> Sections { get; set; } 
    }
}