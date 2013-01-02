namespace Octgn.Data.Database.Entities
{
    using System;
    using System.Collections.Generic;

    public class Card
    {
        public virtual int Real_Id { get; set; }
        public virtual Guid Id { get; set; }
        public virtual Guid Game_Id { get; set; }
        public virtual int Set_Real_Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Image { get; set; }
        public virtual string Alternate { get; set; }
        public virtual string Dependent { get; set; }
        public virtual IList<CustomProperty> CustomProperties { get; set; }
    }
}
