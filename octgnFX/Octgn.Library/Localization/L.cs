using System;
using System.Threading;

namespace Octgn.Library.Localization
{
    public static class L
    {
        private static readonly ILanguageDictionary _defaultDictionary = new EnglishLanguageDictionary();
        private static ILanguageDictionary _d = _defaultDictionary;
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        public static event Action OnLanguageChanged;

        /// <summary>
        /// Gets the current localization dictionary
        /// Setting this updates all of octgn, so don't call this unless you really mean it
        /// </summary>
        public static ILanguageDictionary D
        {
            get
            {
                try
                {
                    _lock.EnterReadLock();
                    return _d;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _lock.EnterWriteLock();
                    if (_d == value) return;
                    _d = value;
                    FireOnLanguageChanged();
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public static string GetFromDefault(string key)
        {
            var property = typeof (ILanguageDictionary).GetProperty(key);
            if (property == null)
                return null;

            return (string)property.GetValue(_defaultDictionary, null);
        }

        private static void FireOnLanguageChanged()
        {
            Action handler = OnLanguageChanged;
            if (handler != null) handler();
        }
    }
}