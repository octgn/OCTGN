using System;
using System.Collections.Generic;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;

// Tiny stand-ins for the OCTGN dependencies of DeckExtensionMethods.Share. Only what Share touches.
namespace log4net
{
    public interface ILog { void Warn(object message); }

    public static class LogManager
    {
        public static readonly List<string> Warnings = new List<string>();

        public static ILog GetLogger(Type type) { return new Capture(); }

        private sealed class Capture : ILog
        {
            public void Warn(object message) { Warnings.Add(message.ToString()); }
        }
    }
}

namespace Octgn
{
    public class FakeLobbyClient { public bool IsConnected; }

    public static class Program
    {
        public static FakeLobbyClient LobbyClient;
        public static string SessionKey;
    }
}

namespace Octgn.Core
{
    public static class Prefs { public static string Username; }
}

namespace Octgn.DataNew.Entities
{
    public interface IDeck { Guid GameId { get; } }

    public class Game { }
}

namespace Octgn.Core.DataManagers
{
    public class GameManager
    {
        public static GameManager Get() { return new GameManager(); }

        public Game GetById(Guid id) { return new Game(); }
    }
}

namespace Octgn.Core.DataExtensionMethods
{
    public static class DeckSaveExtensions
    {
        public static Action<string> OnSave = path => { };

        public static void Save(this IDeck deck, Game game, string path) { OnSave(path); }
    }
}

namespace Octgn.Site.Api
{
    public class ApiClient
    {
        public static Func<string, string, string, string, SharedDeckUploadResult> OnShare;

        public SharedDeckUploadResult ShareDeck(string user, string session, string name, string file)
        {
            return OnShare(user, session, name, file);
        }
    }
}
