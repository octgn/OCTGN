namespace Octgn.Data.Database.Entities
{
    using FluentNHibernate.Mapping;

    public class UserMap : ClassMap<Entities.User>
    {
        public UserMap()
        {
            this.Table("users");
            this.Id(x => x.Id);
            this.Map(x => x.Jid);
            this.Map(x => x.Email);
        }
    }
}
