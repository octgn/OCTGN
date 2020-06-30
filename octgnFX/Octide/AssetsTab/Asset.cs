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
        public AssetType Type { get; private set; }
        internal List<AssetController> _linkedAssets;

        public FileInfo File { get; private set; }
        public string FileName { get; set; }
        public string RelativePath => Utils.MakeRelativePath(ViewModelLocator.GameLoader.WorkingDirectory, File.FullName);
        public string FullPath => File.FullName;
        public string Extension => File?.Extension.Substring(1);

        // targets the full path of the temporary file for rendering images and asset data
        public string SafeFilePath => File.FullName;

        public bool IsReserved { get; set; }

        public Asset()
        {
            _linkedAssets = new List<AssetController>();
        }

        //loads the asset from an existing location
        public Asset(FileInfo file): this()
        {
            File = file;
            FileName = file.Name;
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

        public bool Equals(Asset other)
        {
            if (other == null) return false;
            var ret = RelativePath.Equals(other.RelativePath, StringComparison.InvariantCultureIgnoreCase);
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
            return String.Equals(x.RelativePath, y.RelativePath, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(Asset obj)
        {
            if (obj == null) return 0;
            return obj.RelativePath.GetHashCode();
        }
    }
}
