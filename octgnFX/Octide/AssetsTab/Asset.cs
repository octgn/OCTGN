// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Octide
{
    public interface IAsset
    {
        FileInfo File { get; }
        AssetType Type { get; }
    }

    public enum AssetType
    {
        All,
        PythonScript,
        Image,
        Xml,
        Deck,
        Font,
        Sound,
        Document,
        Other
    }

    public class Asset : IAsset, IEqualityComparer<Asset>, IEquatable<Asset>
    {
        public FileInfo File { get; private set; }
        public AssetType Type { get; private set; }

        internal List<AssetController> _linkedAssets;

        public string FullPath => File?.FullName;
        public string Folder => File?.Directory.FullName;
        public string FileName => File?.Name.Substring(0, File.Name.Length - File.Extension.Length);
        public string FullFileName => File?.Name;
        public string Extension => File?.Extension.Substring(1);
        public string RelativePath => File == null ? null : Utils.MakeRelativePath(ViewModelLocator.GameLoader.Directory, File.FullName);

        public Asset()
        {
            _linkedAssets = new List<AssetController>();
        }

        private static readonly Dictionary<string, Asset> AssetCache = new Dictionary<string, Asset>(StringComparer.InvariantCultureIgnoreCase);


        public static AssetType GetAssetType(FileInfo file)
        {
            switch (file.Extension.Substring(1).ToLower())
            {
                case "jpg":
                case "jpeg":
                case "bmp":
                case "png":
                case "gif":
                case "tiff":
                    return AssetType.Image;
                case "py":
                    return AssetType.PythonScript;
                case "xml":
                    return AssetType.Xml;
                case "ttf":
                    return AssetType.Font;
                case "o8d":
                    return AssetType.Deck;
                case "mp3":
                case "oog":
                case "wav":
                    return AssetType.Sound;
                case "txt":
                case "html":
                case "pdf":
                    return AssetType.Document;
                default:
                    return AssetType.Other;
            }
        }

        public bool UnlinkAsset(AssetController control)
        {
            if (_linkedAssets.Contains(control))
            {
                _linkedAssets.Remove(control);
                return true;
            }
            return false;
        }
        public bool LinkAsset(AssetController control)
        {
            if (_linkedAssets.Contains(control))
            {
                return false;
            }
            _linkedAssets.Add(control);
            return true;
        }

        public bool IsLinked
        {
            get
            {
                return _linkedAssets.Count > 0;
            }
        }

        public static string GetAssetFilters(AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.Image:
                    return "Image Files (*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG";
                case AssetType.PythonScript:
                    return "Python files (*.PY)|*.PY";
                case AssetType.Xml:
                    return "Xml files (*.XML)|*.XML";
                case AssetType.Font:
                    return "Font files (*.TTF)|*.TTF";
                case AssetType.Deck:
                    return "OCTGN Deck files (*.O8D)|*.O8D";
                case AssetType.Document:
                    return "Document files (*.HTML;*.PDF;*.TXT)|*.HTML;*.PDF;*.TXT";
                case AssetType.Sound:
                    return "Sound files (*.MP3;*.WAV;*.OGG)|*.MP3;*.WAV;*.OGG";
                default:
                    return "Any files|*.*";
            }
        }

        public static Asset Load(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var file = new FileInfo(Path.GetFullPath(path));
            if (file == null || path == ViewModelLocator.GameLoader.Directory)
                return null;
            return Load(file);
        }

        public static Asset Load(FileInfo file)
        {
            lock (AssetCache)
            {
                if (AssetCache.ContainsKey(file.FullName))
                {
                    return AssetCache[file.FullName];
                }

                var a = new Asset
                {
                    File = file
                };

                a.Type = GetAssetType(file);

                AssetCache.Add(file.FullName, a);
                return a;
            }
        }

        public bool Equals(Asset other)
        {
            if (other == null) return false;
            var ret = FullPath.Equals(other.FullPath, StringComparison.InvariantCultureIgnoreCase);
            return ret;
        }

        public override string ToString()
        {
            return RelativePath;
        }

        public bool Equals(Asset x, Asset y)
        {
            if (x == null && y != null) return false;
            if (x != null && y == null) return false;
            return String.Equals(x.FullPath, y.FullPath, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(Asset obj)
        {
            if (obj == null) return 0;
            return obj.FullPath.GetHashCode();
        }
    }
}
