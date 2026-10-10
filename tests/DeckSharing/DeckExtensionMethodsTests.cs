using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using NUnit.Framework;
using Octgn;
using Octgn.Core;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;
using Octgn.Extentions;
using Octgn.Library.Exceptions;
using Octgn.Site.Api;

namespace DeckSharing.Tests
{
    [TestFixture]
    public class DeckExtensionMethodsTests
    {
        private const string Generic = "There was an error sharing your deck. 0xFFFF";

        private sealed class TestDeck : IDeck
        {
            public Guid GameId { get { return Guid.Empty; } }
        }

        private static readonly object[] Invalid =
        {
            new object[] { false, "My deck", "You must be logged in to share a deck." },
            new object[] { true, null, "The deck name can't be blank." },
            new object[] { true, " \t ", "The deck name can't be blank." },
            new object[] { true, new string('x', 33), "The deck name is too long." },
        };

        private readonly List<string> saved = new List<string>(); // every path handed to Save
        private bool existedAtSave;
        private string[] uploaded;

        [SetUp]
        public void SetUp()
        {
            saved.Clear();
            existedAtSave = false;
            uploaded = null;
            LogManager.Warnings.Clear();
            Program.LobbyClient = new FakeLobbyClient { IsConnected = true };
            Program.SessionKey = "session-key";
            Prefs.Username = "tester";
            DeckSaveExtensions.OnSave = Record;
            ApiClient.OnShare = (user, key, name, file) =>
            {
                uploaded = new[] { user, key, name, file };
                return new SharedDeckUploadResult { DeckPath = "decks/ok" };
            };
        }

        // Only paths captured from Save are removed, so a baseline that leaks files is cleaned up too.
        [TearDown]
        public void TearDown()
        {
            foreach (var path in saved) File.Delete(path);
        }

        private void Record(string path)
        {
            saved.Add(path);
            existedAtSave = File.Exists(path);
        }

        private static string ShareDeck(string name = "My deck")
        {
            return new TestDeck().Share(name);
        }

        private static UserMessageException Fails(string name = "My deck")
        {
            return Assert.Throws<UserMessageException>(() => ShareDeck(name));
        }

        private void AssertTempDeleted()
        {
            Assert.That(saved, Has.Count.EqualTo(1), "Save should have received one temp path");
            Assert.That(File.Exists(saved[0]), Is.False, "temp file should be deleted");
        }

        [Test]
        public void Success_uploads_the_saved_file_returns_deck_path_and_deletes_it()
        {
            Assert.That(ShareDeck(), Is.EqualTo("decks/ok"));
            AssertTempDeleted();
            Assert.That(existedAtSave, Is.True);
            Assert.That(uploaded, Is.EqualTo(new[] { "tester", "session-key", "My deck", saved[0] }));
        }

        [Test]
        public void Name_of_32_characters_is_accepted()
        {
            Assert.That(ShareDeck(new string('x', 32)), Is.EqualTo("decks/ok"));
        }

        [TestCaseSource(nameof(Invalid))]
        public void Validation_failure_never_saves_or_uploads(bool connected, string name, string message)
        {
            Program.LobbyClient.IsConnected = connected;

            Assert.That(Fails(name).Message, Is.EqualTo(message));
            Assert.That(saved, Is.Empty);
            Assert.That(uploaded, Is.Null);
        }

        [Test]
        public void Server_error_surfaces_its_message_and_deletes_temp_file()
        {
            ApiClient.OnShare = (u, k, n, f) => new SharedDeckUploadResult { Error = true, Message = "Deck rejected." };

            Assert.That(Fails().Message, Is.EqualTo("Deck rejected."));
            AssertTempDeleted();
        }

        [TestCase(null)]
        [TestCase("  ")]
        public void Server_error_without_message_uses_generic_message(string message)
        {
            ApiClient.OnShare = (u, k, n, f) => new SharedDeckUploadResult { Error = true, Message = message };

            Assert.That(Fails().Message, Is.EqualTo(Generic));
            AssertTempDeleted();
        }

        [Test]
        public void Upload_exception_is_generic_logged_by_type_only_and_deletes_temp_file()
        {
            ApiClient.OnShare = (u, k, n, f) => { throw new InvalidOperationException("password=hunter2 " + f); };

            Assert.That(Fails().Message, Is.EqualTo(Generic));
            AssertTempDeleted();
            Assert.That(LogManager.Warnings, Has.Count.EqualTo(1));
            Assert.That(LogManager.Warnings[0], Does.Contain("System.InvalidOperationException"));
            Assert.That(LogManager.Warnings[0], Does.Not.Contain("hunter2"));
            Assert.That(LogManager.Warnings[0], Does.Not.Contain(saved[0]));
        }

        [Test]
        public void Save_error_is_generic_skips_upload_and_deletes_temp_file()
        {
            DeckSaveExtensions.OnSave = path => { Record(path); throw new IOException("disk full"); };

            Assert.That(Fails().Message, Is.EqualTo(Generic));
            Assert.That(uploaded, Is.Null);
            AssertTempDeleted();
        }

        [Test]
        public void User_message_exception_from_a_dependency_is_rethrown_unchanged()
        {
            var original = new UserMessageException("Deck is invalid.");
            DeckSaveExtensions.OnSave = path => { Record(path); throw original; };

            Assert.That(Fails(), Is.SameAs(original));
            Assert.That(LogManager.Warnings, Is.Empty);
            AssertTempDeleted();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Cleanup_failure_is_logged_and_does_not_mask_the_outcome(bool uploadFails)
        {
            FileStream hold = null;
            ApiClient.OnShare = (u, k, n, f) =>
            {
                hold = new FileStream(f, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                if (uploadFails) throw new UserMessageException("Rejected.");
                return new SharedDeckUploadResult { DeckPath = "decks/ok" };
            };

            try
            {
                if (uploadFails) Assert.That(Fails().Message, Is.EqualTo("Rejected."));
                else Assert.That(ShareDeck(), Is.EqualTo("decks/ok"));

                Assert.That(saved, Has.Count.EqualTo(1));
                Assert.That(File.Exists(saved[0]), Is.True, "locked file cannot be deleted");
                Assert.That(LogManager.Warnings, Has.Count.EqualTo(1));
                Assert.That(LogManager.Warnings[0], Does.Contain("Removing the temporary deck export failed"));
            }
            finally
            {
                if (hold != null) hold.Dispose();
                if (saved.Count == 1) File.Delete(saved[0]);
            }
        }
    }
}
