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

        public Asset(FileInfo file): this()
        {
            File = file;
            Type = AssetsTabViewModel.GetAssetType(file);
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

        /*
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

                a.Type = AssetsTabViewModel.GetAssetType(file);

                AssetCache.Add(file.FullName, a);
                return a;
            }
        }
        */

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
