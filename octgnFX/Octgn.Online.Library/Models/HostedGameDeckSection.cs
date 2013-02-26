namespace Octgn.Online.Library.Models
{
    using System.Collections.Generic;

    public class HostedGameDeckSection
    {
        public string Name { get; set; }

        public List<HostedGameCard> Cards { get; set; }

        public int CardCount { get; set; }
    }
}