namespace Octgn.Data.Database.Entities
{
    using System;
    using System.Collections.Generic;

    public class Set
    {
        public virtual int Real_Id { get; set; }
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int Game_Real_Id { get; set; }
        public virtual Version Game_Version { get; set; }
        public virtual Version Version { get; set; }
        public virtual string Package { get; set; }
        public virtual IList<Card> Cards { get; set; }
        public virtual IList<Marker> Markers { get; set; }
        public virtual IList<Pack> Packs { get; set; } 
    }
}
