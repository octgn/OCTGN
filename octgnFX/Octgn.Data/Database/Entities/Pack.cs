namespace Octgn.Data.Database.Entities
{
    using System;

    public class Pack
    {
        public virtual int Real_Id { get; set; }
        public virtual Guid Id { get; set; }
        public virtual int Set_Real_Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Xml { get; set; }
    }
}
