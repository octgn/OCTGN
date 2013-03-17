namespace Octgn.DataNew
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using Octgn.DataNew.Entities;
    using Octgn.DataNew.FileDB;
    using Octgn.Library;
    using Octgn.ProxyGenerator;

    public class GameSerializer : IFileDbSerializer
    {
        public ICollectionDefinition Def { get; set; }
        public object Deserialize(string fileName)
        {
            game g = null;
            using (var fs = File.Open(fileName,FileMode.Open,FileAccess.Read,FileShare.Read))
            {
                g = (game)serializer.Deserialize(fs);
                if (g == null)
                {
                    return null;
                }
            }
                          {
                              Id = new Guid(g.id),
                              Name = g.name,
                              CardBack = g.card.back,
                              CardHeight = int.Parse(g.card.height),
                              CardWidth = int.Parse(g.card.width),
                              Version = Version.Parse(g.version),
                              CustomProperties = new List<PropertyDef>(),
                              DeckSections = new List<string>(),
                              SharedDeckSections = new List<string>(),
                              FileHash = null,
                              Fonts = new List<Font>()
                          };
            if (g.deck != null)
            {
                foreach (var ds in g.deck)
                {
                    ret.DeckSections.Add(ds.name);
                }
            }
            if (g.sharedDeck != null)
            {
                foreach (var s in g.sharedDeck)
                {
                    ret.SharedDeckSections.Add(s.name);
                }
            }
            if (g.card != null && g.card.property != null)
            {
                foreach (var prop in g.card.property)
                {
                    var pd = new PropertyDef();
                    pd.Name = prop.name;
                    switch (prop.textKind)
                    {
                        case propertyDefTextKind.Free:
                            pd.TextKind = PropertyTextKind.FreeText;
                            break;
                        case propertyDefTextKind.Enum:
                            pd.TextKind = PropertyTextKind.Enumeration;
                            break;
                        case propertyDefTextKind.Tokens:
                            pd.TextKind = PropertyTextKind.Tokens;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    pd.Type = (PropertyType)Enum.Parse(typeof(PropertyType),prop.type.ToString());
                    pd.IgnoreText = bool.Parse(prop.ignoreText.ToString());
                    pd.Hidden = bool.Parse(prop.hidden);
                    ret.CustomProperties.Add(pd);
                }
            }
            if (g.fonts != null)
            {
                foreach (gameFont font in g.fonts)
                {
                    Font f = new Font();
                    f.Src = font.src;
                    f.Size = (int)font.size;
                    switch (font.target)
                    {
                        case fonttarget.chat:
                            f.Target = Enum.Parse(typeof(fonttarget), "chat").ToString();
                            break;
                        case fonttarget.context:
                            f.Target = Enum.Parse(typeof(fonttarget), "context").ToString();
                            break;
                    }
                    ret.Fonts.Add(f);
                }
            }
            if (g.scripts != null)
            {
                foreach (var s in g.scripts)
                {
                    var coll = Def.Config
                        .DefineCollection<GameScript>("Scripts")
                        .OverrideRoot(x=>x.Directory("Games"))
                        .SetPart(x=>x.Property(y=>y.GameId));
                    var pathParts = s.src.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    for (var index = 0; index < pathParts.Length; index++)
                    {
                        var i = index;
                        if (i == pathParts.Length - 1) coll.SetPart(x => x.File(pathParts[i]));
                        else coll.SetPart(x => x.Directory(pathParts[i]));
                    }
                    coll.SetSerializer(new GameScriptSerializer(ret.Id));
                }
            }
            if (g.proxygen != null)
            {
                if (ProxyManager.Get().LoadDefinition(g.proxygen.definitionsrc))
                {
                    foreach (gameProxygenMapping mapping in g.proxygen.mappings)
                    {
                        ProxyManager.Get().GetFieldMapper().AddMapping(mapping.name, mapping.mapto);
                    }
                    ProxyManager.Get().GetTemplateSelector().DefaultID = g.proxygen.templatemapping.defaulttemplate;
                    ProxyManager.Get().GetTemplateSelector().TemplateMatchField = g.proxygen.templatemapping.templatemap.field;
                }
            }
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                using (var file = new FileStream(fileName, FileMode.Open))
                {
                    byte[] retVal = md5.ComputeHash(file);
                    ret.FileHash = BitConverter.ToString(retVal).Replace("-", ""); // hex string
                }
            }
            
            return ret;
        }

        public byte[] Serialize(object obj)
        {
            throw new System.NotImplementedException();
        }
    }
    public class SetSerializer : IFileDbSerializer
    {
        public ICollectionDefinition Def { get; set; }
        public static XmlSchema SetSchema {get
        {
            if (setSchema != null) return setSchema;
            var libAss = Assembly.GetAssembly(typeof(Paths));
            var setxsd = libAss.GetManifestResourceNames().FirstOrDefault(x => x.Contains("Set.xsd"));
            setSchema = XmlSchema.Read(libAss.GetManifestResourceStream(setxsd), (sender, args) => Console.WriteLine(args.Exception));
            return setSchema;
        }}

        internal static XmlSchema setSchema;
        public object Deserialize(string fileName)
        {
            //var timer = new Stopwatch();
            //timer.Start();
            var ret = new Set();
            using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var settings = new XmlReaderSettings();
                settings.Schemas.Add(SetSchema);
                var doc = XDocument.Load(fs);
                doc.Validate(settings.Schemas, (a, b) =>
                    {
                        Console.WriteLine(b.Exception);
                    });
                ret.Cards = new List<Card>();
                var root = doc.Element("set");
                ret.Id = new Guid(root.Attribute("id").Value);
                ret.Name = root.Attribute("name").Value;
                ret.Filename = fileName;
                ret.GameId = new Guid(root.Attribute("gameId").Value);
                ret.Cards = new List<Card>();
                ret.GameVersion = new Version(root.Attribute("gameVersion").Value);
                ret.Markers = new List<Marker>();
                ret.Packs = new List<Pack>();
                ret.Version = new Version(root.Attribute("version").Value);
                ret.PackageName = "";
                var game = DbContext.Get().Games.First(x => x.Id == ret.GameId);
                foreach (var c in doc.Document.Descendants("card"))
                {
                    var card = new Card
                                   {
                                       Id = new Guid(c.Attribute("id").Value),
                                       Name = c.Attribute("name").Value,
                                       SetId = ret.Id,
                                       Properties = new Dictionary<PropertyDef, object>(),
                                       ImageUri = c.Attribute("id").Value
                                   };
                    foreach (var p in c.Descendants("property"))
                    {
                        var pd = game.CustomProperties.First(x => x.Name == p.Attribute("name").Value);
                        card.Properties.Add(pd,p.Attribute("value").Value);
                    }
                    (ret.Cards as List<Card>).Add(card);
                }
                foreach (var p in doc.Document.Descendants("pack"))
                {
                    var pack = new Pack();
                    pack.Id = new Guid(p.Attribute("id").Value);
                    pack.Name = p.Attribute("name").Value;
                    pack.Definition = DeserializePack(p.Elements());
                    (ret.Packs as List<Pack>).Add(pack);
                }
                foreach (var m in doc.Document.Descendants("marker"))
                {
                    var marker = new Marker();
                    marker.Id = new Guid(m.Attribute("id").Value);
                    marker.Name = m.Attribute("name").Value;
                    (ret.Markers as List<Marker>).Add(marker);
                }
            }
            
            if(ret.Cards == null) ret.Cards = new Card[0];
            if(ret.Markers == null) ret.Markers = new Marker[0];
            if(ret.Packs == null) ret.Packs = new Pack[0];
            //Console.WriteLine(timer.ElapsedMilliseconds);
            return ret;
        }

        internal OptionsList DeserializeOptions(XElement element)
        {
            var ret = new OptionsList();
            foreach (var op in element.Elements("option"))
            {
                var option = new Option();
                var probAtt = op.Attributes("probability").FirstOrDefault();
                option.Probability = double.Parse(probAtt != null ?probAtt.Value : "1", CultureInfo.InvariantCulture);
                option.Definition = DeserializePack(op.Elements());
            }
            return ret;
        }

        internal PackDefinition DeserializePack(IEnumerable<XElement> element)
        {
            var ret = new PackDefinition();
            foreach (var e in element)
            {
                switch (e.Name.LocalName)
                {
                    case "options":
                        ret.Items.Add(this.DeserializeOptions(e));
                        break;
                    case "pick":
                        var pick = new Pick();
                        var qtyAttr = e.Attributes().FirstOrDefault(x => x.Name.LocalName == "qty");
                        if(qtyAttr != null) pick.Quantity = qtyAttr.Value == "unlimited" ? -1 : int.Parse(qtyAttr.Value);
                        pick.Key = e.Attribute("key").Value;
                        pick.Value = e.Attribute("value").Value;
                        ret.Items.Add(pick);
                        break;
                }
            }
            return ret;
        }

        public byte[] Serialize(object obj)
        {
            throw new System.NotImplementedException();
        }
    }

    public class GameScriptSerializer : IFileDbSerializer
    {
        public ICollectionDefinition Def { get;  set; }

        internal Guid GameId { get; set; }

        public GameScriptSerializer(Guid gameId)
        {
            GameId = gameId;
        }

        public object Deserialize(string fileName)
        {
            var ret = new GameScript();
            ret.Path = fileName;
            ret.GameId = GameId;
            ret.Script = File.ReadAllText(fileName);
            return ret;
        }

        public byte[] Serialize(object obj)
        {
            throw new NotImplementedException();
        }
    }

    public class ProxyGeneratorSerializer : IFileDbSerializer
    {

        public ICollectionDefinition Def { get; set; }

        public object Deserialize(string fileName)
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize(object obj)
        {
            throw new NotImplementedException();
        }
    }
}