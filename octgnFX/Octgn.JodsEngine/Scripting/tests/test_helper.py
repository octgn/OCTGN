"""
Test helper for loading OCTGN Python API scripts under CPython.

The production scripts (3.1.0.x.py) are IronPython and rely on .NET imports
(System.IO, System.Collections.Generic, System) plus Python 2 builtins
(basestring, cmp). This module provides stubs for all of those so the full
production script can be loaded as a module without modification.

Usage:
    from test_helper import MockGameEngine, load_script_version

    engine = MockGameEngine()
    mod = load_script_version("3.1.0.2.py", engine)
    Card, Pile = mod.Card, mod.Pile
"""

import os
import sys
import types


# ---------------------------------------------------------------------------
# IronPython / .NET stubs
# ---------------------------------------------------------------------------

def _install_ironpython_stubs():
    """Install fake System.IO, System.Collections.Generic, System modules."""

    # Subscriptable generic stub: List[String](...) -> list(...)
    class _GenericMeta(type):
        def __getitem__(cls, key):
            return cls

    class _GenericList(list, metaclass=_GenericMeta):
        pass

    class _GenericDict(dict, metaclass=_GenericMeta):
        pass

    # System module stub
    system_mod = types.ModuleType("System")
    system_mod.String = str

    # System.IO stub
    system_io = types.ModuleType("System.IO")
    system_io.Directory = type("Directory", (), {})()
    system_io.Path = type("Path", (), {"Combine": staticmethod(os.path.join)})()
    system_mod.IO = system_io

    # System.Collections.Generic stub
    system_collections = types.ModuleType("System.Collections")
    system_generic = types.ModuleType("System.Collections.Generic")
    system_generic.List = _GenericList
    system_generic.Dictionary = _GenericDict
    system_collections.Generic = system_generic

    sys.modules["System"] = system_mod
    sys.modules["System.IO"] = system_io
    sys.modules["System.Collections"] = system_collections
    sys.modules["System.Collections.Generic"] = system_generic


# ---------------------------------------------------------------------------
# Mock OCTGN game engine
# ---------------------------------------------------------------------------

class MockGameEngine:
    """Simulates the OCTGN C# API bridge (_api) for testing."""

    def __init__(self):
        self._groups = {}       # group_id -> [card_id, ...]
        self._next_id = 100     # start high to avoid collision with table id
        self._card_names = {}   # card_id -> name

    # --- Test helpers ---

    def create_pile(self, name="TestPile", card_ids=None):
        pile_id = self._next_id
        self._next_id += 1
        self._groups[pile_id] = list(card_ids) if card_ids else []
        return pile_id

    def add_card_to_pile(self, pile_id, card_id):
        self._groups.setdefault(pile_id, []).append(card_id)

    # --- _api methods called by the production script ---

    def GroupCount(self, group_id):
        return len(self._groups.get(group_id, []))

    def GroupCard(self, group_id, index):
        return self._groups.get(group_id, [])[index]

    def GroupCards(self, group_id):
        return self._groups.get(group_id, [])

    def CardProperty(self, card_id, *args, **kwargs): return ""
    def CardName(self, card_id):
        return self._card_names.get(card_id, "Card%d" % card_id)
    def CardModel(self, card_id): return ""
    def CardSet(self, card_id): return ""
    def CardSetId(self, card_id): return ""
    def CardOwner(self, card_id): return 1
    def CardController(self, card_id): return 1
    def CardGroup(self, card_id): return 1
    def GroupCtor(self, group_id): return "Group(1,'Table')"
    def LocalPlayerId(self): return 1
    def SharedPlayerId(self): return 0
    def AllPlayers(self): return [1]
    def PlayerCounters(self, player_id): return []
    def PlayerPiles(self, player_id): return []
    def GetActivePlayer(self): return 1
    def PlayerName(self, player_id): return "Player1"
    def PlayerColor(self, player_id): return "#ffffff"
    def GetBoard(self): return ""
    def CardProperties(self): return []
    def OCTGN_Version(self): return "3.4.0.0"
    def GameDef_Version(self): return "3.1.0.2"
    def IsTwoSided(self): return False
    def CardAlternate(self, card_id): return ""
    def CardAlternates(self, card_id): return []
    def CardHasProperty(self, card_id, prop, alt=None): return False
    def CardGetFaceUp(self, card_id): return True
    def CardGetOrientation(self, card_id): return 0
    def CardGetHighlight(self, card_id): return ""
    def CardGetFilter(self, card_id): return ""
    def CardPosition(self, card_id): return (0, 0)
    def RealHeight(self, card_id): return 100
    def RealWidth(self, card_id): return 72
    def CardSize(self, card_id):
        return type("Size", (), {"Name": "Default"})()
    def CardGetIndex(self, card_id): return 0
    def CardIsSelected(self, card_id): return False
    def CardPeekers(self, card_id): return []
    def CardTargeted(self, card_id): return -1
    def CardAnchored(self, card_id): return False
    def PlayerGetGlobalVariable(self, player_id, name): return None
    def PlayerHasInvertedTable(self, player_id): return False
    def IsSubscriber(self, player_id): return False
    def IsTableBackgroundFlipped(self): return False
    def GroupGetCollapsed(self, group_id): return False
    def GroupGetVisibility(self, group_id): return "all"
    def GroupViewers(self, group_id): return []
    def GroupController(self, group_id): return 1
    def PileGetViewState(self, group_id): return ""
    def PileGetProtectionState(self, group_id): return ""
    def MarkerGetCount(self, card_id, *args): return 0
    def MarkerSetCount(self, card_id, count, *args): pass
    def CardGetMarkers(self, card_id): return []
    def CardSetProperty(self, card_id, key, val): pass
    def CardResetProperties(self, card_id): pass
    def Mute(self, value): pass
    def Notify(self, message): pass
    def NotifyBar(self, color, message): pass
    def Whisper(self, message): pass
    def Random(self, min, max): return min
    def RandomArray(self, min, max, count): return [min] * count


# ---------------------------------------------------------------------------
# Script loader
# ---------------------------------------------------------------------------

def load_script_version(version_filename, engine):
    """
    Load a full OCTGN Python API version script as a module.

    Installs IronPython stubs, injects _api and Python 2 builtins into the
    module namespace, then executes the entire script. Returns the module
    object so tests can access Card, Pile, Group, Player, etc.
    """
    _install_ironpython_stubs()

    version_file = os.path.join(
        os.path.dirname(__file__), "..", "Versions", version_filename
    )
    version_file = os.path.normpath(version_file)

    with open(version_file, "r") as f:
        source = f.read()

    # Strip BOM
    if source.startswith("\ufeff"):
        source = source[1:]

    # Create a module namespace
    mod_name = "octgn_script_" + version_filename.replace(".", "_")
    mod = types.ModuleType(mod_name)
    mod.__file__ = version_file

    # Inject _api and working directory
    mod.__dict__["_api"] = engine
    mod.__dict__["_wd"] = os.path.dirname(version_file)

    # Python 2 compatibility builtins
    mod.__dict__["basestring"] = str
    mod.__dict__["cmp"] = lambda a, b: (a > b) - (a < b)

    # idict: case-insensitive dict used by Player.__init__
    class idict(dict):
        def __contains__(self, key):
            if isinstance(key, str):
                return any(k.lower() == key.lower() for k in self.keys())
            return super().__contains__(key)

        def __getitem__(self, key):
            if isinstance(key, str):
                for k, v in self.items():
                    if k.lower() == key.lower():
                        return v
            return super().__getitem__(key)

    mod.__dict__["idict"] = idict

    # Execute the full production script in the module namespace
    code = compile(source, version_file, "exec")
    exec(code, mod.__dict__)

    # Patch Group.__getitem__ for Python 3 slice compatibility.
    # IronPython uses __getslice__; CPython 3 passes slice to __getitem__.
    _orig_getitem = mod.Group.__getitem__
    def _getitem_compat(self, key):
        if isinstance(key, slice):
            return (mod.Card(cid) for cid in mod._api.GroupCards(self._id)[key])
        return _orig_getitem(self, key)
    mod.Group.__getitem__ = _getitem_compat

    return mod
