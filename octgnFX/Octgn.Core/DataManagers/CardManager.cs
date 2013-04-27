namespace Octgn.Core.DataManagers
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Octgn.DataNew.Entities;

    using log4net;

    public class CardManager
    {
        #region Singleton
        private static CardManager Context { get; set; }
        private static object locker = new object();
        public static CardManager Get()
        {
            lock (locker) return Context ?? (Context = new CardManager());
        }
        internal CardManager()
        {

        }
        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public Card GetCardById(Guid id)
        {
            var set = SetManager.Get().Sets.FirstOrDefault(x => x.Cards.Any(y => y.Id == id));
            if (set == null) return null;
            return set.Cards.FirstOrDefault(x => x.Id == id);
        }
    }
}