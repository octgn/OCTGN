namespace Octgn.Data.Database.Entities
{
    using FluentNHibernate.Mapping;

    using NHibernate.Type;

    public class PackMap : ClassMap<Entities.Pack>
    {
        public PackMap()
        {
            this.Table("packs");
            this.Id(x => x.Real_Id);
            this.Map(x => x.Id).CustomType<GuidType>().Unique();
            this.References<Entities.Set>(x => x.Set_Real_Id);
            this.Map(x => x.Name);
            this.Map(x => x.Xml);
        }
    }
}
