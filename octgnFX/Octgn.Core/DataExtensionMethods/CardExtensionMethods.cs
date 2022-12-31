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

        /// <summary>
        /// Returns the Octgn.DataNew.Set that this card belongs to.
        /// </summary>
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

        /// <summary>
        /// Converts this card into a MultiCard object, which extends to support a Quantity property
        /// </summary>
        /// <param name="quantity">The starting quantity of this card.<para>(default is 1)</para></param>
        /// <param name="clone">(unused, defaults to true)</param>
        public static MultiCard ToMultiCard(this ICard card, int quantity = 1, bool clone = true)
        {
            var ret = new MultiCard(card, quantity);
            return ret;
        }

        /// <summary>
        /// Returns the image file for this card in the image database. If there is no image, generate a proxy generated image and return that file path instead.
        /// <para>Respects this card's current alternate state.</para>
        /// </summary>
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

        /// <summary>
        /// Returns the full file path of the proxy generated image for this card.  Will generate a new image if there isn't one cached.
        /// <para>Respects this card's current alternate state.</para>
        /// </summary>
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

        /// <summary>
        /// Returns the filename (no extension) for this card's image, including it's alternate state.
        /// </summary>
        public static string GetImageUri(this ICard card)
        {
            var ret = card.ImageUri;
            if (!String.IsNullOrWhiteSpace(card.Alternate)) ret = ret + "." + card.Alternate;
            return ret;
        }

        /// <summary>
        /// Checks if the given property is defined in this card.
        /// <para>Returns true if the property is defined.</para>
        /// <para>Returns false if the property is missing or its value is null.</para>
        /// </summary>
        /// <param name="name">The property's name</param>
        public static bool HasProperty(this Card card, string name)
        {
            return card.GetCardProperties().Any(x => x.Key.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Returns the value of a specified property on this card.
        /// <para>Respects this card's current alternate state.</para>
        /// </summary>
        /// <param name="name">The name of the property to check.</param>
        public static object GetProperty(this Card card, string name)
        {
            return GetProperty(card, name, null);
        }

        /// <summary>
        /// Returns the value of a given property on this card.
        /// <para>Ignores this card's current alternate state in favor of the specified one.</para>
        /// </summary>
        /// <param name="name">The name of the property to check.</param>
        /// <param name="alternate">The alternate ID to check.</param>
        public static object GetProperty(this Card card, string name, string alternate)
        {
            return card.GetCardProperties(alternate)?.FirstOrDefault(x => x.Key.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Value;
        }

        /// <summary>
        /// Creates a simplified mapping of this card's properties for use by the proxy generator.
        /// </summary>
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

        /// <summary>
        /// Returns the base properties of this card, ignoring the card's current alternate state.
        /// </summary>
        public static IDictionary<PropertyDef, object> GetBaseCardProperties(this ICard card)
        {
            return GetCardProperties(card, "");
        }

        /// <summary>
        /// Returns the properties of this card, respecting its current alternate state.
        /// </summary>
        public static IDictionary<PropertyDef, object> GetCardProperties(this ICard card)
        {
            return GetCardProperties(card, null);
        }

        /// <summary>
        /// Returns all defined properties for this card, from the specified alternate.
        /// <para>It will return null if the card does not have a defined property set for the specified alternate.</para>
        /// </summary>
        /// <param name="alt">The alternate ID to use when looking for properties.</param>
        public static IDictionary<PropertyDef, object> GetCardProperties(this ICard card, string alt)
        {
            if (alt == null)
                alt = card.Alternate;
            if (!card.PropertySets.ContainsKey(alt))
                return null;
            var ret = card.PropertySets[""].Properties.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
            if (alt != "" && card.PropertySets.ContainsKey(alt))
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


        [Obsolete("The PropertySet method will soon be deprecated. Use GetCardProperties() instead.")]
        public static IDictionary<PropertyDef, object> PropertySet(this ICard card) => GetCardProperties(card);

        /// <summary>
        /// Returns the full property dictionary for this card, respecting its current alternate state.
        /// <para>Includes this card's name as a custom property.</para>
        /// </summary>
        /// 
        [Obsolete("The GetFullCardProperties method will soon be deprecated.  It no longer considers the card's Name as a custom property.")]
        public static IDictionary<PropertyDef, object> GetFullCardProperties(this ICard card)
        {
            return GetCardProperties(card);
        }

        /// <summary>
        /// Checks to see if a specified property value on this card is the same as the given value.
        /// <para>Returns true if the property value is a match to the given value.</para>
        /// <para>Returns false if the property value is different.</para>
        /// </summary>
        /// <param name="prop">The custom property to check.</param>
        /// <param name="value">A value to compare with this card's property value.</param>
        public static bool MatchesPropertyValue(this ICard card, PropertyDef prop, object value)
        {
            var cardProperties = GetCardProperties(card);
            if (cardProperties.ContainsKey(prop))
            {
                if (string.IsNullOrWhiteSpace(cardProperties[prop]?.ToString()) && string.IsNullOrWhiteSpace(value?.ToString()))
                    return true;
                return cardProperties[prop].ToString().Equals(value?.ToString(), StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                // if the property is missing then its treated as null for match requests
                return string.IsNullOrWhiteSpace(value.ToString());
            }
        }
        /// <summary>
        /// Checks to see if a specified property exists on this card, and that its value matches the given value.
        /// <para>Returns true if the property value is a match to the given value.</para>
        /// <para>Returns false if the property value is different.</para>
        /// </summary>
        /// <param name="prop">The name of the custom property to check.</param>
        /// <param name="value">A value to compare with this card's property value.</param>
        public static bool MatchesPropertyValue(this ICard card, string prop, object value)
        {
            var cardProperties = GetCardProperties(card);
            if (prop.Equals("name", StringComparison.InvariantCultureIgnoreCase))
            {
                return card.GetName().Equals(value.ToString(), StringComparison.InvariantCultureIgnoreCase);
            }
            else if (prop.Equals("model", StringComparison.InvariantCultureIgnoreCase))
            {
                if (Guid.TryParse(value?.ToString(), out Guid guid))
                    return card.Id.Equals(guid);
                else
                    return false;
            }
            var matchedProperty = cardProperties.Keys.FirstOrDefault(x => x.Name.Equals(prop, StringComparison.InvariantCultureIgnoreCase));
            if (matchedProperty == null)
            {
                // if the property is missing then its treated as null for match requests
                return string.IsNullOrWhiteSpace(value?.ToString());
            }

            if (string.IsNullOrWhiteSpace(cardProperties[matchedProperty]?.ToString()) && string.IsNullOrWhiteSpace(value?.ToString()))
                return true;
            return cardProperties[matchedProperty].ToString().Equals(value?.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Sets this card's alternate state to the specified ID if it exists, then updates the card size accordingly.
        /// </summary>
        /// <param name="alternate">The new alternate ID to set this card to.</param>
        public static void SetPropertySet(this Card card, string alternate = "")
        {
            if (String.IsNullOrWhiteSpace(alternate)) alternate = "";
            if (card.PropertySets.Any(x => x.Key.Equals(alternate, StringComparison.InvariantCultureIgnoreCase)))
            {
                card.Alternate = alternate;
                card.Size = card.PropertySets[alternate].Size;
            }
        }

        [Obsolete("The PropertyName method will soon be deprecated. Use GetName() instead.")]
        public static string PropertyName(this ICard card) => GetName(card);

        /// <summary>
        /// Gets the name of this card, respecting its alternate state.
        /// </summary>
        public static string GetName(this ICard card)
        {
            return card.GetName(card.Alternate);
        }

        /// <summary>
        /// Gets the name of the specified card alternate.
        /// </summary>
        /// <param name="alternate">the string identifier of the alternate</param>
        public static string GetName(this ICard card, string alternate)
        {
            if (card.PropertySets.ContainsKey(alternate))
                return card.PropertySets[alternate].Name;
            else
                return card.Name;
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