namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;

    public interface ICard
    {
        Guid Id { get; }
        Guid SetId { get; }
        string Name { get; }
        string ImageUri { get; }
        string Alternate { get; }
        IDictionary<string , CardPropertySet> Properties { get; } 
    }

    public class Card : ICard
    {
        public Guid Id { get; set; }

        public Guid SetId { get; set; }

        public string Name { get; set; }

        public string ImageUri { get; set; }

        public string Alternate { get; set; }

        public IDictionary<string , CardPropertySet> Properties { get; set; }
    }

    public class CardPropertySet
    {
        public string Type { get; set; }
        public IDictionary<PropertyDef, object> Properties { get; set; }
    }
}