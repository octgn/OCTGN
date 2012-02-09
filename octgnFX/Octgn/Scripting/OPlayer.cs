using System;
using System.Security;
using Octgn.Play;

namespace Octgn.Scripting
{
    public class ScriptObject : MarshalByRefObject
    {
        [SecurityCritical]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    [SecuritySafeCritical]
    public class OPlayer : MarshalByRefObject //, IDynamicMetaObjectProvider
    {
        private readonly Player player;

        public OPlayer(Player player)
        {
            this.player = player;
        }

        public string name
        {
            get { return player.Name; }
        }

        public OCounter counters
        {
            get { return new OCounter(player); }
        }

        //[SecurityCritical]
        //public override object InitializeLifetimeService()
        //{
        //  return null;
        //}
        /*
        [System.Security.SecurityCritical]
        public override object InitializeLifetimeService()
        { 
          var lifetime = (ILease)base.InitializeLifetimeService();
          if (lifetime.CurrentState == LeaseState.Initial)
          {
            lifetime.InitialLeaseTime = TimeSpan.FromSeconds(60);
            lifetime.RenewOnCallTime = TimeSpan.FromSeconds(10);
            lifetime.SponsorshipTimeout = TimeSpan.FromSeconds(5);
          }
          return lifetime;
        }
        */
        /*
        ~OPlayer()
        {
          int i = 0;
        }
        */
        /*
        #region IDynamicMetaObjectProvider

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
          return new OPlayerMeta(parameter, this);
        }

        private class OPlayerMeta : DynamicMetaObject
        {
          public OPlayerMeta(Expression parameter, OPlayer player) : base(parameter, BindingRestrictions.Empty, player)
          { }

          public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
          {
            // Check if the property exists
            if (typeof(OPlayer).GetProperty(binder.Name) != null)
              return binder.FallbackGetMember(this);

            Expression expr = Expression.Property(Expression.Property(GetLimitedSelf(), "counters"), "values", Expression.Constant(binder.Name));
            if (!AreEquivalent(binder.ReturnType, typeof(int)))
              expr = Expression.Convert(expr, binder.ReturnType);

            return new DynamicMetaObject(expr, BindingRestrictions.GetTypeRestriction(Expression, typeof(OPlayer)));
          }

          public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
          {
            // Check if the property exists
            if (typeof(OPlayer).GetProperty(binder.Name) != null)
              return binder.FallbackSetMember(this, value);        

            Expression expr = Expression.Assign(
              Expression.Property(Expression.Property(GetLimitedSelf(), "counters"), "values", Expression.Constant(binder.Name)),
              Expression.Convert(value.Expression, typeof(int)));

            if (!AreEquivalent(binder.ReturnType, typeof(int)))
              expr = Expression.Convert(expr, binder.ReturnType);

            return new DynamicMetaObject(expr, BindingRestrictions.GetTypeRestriction(Expression, typeof(OPlayer)));
          }

          public new OPlayer Value
          { get { return (OPlayer)base.Value; } }

          private Expression GetLimitedSelf()
          {
            if (AreEquivalent(Expression.Type, LimitType))
              return Expression;
            return Expression.Convert(Expression, LimitType);
          }

          private bool AreEquivalent(Type t1, Type t2)
          {
            return t1 == t2 || t1.IsEquivalentTo(t2);
          }
        }

        #endregion
         */
    }
}