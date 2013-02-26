namespace Octgn.Online.Library.Models
{
    using System;
    using System.Collections.Generic;

    public class HostedGameCard
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public Guid Alternate { get; set; }
        public SortedList<string, object> Properties { get; set; } 
    }
}