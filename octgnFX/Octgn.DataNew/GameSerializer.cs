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
using Octgn.ProxyGenerator.Definitions;
using Regex = System.Text.RegularExpressions;

namespace Octgn.DataNew
{
    public class GameSerializer : IFileDbSerializer
    {
        public ICollectionDefinition Def { get; set; }

        private string directory;
        public object Deserialize(string fileName)
        {
            //var serializer = new XmlSerializer(typeof(game));
            // Fix: Do it this way so that it doesn't throw a BindingFailure
            // See Also: http://stackoverflow.com/questions/2209443/c-sharp-xmlserializer-bindingfailure#answer-22187247
            var serializer = XmlSerializer.FromTypes(new[] { typeof(game) })[0];
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
                              CardSizes = new Dictionary<string, CardSize>(),
                              GameBoards = new Dictionary<string, GameBoard>(),
                              Version = Version.Parse(g.version),
                              CustomProperties = new List<PropertyDef>(),
                              DeckSections = new Dictionary<string, DeckSection>(),
                              SharedDeckSections = new Dictionary<string, DeckSection>(),
                              GlobalVariables = new Dictionary<string, GlobalVariable>(),
                              Authors = g.authors.Split(',').ToList(),
                              Description = g.description,
                              Filename = fileName,
                              GameUrl = g.gameurl,
                              SetsUrl = g.setsurl,
                              IconUrl = g.iconurl,
                              Tags = g.tags.Split(' ').ToList(),
                              OctgnVersion = Version.Parse(g.octgnVersion),
                              MarkerSize = int.Parse(g.markersize),
                              Markers = new Dictionary<string, GameMarker>(),
                              Phases = new List<GamePhase>(),
                              Documents = new List<Document>(),
                              Symbols = new List<Symbol>(),
                              Sounds = new Dictionary<string, GameSound>(),
                              FileHash = fileHash,
                              Events = new Dictionary<string, GameEvent[]>(),
                              InstallPath = directory,
                              UseTwoSidedTable = g.usetwosidedtable == boolean.True ? true : false,
                              ChangeTwoSidedTable = g.changetwosidedtable == boolean.True ? true : false,
                              NoteBackgroundColor = g.noteBackgroundColor,
                              NoteForegroundColor = g.noteForegroundColor,
                              ScriptVersion = Version.Parse(g.scriptVersion),
                              Scripts = new List<string>(),
                              Modes = new List<GameMode>(),

                          };
            var defSize = new CardSize
            {
                Name = g.card.name,
                Back = String.IsNullOrWhiteSpace(g.card.back) ? "pack://application:,,,/Resources/Back.jpg" : Path.Combine(directory, g.card.back),
                Front = String.IsNullOrWhiteSpace(g.card.front) ? "pack://application:,,,/Resources/Front.jpg" : Path.Combine(directory, g.card.front),
                Height = int.Parse(g.card.height),
                Width = int.Parse(g.card.width),
                CornerRadius = int.Parse(g.card.cornerRadius),
                BackHeight = int.Parse(g.card.backHeight),
                BackWidth = int.Parse(g.card.backWidth),
                BackCornerRadius = int.Parse(g.card.backCornerRadius)
            };
            if (defSize.BackHeight < 0)
                defSize.BackHeight = defSize.Height;
            if (defSize.BackWidth < 0)
                defSize.BackWidth = defSize.Width;
            if (defSize.BackCornerRadius < 0)
                defSize.BackCornerRadius = defSize.CornerRadius;
            ret.CardSizes.Add("", defSize);

            #region table

            ret.Table = this.DeserialiseGroup(g.table, 0);
            ret.Table.Background = g.table.background == null ? null : Path.Combine(directory, g.table.background);

            switch (g.table.backgroundStyle)
            {
                case tableBackgroundStyle.stretch:
                    ret.Table.BackgroundStyle = BackgroundStyle.Stretch;
                    break;
                case tableBackgroundStyle.tile:
                    ret.Table.BackgroundStyle = BackgroundStyle.Tile;
                    break;
                case tableBackgroundStyle.uniform:
                    ret.Table.BackgroundStyle = BackgroundStyle.Uniform;
                    break;
                case tableBackgroundStyle.uniformToFill:
                    ret.Table.BackgroundStyle = BackgroundStyle.UniformToFill;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ret.Table.Height = Int32.Parse(g.table.height);
            ret.Table.Width = Int32.Parse(g.table.width);
            #endregion table
            #region gameBoards
            if (g.gameboards == null)
            {
                try
                {
                    // if the game doesn't have the gameboards element, check for the default board in the table property
                    var position = g.table.boardPosition?.Split(',') ?? new string[] { "0", "0", "0", "0"};

                    string boardPath = null;
                    if(g.table.board != null) {
                        boardPath = Path.Combine(directory, g.table.board);
                    }

                    var defBoard = new GameBoard
                    {
                        Name = "Default",
                        Source = boardPath,
                        XPos = double.Parse(position[0]),
                        YPos = double.Parse(position[1]),
                        Width = double.Parse(position[2]),
                        Height = double.Parse(position[3])
                    };
                    ret.GameBoards.Add("", defBoard);
                }
                catch
                {
                    // ignores this game board instead of invalidating the entire game definition
                }
            }
            else
            {
                try
                {
                    var defaultBoard = new GameBoard
                    {
                        Name = g.gameboards.name,
                        Source = Path.Combine(directory, g.gameboards.src),
                        XPos = int.Parse(g.gameboards.x),
                        YPos = int.Parse(g.gameboards.y),
                        Width = int.Parse(g.gameboards.width),
                        Height = int.Parse(g.gameboards.height),
                    };
                    ret.GameBoards.Add("", defaultBoard);
                }
                catch
                {
                    // xsd can't make these properties mandatory so ignore the default board if something's missing
                }

                if (g.gameboards.gameboard != null)
                {
                    foreach (var board in g.gameboards.gameboard)
                    {
                        var b = new GameBoard
                        {
                            Name = board.name,
                            XPos = int.Parse(board.x),
                            YPos = int.Parse(board.y),
                            Width = int.Parse(board.width),
                            Height = int.Parse(board.height),
                            Source = Path.Combine(directory, board.src)
                        };
                        ret.GameBoards.Add(board.name, b);
                    }
                }
            }
            #endregion gameBoards
            #region shared
            if (g.shared != null)
            {
                var player = new GlobalPlayer
                                   {
                                     Counters = new List<Counter>(),
                                     Groups = new List<Group>(),
                                     IndicatorsFormat = g.shared.summary
                                   };

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
            #region LEGACY CODE FOR THE OLD HAND GROUP
            Group legacyHandGroup = null;
            #endregion
            if (g.player != null)
            {
                var player = new Player
                                 {
                                     Groups = new List<Group>(),
                                     GlobalVariables = new Dictionary<string, GlobalVariable>(),
                                     Counters = new List<Counter>(),
                                     IndicatorsFormat = g.player.summary
                                 };
                var curCounter = 1;
                var curGroup = 1;
                if (g.player.Items != null)
                {
                foreach (var item in g.player.Items)
                {
                        if (item is counter counter)
                        {
                            (player.Counters as List<Counter>)
                                .Add(new Counter
                                {
                                    Id = (byte)curCounter,
                                    Name = counter.name,
                                    Icon = Path.Combine(directory, counter.icon ?? ""),
                                    Reset = bool.Parse(counter.reset.ToString()),
                                    Start = int.Parse(counter.@default)
                                });
                            curCounter++;
                        }
                        else if (item is gamePlayerGlobalvariable globalvariable)
                        {
                            player.GlobalVariables.Add(globalvariable.name, new GlobalVariable()
                                                                            {
                                                                                Name = globalvariable.name,
                                                                                Value = globalvariable.value
                                                                            });
                        }
                        #region LEGACY CODE FOR THE OLD HAND GROUP
                        else if (item is hand hand)
                        {
                            (player.Groups as List<Group>).Add(this.DeserialiseGroup(hand, curGroup));
                            curGroup++;
                            legacyHandGroup = player.Groups.Last();
                        }
                        #endregion
                        else if (item is group group)
                        {
                            (player.Groups as List<Group>).Add(this.DeserialiseGroup(group, curGroup));
                            curGroup++;
                        }
                }
                }
                ret.Player = player;
            }
            #endregion Player

            #region phases
            if (g.phases != null)
            {
                foreach (var p in g.phases)
                {
                    var gamePhase = new GamePhase
                    {
                        Icon = Path.Combine(directory, p.icon),
                        Name = p.name
                    };
                    ret.Phases.Add(gamePhase);
                }
            }
            #endregion phases

            #region symbols
            if (g.symbols != null)
            {
                foreach (var s in g.symbols)
                {
                    var symbol = new Symbol
                    {
                        Name = s.name,
                        Id = s.id,
                        Source = Path.Combine(directory, s.src)
                    };
                    ret.Symbols.Add(symbol);
                }
            }
            #endregion symbols

            #region documents
            if (g.documents != null)
            {
                foreach (var d in g.documents)
                {
                    var document = new Document
                    {
                        Icon = (d.icon == null) ? null : Path.Combine(directory, d.icon),
                        Name = d.name,
                        Source = Path.Combine(directory, d.src)
                    };
                    ret.Documents.Add(document);
                }
            }
            #endregion documents
            #region sounds
            if (g.sounds != null)
            {
                foreach (var s in g.sounds)
                {
                    var gameSound = new GameSound
                    {
                        Gameid = ret.Id,
                        Name = s.name,
                        Src = Path.Combine(directory, s.src)
                    };
                    ret.Sounds.Add(gameSound.Name.ToLowerInvariant(), gameSound);
                }
            }
            #endregion sounds
            #region deck
            if (g.deck != null)
            {
                foreach (var d in g.deck)
                {
                    var group = ret.Player.Groups.FirstOrDefault(x => x.Name == d.group);
                    if (group != null)
                        ret.DeckSections.Add(d.name, new DeckSection { Group = group, Name = d.name, Shared = false });
                }
            }
            if (g.sharedDeck != null)
            {
                foreach (var s in g.sharedDeck)
                {
                    var group = ret.GlobalPlayer.Groups.FirstOrDefault(x => x.Name == s.group);
                    if (group != null)
                        ret.SharedDeckSections.Add(s.name, new DeckSection { Group = group, Name = s.name, Shared = true });
                }
            }
            #endregion deck
            #region markers
            if (g.markers != null)
            {
                foreach (var m in g.markers)
                {
                    var marker = new GameMarker
                    {
                        Name = m.name,
                        Source = Path.Combine(directory, m.src),
                        Id = m.id
                    };
                    ret.Markers.Add(marker.Id, marker);
                }
            }
            #endregion markers
            #region card
            if (g.card != null)
            {
                if (g.card.property != null)
                {
                    foreach (var p in g.card.property)
                    {
                        var propertyDef = new PropertyDef
                        {
                            Name = p.name,
                            Type = (PropertyType)Enum.Parse(typeof(PropertyType), p.type.ToString()),
                            IgnoreText = bool.Parse(p.ignoreText.ToString()),
                            Hidden = bool.Parse(p.hidden.ToString())
                        };
                        switch (p.textKind)
                        {
                            case propertyDefTextKind.Free:
                                propertyDef.TextKind = PropertyTextKind.FreeText;
                                break;
                            case propertyDefTextKind.Enum:
                                propertyDef.TextKind = PropertyTextKind.Enumeration;
                                break;
                            case propertyDefTextKind.Tokens:
                                propertyDef.TextKind = PropertyTextKind.Tokens;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        ret.CustomProperties.Add(propertyDef);
                    }
                }
                if (g.card.size != null)
                {
                    foreach (var s in g.card.size)
                    {
                        var cardSize = new CardSize
                        {
                            Name = s.name,
                            Width = int.Parse(s.width),
                            Height = int.Parse(s.height),
                            CornerRadius = int.Parse(s.cornerRadius),
                            BackWidth = int.Parse(s.backWidth),
                            BackHeight = int.Parse(s.backHeight),
                            BackCornerRadius = int.Parse(s.backCornerRadius),
                            Front = String.IsNullOrWhiteSpace(s.front) ? "pack://application:,,,/Resources/Front.jpg" : Path.Combine(directory, s.front),
                            Back = String.IsNullOrWhiteSpace(s.back) ? "pack://application:,,,/Resources/Back.jpg" : Path.Combine(directory, s.back)
                        };
                        // xsd sets the default values for the back side sizes to -1 so the deserializer knows they were missing
                        // TODO: handle this better for mising values
                        if (cardSize.BackHeight < 0)
                            cardSize.BackHeight = cardSize.Height;
                        if (cardSize.BackWidth < 0)
                            cardSize.BackWidth = cardSize.Width;
                        if (cardSize.BackCornerRadius < 0)
                            cardSize.BackCornerRadius = cardSize.CornerRadius;
                        ret.CardSizes.Add(cardSize.Name, cardSize);
                    }
                }
            }
            #endregion card
            #region fonts
            if (g.fonts != null)
            {
                foreach (gameFont f in g.fonts)
                {
                    Font font = new Font
                    {
                        Src = (f.src == null) ? null : Path.Combine(directory, f.src).Replace("/", "\\"),
                        Size = int.Parse(f.size),
                        Type = f.target.ToString()
                    };
                    switch (f.target)
                    {
                        case fonttarget.chat:
                            ret.ChatFont = font;
                            break;
                        case fonttarget.context:
                            ret.ContextFont = font;
                            break;
                        case fonttarget.deckeditor:
                            ret.DeckEditorFont = font;
                            break;
                        case fonttarget.notes:
                            ret.NoteFont = font;
                            break;
                    }
                }
            }
            #endregion fonts
            #region scripts
            if (g.scripts != null)
            {
                foreach (var s in g.scripts)
                {
                    ret.Scripts.Add(s.src);
                    var coll = (Def.Config.IsExternalDb)
                        ?
                            Def.Config.DefineCollection<GameScript>("Scripts")
                            .OverrideRoot(x => x.Directory(""))
                            .SetPart(x => x.Directory(ret.Id.ToString()))
                        :
                            Def.Config.DefineCollection<GameScript>("Scripts")
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
                    var gameEvent = new GameEvent()
                      {
                          Name = e.name.Clone() as string,
                          PythonFunction =
                              e.action.Clone() as string
                      };
                    if (ret.Events.ContainsKey(e.name))
                    {
                        var narr = ret.Events[e.name];
                        Array.Resize(ref narr, narr.Length + 1);
                        narr[narr.Length - 1] = gameEvent;
                        ret.Events[e.name] = narr;
                    }
                    else
                    {
                        ret.Events.Add(e.name, new GameEvent[1] { gameEvent });
                    }
                }
            }
            #endregion Events
            #region proxygen
            if (g.proxygen != null && g.proxygen.definitionsrc != null)
            {
                ret.ProxyGenSource = g.proxygen.definitionsrc;
                var coll = (Def.Config.IsExternalDb)
                    ?
                        Def.Config.DefineCollection<ProxyDefinition>("Proxies")
                        .OverrideRoot(x => x.Directory(""))
                        .SetPart(x => x.Directory(ret.Id.ToString()))
                    :
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
                coll.SetSerializer(new ProxyGeneratorSerializer(ret.Id));
            }
            #endregion proxygen
            #region globalvariables
            if (g.globalvariables != null)
            {
                foreach (var item in g.globalvariables)
                {
                    var globalVariable = new GlobalVariable()
                    {
                        Name = item.name,
                        Value = item.value
                    };
                    ret.GlobalVariables.Add(item.name, globalVariable);
                }
            }
            #endregion globalvariables
            #region hash
            #endregion hash
            #region GameModes

            if (g.gameModes != null)
            {
                foreach (var m in g.gameModes)
                {
                    var gameMode = new GameMode
                    {
                        Name = m.name,
                        PlayerCount = int.Parse(m.playerCount),
                        ShortDescription = m.shortDescription,
                        Image = Path.Combine(directory, m.image),
                        UseTwoSidedTable = bool.Parse(m.usetwosidedtable.ToString())
                    };
                    ret.Modes.Add(gameMode);
                }
            }

            if (ret.Modes.Count == 0)
            {
                var m = new GameMode
                {
                    Name = "Default",
                    PlayerCount = 2,
                    ShortDescription = "Default Game Mode"
                };
                ret.Modes.Add(m);
            }

            #endregion GameModes
            #region LEGACY CODE FOR THE OLD HAND GROUP
            if (legacyHandGroup != null)
            {
                legacyHandGroup.Name = "Hand";
            }
            #endregion
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
                Icon = grp.icon == null ? null : Path.Combine(directory, grp.icon),
                Ordered = bool.Parse(grp.ordered.ToString()),
                Shortcut = grp.shortcut,
                MoveTo = bool.Parse(grp.moveto.ToString()),
                CardActions = new List<IGroupAction>(),
                GroupActions = new List<IGroupAction>()
            };
            if (ret.Width == 0)
            {
                ret.Width = 1;
            }
            if (ret.Height == 0)
            {
                ret.Height = 1;
            }
            if (grp.Items != null)
            {
                ret.CardActions = DeserializeGroupActionList(grp.Items, false);
                ret.GroupActions = DeserializeGroupActionList(grp.Items, true);
            }
            #region LEGACY CODE FOR THE OLD HAND GROUP
            if (grp is hand) ret.ViewState = GroupViewState.Expanded;
            else if (grp.collapsed == boolean.True) ret.ViewState = GroupViewState.Collapsed;
            else
            #endregion
            {
                switch (grp.viewState)
                {
                    case groupViewState.collapsed:
                        ret.ViewState = GroupViewState.Collapsed;
                        break;
                    case groupViewState.expanded:
                        ret.ViewState = GroupViewState.Expanded;
                        break;
                    case groupViewState.pile:
                        ret.ViewState = GroupViewState.Pile;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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

        internal IEnumerable<IGroupAction> DeserializeGroupActionList(IEnumerable<baseAction> actiongroup, bool isGroup)
        {
            var ret = new List<IGroupAction>();
       //     if (actiongroup.Items == null) return ret;
            foreach (baseAction item in actiongroup)
            {
                if (item is action action)
                {
                    if ((action is cardAction) == isGroup)
                        continue;
                    var addAction = new GroupAction
                    {
                        Name = action.menu,
                        Shortcut = action.shortcut,
                        IsBatchExecutable = (action.batchExecute != null),
                        ShowExecute = action.showIf,
                        HeaderExecute = action.getName,
                        DefaultAction = bool.Parse(action.@default.ToString())
                    };
                    addAction.Execute = (addAction.IsBatchExecutable) ? action.batchExecute : action.execute;
                    addAction.IsGroup = isGroup;
                    ret.Add(addAction);
                }
                else if (item is actionSubmenu submenu)
                {
                    if ((submenu is cardActionSubmenu) == isGroup)
                        continue;
                    var addgroup = new GroupActionSubmenu
                    {
                        Children = new List<IGroupAction>(),
                        Name = submenu.menu,
                        IsGroup = isGroup,
                        ShowExecute = submenu.showIf,
                        HeaderExecute = submenu.getName
                    };
                    addgroup.Children = this.DeserializeGroupActionList(submenu.Items, isGroup);
                    ret.Add(addgroup);
                }
                else if (item is actionSeparator separator)
                {
                    if ((separator is cardActionSeparator) == isGroup)
                        continue;
                    var groupActionSeparator = new GroupActionSeparator
                    {
                        IsGroup = isGroup,
                        ShowExecute = separator.showIf
                    };
                    ret.Add(groupActionSeparator);
                }
            }
            return ret;
        }
        internal IEnumerable<IGroupAction> DeserializeGroupActionSubmenu(actionSubmenu actiongroup, bool isGroup)
        {
            var ret = new List<IGroupAction>();
            if (actiongroup.Items == null) return ret;
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
                        ShowExecute = i.showIf,
                        HeaderExecute = i.getName,
                        DefaultAction = bool.Parse(i.@default.ToString())
                    };
                    add.IsGroup = isGroup;
                    ret.Add(add);
                }
                if (item is actionSubmenu)
                {
                    var i = item as actionSubmenu;
                    var addgroup = new GroupActionSubmenu
                    {
                        Children = new List<IGroupAction>(),
                        Name = i.menu,
                        IsGroup = isGroup,
                        ShowExecute = i.showIf,
                        HeaderExecute = i.getName
                    };
                    addgroup.Children = this.DeserializeGroupActionSubmenu(i, isGroup);
                    ret.Add(addgroup);
                }
                if (item is actionSeparator)
                {
                    var i = item as actionSeparator;
                    var groupActionSeparator = new GroupActionSeparator
                    {
                        IsGroup = isGroup,
                        ShowExecute = i.showIf
                    };
                    ret.Add(groupActionSeparator);
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
            var parsedRootPath = string.Join("", rootPath, "\\");

            var save = new game();
            save.id = game.Id.ToString();
            save.name = game.Name;
            save.card = new gameCard();
            save.version = game.Version.ToString();
            save.authors = string.Join(",", game.Authors);
            save.description = game.Description;
            save.gameurl = game.GameUrl;
            save.setsurl = game.SetsUrl;
            save.iconurl = game.IconUrl;
            save.tags = string.Join(" ", game.Tags);
            save.octgnVersion = game.OctgnVersion.ToString();
            save.markersize = game.MarkerSize.ToString();
            save.usetwosidedtable = game.UseTwoSidedTable ? boolean.True : boolean.False;
            save.changetwosidedtable = game.ChangeTwoSidedTable ? boolean.True : boolean.False;
            save.noteBackgroundColor = game.NoteBackgroundColor;
            save.noteForegroundColor = game.NoteForegroundColor;
            save.scriptVersion = game.ScriptVersion?.ToString();

            #region table
            save.table = SerializeTable(game.Table, parsedRootPath);
            #endregion table
            #region phases
            if (game.Phases != null)
            {
                var phaseList = new List<gamePhase>();
                foreach (var p in game.Phases)
                {
                    var gamePhase = new gamePhase
                    {
                        icon = (p.Icon ?? "").Replace(parsedRootPath, ""),
                        name = p.Name
                    };
                    phaseList.Add(gamePhase);
                }
                if (phaseList.Count > 0) save.phases = phaseList.ToArray();
            }
            #endregion phases
            #region gameBoards
            if (game.GameBoards != null && game.GameBoards.Count > 0)
            {
                var gameboards = new gameGameboards();
                var boardList = new List<gameBoardDef>();
                foreach (var gbdic in game.GameBoards)
                {
                    if (gbdic.Key == "")
                    {
                        gameboards.name = gbdic.Value.Name;
                        gameboards.height = gbdic.Value.Height.ToString();
                        gameboards.width = gbdic.Value.Width.ToString();
                        gameboards.x = gbdic.Value.XPos.ToString();
                        gameboards.y = gbdic.Value.YPos.ToString();
                        gameboards.src = (gbdic.Value.Source).Replace(parsedRootPath, "").Replace(rootPath, "");
                        continue;
                }
                    if (gbdic.Value.Source == null) continue;
                    var gameboard = new gameBoardDef
                    {
                        name = gbdic.Value.Name,
                        x = gbdic.Value.XPos.ToString(),
                        y = gbdic.Value.YPos.ToString(),
                        width = gbdic.Value.Width.ToString(),
                        height = gbdic.Value.Height.ToString(),
                        src = (gbdic.Value.Source).Replace(parsedRootPath, "").Replace(rootPath, "")
                    };
                    boardList.Add(gameboard);
                }
                if (boardList.Count > 0)
                {
                    gameboards.gameboard = boardList.ToArray();
                }
                if (gameboards.src != null || boardList.Count > 0)
                {
                    save.gameboards = gameboards;
            }
            }
            #endregion gameBoards
            #region shared

            if (game.GlobalPlayer != null)
            {
                var clist = new List<counter>();
                foreach (var c in game.GlobalPlayer.Counters)
                {
                    var counter = new counter
                    {
                        name = c.Name,
                        icon = (c.Icon ?? "").Replace(parsedRootPath, "").Replace(rootPath, ""),
                        reset = c.Reset ? boolean.True : boolean.False,
                        @default = c.Start.ToString()
                    };
                    clist.Add(counter);
                }
                var glist = new List<group>();
                foreach (var group in game.GlobalPlayer.Groups)
                {
                    glist.Add(SerializeGroup(group, parsedRootPath, new group()));
                }
                save.shared = new gameShared
                {
                    group = glist.ToArray(),
                    counter = clist.ToArray()
                };
            }
            #endregion shared
            #region player
            if (game.Player != null)
            {
                var ilist = new List<object>();
                foreach (var c in game.Player.Counters)
                {
                    var counter = new counter
                    {
                        name = c.Name,
                        icon = (c.Icon ?? "").Replace(parsedRootPath, "").Replace(rootPath, ""),
                        reset = c.Reset ? boolean.True : boolean.False,
                        @default = c.Start.ToString()
                    };
                    ilist.Add(counter);
                }
                foreach (var v in game.Player.GlobalVariables)
                {
                    var gamePlayerGlobalVariable = new gamePlayerGlobalvariable
                    {
                        name = v.Value.Name,
                        value = v.Value.Value
                    };
                    ilist.Add(gamePlayerGlobalVariable);
                }
                foreach (var g in game.Player.Groups)
                {
                    ilist.Add(SerializeGroup(g, parsedRootPath, new group()));
                }
                save.player = new gamePlayer
                {
                    Items = ilist.ToArray(),
                    summary = game.Player.IndicatorsFormat
                };
            }
            #endregion player
            #region documents

            if (game.Documents != null)
            {
                var documentList = new List<gameDocument>();
                foreach (var d in game.Documents)
                {
                    var gameDocument = new gameDocument
                    {
                        icon = (d.Icon ?? "").Replace(parsedRootPath, "").Replace(rootPath, ""),
                        name = d.Name,
                        src = (d.Source ?? "").Replace(parsedRootPath, "").Replace(rootPath, "")
                    };
                    documentList.Add(gameDocument);
                }
                if (documentList.Count > 0) save.documents = documentList.ToArray();
            }

            #endregion documents
            #region sounds

            if (game.Sounds != null)
            {
                var soundList = new List<gameSound>();
                foreach (var s in game.Sounds)
                {
                    var gameSound = new gameSound
                    {
                        name = s.Value.Name,
                        src = (s.Value.Src ?? "").Replace(parsedRootPath, "").Replace(rootPath, "")
                    };
                    soundList.Add(gameSound);
                }
                if (soundList.Count > 0) save.sounds = soundList.ToArray();
            }

            #endregion sounds
            #region symbols

            if (game.Symbols != null)
            {
                var symbolList = new List<gameSymbol>();
                foreach (var g in game.Symbols)
                {
                    var gameSymbol = new gameSymbol
                    {
                        name = g.Name,
                        src = (g.Source ?? "").Replace(parsedRootPath, "").Replace(rootPath, ""),
                        id = g.Id
                    };
                    symbolList.Add(gameSymbol);
                }
                if (symbolList.Count > 0) save.symbols= symbolList.ToArray();
            }

            #endregion symbols
            #region deck

            if (game.DeckSections != null)
            {
                var deckSectionList = new List<deckSection>();
                foreach (var s in game.DeckSections)
                {
                    var deckSection = new deckSection
                    {
                        name = s.Value.Name,
                        group = s.Value.Group.Name
                    };
                    deckSectionList.Add(deckSection);
                }
                save.deck = deckSectionList.ToArray();
            }

            if (game.SharedDeckSections != null)
            {
                var deckSectionList = new List<deckSection>();
                foreach (var s in game.SharedDeckSections)
                {
                    var deckSection = new deckSection
                    {
                        name = s.Value.Name,
                        group = s.Value.Group.Name
                    };
                    deckSectionList.Add(deckSection);
                }
                save.sharedDeck = deckSectionList.ToArray();
            }
            #endregion deck
            #region markers
            if (game.Markers != null)
            {
                var markersList = new List<gameMarker>();
                foreach (var m in game.Markers.Values)
                {
                    var marker = new gameMarker
                    {
                        name = m.Name,
                        src = (m.Source ?? "").Replace(parsedRootPath, "").Replace(rootPath, ""),
                        id = m.Id
                    };
                    markersList.Add(marker);
                }
                if (markersList.Count > 0) save.markers = markersList.ToArray();
            }
            #endregion markers
            #region card

            if (game.CustomProperties != null)
            {
                var propertyDefList = new List<propertyDef>();
                foreach (var p in game.CustomProperties)
                {
                    var propertyDef = new propertyDef
                    {
                        name = p.Name,
                        hidden = p.Hidden ? boolean.True : boolean.False,
                        type = (propertyDefType)Enum.Parse(typeof(propertyDefType), p.Type.ToString()),
                        ignoreText = p.IgnoreText ? boolean.True : boolean.False
                    };
                    switch (p.TextKind)
                    {
                        case PropertyTextKind.FreeText:
                            propertyDef.textKind = propertyDefTextKind.Free;
                            break;
                        case PropertyTextKind.Enumeration:
                            propertyDef.textKind = propertyDefTextKind.Enum;
                            break;
                        case PropertyTextKind.Tokens:
                            propertyDef.textKind = propertyDefTextKind.Tokens;
                            break;
                    }
                    propertyDefList.Add(propertyDef);
                }
                save.card.property = propertyDefList.ToArray();

                var sizeList = new List<cardsizeDef>();
                foreach (var c in game.CardSizes)
                {
                    if (c.Key == "")
                    {
                        save.card.name = c.Value.Name;
                        save.card.back = c.Value.Back.Replace(parsedRootPath, "").Replace(rootPath, "");
                        save.card.front = c.Value.Front.Replace(parsedRootPath, "").Replace(rootPath, "");
                        save.card.height = c.Value.Height.ToString();
                        save.card.width = c.Value.Width.ToString();
                        save.card.cornerRadius = c.Value.CornerRadius.ToString();
                        save.card.backCornerRadius = c.Value.BackCornerRadius.ToString();
                        continue;
            }
                    if (c.Value.Front == null || c.Value.Back == null) continue;
                    var cardSize = new cardsizeDef
                    {
                        name = c.Value.Name,
                        front = c.Value.Front.Replace(parsedRootPath, "").Replace(rootPath, ""),
                        height = c.Value.Height.ToString(),
                        width = c.Value.Width.ToString(),
                        cornerRadius = c.Value.CornerRadius.ToString(),
                        back = c.Value.Back.Replace(parsedRootPath, "").Replace(rootPath, ""),
                        backHeight = c.Value.BackHeight.ToString(),
                        backWidth = c.Value.BackWidth.ToString(),
                        backCornerRadius = c.Value.BackCornerRadius.ToString()
                    };
                    sizeList.Add(cardSize);
            }
                save.card.size = sizeList.ToArray();
            }
            #endregion card
            #region fonts

            var fontList = new List<gameFont>();
            if (game.ChatFont.IsSet())
            {
                var gameFont = new gameFont
                {
                    src = (game.ChatFont.Src ?? "").Replace(parsedRootPath, "").Replace(rootPath, ""),
                    size = game.ChatFont.Size.ToString(),
                    target = fonttarget.chat
                };
                fontList.Add(gameFont);
            }
            if (game.ContextFont.IsSet())
            {
                var gameFont = new gameFont
                {
                    src = (game.ContextFont.Src ?? "").Replace(parsedRootPath, "").Replace(rootPath, ""),
                    size = game.ContextFont.Size.ToString(),
                    target = fonttarget.context
                };
                fontList.Add(gameFont);
            }
            if (game.NoteFont.IsSet())
            {
                var gameFont = new gameFont
                {
                    src = (game.NoteFont.Src ?? "").Replace(parsedRootPath, "").Replace(rootPath, ""),
                    size = game.NoteFont.Size.ToString(),
                    target = fonttarget.notes
                };
                fontList.Add(gameFont);
            }
            if (game.DeckEditorFont.IsSet())
            {
                var gameFont = new gameFont
                {
                    src = (game.DeckEditorFont.Src ?? "").Replace(parsedRootPath, "").Replace(rootPath, ""),
                    size = game.DeckEditorFont.Size.ToString(),
                    target = fonttarget.deckeditor
                };
                fontList.Add(gameFont);
            }
            if (fontList.Count > 0) save.fonts = fontList.ToArray();
            #endregion fonts
            #region scripts

            if (game.Scripts != null)
            {
                var scriptList = new List<gameScript>();
                foreach (var s in game.Scripts)
                {
                    var gameScript = new gameScript
                    {
                        src = s
                    };
                    scriptList.Add(gameScript);
                }
                if (scriptList.Count > 0) save.scripts = scriptList.ToArray();
            }

            #endregion scripts
            #region events

            if (game.Events != null)
            {
                var eventList = new List<gameEvent>();
                foreach (var e in game.Events.SelectMany(x => x.Value))
                {
                    var gameEvent = new gameEvent
                    {
                        name = e.Name,
                        action = e.PythonFunction
                    };
                    eventList.Add(gameEvent);
                }
                if (eventList.Count > 0) save.events = eventList.ToArray();
            }

            #endregion events
            #region proxygen

            save.proxygen = new gameProxygen
            {
                definitionsrc = game.ProxyGenSource
            };
            #endregion proxygen
            #region globalvariables

            if (game.GlobalVariables != null)
            {
                var globalVariableList = new List<gameGlobalvariable>();
                foreach (var g in game.GlobalVariables)
                {
                    var globalVariable = new gameGlobalvariable
                    {
                        name = g.Value.Name,
                        value = g.Value.Value
                    };
                    globalVariableList.Add(globalVariable);
                }
                save.globalvariables = globalVariableList.ToArray();
            }

            #endregion globalvariables

            #region GameModes

            if (game.Modes != null)
            {
                var gameModeList = new List<gameGameMode>();
                foreach (var m in game.Modes)
                {
                    var gameMode = new gameGameMode
                    {
                        name = m.Name,
                        image = m.Image = (m.Image ?? "").Replace(parsedRootPath, "").Replace(rootPath, ""),
                        playerCount = m.PlayerCount.ToString(),
                        shortDescription = m.ShortDescription
                    };

                    gameModeList.Add(gameMode);
                }
                save.gameModes = gameModeList.ToArray();
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

        internal table SerializeTable(Group group, string rootPath)
        {
            if (group == null)
                return null;
            var ret = new table
            {
                name = group.Name,
                background = group.Background == null ? null : (group.Background ?? "").Replace(rootPath, ""),
                height = group.Height.ToString(),
                width = group.Width.ToString(),
                ordered = group.Ordered ? boolean.True : boolean.False,
                shortcut = group.Shortcut,
                moveto = group.MoveTo ? boolean.True : boolean.False
            };
            if (group.CardActions != null)
            {
                var itemList = SerializeActions(group.CardActions, false).ToList();
                itemList.AddRange(SerializeActions(group.GroupActions, true).ToArray());
                ret.Items = itemList.ToArray();
            }
            switch (group.BackgroundStyle)
            {
                case BackgroundStyle.Stretch:
                    ret.backgroundStyle = tableBackgroundStyle.stretch;
                    break;
                case BackgroundStyle.Tile:
                    ret.backgroundStyle = tableBackgroundStyle.tile;
                    break;
                case BackgroundStyle.Uniform:
                    ret.backgroundStyle = tableBackgroundStyle.uniform;
                    break;
                case BackgroundStyle.UniformToFill:
                    ret.backgroundStyle = tableBackgroundStyle.uniformToFill;
                    break;
            }
            switch (group.Visibility)
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

        internal group SerializeGroup(Group group, string rootPath, group ret)
        {
            if (group == null)
                return null;
            ret.name = group.Name;
            ret.icon = group.Icon == null ? null : (group.Icon ?? "").Replace(rootPath, "");
            ret.ordered = group.Ordered ? boolean.True : boolean.False;
            ret.shortcut = group.Shortcut;
            ret.moveto = group.MoveTo ? boolean.True : boolean.False;
            List<baseAction> itemList = new List<baseAction>();
            if (group.CardActions != null)
                itemList.AddRange(SerializeActions(group.CardActions, false));
            if (group.GroupActions != null)
                itemList.AddRange(SerializeActions(group.GroupActions, true));
            if (itemList.Count > 0)
                ret.Items = itemList.ToArray();
            switch (group.ViewState)
            {
                case GroupViewState.Collapsed:
                    ret.viewState = groupViewState.collapsed;
                    break;
                case GroupViewState.Pile:
                    ret.viewState = groupViewState.pile;
                    break;
                case GroupViewState.Expanded:
                    ret.viewState = groupViewState.expanded;
                    break;
            }
            switch (group.Visibility)
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

        internal IEnumerable<baseAction> SerializeActions(IEnumerable<IGroupAction> actions, bool IsGroup)
        {
            foreach (var a in actions)
            {
                if (a is GroupAction groupAction)
                {
                    action ret = IsGroup ? (action)new groupAction() : new cardAction();
                    ret.@default = groupAction.DefaultAction ? boolean.True : boolean.False;
                    ret.showIf = groupAction.ShowExecute;
                    ret.getName = groupAction.HeaderExecute;
                    if (groupAction.IsBatchExecutable)
                        ret.batchExecute = groupAction.Execute;
                    else
                        ret.execute = groupAction.Execute;
                    ret.menu = groupAction.Name;
                    ret.shortcut = groupAction.Shortcut;
                    yield return ret;
                }
                else if (a is GroupActionSubmenu actionSubmenu)
                {
                    var ret = IsGroup ? (actionSubmenu)new groupActionSubmenu() : new cardActionSubmenu();
                    ret.menu = actionSubmenu.Name;
                    ret.showIf = actionSubmenu.ShowExecute;
                    ret.getName = actionSubmenu.HeaderExecute;
                    ret.Items = SerializeActions(actionSubmenu.Children, IsGroup).ToArray();
                    yield return ret;
                }
                else if (a is GroupActionSeparator actionSeparator)
                {
                    var ret = IsGroup ? (actionSeparator)new groupActionSeparator() : new cardActionSeparator();
                    ret.showIf = actionSeparator.ShowExecute;
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
                var setxsd = libAss.GetManifestResourceNames().FirstOrDefault(x => x.Contains("CardSet.xsd"));
                setSchema = XmlSchema.Read(libAss.GetManifestResourceStream(setxsd), (sender, args) => Console.WriteLine(args.Exception));
                return setSchema;
            }
        }

        internal static XmlSchema setSchema;

        /// <summary>
        /// The <see cref="Octgn.DataNew.Entities.Game"/> this <see cref="Set"/> belongs to. If this is set,
        /// any <see cref="Set"/> that is serialized or deserialized will be associated with
        /// this <see cref="Octgn.DataNew.Entities.Game"/>, reguardless of what the data says.
        /// </summary>
        public Game Game { get; set; }

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
                var hidden = root.Attribute("hidden");
                ret.Hidden = (hidden == null) ? (bool)false : bool.Parse(hidden.Value);
                ret.Name = root.Attribute("name").Value;
                ret.Filename = fileName;
                ret.GameId = new Guid(root.Attribute("gameId").Value);
                if (root.Attribute("shortName") != null)
                    ret.ShortName = root.Attribute("shortName").Value;
                ret.ReleaseDate = root.Attribute("releaseDate") == null ? new DateTime(0) : DateTime.Parse(root.Attribute("releaseDate").Value);
                if (root.Attribute("description") != null)
                    ret.Description = root.Attribute("description").Value;
                ret.Cards = new List<Card>();
                ret.Packs = new List<Pack>();
                ret.PackageName = "";
                ret.InstallPath = directory;
                ret.DeckPath = Path.Combine(ret.InstallPath, "Decks");
                ret.PackUri = Path.Combine(ret.InstallPath, "Cards");
                var gameImageInstallPath = Path.Combine(Config.Instance.ImageDirectoryFull, ret.GameId.ToString());
                ret.ImageInstallPath = Path.Combine(gameImageInstallPath, "Sets", ret.Id.ToString());
                ret.ImagePackUri = Path.Combine(ret.ImageInstallPath, "Cards");
                ret.ProxyPackUri = Path.Combine(ret.ImagePackUri, "Proxies");

                if (!Directory.Exists(ret.PackUri)) Directory.CreateDirectory(ret.PackUri);
                if (!Directory.Exists(ret.ImagePackUri)) Directory.CreateDirectory(ret.ImagePackUri);
                if (!Directory.Exists(ret.ProxyPackUri)) Directory.CreateDirectory(ret.ProxyPackUri);
                Game = DbContext.Get().Games.First(x => x.Id == ret.GameId);
                foreach (var cardXml in doc.Document.Descendants("card"))
                {
                    var baseCardPropertySet = new CardPropertySet
                    {
                        Name = cardXml.Attribute("name").Value,
                        Type = "",
                        Properties = new Dictionary<PropertyDef, object>(),
                        Size = DeserializeCardSize(cardXml)
                    };
                    var card = new Card(new Guid(cardXml.Attribute("id").Value), ret.Id, cardXml.Attribute("name").Value, cardXml.Attribute("id").Value, "", baseCardPropertySet.Size, new Dictionary<string, CardPropertySet>());

                    // deserialize the base card properties
                    var xmlBaseCardProperties = cardXml.Descendants("property").Where(x => x.Parent.Name == "card");
                    DeserializeCardPropertySet(xmlBaseCardProperties, baseCardPropertySet);
                    card.PropertySets.Add("", baseCardPropertySet);

                    // Add all of the other property sets
                    foreach (var altXml in cardXml.Descendants("alternate"))
                    {
                        var altPropertySet = new CardPropertySet
                        {
                            Name = altXml.Attribute("name").Value,
                            Properties = new Dictionary<PropertyDef, object>(),
                            Type = altXml.Attribute("type").Value,
                            Size = DeserializeCardSize(altXml)
                        };

                        // deserialize the alternate card properties
                        var xmlAltProperties = altXml.Descendants("property");
                        DeserializeCardPropertySet(xmlAltProperties, altPropertySet);
                        card.PropertySets.Add(altPropertySet.Type, altPropertySet);
                    }

                    (ret.Cards as List<Card>).Add(card);
                }
                foreach (var p in doc.Document.Descendants("pack"))
                {
                    var pack = new Pack
                    {
                        Id = new Guid(p.Attribute("id").Value),
                        Name = p.Attribute("name").Value,
                        Items = DeserializePack(p.Elements()),
                        Set = ret
                    };
                    foreach (var includeXml in p.Elements("include"))
                    {
                        var include = new Include
                        {
                            Id = new Guid(includeXml.Attribute("id").Value),
                            SetId = new Guid(includeXml.Attribute("set").Value)
                        };
                        var includesPropertySet = new CardPropertySet() { Properties = new Dictionary<PropertyDef, object>() };
                        DeserializeCardPropertySet(includeXml.Elements("property"), includesPropertySet);
                        include.Properties = includesPropertySet.Properties.Select(x => new PickProperty() { Property = x.Key, Value = x.Value }).ToList();

                        pack.Includes.Add(include);
                    }
                    (ret.Packs as List<Pack>).Add(pack);
                }
                foreach (var m in doc.Document.Descendants("marker"))
                {
                    // set sourced markers are obsolete, must import them into the game for backwards compatibility
                    var marker = new GameMarker
                    {
                        Id = m.Attribute("id").Value,
                        Name = m.Attribute("name").Value
                    };
                    var markerDirectory = new DirectoryInfo(Path.Combine(directory, "Markers"));
                    var markerPath = markerDirectory.Exists == false ? null : markerDirectory.GetFiles(marker.Id.ToString() + ".*", SearchOption.TopDirectoryOnly).First();
                    marker.Source = markerPath == null ? null : Path.Combine(directory, "Markers", markerPath.FullName);
                    if (!Game.Markers.ContainsKey(marker.Id))
                        Game.Markers.Add(marker.Id, marker);
                }
            }

            if (ret.Cards == null) ret.Cards = new Card[0];
           // if (ret.Markers == null) ret.Markers = new Marker[0];
            if (ret.Packs == null) ret.Packs = new Pack[0];
            //Console.WriteLine(timer.ElapsedMilliseconds);
            return ret;
        }

        private CardSize DeserializeCardSize(XElement element)
        {
            var altSize = element.Attribute("size");
            if (altSize != null)
            {
                if (Game.CardSizes.ContainsKey(altSize.Value) == false)
                    throw new UserMessageException(Octgn.Library.Localization.L.D.Exception__BrokenGameContactDev_Format, Game.Name);

                return Game.CardSizes[altSize.Value];
            }
            return Game.CardSizes[""];
        }

        private void DeserializeCardPropertySet(IEnumerable<XElement> cardPropertyElements, CardPropertySet propertySet)
        {
            foreach (var propertyElement in cardPropertyElements)
            {
                PropertyDef gameDefinedProperty = Game.CustomProperties.FirstOrDefault(x => x.Name == propertyElement.Attribute("name").Value);
                if (gameDefinedProperty == null)
                {
                    throw new UserMessageException(Octgn.Library.Localization.L.D.Exception__BrokenGameContactDev_Format, Game.Name);
                }
                if (gameDefinedProperty.Type is PropertyType.RichText)
                {
                    var span = new RichSpan();
                    if (propertyElement.IsEmpty && propertyElement.Attribute("value") != null)
                    {
                        propertyElement.SetValue(propertyElement.Attribute("value").Value);
                    }
                    DeserializeRichCardProperty(span, propertyElement);
                    RichTextPropertyValue propertyDefValue = null;
                    if (span.Items.Count > 0)
                        propertyDefValue = new RichTextPropertyValue { Value = span };
                    propertySet.Properties.Add(gameDefinedProperty, propertyDefValue);
                }
                else
                {
                    propertySet.Properties.Add(gameDefinedProperty, propertyElement.IsEmpty ? propertyElement.Attribute("value")?.Value : propertyElement.Value);
                }

            }
            /* don't add missing properties to the database
            foreach (var gameProperties in game.CustomProperties)
            {
                if (!propertySet.Properties.ContainsKey(gameProperties))
                {
                    var blankCardProperty = gameProperties.Clone() as PropertyDef;
                    blankCardProperty.IsUndefined = true;
                    if (blankCardProperty.Type is PropertyType.RichText)
                    {
                        propertySet.Properties.Add(blankCardProperty, new RichTextPropertyValue() { Value = new RichSpan() });
                    }
                    else
                    {
                        propertySet.Properties.Add(blankCardProperty, null);
                    }
                }
            }*/
        }

        private void DeserializeRichCardProperty(RichSpan span, XElement xmlNode)
        {
            foreach (XNode child in xmlNode.Nodes())
            {
                if (child is XText text)
                {
                    span.Items.Add(new RichText() { Text = text.Value });
                }
                else if (child is XElement element)
                {
                    switch (element.Name.ToString().ToUpper())
                    {
                        case "B":
                        case "BOLD":
                            {
                                RichSpan boldSpan = new RichSpan();
                                DeserializeRichCardProperty(boldSpan, element);
                                boldSpan.Type = RichSpanType.Bold;
                                span.Items.Add(boldSpan);
                                break;
                            }
                        case "I":
                        case "ITALIC":
                            {
                                RichSpan italicSpan = new RichSpan();
                                DeserializeRichCardProperty(italicSpan, element);
                                italicSpan.Type = RichSpanType.Italic;
                                span.Items.Add(italicSpan);
                                break;
                            }
                        case "U":
                        case "UNDERLINE":
                            {
                                RichSpan underlineSpan = new RichSpan();
                                DeserializeRichCardProperty(underlineSpan, element);
                                underlineSpan.Type = RichSpanType.Underline;
                                span.Items.Add(underlineSpan);
                                break;
                            }
                        case "C":
                        case "COLOR":
                            {
                                RichColor colorSpan = new RichColor();
                                DeserializeRichCardProperty(colorSpan, element);
                                colorSpan.Type = RichSpanType.Color;
                                var regexColorCode = new Regex.Regex("^#[a-fA-F0-9]{6}$");
                                var color = element.Attribute("value").Value;
                                if (!regexColorCode.IsMatch(color))
                                    throw new InvalidOperationException($"Invalid HEX Color Code: {color}");
                                colorSpan.Attribute = color;
                                span.Items.Add(colorSpan);
                                break;
                            }
                        case "S":
                        case "SYMBOL":
                            {
                                var symbolId = element.Attribute("value").Value;
                                Symbol symbol = Game.Symbols.FirstOrDefault(x => x.Id == symbolId) ?? throw new InvalidOperationException($"Could not find symbol {symbolId}");
                                RichSymbol symbolSpan = new RichSymbol
                                {
                                    Type = RichSpanType.Symbol,
                                    Attribute = symbol,
                                    Text = element.FirstNode.ToString()
                                };
                                span.Items.Add(symbolSpan);
                                break;
                            }
                    }
                }
            }
        }

        internal OptionsList DeserializeOptions(XElement element)
        {
            var ret = new OptionsList();
            foreach (var op in element.Elements("option"))
            {
                var option = new Option();
                var probAtt = op.Attributes("probability").FirstOrDefault();
                option.Probability = double.Parse(probAtt != null ? probAtt.Value : "1", CultureInfo.InvariantCulture);
                option.Items = DeserializePack(op.Elements());
                ret.Options.Add(option);
            }
            return ret;
        }

        internal List<object> DeserializePack(IEnumerable<XElement> element)
        {
            var ret = new List<object>();
            foreach (var e in element)
            {
                switch (e.Name.LocalName)
                {
                    case "options":
                        ret.Add(this.DeserializeOptions(e));
                        break;
                    case "pick":
                        var pick = new Pick();
                        var qtyAttr = e.Attributes().FirstOrDefault(x => x.Name.LocalName == "qty");
                        if (qtyAttr != null) pick.Quantity = qtyAttr.Value == "unlimited" ? -1 : int.Parse(qtyAttr.Value);
                        var propertyList = new List<PickProperty>();
                        var baseProp = new PickProperty
                        {
                            Property = Game.CustomProperties.FirstOrDefault(x => x.Name == e.Attribute("key").Value),
                            Value = e.Attribute("value").Value
                        };
                        propertyList.Add(baseProp);
                        foreach (var p in e.Elements("property"))
                        {
                            var prop = new PickProperty
                            {
                                Property = Game.CustomProperties.FirstOrDefault(x => x.Name == p.Attribute("key").Value),
                                Value = p.Attribute("value").Value
                            };
                            propertyList.Add(prop);
                        }
                        pick.Properties = propertyList;
                        ret.Add(pick);
                        break;
                }
            }
            return ret;
        }

        public byte[] Serialize(object obj)
        {
            if ((obj is Set) == false)
                throw new InvalidOperationException("obj must be typeof Set");
            var set = obj as Set;
            var game = Game ?? DbContext.Get().Games.First(x => x.Id == set.GameId);
            var rootPath = new DirectoryInfo(set.InstallPath).FullName;
            var parsedRootPath = string.Join("", rootPath, "\\");

            var save = new set
            {
                name = set.Name.ToString(),
                id = set.Id.ToString(),
                gameId = set.GameId.ToString(),
                hidden = set.Hidden,
                shortName = set.ShortName,
                releaseDate = set.ReleaseDate,
                description = set.Description
            };

            var packs = new List<setPack>();
            foreach (var setpack in set.Packs)
            {
                var pack = new setPack
                {
                    name = setpack.Name.ToString(),
                    id = setpack.Id.ToString()
                };

                var packItems = SerializePack(setpack.Items).ToList();
                foreach (Include include in setpack.Includes)
                {
                    packItems.Add(new include()
                    {
                        id = include.Id.ToString(),
                        set = include.SetId.ToString(),
                        property = include.Properties.Select(x => new includeProperty() { name = x.Property.Name.ToString(), value = x.Value?.ToString() }).ToArray()
                    }
                    );
                }
                pack.Items = packItems.ToArray();
                packs.Add(pack);
            }
            save.packaging = packs.ToArray();

            var cards = new List<setCard>();
            foreach (var c in set.Cards)
            {
                var card = new setCard
                {
                //    name = c.Name.ToString(),
                    id = c.Id.ToString(),
                };
                List<setCardAlternate> alts = new List<setCardAlternate>();
                foreach (var propset in c.PropertySets)
                {
                    if (propset.Key == "")
                    {
                        var props = new List<property>();
                        foreach (var p in propset.Value.Properties)
                        {
                            if (p.Value == null) continue;
                            var prop = new property
                            {
                                name = p.Key.Name,
                                value = p.Value.ToString()
                            };
                            props.Add(prop);
                        }
                        card.name = propset.Value.Name;
                        card.property = props.ToArray();
                        //  card.size = (propset.Value.Size.Name == game.CardSize.Name) ? null : propset.Value.Size.Name;
                        if (propset.Value.Size != game.CardSizes[""])
                        {
                            card.size = propset.Value.Size.Name;
                        }
                    }
                    else
                    {
                        var alt = new setCardAlternate
                        {
                            name = propset.Value.Name,
                            type = propset.Value.Type
                        };
                        if (propset.Value.Size != game.CardSizes[""])
                        {
                            alt.size = propset.Value.Size.Name;
                        }
                        var altprops = new List<property>();
                        foreach (var p in propset.Value.Properties)
                        {
                            if (p.Value == null) continue;
                            var prop = new property
                            {
                                name = p.Key.Name,
                                value = p.Value.ToString()
                            };
                            altprops.Add(prop);
                        }
                        alt.property = altprops.ToArray();
                        alts.Add(alt);
                    }
                    card.alternate = alts.ToArray();
                }
                cards.Add(card);
            }
            save.cards = cards.ToArray();

            var serializer = new XmlSerializer(typeof(set));
            Directory.CreateDirectory(new FileInfo(set.Filename).Directory.FullName);

            using (var fs = File.Open(set.Filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                serializer.Serialize(fs, save);
            }
            return File.ReadAllBytes(set.Filename);
        }

        public IEnumerable<object> SerializePack(List<object> packitems)
        {
            foreach (var item in packitems)
            {
                if (item is Pick)
                {
                    var ret = new pick();
                    Pick pick = item as Pick;
                    ret.qty = pick.Quantity == -1 ? "unlimited" : pick.Quantity.ToString();

                    var propertylist = new List<pickProperty>();
                    foreach (var prop in pick.Properties)
                    {
                        if (ret.key == null)
                        {
                            ret.key = prop.Property.Name;
                            ret.value = prop.Value.ToString();
                        }
                        else
                        {
                            var property = new pickProperty
                            {
                                key = prop.Property.Name,
                                value = prop.Value?.ToString()
                            };
                            propertylist.Add(property);
                        }
                    }
                    if (propertylist.Count > 0)
                        ret.property = propertylist.ToArray();

                    yield return ret;
                }
                else if (item is OptionsList)
                {
                    var ret = new options();
                    var optionlist = new List<optionsOption>();
                    foreach (var opt in (item as OptionsList).Options)
                    {
                        var option = new optionsOption
                        {
                            probability = opt.Probability,
                            Items = SerializePack(opt.Items).ToArray()
                        };
                        optionlist.Add(option);
                    }
                    ret.option = optionlist.ToArray();
                    yield return ret;
                }
            }
        }
    }

    public class GameScriptSerializer : IFileDbSerializer
    {
        public ICollectionDefinition Def { get; set; }

        internal Guid GameId { get; set; }
        public Game Game { get; set; }

        public GameScriptSerializer(Guid gameId)
        {
            GameId = gameId;
        }

        public object Deserialize(string fileName)
        {
            var ret = new GameScript
            {
                Path = fileName,
                GameId = GameId,
                Script = File.ReadAllText(fileName)
            };
            return ret;
        }

        public byte[] Serialize(object obj)
        {
            if ((obj is GameScript) == false)
                throw new InvalidOperationException("obj must be typeof GameScript");
            var Script = obj as GameScript;

            var save = new gameScript()
            {
                src = Script.Script
            };


            Directory.CreateDirectory(new FileInfo(Script.Path).Directory.FullName);

            File.WriteAllText(Script.Path, Script.Script);
            return File.ReadAllBytes(Script.Path);
        }
    }

    public class ProxyGeneratorSerializer : IFileDbSerializer
    {
        public ICollectionDefinition Def { get; set; }
        internal Guid GameId { get; set; }

        /// <summary>
        /// The <see cref="Octgn.DataNew.Entities.Game"/> this <see cref="ProxyDefinition"/> belongs to. If this is set,
        /// any <see cref="ProxyDefinition"/> that is serialized or deserialized will be associated with
        /// this <see cref="Octgn.DataNew.Entities.Game"/>, reguardless of what the data says.
        /// </summary>
        public Game Game { get; set; }
        public ProxyGeneratorSerializer(Guid gameId)
        {
            GameId = gameId;
        }

        public object Deserialize(string fileName)
        {
            var game = Game ?? DbContext.Get().Games.First(x => x.Id == GameId);
            var ret = new ProxyDefinition(GameId, fileName, new FileInfo(game.Filename).Directory.FullName);
            return ret;
        }

        public byte[] Serialize(object obj)
        {
            if ((obj is ProxyDefinition) == false)
                throw new InvalidOperationException("obj must be typeof ProxyDefinition");
            var proxyDef = obj as ProxyDefinition;
            var game = Game ?? DbContext.Get().Games.First(x => x.Id == (Guid)proxyDef.Key);
            var rootPath = new DirectoryInfo(proxyDef.RootPath).FullName;
            var parsedRootPath = string.Join("", rootPath, "\\");
            var fullPath = Path.Combine(game.InstallPath, game.ProxyGenSource);

            var save = new templates
            {
            };

            #region blocks
            var blocks = new List<templatesBlock>();
            foreach (BlockDefinition blockdef in proxyDef.BlockManager.GetBlocks())
            {
                var block = new templatesBlock();
                if (blockdef.type == "overlay")
                    block.type = blocktype.overlay;
                else if (blockdef.type == "text")
                    block.type = blocktype.text;
                block.id = blockdef.id;
                block.src = blockdef.src;

                var location = new templatesBlockLocation
                {
                    x = blockdef.location.x.ToString(),
                    y = blockdef.location.y.ToString()
                };
                if (blockdef.location.rotate != 0)
                {
                    location.rotate = blockdef.location.rotate.ToString();
                }
                if (blockdef.location.flip)
                {
                    location.flip = proxyBoolean.True;
                }
                if (blockdef.location.altrotate)
                {
                    location.altrotate = proxyBoolean.True;
                }
                block.location = location;
                if (block.type == blocktype.text)
                {
                    if (blockdef.border.size != 0)
                    {
                        var border = new templatesBlockBorder
                        {
                            size = blockdef.border.size.ToString(),
                            color = System.Drawing.ColorTranslator.ToHtml(blockdef.border.color)
                        };
                        block.border = border;
                    }
                    var wordwrap = new templatesBlockWordwrap();
                    if (blockdef.wordwrap.height != 0)
                    {
                        wordwrap.height = blockdef.wordwrap.height.ToString();
                    }
                    if (blockdef.wordwrap.width != 0)
                    {
                        wordwrap.width = blockdef.wordwrap.width.ToString();
                    }
                    switch (blockdef.wordwrap.align)
                    {
                        case "center":
                            wordwrap.align = alignment.center;
                            break;
                        case "far":
                            wordwrap.align = alignment.far;
                            break;
                        default:
                            wordwrap.align = alignment.near;
                            break;
                    }
                    switch (blockdef.wordwrap.valign)
                    {
                        case "center":
                            wordwrap.valign = alignment.center;
                            break;
                        case "far":
                            wordwrap.valign = alignment.far;
                            break;
                        default:
                            wordwrap.valign = alignment.near;
                            break;
                    }
                    if (blockdef.wordwrap.shrinkToFit)
                    {
                        wordwrap.shrinktofit = proxyBoolean.True;
                    }
                    block.wordwrap = wordwrap;

                    var text = new templatesBlockText();
                    if (blockdef.text.color != null)
                    {
                        text.color = System.Drawing.ColorTranslator.ToHtml(blockdef.text.color);
                    };
                    if (blockdef.text.font != null)
                    {
                        text.font = blockdef.text.font;
                    }
                    if (blockdef.text.size != 0)
                    {
                        text.size = blockdef.text.size.ToString();
                    }
                    block.text = text;
                }
                blocks.Add(block);
            }           
            save.blocks = blocks.ToArray();
            #endregion

            #region templates
            var templates = new List<templatesTemplate>();
            foreach (TemplateDefinition templatedef in proxyDef.TemplateSelector.GetTemplates())
            {
                var template = new templatesTemplate();
                if (templatedef.defaultTemplate)
                {
                    template.@default = proxyBoolean.True;
                }
                template.src = templatedef.src;

                var matches = new List<templatesTemplateMatch>();
                foreach (Property matchdef in templatedef.Matches)
                {
                    var match = new templatesTemplateMatch()
                    {
                        name = matchdef.Name,
                        value = matchdef.Value
                    };
                    matches.Add(match);
                }
                template.matches = matches.ToArray();

                var overlayblocks = new List<object>();
                foreach (LinkDefinition.LinkWrapper overlaydef in templatedef.OverlayBlocks)
                {
                    var item = SerializeLinkWrapper(overlaydef);
                    overlayblocks.Add(item);
                };
                template.overlayblocks = overlayblocks.ToArray();

                var textblocks = new List<object>();
                foreach (LinkDefinition.LinkWrapper textdef in templatedef.TextBlocks)
                {
                    var item = SerializeLinkWrapper(textdef);
                    textblocks.Add(item);
                };
                template.textblocks = textblocks.ToArray();
                templates.Add(template);
            }
            

            save.template = templates.ToArray();
            #endregion

            ///END
            ///

            var serializer = new XmlSerializer(typeof(templates));
            Directory.CreateDirectory(new FileInfo(fullPath).Directory.FullName);

            using (var fs = File.Open(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                serializer.Serialize(fs, save);
            }
            return File.ReadAllBytes(fullPath);
        }

        public object SerializeLinkWrapper(LinkDefinition.LinkWrapper wrapper)
        {
            if (wrapper.Conditional != null)
            {
                if (wrapper.Conditional.ifNode != null)
                {
                    var conditional = new conditional();
                    var conditionalitems = new List<object>();
                    var ifCondition = new conditionalIF
                    {
                        property = wrapper.Conditional.ifNode.property
                    };
                    if (wrapper.Conditional.ifNode.value != null)
                    {
                        ifCondition.value = wrapper.Conditional.ifNode.value;
                    }
                    if (wrapper.Conditional.ifNode.contains != null)
                    {
                        ifCondition.contains = wrapper.Conditional.ifNode.contains;
                    }
                    var ifConditionItems = new List<link>();
                    foreach (var linkItem in wrapper.Conditional.ifNode.linkList)
                    {
                        ifConditionItems.Add(SerializeLink(linkItem.Link));
                    }
                    ifCondition.link = ifConditionItems.ToArray();
                    conditionalitems.Add(ifCondition);

                    foreach (var conditionaldef in wrapper.Conditional.elseifNodeList)
                    {
                        var elseIfCondition = new conditionalElseif
                        {
                            property = conditionaldef.property
                        };
                        if (conditionaldef.value != null)
                        {
                            elseIfCondition.value = conditionaldef.value;
                        }
                        if (conditionaldef.contains != null)
                        {
                            elseIfCondition.contains = conditionaldef.contains;
                        }
                        var elseIfConditionItems = new List<link>();
                        foreach (var linkItem in conditionaldef.linkList)
                        {
                            elseIfConditionItems.Add(SerializeLink(linkItem.Link));
                        }
                        elseIfCondition.link = elseIfConditionItems.ToArray();
                        conditionalitems.Add(elseIfCondition);

                    }

                    if (wrapper.Conditional.elseNode != null)
                    {
                        var elseCondition = new conditionalElse();
                        var elseConditionItems = new List<link>();
                        foreach (var linkItem in wrapper.Conditional.elseNode.linkList)
                        {
                            elseConditionItems.Add(SerializeLink(linkItem.Link));
                        }
                        elseCondition.link = elseConditionItems.ToArray();
                        conditionalitems.Add(elseCondition);
                    }
                    conditional.Items = conditionalitems.ToArray();
                    return conditional;
                }
                if (wrapper.Conditional.switchProperty != null)
                {
                    var conditionalswitch = new conditionalSwitch
                    {
                        property = wrapper.Conditional.switchProperty
                    };
                    var switchitems = new List<conditionalSwitchCase>();

                    foreach (CaseDefinition switchdef in wrapper.Conditional.switchNodeList)
                    {
                        var switchCase = new conditionalSwitchCase();
                        if (switchdef.value != null)
                        {
                            switchCase.value = switchdef.value;
                        }
                        if (switchdef.contains != null)
                        {
                            switchCase.contains = switchdef.contains;
                        }
                        if (switchdef.switchBreak == false)
                        {
                            switchCase.@break = proxyBoolean.False;
                        }
                        var switchItems = new List<link>();
                        foreach (var linkItem in switchdef.linkList)
                        {
                            switchItems.Add(SerializeLink(linkItem.Link));
                        }
                        switchCase.link = switchItems.ToArray();
                        switchitems.Add(switchCase);
                    }

                    conditionalswitch.@case = switchitems.ToArray();
                    if (wrapper.Conditional.elseNode != null)
                    {
                        var defaultCaseItems = new List<link>();
                        foreach (var linkItem in wrapper.Conditional.elseNode.linkList)
                        {
                            defaultCaseItems.Add(SerializeLink(linkItem.Link));
                        }
                        conditionalswitch.@default = defaultCaseItems.ToArray();
                    }
                    var conditional = new conditional();
                    conditional.Items = new List<conditionalSwitch>() { conditionalswitch }.ToArray();
                    return conditional;
                }
            }
            if (wrapper.Link != null)
            {
                return SerializeLink(wrapper.Link);
            }
            return null;
        }

        public link SerializeLink(LinkDefinition linkdef)
        {
            link link = new link
            {
                separator = linkdef.Separator,
                block = linkdef.Block
            };
            var linkItems = new List<linkProperty>();
            foreach (var propertydef in linkdef.NestedProperties)
            {
                var property = new linkProperty
                {
                    format = propertydef.Format,
                    name = propertydef.Name
                };
                linkItems.Add(property);
            }
            link.property = linkItems.ToArray();
            return link;
        }
    }
}