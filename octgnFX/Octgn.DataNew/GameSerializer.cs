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
            var serializer = new XmlSerializer(typeof(game));
            game g = null;
            using (var fs = File.Open(fileName,FileMode.Open,FileAccess.Read,FileShare.Read))
            {
                g = (game)serializer.Deserialize(fs);
                if (g == null)
                {
                    return null;
                }
            }
            var ret = new Game()
                          {
                              Id = new Guid(g.id),
                              Name = g.name,
                              CardBack = String.IsNullOrWhiteSpace(g.card.back) ? "pack://application:,,,/Resources/Back.jpg" : g.card.back,
                              CardFront = String.IsNullOrWhiteSpace(g.card.front) ? "pack://application:,,,/Resources/Front.jpg" : g.card.front ,
                              CardHeight = int.Parse(g.card.height),
                              CardWidth = int.Parse(g.card.width),
                              CardCornerRadius = int.Parse(g.card.cornerRadius),
                              Version = Version.Parse(g.version),
                              CustomProperties = new List<PropertyDef>(),
                              DeckSections = new Dictionary<string,DeckSection>(),
                              SharedDeckSections = new Dictionary<string,DeckSection>(),
                              GlobalVariables = new List<GlobalVariable>(),
                              Authors = g.authors.Split(',').ToList(),
                              Description = g.description,
                              FileHash = null,
                              Filename = fileName,
                              Fonts = new List<Font>(),
                              GameUrl = g.gameurl,
                              IconUrl = g.iconurl,
                              Tags = g.tags.Split(' ').ToList(),
                              OctgnVersion = Version.Parse(g.octgnVersion),
                              Variables = new List<Variable>(),
                              MarkerSize = g.markersize
                          };
            #region variables
            if (g.variables != null)
            {
                foreach (var item in g.variables)
                {
                    ret.Variables.Add(new Variable
                                          {
                                              Name = item.name,
                                              Global = bool.Parse(item.global.ToString()),
                                              Reset = bool.Parse(item.reset.ToString()),
                                              Default = int.Parse(item.@default)
                                          });
                }
            }
            #endregion variables
            #region table
            ret.Table = this.DeserialiseGroup(g.table, 0);
            #endregion table
            #region shared
            if (g.shared != null)
            {
                var player = new GlobalPlayer { Counters = new List<Counter>(), Groups = new List<Group>() };
                var curCounter = 1;
                var curGroup = 1;
                if (g.shared.counter != null)
                {
                    foreach (var i in g.shared.counter)
                    {
                        (player.Counters as List<Counter>).Add(
                            new Counter
                                {
                                    Id = (byte)curCounter,
                                    Name = i.name,
                                    Icon = i.icon,
                                    Reset = bool.Parse(i.reset.ToString()),
                                    Start = int.Parse(i.@default)
                                });
                        curCounter++;
                    }
                }
                if (g.shared.group != null)
                {
                    foreach (var i in g.shared.group)
                    {
                        (player.Groups as List<Group>).Add(this.DeserialiseGroup(i, curGroup));
                        curGroup++;
                    }
                }
                ret.GlobalPlayer = player;
            }
            #endregion shared
            #region Player
            if (g.player != null)
            {
                var player = new Player
                                 {
                                     Groups = new List<Group>(),
                                     GlobalVariables = new List<GlobalVariable>(),
                                     Counters = new List<Counter>(),
                                     IndicatorsFormat = g.player.summary
                                 };
                var curCounter = 1;
                var curGroup = 1;
                foreach (var item in g.player.Items)
                {
                    if (item is counter)
                    {
                        var i = item as counter;
                        (player.Counters as List<Counter>)
                            .Add(new Counter
                                     {
                                         Id = (byte)curCounter,
                                         Name = i.name,
                                         Icon = i.icon,
                                         Reset = bool.Parse(i.reset.ToString()),
                                         Start = int.Parse(i.@default)
                                     });
                        curCounter++;
                    }
                    else if (item is gamePlayerGlobalvariable)
                    {
                        var i = item as gamePlayerGlobalvariable;
                        var to = new GlobalVariable { Name = i.name, Value = i.value, DefaultValue = i.value };
                        (player.GlobalVariables as List<GlobalVariable>).Add(to);
                    }
                    else if (item is hand)
                    {
                        player.Hand = this.DeserialiseGroup(item as hand, 0);
                    }
                    else if (item is group)
                    {
                        var i = item as group;
                        (player.Groups as List<Group>).Add(this.DeserialiseGroup(i,curGroup));
                        curGroup++;
                    }
                }
                ret.Player = player;
            }
            #endregion Player
            #region deck
            if (g.deck != null)
            {
                foreach (var ds in g.deck)
                {
                    ret.DeckSections.Add(ds.name, new DeckSection{Group=ds.group,Name = ds.name});
                }
            }
            if (g.sharedDeck != null)
            {
                foreach (var s in g.sharedDeck)
                {
                    ret.SharedDeckSections.Add(s.name, new DeckSection { Group = s.group, Name = s.name });
                }
            }
            #endregion deck
            #region card
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
            #endregion card
            #region fonts
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
            #endregion fonts
            #region scripts
            if (g.scripts != null)
            {
                foreach (var s in g.scripts)
                {
                    var coll = Def.Config
                        .DefineCollection<GameScript>("Scripts")
                        .OverrideRoot(x => x.Directory("GameDatabase"))
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
            #endregion scripts
            #region proxygen
            if (g.proxygen != null)
            {
                var coll =
                    Def.Config.DefineCollection<ProxyDefinition>("Proxies")
                       .OverrideRoot(x => x.Directory("GameDatabase"))
                       .SetPart(x => x.Property(y => y.Key));
                var pathParts = g.proxygen.definitionsrc.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                for (var index = 0; index < pathParts.Length; index++)
                {
                    var i = index;
                    if (i == pathParts.Length - 1) coll.SetPart(x => x.File(pathParts[i]));
                    else coll.SetPart(x => x.Directory(pathParts[i]));
                }
                coll.SetSerializer(new ProxyGeneratorSerializer(ret.Id,g.proxygen));
            }
            #endregion proxygen
            #region globalvariables
            if (g.globalvariables != null)
            {
                foreach (var item in g.globalvariables)
                {
                    ret.GlobalVariables.Add(new GlobalVariable{Name = item.name,Value = item.value,DefaultValue = item.value});
                }
            }
            #endregion globalvariables
            #region hash
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                using (var file = new FileStream(fileName, FileMode.Open))
                {
                    byte[] retVal = md5.ComputeHash(file);
                    ret.FileHash = BitConverter.ToString(retVal).Replace("-", ""); // hex string
                }
            }
            #endregion hash
            return ret;
        }

        internal Group DeserialiseGroup(group grp, int id)
        {
            var ret = new Group
            {
                Id = (byte)id,
                Name = grp.name,
                Background = grp.background,
                BackgroundStyle = grp.backgroundStyle.ToString(),
                Board = grp.board,
                BoardPosition = grp.boardPosition  == null ? new DataRectangle{X = 0,Y=0,Height = 0,Width = 0} : 
                    new DataRectangle
                    {
                        X = double.Parse(grp.boardPosition.Split(',')[0]),
                        Y = double.Parse(grp.boardPosition.Split(',')[1]),
                        Width = double.Parse(grp.boardPosition.Split(',')[2]),
                        Height = double.Parse(grp.boardPosition.Split(',')[3])
                    },
                Collapsed = bool.Parse(grp.collapsed.ToString()),
                Height = Int32.Parse(grp.height),
                Width = Int32.Parse(grp.width),
                Icon = grp.icon,
                Ordered = bool.Parse(grp.ordered.ToString()),
                Shortcut = grp.shortcut,
                CardActions = new List<IGroupAction>(),
                GroupActions = new List<IGroupAction>()
            };
            if (grp.Items != null)
            {
                foreach (var item in grp.Items)
                {
                    if (item is action)
                    {
                        var i = item as action;
                        var to = new GroupAction
                                     {
                                         Name = i.menu,
                                         Shortcut = i.shortcut,
                                         BatchExecute = i.batchExecute,
                                         Execute = i.execute,
                                         DefaultAction = bool.Parse(i.@default.ToString())
                                     };
                        if (item is cardAction)
                        {
                            (ret.CardActions as List<IGroupAction>).Add(to);
                        }
                        else if (item is groupAction)
                        {
                            (ret.GroupActions as List<IGroupAction>).Add(to);
                        }
                    }
                    else if (item is actionSubmenu)
                    {
                        var i = item as actionSubmenu;
                        var to = new GroupActionGroup { Children = new List<IGroupAction>(), Name = i.menu };
                        to.Children = this.DeserializeGroupActionGroup(i);
                        if (item is cardActionSubmenu)
                        {
                            (ret.CardActions as List<IGroupAction>).Add(to);
                        }
                        else if (item is groupActionSubmenu)
                        {
                            (ret.GroupActions as List<IGroupAction>).Add(to);
                        }
                    }
                }
            }
            switch (grp.visibility)
            {
                case groupVisibility.none:
                    ret.Visibility = GroupVisibility.Nobody;
                    break;
                case groupVisibility.me:
                    ret.Visibility = GroupVisibility.Owner;
                    break;
                case groupVisibility.all:
                    ret.Visibility = GroupVisibility.Everybody;
                    break;
                case groupVisibility.undefined:
                    ret.Visibility = GroupVisibility.Undefined;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return ret;
        }

        internal IEnumerable<IGroupAction> DeserializeGroupActionGroup(actionSubmenu actiongroup)
        {
            var ret = new List<IGroupAction>();
            foreach (var item in actiongroup.Items)
            {
                if (item is action)
                {
                    var i = item as action;
                    var add = new GroupAction
                    {
                        Name = i.menu,
                        Shortcut = i.shortcut,
                        Execute = i.execute,
                        BatchExecute = i.batchExecute,
                        DefaultAction = bool.Parse(i.@default.ToString())
                    };
                    ret.Add(add);
                }
                if (item is actionSubmenu)
                {
                    var i = item as actionSubmenu;
                    var addgroup = new GroupActionGroup { Children = new List<IGroupAction>(), Name = i.menu };
                    addgroup.Children = this.DeserializeGroupActionGroup(i);
                    ret.Add(addgroup);
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
        public static XmlSchema SetSchema
        {
            get
            {
                if (setSchema != null) return setSchema;
                var libAss = Assembly.GetAssembly(typeof(Paths));
                var setxsd = libAss.GetManifestResourceNames().FirstOrDefault(x => x.Contains("Set.xsd"));
                setSchema = XmlSchema.Read(libAss.GetManifestResourceStream(setxsd), (sender, args) => Console.WriteLine(args.Exception));
                return setSchema;
            }
        }

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
                        card.Properties.Add(pd, p.Attribute("value").Value);
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

            if (ret.Cards == null) ret.Cards = new Card[0];
            if (ret.Markers == null) ret.Markers = new Marker[0];
            if (ret.Packs == null) ret.Packs = new Pack[0];
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
                option.Probability = double.Parse(probAtt != null ? probAtt.Value : "1", CultureInfo.InvariantCulture);
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
                        if (qtyAttr != null) pick.Quantity = qtyAttr.Value == "unlimited" ? -1 : int.Parse(qtyAttr.Value);
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
        public ICollectionDefinition Def { get; set; }

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
        internal gameProxygen ProxyGenFromDef { get; set; }
        internal Guid GameId { get; set; }
        public ProxyGeneratorSerializer(Guid gameId, gameProxygen proxygen)
        {
            ProxyGenFromDef = proxygen;
            GameId = gameId;
        }

        public object Deserialize(string fileName)
        {
            var game = DbContext.Get().Games.First(x => x.Id == GameId);
            var ret = new ProxyDefinition(GameId, fileName, new FileInfo(game.Filename).Directory.FullName);
            foreach (gameProxygenMapping mapping in ProxyGenFromDef.mappings)
            {
                ret.FieldMapper.AddMapping(mapping.name, mapping.mapto);
            }
            ret.TemplateSelector.DefaultID = ProxyGenFromDef.templatemapping.defaulttemplate;
            switch (ProxyGenFromDef.templatemapping.usemultimatch)
            {
                case boolean.True:
                    ret.TemplateSelector.UseMultiFieldMatching = true;
                    break;
                case boolean.False:
                    ret.TemplateSelector.UseMultiFieldMatching = false;
                    break;
            }
            foreach (gameProxygenTemplatemappingTemplatemap mapping in ProxyGenFromDef.templatemapping.templatemap)
            {
                ret.TemplateSelector.AddMapping(mapping.field);
            }
            return ret;
        }

        public byte[] Serialize(object obj)
        {
            throw new NotImplementedException();
        }
    }
}