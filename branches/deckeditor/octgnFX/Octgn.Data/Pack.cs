using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Globalization;

namespace Octgn.Data
{
  public class Pack
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Set Set { get; set; }
    public PackDefinition Definition { get; private set; }

    public string FullName
    { get { return Set.Name + ", " + Name; } }

    public Pack(Set set, string xml)
    {
      Set = set;
      using (var stringReader = new StringReader(xml))
      using (var reader = XmlReader.Create(stringReader))
      {
        reader.Read();
        Id = new Guid(reader.GetAttribute("id"));
        Name = reader.GetAttribute("name");
        reader.ReadStartElement("pack");
        Definition = new PackDefinition(reader);
        reader.ReadEndElement();  // </pack>
      }
    }

    public PackContent CrackOpen()
    { 
      return Definition.GenerateContent(this); 
    }

    #region PackDefinition

    public class PackDefinition : List<BasePackItem>
    {
      public PackDefinition(XmlReader reader)
      {
        while (true)
        {
          if (reader.IsStartElement("pick"))
            Add(new Pick(reader));
          else if (reader.IsStartElement("options"))
            Add(new OptionsList(reader));
          else
            return;
        }
      }

      public PackContent GenerateContent(Pack pack)
      {
        var result = new PackContent();
        foreach (var def in this)
        {
          PackContent defContent = def.GetCards(pack);
          result.Merge(defContent);
        }
        return result;
      }
    }

    public abstract class BasePackItem
    {
      public abstract PackContent GetCards(Pack pack);
    }    

    public class Pick : BasePackItem
    {
      public int Quantity { get; set; }
      public string Key { get; set; }
      public string Value { get; set; }

      public Pick(XmlReader reader)
      {
        string qtyAttribute = reader.GetAttribute("qty");
        Quantity = qtyAttribute == "unlimited" ? -1 : int.Parse(qtyAttribute);
        Key = reader.GetAttribute("key");
        Value = reader.GetAttribute("value");
        reader.Read();  // <pick />
      }

      public override PackContent GetCards(Pack pack)
      {
        var result = new PackContent();
        string[] conditions = new string[2];
        conditions[0] = "setId = '" + pack.Set.Id + "'";
        conditions[1] = string.Format("Card.[{0}] = '{1}'", Key, Value);
        if (Quantity < 0)
          result.UnlimitedCards.AddRange(pack.Set.Game.SelectCardModels(conditions));
        else
          result.LimitedCards.AddRange(pack.Set.Game.SelectRandomCardModels(Quantity, conditions));
        return result;
      }
    }

    public class OptionsList : BasePackItem
    {
      public List<Option> Options { get; private set; }

      public OptionsList(XmlReader reader)
      {
        Options = new List<Option>();

        reader.ReadStartElement("options");
        while (reader.IsStartElement("option"))
        {
          var option = new Option { Probability = double.Parse(reader.GetAttribute("probability") ?? "1", CultureInfo.InvariantCulture) };
          reader.Read();  // <option>
          option.Definition = new PackDefinition(reader);
          reader.ReadEndElement();  // </option>
          Options.Add(option);
        }
        reader.ReadEndElement();  // </random>
      }

      public override PackContent GetCards(Pack pack)
      {
        var rnd = new System.Random();
        double value = rnd.NextDouble();
        double threshold = 0;
        foreach (var option in Options)
        {
          threshold += option.Probability;
          if (value <= threshold) return option.Definition.GenerateContent(pack);
        }
        return new PackContent(); // Empty pack
      }

      public class Option
      {
        public double Probability { get; set; }
        public PackDefinition Definition { get; set; }
      }
    }

    #endregion

    public class PackContent
    {
      public List<CardModel> LimitedCards { get; private set; }
      public List<CardModel> UnlimitedCards { get; private  set; }

      public PackContent()
      {
        LimitedCards = new List<CardModel>();
        UnlimitedCards = new List<CardModel>();
      }

      public void Merge(PackContent content)
      {
        LimitedCards.AddRange(content.LimitedCards);
        UnlimitedCards.AddRange(content.UnlimitedCards);
      }
    }
  }
}
