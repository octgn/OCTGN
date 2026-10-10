using System;
using System.Collections;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Octgn.Library;

namespace Octgn.Test.Library
{
    [TestFixture, NonParallelizable]
    public class ConfigFileSaveTests
    {
        private const string OriginalJson = "{\"existing\":\"preserve me\",\"count\":7}";
        private string _path;
        private IX _originalContext;

        [SetUp]
        public void SetUp()
        {
            _originalContext = X.SingletonContext;
            X.SingletonContext = new X();
            var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "config-save-owned-files");
            Directory.CreateDirectory(directory);
            _path = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".json");
            File.WriteAllBytes(_path, Encoding.UTF8.GetBytes(OriginalJson));
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (_path != null && File.Exists(_path)) File.Delete(_path);
            }
            finally
            {
                X.SingletonContext = _originalContext;
            }
        }

        [TestCase("getter")]
        [TestCase("loop")]
        public void Save_SerializationFails_PreservesExistingFile(string failure)
        {
            var original = File.ReadAllBytes(_path);
            using (var config = new ConfigFile(_path))
            {
                Assert.IsTrue(config.OpenedFile);
                config.AddOrSet("unsavable", FailingValue(failure));
                Assert.DoesNotThrow(config.Save, "Save retains its existing caught-error contract");
            }

            CollectionAssert.AreEqual(original, File.ReadAllBytes(_path),
                "Serialization failure must preserve every byte of the existing config");
            Assert.AreEqual("preserve me", (string)JObject.Parse(File.ReadAllText(_path))["existing"]);
        }

        [TestCase("getter")]
        [TestCase("loop")]
        public void Save_AfterRemovingUnsavableValue_PersistsSubsequentChanges(string failure)
        {
            using (var config = new ConfigFile(_path))
            {
                Assert.IsTrue(config.OpenedFile);
                config.AddOrSet("unsavable", FailingValue(failure));
                config.Save();
                config.Remove("unsavable");
                config.AddOrSet("changed", "saved after failure");
                config.Save();
            }

            var persisted = JObject.Parse(File.ReadAllText(_path));
            Assert.AreEqual("preserve me", (string)persisted["existing"]);
            Assert.AreEqual("saved after failure", (string)persisted["changed"]);
            Assert.IsNull(persisted["unsavable"]);
        }

        [Test]
        public void Save_ValidChanges_PreservesOtherSettingsAndReplacesOldContent()
        {
            using (var config = new ConfigFile(_path))
            {
                Assert.IsTrue(config.OpenedFile);
                config.Remove("count");
                config.AddOrSet("existing", "updated");
                config.Save();
            }

            var persisted = JObject.Parse(File.ReadAllText(_path));
            Assert.AreEqual("updated", (string)persisted["existing"]);
            Assert.IsNull(persisted["count"]);
        }

        private static object FailingValue(string failure)
        {
            if (failure == "getter") return new ThrowingValue();
            var loop = new Hashtable();
            loop["self"] = loop;
            return loop;
        }

        private sealed class ThrowingValue
        {
            public string Value { get { throw new InvalidOperationException("Deliberate owned-fixture failure"); } }
        }
    }
}
