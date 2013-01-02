namespace Octgn.Data.Database.Entities
{
    using FluentNHibernate.Mapping;

    using NHibernate.Type;

    public class CardMap : ClassMap<Entities.Card>
    {
        public CardMap()
        {
            this.Table("cards");
            this.Id(x => x.Real_Id);
            this.Map(x => x.Id).CustomType<GuidType>().Unique();
            this.Map(x => x.Game_Id).CustomType<GuidType>();
            this.References<Entities.Set>(x => x.Set_Real_Id);
            this.Map(x => x.Name);
            this.Map(x => x.Image);
            this.Map(x => x.Alternate).Nullable();
            this.Map(x => x.Dependent).Nullable();
            HasMany(x => x.CustomProperties)
                .Inverse()
                .Cascade.All();
        }
    }
}
