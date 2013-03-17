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
        Guid Alternate { get; }
        string Dependent { get; }
        bool IsMutable { get; }
        IDictionary<PropertyDef, object> Properties { get; } 
    }

    public class Card : ICard
    {
        public Guid Id { get; set; }

        public Guid SetId { get; set; }

        public string Name { get; set; }

        public string ImageUri { get; set; }

        /// <summary>
        /// The location of the alternate. If none is specified, will be System.Guid.Empty
        /// </summary>
        public Guid Alternate { get; set; }

        /// <summary>
        /// <TODO>
        /// If not Guid.Empty.ToString(), this card will not be placed inside a deck - 
        /// The card with guid == dependent will be used instead. Mainly used in Deck Editor
        /// </TODO>
        /// </summary>
        public string Dependent { get; set; }

        /// <summary>
        /// <TODO>a flag; if true, this card is read-only. (and will only be instanced once</TODO>)
        /// </summary>
        public bool IsMutable { get; set; }

        public IDictionary<PropertyDef, object> Properties { get; set; }

    }
}