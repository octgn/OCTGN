namespace Octgn.Data.Entities
{
    using System;
    using System.Collections.Generic;

    public class Set
    {
        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
        public Version GameVersion { get; internal set; }
        public Version Version { get; internal set; }
        public string Filename { get; internal set; }
        public string PackageName { get; internal set; }
        public IEnumerable<Pack> Packs { get; internal set; }
        public IEnumerable<Card> Cards { get; internal set; } 
    }

    public class Card
    {
        public Guid Id { get; internal set; }

        public string Name { get; internal set; }

        public string ImageUri { get; internal set; }

        /// <summary>
        /// The location of the alternate. If none is specified, will be System.Guid.Empty
        /// </summary>
        public Guid Alternate { get; internal set; }

        /// <summary>
        /// <TODO>
        /// If not Guid.Empty.ToString(), this card will not be placed inside a deck - 
        /// The card with guid == dependent will be used instead. Mainly used in Deck Editor
        /// </TODO>
        /// </summary>
        public string Dependent { get; internal set; }

        /// <summary>
        /// <TODO>a flag; if true, this card is read-only. (and will only be instanced once</TODO>)
        /// </summary>
        public bool IsMutable { get; internal set; }

    }
}