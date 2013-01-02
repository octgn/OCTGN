namespace Octgn.Data.Database.Entities
{
    using FluentNHibernate.Mapping;

    using NHibernate.Type;

    public class MarkerMap : ClassMap<Entities.Marker>
    {
        public MarkerMap()
        {
            this.Table("markers");
            this.Id(x => x.Read_Id);
            this.Map(x => x.Id).CustomType<GuidType>().Unique();
            this.Map(x => x.Game_Id);
            this.References<Entities.Set>(x => x.Set_Real_Id);
            this.Map(x => x.Name);
            this.Map(x => x.Icon);
        }
    }
}
