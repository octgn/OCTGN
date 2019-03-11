using log4net;
using Octgn.DataNew;
using Octgn.DataNew.Entities;
using Octgn.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Octgn.Core
{
    public class Sleeve : ISleeve
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public byte[] ImageData { get; set; }

        public string Name { get; set; }

        public string FilePath { get; set; }

        public SleeveSource Source { get; set; }

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

        public BitmapImage GetImage() {
            return GetImage(this);
        }

        public static BitmapImage GetImage(ISleeve sleeve) {
            if (sleeve == null) return null;

            BitmapImage image = null;

            using (var memoryStream = new MemoryStream(sleeve.ImageData)) {
                memoryStream.Position = 0;

                image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;

                try {
                    image.StreamSource = memoryStream;

                    image.EndInit();
                } catch (Exception ex) {
                    throw new SleeveException($"Error loading deck sleeve for {sleeve.Name}, the image is invalid.", ex);
                }

                if (image.CanFreeze) {
                    image.Freeze();
                }
            }

            return image;
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

                if (parts[0] == "sleeve64") {
                    var sleeve = new Sleeve();

                    sleeve.Name = parts[1];

                    try {
                        sleeve.ImageData = Convert.FromBase64String(parts[2]);
                    } catch (FormatException ex) {
                        throw new SleeveException("This decks sleeve data is invalid.", ex);
                    }

                    return sleeve;
                } else {
                    throw new SleeveException($"This decks sleeve format is invalid.");
                }
            } else {
                return null;
            }
        }
        
        public static string ToString(ISleeve sleeve) {
            if (sleeve == null) return null;

            return "sleeve64:" + sleeve.Name + ":" + Convert.ToBase64String(sleeve.ImageData);
        }

        public static byte[] FromUrl(Uri url) {
            if (url == null) return null;

            using (var webClient = new WebClient()) {
                return webClient.DownloadData(url);
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
