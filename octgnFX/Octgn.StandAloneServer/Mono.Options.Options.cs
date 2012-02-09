﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;

#if LINQ
using System.Linq;
#endif

#if TEST
using NDesk.Options;
#endif

#if NDESK_OPTIONS
namespace NDesk.Options
#else

namespace Octgn.StandAloneServer
#endif
{
    internal static class StringCoda
    {
        public static IEnumerable<string> WrappedLines(string self, params int[] widths)
        {
            IEnumerable<int> w = widths;
            return WrappedLines(self, w);
        }

        public static IEnumerable<string> WrappedLines(string self, IEnumerable<int> widths)
        {
            if (widths == null)
                throw new ArgumentNullException("widths");
            return CreateWrappedLinesIterator(self, widths);
        }

        private static IEnumerable<string> CreateWrappedLinesIterator(string self, IEnumerable<int> widths)
        {
            if (string.IsNullOrEmpty(self))
            {
                yield return string.Empty;
                yield break;
            }
            using (IEnumerator<int> ewidths = widths.GetEnumerator())
            {
                bool? hw = null;
                int width = GetNextWidth(ewidths, int.MaxValue, ref hw);
                int start = 0;
                do
                {
                    int end = GetLineEnd(start, width, self);
                    char c = self[end - 1];
                    if (char.IsWhiteSpace(c))
                        --end;
                    bool needContinuation = end != self.Length && !IsEolChar(c);
                    string continuation = "";
                    if (needContinuation)
                    {
                        --end;
                        continuation = "-";
                    }
                    string line = self.Substring(start, end - start) + continuation;
                    yield return line;
                    start = end;
                    if (char.IsWhiteSpace(c))
                        ++start;
                    width = GetNextWidth(ewidths, width, ref hw);
                } while (start < self.Length);
            }
        }

        private static int GetNextWidth(IEnumerator<int> ewidths, int curWidth, ref bool? eValid)
        {
            if (!eValid.HasValue || (eValid.Value))
            {
                curWidth = (eValid = ewidths.MoveNext()).Value ? ewidths.Current : curWidth;
                // '.' is any character, - is for a continuation
                const string minWidth = ".-";
                if (curWidth < minWidth.Length)
                    throw new ArgumentOutOfRangeException("ewidths",
                                                          string.Format("Element must be >= {0}, was {1}.",
                                                                        minWidth.Length, curWidth));
                return curWidth;
            }
            // no more elements, use the last element.
            return curWidth;
        }

        private static bool IsEolChar(char c)
        {
            return !char.IsLetterOrDigit(c);
        }

        private static int GetLineEnd(int start, int length, string description)
        {
            int end = Math.Min(start + length, description.Length);
            int sep = -1;
            for (int i = start; i < end; ++i)
            {
                if (description[i] == '\n')
                    return i + 1;
                if (IsEolChar(description[i]))
                    sep = i + 1;
            }
            if (sep == -1 || end == description.Length)
                return end;
            return sep;
        }
    }

    public class OptionValueCollection : IList, IList<string>
    {
        private readonly OptionContext _c;
        private readonly List<string> _values = new List<string>();

        internal OptionValueCollection(OptionContext c)
        {
            _c = c;
        }

        #region ICollection

        void ICollection.CopyTo(Array array, int index)
        {
            (_values as ICollection).CopyTo(array, index);
        }

        bool ICollection.IsSynchronized
        {
            get { return (_values as ICollection).IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return (_values as ICollection).SyncRoot; }
        }

        #endregion

        #region ICollection<T>

        #region IList Members

        public void Clear()
        {
            _values.Clear();
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region IList<string> Members

        public void Add(string item)
        {
            _values.Add(item);
        }

        public bool Contains(string item)
        {
            return _values.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            return _values.Remove(item);
        }

        #endregion

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        #endregion

        #region IEnumerable<T>

        public IEnumerator<string> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        #endregion

        #region IList

        int IList.Add(object value)
        {
            return (_values as IList).Add(value);
        }

        bool IList.Contains(object value)
        {
            return (_values as IList).Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return (_values as IList).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            (_values as IList).Insert(index, value);
        }

        void IList.Remove(object value)
        {
            (_values as IList).Remove(value);
        }

        void IList.RemoveAt(int index)
        {
            (_values as IList).RemoveAt(index);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { (_values as IList)[index] = value; }
        }

        #endregion

        #region IList<T>

        public int IndexOf(string item)
        {
            return _values.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            _values.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _values.RemoveAt(index);
        }

        public string this[int index]
        {
            get
            {
                AssertValid(index);
                return index >= _values.Count ? null : _values[index];
            }
            set { _values[index] = value; }
        }

        private void AssertValid(int index)
        {
            if (_c.Option == null)
                throw new InvalidOperationException("OptionContext.Option is null.");
            if (index >= _c.Option.MaxValueCount)
                throw new ArgumentOutOfRangeException("index");
            if (_c.Option.OptionValueType == OptionValueType.Required &&
                index >= _values.Count)
                throw new OptionException(string.Format(
                    _c.OptionSet.MessageLocalizer("Missing required value for option '{0}'."), _c.OptionName),
                                          _c.OptionName);
        }

        #endregion

        public List<string> ToList()
        {
            return new List<string>(_values);
        }

        public string[] ToArray()
        {
            return _values.ToArray();
        }

        public override string ToString()
        {
            return string.Join(", ", _values.ToArray());
        }
    }

    public class OptionContext
    {
        private readonly OptionValueCollection _c;
        private readonly OptionSet _set;

        public OptionContext(OptionSet set)
        {
            _set = set;
            _c = new OptionValueCollection(this);
        }

        public Option Option { get; set; }

        public string OptionName { get; set; }

        public int OptionIndex { get; set; }

        public OptionSet OptionSet
        {
            get { return _set; }
        }

        public OptionValueCollection OptionValues
        {
            get { return _c; }
        }
    }

    public enum OptionValueType
    {
        None,
        Optional,
        Required,
    }

    public abstract class Option
    {
        private static readonly char[] NameTerminator = new[] {'=', ':'};
        private readonly int _count;
        private readonly string _description;
        private readonly string[] _names;
        private readonly string _prototype;
        private readonly OptionValueType _type;

        protected Option(string prototype, string description)
            : this(prototype, description, 1)
        {
        }

        protected Option(string prototype, string description, int maxValueCount)
        {
            if (prototype == null)
                throw new ArgumentNullException("prototype");
            if (prototype.Length == 0)
                throw new ArgumentException("Cannot be the empty string.", "prototype");
            if (maxValueCount < 0)
                throw new ArgumentOutOfRangeException("maxValueCount");

            _prototype = prototype;
            _description = description;
            _count = maxValueCount;
            _names = (this is OptionSet.Category)
                     // append GetHashCode() so that "duplicate" categories have distinct
                     // names, e.g. adding multiple "" categories should be valid.
                         ? new[] {prototype + GetHashCode()}
                         : prototype.Split('|');

            if (this is OptionSet.Category)
                return;

            _type = ParsePrototype();

            if (_count == 0 && _type != OptionValueType.None)
                throw new ArgumentException(
                    "Cannot provide maxValueCount of 0 for OptionValueType.Required or " +
                    "OptionValueType.Optional.",
                    "maxValueCount");
            if (_type == OptionValueType.None && maxValueCount > 1)
                throw new ArgumentException(
                    string.Format("Cannot provide maxValueCount of {0} for OptionValueType.None.", maxValueCount),
                    "maxValueCount");
            if (Array.IndexOf(_names, "<>") >= 0 &&
                ((_names.Length == 1 && _type != OptionValueType.None) ||
                 (_names.Length > 1 && MaxValueCount > 1)))
                throw new ArgumentException(
                    "The default option handler '<>' cannot require values.",
                    "prototype");
        }

        public string Prototype
        {
            get { return _prototype; }
        }

        public string Description
        {
            get { return _description; }
        }

        public OptionValueType OptionValueType
        {
            get { return _type; }
        }

        public int MaxValueCount
        {
            get { return _count; }
        }

        internal string[] Names
        {
            get { return _names; }
        }

        internal string[] ValueSeparators { get; private set; }

        public string[] GetNames()
        {
            return (string[]) _names.Clone();
        }

        public string[] GetValueSeparators()
        {
            if (ValueSeparators == null)
                return new string[0];
            return (string[]) ValueSeparators.Clone();
        }

        protected static T Parse<T>(string value, OptionContext c)
        {
            Type tt = typeof (T);
            bool nullable = tt.IsValueType && tt.IsGenericType &&
                            !tt.IsGenericTypeDefinition &&
                            tt.GetGenericTypeDefinition() == typeof (Nullable<>);
            Type targetType = nullable ? tt.GetGenericArguments()[0] : typeof (T);
            TypeConverter conv = TypeDescriptor.GetConverter(targetType);
            T t = default(T);
            try
            {
                if (value != null)
                    t = (T) conv.ConvertFromString(value);
            }
            catch (Exception e)
            {
                throw new OptionException(
                    string.Format(
                        c.OptionSet.MessageLocalizer("Could not convert string `{0}' to type {1} for option `{2}'."),
                        value, targetType.Name, c.OptionName),
                    c.OptionName, e);
            }
            return t;
        }

        private OptionValueType ParsePrototype()
        {
            char c = '\0';
            var seps = new List<string>();
            for (int i = 0; i < _names.Length; ++i)
            {
                string name = _names[i];
                if (name.Length == 0)
                    throw new ArgumentException("Empty option names are not supported.", "prototype");

                int end = name.IndexOfAny(NameTerminator);
                if (end == -1)
                    continue;
                _names[i] = name.Substring(0, end);
                if (c == '\0' || c == name[end])
                    c = name[end];
                else
                    throw new ArgumentException(
                        string.Format("Conflicting option types: '{0}' vs. '{1}'.", c, name[end]),
                        "prototype");
                AddSeparators(name, end, seps);
            }

            if (c == '\0')
                return OptionValueType.None;

            if (_count <= 1 && seps.Count != 0)
                throw new ArgumentException(
                    string.Format("Cannot provide key/value separators for Options taking {0} value(s).", _count),
                    "prototype");
            if (_count > 1)
            {
                if (seps.Count == 0)
                    ValueSeparators = new[] {":", "="};
                else if (seps.Count == 1 && seps[0].Length == 0)
                    ValueSeparators = null;
                else
                    ValueSeparators = seps.ToArray();
            }

            return c == '=' ? OptionValueType.Required : OptionValueType.Optional;
        }

        private static void AddSeparators(string name, int end, ICollection<string> seps)
        {
            int start = -1;
            for (int i = end + 1; i < name.Length; ++i)
            {
                switch (name[i])
                {
                    case '{':
                        if (start != -1)
                            throw new ArgumentException(
                                string.Format("Ill-formed name/value separator found in \"{0}\".", name),
                                "prototype");
                        start = i + 1;
                        break;
                    case '}':
                        if (start == -1)
                            throw new ArgumentException(
                                string.Format("Ill-formed name/value separator found in \"{0}\".", name),
                                "prototype");
                        seps.Add(name.Substring(start, i - start));
                        start = -1;
                        break;
                    default:
                        if (start == -1)
                            seps.Add(name[i].ToString(CultureInfo.InvariantCulture));
                        break;
                }
            }
            if (start != -1)
                throw new ArgumentException(
                    string.Format("Ill-formed name/value separator found in \"{0}\".", name),
                    "prototype");
        }

        public void Invoke(OptionContext c)
        {
            OnParseComplete(c);
            c.OptionName = null;
            c.Option = null;
            c.OptionValues.Clear();
        }

        protected abstract void OnParseComplete(OptionContext c);

        public override string ToString()
        {
            return Prototype;
        }
    }

    public abstract class ArgumentSource
    {
        public abstract string Description { get; }
        public abstract string[] GetNames();
        public abstract bool GetArguments(string value, out IEnumerable<string> replacement);

        public static IEnumerable<string> GetArgumentsFromFile(string file)
        {
            return GetArguments(File.OpenText(file), true);
        }

        public static IEnumerable<string> GetArguments(TextReader reader)
        {
            return GetArguments(reader, false);
        }

        // Cribbed from mcs/driver.cs:LoadArgs(string)
        private static IEnumerable<string> GetArguments(TextReader reader, bool close)
        {
            try
            {
                var arg = new StringBuilder();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    int t = line.Length;

                    for (int i = 0; i < t; i++)
                    {
                        char c = line[i];

                        switch (c)
                        {
                            case '\'':
                            case '"':
                                {
                                    char end = c;

                                    for (i++; i < t; i++)
                                    {
                                        c = line[i];

                                        if (c == end)
                                            break;
                                        arg.Append(c);
                                    }
                                }
                                break;
                            case ' ':
                                if (arg.Length > 0)
                                {
                                    yield return arg.ToString();
                                    arg.Length = 0;
                                }
                                break;
                            default:
                                arg.Append(c);
                                break;
                        }
                    }
                    if (arg.Length <= 0) continue;
                    yield return arg.ToString();
                    arg.Length = 0;
                }
            }
            finally
            {
                if (close)
                    reader.Close();
            }
        }
    }

    public class ResponseFileSource : ArgumentSource
    {
        public override string Description
        {
            get { return "Read response file for more options."; }
        }

        public override string[] GetNames()
        {
            return new[] {"@file"};
        }

        public override bool GetArguments(string value, out IEnumerable<string> replacement)
        {
            if (string.IsNullOrEmpty(value) || !value.StartsWith("@"))
            {
                replacement = null;
                return false;
            }
            replacement = GetArgumentsFromFile(value.Substring(1));
            return true;
        }
    }

    [Serializable]
    public class OptionException : Exception
    {
        private readonly string _option;

        public OptionException()
        {
        }

        public OptionException(string message, string optionName)
            : base(message)
        {
            _option = optionName;
        }

        public OptionException(string message, string optionName, Exception innerException)
            : base(message, innerException)
        {
            _option = optionName;
        }

        protected OptionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _option = info.GetString("OptionName");
        }

        public string OptionName
        {
            get { return _option; }
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("OptionName", _option);
        }
    }

    public delegate void OptionAction<in TKey, in TValue>(TKey key, TValue value);

    public class OptionSet : KeyedCollection<string, Option>
    {
        private const int OptionWidth = 29;
        private const int DescriptionFirstWidth = 80 - OptionWidth;
        private const int DescriptionRemWidth = 80 - OptionWidth - 2;

        private readonly Converter<string, string> _localizer;
        private readonly ReadOnlyCollection<ArgumentSource> _roSources;
        private readonly List<ArgumentSource> _sources = new List<ArgumentSource>();

        private readonly Regex _valueOption = new Regex(
            @"^(?<flag>--|-|/)(?<name>[^:=]+)((?<sep>[:=])(?<value>.*))?$");

        public OptionSet()
            : this(f => f)
        {
        }

        public OptionSet(Converter<string, string> localizer)
        {
            _localizer = localizer;
            _roSources = new ReadOnlyCollection<ArgumentSource>(_sources);
        }

        public Converter<string, string> MessageLocalizer
        {
            get { return _localizer; }
        }

        public ReadOnlyCollection<ArgumentSource> ArgumentSources
        {
            get { return _roSources; }
        }


        protected override string GetKeyForItem(Option item)
        {
            if (item == null)
                throw new ArgumentNullException("option");
            if (item.Names != null && item.Names.Length > 0)
                return item.Names[0];
            // This should never happen, as it's invalid for Option to be
            // constructed w/o any names.
            throw new InvalidOperationException("Option has no names!");
        }

        [Obsolete("Use KeyedCollection.this[string]")]
        protected Option GetOptionForName(string option)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            try
            {
                return base[option];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        protected override void InsertItem(int index, Option item)
        {
            base.InsertItem(index, item);
            AddImpl(item);
        }

        protected override void RemoveItem(int index)
        {
            Option p = Items[index];
            base.RemoveItem(index);
            // KeyedCollection.RemoveItem() handles the 0th item
            for (int i = 1; i < p.Names.Length; ++i)
            {
                Dictionary.Remove(p.Names[i]);
            }
        }

        protected override void SetItem(int index, Option item)
        {
            base.SetItem(index, item);
            AddImpl(item);
        }

        private void AddImpl(Option option)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            var added = new List<string>(option.Names.Length);
            try
            {
                // KeyedCollection.InsertItem/SetItem handle the 0th name.
                for (int i = 1; i < option.Names.Length; ++i)
                {
                    Dictionary.Add(option.Names[i], option);
                    added.Add(option.Names[i]);
                }
            }
            catch (Exception)
            {
                foreach (string name in added)
                    Dictionary.Remove(name);
                throw;
            }
        }

        public OptionSet Add(string header)
        {
            if (header == null)
                throw new ArgumentNullException("header");
            Add(new Category(header));
            return this;
        }


        public new OptionSet Add(Option option)
        {
            base.Add(option);
            return this;
        }

        public OptionSet Add(string prototype, Action<string> action)
        {
            return Add(prototype, null, action);
        }

        public OptionSet Add(string prototype, string description, Action<string> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            Option p = new ActionOption(prototype, description, 1,
                                        v => action(v[0]));
            base.Add(p);
            return this;
        }

        public OptionSet Add(string prototype, OptionAction<string, string> action)
        {
            return Add(prototype, null, action);
        }

        public OptionSet Add(string prototype, string description, OptionAction<string, string> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            Option p = new ActionOption(prototype, description, 2,
                                        v => action(v[0], v[1]));
            base.Add(p);
            return this;
        }

        public OptionSet Add<T>(string prototype, Action<T> action)
        {
            return Add(prototype, null, action);
        }

        public OptionSet Add<T>(string prototype, string description, Action<T> action)
        {
            return Add(new ActionOption<T>(prototype, description, action));
        }

        public OptionSet Add<TKey, TValue>(string prototype, OptionAction<TKey, TValue> action)
        {
            return Add(prototype, null, action);
        }

        public OptionSet Add<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action)
        {
            return Add(new ActionOption<TKey, TValue>(prototype, description, action));
        }

        public OptionSet Add(ArgumentSource source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            _sources.Add(source);
            return this;
        }

        protected virtual OptionContext CreateOptionContext()
        {
            return new OptionContext(this);
        }

        public List<string> Parse(IEnumerable<string> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            OptionContext c = CreateOptionContext();
            c.OptionIndex = -1;
            bool process = true;
            var unprocessed = new List<string>();
            Option def = Contains("<>") ? this["<>"] : null;
            var ae = new ArgumentEnumerator(arguments);
            foreach (string argument in ae)
            {
                ++c.OptionIndex;
                if (argument == "--")
                {
                    process = false;
                    continue;
                }
                if (!process)
                {
                    Unprocessed(unprocessed, def, c, argument);
                    continue;
                }
                if (AddSource(ae, argument))
                    continue;
                if (!Parse(argument, c))
                    Unprocessed(unprocessed, def, c, argument);
            }
            if (c.Option != null)
                c.Option.Invoke(c);
            return unprocessed;
        }

        private bool AddSource(ArgumentEnumerator ae, string argument)
        {
            foreach (ArgumentSource source in _sources)
            {
                IEnumerable<string> replacement;
                if (!source.GetArguments(argument, out replacement))
                    continue;
                ae.Add(replacement);
                return true;
            }
            return false;
        }

        private static void Unprocessed(ICollection<string> extra, Option def, OptionContext c, string argument)
        {
            if (def == null)
            {
                extra.Add(argument);
            }
            c.OptionValues.Add(argument);
            c.Option = def;
            if (c.Option != null) c.Option.Invoke(c);
        }

        protected bool GetOptionParts(string argument, out string flag, out string name, out string sep,
                                      out string value)
        {
            if (argument == null)
                throw new ArgumentNullException("argument");

            flag = name = sep = value = null;
            Match m = _valueOption.Match(argument);
            if (!m.Success)
            {
                return false;
            }
            flag = m.Groups["flag"].Value;
            name = m.Groups["name"].Value;
            if (m.Groups["sep"].Success && m.Groups["value"].Success)
            {
                sep = m.Groups["sep"].Value;
                value = m.Groups["value"].Value;
            }
            return true;
        }

        protected virtual bool Parse(string argument, OptionContext c)
        {
            if (c.Option != null)
            {
                ParseValue(argument, c);
                return true;
            }

            string f, n, s, v;
            if (!GetOptionParts(argument, out f, out n, out s, out v))
                return false;

            if (Contains(n))
            {
                Option p = this[n];
                c.OptionName = f + n;
                c.Option = p;
                switch (p.OptionValueType)
                {
                    case OptionValueType.None:
                        c.OptionValues.Add(n);
                        c.Option.Invoke(c);
                        break;
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                        ParseValue(v, c);
                        break;
                }
                return true;
            }
            // no match; is it a bool option?
            return ParseBool(argument, n, c) ||
                   ParseBundledValue(f, string.Concat(string.Format("{0}{1}{2}", n, s, v)), c);
            // is it a bundled option?
        }

        private void ParseValue(string option, OptionContext c)
        {
            if (option != null)
                foreach (string o in c.Option.ValueSeparators != null
                                         ? option.Split(c.Option.ValueSeparators,
                                                        c.Option.MaxValueCount - c.OptionValues.Count,
                                                        StringSplitOptions.None)
                                         : new[] {option})
                {
                    c.OptionValues.Add(o);
                }
            if (c.OptionValues.Count == c.Option.MaxValueCount ||
                c.Option.OptionValueType == OptionValueType.Optional)
                c.Option.Invoke(c);
            else if (c.OptionValues.Count > c.Option.MaxValueCount)
            {
                throw new OptionException(_localizer(string.Format(
                    "Error: Found {0} option values when expecting {1}.",
                    c.OptionValues.Count, c.Option.MaxValueCount)),
                                          c.OptionName);
            }
        }

        private bool ParseBool(string option, string n, OptionContext c)
        {
            string rn;
            if (n.Length >= 1 && (n[n.Length - 1] == '+' || n[n.Length - 1] == '-') &&
                Contains((rn = n.Substring(0, n.Length - 1))))
            {
                Option p = this[rn];
                string v = n[n.Length - 1] == '+' ? option : null;
                c.OptionName = option;
                c.Option = p;
                c.OptionValues.Add(v);
                p.Invoke(c);
                return true;
            }
            return false;
        }

        private bool ParseBundledValue(string f, string n, OptionContext c)
        {
            if (f != "-")
                return false;
            for (int i = 0; i < n.Length; ++i)
            {
                string opt = f + n[i].ToString(CultureInfo.InvariantCulture);
                string rn = n[i].ToString(CultureInfo.InvariantCulture);
                if (!Contains(rn))
                {
                    if (i == 0)
                        return false;
                    throw new OptionException(string.Format(_localizer(
                        "Cannot bundle unregistered option '{0}'."), opt), opt);
                }
                Option p = this[rn];
                switch (p.OptionValueType)
                {
                    case OptionValueType.None:
                        Invoke(c, opt, n, p);
                        break;
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                        {
                            string v = n.Substring(i + 1);
                            c.Option = p;
                            c.OptionName = opt;
                            ParseValue(v.Length != 0 ? v : null, c);
                            return true;
                        }
                    default:
                        throw new InvalidOperationException("Unknown OptionValueType: " + p.OptionValueType);
                }
            }
            return true;
        }

        private static void Invoke(OptionContext c, string name, string value, Option option)
        {
            c.OptionName = name;
            c.Option = option;
            c.OptionValues.Add(value);
            option.Invoke(c);
        }

        public void WriteOptionDescriptions(TextWriter o)
        {
            foreach (Option p in this)
            {
                int written = 0;

                var c = p as Category;
                if (c != null)
                {
                    WriteDescription(o, p.Description, "", 80, 80);
                    continue;
                }

                if (!WriteOptionPrototype(o, p, ref written))
                    continue;

                if (written < OptionWidth)
                    o.Write(new string(' ', OptionWidth - written));
                else
                {
                    o.WriteLine();
                    o.Write(new string(' ', OptionWidth));
                }

                WriteDescription(o, p.Description, new string(' ', OptionWidth + 2),
                                 DescriptionFirstWidth, DescriptionRemWidth);
            }

            foreach (ArgumentSource s in _sources)
            {
                string[] names = s.GetNames();
                if (names == null || names.Length == 0)
                    continue;

                int written = 0;

                Write(o, ref written, "  ");
                Write(o, ref written, names[0]);
                for (int i = 1; i < names.Length; ++i)
                {
                    Write(o, ref written, ", ");
                    Write(o, ref written, names[i]);
                }

                if (written < OptionWidth)
                    o.Write(new string(' ', OptionWidth - written));
                else
                {
                    o.WriteLine();
                    o.Write(new string(' ', OptionWidth));
                }

                WriteDescription(o, s.Description, new string(' ', OptionWidth + 2),
                                 DescriptionFirstWidth, DescriptionRemWidth);
            }
        }

        private void WriteDescription(TextWriter o, string value, string prefix, int firstWidth, int remWidth)
        {
            bool indent = false;
            foreach (string line in GetLines(_localizer(GetDescription(value)), firstWidth, remWidth))
            {
                if (indent)
                    o.Write(prefix);
                o.WriteLine(line);
                indent = true;
            }
        }

        private bool WriteOptionPrototype(TextWriter o, Option p, ref int written)
        {
            string[] names = p.Names;

            int i = GetNextOptionIndex(names, 0);
            if (i == names.Length)
                return false;

            if (names[i].Length == 1)
            {
                Write(o, ref written, "  -");
                Write(o, ref written, names[0]);
            }
            else
            {
                Write(o, ref written, "      --");
                Write(o, ref written, names[0]);
            }

            for (i = GetNextOptionIndex(names, i + 1);
                 i < names.Length;
                 i = GetNextOptionIndex(names, i + 1))
            {
                Write(o, ref written, ", ");
                Write(o, ref written, names[i].Length == 1 ? "-" : "--");
                Write(o, ref written, names[i]);
            }

            if (p.OptionValueType == OptionValueType.Optional ||
                p.OptionValueType == OptionValueType.Required)
            {
                if (p.OptionValueType == OptionValueType.Optional)
                {
                    Write(o, ref written, _localizer("["));
                }
                Write(o, ref written, _localizer("=" + GetArgumentName(0, p.MaxValueCount, p.Description)));
                string sep = p.ValueSeparators != null && p.ValueSeparators.Length > 0
                                 ? p.ValueSeparators[0]
                                 : " ";
                for (int c = 1; c < p.MaxValueCount; ++c)
                {
                    Write(o, ref written, _localizer(sep + GetArgumentName(c, p.MaxValueCount, p.Description)));
                }
                if (p.OptionValueType == OptionValueType.Optional)
                {
                    Write(o, ref written, _localizer("]"));
                }
            }
            return true;
        }

        private static int GetNextOptionIndex(IList<string> names, int i)
        {
            while (i < names.Count && names[i] == "<>")
            {
                ++i;
            }
            return i;
        }

        private static void Write(TextWriter o, ref int n, string s)
        {
            n += s.Length;
            o.Write(s);
        }

        private static string GetArgumentName(int index, int maxIndex, string description)
        {
            if (description == null)
                return maxIndex == 1 ? "VALUE" : "VALUE" + (index + 1);
            string[] nameStart = maxIndex == 1 ? new[] {"{0:", "{"} : new[] {"{" + index + ":"};
            foreach (string t in nameStart)
            {
                int start, j = 0;
                do
                {
                    start = description.IndexOf(t, j, StringComparison.Ordinal);
                } while (start >= 0 && j != 0 && description[j++ - 1] == '{');
                if (start == -1)
                    continue;
                int end = description.IndexOf("}", start, StringComparison.Ordinal);
                if (end == -1)
                    continue;
                return description.Substring(start + t.Length, end - start - t.Length);
            }
            return maxIndex == 1 ? "VALUE" : "VALUE" + (index + 1);
        }

        private static string GetDescription(string description)
        {
            if (description == null)
                return string.Empty;
            var sb = new StringBuilder(description.Length);
            int start = -1;
            for (int i = 0; i < description.Length; ++i)
            {
                switch (description[i])
                {
                    case '{':
                        if (i == start)
                        {
                            sb.Append('{');
                            start = -1;
                        }
                        else if (start < 0)
                            start = i + 1;
                        break;
                    case '}':
                        if (start < 0)
                        {
                            if ((i + 1) == description.Length || description[i + 1] != '}')
                                throw new InvalidOperationException("Invalid option description: " + description);
                            ++i;
                            sb.Append("}");
                        }
                        else
                        {
                            sb.Append(description.Substring(start, i - start));
                            start = -1;
                        }
                        break;
                    case ':':
                        if (start < 0)
                            goto default;
                        start = i + 1;
                        break;
                    default:
                        if (start < 0)
                            sb.Append(description[i]);
                        break;
                }
            }
            return sb.ToString();
        }

        private static IEnumerable<string> GetLines(string description, int firstWidth, int remWidth)
        {
            return StringCoda.WrappedLines(description, firstWidth, remWidth);
        }

        #region Nested type: ActionOption

        private sealed class ActionOption : Option
        {
            private readonly Action<OptionValueCollection> _action;

            public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action)
                : base(prototype, description, count)
            {
                if (action == null)
                    throw new ArgumentNullException("action");
                _action = action;
            }

            protected override void OnParseComplete(OptionContext c)
            {
                _action(c.OptionValues);
            }
        }

        private sealed class ActionOption<T> : Option
        {
            private readonly Action<T> _action;

            public ActionOption(string prototype, string description, Action<T> action)
                : base(prototype, description, 1)
            {
                if (action == null)
                    throw new ArgumentNullException("action");
                _action = action;
            }

            protected override void OnParseComplete(OptionContext c)
            {
                _action(Parse<T>(c.OptionValues[0], c));
            }
        }

        private sealed class ActionOption<TKey, TValue> : Option
        {
            private readonly OptionAction<TKey, TValue> _action;

            public ActionOption(string prototype, string description, OptionAction<TKey, TValue> action)
                : base(prototype, description, 2)
            {
                if (action == null)
                    throw new ArgumentNullException("action");
                _action = action;
            }

            protected override void OnParseComplete(OptionContext c)
            {
                _action(
                    Parse<TKey>(c.OptionValues[0], c),
                    Parse<TValue>(c.OptionValues[1], c));
            }
        }

        #endregion

        #region Nested type: ArgumentEnumerator

        private class ArgumentEnumerator : IEnumerable<string>
        {
            private readonly List<IEnumerator<string>> _sources = new List<IEnumerator<string>>();

            public ArgumentEnumerator(IEnumerable<string> arguments)
            {
                _sources.Add(arguments.GetEnumerator());
            }

            #region IEnumerable<string> Members

            public IEnumerator<string> GetEnumerator()
            {
                do
                {
                    IEnumerator<string> c = _sources[_sources.Count - 1];
                    if (c.MoveNext())
                        yield return c.Current;
                    else
                    {
                        c.Dispose();
                        _sources.RemoveAt(_sources.Count - 1);
                    }
                } while (_sources.Count > 0);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            public void Add(IEnumerable<string> arguments)
            {
                _sources.Add(arguments.GetEnumerator());
            }
        }

        #endregion

        #region Nested type: Category

        internal sealed class Category : Option
        {
            // Prototype starts with '=' because this is an invalid prototype
            // (see Option.ParsePrototype(), and thus it'll prevent Category
            // instances from being accidentally used as normal options.
            public Category(string description)
                : base("=:Category:= " + description, description)
            {
            }

            protected override void OnParseComplete(OptionContext c)
            {
                throw new NotSupportedException("Category.OnParseComplete should not be invoked.");
            }
        }

        #endregion
    }
}