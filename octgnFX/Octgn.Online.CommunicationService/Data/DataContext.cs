using System.Data.Entity;

namespace Octgn.ChatService.Data
{
    public class DataContext : DbContext
    {
        public DbSet<UserSubscriptionModel> UserSubscriptions { get; set; }

        public DataContext() : this("octgnContext") {

        }

        public DataContext(string connectionString) : base(connectionString) {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            var entity = modelBuilder.Entity<UserSubscriptionModel>();

            entity
                .Ignore(sub => sub.Id);

            entity.HasKey(e => new { e.UserId, e.SubscriberUserId });

            base.OnModelCreating(modelBuilder);
        }
    }
}
