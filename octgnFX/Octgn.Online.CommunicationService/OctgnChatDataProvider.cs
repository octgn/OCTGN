using Octgn.ChatService.Data;
using Octgn.Communication;
using Octgn.Communication.Modules.SubscriptionModule;
using System;
using System.Collections.Generic;
using System.Linq;

using UserSubscription = Octgn.Communication.Modules.SubscriptionModule.UserSubscription;
using UserSubscriptionDB = Octgn.ChatService.Data.UserSubscriptionModel;

namespace Octgn
{
    public class OctgnChatDataProvider : IDataProvider
    {
        public string GameServerName { get; set; }

        public OctgnChatDataProvider(string gameServerName) {
            GameServerName = gameServerName;

            using (var context = new DataContext()) {
                context.UserSubscriptions.Count();
            }
        }

        public event EventHandler<UserSubscriptionUpdatedEventArgs> UserSubscriptionUpdated;

        protected void FireUserSubscriptionUpdated(UserSubscription sub) {
            try {
                var args = new UserSubscriptionUpdatedEventArgs() {
                    Subscription = sub
                };
                UserSubscriptionUpdated?.Invoke(this, args);
            } catch (Exception ex) {
                Signal.Exception(ex, nameof(FireUserSubscriptionUpdated));
            }
        }

        public void AddUserSubscription(UserSubscription subscription) {
            var dbsub = new UserSubscriptionDB(subscription);

            // No other values are valid
            dbsub.Id = null;
            dbsub.DBID = 0;

            using(var context = new DataContext()) {
                context.UserSubscriptions.Add(dbsub);
            }

            dbsub.UpdateType = UpdateType.Add;

            FireUserSubscriptionUpdated(dbsub);
        }

        public IEnumerable<string> GetUserSubscribers(string userId) {
            string[] subscriberIds = null;
            using(var context = new DataContext()) {

                subscriberIds = context.UserSubscriptions
                    .Where(x => x.UserId == userId)
                    .Select(x=>x.SubscriberUserId)
                    .ToArray();
            }

            return subscriberIds;
        }

        public IEnumerable<UserSubscription> GetUserSubscriptions(string userId) {
            var subscriberId = userId;

            UserSubscriptionDB[] subscriptions = null;
            using (var context = new DataContext()) {
                subscriptions = context.UserSubscriptions.Where(x => x.SubscriberUserId == subscriberId).ToArray();
            }

            return subscriptions
                .ToArray();
        }

        public void RemoveUserSubscription(string subscriptionId) {
            var subid = int.Parse(subscriptionId);

            UserSubscriptionDB subscription = null;
            using (var context = new DataContext()) {
                subscription = context.UserSubscriptions.Single(sub => sub.DBID == subid);
                context.UserSubscriptions.Remove(subscription);
                context.SaveChanges();
            }

            subscription.UpdateType = UpdateType.Remove;

            FireUserSubscriptionUpdated(subscription);
        }

        public void UpdateUserSubscription(UserSubscription subscription) {
            var subid = int.Parse(subscription.Id);

            UserSubscriptionDB dbSubscription = null;

            using (var context = new DataContext()) {
                dbSubscription = context.UserSubscriptions.Single(sub => sub.DBID == subid);

                dbSubscription.Category = subscription.Category;

                context.SaveChanges();
            }

            dbSubscription.UpdateType = UpdateType.Update;

            FireUserSubscriptionUpdated(dbSubscription);
        }

        public UserSubscription GetUserSubscription(string subscriptionId) {
            var subid = int.Parse(subscriptionId);
            using (var context = new DataContext()) {
                return context.UserSubscriptions.Single(x => x.DBID == subid);
            }
        }
    }
}
