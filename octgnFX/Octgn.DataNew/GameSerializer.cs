namespace Octgn.DataNew
{
    using System;
    using System.Collections.Generic;
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
    using Octgn.Library.Exceptions;
    using Octgn.ProxyGenerator;

    public class GameSerializer : IFileDbSerializer
    {
        public ICollectionDefinition Def { get; set; }

        private string directory;
        public object Deserialize(string fileName)
        {
            var serializer = new XmlSerializer(typeof(game));
            directory = new FileInfo(fileName).Directory.FullName;
            game g = null;
            var fileHash = "";
            using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                g = (game)serializer.Deserialize(fs);
                if (g == null)
                {
                    return null;
                }
                using (MD5 md5 = new MD5CryptoServiceProvider())
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] retVal = md5.ComputeHash(fs);
                    fileHash = BitConverter.ToString(retVal).Replace("-", ""); // hex string
                }
            }
            var ret = new Game()
                          {
                              Id = new Guid(g.id),
                              Name = g.name,
                              CardBack = String.IsNullOrWhiteSpace(g.card.back) ? "pack://application:,,,/Resources/Back.jpg" : Path.Combine(directory, g.card.back),
                              CardFront = String.IsNullOrWhiteSpace(g.card.front) ? "pack://application:,,,/Resources/Front.jpg" : Path.Combine(directory, g.card.front),
                              CardHeight = int.Parse(g.card.height),
                              CardWidth = int.Parse(g.card.width),
                              CardCornerRadius = int.Parse(g.card.cornerRadius),
                              Version = Version.Parse(g.version),
                              CustomProperties = new List<PropertyDef>(),
                              DeckSections = new Dictionary<string, DeckSection>(),
                              SharedDeckSections = new Dictionary<string, DeckSection>(),
                              GlobalVariables = new List<GlobalVariable>(),
                              Authors = g.authors.Split(',').ToList(),
                              Description = g.description,
                              Filename = fileName,
                              Fonts = new List<Font>(),
                              GameUrl = g.gameurl,
                              IconUrl = g.iconurl,
                              Tags = g.tags.Split(' ').ToList(),
                              OctgnVersion = Version.Parse(g.octgnVersion),
                              Variables = new List<Variable>(),
                              MarkerSize = g.markersize,
                              Documents = new List<Document>(),
                              Sounds = new Dictionary<string, GameSound>(),
                              FileHash = fileHash,
                              Events = new Dictionary<string, GameEvent[]>(),
                              InstallPath = directory,
                              UseTwoSidedTable = g.usetwosidedtable == boolean.True ? true : false,
                              NoteBackgroundColor = g.noteBackgroundColor,
                              NoteForegroundColor = g.noteForegroundColor,
                              ScriptVersion = Version.Parse(g.scriptVersion),
                              Scripts = new List<string>()
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
                                    Icon = Path.Combine(directory, i.icon ?? ""),
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
                                         Icon = Path.Combine(directory, i.icon ?? ""),
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
                        (player.Groups as List<Group>).Add(this.DeserialiseGroup(i, curGroup));
                        curGroup++;
                    }
                }
                ret.Player = player;
            }
            #endregion Player

            #region documents
            if (g.documents != null)
            {
                foreach (var doc in g.documents)
                {
                    var d = new Document();
                    d.Icon = Path.Combine(directory, doc.icon);
                    d.Name = doc.name;
                    d.Source = Path.Combine(directory, doc.src);
                    ret.Documents.Add(d);
                }
            }
            #endregion documents
            #region sounds
            if (g.sounds != null)
            {
                foreach (var sound in g.sounds)
                {
                    var s = new GameSound();
                    s.Gameid = ret.Id;
                    s.Name = sound.name;
                    s.Src = Path.Combine(directory, sound.src);
                    ret.Sounds.Add(s.Name.ToLowerInvariant(), s);
                }
            }
            #endregion sounds
            #region deck
            if (g.deck != null)
            {
                foreach (var ds in g.deck)
                {
                    ret.DeckSections.Add(ds.name, new DeckSection { Group = ds.group, Name = ds.name, Shared = false });
                }
            }
            if (g.sharedDeck != null)
            {
                foreach (var s in g.sharedDeck)
                {
                    ret.SharedDeckSections.Add(s.name, new DeckSection { Group = s.group, Name = s.name, Shared = true });
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
                    pd.Type = (PropertyType)Enum.Parse(typeof(PropertyType), prop.type.ToString());
                    pd.IgnoreText = bool.Parse(prop.ignoreText.ToString());
                    pd.Hidden = bool.Parse(prop.hidden);
                    ret.CustomProperties.Add(pd);
                }
            }
            var namepd = new PropertyDef();
            namepd.Name = "Name";
            namepd.TextKind = PropertyTextKind.FreeText;
            namepd.Type = PropertyType.String;
            ret.CustomProperties.Add(namepd);
            #endregion card
            #region fonts
            if (g.fonts != null)
            {
                foreach (gameFont font in g.fonts)
                {
                    Font f = new Font();
                    f.Src = Path.Combine(directory, font.src ?? "");
                    f.Size = (int)font.size;
                    switch (font.target)
                    {
                        case fonttarget.chat:
                            f.Target = Enum.Parse(typeof(fonttarget), "chat").ToString();
                            break;
                        case fonttarget.context:
                            f.Target = Enum.Parse(typeof(fonttarget), "context").ToString();
                            break;
                        case fonttarget.deckeditor:
                            f.Target = Enum.Parse(typeof(fonttarget), "deckeditor").ToString();
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
                    ret.Scripts.Add(s.src);
                    var coll = Def.Config
                        .DefineCollection<GameScript>("Scripts")
                        .OverrideRoot(x => x.Directory("GameDatabase"))
                        .SetPart(x => x.Directory(ret.Id.ToString()));
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

            #region events

            if (g.events != null)
            {
                foreach (var e in g.events)
                {
                    var eve = new GameEvent()
                      {
                          Name = e.name.Clone() as string,
                          PythonFunction =
                              e.action.Clone() as string
                      };
                    if (ret.Events.ContainsKey(e.name))
                    {
                        var narr = ret.Events[e.name];
                        Array.Resize(ref narr, narr.Length + 1);
                        narr[narr.Length - 1] = eve;
                        ret.Events[e.name] = narr;
                    }
                    else
                    {
                        ret.Events.Add(e.name, new GameEvent[1] { eve });
                    }
                }
            }
            #endregion Events
            #region proxygen
            if (g.proxygen != null && g.proxygen.definitionsrc != null)
            {
                ret.ProxyGenSource = g.proxygen.definitionsrc;
                var coll =
                    Def.Config.DefineCollection<ProxyDefinition>("Proxies")
                       .OverrideRoot(x => x.Directory("GameDatabase"))
                       .SetPart(x => x.Directory(ret.Id.ToString()));
                //.SetPart(x => x.Property(y => y.Key));
                var pathParts = g.proxygen.definitionsrc.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                for (var index = 0; index < pathParts.Length; index++)
                {
                    var i = index;
                    if (i == pathParts.Length - 1) coll.SetPart(x => x.File(pathParts[i]));
                    else coll.SetPart(x => x.Directory(pathParts[i]));
                }
                coll.SetSerializer(new ProxyGeneratorSerializer(ret.Id, g.proxygen));
            }
            #endregion proxygen
            #region globalvariables
            if (g.globalvariables != null)
            {
                foreach (var item in g.globalvariables)
                {
                    ret.GlobalVariables.Add(new GlobalVariable { Name = item.name, Value = item.value, DefaultValue = item.value });
                }
            }
            #endregion globalvariables
            #region hash
            #endregion hash
            #region GameModes

            if (g.gameModes != null)
            {
                foreach (var mode in g.gameModes)
                {
                    var m = new GameMode();
                    m.Name = mode.name;
                    m.PlayerCount = mode.playerCount;
                    m.ShortDescription = mode.shortDescription;
                    m.Image = Path.Combine(directory, mode.image);
                    ret.Modes.Add(m);
                }
            }
            #endregion GameModes
            return ret;
        }

        internal Group DeserialiseGroup(group grp, int id)
        {
            if (grp == null)
                return null;
            var ret = new Group
            {
                Id = (byte)id,
                Name = grp.name,
                Background = grp.background == null ? null : Path.Combine(directory, grp.background),
                BackgroundStyle = grp.backgroundStyle.ToString(),
                Board = grp.board == null ? null : Path.Combine(directory, grp.board),
                BoardPosition = grp.boardPosition == null ? new DataRectangle { X = 0, Y = 0, Height = 0, Width = 0 } :
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
                Icon = grp.icon == null ? null : Path.Combine(directory, grp.icon),
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
                            to.IsGroup = false;
                            (ret.CardActions as List<IGroupAction>).Add(to);
                        }
                        else if (item is groupAction)
                        {
                            to.IsGroup = true;
                            (ret.GroupActions as List<IGroupAction>).Add(to);
                        }
                    }
                    else if (item is actionSubmenu)
                    {
                        var i = item as actionSubmenu;
                        var to = new GroupActionGroup
                        {
                            Children = new List<IGroupAction>(),
                            Name = i.menu
                        };
                        if (item is cardActionSubmenu)
                        {
                            to.IsGroup = false;
                            to.Children = this.DeserializeGroupActionGroup(i, false);
                            (ret.CardActions as List<IGroupAction>).Add(to);
                        }
                        else if (item is groupActionSubmenu)
                        {
                            to.IsGroup = true;
                            to.Children = this.DeserializeGroupActionGroup(i, true);
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

        internal IEnumerable<IGroupAction> DeserializeGroupActionGroup(actionSubmenu actiongroup, bool isGroup)
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
                    add.IsGroup = isGroup;
                    ret.Add(add);
                }
                if (item is actionSubmenu)
                {
                    var i = item as actionSubmenu;
                    var addgroup = new GroupActionGroup { Children = new List<IGroupAction>(), Name = i.menu };
                    addgroup.IsGroup = isGroup;
                    addgroup.Children = this.DeserializeGroupActionGroup(i, isGroup);
                    ret.Add(addgroup);
                }
            }
            return ret;
        }

        public byte[] Serialize(object obj)
        {
            if ((obj is Game) == false)
                throw new InvalidOperationException("obj must be typeof Game");

            var game = obj as Game;
            var rootPath = new DirectoryInfo(game.InstallPath).FullName;

            var save = new game();
            save.id = game.Id.ToString();
            save.name = game.Name;
            save.card = new gameCard();
            save.card.back = game.CardBack;
            save.card.front = game.CardFront;
            save.card.height = game.CardHeight.ToString();
            save.card.width = game.CardWidth.ToString();
            save.card.cornerRadius = game.CardCornerRadius.ToString();
            save.version = game.Version.ToString();
            save.authors = string.Join(",", game.Authors);
            save.description = game.Description;
            save.gameurl = game.GameUrl;
            save.iconurl = game.IconUrl;
            save.tags = string.Join(" ", game.Tags);
            save.octgnVersion = game.OctgnVersion.ToString();
            save.markersize = game.MarkerSize;
            save.usetwosidedtable = game.UseTwoSidedTable ? boolean.True : boolean.False;
            save.noteBackgroundColor = game.NoteBackgroundColor;
            save.noteForegroundColor = game.NoteForegroundColor;
            save.scriptVersion = game.ScriptVersion == null ? null : game.ScriptVersion.ToString();

            #region Variables

            save.variables = (game.Variables ?? new List<Variable>()).Select(x => new gameVariable()
            {
                @default = x.Default.ToString(),
                global = x.Global ? boolean.True : boolean.False,
                name = x.Name,
                reset = x.Reset ? boolean.True : boolean.False,
            }).ToArray();

            #endregion
            #region table
            save.table = SerializeGroup(game.Table, rootPath);
            #endregion table
            #region shared

            if (game.GlobalPlayer != null)
            {
                var gs = new gameShared();
                var clist = new List<counter>();
                foreach (var counter in game.GlobalPlayer.Counters)
                {
                    var c = new counter();
                    c.name = counter.Name;
                    c.icon = (counter.Icon ?? "").Replace(rootPath, "");
                    c.reset = counter.Reset ? boolean.True : boolean.False;
                    c.@default = counter.Start.ToString();
                    clist.Add(c);
                }
                var glist = new List<group>();
                foreach (var group in game.GlobalPlayer.Groups)
                {
                    glist.Add(SerializeGroup(group, rootPath));
                }
                gs.group = glist.ToArray();
                gs.counter = clist.ToArray();
                save.shared = gs;
            }
            #endregion shared
            #region player
            if (game.Player != null)
            {
                var player = new gamePlayer();
                var ilist = new List<object>();
                foreach (var counter in game.Player.Counters)
                {
                    var c = new counter();
                    c.name = counter.Name;
                    c.icon = (counter.Icon ?? "").Replace(rootPath, "");
                    c.reset = counter.Reset ? boolean.True : boolean.False;
                    c.@default = counter.Start.ToString();
                    ilist.Add(c);
                }
                foreach (var v in game.Player.GlobalVariables)
                {
                    var gv = new gamePlayerGlobalvariable();
                    gv.name = v.Name;
                    gv.value = v.Value;
                    ilist.Add(gv);
                }
                ilist.Add(SerializeGroup(game.Player.Hand, rootPath));
                foreach (var g in game.Player.Groups)
                {
                    ilist.Add(SerializeGroup(g, rootPath));
                }
                player.Items = ilist.ToArray();
                player.summary = game.Player.IndicatorsFormat;
                save.player = player;
            }
            #endregion player
            #region documents

            if (game.Documents != null)
            {
                var docList = new List<gameDocument>();
                foreach (var d in game.Documents)
                {
                    var doc = new gameDocument();
                    doc.icon = (d.Icon ?? "").Replace(rootPath, "");
                    doc.name = d.Name;
                    doc.src = (d.Source ?? "").Replace(rootPath, "");
                    docList.Add(doc);
                }
                save.documents = docList.ToArray();
            }

            #endregion documents
            #region sounds

            if (game.Sounds != null)
            {
                var soundList = new List<gameSound>();
                foreach (var d in game.Sounds)
                {
                    var doc = new gameSound();
                    doc.name = d.Value.Name;
                    doc.src = (d.Value.Src ?? "").Replace(rootPath, "");
                    soundList.Add(doc);
                }
                save.sounds = soundList.ToArray();
            }

            #endregion sounds
            #region deck

            if (game.DeckSections != null)
            {
                var dl = new List<deckSection>();
                foreach (var s in game.DeckSections)
                {
                    var sec = new deckSection();
                    sec.name = s.Value.Name;
                    sec.group = s.Value.Group;
                    dl.Add(sec);
                }
                save.deck = dl.ToArray();
            }

            if (game.SharedDeckSections != null)
            {
                var dl = new List<deckSection>();
                foreach (var s in game.SharedDeckSections)
                {
                    var sec = new deckSection();
                    sec.name = s.Value.Name;
                    sec.group = s.Value.Group;
                    dl.Add(sec);
                }
                save.sharedDeck = dl.ToArray();
            }
            #endregion deck
            #region card

            if (game.CustomProperties != null)
            {
                var pl = new List<propertyDef>();
                foreach (var prop in game.CustomProperties)
                {
                    if (prop.Name == "Name") continue;
                    var pd = new propertyDef();
                    pd.name = prop.Name;
                    pd.type = (propertyDefType)Enum.Parse(typeof(propertyDefType), prop.Type.ToString());
                    pd.hidden = prop.Hidden.ToString();
                    pd.ignoreText = prop.IgnoreText ? boolean.True : boolean.False;
                    switch (prop.TextKind)
                    {
                        case PropertyTextKind.FreeText:
                            pd.textKind = propertyDefTextKind.Free;
                            break;
                        case PropertyTextKind.Enumeration:
                            pd.textKind = propertyDefTextKind.Enum;
                            break;
                        case PropertyTextKind.Tokens:
                            pd.textKind = propertyDefTextKind.Tokens;
                            break;
                    }
                    pl.Add(pd);
                }
                save.card.property = pl.ToArray();
            }
            #endregion card
            #region fonts

            if (game.Fonts != null)
            {
                var flist = new List<gameFont>();
                foreach (var font in game.Fonts)
                {
                    var f = new gameFont();
                    f.src = (font.Src ?? "").Replace(rootPath, "");
                    f.size = (uint)font.Size;
                    f.target = (fonttarget)Enum.Parse(typeof(fonttarget), font.Target);
                    flist.Add(f);
                }
                save.fonts = flist.ToArray();
            }
            #endregion fonts
            #region scripts

            if (game.Scripts != null)
            {
                var scriptList = new List<gameScript>();
                foreach (var script in game.Scripts)
                {
                    var f = new gameScript();
                    f.src = script;
                    scriptList.Add(f);
                }
                save.scripts = scriptList.ToArray();
            }

            #endregion scripts
            #region events

            if (game.Events != null)
            {
                var eventList = new List<gameEvent>();
                foreach (var e in game.Events.SelectMany(x => x.Value))
                {
                    var eve = new gameEvent();
                    eve.name = e.Name;
                    eve.action = e.PythonFunction;
                    eventList.Add(eve);
                }
                save.events = eventList.ToArray();
            }

            #endregion events
            #region proxygen

            save.proxygen = new gameProxygen();
            save.proxygen.definitionsrc = game.ProxyGenSource;
            #endregion proxygen
            #region globalvariables

            if (game.GlobalVariables != null)
            {
                var gllist = new List<gameGlobalvariable>();
                foreach (var gv in game.GlobalVariables)
                {
                    var v = new gameGlobalvariable();
                    v.name = gv.Name;
                    v.value = gv.Value;
                    gllist.Add(v);
                }
                save.globalvariables = gllist.ToArray();
            }

            #endregion globalvariables

            #region GameModes

            if (game.Modes != null)
            {
                var list = new List<gameGameMode>();
                foreach (var m in game.Modes)
                {
                    var nm = new gameGameMode();
                    nm.name = m.Name;
                    nm.image = m.Image = (m.Image ?? "").Replace(rootPath, "");
                    nm.playerCount = m.PlayerCount;
                    nm.shortDescription = nm.shortDescription;

					list.Add(nm);
                }
                save.gameModes = list.ToArray();
            }

            #endregion GameModes

            var serializer = new XmlSerializer(typeof(game));
            directory = new FileInfo(game.InstallPath).Directory.FullName;
            using (var fs = File.Open(game.Filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                serializer.Serialize(fs, save);
            }
            return File.ReadAllBytes(game.Filename);
        }

        internal group SerializeGroup(Group grp, string rootPath)
        {
            if (grp == null)
                return null;
            var ret = new group();
            ret.name = grp.Name;
            ret.background = (grp.Background ?? "").Replace(rootPath, "");
            ret.backgroundStyle = (groupBackgroundStyle)Enum.Parse(typeof(groupBackgroundStyle), grp.BackgroundStyle);
            grp.BoardPosition = grp.BoardPosition ?? new DataRectangle();
            ret.boardPosition = string.Join(",", grp.BoardPosition.X, grp.BoardPosition.Y, grp.BoardPosition.Width, grp.BoardPosition.Height);
            ret.collapsed = grp.Collapsed ? boolean.True : boolean.False;
            ret.height = grp.Height.ToString();
            ret.width = grp.Width.ToString();
            ret.icon = (grp.Icon ?? "").Replace(rootPath, "");
            ret.ordered = grp.Ordered ? boolean.True : boolean.False;
            ret.shortcut = grp.Shortcut;
            var itemList = SerializeActions(grp.CardActions).ToList();
            itemList.AddRange(SerializeActions(grp.GroupActions).ToArray());
            ret.Items = itemList.ToArray();
            switch (grp.Visibility)
            {
                case GroupVisibility.Undefined:
                    ret.visibility = groupVisibility.undefined;
                    break;
                case GroupVisibility.Nobody:
                    ret.visibility = groupVisibility.none;
                    break;
                case GroupVisibility.Owner:
                    ret.visibility = groupVisibility.me;
                    break;
                case GroupVisibility.Everybody:
                    ret.visibility = groupVisibility.all;
                    break;
            }
            return ret;
        }

        internal IEnumerable<baseAction> SerializeActions(IEnumerable<IGroupAction> actions)
        {
            foreach (var a in actions)
            {
                if (a is GroupAction)
                {
                    var i = a as GroupAction;
                    action ret = i.IsGroup ? (action)new groupAction() : new cardAction();
                    ret.@default = i.DefaultAction ? boolean.True : boolean.False;
                    ret.batchExecute = i.BatchExecute;
                    ret.execute = i.Execute;
                    ret.menu = i.Name;
                    ret.shortcut = i.Shortcut;
                    yield return ret;
                }
                else if (a is GroupActionGroup)
                {
                    var i = a as GroupActionGroup;
                    var ret = i.IsGroup ? (actionSubmenu)new groupActionSubmenu() : new cardActionSubmenu();
                    ret.menu = i.Name;
                    ret.Items = SerializeActions(i.Children).ToArray();
                    yield return ret;
                }
            }
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
            var directory = new FileInfo(fileName).Directory.FullName;
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
                ret.InstallPath = directory;
                ret.DeckPath = Path.Combine(ret.InstallPath, "Decks");
                ret.PackUri = Path.Combine(ret.InstallPath, "Cards");
                var gameImageInstallPath = Path.Combine(Paths.Get().ImageDatabasePath, ret.GameId.ToString());
                ret.ImageInstallPath = Path.Combine(gameImageInstallPath, "Sets", ret.Id.ToString());
                ret.ImagePackUri = Path.Combine(ret.ImageInstallPath, "Cards");
                ret.ProxyPackUri = Path.Combine(ret.ImagePackUri, "Proxies");

                if (!Directory.Exists(ret.PackUri)) Directory.CreateDirectory(ret.PackUri);
                if (!Directory.Exists(ret.ImagePackUri)) Directory.CreateDirectory(ret.ImagePackUri);
                if (!Directory.Exists(ret.ProxyPackUri)) Directory.CreateDirectory(ret.ProxyPackUri);
                var game = DbContext.Get().Games.First(x => x.Id == ret.GameId);
                foreach (var c in doc.Document.Descendants("card"))
                {
                    var card = new Card
                                   {
                                       Id = new Guid(c.Attribute("id").Value),
                                       Name = c.Attribute("name").Value,
                                       SetId = ret.Id,
                                       Properties = new Dictionary<string, CardPropertySet>(),
                                       ImageUri = c.Attribute("id").Value,
                                       Alternate = ""
                                   };
                    var defaultProperties = new CardPropertySet();
                    defaultProperties.Type = "";
                    defaultProperties.Properties = new Dictionary<PropertyDef, object>();
                    foreach (var p in c.Descendants("property").Where(x => x.Parent.Name == "card"))
                    {
                        var pd = game.CustomProperties.FirstOrDefault(x => x.Name == p.Attribute("name").Value);
                        if (pd == null)
                        {
                            throw new UserMessageException("The game {0} you are trying to install/update/play is broken. Please contact the game developer.", game.Name);
                        }
                        var newpd = pd.Clone() as PropertyDef;
                        defaultProperties.Properties.Add(newpd, p.Attribute("value").Value);
                    }
                    foreach (var cp in game.CustomProperties)
                    {
                        if (!defaultProperties.Properties.ContainsKey(cp))
                        {
                            var cpnew = cp.Clone() as PropertyDef;
                            cpnew.IsUndefined = true;
                            defaultProperties.Properties.Add(cpnew, "");
                        }
                    }
                    var np = new PropertyDef()
                                 {
                                     Hidden = false,
                                     Name = "Name",
                                     Type = PropertyType.String,
                                     TextKind = PropertyTextKind.FreeText,
                                     IgnoreText = false,
                                     IsUndefined = false
                                 };
                    if (defaultProperties.Properties.ContainsKey(np))
                        defaultProperties.Properties.Remove(np);
                    defaultProperties.Properties.Add(np, card.Name);
                    card.Properties.Add("", defaultProperties);

                    // Add all of the other property sets
                    foreach (var a in c.Descendants("alternate"))
                    {
                        var propset = new CardPropertySet();
                        propset.Properties = new Dictionary<PropertyDef, object>();
                        propset.Type = a.Attribute("type").Value;
                        var thisName = a.Attribute("name").Value;
                        foreach (var p in a.Descendants("property"))
                        {
                            var pd = game.CustomProperties.First(x => x.Name.Equals(p.Attribute("name").Value, StringComparison.InvariantCultureIgnoreCase));
                            var newprop = pd.Clone() as PropertyDef;
                            var val = p.Attribute("value").Value;
                            propset.Properties.Add(newprop, val);
                        }
                        foreach (var cp in game.CustomProperties)
                        {
                            if (!propset.Properties.ContainsKey(cp))
                            {
                                var cpnew = cp.Clone() as PropertyDef;
                                cpnew.IsUndefined = true;
                                propset.Properties.Add(cpnew, "");
                            }
                        }
                        var np2 = new PropertyDef()
                        {
                            Hidden = false,
                            Name = "Name",
                            Type = PropertyType.String,
                            TextKind = PropertyTextKind.FreeText,
                            IgnoreText = false,
                            IsUndefined = false
                        };
                        if (propset.Properties.ContainsKey(np2))
                            propset.Properties.Remove(np2);
                        propset.Properties.Add(np2, thisName);
                        card.Properties.Add(propset.Type, propset);
                    }

                    (ret.Cards as List<Card>).Add(card);
                }
                foreach (var p in doc.Document.Descendants("pack"))
                {
                    var pack = new Pack();
                    pack.Id = new Guid(p.Attribute("id").Value);
                    pack.Name = p.Attribute("name").Value;
                    pack.Definition = DeserializePack(p.Elements());
                    pack.SetId = ret.Id;
                    (ret.Packs as List<Pack>).Add(pack);
                }
                foreach (var m in doc.Document.Descendants("marker"))
                {
                    var marker = new Marker();
                    marker.Id = new Guid(m.Attribute("id").Value);
                    marker.Name = m.Attribute("name").Value;
                    var mpathd = new DirectoryInfo(Path.Combine(directory, "Markers"));
                    var mpath = mpathd.Exists == false ? null : mpathd.GetFiles(marker.Id.ToString() + ".*", SearchOption.TopDirectoryOnly).First();
                    marker.IconUri = mpath == null ? null : Path.Combine(directory, "Markers", mpath.FullName);
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
                ret.Options.Add(option);
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
            return ret;
        }

        public byte[] Serialize(object obj)
        {
            throw new NotImplementedException();
        }
    }
}