namespace Octgn.DataNew.Entities
{
    using System;
    using System.Security.Cryptography;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;

    public class OptionsList : IPackItem
    {
        public OptionsList()
        {
            Options = new List<Option>();
        }
        public OptionsList(XmlReader reader)
        {
            Options = new List<Option>();

            reader.ReadStartElement("options");
            while (reader.IsStartElement("option"))
            {
                var option = new Option
                                 {
                                     Probability =
                                         double.Parse(reader.GetAttribute("probability") ?? "1",
                                                      CultureInfo.InvariantCulture)
                                 };
                reader.Read(); // <option>
                option.Definition = new PackDefinition(reader);
                reader.ReadEndElement(); // </option>
                Options.Add(option);
            }
            reader.ReadEndElement(); // </random>
        }
        public IList<Option> Options { get; set; }

        private RNGCryptoServiceProvider _provider;

        private double GetProbability()
        {
            if (_provider == null)
            {
                _provider = new RNGCryptoServiceProvider();
            }
            var result = new byte[8];
            _provider.GetBytes(result);
            return ((double)BitConverter.ToUInt64(result, 0) / ulong.MaxValue);
        }

        public PackContent GetCards(Pack pack, Set set)
        {
            double threshold = 0;
            var value = GetProbability();
            foreach (Option option in Options)
            {
                threshold += option.Probability;
                if (value <= threshold) return option.Definition.GenerateContent(pack, set);
            }
            return new PackContent(); // Empty pack
        }
    }
}