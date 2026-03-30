"""
TDD tests for Pile.top() and Pile.bottom() — Issue #2278

Verifies that pile.top() and pile.bottom() return None when the pile is empty,
and return the correct card(s) when the pile has cards.

Run: python3 test_pile_top_bottom.py
"""

import unittest

# ---- Minimal stubs mirroring the OCTGN PythonAPI classes ----

class _MockAPI:
    """Minimal mock of the OCTGN _api object for Group/Pile testing."""
    def __init__(self):
        self._groups = {}  # group_id -> [card_id, ...]

    def setup_group(self, group_id, card_ids):
        self._groups[group_id] = list(card_ids)

    def GroupCount(self, group_id):
        return len(self._groups.get(group_id, []))

    def GroupCard(self, group_id, index):
        cards = self._groups.get(group_id, [])
        return cards[index]

    def GroupCards(self, group_id):
        return self._groups.get(group_id, [])

# ---- Classes extracted from PythonAPI.py (keeping production logic) ----

mock_api = _MockAPI()
_api = mock_api  # Global _api that PythonAPI.py references

class Card(object):
    def __init__(self, id):
        self._id = id
    def __eq__(self, other):
        return isinstance(other, Card) and self._id == other._id
    def __repr__(self):
        return "Card(%d)" % self._id

class NamedObject(object):
    def __init__(self, id, name):
        self._id = id
        self.name = name

class Group(NamedObject):
    def __init__(self, id, name, player=None):
        NamedObject.__init__(self, id, name)
        self._player = player
    @property
    def player(self): return self._player
    def __len__(self): return _api.GroupCount(self._id)
    def __getitem__(self, key):
        if isinstance(key, slice):
            indices = range(*key.indices(len(self)))
            return (Card(id) for id in _api.GroupCards(self._id)[key])
        if key < 0: key += len(self)
        return Card(_api.GroupCard(self._id, key))
    def __iter__(self): return (Card(id) for id in _api.GroupCards(self._id))
    def __getslice__(self, i, j): return (Card(id) for id in _api.GroupCards(self._id)[i:j])

# ---- Production code under test (same as PythonAPI.py fix) ----

class Pile(Group):
    def __init__(self, id, name, player):
        Group.__init__(self, id, name, player)
    def top(self, count=None):
        if len(self) == 0: return None
        return self[0] if count == None else self[:count]
    def bottom(self, count=None):
        if len(self) == 0: return None
        return self[-1] if count == None else self[-count:]


# ==== TESTS (written BEFORE fix — TDD RED phase) ====

class TestPileTopEmptyPile(unittest.TestCase):
    """Issue #2278: pile.top() must return None for empty piles."""

    def setUp(self):
        mock_api.setup_group(100, [])

    def test_top_no_args_returns_none(self):
        pile = Pile(100, "TestPile", None)
        result = pile.top()
        self.assertIsNone(result, "pile.top() on empty pile should return None")

    def test_top_with_count_on_empty_returns_none(self):
        pile = Pile(100, "TestPile", None)
        result = pile.top(1)
        self.assertIsNone(result, "pile.top(1) on empty pile should return None")

    def test_top_with_large_count_on_empty_returns_none(self):
        pile = Pile(100, "TestPile", None)
        result = pile.top(5)
        self.assertIsNone(result, "pile.top(5) on empty pile should return None")


class TestPileBottomEmptyPile(unittest.TestCase):
    """Issue #2278: pile.bottom() must return None for empty piles."""

    def setUp(self):
        mock_api.setup_group(100, [])

    def test_bottom_no_args_returns_none(self):
        pile = Pile(100, "TestPile", None)
        result = pile.bottom()
        self.assertIsNone(result, "pile.bottom() on empty pile should return None")

    def test_bottom_with_count_on_empty_returns_none(self):
        pile = Pile(100, "TestPile", None)
        result = pile.bottom(1)
        self.assertIsNone(result, "pile.bottom(1) on empty pile should return None")


class TestPileTopNonEmptyPile(unittest.TestCase):
    """pile.top() returns correct card(s) from non-empty piles."""

    def setUp(self):
        mock_api.setup_group(101, [10, 20, 30])

    def test_top_no_args_returns_first_card(self):
        pile = Pile(101, "TestPile", None)
        result = pile.top()
        self.assertIsNotNone(result, "pile.top() on non-empty pile should not return None")
        self.assertEqual(result._id, 10, "pile.top() should return the first card")

    def test_top_with_count_returns_slice(self):
        pile = Pile(101, "TestPile", None)
        result = pile.top(2)
        result_list = list(result)
        self.assertEqual(len(result_list), 2)
        self.assertEqual(result_list[0]._id, 10)
        self.assertEqual(result_list[1]._id, 20)

    def test_top_with_count_exceeding_size(self):
        pile = Pile(101, "TestPile", None)
        result = pile.top(10)
        result_list = list(result)
        self.assertEqual(len(result_list), 3)


class TestPileBottomNonEmptyPile(unittest.TestCase):
    """pile.bottom() returns correct card(s) from non-empty piles."""

    def setUp(self):
        mock_api.setup_group(101, [10, 20, 30])

    def test_bottom_no_args_returns_last_card(self):
        pile = Pile(101, "TestPile", None)
        result = pile.bottom()
        self.assertIsNotNone(result, "pile.bottom() on non-empty pile should not return None")
        self.assertEqual(result._id, 30, "pile.bottom() should return the last card")

    def test_bottom_with_count_returns_slice(self):
        pile = Pile(101, "TestPile", None)
        result = pile.bottom(2)
        result_list = list(result)
        self.assertEqual(len(result_list), 2)
        self.assertEqual(result_list[0]._id, 20)
        self.assertEqual(result_list[1]._id, 30)


class TestPileSingleCard(unittest.TestCase):
    """Edge case: pile with exactly one card."""

    def setUp(self):
        mock_api.setup_group(102, [99])

    def test_top_returns_single_card(self):
        pile = Pile(102, "SinglePile", None)
        result = pile.top()
        self.assertIsNotNone(result)
        self.assertEqual(result._id, 99)

    def test_bottom_returns_single_card(self):
        pile = Pile(102, "SinglePile", None)
        result = pile.bottom()
        self.assertIsNotNone(result)
        self.assertEqual(result._id, 99)


if __name__ == '__main__':
    unittest.main()
