namespace Octgn.Online.Library.Models
{
    using System;
    using System.Collections.Generic;

    public class HostedGameCard
    {
        public Guid Id { get; set; }
        public int InGameId { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public Guid Alternate { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsFaceUp { get; set; }
        /// <summary>
        /// Not sure what this is.
        /// TODO Figure this one out.
        /// </summary>
        public bool Persist { get; set; }
        public SortedList<string, object> Properties { get; set; } 
    }

    public class HostedGameGroup
    {
        public int Id { get; set; }
        public int Owner { get; set; }
        public string Name { get; set; }
        public int[] VisibleTo { get; set; }
        public bool Ordered { get; set; }
        public List<HostedGameCard> Cards { get; set; } 
    }
}