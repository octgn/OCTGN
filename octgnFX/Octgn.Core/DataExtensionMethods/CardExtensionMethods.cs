/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Diagnostics;
using Octgn.Library;
using Octgn.Library.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Octgn.Core.DataManagers;
using Octgn.DataNew.Entities;

using log4net;
using Octgn.Library.Localization;

namespace Octgn.Core.DataExtensionMethods
{

    public static class CardExtensionMethods
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ReaderWriterLockSlim GetSetLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private static readonly Dictionary<Guid, Set> CardSetIndex = new Dictionary<Guid, Set>();

        public static Set GetSet(this ICard card)
        {
            try
            {
                GetSetLock.EnterUpgradeableReadLock();
                Set ret = null;
                if (!CardSetIndex.TryGetValue(card.SetId, out ret))
                {
                    GetSetLock.EnterWriteLock();
                    ret = SetManager.Get().GetById(card.SetId);
                    CardSetIndex[card.SetId] = ret;
                    GetSetLock.ExitWriteLock();
                }
                return ret;
            }
            finally
            {
                GetSetLock.ExitUpgradeableReadLock();
            }
        }
        public static MultiCard ToMultiCard(this ICard card, int quantity = 1, bool clone = true)
        {
            var ret = new MultiCard(card, quantity);
            return ret;
        }
        public static string GetPicture(this ICard card)
        {
            if (String.IsNullOrWhiteSpace(card.ImageUri) == false && card.ImageUri.StartsWith("pack://", StringComparison.InvariantCultureIgnoreCase))
            {
                return card.ImageUri;
            }
            var set = card.GetSet();

            Uri uri = null;
            
            var imageUri = card.GetImageUri();
            if (Directory.Exists(set.ImagePackUri) == false)
            {
                throw new UserMessageException(L.D.Exception__CanNotFindDirectoryGameDefBroken_Format, set.ImagePackUri);
            }
            var files = Directory.GetFiles(set.ImagePackUri, imageUri + ".*").Where(x => Path.GetFileNameWithoutExtension(x).Equals(imageUri, StringComparison.InvariantCultureIgnoreCase)).OrderBy(x => x.Length).ToArray();
            if (files.Length == 0) //Generate or grab proxy
            {
                files = Directory.GetFiles(set.ProxyPackUri, imageUri + ".png");
                if (files.Length != 0)
                {
                    uri = new Uri(files.First());
                }
            }
            else
                uri = new Uri(files.First());

            if (uri == null)
            {
                uri = new System.Uri(Path.Combine(set.ProxyPackUri, imageUri + ".png"));
                card.GenerateProxyImage(set, uri.LocalPath);
            }
            return uri.LocalPath;
        }

        public static string GetProxyPicture(this ICard card)
        {
            var set = card.GetSet();

            Uri uri = new System.Uri(Path.Combine(set.ProxyPackUri, card.GetImageUri() + ".png"));

            var files = Directory.GetFiles(set.ProxyPackUri, card.GetImageUri() + ".png");

            if (files.Length == 0)
            {
                card.GenerateProxyImage(set, uri.LocalPath);
                //set.GetGame().GetCardProxyDef().SaveProxyImage(card.GetProxyMappings(), uri.LocalPath);
            }

            return uri.LocalPath;
        }

        public static string GetImageUri(this ICard card)
        {
            var ret = card.ImageUri;
            if (!String.IsNullOrWhiteSpace(card.Alternate)) ret = ret + "." + card.Alternate;
            return ret;
        }

        public static bool HasProperty(this Card card, string name)
        {
            return card.GetCardProperties().Any(x => x.Key.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        private static void GenerateProxyImage(this ICard card, Set set, string uri)
        {
            string cropPath = Path.Combine(Path.Combine(set.ImagePackUri, "Crops"));
            if (Directory.Exists(cropPath))
            {
                var files = Directory.GetFiles(cropPath, card.GetImageUri() + ".*").OrderBy(x => x.Length).ToArray();
                if (files.Length == 0)
                {
                    set.GetGame().GetCardProxyDef().SaveProxyImage(card.GetProxyMappings(), uri);
                }
                else
                {
                    set.GetGame().GetCardProxyDef().SaveProxyImage(card.GetProxyMappings(), uri, files.First());
                }
            }
            else
            {
                set.GetGame().GetCardProxyDef().SaveProxyImage(card.GetProxyMappings(), uri);
            }
        }

        public static Dictionary<string, string> GetProxyMappings(this ICard card)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (KeyValuePair<PropertyDef, object> kvi in card.GetCardProperties())
            {
                ret.Add(kvi.Key.Name, kvi.Value.ToString());
            }
            ret.Add("SetName", card.GetSet().Name);
            ret.Add("Name", GetName(card));
            ret.Add("CardSizeName", card.Size.Name);
            ret.Add("CardSizeHeight", card.Size.Height.ToString());
            ret.Add("CardSizeWidth", card.Size.Width.ToString());
            return (ret);
        }

        public static IDictionary<PropertyDef, object> GetBaseCardProperties(this ICard card)
        {
            var ret = card.PropertySets[""].Properties.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
            return ret;
        }
        public static IDictionary<PropertyDef, object> GetCardProperties(this ICard card, string alt = null)
        {
            if (alt == null || !card.PropertySets.ContainsKey(alt))
                alt = card.Alternate;

            var ret = GetBaseCardProperties(card);
            if (alt != "")
            {
                foreach (var altProperty in card.PropertySets[alt].Properties)
                {
                    if (altProperty.Value == null)
                        ret.Remove(altProperty.Key);
                    else
                        ret[altProperty.Key] = altProperty.Value;
                }
            }
            return ret;
        }


        [Obsolete("The PropertySet method will soon be deprecated. Use GetFullCardProperties() instead.")]
        public static IDictionary<PropertyDef, object> PropertySet(this ICard card) => GetFullCardProperties(card);

        /// <summary>
        /// Returns the full property dictionary for this card, respecting its current alternate state.
        /// <para>Includes this card's name as a custom property.</para>
        /// </summary>
        public static IDictionary<PropertyDef, object> GetFullCardProperties(this ICard card)
        {
            var ret = GetCardProperties(card);
            ret.Add(GameExtensionMethods.NameProperty, card.Name);
            return ret;
        }

        public static bool MatchesPropertyValue(this ICard card, PropertyDef prop, object value)
        {
            var cardProperties = GetFullCardProperties(card);
            if (cardProperties.ContainsKey(prop))
            {
                if (string.IsNullOrWhiteSpace(cardProperties[prop]?.ToString()) && string.IsNullOrWhiteSpace(value?.ToString()))
                    return true;
                return cardProperties[prop].ToString().Equals(value.ToString(), StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                // if the property is missing then its treated as null for match requests
                return string.IsNullOrWhiteSpace(value.ToString());
            }
        }

        public static void SetPropertySet(this Card card, string propertyType = "")
        {
            if (String.IsNullOrWhiteSpace(propertyType)) propertyType = "";
            if (card.PropertySets.Any(x => x.Key.Equals(propertyType, StringComparison.InvariantCultureIgnoreCase)))
            {
                card.Alternate = propertyType;
                card.Size = card.PropertySets[propertyType].Size;
            }
        }

        [Obsolete("The PropertyName method will soon be deprecated. Use GetName() instead.")]
        public static string PropertyName(this ICard card) => GetName(card);

        /// <summary>
        /// Gets the name of this card, respecting its alternate state.
        /// </summary>
        public static string GetName(this ICard card)
        {
            return card.PropertySets[card.Alternate].Name;
        }

        public static MultiCard Clone(this MultiCard card)
        {
            var ret = new MultiCard(card);
            foreach (var p in card.PropertySets)
            {
                ret.PropertySets.Add(p.Key, p.Value);
            }
            return ret;
        }

        public static Card Clone(this Card card)
        {
            if (card == null) return null;
            var ret = new Card(card);
            return ret;
        }

    }
}