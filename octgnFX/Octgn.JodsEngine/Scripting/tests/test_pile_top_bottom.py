"""
TDD tests for Pile.top() and Pile.bottom() — Issue #2278

Verifies that pile.top() and pile.bottom() return None when the pile is empty,
and return the correct card(s) when the pile has cards.

These tests import the real Group/Pile/Card classes from the production code
(3.1.0.2.py) and use a mock game environment that simulates the OCTGN _api
object. Only the _api bridge is mocked — the actual game logic under test is
the production code.

Run: python3 test_pile_top_bottom.py
"""

import os
import sys
import unittest
import importlib
import types

# ---------------------------------------------------------------------------
# Mock OCTGN game engine environment
# ---------------------------------------------------------------------------
# This simulates the minimal _api surface that Group/Pile/Card need.
# Game scripts call _api.GroupCount, _api.GroupCard, _api.GroupCards, etc.
# We provide a fake implementation backed by simple Python dicts/lists.
# ---------------------------------------------------------------------------

class _MockGameEngine:
    """Simulates the OCTGN C# API bridge that _api normally provides."""

    def __init__(self):
        self._groups = {}   # group_id -> [card_id, ...]
        self._next_id = 1
        self._card_names = {}  # card_id -> name
        self._card_props = {}  # card_id -> {prop: value}

    # --- Game setup helpers for tests ---

    def create_pile(self, name="TestPile", card_ids=None):
        """Create a pile and return its id. Populates it with given card_ids."""
        pile_id = self._next_id
        self._next_id += 1
        self._groups[pile_id] = list(card_ids) if card_ids else []
        return pile_id

    def add_card_to_pile(self, pile_id, card_id):
        self._groups.setdefault(pile_id, []).append(card_id)

    # --- _api methods that the scripting API calls ---

    def GroupCount(self, group_id):
        return len(self._groups.get(group_id, []))

    def GroupCard(self, group_id, index):
        cards = self._groups.get(group_id, [])
        return cards[index]

    def GroupCards(self, group_id):
        return self._groups.get(group_id, [])

    def CardProperty(self, card_id, alt="", prop="", default=False):
        return ""

    def CardName(self, card_id):
        return self._card_names.get(card_id, "Card%d" % card_id)

    def CardModel(self, card_id):
        return ""

    def CardSet(self, card_id):
        return ""

    def CardSetId(self, card_id):
        return ""

    def CardOwner(self, card_id):
        return 1

    def CardController(self, card_id):
        return 1

    def CardGroup(self, card_id):
        return 1

    def GroupCtor(self, group_id):
        return "Group(1,'Table')"

    def LocalPlayerId(self):
        return 1

    def SharedPlayerId(self):
        return 0

    def AllPlayers(self):
        return [1]

    def PlayerCounters(self, player_id):
        return []

    def PlayerPiles(self, player_id):
        return []

    def GetActivePlayer(self):
        return 1

    def PlayerName(self, player_id):
        return "Player1"

    def PlayerColor(self, player_id):
        return "#ffffff"

    def GetBoard(self):
        return ""

    def CardProperties(self):
        return []

    def OCTGN_Version(self):
        return "3.4.0.0"

    def GameDef_Version(self):
        return "3.1.0.2"

    def IsTwoSided(self):
        return False

    def CardAlternate(self, card_id):
        return ""

    def CardAlternates(self, card_id):
        return []

    def CardHasProperty(self, card_id, prop, alt=None):
        return False

    def CardGetFaceUp(self, card_id):
        return True

    def CardGetOrientation(self, card_id):
        return 0

    def CardGetHighlight(self, card_id):
        return ""

    def CardGetFilter(self, card_id):
        return ""

    def CardPosition(self, card_id):
        return (0, 0)

    def RealHeight(self, card_id):
        return 100

    def RealWidth(self, card_id):
        return 72

    def CardSize(self, card_id):
        return type('Size', (), {'Name': 'Default'})()

    def CardGetIndex(self, card_id):
        return 0

    def CardIsSelected(self, card_id):
        return False

    def CardPeekers(self, card_id):
        return []

    def CardTargeted(self, card_id):
        return -1

    def CardAnchored(self, card_id):
        return False

    def PlayerGetGlobalVariable(self, player_id, name):
        return None

    def PlayerHasInvertedTable(self, player_id):
        return False

    def IsSubscriber(self, player_id):
        return False

    def IsTableBackgroundFlipped(self):
        return False

    def GroupGetCollapsed(self, group_id):
        return False

    def GroupGetVisibility(self, group_id):
        return "all"

    def GroupViewers(self, group_id):
        return []

    def GroupController(self, group_id):
        return 1

    def PileGetViewState(self, group_id):
        return ""

    def PileGetProtectionState(self, group_id):
        return ""

    def MarkerGetCount(self, card_id, *args):
        return 0

    def MarkerSetCount(self, card_id, count, *args):
        pass

    def CardGetMarkers(self, card_id):
        return []

    def CardSetProperty(self, card_id, key, val):
        pass

    def CardResetProperties(self, card_id):
        pass


def _load_production_classes(engine):
    """
    Extract Card, CardProperties, NamedObject, Group, Pile classes from the
    production PythonAPI version file (3.1.0.2.py) and execute them against
    a mock _api object.

    Only the class definitions are extracted (lines 274–517) — top-level
    initialization code, IronPython imports, and helper functions are
    excluded. This means the tested code IS the production code, not a copy.
    """
    version_file = os.path.join(
        os.path.dirname(__file__),
        "..", "Versions", "3.1.0.2.py"
    )
    version_file = os.path.normpath(version_file)
    with open(version_file, "r") as f:
        source = f.read()

    # Strip BOM
    if source.startswith("\ufeff"):
        source = source[1:]

    # Extract only the class definitions (Markers through Pile.lookAt)
    # Markers starts at line 258, CardProperties at 274, Pile.lookAt ends ~517
    source_lines = source.split("\n")
    class_source = "\n".join(source_lines[257:517])

    # Create a module namespace with our mock _api and required globals
    mod = types.ModuleType("octgn_script_api_test")
    mod.__dict__["_api"] = engine
    mod.__dict__["cardProperties"] = []  # global used by Card.__getattr__
    mod.__dict__["convertToString"] = lambda obj: str(obj)  # stub
    mod.__dict__["convertToArgsString"] = lambda obj: str(obj)  # stub
    mod.__dict__["cmp"] = lambda a, b: (a > b) - (a < b)  # Python 2 compat

    # Execute the source to define classes in our namespace
    exec(compile(class_source, version_file, "exec"), mod.__dict__)

    # Patch Group.__getitem__ to handle slices (Python 3 compatibility).
    # In IronPython (Python 2), __getslice__ handles slice indexing.
    # In CPython 3, __getitem__ receives slice objects but the production
    # code only handles integer keys.
    _orig_getitem = mod.Group.__getitem__
    def _getitem_compat(self, key):
        if isinstance(key, slice):
            # Emulate __getslice__ behavior
            indices = key.indices(len(self))
            return (Card(cid) for cid in mod._api.GroupCards(self._id)[key])
        return _orig_getitem(self, key)
    mod.Group.__getitem__ = _getitem_compat

    return mod.Card, mod.Pile


# ---------------------------------------------------------------------------
# Fixtures
# ---------------------------------------------------------------------------

engine = _MockGameEngine()
Card, Pile = _load_production_classes(engine)


# ==== TESTS ====

class TestPileTopEmptyPile(unittest.TestCase):
    """Issue #2278: pile.top() must return None for empty piles."""

    def setUp(self):
        self.pile_id = engine.create_pile("EmptyPile", [])

    def test_top_no_args_returns_none(self):
        pile = Pile(self.pile_id, "EmptyPile", None)
        result = pile.top()
        self.assertIsNone(result, "pile.top() on empty pile should return None")

    def test_top_with_count_on_empty_returns_none(self):
        pile = Pile(self.pile_id, "EmptyPile", None)
        result = pile.top(1)
        self.assertIsNone(result, "pile.top(1) on empty pile should return None")

    def test_top_with_large_count_on_empty_returns_none(self):
        pile = Pile(self.pile_id, "EmptyPile", None)
        result = pile.top(5)
        self.assertIsNone(result, "pile.top(5) on empty pile should return None")


class TestPileBottomEmptyPile(unittest.TestCase):
    """Issue #2278: pile.bottom() must return None for empty piles."""

    def setUp(self):
        self.pile_id = engine.create_pile("EmptyPile", [])

    def test_bottom_no_args_returns_none(self):
        pile = Pile(self.pile_id, "EmptyPile", None)
        result = pile.bottom()
        self.assertIsNone(result, "pile.bottom() on empty pile should return None")

    def test_bottom_with_count_on_empty_returns_none(self):
        pile = Pile(self.pile_id, "EmptyPile", None)
        result = pile.bottom(1)
        self.assertIsNone(result, "pile.bottom(1) on empty pile should return None")


class TestPileTopNonEmptyPile(unittest.TestCase):
    """pile.top() returns correct card(s) from non-empty piles."""

    def setUp(self):
        self.pile_id = engine.create_pile("ThreeCards", [10, 20, 30])

    def test_top_no_args_returns_first_card(self):
        pile = Pile(self.pile_id, "ThreeCards", None)
        result = pile.top()
        self.assertIsNotNone(result, "pile.top() on non-empty pile should not return None")
        self.assertEqual(result._id, 10, "pile.top() should return the first card")

    def test_top_with_count_returns_slice(self):
        pile = Pile(self.pile_id, "ThreeCards", None)
        result = pile.top(2)
        result_list = list(result)
        self.assertEqual(len(result_list), 2)
        self.assertEqual(result_list[0]._id, 10)
        self.assertEqual(result_list[1]._id, 20)

    def test_top_with_count_exceeding_size(self):
        pile = Pile(self.pile_id, "ThreeCards", None)
        result = pile.top(10)
        result_list = list(result)
        self.assertEqual(len(result_list), 3)


class TestPileBottomNonEmptyPile(unittest.TestCase):
    """pile.bottom() returns correct card(s) from non-empty piles."""

    def setUp(self):
        self.pile_id = engine.create_pile("ThreeCards", [10, 20, 30])

    def test_bottom_no_args_returns_last_card(self):
        pile = Pile(self.pile_id, "ThreeCards", None)
        result = pile.bottom()
        self.assertIsNotNone(result, "pile.bottom() on non-empty pile should not return None")
        self.assertEqual(result._id, 30, "pile.bottom() should return the last card")

    def test_bottom_with_count_returns_slice(self):
        pile = Pile(self.pile_id, "ThreeCards", None)
        result = pile.bottom(2)
        result_list = list(result)
        self.assertEqual(len(result_list), 2)
        self.assertEqual(result_list[0]._id, 20)
        self.assertEqual(result_list[1]._id, 30)


class TestPileSingleCard(unittest.TestCase):
    """Edge case: pile with exactly one card."""

    def setUp(self):
        self.pile_id = engine.create_pile("SingleCard", [99])

    def test_top_returns_single_card(self):
        pile = Pile(self.pile_id, "SingleCard", None)
        result = pile.top()
        self.assertIsNotNone(result)
        self.assertEqual(result._id, 99)

    def test_bottom_returns_single_card(self):
        pile = Pile(self.pile_id, "SingleCard", None)
        result = pile.bottom()
        self.assertIsNotNone(result)
        self.assertEqual(result._id, 99)


if __name__ == '__main__':
    unittest.main()
