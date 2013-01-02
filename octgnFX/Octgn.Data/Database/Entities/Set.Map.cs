namespace Octgn.Data.Database.Entities
{
    using FluentNHibernate.Mapping;

    using NHibernate.Type;

    using Octgn.Data.Database.UserTypes;

    public class SetMap : ClassMap<Entities.Set>
    {
        public SetMap()
        {
            this.Table("sets");
            this.Id(x => x.Real_Id);
            this.Map(x => x.Id).CustomType<GuidType>().Unique();
            this.Map(x => x.Name);
            this.References<Entities.Game>(x => x.Game_Real_Id);
            this.Map(x => x.Game_Version).CustomType<VersionType>();
            this.Map(x => x.Version).CustomType<VersionType>();
            this.Map(x => x.Package).Nullable();
            HasMany(x => x.Cards).Inverse().Cascade.All();
            HasMany(x => x.Markers).Inverse().Cascade.All();
            HasMany(x => x.Packs).Inverse().Cascade.All();
        }
    }
}
