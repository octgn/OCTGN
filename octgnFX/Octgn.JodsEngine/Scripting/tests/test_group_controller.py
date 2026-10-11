"""Whole production script tests for the Group.controller bridge contract.

The NUnit GroupControllerTests fixture separately exercises actual C# table
lookup and the IronPython runtime. These tests complement that coverage; a
mocked bridge alone cannot reproduce the original C# null dereference.
"""
import builtins
import unittest
from unittest.mock import patch

from test_helper import MockGameEngine, load_script_version


class ControllerEngine(MockGameEngine):
    def __init__(self):
        super().__init__()
        self.controller_result = None
        self.controller_error = None
        self.controller_calls = []
        self.player_lookups = []

    def LocalPlayerId(self): return -1
    def SharedPlayerId(self): return -1
    def AllPlayers(self): return []
    def IsSpectator(self): return True
    def PlayerHandId(self, player_id): return 0

    def GroupController(self, group_id):
        self.controller_calls.append(group_id)
        if self.controller_error is not None:
            raise self.controller_error
        return self.controller_result

    def player_data(self, player_id):
        if player_id is None or player_id < 0:
            raise AssertionError("Absent controller reached the Player constructor")
        self.player_lookups.append(player_id)
        return []

    def PlayerCounters(self, player_id): return self.player_data(player_id)
    def PlayerPiles(self, player_id): return self.player_data(player_id)


class GroupControllerTests(unittest.TestCase):
    scripts = ("3.1.0.0.py", "3.1.0.1.py", "3.1.0.2.py", "../PythonAPI.py")

    def load(self, filename):
        engine = ControllerEngine()
        # The unregistered legacy script expects the lowercase .NET alias.
        # The full production source is executed without replacing its getter.
        with patch.object(builtins, "false", False, create=True):
            script = load_script_version(filename, engine)
        return engine, script

    def test_absent_controller_returns_none_without_player_construction(self):
        for filename in self.scripts:
            for result in (None, -1):
                with self.subTest(script=filename, controller=result):
                    engine, script = self.load(filename)
                    engine.controller_result = result
                    self.assertIsNone(script.table.controller)
                    self.assertEqual(engine.controller_calls, [0x01000000])
                    self.assertEqual(engine.player_lookups, [])

    def test_controlled_group_preserves_zero_and_positive_player_ids(self):
        for filename in self.scripts:
            for result in (0, 7):
                with self.subTest(script=filename, controller=result):
                    engine, script = self.load(filename)
                    engine.controller_result = result
                    controller = script.table.controller
                    self.assertIsInstance(controller, script.Player)
                    self.assertEqual(controller._id, result)
                    self.assertEqual(engine.controller_calls, [0x01000000])
                    self.assertEqual(engine.player_lookups, [result, result])

    def test_bridge_lookup_errors_are_not_suppressed(self):
        for filename in self.scripts:
            with self.subTest(script=filename):
                engine, script = self.load(filename)
                error = RuntimeError("Owned invalid group lookup")
                engine.controller_error = error
                with self.assertRaises(RuntimeError) as caught:
                    script.table.controller
                self.assertIs(caught.exception, error)
                self.assertEqual(engine.controller_calls, [0x01000000])
                self.assertEqual(engine.player_lookups, [])


if __name__ == "__main__":
    unittest.main()
