using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Octgn.ChatService.Data
{
    [Table("UserSubscriptions")]
    public class UserSubscriptionModel : Octgn.Communication.Modules.SubscriptionModule.UserSubscription
    {
        [Key]
        [Column("Id")]
        public int DBID { get; set; }

        [Index("IX_UserIdAndSubscriberUserId", 1, IsUnique = true)]
        new public string UserId { get; set; }
        [Index("IX_UserIdAndSubscriberUserId", 2, IsUnique = true)]
        new public string SubscriberUserId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        public UserSubscriptionModel() {
        }

        public UserSubscriptionModel(Octgn.Communication.Modules.SubscriptionModule.UserSubscription sub) {
            this.Category = sub.Category;
            this.Id = sub.Id;
            if (!string.IsNullOrWhiteSpace(this.Id))
                this.DBID = int.Parse(this.Id);

            this.SubscriberUserId = sub.SubscriberUserId;
            this.UpdateType = sub.UpdateType;
            this.UserId = sub.UserId;
        }
    }
}
