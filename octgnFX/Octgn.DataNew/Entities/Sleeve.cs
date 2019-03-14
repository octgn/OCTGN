using log4net;
using Octgn.DataNew.Entities;
using Octgn.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Octgn.DataNew
{
    public class Sleeve : ISleeve
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public byte[] ImageData { get; set; }

        public string Name { get; set; }

        public string FilePath { get; set; }

        public SleeveSource Source { get; set; }
        public Guid GameId { get; set; }

        public Sleeve() {

        }

        private void VerifyIsValid() {
            var isNameValid = !string.IsNullOrEmpty(Name) &&
              Name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

            var isNameTooLong = Name.Length > 120;

            var isNameTooShort = Name.Length == 0;

            var isImageDataValid = ImageData != null &&
                ImageData.Length > 0;

            if (!isNameValid)
                throw new SleeveException("Sleeve Name is invalid.");

            if (isNameTooLong)
                throw new SleeveException("Sleeve Name is too long.");

            if (isNameTooShort)
                throw new SleeveException("Sleeve Name is too short.");

            if (!isImageDataValid)
                throw new SleeveException("Sleeve Image is invalid.");
        }

        public object Clone() {
            return new Sleeve() {
                Name = Name,
                ImageData = ImageData.ToArray(),
                FilePath = FilePath,
                Source = Source
            };
        }

        public static Sleeve FromString(string sleeveImageString) {
            if (!string.IsNullOrWhiteSpace(sleeveImageString)) {
                var parts = sleeveImageString.Split(new char[] { ':' }, 3);

                if (parts.Length != 3)
                    throw new SleeveException($"This decks sleeve format is invalid.");

                var name = parts[1];

                switch (parts[0]) {
                    case "custom": {
                            var sleeve = new Sleeve();

                            sleeve.Name = name;

                            try {
                                sleeve.ImageData = Convert.FromBase64String(parts[2]);
                            } catch (FormatException ex) {
                                throw new SleeveException("This decks sleeve data is invalid.", ex);
                            }

                            return sleeve;
                        }
                    case "game": {

                            var sleeve = GetSleeves()
                                .Where(x => x.Source == SleeveSource.Game)
                                .Where(x => {
                                    var dir = new FileInfo(x.FilePath).Directory;
                                    var gameIdString = dir.Parent.Name;
                                    return string.Equals(gameIdString, x.GameId.ToString(), StringComparison.InvariantCultureIgnoreCase);
                                })
                                .Where(x => x.Name == name)
                                .FirstOrDefault();

                            return sleeve ?? throw new SleeveException($"Unable to find Game sleeve ");
                        }
                    case "octgn": {
                            var sleeve = GetSleeves()
                                .Where(x => x.Source == SleeveSource.OCTGN)
                                .Where(x => x.Name == name)
                                .FirstOrDefault();

                            return sleeve ?? throw new SleeveException($"Unable to find Game sleeve ");
                        }
                    default: throw new SleeveException($"Sleeve type {parts[0]} is not valid.");
                }
            } else {
                return null;
            }
        }
        
        public static string ToString(ISleeve sleeve) {
            if (sleeve == null) return null;

            switch (sleeve.Source) {
                case SleeveSource.User:
                    return "custom:" + sleeve.Name + ":" + Convert.ToBase64String(sleeve.ImageData);
                case SleeveSource.Game:
                    return "game:" + sleeve.Name + ":" + sleeve.GameId.ToString();
                case SleeveSource.OCTGN:
                    return "octgn:" + sleeve.Name + ":";
                default: throw new InvalidOperationException($"Can't use SleeveSource.{sleeve.Source}, it hasn't been implemented.");
            }
        }

        public static IEnumerable<Sleeve> GetSleeves() {
            var mainSleeveDirectory = new DirectoryInfo(Config.Instance.Paths.SleevePath);

            foreach(var sleeve in GetFromFolder(mainSleeveDirectory)) {
                sleeve.Source = SleeveSource.User;

                yield return sleeve;
            }

            foreach(var game in DbContext.Get().Games) {
                var path = Path.Combine(Config.Instance.Paths.DatabasePath, game.Id.ToString(), "Sleeves");

                var dir = new DirectoryInfo(path);

                foreach(var sleeve in GetFromFolder(dir)) {
                    sleeve.Source = SleeveSource.Game;
                    sleeve.GameId = game.Id;

                    yield return sleeve;
                }
            }

            var builtInSleeveDirectory = new DirectoryInfo(Path.Combine(Config.Instance.Paths.BasePath, "Sleeves"));
            foreach(var sleeve in GetFromFolder(builtInSleeveDirectory)) {
                sleeve.Source = SleeveSource.OCTGN;

                yield return sleeve;
            }
        }

        private static IEnumerable<Sleeve> GetFromFolder(DirectoryInfo dir) {
            if (dir == null) throw new ArgumentNullException(nameof(dir));

            FileInfo[] files = null;

            try {
                if (!dir.Exists)
                    yield break;

                files = dir.GetFiles();
            } catch(Exception ex) {
                Log.Warn($"Error loading sleeves in {dir.FullName}", ex);

                yield break;
            }

            foreach(var file in files) {
                Sleeve sleeve = null;

                try {
                    sleeve = new Sleeve();
                    sleeve.Name = Path.GetFileNameWithoutExtension(file.Name);
                    sleeve.ImageData = File.ReadAllBytes(file.FullName);
                    sleeve.FilePath = file.FullName;

                    sleeve.VerifyIsValid();
                } catch (Exception ex) {
                    Log.Warn($"Error loading sleeve {file.FullName}", ex);

                    continue;
                }

                yield return sleeve;
            }
        }
    }

    public class SleeveException : Exception
    {
        public SleeveException() : base() {
        }

        public SleeveException(string message) : base(message) {
        }

        public SleeveException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
