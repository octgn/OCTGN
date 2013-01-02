namespace Octgn.Data.Database.Entities
{
    using System;

    public class Marker
    {
        public virtual int Read_Id { get; set; }
        public virtual Guid Id { get; set; }
        public virtual string Game_Id { get; set; }
        public virtual int Set_Real_Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Icon { get; set; }
    }
}
