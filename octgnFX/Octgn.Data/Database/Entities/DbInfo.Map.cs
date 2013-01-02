namespace Octgn.Data.Database.Entities
{
    using FluentNHibernate.Mapping;

    public class DbInfoMap : ClassMap<DbInfo>
    {
        public DbInfoMap()
        {
            this.Id(x => x.Version);
            this.Map(x => x.Version);
            this.Table("dbinfo");
        }
    }
}
