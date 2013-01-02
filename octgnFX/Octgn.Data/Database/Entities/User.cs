namespace Octgn.Data.Database.Entities
{
    public class User
    {
        public virtual int Id { get; set; }
        public virtual string Jid { get; set; }
        public virtual string Email { get; set; }
    }
}
