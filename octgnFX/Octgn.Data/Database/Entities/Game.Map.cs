namespace Octgn.Data.Database.Entities
{
    using System;

    using FluentNHibernate.Mapping;

    using NHibernate.Type;

    using Octgn.Data.Database.UserTypes;

    public class GameMap : ClassMap<Entities.Game>
    {
        public GameMap()
        {
            this.Table("games");
            this.Id(x => x.Real_Id).GeneratedBy.Increment();
            this.Map(x => x.Id).CustomType<GuidType>().Unique();
            this.Map(x => x.Name);
            this.Map(x => x.Filename);
            this.Map(x => x.Version).CustomType<VersionType>();
            this.Map(x => x.Card_Width);
            this.Map(x => x.Card_Height);
            this.Map(x => x.Card_Back).Nullable();
            this.Map(x => x.Deck_Sections).Nullable();
            this.Map(x => x.Shared_Deck_Sections).Nullable();
            this.Map(x => x.File_Hash).Nullable();
            HasMany(x => x.Sets)
                .KeyColumn("game_real_id");
        }
    }
}
