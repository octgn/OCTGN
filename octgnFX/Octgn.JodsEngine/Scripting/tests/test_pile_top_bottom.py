"""
TDD tests for Pile.top() and Pile.bottom() — Issue #2278

Verifies that pile.top() and pile.bottom() return None when the pile is empty,
and return the correct card(s) when the pile has cards.

The full production script (3.1.0.2.py) is loaded via test_helper, which
provides IronPython stubs and a mock game engine. Only the _api bridge is
mocked — the actual game logic under test is the production code.

Run: python3 -m pytest test_pile_top_bottom.py -v
"""

import os
import sys
import unittest

# Ensure the tests directory is on the path so test_helper can be imported
sys.path.insert(0, os.path.dirname(__file__))

from test_helper import MockGameEngine, load_script_version

# ---------------------------------------------------------------------------
# Load the full production script with a mock engine
# ---------------------------------------------------------------------------

engine = MockGameEngine()
mod = load_script_version("3.1.0.2.py", engine)
Card = mod.Card
Pile = mod.Pile


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
