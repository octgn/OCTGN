// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Octide
{
    public interface IAsset
    {
        AssetType Type { get; }
    }

    public class Asset : ViewModelBase, IAsset, IEqualityComparer<Asset>, IEquatable<Asset>
    {
        public string[] Hierarchy { get; set; }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name == value) return;
                _name = value;
                //TODO: Name must be unique
                RaisePropertyChanged("Name");
                RaisePropertyChanged("FileName");
                RaisePropertyChanged("RelativePath");
                UpdateLinkedAssetPaths();
            }
        }
        public bool LockName { get; set; }
        public string Extension { get; set; }

        public string FileName => Name + Extension;
        public string RelativePath => Path.Combine(Path.Combine(Hierarchy), FileName);
        public string FullPath => Path.Combine(ViewModelLocator.GameLoader.WorkingDirectory?.FullName, RelativePath);
        public bool IsReserved { get; set; }


        public FileInfo TargetFile { get; set; }
        public string TargetFilePath => TargetFile?.FullName;
        public FileInfo SafeFile { get; set; }
        public string SafeFilePath => SafeFile?.FullName;
        public AssetType Type => GetAssetType(Extension);
        public IEnumerable<string> UniqueNames => ViewModelLocator.AssetsTabViewModel.Assets.Where(x => x.Hierarchy.SequenceEqual(Hierarchy)).Select(x => x.Name);

        internal List<AssetController> _linkedAssets;

        public Asset()
        {
            _linkedAssets = new List<AssetController>();
        }

        //loads the asset from an existing location within the working directory
        public Asset(FileInfo file): this()
        {
            var safeFilePath = Path.Combine(ViewModelLocator.AssetsTabViewModel.AssetTempDirectory.FullName, Guid.NewGuid().ToString() + file.Extension);
            SafeFile = file.CopyTo(safeFilePath);
            Extension = SafeFile.Extension;
            SafeFile.Attributes = FileAttributes.Temporary;
        }

        public bool UnlinkAsset(AssetController control)
        {
            if (_linkedAssets.Contains(control))
            {
                _linkedAssets.Remove(control);
                RaisePropertyChanged("LinkedAssetsCount");
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
            RaisePropertyChanged("LinkedAssetsCount");
            return true;
        }

        public int LinkedAssetsCount => _linkedAssets.Count;
        public bool IsLinked => LinkedAssetsCount > 0;

        public void UpdateLinkedAssetPaths()
        {
            foreach (var controller in _linkedAssets)
            {
                controller.RaisePropertyChanged("Path");
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
        public static AssetType GetAssetType(string extension)
        {
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".png":
                case ".gif":
                case ".tiff":
                    return AssetType.Image;
                case ".py":
                    return AssetType.PythonScript;
                case ".xml":
                    return AssetType.Xml;
                case ".ttf":
                    return AssetType.Font;
                case ".o8d":
                    return AssetType.Deck;
                case ".mp3":
                case ".oog":
                case ".wav":
                    return AssetType.Sound;
                case ".txt":
                case ".html":
                case ".pdf":
                    return AssetType.Document;
                case ".dll":
                    return AssetType.Library;
                default:
                    return AssetType.Other;
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
                case AssetType.Library:
                    return "DLL files (*.DLL)|*.DLL";
                case AssetType.Sound:
                    return "Sound files (*.MP3;*.WAV;*.OGG)|*.MP3;*.WAV;*.OGG";
                default:
                    return "Any files|*.*";
            }
        }
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
        Library,
        Other
    }
}
