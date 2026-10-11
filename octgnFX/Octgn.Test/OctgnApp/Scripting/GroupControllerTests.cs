using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using IronPython.Hosting;
using NUnit.Framework;
using Octgn.Play;
using Octgn.Scripting;
using Octgn.Scripting.Versions;

namespace Octgn.Test.OctgnApp.Scripting
{
    [TestFixture, NonParallelizable]
    public class GroupControllerTests
    {
        private const int TableId = 0x01000000;
        private GameEngine _previousEngine;
        private Table _table;

        [SetUp]
        public void SetUp()
        {
            _previousEngine = Program.GameEngine;
            _table = new Table(new Octgn.DataNew.Entities.Group { Name = "Owned test table" });
#pragma warning disable 618 // This constructor exists for test-only GameEngine setup.
            var engine = new GameEngine();
#pragma warning restore 618
            typeof(GameEngine).GetField("_table", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(engine, _table);
            Program.GameEngine = engine;
        }

        [TearDown]
        public void TearDown()
        {
            Program.GameEngine = _previousEngine;
        }

        private static Func<int, int> Getter(int version)
        {
            switch (version)
            {
                case 0: return new Script_3_1_0_0().GroupController;
                case 1: return new Script_3_1_0_1().GroupController;
                default: return new Script_3_1_0_2().GroupController;
            }
        }

        private void SetController(byte id)
        {
            // Only Id is used by this getter. Avoid Player's constructor, which
            // registers players and may query subscription/account services.
            var player = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
            player.Id = id;
            _table.Controller = player;
        }

        [TestCase(0), TestCase(1), TestCase(2)]
        public void Backend_UnownedTableReturnsSentinel(int version)
        {
            Assert.IsNull(_table.Controller);
            Assert.AreSame(_table, Group.Find(TableId));
            Assert.AreEqual(-1, Getter(version)(TableId));
        }

        [TestCase(0, 0), TestCase(0, 7), TestCase(1, 0), TestCase(1, 7), TestCase(2, 0), TestCase(2, 7)]
        public void Backend_ControlledTablePreservesPlayerId(int version, byte id)
        {
            SetController(id);
            Assert.AreEqual(id, Getter(version)(TableId));
        }

        [TestCase(0), TestCase(1), TestCase(2)]
        public void Backend_UnknownGroupStillThrows(int version)
        {
            var unusedPlayerId = Enumerable.Range(0, 256).First(id => Player.Find((byte)id) == null);
            var invalidGroupId = TableId | (unusedPlayerId << 16) | 1;
            Assert.Throws<NullReferenceException>(() => Getter(version)(invalidGroupId));
        }

        [TestCase(0), TestCase(1), TestCase(2)]
        public void Backend_ReturnTypeRemainsInt(int version)
        {
            Assert.AreEqual(typeof(int), Getter(version).Method.ReturnType);
        }

        private static string ScriptPath(int version)
        {
            return version < 0 ? "scripting/pythonapi.py" : "scripting/versions/3.1.0." + version + ".py";
        }

        private static string ReadProductionScript(int version)
        {
            var assembly = typeof(ScriptApi).Assembly;
            var resources = new ResourceManager(assembly.GetName().Name + ".g", assembly);
            try
            {
                using (var stream = resources.GetStream(ScriptPath(version), CultureInfo.InvariantCulture))
                {
                    Assert.IsNotNull(stream, "The actual production Python resource must be available.");
                    using (var reader = new StreamReader(stream)) return reader.ReadToEnd();
                }
            }
            finally { resources.ReleaseAllResources(); }
        }

        private static object ExecuteGetter(int version, ControllerBridge bridge, string expression)
        {
            var engine = Python.CreateEngine();
            try
            {
                engine.Runtime.LoadAssembly(typeof(string).Assembly);
                var scope = engine.CreateScope();
                scope.SetVariable("_api", bridge);
                scope.SetVariable("_wd", "");
                // The unused legacy script expects lowercase false. Empty
                // player data needs no case-insensitive dictionary behavior.
                scope.SetVariable("false", false);
                engine.Execute("idict = dict", scope);
                engine.Execute(ReadProductionScript(version), scope);
                return engine.Execute(expression, scope);
            }
            finally { engine.Runtime.Shutdown(); }
        }

        [TestCase(0), TestCase(1), TestCase(2), TestCase(-1)]
        public void IronPython_WholeScriptUnownedTableReturnsNone(int version)
        {
            var bridge = new ControllerBridge(Getter(version));
            Assert.AreEqual(true, ExecuteGetter(version, bridge, "table.controller is None"));
            Assert.AreEqual(1, bridge.ControllerCalls);
            Assert.AreEqual(TableId, bridge.LastGroupId);
            Assert.IsEmpty(bridge.PlayerLookups, "No Player wrapper may be constructed for an absent controller.");
        }

        [TestCase(0, 0), TestCase(0, 7), TestCase(1, 0), TestCase(1, 7)]
        [TestCase(2, 0), TestCase(2, 7), TestCase(-1, 0), TestCase(-1, 7)]
        public void IronPython_WholeScriptControlledTablePreservesPlayer(int version, byte id)
        {
            SetController(id);
            var bridge = new ControllerBridge(Getter(version));
            Assert.AreEqual(id, ExecuteGetter(version, bridge, "table.controller._id"));
            Assert.AreEqual(1, bridge.ControllerCalls);
            CollectionAssert.AreEqual(new[] { (int)id, (int)id }, bridge.PlayerLookups);
        }

        [TestCase(0), TestCase(1), TestCase(2), TestCase(-1)]
        public void IronPython_WholeScriptRetainsNoneCompatibility(int version)
        {
            var bridge = new ControllerBridge(Getter(version)) { ReturnNone = true };
            Assert.AreEqual(true, ExecuteGetter(version, bridge, "table.controller is None"));
            Assert.AreEqual(1, bridge.ControllerCalls);
            Assert.IsEmpty(bridge.PlayerLookups);
        }

        // Initialization/player-data doubles avoid live game/account services.
        // GroupController delegates to the actual versioned C# implementation.
        public sealed class ControllerBridge
        {
            private readonly Func<int, int> _getter;
            public ControllerBridge(Func<int, int> getter) { _getter = getter; }
            public bool ReturnNone { get; set; }
            public int ControllerCalls { get; private set; }
            public int LastGroupId { get; private set; }
            public List<int> PlayerLookups { get; } = new List<int>();
            public object GroupController(int id)
            {
                ControllerCalls++;
                LastGroupId = id;
                return ReturnNone ? null : (object)_getter(id);
            }
            private Dictionary<int, string> PlayerData(int id)
            {
                if (id < 0) throw new InvalidOperationException("Absent controller was passed to Player.");
                PlayerLookups.Add(id);
                return new Dictionary<int, string>();
            }
            public Dictionary<int, string> PlayerCounters(int id) { return PlayerData(id); }
            public Dictionary<int, string> PlayerPiles(int id) { return PlayerData(id); }
            public int PlayerHandId(int id) { return 0; }
            public int LocalPlayerId() { return -1; }
            public int SharedPlayerId() { return -1; }
            public int[] AllPlayers() { return new int[0]; }
            public string[] CardProperties() { return new string[0]; }
            public string OCTGN_Version() { return "3.4.0.0"; }
            public string GameDef_Version() { return "3.1.0.2"; }
            public bool IsSpectator() { return true; }
        }
    }
}
